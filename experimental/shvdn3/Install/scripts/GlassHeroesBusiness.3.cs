// Glass Heroes Business Manager - v0.3.3.4 SHVDN3 SERVICE BAY INTEGRATION / EXPERIMENTAL
// Idle crash cause unproven; diagnostics are not a stack-overflow fix.
// Original project. Does not reference or modify Auto Work or its data.
// GTA V Story Mode only | ScriptHookVDotNet3 3.6.0 / .NET Framework 4.8

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GTA;
using GTA.Math;

public class GlassHeroesBusinessV3Experimental : Script
{
    private const string DataDirectory = @"scripts\GlassHeroesBusiness";
    private const string SettingsFile = @"scripts\GlassHeroesBusiness\Settings.ini";
    private const string SaveFile = @"scripts\GlassHeroesBusiness\Save.ini";
    private const int MainMenuItems = 4;
    private const int EmployeeMenuItems = 6;
    private const int PhaseNone = 0;
    private const int PhaseToCheckpoint = 1;
    private const int PhaseToBreak = 2;
    private const int PhaseAtBreak = 3;
    private const int PhaseToBay = 4;
    private const int PhaseWorking = 5;
    private const int PhaseReturningToBreak = 6;
    private const int PhaseRouteBlocked = 7;
    // Categories keep the large Employee Settings menu readable.
    // PointEditor is a reusable sub-menu for EACH individual saved point.
    private enum MenuPage {
        Business, BusinessManagement, Employees, EmployeeSettings, EmployeePaths, ArrivalWaypoints, MechanicBays, ServiceWaypoints,
        BayApproaches, ServiceVehicleBays, ServiceBayProfile, PointEditor, RouteRecovery, PlacementMarkers, Customers, Finances, FinancialOverview, IncomeSales, ProfitLoss, TransactionHistory, OwnerTransfers,
        StaffRoster, StaffProfile, VehicleDriveOptions, CustomerPlacements, EmployeePlacements,
        MechanicWalkingPaths, ReceptionWalkingPaths, ReceptionWaypoints, CustomerQueueSettings,
        PartsSales, VehicleAutoLotOptions, BusinessExpenses, MechanicInvoices, CustomerWalkingRoutes,
        InventoryManagement, InventoryOverview, BuildSupplierOrder, ActiveDeliveries, EquipmentManagement, ControllerSettings, MechanicRouteRoster,
        MechanicRouteDirection, MechanicRoutePoints, SupplierDeliverySetup, SupplierDriverWaypoints,
        HireMechanicRoster, HireReceptionistRoster, MechanicManagement, ReceptionistManagement, ReceptionistProfile, SupplierPalletStorage, ManagerManagement, HireManagerRoster, ManagerProfile, ManagerWalkingPaths, ManagerWaypoints
    }
    private const int VisibleMenuRows = 10;
    // Compact F2 menu (75% of the previous layout), anchored to upper-left.
    private const float MenuScale = 0.75f;
    private const int MenuDesignLeft = 35;
    private const int MenuDesignTop = 74;
    private const int MenuDrawLeft = 18;
    private const int MenuDrawTop = 23;
    private const int VisualBayCount = 3;
    // Reused every frame: avoid allocating a fresh control list on every tick.
    private static readonly GTA.Control[] PhoneControlsToSuppress = new GTA.Control[]
    {
        GTA.Control.Phone, GTA.Control.PhoneUp, GTA.Control.PhoneDown,
        GTA.Control.PhoneLeft, GTA.Control.PhoneRight, GTA.Control.PhoneSelect,
        GTA.Control.PhoneCancel, GTA.Control.PhoneOption,
        GTA.Control.PhoneExtraOption, GTA.Control.PhoneScrollForward,
        GTA.Control.PhoneScrollBackward
    };

    // Save data is NOT stored on NPC entities: entities exist only near the active shop.
    private sealed class Mechanic
    {
        public RepairQuote Pricing; // quoted background job, never an additional expense
        public double WorkSeconds;
        public double IdleSeconds;
        public bool HasJob;  // an assigned repair order; independent of NPC visual state
        public Ped Worker;
        public int WorkMode = -1; // -1 not animating, 0 break/idle, 1 scheduled repair
        public DateTime NextSpawnAttempt = DateTime.MinValue;
        public int ArrivalPhase; // See Phase* constants; employees start at entrance, not at bay
        public DateTime RouteSince = DateTime.MinValue;
        public DateTime LastProgressAt = DateTime.MinValue;
        public float LastDistanceSquared = float.MaxValue;
        public int RouteRetries;
        public int RouteTeleports; // safety cap per route leg
        public List<Vector3> PathNodes = new List<Vector3>();
        public bool ReturningFromCustomerCar; // separate from normal workstation->break return
        public int PathNodeIndex;
        // These belong to the employee's persistent personnel record, not their ped.
        public string Name = "";
        public int Xp;
        public int Specialty; // 0 general, 1 brakes, 2 diagnostics
        public int WageBonus;
        public int JobsCompleted;
        public long UnpaidWages; // accrued by shift; charged to cash only when player pays invoice
        public double WageUnpaidSeconds; // open-shop time since oldest unpaid wage accrual
        public int BreakAnimation;
        public int JobOutboundMask = 31; // five saved Break -> Bay route points; bit=enabled
        public int JobReturnMask = 31; // five saved Bay -> Break route points; bit=enabled
        public double BreakAnimationSeconds;
        // Last known physical post: snapshotted before distant visual cleanup.
        public WorkBay SavedPost = new WorkBay();
    }

    // Receptionist has a separate visual route and no simulated repair orders.
    private sealed class Receptionist
    {
        public Ped Worker;
        public DateTime NextSpawnAttempt = DateTime.MinValue;
        public DateTime LastProgressAt = DateTime.MinValue;
        public DateTime RouteSince = DateTime.MinValue;
        public float LastDistanceSquared = float.MaxValue;
        public int RouteRetries;
        public int RouteTeleports;
        public int NodeIndex;
        public bool AtDesk;
        public bool Blocked;
        public List<Vector3> Route = new List<Vector3>();
        public WorkBay SavedPost = new WorkBay();
    }

    // Customer-order state is separate from employee visual movement.
    // Optional road AI runs only on an exterior route; no vehicle teleport into an MLO.
    private enum CustomerStage { DrivingToDropoff, ParkingAtDropoff, ExitingVehicle, ToApproach, ToCounter,
        CheckingIn, WaitingForMechanic, MechanicWalkingToPickup, MechanicEnteringVehicle, MechanicReversingFromDropoff,
        MechanicDrivingToBay, MechanicParkingAtBay, MechanicExitingAtBay, Repairing,
        MechanicWalkingToReturnCar, MechanicEnteringForReturn, MechanicReversingFromBay, MechanicDrivingToDropoff,
        MechanicParkingAtDropoff, MechanicExitingAtDropoff, MechanicWaitingToReturn,
        ToExitApproach, ReturningToVehicle, DrivingAway, WaitingAtRoadside, DrivingToQueueSpace, ParkingAtQueueSpace, ToWalkWaypoint, ToPickupWaypoint, TeleportWaitingBay, TeleportWaitingPickup }
    private sealed class CustomerOrder
    {
        public bool RestoredPromise;
        public int ResumeMechanicSlot = -1;
        public double ResumeWorkSeconds;
        public int Id;
        public CustomerStage Stage;
        public double SecondsInStage;
        public int MechanicSlot = -1;
        public int ServiceKind;
        public int PartsNeeded;
        public long Price;
        public RepairQuote Pricing;
        public double RepairSeconds;
        public Ped Customer;
        public Vehicle Vehicle;
        public Blip CustomerBlip;
        public bool DroveIn;
        public bool DepartureIssued;
        public bool ParkingTaskIssued;
        public bool Paid; // settlement is separate from customer departure/cleanup
        public bool VehicleTeleport, TeleportPositionApplied, TeleportedToBay, TeleportedToPickup, TeleportAttempted, TeleportWaitNotified;
        public int LastTeleportAttempt;
        public bool VehicleHandoff; // elected at assignment; never changes mid-order
        public bool AutomaticLot; // snapshot of active mode; no changes mid-order
        public WorkBay ResolvedParkingBay; // frozen auto or custom bay per order
        public Vector3 LastDrivePosition;
        public double DriveNoProgressSeconds;
        public int DriveRetryCount;
        public List<Vector3> VehicleWalkPath = new List<Vector3>();
        public int VehicleWalkIndex;
        public int VehicleWalkRecoveries;
        public double VehicleWalkNodeSeconds;
        public float VehicleWalkLastDistanceSq = float.MaxValue;
        public bool VehicleTaskIssued;
        public bool ReturnDriverInCar;
        public WorkBay QueueDestination;
        public List<Vector3> CustomerRoute = new List<Vector3>();
        public int CustomerRouteIndex;
        public bool CustomerRouteReturning;
    }

    private sealed class RemoteJob
    {
        public int MechanicSlot, OrderId, ServiceKind, PartsNeeded;
        public long Price;
        public RepairQuote Pricing;
        public double RemainingGameSeconds;
    }

    private sealed class WorkBay
    {
        public bool IsSet;
        public Vector3 Position;
        public float Heading;
    }

    private readonly List<Mechanic> _mechanics = new List<Mechanic>();
    private readonly WorkBay[] _bays = { new WorkBay(), new WorkBay(), new WorkBay() };
    // Shop-specific arrival points are independent of each mechanic's bay.
    private readonly WorkBay _employeeEntrance = new WorkBay();
    private readonly WorkBay _walkCheckpoint = new WorkBay();
    private readonly WorkBay _breakArea = new WorkBay();
    private readonly WorkBay _receptionDesk = new WorkBay();
    private readonly WorkBay _receptionApproach = new WorkBay();
    private readonly Receptionist _receptionist = new Receptionist();
    private readonly WorkBay _customerDriveSpawn = new WorkBay();
    private readonly WorkBay _customerDropoff = new WorkBay();
    // Distinct vehicle parking positions; _bays[] are PED repair animation points.
    private readonly WorkBay[] _vehicleParkingBays = { new WorkBay(), new WorkBay(), new WorkBay() };
    private bool _vehicleTransfersEnabled = true;
    private int _vehicleTransferTimeout = 90;
    private float _vehicleTransferSpeed = 5.0f;
    // Default remains CUSTOM MLO so upgrading never silently changes a working business.
    // Enable AUTO LOT from Customers > Vehicle Driving Options (no vehicle-bay markers needed).
    private bool _automaticLotMode;
    private bool _autoLotRightSide = true;
    private float _autoLotOffsetMeters = 4.0f;
    private bool _autoLotPreview;
    private bool _customCarBaysFromDriveOptions;
    private const int CarefulDrivingStyle = 786603; // stop/steer for traffic, people, parked cars & props
    private const int RecoveryDrivingStyle = 786603 | 4; // allow swerving around traffic on ONE retry
    private readonly WorkBay _customerApproach = new WorkBay();
    private readonly WorkBay _customerQueue = new WorkBay();
    // 0-3 optional pedestrian corridor points from drop-off to reception. Reverse for pickup.
    private readonly WorkBay[] _customerWalkingRoute = { new WorkBay(), new WorkBay(), new WorkBay() };
    // Queue waiting spaces are for CARS, not the customer walking-to-reception queue.
    // Slot 2 defaults to the existing road spawn until the user saves a dedicated position.
    // Slot 3 is opt-in and stays virtual until its waiting space is saved.
    private readonly WorkBay _queueCar2 = new WorkBay();
    private readonly WorkBay _queueCar3 = new WorkBay();
    // Multiple independent live orders; _customerOrder points to whichever is
    // currently being processed, and is restored to the oldest order after each pass.
    // This retains the legacy helper methods while adding safe per-mechanic work.
    private readonly List<CustomerOrder> _activeCustomerOrders = new List<CustomerOrder>();
    private CustomerOrder _customerOrder;
    // One optional roadside vehicle awaits a clear drop-off. It has NO work order,
    // mechanic assignment, receptionist task or financial effects until promoted.
    private CustomerOrder _stagedCustomerOrder;
    private CustomerOrder _stagedThirdCustomerOrder;
    // Pending requests beyond available staffed bays remain virtual.
    private int _waitingCustomerRequests;
    private int _customerQueueCapacity = 3;
    private float _dropoffClearanceRadius = 4.5f;
    private bool _waitingForDropoff;
    private DateTime _lastDropoffWarningAt = DateTime.MinValue;
    private bool _customerServiceEnabled;
    private Model _customerPedModel;
    private Model _customerVehicleModel;
    private int _customerIntervalSeconds;
    private bool _driveInEnabled;
    private int _driveArrivalTimeout;
    private float _driveSpeed;
    private bool _customerBlipsEnabled;
    // Fine parking is opt-out to preserve older installations without new Settings.ini keys.
    private bool _fineParkingEnabled;
    // Optional low-speed reverse maneuver at departure, with a rear-clearance check.
    private bool _valetReverseOut;
    // Suppress routine driving ticker spam; preserve order/finance and manual status notices.
    private static bool _compactCustomerNotices = true;
    private int _parkingTimeoutSeconds;
    private float _parkingRadius;
    private string _lastOrderOutcome = "NONE";
    private const int TrainingCost = 750;
    private const int SpecialistTrainingCost = 350;
    private readonly string[] _staffNames = { "Luis Moreno", "Derek Price", "Andre Bell", "Malik Brooks",
        "Tony Reyes", "Jesse Cruz", "Marcus Cole", "Eddie Hayes" };
    private int _editingStaffSlot = -1;
    private long _trainingSpend;
    private int _customerCompleted;
    private int _lastCustomerOrderId;
    private long _customerRevenue;
    private long _partsSalesRevenue, _partsSalesUnitsSold;
    private int _lastPartsSaleGameTime = int.MinValue;
    private DateTime _nextCustomerAt = DateTime.MinValue;
    private readonly Random _customerRandom = new Random();
    private const string InvoiceFile = @"scripts\GlassHeroesBusiness\CustomerOrders.csv";
    private bool _receptionistHired;
    private Model _receptionistModel;
    private string _receptionistModelName;
    private int _receptionistHiringCost;
    private int _receptionistWage;
    private bool _warnedInvalidReceptionistModel;
    private bool _routeRecoveryEnabled;
    private int _recoveryLimit;
    private int _stallSeconds;
    // Shared clear walking corridor (office -> repair floor), in walking order.
    private const int OptionalPathPointCount = 5;
    // Up to five OPTIONAL points per walking route; gaps are skipped in order.
    private readonly WorkBay[] _arrivalRoute = { new WorkBay(), new WorkBay(), new WorkBay(), new WorkBay(), new WorkBay() };
    private readonly WorkBay[] _serviceRoute = { new WorkBay(), new WorkBay(), new WorkBay(), new WorkBay(), new WorkBay() };
    // False by default: exterior parking lots use direct navmesh paths, never old MLO checkpoints.
    // When true, newly arriving staff and mechanic bay travel may follow optional points.
    // Customer-car pickup is ALWAYS direct, whichever walking mode is selected.
    private bool _customEmployeeWalkingPaths;
    private bool _customMechanicJobRoutes; // opt-in; saved route points stay intact
    private bool _pickupUsesArrivalExit = true;
    private bool _smoothMechanicWaypoints = true;
    private bool _safeTeleportOnStall = true;
    private int _routeConfigSlot = 0;
    private bool _routeConfigReturning;
    private bool _mechanicPathsFromManagement;
    private bool _receptionPathsFromManagement;
    private bool _managerPathsFromManagement;
    private bool _mechanicRosterFromManagement;
    // Receptionist routes are independent of mechanic routes. Preserve older saved
    // reception checkpoint/approach until the player chooses DIRECT or sets new points.
    private bool _customReceptionWalkingPaths;
    private readonly WorkBay[] _receptionRoute = { new WorkBay(), new WorkBay(), new WorkBay(), new WorkBay(), new WorkBay() };
    // Optional final turning point for each of the three physical bays.
    private readonly WorkBay[] _bayApproaches = { new WorkBay(), new WorkBay(), new WorkBay() };
    private bool _warnedMissingBreakArea;
    private bool _warnedMissingEntrance;
    private bool _workersPreviouslyInRange;
    private Model _mechanicModel;
    // Issue #20: retain only visual model requests owned by this v3 instance.
    private readonly HashSet<Model> _requestedVisualModels = new HashSet<Model>();
    private string _pedModelName;
    private int _spawnDistance;
    private int _arrivalSpacingSeconds;
    private float _walkSpeed;
    private float _breakSpacing;
    private bool _warnedInvalidModel;
    private double _visualRefreshClock;
    private ScriptSettings _configuration;
    // v0.2.6: Menu keybinds live in Settings.ini, never in the business Save.ini.
    // An old Settings.ini without [Controls] safely defaults to F2 without Ctrl.
    private Keys _menuKey = Keys.F2;
    private bool _requireCtrlForMenu;
    private DateTime _lastTick;
    // Diagnostic boot guard: wall clock is used ONLY for initialization deferral,
    // never for earnings, worker wages, remote jobs or supplier progress.
    private DateTime _bootGateUntilUtc = DateTime.MinValue;
    private bool _bootGameplayActivated;
    private string _diagnosticTickStage = "startup gate";
    private bool _diagnosticHoldGameplayTick;
    private int _diagnosticDelaySeconds = 30;
    private bool _showMenu;
    private MenuPage _menuPage = MenuPage.Business;
    private MenuPage _editorReturnPage = MenuPage.EmployeeSettings;
    private int _editorReturnSelection;
    private WorkBay _editingPoint;
    private string _editingPointLabel = "";
    private int _editingBayIndex = -1;
    private bool _editingRoutePoint;
    // Debug draw markers are session-only; the saved marker COORDINATES remain permanent.
    private bool _showPlacementMarkers;
    private bool _clockedIn;
    private bool _placed;
    private int _selection;
    private int _maxMechanics;
    private int _partCost;
    private int _jobRevenue;
    private int _wagePerEmployee;
    private int _rentAndUtilities;
    private int _onboardingCost;
    private int _jobSeconds;
    private int _betweenJobsSeconds;
    private int _wagePeriodSeconds;
    private int _overheadPeriodSeconds;
    // Billable liabilities are persisted separately from lifetime PAID expenses.
    // Existing installations start at zero due: historical wages/rent were already paid.
    private long _receptionWagesDue;
    private long _formerStaffWagesDue;
    private long _rentUtilitiesDue;
    // The oldest invoice ages only during open, unpaused gameplay. Keep morale informative:
    // no job cancellations, strikes or automatic firings in this release.
    private double _receptionWageUnpaidSeconds;
    private double _rentUnpaidSeconds;
    // Remote operations are advanced ONLY by GTA Game.GameTime while the game is active.
    // Never persist the clock sample; starting a new session cannot create offline earnings.
    private int _lastGameTimeSample;
    private bool _remoteMode;
    private readonly List<RemoteJob> _remoteJobs = new List<RemoteJob>();
    private double _remoteIdleGameSeconds;
    private int _remoteJobGameSeconds = 180;
    private float _remoteDistanceMeters = 200f;
    private int _managerTier = -1;
    private int _managerXp;
    private string _managerName = "";
    private Ped _managerWorker;
    private Model _managerPedModel;
    private readonly WorkBay _managerWorkstation = new WorkBay();
    // v0.3.3: independent receptionist-style manager entrance-to-desk route.
    private readonly WorkBay _managerApproach = new WorkBay();
    private readonly WorkBay[] _managerRoute = { new WorkBay(), new WorkBay(), new WorkBay(), new WorkBay(), new WorkBay() };
    private bool _customManagerWalkingPaths;
    private readonly List<Vector3> _managerPathNodes = new List<Vector3>();
    private int _managerNodeIndex;
    private DateTime _managerNodeSince = DateTime.MinValue;
    private float _managerLastDistanceSq = float.MaxValue;
    private DateTime _managerLastProgress = DateTime.MinValue;
    private int _managerRouteRetries;
    private DateTime _nextManagerSpawn = DateTime.MinValue;
    private DateTime _nextManagerTask = DateTime.MinValue;
    private bool _managerAtPost;
    private long _managerWagesDue;
    private double _managerWageAgeSeconds;
    private long _managerSavings;
    private long _remoteSales;
    private long _remoteJobsCompleted;
    private long _remotePartsConsumed;
    private int _payrollDueSeconds = 300;
    private int _payrollGraceSeconds = 300;
    private bool _receptionOverdueNotified;
    private readonly HashSet<int> _mechanicOverdueNotified = new HashSet<int>();
    private bool _shiftReportReady;
    private bool _financesFromExpenses;
    private long _shiftSalesBaseline;
    private long _shiftCostsBaseline;
    private bool _breakAnimationsEnabled;
    private float _queueSpawnSafetyMeters;
    private int _selectedInvoiceMechanicSlot;
    private readonly string[] _breakScenarios = {
        "WORLD_HUMAN_CLIPBOARD", "WORLD_HUMAN_STAND_MOBILE",
        "WORLD_HUMAN_SMOKING", "WORLD_HUMAN_AA_COFFEE"
    };

    // v0.3.2.0 — repair-specific stock is additional to legacy universal parts.
    // All older Save.ini entries still load; unpurchased specialty stock is zero.
    private readonly int[] _specialStock = new int[3];
    private readonly int[] _specialUnitCosts = { 90, 160, 125 };
    private static readonly string[] _stockNames = { "Service filters", "Brake kits", "Diagnostic supplies" };
    private int _selectedSupplier = 1; // 0 wholesale, 1 local, 2 express
    // One outstanding supplier delivery. NEW orders are billed at receipt;
    // purchases pending in older saves retain their paid-at-order state.
    // -2 = none, -1 = universal, 0/1/2 = specialized filter/brake/diagnostic stock.
    private int _deliveryKind = -2;
    private int _deliveryQuantity;
    private int _deliverySupplier;
    private long _deliveryCost;
    private long _deliveryDueUtcTicks; // order identity / legacy migration only; never gates delivery
    private double _deliveryRemainingGameSeconds; // persists; only unpaused GTA gameplay decrements
    private double _supplierTimerSaveSeconds;
    private DateTime _cancelSupplierConfirmUntil = DateTime.MinValue;
    // Mule delivery is optional for Local and Express; Wholesale becomes a manual
    // warehouse pickup mission when its location is configured. Legacy paid orders survive.
    private bool _physicalMuleDeliveries = true;
    private readonly WorkBay _supplierTruckSpawn = new WorkBay();
    private readonly WorkBay _supplierTruckPark = new WorkBay();
    private readonly WorkBay _supplierHandoff = new WorkBay();
    private readonly WorkBay _supplierPartsPallet = new WorkBay();
    // Persistent modular parts room: the legacy first pallet remains included.
    // Extra storage needs both purchase AND physical placement.
    private const int MaxStoragePallets = 4;
    private const int PartsPerPallet = 25;
    private const int CratesPerPallet = 9;
    private readonly Dictionary<long, PalletBoxLayout> _palletBoxLayouts = new Dictionary<long, PalletBoxLayout>();
    private const long ExtraPalletPrice = 750;
    private int _ownedStoragePallets = 1;
    private readonly WorkBay[] _extraStoragePallets = { new WorkBay(), new WorkBay(), new WorkBay() };
    private readonly int[] _storedDeliveryCrates = new int[MaxStoragePallets];
    private readonly Prop[] _storagePalletProps = new Prop[MaxStoragePallets];
    private readonly List<Prop> _storedCrateProps = new List<Prop>(); // obsolete display ownership only
    private readonly Prop[] _deliveredPackageProps = new Prop[MaxStoragePallets * CratesPerPallet];
    private readonly int[] _deliveredPackageMasks = new int[MaxStoragePallets];
    private DateTime _nextStorageVisualAttempt = DateTime.MinValue;
    private int _storageVisualAttempts;
    private bool _palletVisualDirty = true;
    private readonly WorkBay _wholesaleWarehousePickup = new WorkBay();
    private Prop _wholesaleWarehousePallet; // visual-only, never counted as shop storage
    private const float WholesalePickupX = -164.2800f;
    private const float WholesalePickupY = -2676.5900f;
    private const float WholesalePickupZ = 6.0141f;
    private string _wholesaleVehiclePlate = "";
    private int _wholesaleVehicleModel;
    private bool _wholesaleCargoLoaded;
    private bool _wholesaleCargoUnloaded;
    // Heavy-script-load safeguard: resolve the assigned pickup vehicle no more than
    // once per 750 ms; re-use the last matching entity while it remains in range.
    private Vehicle _cachedWholesaleVehicle;
    private DateTime _nextWholesaleVehicleLookup = DateTime.MinValue;
    private DateTime _nextWholesalePlateCheck = DateTime.MinValue;
    private bool _wholesaleCachedPlateValid;
    private Vector3 _lastWholesaleLookupCenter;
    private DateTime _nextSupplierInteractionHint = DateTime.MinValue;
    private DateTime _lastSupplierCheckpoint = DateTime.MinValue;
    private DateTime _nextWarehouseVisualAttempt = DateTime.MinValue;
    private int _warehouseVisualAttempts;
    private long _supplierInvoicesDue; // Earned at stocking, paid only in Expenses.
    private bool _deliveryPrepaid; // Legacy pending orders were paid up-front; no second charge.
    private Prop _supplierPalletProp; // legacy field retained for cleanup compatibility
    private bool _supplierPlayerCarrying;
    private readonly int[] _muleLiveryBySupplier = { 0, 1, 2 };
    private readonly WorkBay[] _supplierDriverWaypoints = { new WorkBay(), new WorkBay(), new WorkBay() };
    private string _muleModelName = "mule2"; // GTA V ramp-door Mule variant; configurable
    private Model _muleModel, _supplierDriverModel;
    private Vehicle _supplierTruck;
    private Ped _supplierDriver;
    private Prop _supplierCrate;
    private int _supplierVisualStage; // 0 absent, 1 drive, 2 exit, 3 walk, 4 await handoff, 5 depart, 6 player carrying
    private int _supplierRouteIndex;
    private bool _supplierDirectToPlayer;
    private DateTime _supplierPlayerApproachDeadline = DateTime.MinValue;
    private DateTime _nextSupplierPlayerRepath = DateTime.MinValue;
    private DateTime _nextSupplierVisualRetryUtc = DateTime.MinValue;
    private int _supplierPlayerRepaths;
    private bool _supplierExitIssued, _supplierDepartIssued;
    private DateTime _supplierStageAt = DateTime.MinValue;
    private long _supplierVisualOrderTicks;
    private readonly List<Vector3> _supplierWalkingNodes = new List<Vector3>();
    private DateTime _lastSupplierAlert = DateTime.MinValue;
    private bool _supplierBoxAttachedToDriver;
    private bool _muleRearDoorsOpened;
    private DateTime _lastCarrierAnimation = DateTime.MinValue;
    private int _receptionistTier = 2; // pre-roster saves with staff default to 3-slot veteran, preserve prior behavior
    private int _receptionistWageBonus;
    private string _receptionistName = "Receptionist";
    private int _reorderThreshold = 3;
    private int _supplierWholesaleSeconds = 180;
    private int _supplierLocalSeconds = 75;
    private int _supplierExpressSeconds = 20;
    private long _completedSupplierDeliveries;
    private bool _toolSetPurchased, _diagnosticScannerPurchased, _officeDeskPurchased, _officeDeskPlaced;
    private long _equipmentSpend;
    private readonly WorkBay _officeDeskLocation = new WorkBay();
    private Prop _officeDeskProp, _officeLaptopProp;
    // Equipment visuals are independent of ownership: removing a prop never removes its upgrade.
    private readonly WorkBay _toolSetLocation = new WorkBay();
    private readonly WorkBay _scannerLocation = new WorkBay();
    private bool _toolSetPlaced, _scannerPlaced;
    private Prop _toolSetProp, _scannerBenchProp, _scannerTabletProp;
    private DateTime _nextEquipmentSpawn = DateTime.MinValue;
    private DateTime _nextOfficeSpawn = DateTime.MinValue;
    private DateTime _nextDeskHint = DateTime.MinValue;
    private bool _controllerEnabled;
    private GTA.Control _controllerOpenModifier = GTA.Control.FrontendLb;
    private GTA.Control _controllerOpenButton = GTA.Control.ScriptPadRight;
    private GTA.Control _controllerUp = GTA.Control.FrontendUp;
    private GTA.Control _controllerDown = GTA.Control.FrontendDown;
    private GTA.Control _controllerAccept = GTA.Control.FrontendAccept;
    private GTA.Control _controllerBack = GTA.Control.FrontendCancel;
    private GTA.Control _controllerInteract = GTA.Control.Context;
    private DateTime _lastControllerAction = DateTime.MinValue;
    private DateTime _lastControllerToggle = DateTime.MinValue;
    private readonly HashSet<Keys> _heldKeys = new HashSet<Keys>();
    private readonly MenuInputGate _menuInput = new MenuInputGate();
    private string _lastPadDetected = "NONE";
    private DateTime _lastPadDetectedAt = DateTime.MinValue;
    private DateTime _lastPadReadErrorAt = DateTime.MinValue;
    private long _cash;
    private long _revenue;
    private long _partsSpend;
    private long _payrollSpend;
    private long _overheadSpend;
    private long _hireSpend;
    private const int MaxSavedRepairPromises = 16;
    private readonly List<SavedRepairPromise> _pendingRepairPromises = new List<SavedRepairPromise>();
    private readonly long[] _unitCostEstimates = new long[4]; // universal, filters, brakes, diagnostics
    private int _parts;
    private int _completed;
    private double _payrollClock;
    private double _overheadClock;
    private Vector3 _location;

    public GlassHeroesBusinessV3Experimental()
    {
        try
        {
            RequireExclusiveBusinessInstallation(File.Exists);
            Directory.CreateDirectory(DataDirectory);
            Log("BOOT v0.3.3.4 SHVDN3 SERVICE BAY INTEGRATION EXPERIMENTAL / constructor entered", null);
            _configuration = ScriptSettings.Load(SettingsFile);
            _diagnosticHoldGameplayTick = _configuration.GetValue("Diagnostics", "HoldGameplayTick", false);
            _diagnosticDelaySeconds = Math.Max(10, Math.Min(180,
                _configuration.GetValue("Diagnostics", "StartupDelaySeconds", 30)));
            Log("BOOT configuration loaded; HoldGameplayTick=" + _diagnosticHoldGameplayTick +
                "; StartupDelaySeconds=" + _diagnosticDelaySeconds, null);
            // F2 is the default menu key. Players can require Ctrl+F2 from Settings.ini.
            // Only plain F-keys are accepted, so no invalid modifier combinations
            // or accidental key codes can disable the menu entirely.
            string keySetting = _configuration.GetValue("Controls", "MenuKey", "F2");
            Keys parsedMenuKey;
            if (!Enum.TryParse<Keys>(keySetting.Trim(), true, out parsedMenuKey) ||
                parsedMenuKey < Keys.F1 || parsedMenuKey > Keys.F24)
            {
                Log("Controls", new ArgumentException("Unsupported MenuKey: " + keySetting + ". Using F2."));
                parsedMenuKey = Keys.F2;
            }
            _menuKey = parsedMenuKey;
            _requireCtrlForMenu = _configuration.GetValue("Controls", "RequireCtrl", false);
            _controllerEnabled = _configuration.GetValue("Gamepad", "Enabled", false);
            _controllerOpenModifier = ReadPadControl("OpenModifier", GTA.Control.FrontendLb);
            _controllerOpenButton = ReadPadControl("OpenButton", GTA.Control.ScriptPadRight);
            _controllerUp = ReadPadControl("MoveUp", GTA.Control.FrontendUp);
            _controllerDown = ReadPadControl("MoveDown", GTA.Control.FrontendDown);
            _controllerAccept = ReadPadControl("Select", GTA.Control.FrontendAccept);
            _controllerBack = ReadPadControl("Back", GTA.Control.FrontendCancel);
            _controllerInteract = ReadPadControl("Interact", GTA.Control.Context);
            _maxMechanics = Math.Max(1, Math.Min(3, _configuration.GetValue("Economy", "MaxMechanics", 3)));
            _partCost = Math.Max(1, _configuration.GetValue("Economy", "PartUnitCost", 100));
            _supplierWholesaleSeconds = Math.Max(10, Math.Min(3600,
                _configuration.GetValue("Suppliers", "WholesaleDeliverySeconds", 180)));
            _supplierLocalSeconds = Math.Max(10, Math.Min(3600,
                _configuration.GetValue("Suppliers", "LocalDeliverySeconds", 75)));
            _supplierExpressSeconds = Math.Max(10, Math.Min(3600,
                _configuration.GetValue("Suppliers", "ExpressDeliverySeconds", 20)));
            _physicalMuleDeliveries = _configuration.GetValue("Suppliers", "PhysicalMuleDeliveries", true);
            _muleModelName = _configuration.GetValue("Suppliers", "MuleModel", "mule2");
            _muleLiveryBySupplier[0] = _configuration.GetValue("Suppliers", "WholesaleMuleLivery", 0);
            _muleLiveryBySupplier[1] = _configuration.GetValue("Suppliers", "LocalMuleLivery", 1);
            _muleLiveryBySupplier[2] = _configuration.GetValue("Suppliers", "ExpressMuleLivery", 2);
            _supplierDriverModelName = _configuration.GetValue("Suppliers", "DeliveryDriverModel", "s_m_m_trucker_01");
            _jobRevenue = Math.Max(1, _configuration.GetValue("Economy", "JobRevenue", 500));
            _wagePerEmployee = Math.Max(0, _configuration.GetValue("Economy", "WagePerPeriod", 250));
            _rentAndUtilities = Math.Max(0, _configuration.GetValue("Economy", "OverheadPerPeriod", 575));
            _onboardingCost = Math.Max(0, _configuration.GetValue("Economy", "HiringCost", 500));
            _jobSeconds = Math.Max(15, _configuration.GetValue("Economy", "SecondsPerEmployeeJob", 120));
            _betweenJobsSeconds = Math.Max(10, _configuration.GetValue("Economy", "SecondsBetweenEmployeeJobs", 45));
            _wagePeriodSeconds = Math.Max(30, _configuration.GetValue("Economy", "WagePeriodSeconds", 300));
            InitialiseRepairPricing();
            _overheadPeriodSeconds = Math.Max(30, _configuration.GetValue("Economy", "OverheadPeriodSeconds", 600));
            _remoteJobGameSeconds = Math.Max(60, Math.Min(1200,
                _configuration.GetValue("Management", "SecondsPerRemoteRepair", 180)));
            _remoteDistanceMeters = Math.Max(90f, Math.Min(700f,
                _configuration.GetValue("Management", "AwayDistanceMeters", 200f)));
            _payrollDueSeconds = Math.Max(60, Math.Min(86400,
                _configuration.GetValue("Economy", "PayrollDueSeconds", 300)));
            _payrollGraceSeconds = Math.Max(0, Math.Min(86400,
                _configuration.GetValue("Economy", "PayrollGraceSeconds", 300)));
            // Visual workers are optional and deliberately separate from the economy.
            _pedModelName = _configuration.GetValue("Visuals", "MechanicPedModel", "s_m_m_autoshop_01");
            _spawnDistance = Math.Max(20, Math.Min(100, _configuration.GetValue("Visuals", "SpawnDistanceMeters", 65)));
            _arrivalSpacingSeconds = Math.Max(2, Math.Min(30, _configuration.GetValue("Visuals", "ArrivalSpacingSeconds", 5)));
            _walkSpeed = Math.Max(0.65f, Math.Min(2.0f, _configuration.GetValue("Visuals", "WalkSpeed", 1.0f)));
            _breakSpacing = Math.Max(0.5f, Math.Min(2.0f, _configuration.GetValue("Visuals", "BreakSpotSpacingMeters", 1.0f)));
            _breakAnimationsEnabled = _configuration.GetValue("Visuals", "BreakAnimationsEnabled", true);
            _receptionistModelName = _configuration.GetValue("Visuals", "ReceptionistPedModel", "s_f_y_shop_mid");
            _managerPedModelName = _configuration.GetValue("Management", "ManagerPedModel", "a_m_y_business_01");
            _customerPedModelName = _configuration.GetValue("CustomerService", "CustomerPedModel", "a_m_y_business_01");
            _customerVehicleModelName = _configuration.GetValue("CustomerService", "CustomerVehicleModel", "asea");
            _customerIntervalSeconds = Math.Max(30, Math.Min(900,
                _configuration.GetValue("CustomerService", "SecondsBetweenCustomers", 150)));
            _driveInEnabled = _configuration.GetValue("CustomerService", "DriveInEnabled", false);
            _driveArrivalTimeout = Math.Max(20, Math.Min(120,
                _configuration.GetValue("CustomerService", "DriveArrivalTimeoutSeconds", 65)));
            _driveSpeed = Math.Max(3f, Math.Min(18f,
                _configuration.GetValue("CustomerService", "DriveSpeed", 9.0f)));
            _customerBlipsEnabled = _configuration.GetValue("CustomerService", "CustomerBlipsEnabled", true);
            _customerQueueCapacity = Math.Max(1, Math.Min(3,
                _configuration.GetValue("CustomerService", "PendingCustomerLimit", 3)));
            _dropoffClearanceRadius = Math.Max(2.5f, Math.Min(8.0f,
                _configuration.GetValue("CustomerService", "DropoffClearanceMeters", 4.5f)));
            // Avoid spawning on top of the player, but do not require them to leave
            // the entire parking lot just to observe the queued car arrive.
            _queueSpawnSafetyMeters = Math.Max(3.0f, Math.Min(8.0f,
                _configuration.GetValue("CustomerService", "QueueSpawnSafetyMeters", 4.0f)));
            _vehicleTransfersEnabled = _configuration.GetValue("CustomerService", "VehicleTransfersEnabled", true);
            _vehicleTransferTimeout = Math.Max(35, Math.Min(180,
                _configuration.GetValue("CustomerService", "VehicleTransferTimeoutSeconds", 90)));
            _vehicleTransferSpeed = Math.Max(2.0f, Math.Min(12.0f,
                _configuration.GetValue("CustomerService", "VehicleTransferSpeed", 5.0f)));
            _automaticLotMode = _configuration.GetValue("CustomerService", "AutomaticLotMode", false);
            _autoLotRightSide = _configuration.GetValue("CustomerService", "AutoLotRightSide", true);
            _autoLotOffsetMeters = Math.Max(3.0f, Math.Min(7.0f,
                _configuration.GetValue("CustomerService", "AutoLotOffsetMeters", 4.0f)));
            _fineParkingEnabled = _configuration.GetValue("CustomerService", "FineParkingEnabled", true);
            _valetReverseOut = _configuration.GetValue("CustomerService", "ValetReverseOut", false);
            _compactCustomerNotices = _configuration.GetValue("CustomerService", "CompactNotifications", true);
            _parkingTimeoutSeconds = Math.Max(12, Math.Min(60,
                _configuration.GetValue("CustomerService", "ParkingTimeoutSeconds", 25)));
            _parkingRadius = Math.Max(3f, Math.Min(20f,
                _configuration.GetValue("CustomerService", "ParkingRadiusMeters", 10f)));
            _receptionistHiringCost = Math.Max(0, _configuration.GetValue("Economy", "ReceptionistHiringCost", 350));
            _receptionistWage = Math.Max(0, _configuration.GetValue("Economy", "ReceptionistWagePerPeriod", 150));
            _recoveryLimit = Math.Max(1, Math.Min(5, _configuration.GetValue("Visuals", "MaxTeleportsPerRoute", 3)));
            _stallSeconds = Math.Max(4, Math.Min(30, _configuration.GetValue("Visuals", "RouteNoProgressSeconds", 9)));

            ScriptSettings data = ScriptSettings.Load(SaveFile);
            Log("BOOT save file opened", null);
            _cash = data.GetValue("Business", "Cash", (long)_configuration.GetValue("Economy", "TestStartingFunds", 5000));
            _revenue = data.GetValue("Business", "Revenue", 0L);
            _partsSpend = data.GetValue("Business", "PartsSpent", 0L);
            _payrollSpend = data.GetValue("Business", "PayrollSpent", 0L);
            _overheadSpend = data.GetValue("Business", "OverheadSpent", 0L);
            _hireSpend = data.GetValue("Business", "HiringSpent", 0L);
            _parts = Math.Max(0, data.GetValue("Business", "Parts", 0));
            _completed = Math.Max(0, data.GetValue("Business", "CompletedEmployeeJobs", 0));
            _receptionistHired = data.GetValue("Business", "ReceptionistHired", false);
            _receptionistTier = Math.Max(0, Math.Min(2, data.GetValue("ReceptionistProfile", "Tier", 2)));
            _receptionistWageBonus = Math.Max(0, data.GetValue("ReceptionistProfile", "WageBonus", 0));
            _receptionistName = data.GetValue("ReceptionistProfile", "Name", "Receptionist");
            _customerServiceEnabled = data.GetValue("CustomerService", "Enabled", false);
            _customerCompleted = Math.Max(0, data.GetValue("CustomerService", "CompletedOrders", 0));
            _lastOrderOutcome = data.GetValue("CustomerService", "LastOutcome", "NONE");
            _lastCustomerOrderId = Math.Max(0, data.GetValue("CustomerService", "LastOrderId", 0));
            _customerRevenue = data.GetValue("CustomerService", "GrossRevenue", 0L);
            _partsSalesRevenue = Math.Max(0, data.GetValue("Business", "PartsSalesRevenue", 0L));
            _partsSalesUnitsSold = Math.Max(0, data.GetValue("Business", "PartsSalesUnitsSold", 0L));
            _trainingSpend = Math.Max(0L, data.GetValue("Business", "TrainingSpent", 0L));
            for (int stock = 0; stock < 3; stock++)
                _specialStock[stock] = Math.Max(0, data.GetValue("DetailedInventory", "Stock" + stock, 0));
            _selectedSupplier = Math.Max(0, Math.Min(2, data.GetValue("Purchasing", "Supplier", 1)));
            _reorderThreshold = Math.Max(1, Math.Min(20, data.GetValue("Purchasing", "ReorderThreshold", 3)));
            _deliveryKind = data.GetValue("Purchasing", "DeliveryKind", -2);
            _deliveryQuantity = Math.Max(0, data.GetValue("Purchasing", "DeliveryQuantity", 0));
            _deliverySupplier = Math.Max(0, Math.Min(2, data.GetValue("Purchasing", "DeliverySupplier", 1)));
            _physicalMuleDeliveries = data.GetValue("SupplierVisuals", "PhysicalMuleDeliveries", _physicalMuleDeliveries);
            LoadPoint(data, "SupplierTruckSpawn", _supplierTruckSpawn);
            LoadPoint(data, "SupplierTruckPark", _supplierTruckPark);
            LoadPoint(data, "SupplierDriverHandoff", _supplierHandoff);
            LoadPoint(data, "SupplierPartsPallet", _supplierPartsPallet);
            for (int w = 0; w < 3; w++)
                LoadPoint(data, "SupplierDriverWaypoint" + (w + 1), _supplierDriverWaypoints[w]);
            _deliveryCost = Math.Max(0L, data.GetValue("Purchasing", "DeliveryCost", 0L));
            _deliveryPrepaid = data.GetValue("Purchasing", "DeliveryPrepaid", true);
            _supplierInvoicesDue = Math.Max(0L, data.GetValue("BusinessBills", "SupplierInvoicesDue", 0L));
            _ownedStoragePallets = Math.Max(1, Math.Min(MaxStoragePallets,
                data.GetValue("Storage", "OwnedPallets", 1)));
            // Fixed warehouse point: old user-placed coordinates may no longer override it.
            _wholesaleWarehousePickup.IsSet = true;
            _wholesaleWarehousePickup.Position = new Vector3(WholesalePickupX, WholesalePickupY, WholesalePickupZ);
            _wholesaleWarehousePickup.Heading = 0f;
            _wholesaleVehiclePlate = data.GetValue("WholesaleVehicle", "Plate", "");
            _wholesaleVehicleModel = data.GetValue("WholesaleVehicle", "Model", 0);
            _wholesaleCargoLoaded = data.GetValue("WholesaleVehicle", "CargoLoaded", false);
            _wholesaleCargoUnloaded = data.GetValue("WholesaleVehicle", "CargoUnloaded", false);
            for (int pallet = 0; pallet < MaxStoragePallets; pallet++)
            {
                if (pallet > 0) LoadPoint(data, "SupplierStoragePallet" + (pallet + 1), _extraStoragePallets[pallet - 1]);
                _storedDeliveryCrates[pallet] = Math.Max(0, Math.Min(CratesPerPallet,
                    data.GetValue("Storage", "BoxCount" + (pallet + 1), 0)));
                _deliveredPackageMasks[pallet] = CompatiblePackageVisualMask(_storedDeliveryCrates[pallet],
                    data.GetValue("Storage", "DeliveredPackageMask" + (pallet + 1), 0));
            }
            _palletVisualDirty = true;
            _deliveryDueUtcTicks = data.GetValue("Purchasing", "DeliveryDueUtcTicks", 0L);
            // Legacy saves lack a game-time remaining-duration key. Restart that
            // pending delivery's lead time rather than crediting time GTA was closed.
            _deliveryRemainingGameSeconds = data.GetValue("Purchasing", "DeliveryRemainingGameSeconds", -1.0);
            if (_deliveryRemainingGameSeconds < 0 || _deliveryRemainingGameSeconds > 604800)
                _deliveryRemainingGameSeconds = _deliveryKind == -2 ? 0 : SupplierDeliverySeconds(_deliverySupplier);
            _completedSupplierDeliveries = Math.Max(0L, data.GetValue("Purchasing", "CompletedDeliveries", 0L));
            if (_deliveryKind < -1 || _deliveryKind > 2 || _deliveryQuantity == 0 ||
                _deliveryDueUtcTicks < DateTime.MinValue.Ticks || _deliveryDueUtcTicks > DateTime.MaxValue.Ticks)
            { _deliveryKind = -2; _deliveryQuantity = 0; _deliveryDueUtcTicks = 0L; _deliveryRemainingGameSeconds = 0; }
            if (_deliveryKind == -2) { _wholesaleCargoLoaded = false; _wholesaleCargoUnloaded = false; }
            else if (_deliverySupplier == 0 && _wholesaleCargoUnloaded)
            {
                // Player carry props don't survive GTA exit/reload. Recover the
                // unfinished order at the fixed warehouse, never issue stock/invoice.
                _wholesaleCargoUnloaded = false;
                _wholesaleCargoLoaded = false;
            }
            _toolSetPurchased = data.GetValue("Equipment", "ToolSet", false);
            _diagnosticScannerPurchased = data.GetValue("Equipment", "Scanner", false);
            _officeDeskPurchased = data.GetValue("Equipment", "OfficeDeskPurchased", false);
            _officeDeskPlaced = data.GetValue("Equipment", "OfficeDeskPlaced", false);
            _toolSetPlaced = data.GetValue("Equipment", "ToolSetPlaced", false);
            _scannerPlaced = data.GetValue("Equipment", "ScannerPlaced", false);
            LoadPoint(data, "ToolSetPlacement", _toolSetLocation);
            LoadPoint(data, "ScannerPlacement", _scannerLocation);
            if (!_toolSetPurchased || !_toolSetLocation.IsSet) _toolSetPlaced = false;
            if (!_diagnosticScannerPurchased || !_scannerLocation.IsSet) _scannerPlaced = false;
            _equipmentSpend = Math.Max(0L, data.GetValue("Business", "EquipmentSpent", 0L));
            _controllerEnabled = data.GetValue("Gamepad", "Enabled", _controllerEnabled);
            LoadPoint(data, "OfficeDesk", _officeDeskLocation);
            if (!_officeDeskPurchased || !_officeDeskLocation.IsSet) _officeDeskPlaced = false;
            _receptionWagesDue = Math.Max(0L, data.GetValue("BusinessBills", "ReceptionistWagesDue", 0L));
            _formerStaffWagesDue = Math.Max(0L, data.GetValue("BusinessBills", "FormerStaffWagesDue", 0L));
            _rentUtilitiesDue = Math.Max(0L, data.GetValue("BusinessBills", "RentUtilitiesDue", 0L));
            _managerTier = Math.Max(-1, Math.Min(2, data.GetValue("Management", "Tier", -1)));
            _managerXp = Math.Max(0, data.GetValue("Management", "XP", 0));
            _managerName = data.GetValue("Management", "Name", "");
            _managerWagesDue = Math.Max(0L, data.GetValue("Management", "WagesDue", 0L));
            _managerWageAgeSeconds = Math.Max(0d, data.GetValue("Management", "WageAgeSeconds", 0d));
            _managerSavings = Math.Max(0L, data.GetValue("Management", "Savings", 0L));
            _remoteSales = Math.Max(0L, data.GetValue("Management", "RemoteRevenue", 0L));
            _remoteJobsCompleted = Math.Max(0L, data.GetValue("Management", "RemoteCompleted", 0L));
            _remotePartsConsumed = Math.Max(0L, data.GetValue("Management", "RemotePartsConsumed", 0L));
            LoadPoint(data, "ManagerWorkstation", _managerWorkstation);
            LoadPoint(data, "ManagerApproach", _managerApproach);
            _customManagerWalkingPaths = data.GetValue("EmployeeSettings", "CustomManagerPaths", false);
            for (int i = 0; i < OptionalPathPointCount; i++)
                LoadPoint(data, "ManagerWaypoint" + (i + 1), _managerRoute[i]);
            LoadPoint(data, "ReceptionistResumePost", _receptionist.SavedPost);
            _receptionWageUnpaidSeconds = Math.Max(0d, data.GetValue("BusinessBills", "ReceptionAgeSeconds", 0d));
            _rentUnpaidSeconds = Math.Max(0d, data.GetValue("BusinessBills", "RentAgeSeconds", 0d));
            _shiftReportReady = data.GetValue("ShiftReport", "Ready", false);
            _shiftSalesBaseline = Math.Max(0L, data.GetValue("ShiftReport", "SalesAtOpen", 0L));
            _shiftCostsBaseline = Math.Max(0L, data.GetValue("ShiftReport", "CostsAtOpen", 0L));
            _breakAnimationsEnabled = data.GetValue("EmployeeSettings", "BreakAnimationsEnabled", _breakAnimationsEnabled);
            _driveInEnabled = data.GetValue("CustomerService", "DriveInEnabled", _driveInEnabled);
            _customerBlipsEnabled = data.GetValue("CustomerService", "CustomerBlipsEnabled", _customerBlipsEnabled);
            _vehicleTransfersEnabled = data.GetValue("CustomerService", "VehicleTransfersEnabled", _vehicleTransfersEnabled);
            _automaticLotMode = data.GetValue("CustomerService", "AutomaticLotMode", _automaticLotMode);
            _autoLotRightSide = data.GetValue("CustomerService", "AutoLotRightSide", _autoLotRightSide);
            _valetReverseOut = data.GetValue("CustomerService", "ValetReverseOut", _valetReverseOut);
            _autoLotOffsetMeters = Math.Max(3f, Math.Min(7f,
                data.GetValue("CustomerService", "AutoLotOffsetMeters", _autoLotOffsetMeters)));
            _routeRecoveryEnabled = data.GetValue("EmployeeSettings", "TeleportOnBlockedRoute",
                _configuration.GetValue("Visuals", "TeleportOnBlockedRoute", true));
            _customEmployeeWalkingPaths = data.GetValue("EmployeeSettings", "CustomWalkingPaths", false);
            _customMechanicJobRoutes = data.GetValue("EmployeeSettings", "CustomJobRoutes", false);
            _pickupUsesArrivalExit = data.GetValue("EmployeeSettings", "PickupUseReverseArrivalWaypoints",
                _configuration.GetValue("EmployeeSettings", "PickupUseReverseArrivalWaypoints", true));
            _smoothMechanicWaypoints = data.GetValue("EmployeeSettings", "SmoothWaypointTransitions",
                _configuration.GetValue("Visuals", "SmoothWaypointTransitions", true));
            _safeTeleportOnStall = data.GetValue("EmployeeSettings", "SafeTeleportOnStall",
                _configuration.GetValue("Visuals", "SafeTeleportOnStall", true));
            _customerQueueCapacity = Math.Max(1, Math.Min(3,
                data.GetValue("CustomerService", "PendingCustomerLimit", _customerQueueCapacity)));

            _placed = data.GetValue("Shop", "Placed", false);
            _location = new Vector3(
                data.GetValue("Shop", "X", 0.0f),
                data.GetValue("Shop", "Y", 0.0f),
                data.GetValue("Shop", "Z", 0.0f));
            for (int i = 0; i < VisualBayCount; i++)
            {
                string section = "WorkBay" + (i + 1);
                _bays[i].IsSet = data.GetValue(section, "Set", false);
                _bays[i].Position = new Vector3(
                    data.GetValue(section, "X", 0.0f),
                    data.GetValue(section, "Y", 0.0f),
                    data.GetValue(section, "Z", 0.0f));
                _bays[i].Heading = data.GetValue(section, "Heading", 0.0f);
            }
            LoadPoint(data, "EmployeeEntrance", _employeeEntrance);
            LoadPoint(data, "WalkCheckpoint", _walkCheckpoint);
            LoadPoint(data, "EmployeeBreakArea", _breakArea);
            LoadPoint(data, "ReceptionDesk", _receptionDesk);
            LoadPoint(data, "ReceptionApproach", _receptionApproach);
            _customReceptionWalkingPaths = data.GetValue("EmployeeSettings", "CustomReceptionistPaths",
                data.GetValue("WalkCheckpoint", "Set", false) || _receptionApproach.IsSet);
            for (int i = 0; i < OptionalPathPointCount; i++)
                LoadPoint(data, "ReceptionWaypoint" + (i + 1), _receptionRoute[i]);
            LoadPoint(data, "CustomerDriveSpawn", _customerDriveSpawn);
            LoadPoint(data, "CustomerDropoff", _customerDropoff);
            for (int i = 0; i < VisualBayCount; i++)
                LoadPoint(data, "VehicleParkingBay" + (i + 1), _vehicleParkingBays[i]);
            LoadPoint(data, "CustomerApproach", _customerApproach);
            LoadPoint(data, "CustomerQueue", _customerQueue);
            for (int i = 0; i < _customerWalkingRoute.Length; i++)
                LoadPoint(data, "CustomerWalkingPoint" + (i + 1), _customerWalkingRoute[i]);
            LoadPoint(data, "CustomerQueueCar2", _queueCar2);
            LoadPoint(data, "CustomerQueueCar3", _queueCar3);
            for (int i = 0; i < OptionalPathPointCount; i++)
            {
                LoadPoint(data, "ArrivalWaypoint" + (i + 1), _arrivalRoute[i]);
                LoadPoint(data, "ServiceWaypoint" + (i + 1), _serviceRoute[i]);
            }
            for (int i = 0; i < VisualBayCount; i++)
                LoadPoint(data, "BayApproach" + (i + 1), _bayApproaches[i]);
            int mechanicCount = Math.Max(0, Math.Min(_maxMechanics, data.GetValue("Business", "Mechanics", 0)));
            for (int i = 0; i < mechanicCount; i++)
            {
                string section = "MechanicProfile" + (i + 1);
                Mechanic profile = new Mechanic();
                profile.Name = data.GetValue(section, "Name", _staffNames[i % _staffNames.Length]);
                profile.Xp = Math.Max(0, data.GetValue(section, "XP", 0));
                profile.Specialty = Math.Max(0, Math.Min(2, data.GetValue(section, "Specialty", 0)));
                profile.WageBonus = Math.Max(0, data.GetValue(section, "WageBonus", 0));
                profile.JobsCompleted = Math.Max(0, data.GetValue(section, "CompletedJobs", 0));
                profile.UnpaidWages = Math.Max(0L, data.GetValue(section, "WagesDue", 0L));
                profile.WageUnpaidSeconds = Math.Max(0d, data.GetValue(section, "WageAgeSeconds", 0d));
                profile.JobOutboundMask = data.GetValue(section, "JobOutboundMask", 31) & 31;
                profile.JobReturnMask = data.GetValue(section, "JobReturnMask", 31) & 31;
                LoadPoint(data, "MechanicResumePost" + (i + 1), profile.SavedPost);
                _mechanics.Add(profile);
            }

            LoadSupplierOrderLines(data);
            LoadRepairPromises(data);
            LogStartupSourceIdentity();
            Log("BOOT FinanceV1 read begin", null);
            LoadFinanceBook(data);
            Log("BOOT FinanceV1 read complete; retained rows=" + _financeBook.Entries.Count + "; activeMs=" + _financeActiveMs, null);

            _clockedIn = false; // Opening shift is a deliberate action after every load.
            _lastTick = DateTime.UtcNow;
            // Do not invoke GTA natives during the constructor while world/DLCs load.
            _lastGameTimeSample = 0;
            _bootGateUntilUtc = DateTime.UtcNow.AddSeconds(_diagnosticDelaySeconds);
            Log("BOOT save records parsed; registering callbacks", null);
            Interval = 0;
            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Aborted += OnAborted;
            Log("BOOT constructor completed; waiting for safe gameplay gate", null);
            // The old startup UI.Notify ran before the player/world was ready.
            // It is now issued only after successful player validation in OnTick.
        }
        catch (Exception ex)
        {
            Log("Startup failure", ex);
            throw;
        }
    }

    private string _supplierDriverModelName, _managerPedModelName, _customerPedModelName, _customerVehicleModelName;
    private bool _gameplayModelsReady;
    private int _gameplayModelAttempts;
    private DateTime _nextGameplayModelAttemptUtc;

    private bool TryInitializeGameplayModels(DateTime now)
    {
        if (_gameplayModelsReady) return true;
        if (_gameplayModelAttempts >= 3 || now < _nextGameplayModelAttemptUtc) return false;
        _gameplayModelAttempts++;
        _nextGameplayModelAttemptUtc = now.AddSeconds(5); // Startup retry throttle, never economic elapsed time.
        try
        {
            Log("BOOT gameplay model hashes begin; attempt=" + _gameplayModelAttempts, null);
            // Model(string) reaches Game.GenerateHash / SHVDN.NativeMemory.GetHashKey.
            // Invoke only after the delay and a valid, unpaused player. No model requests/spawns.
            Model mule = new Model(_muleModelName), driver = new Model(_supplierDriverModelName);
            Model mechanic = new Model(_pedModelName), receptionist = new Model(_receptionistModelName);
            Model manager = new Model(_managerPedModelName), customer = new Model(_customerPedModelName);
            Model vehicle = new Model(_customerVehicleModelName);
            _muleModel = mule; _supplierDriverModel = driver; _mechanicModel = mechanic;
            _receptionistModel = receptionist; _managerPedModel = manager;
            _customerPedModel = customer; _customerVehicleModel = vehicle;
            _gameplayModelsReady = true;
            Log("BOOT gameplay model hashes complete", null);
            return true;
        }
        catch (Exception ex)
        {
            Log("BOOT gameplay model hashes failed; attempt=" + _gameplayModelAttempts + "/3; business remains inactive", ex);
            return false; // An unmanaged access violation is not claimed catchable or fixed.
        }
    }

    private void LogStartupSourceIdentity()
    {
        const string path = @"scripts\GlassHeroesBusiness.3.cs";
        try
        {
            if (!File.Exists(path) || new FileInfo(path).Length > 2097152)
            { Log("BOOT source SHA256=UNVERIFIED; conventional source missing/oversized", null); return; }
            using (var stream = File.OpenRead(path))
            using (var sha = System.Security.Cryptography.SHA256.Create())
                Log("BOOT conventional source SHA256=" + BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(), null);
        }
        catch { Log("BOOT source SHA256=UNVERIFIED; source read failed", null); }
    }

    private void OnTick(object sender, EventArgs e)
    {
        try
        {
            // This branch intentionally performs NO GTA native calls until the
            // real-world loading buffer has elapsed. It creates no income/time.
            if (!_bootGameplayActivated)
            {
                if (_diagnosticHoldGameplayTick || DateTime.UtcNow < _bootGateUntilUtc)
                    return;
                Ped readyPlayer = Game.Player.Character;
                if (readyPlayer == null || !readyPlayer.Exists() || Game.IsPaused)
                    return;
                if (!TryInitializeGameplayModels(DateTime.UtcNow)) return;
                _lastGameTimeSample = Game.GameTime;
                _lastTick = DateTime.UtcNow;
                _bootGameplayActivated = true;
                Log("BOOT gameplay gate passed; OnTick systems enabled", null);
                Notify("Loaded. Press " + MenuKeyLabel() + " for business management. Stand at your shop to set its location.");
                return;
            }
            _diagnosticTickStage = "game clock";
            DateTime now = DateTime.UtcNow;
            double seconds = (now - _lastTick).TotalSeconds;
            _lastTick = now;
            // Avoid a giant back-bill if the game freezes, reloads, or is paused.
            if (seconds < 0) seconds = 0;
            if (seconds > 2) seconds = 2;
            if (Game.IsPaused) seconds = 0; // Do not advance employee shifts in the pause menu.
            int gameTimeNow = Game.GameTime;
            long elapsedGameMs = (long)gameTimeNow - _lastGameTimeSample;
            _lastGameTimeSample = gameTimeNow;
            // Clamp game-clock jumps from loading, cutscenes and script reloads.
            double activeGameSeconds = !Game.IsPaused && elapsedGameMs > 0 && elapsedGameMs <= 30000L
                ? Math.Min(2d, elapsedGameMs / 1000d) : 0d;
            _supplierGameplayNow = _supplierGameplayNow.AddSeconds(activeGameSeconds);
            _financeActiveMs = checked(_financeActiveMs + (long)Math.Round(activeGameSeconds * 1000d));
            _diagnosticTickStage = "remote mode transition";
            if (_clockedIn && _managerTier >= 0 && Game.Player.Character != null && _placed &&
                DistanceSquared(Game.Player.Character.Position, _location) >
                _remoteDistanceMeters * _remoteDistanceMeters)
            {
                if (!_remoteMode) EnterRemoteOperations();
            }
            else if (_remoteMode) LeaveRemoteOperations();
            _diagnosticTickStage = "economy";
            if (_clockedIn && activeGameSeconds > 0) ProcessEconomy(activeGameSeconds);
            // Advance supplier ETA only while GTA is actively running and unpaused.
            // Persist every 15 seconds of gameplay to avoid per-frame disk writes.
            _diagnosticTickStage = "supplier timer";
            if (_deliveryKind != -2 && activeGameSeconds > 0 && _deliveryRemainingGameSeconds > 0)
            {
                _deliveryRemainingGameSeconds = Math.Max(0, _deliveryRemainingGameSeconds - activeGameSeconds);
                _supplierTimerSaveSeconds += activeGameSeconds;
                if (_deliveryRemainingGameSeconds <= 0 || _supplierTimerSaveSeconds >= 15)
                { _supplierTimerSaveSeconds = 0; Save(); }
            }
            _diagnosticTickStage = "remote operations";
            if (_clockedIn && activeGameSeconds > 0 && (_remoteMode || _remoteJobs.Count > 0))
                ProcessRemoteOperations(activeGameSeconds);
            _diagnosticTickStage = "controller input";
            _nativePauseYielded = YieldMenuToNativePause();
            if (_showMenu && _controllerEnabled) DisableControllerControlsWhileMenuOpen();
            PollController();
            _diagnosticTickStage = "supplier interaction";
            HandleSupplierPackageInteraction();
            DrawWholesalePackageMarker();
            _diagnosticTickStage = "office interaction";
            HandleOfficeInteraction();
            // NPC lifecycle is throttled to once a second, never on every draw frame.
            _visualRefreshClock += seconds;
            if (_visualRefreshClock >= 1.0)
            {
                _visualRefreshClock = 0;
                _diagnosticTickStage = "staff visuals";
                UpdateVisualMechanics();
                _diagnosticTickStage = "supplier delivery visuals";
                try { ProcessSupplierDelivery(_supplierGameplayNow); }
                catch (Exception deliveryError)
                { Log("Mule delivery visual (order preserved)", deliveryError); DeferSupplierPlayerHandoff(_supplierGameplayNow); }
                _diagnosticTickStage = "customer visuals";
                UpdateCustomerVisuals();
                _diagnosticTickStage = "office visuals";
                UpdateOfficeDeskVisuals();
                _diagnosticTickStage = "equipment visuals";
                UpdateEquipmentVisuals();
                _diagnosticTickStage = "stockroom visuals";
                UpdateSupplierPalletVisual();
                _diagnosticTickStage = "warehouse visuals";
                UpdateWholesalePalletVisual();
                _diagnosticLog.RecordTransition(_clockedIn, _remoteMode,
                    _workersPreviouslyInRange, _showMenu, _supplierVisualStage,
                    _activeCustomerOrders.Count, _remoteJobs.Count);
            }
            // GTA help text is transient. Render it each game tick while the menu is open.
            // This affects UI presentation only; ProcessEconomy still uses elapsed seconds.
            _diagnosticTickStage = "menu rendering";
            if (_showMenu)
            {
                DisablePhoneWhileMenuOpen(); // GTA control disabling lasts one frame.
                DrawMenu();                 // Draw our full panel once per frame.
            }
            // Placement preview can remain on after F2 closes so you can WALK
            // around the custom MLO and inspect every marker from different angles.
            _diagnosticTickStage = "placement rendering";
            if (_showPlacementMarkers || _autoLotPreview) DrawPlacementMarkers();
            _diagnosticTickStage = "idle between ticks";
        }
        catch (Exception ex)
        {
            Log("Tick / " + _diagnosticTickStage, ex);
            _clockedIn = false; // Fail safely instead of repeatedly charging payroll.
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (_heldKeys.Remove(e.KeyCode))
            _menuInput.ReleaseKeyboard(MenuActionForKey(e.KeyCode), DateTime.UtcNow.Ticks);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            // No menu or supplier native calls before a live Story Mode player exists.
            if (!_bootGameplayActivated) return;
            if (e.KeyCode == Keys.Escape || Game.IsPaused)
            { HideBusinessMenuForPause(); return; }
            // The game's key callback may auto-repeat while Enter is held.
            // A single physical press must cause only one menu selection.
            if (!_heldKeys.Add(e.KeyCode)) return;
            int action = MenuActionForKey(e.KeyCode);
            if (action >= 0 && !_menuInput.TryKeyboard(action, DateTime.UtcNow.Ticks)) return;
            // Test KeyCode (not KeyData), since KeyData also contains Shift/Ctrl/Alt flags.
            // When RequireCtrl=false, F2 and Ctrl+F2 both toggle the menu.
            // When true, ordinary F2 is ignored; Ctrl+F2 is required.
            if (e.KeyCode == _menuKey && (!_requireCtrlForMenu || e.Control))
            {
                _showMenu = !_showMenu;
                if (_showMenu) { _menuPage = MenuPage.Business; _selection = 0; }
                return;
            }
            if (!_showMenu && e.KeyCode == Keys.E)
            {
                if (TrySupplierPackageInteraction()) return;
                if (IsPlayerAtOfficeDesk()) { OpenBusinessFromOffice(); return; }
            }
            if (!_showMenu) return;
            if (e.KeyCode == Keys.Back)
            {
                NavigateBack();
                return;
            }
            if (_menuPage == MenuPage.BuildSupplierOrder && (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right))
            { AdjustSupplierCart(e.KeyCode == Keys.Left ? -1 : 1); return; }
            int menuCount = GetMenuItems().Length;
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                _selection = (e.KeyCode == Keys.Up ?
                    _selection + menuCount - 1 : _selection + 1) % menuCount;
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                ExecuteSelection();
        }
        catch (Exception ex) { Log("Key press", ex); }
    }

    // ISSUE2: semantic input arbitration; no gameplay clock or disk access.
    private sealed class MenuInputGate
    {
        private readonly int[] _keyboardHeld = new int[7];
        private readonly long[] _keyboardAt = { -1, -1, -1, -1, -1, -1, -1 };
        private readonly long[] _controllerAt = { -1, -1, -1, -1, -1, -1, -1 };
        private const long EchoTicks = 250 * TimeSpan.TicksPerMillisecond;

        internal void Reset()
        {
            Array.Clear(_keyboardHeld, 0, _keyboardHeld.Length);
            for (int i = 0; i < _keyboardAt.Length; i++)
            { _keyboardAt[i] = -1; _controllerAt[i] = -1; }
        }

        internal bool TryKeyboard(int action, long now)
        {
            if (++_keyboardHeld[action] > 1) return false;
            _keyboardAt[action] = now;
            return !Recent(_controllerAt[action], now);
        }

        internal void ReleaseKeyboard(int action, long now)
        {
            if (action < 0) return;
            if (_keyboardHeld[action] > 0) _keyboardHeld[action]--;
            _keyboardAt[action] = now; // suppress a frontend edge delayed until release
        }

        internal bool TryController(int action, long now)
        {
            if (_keyboardHeld[action] > 0 || Recent(_keyboardAt[action], now)) return false;
            _controllerAt[action] = now;
            return true; // controller-only timing remains in PollController
        }

        private static bool Recent(long last, long now)
        {
            return last >= 0 && now >= last && now - last < EchoTicks;
        }
    }

    private int MenuActionForKey(Keys key)
    {
        if (key == Keys.Left) return 5;
        if (key == Keys.Right) return 6;
        if (key == Keys.Up) return 0;
        if (key == Keys.Down) return 1;
        if (key == Keys.Enter || key == Keys.Return) return 2;
        if (key == Keys.Back) return 3;
        return key == _menuKey ? 4 : -1;
    }

    private static string PointEditorActionLabel(int index, bool isSet, bool markers)
    {
        if (index == 0) return isSet ? "UPDATE / SAVE" : "SET / SAVE";
        if (index == 1) return isSet ? "CLEAR" : "NOT SET";
        if (index == 2) return markers ? "ON" : "OFF";
        return ">";
    }

    private string MenuKeyLabel()
    {
        return (_requireCtrlForMenu ? "CTRL+" : "") + _menuKey.ToString().ToUpperInvariant();
    }

    private void NavigateBack()
    {
        switch (_menuPage)
        {
            case MenuPage.Business: _showMenu = false; break;
            case MenuPage.BusinessManagement: _menuPage = MenuPage.Business; _selection = 3; break;
            case MenuPage.Employees: _menuPage = MenuPage.BusinessManagement; _selection = 0; break;
            case MenuPage.HireMechanicRoster: _menuPage = MenuPage.MechanicManagement; _selection = 1; break;
            case MenuPage.HireReceptionistRoster: _menuPage = MenuPage.ReceptionistManagement; _selection = 1; break;
            case MenuPage.SupplierDeliverySetup: _menuPage = MenuPage.InventoryManagement; _selection = 5; break;
            case MenuPage.SupplierPalletStorage: _menuPage = MenuPage.InventoryManagement; _selection = 4; break;
            case MenuPage.SupplierDriverWaypoints: _menuPage = MenuPage.SupplierDeliverySetup; _selection = 5; break;
            case MenuPage.StaffRoster: _menuPage = MenuPage.MechanicManagement; _selection = 0; break;
            case MenuPage.StaffProfile: _menuPage = MenuPage.StaffRoster; _selection = Math.Max(0, _editingStaffSlot); break;
            case MenuPage.MechanicManagement: _menuPage = MenuPage.Employees; _selection = 1; break;
            case MenuPage.ReceptionistManagement: _menuPage = MenuPage.Employees; _selection = 2; break;
            case MenuPage.ReceptionistProfile: _menuPage = MenuPage.ReceptionistManagement; _selection = 0; break;
            case MenuPage.ManagerManagement: _menuPage = MenuPage.Employees; _selection = 3; break;
            case MenuPage.ManagerProfile: _menuPage = MenuPage.ManagerManagement; _selection = 0; break;
            case MenuPage.ManagerWalkingPaths:
                _menuPage = _managerPathsFromManagement ? MenuPage.ManagerManagement : MenuPage.EmployeePaths;
                _selection = 2; _managerPathsFromManagement = false; break;
            case MenuPage.ManagerWaypoints: _menuPage = MenuPage.ManagerWalkingPaths; _selection = 1; break;
            case MenuPage.HireManagerRoster: _menuPage = MenuPage.ManagerManagement; _selection = 1; break;
            case MenuPage.EmployeeSettings: _menuPage = MenuPage.Employees; _selection = 4; break;
            case MenuPage.EmployeePlacements: _menuPage = MenuPage.EmployeeSettings; _selection = 0; break;
            case MenuPage.CustomerPlacements: _menuPage = MenuPage.Customers; _selection = 2; break;
            case MenuPage.CustomerWalkingRoutes: _menuPage = MenuPage.CustomerPlacements; _selection = 7; break;
            case MenuPage.CustomerQueueSettings: _menuPage = MenuPage.Customers; _selection = 1; break;
            case MenuPage.MechanicWalkingPaths:
                _menuPage = _mechanicPathsFromManagement ? MenuPage.MechanicManagement : MenuPage.EmployeePaths;
                _selection = _mechanicPathsFromManagement ? 3 : 0;
                _mechanicPathsFromManagement = false; break;
            case MenuPage.ReceptionWalkingPaths:
                _menuPage = _receptionPathsFromManagement ? MenuPage.ReceptionistManagement : MenuPage.EmployeePaths;
                _selection = _receptionPathsFromManagement ? 2 : 1;
                _receptionPathsFromManagement = false; break;
            case MenuPage.ReceptionWaypoints: _menuPage = MenuPage.ReceptionWalkingPaths; _selection = 1; break;
            case MenuPage.PointEditor:
                _menuPage = _editorReturnPage; _selection = _editorReturnSelection; break;
            case MenuPage.ServiceBayProfile: _menuPage = _serviceBayParent; _selection = _serviceBaySlot; break;
            case MenuPage.MechanicBays: _menuPage = MenuPage.EmployeePlacements; _selection = 1; break;
            case MenuPage.EmployeePaths: _menuPage = MenuPage.EmployeeSettings; _selection = 1; break;
            case MenuPage.ArrivalWaypoints: _menuPage = MenuPage.MechanicWalkingPaths; _selection = 2; break;
            case MenuPage.ServiceWaypoints: _menuPage = MenuPage.MechanicWalkingPaths; _selection = 3; break;
            case MenuPage.BayApproaches: _menuPage = MenuPage.MechanicWalkingPaths; _selection = 4; break;
            case MenuPage.RouteRecovery: _menuPage = MenuPage.EmployeeSettings; _selection = 2; break;
            case MenuPage.PlacementMarkers: _menuPage = MenuPage.EmployeeSettings; _selection = 3; break;
            case MenuPage.ServiceVehicleBays: _menuPage = _customCarBaysFromDriveOptions ?
                MenuPage.VehicleDriveOptions : MenuPage.CustomerPlacements;
                _selection = _customCarBaysFromDriveOptions ? 4 : 6; break;
            case MenuPage.VehicleDriveOptions: _menuPage = MenuPage.Customers; _selection = 3; break;
            case MenuPage.VehicleAutoLotOptions: _menuPage = MenuPage.VehicleDriveOptions; _selection = 3; break;
            case MenuPage.Customers: _menuPage = MenuPage.BusinessManagement; _selection = 1; break;
            case MenuPage.Finances:
                if (_financesFromExpenses) _financesFromExpenses = false;
                _menuPage = MenuPage.BusinessManagement; _selection = 3; break;
            case MenuPage.FinancialOverview: _menuPage = MenuPage.Finances; _selection = 0; break;
            case MenuPage.IncomeSales: _menuPage = MenuPage.Finances; _selection = 1; break;
            case MenuPage.BusinessExpenses: _menuPage = MenuPage.Finances; _selection = 2; break;
            case MenuPage.ProfitLoss: _menuPage = MenuPage.Finances; _selection = 3; break;
            case MenuPage.TransactionHistory: _menuPage = MenuPage.Finances; _selection = 4; break;
            case MenuPage.OwnerTransfers: _menuPage = MenuPage.Finances; _selection = 5; break;
            case MenuPage.PartsSales: _menuPage = MenuPage.InventoryManagement; _selection = 3; break;
            case MenuPage.InventoryOverview: _menuPage = MenuPage.InventoryManagement; _selection = 0; break;
            case MenuPage.BuildSupplierOrder: _menuPage = MenuPage.InventoryManagement; _selection = 1; break;
            case MenuPage.ActiveDeliveries: _menuPage = MenuPage.InventoryManagement; _selection = 2; break;
            case MenuPage.InventoryManagement: _menuPage = MenuPage.BusinessManagement; _selection = 2; break;
            case MenuPage.EquipmentManagement: _menuPage = MenuPage.BusinessExpenses; _selection = 7; break;
            case MenuPage.ControllerSettings: _menuPage = MenuPage.BusinessManagement; _selection = 4; break;
            case MenuPage.MechanicRouteRoster:
                _menuPage = _mechanicRosterFromManagement ? MenuPage.MechanicManagement : MenuPage.MechanicWalkingPaths;
                _selection = _mechanicRosterFromManagement ? 2 : 6;
                _mechanicRosterFromManagement = false; break;
            case MenuPage.MechanicRouteDirection: _menuPage = MenuPage.MechanicRouteRoster; _selection = _routeConfigSlot; break;
            case MenuPage.MechanicRoutePoints: _menuPage = MenuPage.MechanicRouteDirection; _selection = _routeConfigReturning ? 1 : 0; break;
            case MenuPage.MechanicInvoices: _menuPage = MenuPage.BusinessExpenses; _selection = 1; break;
        }
    }

    // Vehicle parking uses its own placement rules. Unlike employee walking routes,
    // these car spots can be updated while the shop is open, but never while an
    // active customer order may still be driving toward an old bay location.
    private int EditingVehicleParkingSlot()
    {
        for (int i = 0; i < VisualBayCount; i++)
            if (Object.ReferenceEquals(_editingPoint, _vehicleParkingBays[i])) return i;
        return -1;
    }

    private void OpenPointEditor(WorkBay point, string label, int bayIndex, bool routePoint)
    {
        _editingPoint = point;
        _editingPointLabel = label;
        _editingBayIndex = bayIndex;
        _editingRoutePoint = routePoint;
        _editorReturnPage = _menuPage;
        _editorReturnSelection = _selection;
        _menuPage = MenuPage.PointEditor;
        _selection = 0;
    }

    // GTA's phone-opening key (Up arrow) overlaps our menu navigation key.
    // Disable only the phone's input controls, only while our F2 menu is shown.
    // GTA resets disabled controls every frame, hence this runs in OnTick().
    private static void DisablePhoneWhileMenuOpen()
    {
        foreach (GTA.Control control in PhoneControlsToSuppress)
        {
            GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION,
                0, (int)control, true);
        }
    }

    // UIRectangle and UIText are SHVDN v2's built-in drawing primitives.
    // Positions are in the game's 1280x720 reference resolution.
    private static int CompactX(int x)
    {
        return MenuDrawLeft + (int)Math.Round((x - MenuDesignLeft) * MenuScale);
    }

    private static int CompactY(int y)
    {
        return MenuDrawTop + (int)Math.Round((y - MenuDesignTop) * MenuScale);
    }

    private static void Panel(int x, int y, int w, int h, Color color)
    {
        new GTA.UI.ContainerElement(new PointF(CompactX(x), CompactY(y)),
            new Size(Math.Max(1, (int)Math.Round(w * MenuScale)),
                Math.Max(1, (int)Math.Round(h * MenuScale))), color, false).Draw();
    }

    private static void Label(string caption, int x, int y, float scale, Color color)
    {
        new GTA.UI.TextElement(caption, new PointF(CompactX(x), CompactY(y)), scale * MenuScale, color,
            GTA.UI.Font.ChaletLondon, GTA.UI.Alignment.Left).Draw();
    }

    private static readonly Dictionary<string, string> _menuFitCache = new Dictionary<string, string>();

    private static string FitMenuText(string caption, float maxWidth, float scale)
    {
        string key = maxWidth + "|" + scale + "|" + caption;
        string fitted;
        if (_menuFitCache.TryGetValue(key, out fitted)) return fitted;
        fitted = caption;
        if (GTA.UI.TextElement.GetStringWidth(fitted, GTA.UI.Font.ChaletLondon, scale * MenuScale) > maxWidth * MenuScale)
        {
            int low = 0, high = Math.Min(128, caption.Length);
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (GTA.UI.TextElement.GetStringWidth(caption.Substring(0, mid) + "...", GTA.UI.Font.ChaletLondon, scale * MenuScale) <= maxWidth * MenuScale) low = mid;
                else high = mid - 1;
            }
            fitted = caption.Substring(0, low) + "...";
        }
        if (_menuFitCache.Count >= 128) _menuFitCache.Clear();
        _menuFitCache[key] = fitted;
        return fitted;
    }

    private static void LabelColumn(string caption, int x, int y, int width, float scale, Color color, bool right)
    {
        new GTA.UI.TextElement(FitMenuText(caption, width, scale), new PointF(CompactX(x), CompactY(y)),
            scale * MenuScale, color, GTA.UI.Font.ChaletLondon, right ? GTA.UI.Alignment.Right : GTA.UI.Alignment.Left).Draw();
    }

    private string[] GetMenuItems()
    {
        switch (_menuPage)
        {
            case MenuPage.BusinessManagement:
                return new string[] { "Employees >", "Customers >", "Inventory & Suppliers >",
                    "Finances & Accounting >", "Controller Controls >",
                    "Deposit $1,000", "Withdraw $1,000", "Set Shop Location",
                    "< Back to Main" };
            case MenuPage.Employees:
                return new string[] { "Employee Status", "Mechanics >", "Receptionist >",
                    "Business Manager >", "Employee Settings >", "< Back to Business Management" };
            case MenuPage.ManagerManagement:
                return new string[] { "Manager Profile >", "Hire Business Manager >",
                    "Manager Walking Paths >", "< Back to Employees" };
            case MenuPage.ManagerProfile:
                return new string[] { "Profile & Business Reports", "Train Manager (+40 XP)",
                    "Pay Manager Wage Invoice", "Dismiss Manager", "< Back to Manager" };
            case MenuPage.ManagerWalkingPaths:
                return new string[] { "Manager Walking Mode", "Spawn -> Workstation Waypoints >",
                    "Final Workstation Approach >", "< Back to Walking Paths" };
            case MenuPage.ManagerWaypoints:
                return new string[] { "Manager Stop 1 / 5 >", "Manager Stop 2 / 5 >",
                    "Manager Stop 3 / 5 >", "Manager Stop 4 / 5 >",
                    "Manager Stop 5 / 5 >", "< Back to Manager Paths" };
            case MenuPage.HireManagerRoster:
                return new string[] { "Assistant Manager — 3% discount",
                    "Experienced Manager — 6% discount", "General Manager — 10% discount",
                    "< Back to Management" };
            case MenuPage.MechanicManagement:
                return new string[] { "Mechanic Roster >", "Hire Mechanic >",
                    "Mechanic Job Waypoints >", "Mechanic Walking Paths >", "< Back to Employees" };
            case MenuPage.ReceptionistManagement:
                return new string[] { "Receptionist Profile >", "Hire Receptionist >",
                    "Receptionist Walking Paths >", "< Back to Employees" };
            case MenuPage.ReceptionistProfile:
                return new string[] { "Profile & Queue Skill", "Train Receptionist (+1 Capacity)",
                    "Increase Period Wage (+$25)", "Dismiss Receptionist", "< Back to Receptionists" };
            case MenuPage.EmployeeSettings:
                return new string[] { "Employee Placement Settings >", "Employee Walking Paths >",
                    "Blocked Path Recovery >", "Placement Markers >", "Break-Time Animations", "< Back to Employees" };
            case MenuPage.EmployeePlacements:
                return new string[] { "Staff Break Area >", "Service Bays >",
                    "Receptionist Desk >", "Receptionist Desk Approach >", "Manager Workstation >",
                    "Manager Workstation Approach >", "Staff Spawn / Entry >", "< Back to Employee Settings" };
            case MenuPage.EmployeePaths:
                return new string[] { "Mechanic Walking Paths >", "Receptionist Walking Paths >",
                    "Manager Walking Paths >", "< Back to Employee Settings" };
            case MenuPage.MechanicWalkingPaths:
                return new string[] { "Mechanic Walking Mode", "Pickup + Return Exit Route",
                    "Spawn -> Break Waypoints >", "Break -> Bay Waypoints >",
                    "Final Bay Approaches >", "Legacy Staff Checkpoint (MLO) >",
                    "Choose Job Waypoints by Mechanic >", "Smooth Through-Waypoints",
                    "Teleport Stuck Pedestrians", "Advanced Break <-> Bay Routes", "< Back to Walking Paths" };
            case MenuPage.ReceptionWalkingPaths:
                return new string[] { "Receptionist Walking Mode", "Spawn -> Desk Waypoints >",
                    "Final Desk Approach >", "< Back to Walking Paths" };
            case MenuPage.ReceptionWaypoints:
                return new string[] { "Reception Stop 1 / 5 >", "Reception Stop 2 / 5 >",
                    "Reception Stop 3 / 5 >", "Reception Stop 4 / 5 >",
                    "Reception Stop 5 / 5 >", "< Back to Reception Paths" };
            case MenuPage.ArrivalWaypoints:
                return new string[] { "Arrival Point 1 / 5 >", "Arrival Point 2 / 5 >",
                    "Arrival Point 3 / 5 >", "Arrival Point 4 / 5 >",
                    "Arrival Point 5 / 5 >", "< Back to Walking Paths" };
            case MenuPage.MechanicBays:
                return new string[] { "Service Bay 1 >", "Service Bay 2 >",
                    "Service Bay 3 >", "< Back to Placements" };
            case MenuPage.ServiceBayProfile:
                return new string[] { "Set Vehicle Position (CUSTOM)", "Set Mechanic Work Position", "Mechanic Assignment", "View Current Configuration", "< Back to Service Bays" };
            case MenuPage.ServiceVehicleBays:
                return new string[] { "Service Bay 1 >", "Service Bay 2 >",
                    "Service Bay 3 >", "< Back" };
            case MenuPage.VehicleDriveOptions:
                return new string[] { "Drive-in Customer Cars", "Mechanic Vehicle Transfers",
                    "Service Parking Mode", "Auto Lot Settings >", "Custom MLO Parking Bays >",
                    "Reverse Before Driving", "Driving Status / Diagnostics", "< Back to Customers" };
            case MenuPage.VehicleAutoLotOptions:
                return new string[] { "Car Parking Side", "Car Offset from Mechanic Bay",
                    "Auto Parking Preview", "< Back to Driving" };
            case MenuPage.ServiceWaypoints:
                return new string[] { "Work Route 1 / 5 >", "Work Route 2 / 5 >",
                    "Work Route 3 / 5 >", "Work Route 4 / 5 >",
                    "Work Route 5 / 5 >", "< Back to Walking Paths" };
            case MenuPage.BayApproaches:
                return new string[] { "Bay 1 Final Turn >", "Bay 2 Final Turn >",
                    "Bay 3 Final Turn >", "< Back to Walking Paths" };
            case MenuPage.PointEditor:
                return new string[] { "Set / Save Position + Facing", "Clear Saved Position",
                    "Marker Preview", "Show Saved Coordinates", "< Back" };
            case MenuPage.RouteRecovery:
                return new string[] { "Blocked-Route Teleport", "Recovery Status", "< Back" };
            case MenuPage.PlacementMarkers:
                return new string[] { "Placement Markers", "Show Marker Color Legend", "< Back" };
            case MenuPage.StaffRoster:
                return new string[] { "Mechanic 1 >", "Mechanic 2 >", "Mechanic 3 >", "< Back to Mechanics" };
            case MenuPage.StaffProfile:
                return new string[] { "Profile / Performance", "Training: +40 XP", "Change Specialization",
                    "Raise Period Wage (+$50)", "Fire This Mechanic", "< Back to Roster" };
            case MenuPage.BusinessExpenses:
                return new string[] { "Outstanding Bills", "Mechanic Wage Invoices >",
                    "Pay Receptionist Wages", "Pay Rent & Utilities", "Pay Supplier Invoices",
                    "Pay All Bills", "Order 10 Universal Parts", "Equipment & Office >",
                    "Financial Reports >", "Pay Manager Wages", "< Back to Business Management" };
            case MenuPage.InventoryManagement:
                return new string[] { "Inventory Overview >", "Build Supplier Order >", "Active Deliveries >", "Parts Sales >", "Stockroom & Pallets >", "Supplier Settings >", "< Back to Management" };
            case MenuPage.InventoryOverview:
                return new string[] { "Universal Parts", "Service Filters", "Brake Kits", "Diagnostic Supplies", "Storage Capacity", "Reorder Advice", "Low-stock Threshold", "< Back to Inventory" };
            case MenuPage.BuildSupplierOrder:
                return new string[] { "Universal Parts", "Service Filters", "Brake Kits", "Diagnostic Supplies", "Supplier: Change", "Cost / Capacity Preview", "Place Purchase Order", "Fill Low-stock Cart", "Clear Cart", "< Back to Inventory" };
            case MenuPage.ActiveDeliveries:
                return new string[] { "Delivery Status", "Order Contents", "Recover Original Order", "Cancel Pending Order", "< Back to Inventory" };
            case MenuPage.PartsSales:
                return new string[] { "Universal Part", "Service Filter", "Brake Kit",
                    "Diagnostic Supply", "< Back to Inventory" };
            case MenuPage.EquipmentManagement:
                return new string[] { "Equipment Status", "Buy Tool Set", "Buy Diagnostic Scanner",
                    "Buy Desk + Laptop", "Place Tool Chest", "Remove Tool Chest",
                    "Place Diagnostic Trolley", "Remove Diagnostic Trolley",
                    "Place Desk in Front", "Remove Placed Desk", "Office PC Status", "< Back to Expenses" };
            case MenuPage.ControllerSettings:
                return new string[] { "Controller Menu Controls (ENTER)", "View Controller Key Map", "Controller Input Diagnostics", "< Back to Business Management" };
            case MenuPage.MechanicRouteRoster:
                return new string[] { "Mechanic 1 Job Route >", "Mechanic 2 Job Route >",
                    "Mechanic 3 Job Route >", "< Back to Mechanic Paths" };
            case MenuPage.MechanicRouteDirection:
                return new string[] { "Break -> Job Waypoint Selection >", "Job -> Break Waypoint Selection >",
                    "Route Summary", "< Back to Mechanic List" };
            case MenuPage.MechanicRoutePoints:
                return new string[] { "Use Work Waypoint 1", "Use Work Waypoint 2",
                    "Use Work Waypoint 3", "Use Work Waypoint 4", "Use Work Waypoint 5",
                    "All Waypoints ON", "All Waypoints OFF", "< Back to Route Direction" };
            case MenuPage.MechanicInvoices:
                return new string[] { "Mechanic 1 Wage Invoice", "Mechanic 2 Wage Invoice",
                    "Mechanic 3 Wage Invoice", "Pay Former Employee Wages", "Pay All Mechanic Wages",
                    "< Back to Expenses" };
            case MenuPage.Finances:
                return new string[] { "Financial Overview >", "Income & Sales >", "Expenses & Bills >", "Profit & Loss >", "Transaction History >", "Owner Cash Transfers >", "Reporting Period: Toggle", "< Back to Management" };
            case MenuPage.FinancialOverview:
                return new string[] { "Reporting Period: Toggle", "Period Closing Cash", "Gross Revenue", "Cash Flow Reconciliation", "Outstanding Bills at Close", "Save / Audit State", "< Back to Accounting" };
            case MenuPage.IncomeSales:
                return new string[] { "Reporting Period: Toggle", "Physical Customer Repairs", "Manager Remote Repairs", "Autonomous Mechanic Jobs", "Parts Retail Sales", "Gross Revenue (No Overlap)", "Known Rebates / Refunds", "< Back to Accounting" };
            case MenuPage.ProfitLoss:
                return new string[] { "Reporting Period: Toggle", "Gross Revenue", "Cash Expense Payments", "Expenses Incurred", "Accounting Profit", "Accounting Basis / Notes", "< Back to Accounting" };
            case MenuPage.TransactionHistory: return FinanceHistoryItems();
            case MenuPage.OwnerTransfers:
                return new string[] { "Deposit Owner Funds", "Withdraw Owner Funds", "Recorded Owner Deposits", "Recorded Owner Withdrawals", "Reporting Period: Toggle", "< Back to Accounting" };
            case MenuPage.SupplierDeliverySetup:
                return new string[] { "Physical Mule Delivery", "Mule Road Spawn >", "Truck Unloading Spot >",
                    "Driver Box Handoff >", "Pallet Storage & Capacity >", "Driver Walk Stops (0-3) >",
                    "Wholesale Warehouse: Fixed", "Assign Current Pickup Vehicle",
                    "Assigned Vehicle / Cargo Status", "Delivery Route / Stock Status",
                    "Recover Stuck Shipment", "Cancel Pending Order (press twice)", "< Back to Inventory" };
            case MenuPage.SupplierPalletStorage:
                return new string[] { "Storage Capacity Status", "Purchase Additional Pallet ($750)",
                    "Place / Move Pallet 1 >", "Place / Move Pallet 2 >", "Place / Move Pallet 3 >",
                    "Place / Move Pallet 4 >", "< Back to Inventory" };
            case MenuPage.SupplierDriverWaypoints:
                return new string[] { "Driver Stop 1 / 3 >", "Driver Stop 2 / 3 >", "Driver Stop 3 / 3 >",
                    "< Back to Delivery Setup" };
            case MenuPage.HireMechanicRoster:
                return new string[] { "Junior General Mechanic", "Skilled Brake Technician",
                    "Experienced Diagnostic Technician", "< Back to Mechanics" };
            case MenuPage.HireReceptionistRoster:
                return new string[] { "Entry Receptionist (1 queue)", "Experienced Receptionist (2 queue)",
                    "Senior Service Advisor (3 queue)", "< Back to Receptionist" };
            case MenuPage.Customers:
                return new string[] { "Customer Order Status", "Customer Queue >",
                    "Customer Placement Settings >", "Vehicle Driving Options >",
                    "Customer Arrivals", "Schedule Next Customer", "Customer Blips",
                    "Placement Markers", "< Back to Business Management" };
            case MenuPage.CustomerQueueSettings:
                return new string[] { "Pending Customer Requests", "Maximum Queue Capacity",
                    "Schedule One Customer", "Clear Pending Requests", "< Back to Customers" };
            case MenuPage.CustomerWalkingRoutes:
                return new string[] { "Customer Walking Stop 1 / 3 >",
                    "Customer Walking Stop 2 / 3 >", "Customer Walking Stop 3 / 3 >",
                    "< Back to Customer Placements" };
            case MenuPage.CustomerPlacements:
                return new string[] { "Customer Drop-off Parking >", "Customer Road Spawn >",
                    "Customer Reception Approach >", "Customer Check-in Position >",
                    "Waiting Car 2 Position >", "Waiting Car 3 Position >",
                    "Service Vehicle Parking Bays >", "Customer Walking Route (0-3) >", "< Back to Customers" };
            default:
                return new string[] {
                    _clockedIn ? "Close Business" : "Open Business",
                    "Save Business", "Business Status", "Business Management >" };
        }
    }

    private string GetMenuItemValue(int index)
    {
        switch (_menuPage)
        {
            case MenuPage.ManagerManagement:
                if (index == 0) return _managerTier < 0 ? "VACANT" : _managerName;
                if (index == 1) return _managerTier < 0 ? "OPEN" : "EMPLOYED";
                if (index == 2) return _managerTier < 0 ? "DIRECT" : (_customManagerWalkingPaths ? "CUSTOM" : "DIRECT");
                return ">";
            case MenuPage.ManagerProfile:
                if (index == 0) return _managerTier < 0 ? "VACANT" : _managerName;
                if (index == 1) return _managerTier < 0 ? "N/A" : "$" + (750L * (100 - ManagerDiscountPercent()) / 100).ToString("N0");
                if (index == 2) return "$" + _managerWagesDue.ToString("N0");
                return ">";
            case MenuPage.ManagerWalkingPaths:
                if (index == 0) return _customManagerWalkingPaths ? "CUSTOM MLO" : "AUTO DIRECT";
                if (index == 1) return CountSet(_managerRoute) + "/5";
                if (index == 2) return Status(_managerApproach);
                return ">";
            case MenuPage.ManagerWaypoints:
                return index < OptionalPathPointCount ? Status(_managerRoute[index]) : ">";
            case MenuPage.HireManagerRoster:
                return index < 3 ? "$" + ManagerCandidateCost(index).ToString("N0") : ">";
            case MenuPage.Employees:
                if (index == 0) return _mechanics.Count + " M / " + (_receptionistHired ? "1 R" : "0 R");
                if (index == 1) return _mechanics.Count + "/" + _maxMechanics;
                if (index == 2) return _receptionistHired ? "HIRED" : "VACANT";
                return ">";
            case MenuPage.MechanicManagement:
                return index == 0 ? _mechanics.Count + " HIRED" : ">";
            case MenuPage.ReceptionistManagement:
                return index == 0 ? (_receptionistHired ? _receptionistName : "VACANT") : ">";
            case MenuPage.ReceptionistProfile:
                if (!_receptionistHired) return index == 0 ? "VACANT" : ">";
                if (index == 0) return "QUEUE " + EffectiveQueueCapacity();
                if (index == 1) return _receptionistTier == 2 ? "MAX" : "$500";
                if (index == 2) return "$" + (_receptionistWage + _receptionistWageBonus);
                return ">";
            case MenuPage.StaffRoster:
                return index < 3 ? (index < _mechanics.Count ? _mechanics[index].Name : "VACANT") : ">";
            case MenuPage.StaffProfile:
                if (_editingStaffSlot >= 0 && _editingStaffSlot < _mechanics.Count)
                {
                    Mechanic m = _mechanics[_editingStaffSlot];
                    if (index == 0) return "LV " + MechanicLevel(m);
                    if (index == 1) return "$" + TrainingCost;
                    if (index == 2) return SpecialtyName(m.Specialty);
                    if (index == 3) return "$" + MechanicWage(m);
                    if (index == 4) return m.HasJob ? "BUSY" : "DISMISS";
                }
                return ">";
            case MenuPage.VehicleDriveOptions:
                if (index == 0) return _driveInEnabled ? "ON" : "OFF";
                if (index == 1) return _vehicleTransfersEnabled ? "ON" : "OFF";
                if (index == 2) return _automaticLotMode ? "AUTO LOT" : "CUSTOM MLO";
                if (index == 5) return _valetReverseOut ? "ON" : "OFF";
                if (index == 6) return "VIEW";
                return ">";
            case MenuPage.VehicleAutoLotOptions:
                if (index == 0) return _autoLotRightSide ? "RIGHT" : "LEFT";
                if (index == 1) return _autoLotOffsetMeters.ToString("0") + " m";
                if (index == 2) return _autoLotPreview ? "ON" : "OFF";
                return ">";
            case MenuPage.InventoryManagement: return ">";
            case MenuPage.InventoryOverview:
                if (index == 0) return _parts.ToString("N0");
                if (index < 4) return _specialStock[index - 1].ToString("N0");
                return index == 4 ? StoredPartsTotal() + "/" + TotalStorageCapacity() : index == 6 ? _reorderThreshold.ToString() : ">";
            case MenuPage.BuildSupplierOrder:
                return index < 4 ? _supplierCart[index] + " units" : index == 4 ? SupplierName(_selectedSupplier) : index == 5 ? (SupplierLineTotal(_supplierCart) < 0 ? "OVER LIMIT" : "$" + SupplierCartCost(_supplierCart).ToString("N0")) : index == 6 ? (_deliveryKind == -2 ? "CONFIRM" : "PENDING") : ">";
            case MenuPage.ActiveDeliveries:
                return index == 0 ? (_deliveryKind == -2 ? "NONE" : ((int)Math.Ceiling(_deliveryRemainingGameSeconds)) + "s") : ">";
            case MenuPage.PartsSales:
                return index < 4 ? "$" + EstimatedUnitAcquisitionCost(index - 1).ToString("N0") : "BACK";
            case MenuPage.EquipmentManagement:
                if (index == 1) return _toolSetPurchased ? "OWNED" : "$1,800";
                if (index == 2) return _diagnosticScannerPurchased ? "OWNED" : "$1,600";
                if (index == 3) return _officeDeskPurchased ? "OWNED" : "$2,400";
                if (index == 4) return _toolSetPlaced ? "REPOSITION" : "PLACE";
                if (index == 6) return _scannerPlaced ? "REPOSITION" : "PLACE";
                if (index == 8) return _officeDeskPlaced ? "REPOSITION" : "PLACE";
                return ">";
            case MenuPage.ControllerSettings:
                return index == 0 ? (_controllerEnabled ? "ON" : "OFF") : ">";
            case MenuPage.SupplierDeliverySetup:
                if (index == 0) return _physicalMuleDeliveries ? "ON" : "OFF";
                if (index == 1) return Status(_supplierTruckSpawn);
                if (index == 2) return Status(_supplierTruckPark);
                if (index == 3) return Status(_supplierHandoff);
                if (index == 4) return TotalStorageCapacity() + " CAP";
                if (index == 5) return CountSet(_supplierDriverWaypoints) + "/3";
                if (index == 6) return "FIXED";
                if (index == 7) return HasWholesaleVehicle() ? "CHANGE" : "ASSIGN";
                if (index == 8) return _wholesaleCargoLoaded ? "LOADED" : HasWholesaleVehicle() ? "REGISTERED" : "NONE";
                if (index == 9) return _deliveryKind == -2 ? "READY" : MuleStageName();
                if (index == 10) return _deliveryKind == -2 ? "NONE" : "RESET";
                if (index == 11) return _deliveryKind == -2 ? "NONE" : "CONFIRM";
                return ">";
            case MenuPage.SupplierDriverWaypoints:
                return index < 3 ? Status(_supplierDriverWaypoints[index]) : ">";
            case MenuPage.SupplierPalletStorage:
                if (index == 0) return StoredPartsTotal() + "/" + TotalStorageCapacity();
                if (index == 1) return _ownedStoragePallets == MaxStoragePallets ? "MAX" : "$750";
                if (index >= 2 && index <= 5)
                    return index - 2 >= _ownedStoragePallets ? "LOCKED" : Status(StoragePalletPoint(index - 2));
                return ">";
            case MenuPage.HireMechanicRoster:
                return index < 3 ? "$" + MechanicCandidatePrice(index).ToString("N0") : ">";
            case MenuPage.HireReceptionistRoster:
                return index < 3 ? "$" + ReceptionCandidatePrice(index).ToString("N0") : ">";
            case MenuPage.MechanicRouteRoster:
                return index < 3 ? (index < _mechanics.Count ? _mechanics[index].Name : "VACANT") : ">";
            case MenuPage.MechanicRouteDirection:
                if (_routeConfigSlot < _mechanics.Count)
                {
                    Mechanic routeOwner = _mechanics[_routeConfigSlot];
                    if (index == 0) return CountMask(routeOwner.JobOutboundMask) + "/5";
                    if (index == 1) return CountMask(routeOwner.JobReturnMask) + "/5";
                }
                return ">";
            case MenuPage.MechanicRoutePoints:
                if (index < 5 && _routeConfigSlot < _mechanics.Count)
                {
                    Mechanic routeOwner = _mechanics[_routeConfigSlot];
                    int mask = _routeConfigReturning ? routeOwner.JobReturnMask : routeOwner.JobOutboundMask;
                    return (mask & (1 << index)) != 0 ? "USE" : "SKIP";
                }
                return ">";
            case MenuPage.BusinessManagement:
                return index == 8 ? "BACK" : index == 7 ? "SET" : index < 5 ? "OPEN" : "TRANSFER";
            case MenuPage.Business:
                if (index == 0) return _clockedIn ? "OPEN" : "CLOSED";
                if (index == 1) return "SAVE";
                if (index == 2) return "VIEW";
                return "OPEN";
            case MenuPage.BusinessExpenses:
                if (index == 0) return "$" + TotalOutstandingBills().ToString("N0");
                if (index == 1) return "$" + MechanicWagesDue().ToString("N0");
                if (index == 2) return "$" + _receptionWagesDue.ToString("N0");
                if (index == 3) return "$" + _rentUtilitiesDue.ToString("N0");
                if (index == 4) return "$" + _supplierInvoicesDue.ToString("N0");
                if (index == 5) return "$" + TotalOutstandingBills().ToString("N0");
                if (index == 9) return "$" + _managerWagesDue.ToString("N0");
                return ">";
            case MenuPage.MechanicInvoices:
                if (index < 3) return index < _mechanics.Count ? "$" + _mechanics[index].UnpaidWages.ToString("N0") : "VACANT";
                if (index == 3) return "$" + _formerStaffWagesDue.ToString("N0");
                if (index == 4) return "$" + MechanicWagesDue().ToString("N0");
                return ">";
            case MenuPage.EmployeeSettings:
                if (index == 0) return CountSet(_bays) + "/3 BAYS";
                if (index == 1) return _customEmployeeWalkingPaths ? "CUSTOM" : "DIRECT";
                if (index == 2) return _routeRecoveryEnabled ? "ON" : "OFF";
                if (index == 3) return _showPlacementMarkers ? "ON" : "OFF";
                if (index == 4) return _breakAnimationsEnabled ? "ON" : "OFF";
                return ">";
            case MenuPage.EmployeePlacements:
                switch (index)
                {
                    case 0: return Status(_breakArea);
                    case 1: return CountSet(_bays) + "/3";
                    case 2: return Status(_receptionDesk);
                    case 3: return Status(_receptionApproach);
                    case 4: return Status(_managerWorkstation);
                    case 5: return Status(_managerApproach);
                    case 6: return Status(_employeeEntrance);
                    default: return ">";
                }
            case MenuPage.EmployeePaths: return index == 0 ? (_customEmployeeWalkingPaths ? "CUSTOM" : "DIRECT") :
                index == 1 ? (_customReceptionWalkingPaths ? "CUSTOM" : "DIRECT") :
                index == 2 ? (_customManagerWalkingPaths ? "CUSTOM" : "DIRECT") : ">";
            case MenuPage.MechanicWalkingPaths:
                if (index == 9) return _customMechanicJobRoutes ? "SAVED WAYPOINTS" : "DIRECT";
                if (index == 7) return _smoothMechanicWaypoints ? "ON" : "OFF";
                if (index == 8) return _safeTeleportOnStall ? "ON" : "OFF";
                switch (index)
                {
                    case 0: return _customEmployeeWalkingPaths ? "CUSTOM MLO" : "AUTO DIRECT";
                    case 1: return _pickupUsesArrivalExit ? "REVERSE PATH" : "DIRECT";
                    case 2: return CountSet(_arrivalRoute) + "/5";
                    case 3: return CountSet(_serviceRoute) + "/5";
                    case 4: return CountSet(_bayApproaches) + "/3";
                    case 5: return Status(_walkCheckpoint);
                    default: return ">";
                }
            case MenuPage.ReceptionWalkingPaths:
                if (index == 0) return _customReceptionWalkingPaths ? "CUSTOM MLO" : "AUTO DIRECT";
                if (index == 1) return CountSet(_receptionRoute) + "/5";
                if (index == 2) return Status(_receptionApproach);
                return ">";
            case MenuPage.ReceptionWaypoints:
                return index < OptionalPathPointCount ? Status(_receptionRoute[index]) : ">";
            case MenuPage.ArrivalWaypoints:
                return index < OptionalPathPointCount ? Status(_arrivalRoute[index]) : ">";
            case MenuPage.ServiceBayProfile: return index == 2 ? "SLOT " + (_serviceBaySlot + 1) : ">";
            case MenuPage.MechanicBays:
                return index < 3 ? Status(_bays[index]) : ">";
            case MenuPage.ServiceVehicleBays:
                return index < 3 ? Status(_vehicleParkingBays[index]) : ">";
            case MenuPage.ServiceWaypoints:
                return index < OptionalPathPointCount ? Status(_serviceRoute[index]) : ">";
            case MenuPage.BayApproaches:
                return ">";
            case MenuPage.PointEditor:
                return PointEditorActionLabel(index,
                    _editingPoint != null && _editingPoint.IsSet, _showPlacementMarkers);
            case MenuPage.RouteRecovery:
                return index == 0 ? (_routeRecoveryEnabled ? "ON" : "OFF") : ">";
            case MenuPage.Finances:
                return index == 6 ? FinancePeriodLabel() : ">";
            case MenuPage.FinancialOverview: case MenuPage.IncomeSales: case MenuPage.ProfitLoss: case MenuPage.OwnerTransfers:
                return FinanceValue(_menuPage, index);
            case MenuPage.TransactionHistory:
                FinanceEntry[] rows = FinanceHistoryRows();
                return index < rows.Length ? "$" + rows[index].Amount.ToString("N0") : ">";
            case MenuPage.CustomerQueueSettings:
                if (index == 0) return _waitingCustomerRequests + "/" + EffectiveQueueCapacity() +
                    " (" + StagedCount() + " CARS)";
                if (index == 1) return _customerQueueCapacity + " MAX / " + EffectiveQueueCapacity() + " STAFF";
                if (index == 2) return _clockedIn && _customerServiceEnabled ? "ADD" : "CLOSED";
                if (index == 3) return _waitingCustomerRequests > 0 ? "CLEAR" : "EMPTY";
                return ">";
            case MenuPage.CustomerWalkingRoutes:
                return index < _customerWalkingRoute.Length ? Status(_customerWalkingRoute[index]) : ">";
            case MenuPage.CustomerPlacements:
                if (index == 0) return Status(_customerDropoff);
                if (index == 1) return Status(_customerDriveSpawn);
                if (index == 2) return Status(_customerApproach);
                if (index == 3) return Status(_customerQueue);
                if (index == 4) return _queueCar2.IsSet ? "SET" : "ROAD SPAWN";
                if (index == 5) return Status(_queueCar3);
                if (index == 6) return CountSet(_vehicleParkingBays) + "/3";
                if (index == 7) return CountSet(_customerWalkingRoute) + "/3";
                return ">";
            case MenuPage.Customers:
                if (index == 0)
                {
                    if (_customerOrder == null) return _waitingForDropoff ? "BLOCKED" : "IDLE";
                    if (_customerOrder.Stage == CustomerStage.Repairing &&
                        _customerOrder.MechanicSlot >= 0 &&
                        _customerOrder.MechanicSlot < _mechanics.Count)
                    {
                        Mechanic worker = _mechanics[_customerOrder.MechanicSlot];
                        return worker.ArrivalPhase == PhaseWorking ?
                            ((int)Math.Min(100.0, worker.WorkSeconds /
                                Math.Max(1.0, _customerOrder.RepairSeconds) * 100.0)) + "%" : "EN ROUTE";
                    }
                    return "ACTIVE";
                }
                if (index == 1) return _waitingCustomerRequests + "/" + _customerQueueCapacity +
                    (StagedCount() == 0 ? "" : " " + StagedCount() + " CARS");
                if (index == 4) return _customerServiceEnabled ? "ON" : "OFF";
                if (index == 6) return _customerBlipsEnabled ? "ON" : "OFF";
                if (index == 7) return _showPlacementMarkers ? "ON" : "OFF";
                return ">";
            case MenuPage.PlacementMarkers:
                return index == 0 ? (_showPlacementMarkers ? "ON" : "OFF") : ">";
            default:
                switch (index)
                {
                    case 0: return _clockedIn ? "OPEN" : "CLOSED";
                    case 1: return (_mechanics.Count + (_receptionistHired ? 1 : 0)).ToString();
                    case 2: return "$" + ((long)_partCost * 10).ToString("N0");
                    case 5: return _placed ? "SET" : "NOT SET";
                    case 9: return _customerServiceEnabled ? "ON" : "OFF";
                    default: return ">";
                }
        }
    }

    private static string Status(WorkBay point)
    {
        return point != null && point.IsSet ? "SET" : "NOT SET";
    }

    private static int CountSet(WorkBay[] points)
    {
        int total = 0;
        foreach (WorkBay point in points) if (point.IsSet) total++;
        return total;
    }

    private string EditingPointPurpose()
    {
        if (_editingPoint == _employeeEntrance)
            return "Spawn for clocking-in staff ONLY. Not a customer-car stop.";
        if (_editingPoint == _breakArea)
            return "Idle/office spot for mechanics between assigned jobs.";
        if (_editingPoint == _receptionDesk)
            return "Where receptionist stands behind the service counter.";
        if (_editingPoint == _receptionApproach)
            return "Staff-side approach, NOT where customers queue.";
        if (_editingPoint == _managerWorkstation)
            return "Manager clipboard workstation; configured in Employee Placements.";
        if (_editingPoint == _managerApproach)
            return "Final optional manager approach before workstation (CUSTOM MLO).";
        foreach (WorkBay stop in _managerRoute)
            if (Object.ReferenceEquals(stop, _editingPoint))
                return "Optional manager-only entry-to-workstation walking stop.";
        if (_editingPoint == _supplierTruckSpawn) return "Truck spawns here on the ROAD, facing outward toward shop.";
        if (_editingPoint == _supplierTruckPark) return "Truck parks here to unload; allow 8m+ clearance for a Mule.";
        if (_editingPoint == _supplierHandoff) return "Driver waits here with box; press E to accept before carrying it to storage.";
        if (_editingPoint == _supplierPartsPallet) return "First stockroom pallet; parts delivered here only if room is available.";
        if (_editingPoint == _wholesaleWarehousePickup) return "Warehouse pickup mission: collect the box here, then return to the shop.";
        foreach (WorkBay pallet in _extraStoragePallets)
            if (Object.ReferenceEquals(pallet, _editingPoint)) return "Additional purchased pallet: stand where it should be placed.";
        foreach (WorkBay deliveryNode in _supplierDriverWaypoints)
            if (Object.ReferenceEquals(deliveryNode, _editingPoint))
                return "Optional delivery driver pedestrian corridor; 0-3 points.";
        if (_editingPoint == _walkCheckpoint)
            return "Legacy arrival detour; CUSTOM MLO only, if 0 new stops.";
        foreach (WorkBay point in _receptionRoute)
            if (Object.ReferenceEquals(point, _editingPoint))
                return "OPTIONAL receptionist spawn -> desk stop; CUSTOM MLO only.";
        foreach (WorkBay point in _arrivalRoute)
            if (Object.ReferenceEquals(point, _editingPoint))
                return "OPTIONAL staff entry -> break path; CUSTOM MLO only.";
        foreach (WorkBay point in _serviceRoute)
            if (Object.ReferenceEquals(point, _editingPoint))
                return "OPTIONAL break -> mechanic bay path; CUSTOM MLO only.";
        foreach (WorkBay point in _bayApproaches)
            if (Object.ReferenceEquals(point, _editingPoint))
                return "OPTIONAL final turn before work bay; CUSTOM MLO only.";
        foreach (WorkBay point in _bays)
            if (Object.ReferenceEquals(point, _editingPoint))
                return "Mechanic repair animation location, NOT the car's spot.";
        return "Stand at the destination and face its direction; save.";
    }

    private string GetSelectedDescription()
    {
        switch (_menuPage)
        {
            case MenuPage.PointEditor:
                switch (_selection)
                {
                    case 0: return EditingVehicleParkingSlot() >= 0 ?
                        "Car's final parking position + facing; no active job." :
                        EditingPointPurpose();
                    case 1: return EditingVehicleParkingSlot() >= 0 ?
                        "Clear vehicle spot when no customer order is active." :
                        "Remove this placement (shop must be closed).";
                    case 2: return "Keep world markers visible, even after closing " + MenuKeyLabel() + ".";
                    case 3: return "Print coordinates and heading in a notification.";
                    default: return "Return to the previous settings page.";
                }
            case MenuPage.MechanicBays: return "One logical service bay; separate car and mechanic positions, existing roster slot preserved.";
            case MenuPage.ServiceBayProfile: return "CUSTOM uses the saved car position; AUTO LOT uses its existing workstation offset. Keep the work position beside the car. Assignment stays with its existing roster slot.";
            case MenuPage.ServiceVehicleBays: return "Custom MLO car bays; AUTO LOT does not require these.";
            case MenuPage.VehicleDriveOptions:
                switch (_selection)
                {
                    case 0: return "Customers drive from ROAD SPAWN to DROP-OFF.";
                    case 1: return "Mechanic collects car, drives to service bay, returns it.";
                    case 2: return "AUTO LOT derives spots; CUSTOM MLO uses saved car spots.";
                    case 3: return "AUTO LOT: choose side, distance and target preview.";
                    case 4: return "CUSTOM MLO: save independent car parking spots for bays.";
                    case 5: return "Optional careful reverse maneuver when exiting a service bay.";
                    case 6: return "Detailed live car-driving status and setup checks.";
                    default: return "Return to customer-service settings.";
                }
            case MenuPage.VehicleAutoLotOptions:
                return "Adjust only AUTO LOT. Does not modify manually saved bays.";
            case MenuPage.EmployeePaths: return "Configure walking routes for mechanics and receptionist independently.";
            case MenuPage.MechanicWalkingPaths:
                switch (_selection)
                {
                    case 0: return "DIRECT for outside; CUSTOM MLO for optional paths.";
                    case 1: return "REVERSE PATH follows saved Spawn -> Break points back to the car.";
                    case 2: return "0-5 staff spawn to break points; reversed during car pickup.";
                    case 3: return "0-5 mechanic break to repair bay route stops.";
                    case 4: return "Optional last turn beside mechanic repair workstations.";
                    case 5: return "Legacy checkpoint, only when arrival points are absent.";
                    case 6: return "Choose a separate set of existing job-route stops for each mechanic.";
                    case 7: return "Advance ~1.9m before intermediate waypoints; final target unchanged.";
                    case 8: return "Recover stuck workers beside saved points rather than cancelling jobs.";
                    case 9: return "Default DIRECT Break <-> Bay; opt in to existing saved job stops and approaches.";
                    default: return "Return to separate employee walking settings.";
                }
            case MenuPage.MechanicRouteRoster:
                return "Select a mechanic to configure their individual Break <-> Job route.";
            case MenuPage.MechanicRouteDirection:
                return "Set outbound and return waypoint use independently.";
            case MenuPage.MechanicRoutePoints:
                return "USE / SKIP saved points; unset points are always skipped.";
            case MenuPage.ControllerSettings:
                return "Press ENTER to change ON/OFF. Menu does not support mouse clicks.";
            case MenuPage.ReceptionWalkingPaths:
                return _selection == 0 ? "DIRECT goes straight to desk; CUSTOM follows saved route." :
                    "Receptionist-only 0/5 route and desk approach.";
            case MenuPage.ReceptionWaypoints: return "OPTIONAL receptionist-only stops, staff entry to desk.";
            case MenuPage.ManagerWalkingPaths: return "DIRECT follows navmesh; CUSTOM visits saved manager stops and final approach.";
            case MenuPage.ManagerWaypoints: return "Optional 0-5 manager-only route stops from staff entrance to workstation.";
            case MenuPage.EmployeePlacements: return "Set spawn, mechanic bays, receptionist and manager workstations and approaches.";
            case MenuPage.CustomerPlacements: return "Road spawn, two waiting-car spaces, drop-off and optional customer path.";
            case MenuPage.CustomerWalkingRoutes: return "Optional 0-3 walking stops in order from drop-off to reception; reverse on pickup.";
            case MenuPage.CustomerQueueSettings: return "Cars 2 and 3 stage roadside. Multiple staffed bays can repair while arrivals take turns at drop-off.";
            case MenuPage.ArrivalWaypoints: return "Optional in CUSTOM MLO: staff spawn to break room.";
            case MenuPage.ServiceWaypoints: return "Optional in CUSTOM MLO: break room to work bay.";
            case MenuPage.BayApproaches: return "Optional last turn before work bay; CUSTOM only.";
            case MenuPage.RouteRecovery: return "Stuck employees can recover at saved waypoints.";
            case MenuPage.BusinessManagement:
                return new string[] {
                    "Recruit employees, review mechanic profiles and receptionist queue skill.",
                    "Customer requests, queue spaces, arrivals and vehicle transfers.",
                    "Order parts, select suppliers and configure Mule deliveries.",
                    "Review sales, wage liabilities, supplier costs and shift profit.",
                    "Change controller support and inspect the active button map.",
                    "Move personal funds into business cash (not recorded as revenue).",
                    "Withdraw available business cash to the player account.",
                    "Set the shop's permanent location while it is closed.",
                    "Return to the four-item main menu." }[Math.Min(_selection, 8)];
            case MenuPage.BusinessExpenses: return _selection == 9 ?
                "Pay only the outstanding manager wage invoice; no manager rebate on their own wages." :
                "Settle wages, rent and delivered supplier invoices; restock and equipment are separate.";
            case MenuPage.SupplierDeliverySetup:
                return new string[] {
                    "Use a Mule for local/express deliveries. Wholesale uses warehouse pickup.",
                    "Road location where the supplier vehicle initially appears.",
                    "Saved unloading parking space; leave room for a box truck.",
                    "Driver opens rear doors, retrieves box and walks here for handoff.",
                    "Buy extra storage pallets, place them and inspect stock capacity.",
                    "Up to three optional stops for the delivery driver's walking route.",
                    "Permanent wholesale parcel spawn: -164.2800, -2676.5900, 6.0141.",
                    "Assign the CURRENT vehicle (plate and model) for wholesale cargo missions.",
                    "View registered vehicle, loaded cargo and active wholesale order.",
                    "Truck route, warehouse pickup, box carry, supplier invoices and capacity.",
                    "Recover original pending shipment; no new order or stock credit.",
                    "Press twice within 12 seconds to cancel; legacy prepayments are refunded.",
                    "Return to Inventory & Suppliers." }[Math.Min(_selection, 12)];
            case MenuPage.SupplierPalletStorage:
                return "Each placed pallet holds 25 parts and 9 delivered boxes. Buy and place more to expand.";
            case MenuPage.SupplierDriverWaypoints: return "Optional driver walk stops from the truck to parts handoff.";
            case MenuPage.ManagerManagement:
                return new string[] { "Review manager finances, skills and wages.",
                    "Hire or replace an existing manager after dismissal.",
                    "Open manager-only walking settings; placement is under Employee Settings.",
                    "Return to Employees." }[Math.Min(_selection, 3)];
            case MenuPage.ManagerProfile:
                return new string[] { "Manager level, savings, offsite jobs and revenue.",
                    "Train manager to improve experience and savings.",
                    "Pay currently owed manager wages.",
                    "Dismiss manager; unpaid wages move to former staff.",
                    "Return to Manager." }[Math.Min(_selection, 4)];
            case MenuPage.HireManagerRoster: return "Recruit a manager: higher tiers reduce expenses and improve offsite shop efficiency.";
            case MenuPage.HireMechanicRoster: return "Candidates have different starting skill, specialty, hiring price and wages.";
            case MenuPage.HireReceptionistRoster: return "Recruit a receptionist; skills permit one, two or three pending customers.";
            case MenuPage.PartsSales: return _selection < 4 ? PartsSalePreview(_selection - 1) + ". ENTER sells ONE unreserved unit. Pending cargo excluded." : "Return to Inventory & Suppliers.";
            case MenuPage.InventoryManagement:
                return new string[] { "Inspect stock on hand and shared storage capacity.", "Choose supplier and build an order; invoiced only after stocking.", "Track ETA, inspect contents, recover or cancel the original shipment.", "Sell one unreserved unit; see availability and fixed sale estimate below.", "Buy or position pallets and inspect the stockroom.", "Configure supplier fleet, road route and pickup vehicle.", "Return to Business Management." }[Math.Min(_selection, 6)];
            case MenuPage.InventoryOverview: return "Stock on hand excludes pending cargo. Repair-reserved stock cannot be sold. Capacity is shared by all four items.";
            case MenuPage.BuildSupplierOrder: return _selection < 4 ? "LEFT subtracts 1; RIGHT or ENTER adds 1. Maximum 100 total units. Draft saves only on Place Purchase Order." : "Preview total/capacity before Place Purchase Order. Fill Low-stock prepares a draft only. Wholesale remains player pickup.";
            case MenuPage.ActiveDeliveries: return "Inspect the original pending order. Recover retains its contents; cancel requires a second confirmation and adds no stock.";
            case MenuPage.EquipmentManagement:
                if (_selection == 1) return "Tool set $1,800: permanent 7% shorter repair duration on all jobs.";
                if (_selection == 2) return "Scanner $1,600: permanent 10% shorter engine diagnostics.";
                if (_selection == 4 || _selection == 5) return "Place/hide tool chest. Ownership and repair bonus are never lost.";
                if (_selection == 6 || _selection == 7) return "Place/hide diagnostic trolley. Tablet removed; bonus remains active.";
                if (_selection == 8) return "Office desk: E or D-pad RIGHT nearby opens Business Management.";
                return "Purchased equipment saves permanently; placements only control visible props.";
            case MenuPage.MechanicInvoices: return "Pay outstanding per-mechanic wage invoices or settle all at once.";
            case MenuPage.PlacementMarkers: return "Placement circles align with floor; car spots use ground-level rectangular outlines.";
            case MenuPage.EmployeeSettings: return "Staff placement and walking options are separated.";
            case MenuPage.Employees:
                return new string[] {
                    "Review all employee assignments, wages and on-duty status.",
                    "Manage mechanics, hire candidates and edit each individual profile.",
                    "Hire, train or dismiss reception; skill sets customer queue capacity.",
                    "Shared staff workstations, entrances, paths and placement markers.",
                    "Return to Business Management." }[Math.Min(_selection, 4)];
            case MenuPage.MechanicManagement:
                return new string[] {
                    "View hired mechanics; fire, train or raise wages from their profiles.",
                    "Select a new general, brake-specialist or diagnostics candidate.",
                    "Choose which job-route waypoints each individual mechanic uses.",
                    "Configure custom MLO walk routing and stuck recovery for mechanics.",
                    "Return to the employee directory." }[Math.Min(_selection, 4)];
            case MenuPage.ReceptionistManagement:
                return new string[] {
                    "See reception skill, queue limit, salary and unpaid wages.",
                    "Browse three receptionist tiers; dismiss current staff before hiring.",
                    "Configure reception-only walking waypoints into the shop.",
                    "Return to the employee directory." }[Math.Min(_selection, 3)];
            case MenuPage.ReceptionistProfile:
                return new string[] {
                    "Show receptionist performance and effective customer queue capacity.",
                    "Spend $500 on training to increase queue skill by one, up to 3.",
                    "Give your receptionist a $25 wage increase per payroll period.",
                    "Dismiss reception; unpaid wages stay owed to former staff.",
                    "Return to reception management." }[Math.Min(_selection, 4)];
            case MenuPage.StaffRoster:
                return "Select a hired mechanic to view their persistent personnel record.";
            case MenuPage.StaffProfile:
                return new string[] {
                    "View this mechanic's XP, skill, specialty, wages and active job.",
                    "Spend $750 for +40 XP without counting a repair completion.",
                    "Pay for a new specialty certification at mechanic level 2+.",
                    "Increase this mechanic's wage by $50 for each payroll period.",
                    "Dismiss this mechanic when no customer repairs are active.",
                    "Return to the mechanic roster." }[Math.Min(_selection, 5)];
            case MenuPage.Customers:
                if (_selection == 0) return "Open a detailed order-status notification.";
                if (_selection == 1) return "Queue 1-3 requests; no car enters occupied drop-off.";
                if (_selection == 5) return "Schedule another customer, up to current queue limit.";
                return "Customer queue, placements, driving and service settings.";
            case MenuPage.Finances: return "Separate gross sales, cash movements, bills and profit. Toggle lifetime/current shift; lifetime cash audit begins at upgrade.";
            case MenuPage.FinancialOverview: return FinancePeriodLabel() + ": closing cash is not gross sales. Reconciliation separates expenses, owner transfers and adjustments.";
            case MenuPage.IncomeSales: return FinancePeriodLabel() + ": four non-overlapping revenue sources. Remote was already included in legacy customer totals.";
            case MenuPage.ProfitLoss: return FinancePeriodLabel() + ": purchase-expense accounting profit; unpaid accruals are not cash payments. Owner capital is excluded.";
            case MenuPage.TransactionHistory: return "Latest 256 committed entries; older cash rolls into history opening checkpoint. Select for active time, identity and resulting cash.";
            case MenuPage.OwnerTransfers: return "Capital transfers are not revenue. Legacy transfer totals are unknown; recorded totals start with this audit version.";
            default:
                switch (_selection)
                {
                    case 0: return "Start or end your shop's work shift.";
                    case 1: return "Save shop funds, invoices, employees and positions.";
                    case 2: return "Show detailed shop, customer and payroll status.";
                    default: return "Open the complete business management directory.";
                }
        }
    }

    private string GetMenuTitle()
    {
        switch (_menuPage)
        {
            case MenuPage.Business: return "GLASS HEROES";
            case MenuPage.BusinessManagement: return "BUSINESS MANAGEMENT";
            case MenuPage.Employees: return "EMPLOYEES";
            case MenuPage.ManagerManagement: return "BUSINESS MANAGER";
            case MenuPage.ManagerProfile: return "MANAGER PROFILE";
            case MenuPage.ManagerWalkingPaths: return "MANAGER WALKING";
            case MenuPage.ManagerWaypoints: return "MANAGER ROUTE (0-5)";
            case MenuPage.HireManagerRoster: return "MANAGEMENT CANDIDATES";
            case MenuPage.MechanicManagement: return "MECHANIC MANAGEMENT";
            case MenuPage.ReceptionistManagement: return "RECEPTION MANAGEMENT";
            case MenuPage.ReceptionistProfile: return "RECEPTIONIST PROFILE";
            case MenuPage.StaffRoster: return "MECHANIC ROSTER";
            case MenuPage.StaffProfile: return "MECHANIC PROFILE";
            case MenuPage.EmployeeSettings: return "EMPLOYEE SETTINGS";
            case MenuPage.EmployeePaths: return "EMPLOYEE WALKING PATHS";
            case MenuPage.EmployeePlacements: return "EMPLOYEE PLACEMENTS";
            case MenuPage.CustomerPlacements: return "CUSTOMER PLACEMENTS";
            case MenuPage.CustomerWalkingRoutes: return "CUSTOMER WALK (0-3)";
            case MenuPage.CustomerQueueSettings: return "CUSTOMER QUEUE";
            case MenuPage.MechanicWalkingPaths: return "MECHANIC WALKING";
            case MenuPage.ReceptionWalkingPaths: return "RECEPTION WALKING";
            case MenuPage.ReceptionWaypoints: return "RECEPTION ROUTE (0-5)";
            case MenuPage.ArrivalWaypoints: return "SPAWN -> BREAK (0-5)";
            case MenuPage.MechanicBays: return "SERVICE BAYS";
            case MenuPage.ServiceBayProfile: return "SERVICE BAY " + (_serviceBaySlot + 1);
            case MenuPage.ServiceWaypoints: return "BREAK -> BAY (0-5)";
            case MenuPage.BayApproaches: return "BAY APPROACHES";
            case MenuPage.PointEditor: return _editingPointLabel.ToUpperInvariant();
            case MenuPage.RouteRecovery: return "ROUTE RECOVERY";
            case MenuPage.PlacementMarkers: return "PLACEMENT MARKERS";
            case MenuPage.Customers: return "CUSTOMER SERVICE";
            case MenuPage.ServiceVehicleBays: return "SERVICE VEHICLE PARKING";
            case MenuPage.VehicleDriveOptions: return "VEHICLE DRIVING";
            case MenuPage.VehicleAutoLotOptions: return "AUTO LOT PARKING";
            case MenuPage.Finances: return "FINANCES & ACCOUNTING";
            case MenuPage.FinancialOverview: return "FINANCIAL OVERVIEW";
            case MenuPage.IncomeSales: return "INCOME & SALES";
            case MenuPage.ProfitLoss: return "PROFIT & LOSS";
            case MenuPage.TransactionHistory: return "TRANSACTION HISTORY";
            case MenuPage.OwnerTransfers: return "OWNER CASH TRANSFERS";
            case MenuPage.BusinessExpenses: return "EXPENSES & BILLS";
            case MenuPage.PartsSales: return "PARTS SALES - ONE UNIT";
            case MenuPage.InventoryManagement: return "INVENTORY & SUPPLIERS";
            case MenuPage.InventoryOverview: return "INVENTORY OVERVIEW";
            case MenuPage.BuildSupplierOrder: return "BUILD SUPPLIER ORDER";
            case MenuPage.ActiveDeliveries: return "ACTIVE DELIVERIES";
            case MenuPage.SupplierDeliverySetup: return "SUPPLIER SETTINGS";
            case MenuPage.SupplierPalletStorage: return "STOCKROOM & PALLETS";
            case MenuPage.SupplierDriverWaypoints: return "DRIVER WALK (0-3)";
            case MenuPage.HireMechanicRoster: return "HIRE A MECHANIC";
            case MenuPage.HireReceptionistRoster: return "HIRE RECEPTIONIST";
            case MenuPage.EquipmentManagement: return "SHOP EQUIPMENT";
            case MenuPage.ControllerSettings: return "CONTROLLER INPUT";
            case MenuPage.MechanicRouteRoster: return "MECHANIC JOB ROUTES";
            case MenuPage.MechanicRouteDirection: return "SELECT WALK DIRECTION";
            case MenuPage.MechanicRoutePoints: return _routeConfigReturning ? "RETURN WALK WAYPOINTS" : "JOB WALK WAYPOINTS";
            case MenuPage.MechanicInvoices: return "MECHANIC WAGES";
            default: return "GLASS HEROES";
        }
    }

    private static string[] WrapMenuDescription(string input, int maxChars)
    {
        if (String.IsNullOrEmpty(input)) return new string[] { "", "" };
        if (input.Length <= maxChars) return new string[] { input, "" };
        int split = input.LastIndexOf(' ', Math.Min(maxChars, input.Length - 1));
        if (split < maxChars / 2) split = maxChars;
        string first = input.Substring(0, split).Trim();
        string remainder = input.Substring(split).Trim();
        if (remainder.Length > maxChars)
        {
            int lastSpace = remainder.LastIndexOf(' ', Math.Min(maxChars - 3, remainder.Length - 1));
            remainder = remainder.Substring(0, lastSpace > 20 ? lastSpace : maxChars - 3).Trim() + "...";
        }
        return new string[] { first, remainder };
    }

    private void DrawMenu()
    {
        // GTA Online-inspired layout; intentionally uses original branding,
        // not a copied GTA Online texture or internal menu asset.
        const int left = 35;
        const int top = 74;
        const int width = 420;
        const int headerHeight = 70;
        const int summaryHeight = 61;
        const int rowHeight = 31;
        const int rowTop = top + headerHeight + summaryHeight;
        const int footerTop = rowTop + (VisibleMenuRows * rowHeight);

        Color white = Color.FromArgb(245, 245, 245);
        Color subtle = Color.FromArgb(180, 201, 209);
        Color yellow = Color.FromArgb(255, 197, 43);
        Color dark = Color.FromArgb(222, 13, 20, 27);

        string[] items = GetMenuItems();
        int menuCount = items.Length;
        // Header + shop summary.
        Panel(left, top, width, headerHeight, Color.FromArgb(240, 16, 55, 80));
        Panel(left, top, 6, headerHeight, yellow);
        Label(GetMenuTitle(), left + 18, top + 7, 0.46f, white);
        Label(_menuPage == MenuPage.Business ? "AUTO SERVICE   /   BUSINESS" :
            _menuPage == MenuPage.BusinessManagement ? "GLASS HEROES  /  MANAGEMENT" :
            "GLASS HEROES  /  BUSINESS SETTINGS", left + 18, top + 43, 0.27f,
            Color.FromArgb(225, 220, 229, 235));
        Panel(left, top + headerHeight, width, summaryHeight, Color.FromArgb(230, 23, 30, 38));
        Label("SHOP  " + (_clockedIn ? "OPEN" : "CLOSED"), left + 14, top + 75, 0.33f,
            _clockedIn ? Color.FromArgb(114, 225, 145) : Color.FromArgb(240, 169, 153));
        Label("CASH  $" + _cash.ToString("N0"), left + 192, top + 75, 0.33f, white);
        Label("MECHANICS  " + _mechanics.Count + "/" + _maxMechanics +
            "    STOCK  " + (_parts + _specialStock[0] + _specialStock[1] + _specialStock[2]) +
            "    LOCATION  " + (_placed ? "SET" : "NOT SET"),
            left + 14, top + 105, 0.29f, subtle);

        // Keep the panel in the 720p safe area as the list grows beyond ten items.
        int firstVisible = Math.Max(0, _selection - VisibleMenuRows + 1);
        firstVisible = Math.Min(firstVisible, Math.Max(0, menuCount - VisibleMenuRows));
        for (int i = firstVisible; i < Math.Min(firstVisible + VisibleMenuRows, menuCount); i++)
        {
            bool selected = i == _selection;
            int rowY = rowTop + (i - firstVisible) * rowHeight;
            Panel(left, rowY, width, rowHeight - 1,
                selected ? Color.FromArgb(244, 244, 245, 246) : dark);
            if (selected)
                Panel(left, rowY, 5, rowHeight - 1, yellow);

            Color textColor = selected ? Color.FromArgb(20, 29, 34) : white;
            LabelColumn(items[i], left + 15, rowY + 3, 255, 0.34f, textColor, false);
            LabelColumn(GetMenuItemValue(i), left + width - 14, rowY + 3, 125, 0.31f, textColor, true);
        }

        // Help footer replaces the former flickering GTA help-message overlay.
        Panel(left, footerTop, width, 119, Color.FromArgb(231, 16, 23, 30));
        Panel(left, footerTop, width, 2, yellow);
        // Two-line descriptions allow the highlighted action to be explained fully.
        string hint = GetSelectedDescription();
        string[] wrapped = WrapMenuDescription(hint, 53);
        Label(wrapped[0], left + 13, footerTop + 10, 0.30f, subtle);
        if (wrapped[1].Length > 0) Label(wrapped[1], left + 13, footerTop + 40, 0.30f, subtle);
        Label("ENTER SELECT  BACKSPACE BACK  ESC PAUSE", left + 13,
            footerTop + 86, 0.27f, white);
        Label((_selection + 1) + "/" + menuCount, left + 367, footerTop + 86, 0.27f, white);
    }

    private void ExecuteSelection()
    {
        switch (_menuPage)
        {
            case MenuPage.Business: ExecuteBusinessSelection(); break;
            case MenuPage.BusinessManagement: ExecuteManagementSelection(); break;
            case MenuPage.Employees: ExecuteEmployeeSelection(); break;
            case MenuPage.ManagerManagement: ExecuteManagerManagement(); break;
            case MenuPage.ManagerProfile: ExecuteManagerProfile(); break;
            case MenuPage.ManagerWalkingPaths: ExecuteManagerWalkingSelection(); break;
            case MenuPage.HireManagerRoster: ExecuteHireManagerRoster(); break;
            case MenuPage.MechanicManagement: ExecuteMechanicManagement(); break;
            case MenuPage.ReceptionistManagement: ExecuteReceptionistManagement(); break;
            case MenuPage.ReceptionistProfile: ExecuteReceptionistProfile(); break;
            case MenuPage.HireMechanicRoster: ExecuteHireMechanicRoster(); break;
            case MenuPage.HireReceptionistRoster: ExecuteHireReceptionistRoster(); break;
            case MenuPage.SupplierDeliverySetup: ExecuteSupplierDeliverySetup(); break;
            case MenuPage.SupplierPalletStorage: ExecuteSupplierPalletStorage(); break;
            case MenuPage.SupplierDriverWaypoints: ExecuteSupplierDriverWaypoints(); break;
            case MenuPage.StaffRoster: ExecuteStaffRosterSelection(); break;
            case MenuPage.StaffProfile: ExecuteStaffProfileSelection(); break;
            case MenuPage.EmployeeSettings: ExecuteEmployeeSettingsSelection(); break;
            case MenuPage.EmployeePlacements: ExecuteEmployeePlacementsSelection(); break;
            case MenuPage.CustomerPlacements: ExecuteCustomerPlacementsSelection(); break;
            case MenuPage.CustomerWalkingRoutes: ExecuteCustomerWalkingRouteSelection(); break;
            case MenuPage.CustomerQueueSettings: ExecuteCustomerQueueSettingsSelection(); break;
            case MenuPage.MechanicWalkingPaths: ExecuteMechanicWalkingSelection(); break;
            case MenuPage.ReceptionWalkingPaths: ExecuteReceptionWalkingSelection(); break;
            case MenuPage.ServiceBayProfile: ExecuteServiceBayProfile(); break;
            case MenuPage.MechanicBays:
            case MenuPage.ServiceVehicleBays: OpenServiceBayProfile(); break;
            case MenuPage.ArrivalWaypoints:
            case MenuPage.ReceptionWaypoints:
            case MenuPage.ManagerWaypoints:
            case MenuPage.ServiceWaypoints:
            case MenuPage.BayApproaches: ExecutePointListSelection(); break;
            case MenuPage.EmployeePaths: ExecuteEmployeePathsSelection(); break;
            case MenuPage.PointEditor: ExecutePointEditorSelection(); break;
            case MenuPage.RouteRecovery: ExecuteRecoverySelection(); break;
            case MenuPage.PlacementMarkers: ExecuteMarkerSelection(); break;
            case MenuPage.Customers: ExecuteCustomerSelection(); break;
            case MenuPage.VehicleDriveOptions: ExecuteVehicleDriveOptionSelection(); break;
            case MenuPage.VehicleAutoLotOptions: ExecuteVehicleAutoLotSelection(); break;
            case MenuPage.Finances: ExecuteFinancesSelection(); break;
            case MenuPage.FinancialOverview: case MenuPage.IncomeSales: case MenuPage.ProfitLoss:
            case MenuPage.TransactionHistory: case MenuPage.OwnerTransfers: ExecuteFinancePage(); break;
            case MenuPage.BusinessExpenses: ExecuteBusinessExpensesSelection(); break;
            case MenuPage.MechanicInvoices: ExecuteMechanicInvoicesSelection(); break;
            case MenuPage.InventoryManagement: ExecuteInventorySelection(); break;
            case MenuPage.InventoryOverview:
                if (_selection == 5) ShowReorderAdvice();
                else if (_selection == 6) { _reorderThreshold = _reorderThreshold == 3 ? 5 : _reorderThreshold == 5 ? 10 : 3; Save(); }
                else if (_selection == 7) NavigateBack(); else ShowDetailedInventory(); break;
            case MenuPage.BuildSupplierOrder: ExecuteSupplierCart(); break;
            case MenuPage.ActiveDeliveries:
                if (_selection == 0) ShowSupplierDeliveryStatus();
                else if (_selection == 1) ShowSupplierOrderContents();
                else if (_selection == 2) RecoverSupplierShipment();
                else if (_selection == 3) CancelSupplierOrderWithConfirmation();
                else NavigateBack(); break;
            case MenuPage.PartsSales: if (_selection < 4) TrySellPart(_selection - 1); else NavigateBack(); break;
            case MenuPage.EquipmentManagement: ExecuteEquipmentSelection(); break;
            case MenuPage.ControllerSettings: ExecuteControllerSelection(); break;
            case MenuPage.MechanicRouteRoster: ExecuteRouteRosterSelection(); break;
            case MenuPage.MechanicRouteDirection: ExecuteRouteDirectionSelection(); break;
            case MenuPage.MechanicRoutePoints: ExecuteRoutePointSelection(); break;
        }
    }

    private void ExecuteBusinessSelection()
    {
        switch (_selection)
        {
            case 0: ToggleBusinessShift(); break;
            case 1:
                Save();
                Notify(File.Exists(SaveFile) ? "Business saved to " + SaveFile :
                    "WARNING: Save.ini not found! Check Errors.log.");
                break;
            case 2: ShowShopStatus(); break;
            case 3: _menuPage = MenuPage.BusinessManagement; _selection = 0; break;
        }
    }

    // Former main-menu actions now live exclusively inside Business Management.
    private void ExecuteManagementSelection()
    {
        switch (_selection)
        {
            case 0: _menuPage = MenuPage.Employees; _selection = 0; break;
            case 1: _menuPage = MenuPage.Customers; _selection = 0; break;
            case 2: _menuPage = MenuPage.InventoryManagement; _selection = 0; break;
            case 3: _financesFromExpenses = false; _menuPage = MenuPage.Finances; _selection = 0; break;
            case 4: _menuPage = MenuPage.ControllerSettings; _selection = 0; break;
            case 5: TransferPlayerMoney(true); break;
            case 6: TransferPlayerMoney(false); break;
            case 7: SetShopLocationFromPlayer(); break;
            default: NavigateBack(); break;
        }
    }

    private void ToggleBusinessShift()
    {
        if (!_placed) { Notify("Cannot open: set shop location under Business Management first."); return; }
        bool opening = !_clockedIn;
        if (opening)
        {
            _shiftReportReady = true;
            _shiftSalesBaseline = _revenue;
            _shiftCostsBaseline = AccruedBusinessCosts();
        }
        _clockedIn = opening;
        if (!opening)
        {
            ParkRuntimeRepairPromises(); // Closing holds accepted work without advancing it.
            _remoteMode = false;
            _remoteJobs.Clear(); // Never pay interrupted jobs when business closes.
            _remoteIdleGameSeconds = 0;
        }
        _payrollClock = 0;
        _overheadClock = 0;
        foreach (Mechanic staff in _mechanics)
        {
            staff.HasJob = false;
            staff.WorkSeconds = 0;
            staff.IdleSeconds = 0;
        }
        if (!_clockedIn)
        {
            CancelAllCustomerOrders("Shop closed");
            CleanupVisualMechanics();
        }
        else _nextCustomerAt = DateTime.UtcNow.AddSeconds(12);
        _warnedMissingEntrance = false;
        _warnedMissingBreakArea = false;
        if (_clockedIn)
            for (int i = 0; i < _mechanics.Count; i++)
                _mechanics[i].NextSpawnAttempt = DateTime.UtcNow.AddSeconds(i * _arrivalSpacingSeconds);
        _visualRefreshClock = 1.0;
        Notify(_clockedIn ? "Shop OPEN. Staff will report to their break area." :
            "Shop CLOSED. Employees dismissed; expenses stopped.");
        if (!Save() && opening)
        { _clockedIn = false; Notify("Opening save failed; shop remains closed and no income clock starts."); }
    }

    private void SetShopLocationFromPlayer()
    {
        if (_clockedIn) { Notify("Close the shop before moving its location."); return; }
        if (Game.Player.Character == null) return;
        Vector3 newLocation = Game.Player.Character.Position;
        if (_placed && DistanceSquared(_location, newLocation) > 400f)
        {
            foreach (WorkBay bay in _bays) bay.IsSet = false;
            _employeeEntrance.IsSet = false;
            _walkCheckpoint.IsSet = false;
            foreach (WorkBay point in _arrivalRoute) point.IsSet = false;
            foreach (WorkBay point in _receptionRoute) point.IsSet = false;
            _breakArea.IsSet = false;
            _receptionDesk.IsSet = false;
            _receptionApproach.IsSet = false;
            _customerDriveSpawn.IsSet = false;
            _customerDropoff.IsSet = false;
            _customerApproach.IsSet = false;
            _customerQueue.IsSet = false;
            _queueCar2.IsSet = false;
            _queueCar3.IsSet = false;
            foreach (WorkBay point in _serviceRoute) point.IsSet = false;
            foreach (WorkBay point in _bayApproaches) point.IsSet = false;
            Notify("New shop: bays, entrance, break and walking routes cleared.");
        }
        _location = newLocation;
        _placed = true;
        Save();
        Notify("Shop location SET: " + FormatLocation() + ". " +
            (File.Exists(SaveFile) ? "Save.ini exists." : "WARNING: Save.ini missing!"));
    }

    private long MechanicWagesDue()
    {
        long result = _formerStaffWagesDue;
        foreach (Mechanic m in _mechanics) result += m.UnpaidWages;
        return result;
    }

    private long TotalOutstandingBills()
    {
        return MechanicWagesDue() + _receptionWagesDue + _managerWagesDue + _rentUtilitiesDue + _supplierInvoicesDue;
    }

    // Bills do not overdraft the business. An invoice is either paid in full
    // or remains due; there is no accidental double-settlement on retry.
    private bool PayInvoice(ref long due, bool wages, string label)
    {
        if (!_bootGameplayActivated || Game.IsPaused) return false;
        if (due <= 0) { Notify(label + ": nothing owed."); return false; }
        if (_cash < due)
        {
            Notify(label + ": $" + due.ToString("N0") + " due; business cash $" +
                _cash.ToString("N0") + ". Deposit funds or complete more orders.");
            return false;
        }
        long payment = due;
        if ((wages ? _payrollSpend : _overheadSpend) > long.MaxValue - payment)
        { Notify(label + ": payment would overflow the expense ledger."); return false; }
        long cash = _cash, payroll = _payrollSpend, overhead = _overheadSpend, savings = _managerSavings, rebate = _financeRebateIncome;
        double managerAge = _managerWageAgeSeconds, receptionAge = _receptionWageUnpaidSeconds, rentAge = _rentUnpaidSeconds;
        bool receptionNotified = _receptionOverdueNotified;
        _cash -= payment;
        if (wages) _payrollSpend += payment;
        else _overheadSpend += payment;
        due = 0;
        if (wages && label != "Manager wages") GiveManagerPayrollRebate(payment);
        if (label == "Manager wages") _managerWageAgeSeconds = 0;
        if (label == "Receptionist wages") { _receptionWageUnpaidSeconds = 0; _receptionOverdueNotified = false; }
        if (label == "Rent & utilities") _rentUnpaidSeconds = 0;
        if (!Save())
        {
            due = payment; _financeRebateIncome = rebate; _cash = cash; _payrollSpend = payroll; _overheadSpend = overhead; _managerSavings = savings;
            _managerWageAgeSeconds = managerAge; _receptionWageUnpaidSeconds = receptionAge;
            _rentUnpaidSeconds = rentAge; _receptionOverdueNotified = receptionNotified;
            Notify(label + ": payment cancelled because its save did not commit.");
            return false;
        }
        Notify(label + " PAID: $" + payment.ToString("N0") + ".");
        return true;
    }

    private bool PayMechanicInvoice(int slot) { return TryPayBusinessBills(3, slot); }

    private void PayAllMechanicWages() { TryPayBusinessBills(2); }

    // Supplier/all/mechanic settlements share one rollback-safe save boundary.
    private bool TryPayBusinessBills(int mode, int slot = -1)
    {
        if (!_bootGameplayActivated || Game.IsPaused || mode < 0 || mode > 3) return false;
        if (mode == 3 && (slot < 0 || slot >= _mechanics.Count)) return false;
        long staff = mode == 1 ? checked(MechanicWagesDue() + _receptionWagesDue) :
            mode == 2 ? MechanicWagesDue() : mode == 3 ? _mechanics[slot].UnpaidWages : 0;
        long wages = checked(staff + (mode == 1 ? _managerWagesDue : 0));
        long rent = mode == 1 ? _rentUtilitiesDue : 0;
        long supplies = mode <= 1 ? _supplierInvoicesDue : 0;
        long total = checked(wages + rent + supplies);
        if (total <= 0 || _cash < total) { Notify("No payable bill, or insufficient business cash. No payment made."); return false; }
        if (_payrollSpend > long.MaxValue - wages || _overheadSpend > long.MaxValue - rent || _partsSpend > long.MaxValue - supplies) return false;
        FinanceSnapshot before = CaptureFinanceSnapshot();
        long former = _formerStaffWagesDue, reception = _receptionWagesDue, manager = _managerWagesDue, savings = _managerSavings;
        double managerAge = _managerWageAgeSeconds, receptionAge = _receptionWageUnpaidSeconds, rentAge = _rentUnpaidSeconds;
        bool notified = _receptionOverdueNotified;
        long[] unpaid = new long[_mechanics.Count]; double[] ages = new double[_mechanics.Count];
        var overdue = new HashSet<int>(_mechanicOverdueNotified);
        for (int i = 0; i < _mechanics.Count; i++)
        {
            unpaid[i] = _mechanics[i].UnpaidWages; ages[i] = _mechanics[i].WageUnpaidSeconds;
            if (mode == 1 || mode == 2 || mode == 3 && i == slot)
            { _mechanics[i].UnpaidWages = 0; _mechanics[i].WageUnpaidSeconds = 0; _mechanicOverdueNotified.Remove(i); }
        }
        if (mode == 1 || mode == 2) _formerStaffWagesDue = 0;
        if (mode <= 1) _supplierInvoicesDue = 0;
        if (mode == 1)
        {
            _receptionWagesDue = 0; _managerWagesDue = 0; _rentUtilitiesDue = 0;
            _managerWageAgeSeconds = 0; _receptionWageUnpaidSeconds = 0; _rentUnpaidSeconds = 0; _receptionOverdueNotified = false;
        }
        _cash -= total; _payrollSpend += wages; _overheadSpend += rent; _partsSpend += supplies;
        GiveManagerPayrollRebate(staff);
        if (!Save())
        {
            _cash = before.V[0]; _partsSpend = before.V[5]; _payrollSpend = before.V[6]; _overheadSpend = before.V[7];
            _rentUtilitiesDue = before.V[12]; _supplierInvoicesDue = before.V[13]; _financeRebateIncome = before.V[16];
            _formerStaffWagesDue = former; _receptionWagesDue = reception; _managerWagesDue = manager; _managerSavings = savings;
            _managerWageAgeSeconds = managerAge; _receptionWageUnpaidSeconds = receptionAge; _rentUnpaidSeconds = rentAge; _receptionOverdueNotified = notified;
            _mechanicOverdueNotified.Clear(); foreach (int item in overdue) _mechanicOverdueNotified.Add(item);
            for (int i = 0; i < _mechanics.Count; i++) { _mechanics[i].UnpaidWages = unpaid[i]; _mechanics[i].WageUnpaidSeconds = ages[i]; }
            Notify("Payment save failed; cash and bills restored."); return false;
        }
        Notify("Bills PAID $" + total.ToString("N0") + ". Cash, liabilities and audit committed together.");
        return true;
    }

    private void ShowExpenseStatus()
    {
        ShowStatusNotice("BUSINESS BILLS",
            "Outstanding $" + TotalOutstandingBills().ToString("N0") +
                " | Cash $" + _cash.ToString("N0"),
            "Mechanics $" + MechanicWagesDue().ToString("N0") +
                " | Reception $" + _receptionWagesDue.ToString("N0"),
            "Rent/utility $" + _rentUtilitiesDue.ToString("N0") +
                " | Supplier invoices $" + _supplierInvoicesDue.ToString("N0"),
            "Manager wages $" + _managerWagesDue.ToString("N0") +
                " | Overdue payroll " + CountOverduePayroll() + " | Bills age while shop is open.");
    }

    private void ExecuteBusinessExpensesSelection()
    {
        switch (_selection)
        {
            case 0: ShowExpenseStatus(); break;
            case 1: _menuPage = MenuPage.MechanicInvoices; _selection = 0; break;
            case 2: PayInvoice(ref _receptionWagesDue, true, "Receptionist wages"); break;
            case 3: PayInvoice(ref _rentUtilitiesDue, false, "Rent & utilities"); break;
            case 4: TryPayBusinessBills(0); break;
            case 5: TryPayBusinessBills(1); break;
            case 6: BuyInventory(-1, 10); break;
            case 7: _menuPage = MenuPage.EquipmentManagement; _selection = 0; break;
            case 8: _financesFromExpenses = true; _menuPage = MenuPage.Finances; _selection = 0; break;
            case 9: PayInvoice(ref _managerWagesDue, true, "Manager wages"); break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteMechanicInvoicesSelection()
    {
        if (_selection >= 0 && _selection <= 2) PayMechanicInvoice(_selection);
        else if (_selection == 3) PayInvoice(ref _formerStaffWagesDue, true, "Former staff wages");
        else if (_selection == 4) PayAllMechanicWages();
        else NavigateBack();
    }

    // Aging is measured in ACTIVE shop seconds, not wall time, so time spent outside
    // the shop or with the game paused does not make staff suddenly angry.
    private int PayrollMorale(double oldestAge, long due)
    {
        if (due <= 0) return 100;
        double overdueTime = oldestAge - _payrollDueSeconds - _payrollGraceSeconds;
        if (overdueTime <= 0) return 100;
        int steps = 1 + (int)Math.Floor(overdueTime / Math.Max(60, _wagePeriodSeconds));
        return Math.Max(50, 100 - 5 * steps);
    }

    private int CountOverduePayroll()
    {
        int count = (_receptionWagesDue > 0 && _receptionWageUnpaidSeconds > _payrollDueSeconds) ? 1 : 0;
        foreach (Mechanic m in _mechanics)
            if (m.UnpaidWages > 0 && m.WageUnpaidSeconds > _payrollDueSeconds) count++;
        return count;
    }

    private string PayrollDueText(double age, long due)
    {
        if (due <= 0) return "PAID";
        double until = _payrollDueSeconds - age;
        return until >= 0 ? "DUE IN " + Math.Ceiling(until / 60) + "m" :
            "OVERDUE " + Math.Ceiling(-until / 60) + "m";
    }

    private void ShowPayrollHealth()
    {
        string first = _mechanics.Count > 0 ?
            _mechanics[0].Name + " $" + _mechanics[0].UnpaidWages.ToString("N0") +
                " " + PayrollDueText(_mechanics[0].WageUnpaidSeconds, _mechanics[0].UnpaidWages) +
                " morale " + PayrollMorale(_mechanics[0].WageUnpaidSeconds, _mechanics[0].UnpaidWages) : "Mechanic 1 NONE";
        string second = _mechanics.Count > 1 ?
            _mechanics[1].Name + " $" + _mechanics[1].UnpaidWages.ToString("N0") +
                " " + PayrollDueText(_mechanics[1].WageUnpaidSeconds, _mechanics[1].UnpaidWages) +
                " morale " + PayrollMorale(_mechanics[1].WageUnpaidSeconds, _mechanics[1].UnpaidWages) : "Mechanic 2 NONE";
        string third = _mechanics.Count > 2 ?
            _mechanics[2].Name + " $" + _mechanics[2].UnpaidWages.ToString("N0") +
                " " + PayrollDueText(_mechanics[2].WageUnpaidSeconds, _mechanics[2].UnpaidWages) +
                " morale " + PayrollMorale(_mechanics[2].WageUnpaidSeconds, _mechanics[2].UnpaidWages) : "Mechanic 3 NONE";
        ShowStatusNotice("STAFF PAYROLL / MORALE",
            "Due " + _payrollDueSeconds + "s + grace " + _payrollGraceSeconds + "s | Overdue " + CountOverduePayroll(),
            first, second, third,
            "Reception $" + _receptionWagesDue.ToString("N0") + " " +
                PayrollDueText(_receptionWageUnpaidSeconds, _receptionWagesDue) +
                " | morale " + PayrollMorale(_receptionWageUnpaidSeconds, _receptionWagesDue));
    }

    private long AccruedBusinessCosts()
    {
        return _partsSpend + _payrollSpend + _overheadSpend + _hireSpend +
            _trainingSpend + _equipmentSpend + TotalOutstandingBills();
    }

    private void ShowShiftProfit()
    {
        if (!_shiftReportReady) { Notify("Open the business once to begin shift profit tracking."); return; }
        long sales = _revenue - _shiftSalesBaseline;
        long accruedCosts = AccruedBusinessCosts() - _shiftCostsBaseline;
        ShowStatusNotice("SHIFT PROFIT / " + (_clockedIn ? "OPEN" : "LAST"),
            "Shift revenue $" + sales.ToString("N0") + " | Accrued costs $" + accruedCosts.ToString("N0"),
            "Shift accrued profit $" + (sales - accruedCosts).ToString("N0"),
            "Lifetime revenue $" + _revenue.ToString("N0") + " | costs $" + AccruedBusinessCosts().ToString("N0"),
            "Includes unpaid invoices; supplier orders are expensed at purchase.");
    }

    // A summarized accounting view; customer receipts are also written to CSV.
    private void ExecuteFinancesSelection()
    {
        MenuPage[] pages = { MenuPage.FinancialOverview, MenuPage.IncomeSales, MenuPage.BusinessExpenses,
            MenuPage.ProfitLoss, MenuPage.TransactionHistory, MenuPage.OwnerTransfers };
        if (_selection == 6) { _financeLifetime = !_financeLifetime; return; }
        if (_selection >= pages.Length) { NavigateBack(); return; }
        _menuPage = pages[_selection]; _selection = 0; _financeHistoryPage = 0;
    }

    private string FinancePeriodLabel()
    { return _financeLifetime ? "LIFETIME" : _financeBook == null || _financeBook.ShiftNumber == 0 ? "NO SHIFT" : _financeBook.ShiftOpen && _clockedIn ? "CURRENT SHIFT" : "LAST SHIFT"; }

    private FinanceSnapshot FinanceViewStart()
    { return _financeLifetime ? new FinanceSnapshot() : _financeBook == null || _financeBook.ShiftNumber == 0 ? CaptureFinanceSnapshot() : _financeBook.ShiftStart; }

    private FinanceSnapshot FinanceViewEnd()
    {
        if (_financeLifetime || _financeBook == null || _financeBook.ShiftNumber == 0 || (_financeBook.ShiftOpen && _clockedIn)) return CaptureFinanceSnapshot();
        return _financeBook.ShiftEnd;
    }

    private string FinanceValue(MenuPage page, int index)
    {
        if (index == 0 && page != MenuPage.OwnerTransfers) return FinancePeriodLabel();
        FinanceSnapshot end = FinanceViewEnd(), delta = FinanceSnapshot.Difference(end, FinanceViewStart());
        long value;
        if (page == MenuPage.IncomeSales)
        { if (index >= 1 && index <= 4) value = delta.V[index]; else if (index == 5) value = FinanceSnapshot.Sales(delta); else if (index == 6) value = checked(delta.V[16] + delta.V[17]); else return "BACK"; }
        else if (page == MenuPage.ProfitLoss)
        { if (index == 1) value = FinanceSnapshot.Sales(delta); else if (index == 2) value = FinanceSnapshot.Paid(delta); else if (index == 3) value = checked(FinanceSnapshot.Paid(delta) + FinanceSnapshot.Due(delta)); else if (index == 4) value = FinanceSnapshot.Profit(delta); else return ">"; }
        else if (page == MenuPage.OwnerTransfers)
        { if (index == 2) value = delta.V[14]; else if (index == 3) value = delta.V[15]; else return index < 2 ? "$1,000" : index == 4 ? FinancePeriodLabel() : "BACK"; }
        else
        {
            if (index == 5)
            {
                FinanceSnapshot live = CaptureFinanceSnapshot();
                for (int i = 0; i < FinanceSnapshot.Size; i++) if (_financeBook == null || live.V[i] != _financeBook.Checkpoint.V[i]) return "UNSAVED";
                return "COMMITTED";
            }
            if (index == 1) value = end.V[0]; else if (index == 2) value = FinanceSnapshot.Sales(delta); else if (index == 4) value = FinanceSnapshot.Due(end); else return ">"; }
        return "$" + value.ToString("N0");
    }

    private void ShowFinanceBridge()
    {
        FinanceSnapshot start = _financeLifetime ? _financeBook.Opening : FinanceViewStart();
        FinanceSnapshot end = FinanceViewEnd(), d = FinanceSnapshot.Difference(end, start);
        long sales = FinanceSnapshot.Sales(d), paid = FinanceSnapshot.Paid(d);
        long other = checked(d.V[16] + d.V[17]);
        long adjustment = checked(d.V[0] - sales + paid - d.V[14] + d.V[15] - other);
        long inflow = checked(sales + other + d.V[14] + Math.Max(0L, adjustment));
        long outflow = checked(paid + d.V[15] - Math.Min(0L, adjustment));
        ShowStatusNotice(_financeLifetime ? "CASH BRIDGE - SINCE AUDIT START" : "CASH BRIDGE - " + FinancePeriodLabel(),
            "Opening $" + start.V[0].ToString("N0") + " + inflows $" + inflow.ToString("N0") + " - outflows $" + outflow.ToString("N0") + " = $" + end.V[0].ToString("N0"),
            "Sales $" + sales.ToString("N0") + " | payments $" + paid.ToString("N0") + " | rebates/refunds $" + other.ToString("N0"),
            "Owner deposits $" + d.V[14].ToString("N0") + " | withdrawals $" + d.V[15].ToString("N0"),
            "Unclassified adjustment $" + adjustment.ToString("N0") + (adjustment != 0 ? " - REVIEW HISTORY" : " | balanced without adjustments"));
    }

    private FinanceEntry[] FinanceHistoryRows()
    {
        var rows = new List<FinanceEntry>();
        if (_financeBook == null) return rows.ToArray();
        int end = _financeBook.Entries.Count - 1 - _financeHistoryPage * 8;
        for (int i = end; i >= Math.Max(0, end - 7); i--) rows.Add(_financeBook.Entries[i]);
        return rows.ToArray();
    }

    private string[] FinanceHistoryItems()
    {
        var labels = new List<string>();
        foreach (FinanceEntry e in FinanceHistoryRows()) labels.Add("#" + e.Id + " " + FinanceCategories[e.Category]);
        labels.Add("Older Transactions >"); labels.Add("Newer Transactions >"); labels.Add("< Back to Accounting");
        return labels.ToArray();
    }

    private void ExecuteFinancePage()
    {
        if (_menuPage == MenuPage.TransactionHistory)
        {
            FinanceEntry[] rows = FinanceHistoryRows();
            if (_selection < rows.Length)
            {
                FinanceEntry e = rows[_selection];
                ShowStatusNotice("TRANSACTION #" + e.Id, FinanceCategories[e.Category] + " | amount $" + e.Amount.ToString("N0"),
                    "Active GTA seconds " + (e.ActiveMs / 1000d).ToString("0.000") + " | shift " + e.Shift,
                    "Resulting business cash $" + e.CashAfter.ToString("N0"),
                    e.IsCash ? "Cash movement; same-save entries form one batch." : "ACCRUAL ONLY - no cash payment. Purchase-expense basis.");
            }
            else if (_selection == rows.Length) { if ((_financeHistoryPage + 1) * 8 < _financeBook.Entries.Count) _financeHistoryPage++; _selection = 0; }
            else if (_selection == rows.Length + 1) { _financeHistoryPage = Math.Max(0, _financeHistoryPage - 1); _selection = 0; }
            else NavigateBack();
            return;
        }
        if (_menuPage == MenuPage.OwnerTransfers)
        {
            if (_selection < 2) TransferPlayerMoney(_selection == 0);
            else if (_selection == 4) _financeLifetime = !_financeLifetime;
            else if (_selection == 5) NavigateBack();
            else Notify("Owner transfers are capital movements, never sales or profit. Totals before the audit upgrade are unknown.");
            return;
        }
        if (_selection == 0) { _financeLifetime = !_financeLifetime; return; }
        if (_menuPage == MenuPage.IncomeSales)
        {
            if (_selection == 7) NavigateBack();
            else ShowStatusNotice("INCOME & SALES - " + FinancePeriodLabel(),
                "Physical customer repairs = old customer revenue minus remote subset.",
                "Remote + physical + autonomous + retail = gross sales exactly once.",
                "Manager rebates/refunds are other credits, not sales. Historical credits before audit are unknown.",
                "Cash payments, deposits/withdrawals and outstanding bills are shown separately.");
        }
        else if (_menuPage == MenuPage.ProfitLoss)
        {
            if (_selection == 6) NavigateBack();
            else ShowStatusNotice("PROFIT & LOSS - " + FinancePeriodLabel(),
                "Accounting profit = sales + known rebates/refunds - paid expenses - outstanding expense change.",
                "Purchase-expense basis: supplies recognized at receipt; not inventory-valued/COGS financial statements.",
                "Paying an opening bill reduces cash but is not a second current-shift expense.",
                "Owner deposits/withdrawals are excluded from profit. Lifetime historical credits may be incomplete.");
        }
        else if (_selection == 6) NavigateBack();
        else if (_selection == 4) ShowExpenseStatus();
        else ShowFinanceBridge();
    }


    private void ShowCustomerOrderStatus()
    {
        foreach (SavedRepairPromise p in _pendingRepairPromises)
            Notify("Saved repair #" + p.Quote.OrderId + " | fixed $" + p.Quote.Total +
                " | " + p.RemainingSeconds.ToString("0.0") + " active seconds remaining; awaiting normal dispatch.");
        CustomerOrder order = _customerOrder;
        if (order == null)
        {
            ShowStatusNotice("CUSTOMER ORDER STATUS",
                "Active order NONE | Pending " + _waitingCustomerRequests +
                    "/" + _customerQueueCapacity + " | Waiting cars " + StagedCount(),
                "Drop-off " + (DropoffOccupied(null) ? "OCCUPIED" : "CLEAR") +
                    " | Completed " + _customerCompleted,
                "Customer sales $" + _customerRevenue.ToString("N0"),
                "Last: " + _lastOrderOutcome);
            return;
        }
        ShowStatusNotice("CUSTOMER ORDER STATUS",
            "Live " + _activeCustomerOrders.Count + "/" + _mechanics.Count +
                " | Oldest #" + order.Id + " | " + ServiceName(order.ServiceKind),
            "Quote $" + order.Price.ToString("N0") +
                " | Parts " + order.PartsNeeded + " | Paid " + (order.Paid ? "YES" : "NO"),
            "Stage: " + order.Stage,
            "Live orders " + _activeCustomerOrders.Count + "/" + _mechanics.Count +
                " | Queued " + _waitingCustomerRequests + "/" + EffectiveQueueCapacity() +
                " | Roadside " + StagedCount());
        Notify(RepairQuoteBreakdown(order.Pricing));
        if (_activeCustomerOrders.Count > 1)
            foreach (CustomerOrder other in _activeCustomerOrders)
                if (other != order)
                    Notify("Live order #" + other.Id + " | " + other.Stage + " | " +
                        (other.Paid ? "PAID" : "$" + other.Price.ToString("N0")));
        if (_waitingCustomerRequests > 0)
            Notify("Queued: " + _waitingCustomerRequests + "/" + EffectiveQueueCapacity() +
                " | parked waiting cars " + StagedCount());
        if (order.Stage == CustomerStage.WaitingForMechanic)
        {
            int available = FreeStockForService(order.ServiceKind);
            Notify(available < order.PartsNeeded ?
                "WAITING FOR PARTS: unreserved " + available + ", need " + order.PartsNeeded :
                "WAITING FOR AVAILABLE MECHANIC at break area.");
            return;
        }
        if (order.VehicleHandoff && order.Stage != CustomerStage.Repairing &&
            order.MechanicSlot >= 0 && order.MechanicSlot < _mechanics.Count)
            Notify(_mechanics[order.MechanicSlot].Name + " | Vehicle handoff: " + order.Stage);
        if (order.Stage == CustomerStage.Repairing &&
            order.MechanicSlot >= 0 && order.MechanicSlot < _mechanics.Count)
        {
            Mechanic worker = _mechanics[order.MechanicSlot];
            if (worker.ArrivalPhase != PhaseWorking)
            {
                Notify(worker.Name + " assigned | On route to Bay " +
                    (order.MechanicSlot + 1) + " | Repair not yet started.");
                return;
            }
            int progress = (int)Math.Min(100.0, Math.Max(0.0,
                worker.WorkSeconds / Math.Max(1.0, order.RepairSeconds) * 100.0));
            Notify(worker.Name + " | Repair " + progress + "% | " +
                Math.Max(0, (int)Math.Ceiling(order.RepairSeconds - worker.WorkSeconds)) +
                " seconds remaining.");
        }
    }

    // Keep placement controls out of the general Customer Service menu.
    private void ExecuteCustomerPlacementsSelection()
    {
        switch (_selection)
        {
            case 0: OpenPointEditor(_customerDropoff, "Customer Drop-off Parking", -1, false); break;
            case 1: OpenPointEditor(_customerDriveSpawn, "Customer Drive Spawn (road)", -1, false); break;
            case 2: OpenPointEditor(_customerApproach, "Customer Reception Approach", -1, true); break;
            case 3: OpenPointEditor(_customerQueue, "Customer Reception Queue", -1, false); break;
            case 4: OpenPointEditor(_queueCar2, "Waiting Car 2 (road queue)", -1, false); break;
            case 5: OpenPointEditor(_queueCar3, "Waiting Car 3 (road queue)", -1, false); break;
            case 6:
                _customCarBaysFromDriveOptions = false;
                _menuPage = MenuPage.ServiceVehicleBays; _selection = 0; break;
            case 7: _menuPage = MenuPage.CustomerWalkingRoutes; _selection = 0; break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteCustomerWalkingRouteSelection()
    {
        if (_selection >= _customerWalkingRoute.Length) { NavigateBack(); return; }
        OpenPointEditor(_customerWalkingRoute[_selection],
            "Customer Walk " + (_selection + 1) + "/3 (optional)", -1, true);
    }

    private void ExecuteCustomerQueueSettingsSelection()
    {
        switch (_selection)
        {
            case 0: ShowQueueStatus(); break;
            case 1:
                _customerQueueCapacity = _customerQueueCapacity >= 3 ? 1 : _customerQueueCapacity + 1;
                if (_waitingCustomerRequests > EffectiveQueueCapacity())
                    _waitingCustomerRequests = EffectiveQueueCapacity();
                // Discard newest physical reservations first if capacity decreased.
                if (_waitingCustomerRequests < 2 && _stagedThirdCustomerOrder != null)
                {
                    CancelCreatedCustomer(_stagedThirdCustomerOrder);
                    _stagedThirdCustomerOrder = null;
                }
                if (_waitingCustomerRequests == 0) ClearStagedCustomers();
                Save();
                Notify("Queue maximum " + _customerQueueCapacity + "; staff allows " + EffectiveQueueCapacity() + ". " +
                    "Up to two physical cars can wait in lanes 2/3.");
                break;
            case 2: ScheduleCustomerRequest(); break;
            case 3:
                _waitingCustomerRequests = 0;
                ClearStagedCustomers();
                _waitingForDropoff = false;
                Notify("Pending requests and waiting cars cleared. Active order is untouched.");
                break;
            default: NavigateBack(); break;
        }
    }

    private void ShowQueueStatus()
    {
        ShowStatusNotice("CUSTOMER QUEUE STATUS",
            "Pending " + _waitingCustomerRequests + "/" + EffectiveQueueCapacity() +
                " | Staged cars " + StagedCount(),
            "Drop-off " + (DropoffOccupied(null) ? "OCCUPIED" : "CLEAR") +
                " | Active order " + (_customerOrder == null ? "NONE" : "#" + _customerOrder.Id),
            "Car 2 " + (_queueCar2.IsSet ? "WAITING SPOT SET" : (_customerDriveSpawn.IsSet ? "USING ROAD SPAWN" : "NO SPOT")) +
                " | Car 3 " + (_queueCar3.IsSet ? "SAVED SPOT" : "NOT SET"),
            RoadsideQueueDiagnostic());
    }

    private void ScheduleCustomerRequest()
    {
        if (!_customerServiceEnabled || !_clockedIn)
            Notify("Open shop and enable customer arrivals first.");
        else if (_waitingCustomerRequests >= EffectiveQueueCapacity())
            Notify("Customer queue full (" + EffectiveQueueCapacity() + "). Upgrade receptionist for more capacity.");
        else
        {
            _waitingCustomerRequests++;
            Notify("Customer scheduled: " + _waitingCustomerRequests + "/" + EffectiveQueueCapacity() +
                ". Dispatch waits for a clear drop-off.");
        }
    }

    private void ExecuteCustomerSelection()
    {
        switch (_selection)
        {
            case 0: ShowCustomerOrderStatus(); break;
            case 1: _menuPage = MenuPage.CustomerQueueSettings; _selection = 0; break;
            case 2: _menuPage = MenuPage.CustomerPlacements; _selection = 0; break;
            case 3: _menuPage = MenuPage.VehicleDriveOptions; _selection = 0; break;
            case 4:
                if (!_customerServiceEnabled && !CustomerConfigurationReady())
                { Notify("Set customer positions, reception, break area and a mechanic bay first."); break; }
                _customerServiceEnabled = !_customerServiceEnabled;
                CancelAllCustomerOrders("Customer mode switched");
                ClearStagedCustomers();
                _waitingCustomerRequests = 0;
                _waitingForDropoff = false;
                foreach (Mechanic mechanic in _mechanics)
                { mechanic.HasJob = false; mechanic.WorkSeconds = 0; mechanic.IdleSeconds = 0; }
                _nextCustomerAt = DateTime.UtcNow.AddSeconds(5);
                Save();
                Notify("Customer arrivals " + (_customerServiceEnabled ? "ON" : "OFF") + ".");
                break;
            case 5: ScheduleCustomerRequest(); break;
            case 6:
                _customerBlipsEnabled = !_customerBlipsEnabled;
                foreach (CustomerOrder order in _activeCustomerOrders) SyncCustomerBlip(order);
                if (_stagedCustomerOrder != null) SyncCustomerBlip(_stagedCustomerOrder);
                if (_stagedThirdCustomerOrder != null) SyncCustomerBlip(_stagedThirdCustomerOrder);
                Save();
                Notify("Customer Blips " + (_customerBlipsEnabled ? "ON" : "OFF") + ".");
                break;
            case 7:
                _showPlacementMarkers = !_showPlacementMarkers;
                Notify("Placement Markers " + (_showPlacementMarkers ? "ON" : "OFF") + ".");
                break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteVehicleDriveOptionSelection()
    {
        switch (_selection)
        {
            case 0:
                if (!_driveInEnabled && !_customerDriveSpawn.IsSet)
                { Notify("Save a Customer Road Spawn before enabling drive-in."); return; }
                if (_customerOrder != null || _stagedCustomerOrder != null || _stagedThirdCustomerOrder != null)
                { Notify("Wait for customer vehicles to finish before changing drive-in."); return; }
                _driveInEnabled = !_driveInEnabled;
                Save();
                Notify("Drive-in Customers " + (_driveInEnabled ? "ON" : "OFF") + ".");
                break;
            case 1:
                if (_customerOrder != null) { Notify("Finish the active order before changing valet transfer."); return; }
                _vehicleTransfersEnabled = !_vehicleTransfersEnabled;
                Save();
                Notify("Mechanic vehicle transfers " + (_vehicleTransfersEnabled ? "ON" : "OFF") + ".");
                break;
            case 2:
                if (_customerOrder != null) { Notify("Finish active order before changing parking mode."); return; }
                _automaticLotMode = !_automaticLotMode;
                Save();
                Notify("Service parking: " + (_automaticLotMode ?
                    "AUTO LOT uses mechanic workstation offsets." :
                    "CUSTOM MLO uses manually saved vehicle bay positions."));
                break;
            case 3: _menuPage = MenuPage.VehicleAutoLotOptions; _selection = 0; break;
            case 4:
                _customCarBaysFromDriveOptions = true;
                _menuPage = MenuPage.ServiceVehicleBays; _selection = 0;
                break;
            case 5:
                if (_activeCustomerOrders.Count > 0)
                { Notify("Finish active vehicle orders before changing reverse mode."); return; }
                _valetReverseOut = !_valetReverseOut;
                Save();
                Notify("Reverse before driving " + (_valetReverseOut ? "ON" : "OFF") +
                    ". Requires clear space behind the car; disabled if blocked.");
                break;
            case 6: ShowVehicleDrivingStatus(); break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteVehicleAutoLotSelection()
    {
        if (_selection == 3) { NavigateBack(); return; }
        if (_selection == 2)
        {
            _autoLotPreview = !_autoLotPreview;
            Notify("Computed auto-bay preview " + (_autoLotPreview ? "ON" : "OFF") + ".");
            return;
        }
        if (_customerOrder != null)
        { Notify("Finish active vehicle job before adjusting parking targets."); return; }
        if (_selection == 0) _autoLotRightSide = !_autoLotRightSide;
        else if (_selection == 1)
            _autoLotOffsetMeters = _autoLotOffsetMeters >= 6.9f ? 3f : _autoLotOffsetMeters + 1f;
        Save();
        Notify("Auto-lot: " + (_autoLotRightSide ? "right" : "left") +
            " side, offset " + _autoLotOffsetMeters.ToString("0") + " m. Preview available.");
    }

    private void ExecuteEmployeeSelection()
    {
        switch (_selection)
        {
            case 0: ShowEmployeeStatus(); break;
            case 1: _menuPage = MenuPage.MechanicManagement; _selection = 0; break;
            case 2: _menuPage = MenuPage.ReceptionistManagement; _selection = 0; break;
            case 3: _menuPage = MenuPage.ManagerManagement; _selection = 0; break;
            case 4: _menuPage = MenuPage.EmployeeSettings; _selection = 0; break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteMechanicManagement()
    {
        switch (_selection)
        {
            case 0: _menuPage = MenuPage.StaffRoster; _selection = 0; break;
            case 1: _menuPage = MenuPage.HireMechanicRoster; _selection = 0; break;
            case 2: _mechanicRosterFromManagement = true; _menuPage = MenuPage.MechanicRouteRoster; _selection = 0; break;
            case 3: _mechanicPathsFromManagement = true; _menuPage = MenuPage.MechanicWalkingPaths; _selection = 0; break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteReceptionistManagement()
    {
        switch (_selection)
        {
            case 0: _menuPage = MenuPage.ReceptionistProfile; _selection = 0; break;
            case 1: _menuPage = MenuPage.HireReceptionistRoster; _selection = 0; break;
            case 2: _receptionPathsFromManagement = true; _menuPage = MenuPage.ReceptionWalkingPaths; _selection = 0; break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteReceptionistProfile()
    {
        if (_selection >= 4) { NavigateBack(); return; }
        if (!_receptionistHired)
        {
            Notify("No receptionist hired. Open Hire / Replace Receptionist first."); return;
        }
        switch (_selection)
        {
            case 0:
                ShowStatusNotice("RECEPTIONIST PROFILE",
                    _receptionistName + " | Skill " + (_receptionistTier + 1) + "/3",
                    "Pending queue capacity " + EffectiveQueueCapacity() + "/" + _customerQueueCapacity,
                    "Wage $" + (_receptionistWage + _receptionistWageBonus) + " per period",
                    "Unpaid $" + _receptionWagesDue + " | " + PayrollDueText(_receptionWageUnpaidSeconds, _receptionWagesDue));
                break;
            case 1:
                if (_receptionistTier >= 2) { Notify("Receptionist already has maximum queue skill."); return; }
                if (_cash < 500) { Notify("Training requires $500 business cash."); return; }
                long trainingActual = ManagerAdjustedExpense(500);
                _cash -= trainingActual; _trainingSpend += trainingActual; _receptionistTier++;
                Save(); Notify(_receptionistName + " trained. Queue capacity " + EffectiveQueueCapacity() + "."); break;
            case 2:
                _receptionistWageBonus += 25;
                Save(); Notify("Receptionist wage now $" + (_receptionistWage + _receptionistWageBonus) + "/period."); break;
            case 3:
                // Preserve unpaid wages in the common outstanding staff ledger.
                _formerStaffWagesDue += _receptionWagesDue;
                _receptionWagesDue = 0;
                _receptionWageUnpaidSeconds = 0;
                _receptionistHired = false;
                DismissReceptionist(); Save();
                Notify("Receptionist dismissed; outstanding wages retained under former staff.");
                _menuPage = MenuPage.ReceptionistManagement; _selection = 0; break;
        }
    }

    private void FireSelectedMechanic()
    {
        int slot = _editingStaffSlot;
        if (slot < 0 || slot >= _mechanics.Count) { Notify("Mechanic profile is unavailable."); return; }
        // Never cancel a paid customer job or invalidate another mechanic index.
        foreach (CustomerOrder order in _activeCustomerOrders)
            if (order.MechanicSlot >= 0)
            { Notify("Finish active customer repairs before dismissing staff."); return; }
        Mechanic fired = _mechanics[slot];
        if (fired.HasJob) { Notify("Finish this mechanic's assigned job first."); return; }
        DismissWorker(fired);
        _formerStaffWagesDue += fired.UnpaidWages;
        _mechanics.RemoveAt(slot);
        _mechanicOverdueNotified.Clear();
        _editingStaffSlot = -1;
        Save(); Notify(fired.Name + " dismissed. Unpaid wages retained for settlement.");
        _menuPage = MenuPage.StaffRoster; _selection = 0;
    }

    // Levels are derived from XP so there is only one persisted progression source.
    private static int MechanicLevel(Mechanic mechanic)
    {
        return Math.Min(10, 1 + mechanic.Xp / 100);
    }

    private static string RankName(Mechanic mechanic)
    {
        int level = MechanicLevel(mechanic);
        return level >= 6 ? "Master" : level >= 3 ? "Senior" : "Junior";
    }

    private static string SpecialtyName(int specialty)
    {
        return specialty == 1 ? "Brake Specialist" : specialty == 2 ? "Diagnostic Tech" : "General Mechanic";
    }

    private long MechanicWage(Mechanic mechanic)
    {
        return Math.Max(0L, (long)_wagePerEmployee + Math.Max(0, mechanic.WageBonus) +
            (mechanic.Specialty == 0 ? 0L : 50L));
    }

    private bool SpecialistMatches(Mechanic mechanic, int serviceKind)
    {
        return (mechanic.Specialty == 1 && serviceKind == 1) ||
               (mechanic.Specialty == 2 && serviceKind == 2);
    }

    private void AwardMechanicXp(Mechanic mechanic, int amount)
    {
        int before = MechanicLevel(mechanic);
        mechanic.Xp = Math.Min(100000, mechanic.Xp + Math.Max(0, amount));
        mechanic.JobsCompleted++;
        if (MechanicLevel(mechanic) > before)
            Notify(mechanic.Name + " reached level " + MechanicLevel(mechanic) + "!");
    }

    private int EffectiveQueueCapacity()
    {
        // Receptionist skill cannot exceed the user's own configured maximum.
        return Math.Max(1, Math.Min(_customerQueueCapacity, _receptionistHired ? _receptionistTier + 1 : 1));
    }

    private int MechanicCandidatePrice(int tier)
    {
        return _onboardingCost * (tier == 0 ? 1 : tier == 1 ? 2 : 3);
    }

    private int ReceptionCandidatePrice(int tier)
    {
        return _receptionistHiringCost * (tier == 0 ? 1 : tier == 1 ? 2 : 3);
    }

    private void ExecuteHireMechanicRoster()
    {
        if (_selection >= 3) { NavigateBack(); return; }
        if (_mechanics.Count >= _maxMechanics) { Notify("All mechanic bays are staffed."); return; }
        int tier = _selection;
        int basePrice = MechanicCandidatePrice(tier);
        int price = (int)(basePrice * (100 - ManagerDiscountPercent()) / 100);
        if (_cash < price) { Notify("Need $" + price.ToString("N0") + " to hire this mechanic."); return; }
        Mechanic hired = new Mechanic();
        hired.Name = _staffNames[_mechanics.Count % _staffNames.Length];
        hired.Specialty = tier == 1 ? 1 : tier == 2 ? 2 : 0;
        hired.Xp = tier == 1 ? 200 : tier == 2 ? 400 : 0;
        hired.WageBonus = tier * 75;
        hired.NextSpawnAttempt = DateTime.UtcNow.AddSeconds(_mechanics.Count * _arrivalSpacingSeconds);
        _cash -= price;
        _hireSpend += price;
        _managerSavings += basePrice - price;
        _mechanics.Add(hired);
        _menuPage = MenuPage.MechanicManagement; _selection = 0;
        Save();
        Notify("Hired " + hired.Name + " — " + SpecialtyName(hired.Specialty) +
            ", Level " + MechanicLevel(hired) + ", wage $" + MechanicWage(hired) + "/period.");
    }

    private void ExecuteHireReceptionistRoster()
    {
        if (_selection >= 3) { NavigateBack(); return; }
        if (_receptionistHired) { Notify("Dismiss your current receptionist before hiring another."); return; }
        int tier = _selection;
        int basePrice = ReceptionCandidatePrice(tier);
        int price = (int)(basePrice * (100 - ManagerDiscountPercent()) / 100);
        if (_cash < price) { Notify("Need $" + price.ToString("N0") + " for this receptionist."); return; }
        _cash -= price;
        _hireSpend += price;
        _managerSavings += basePrice - price;
        _receptionistTier = tier;
        _receptionistWageBonus = tier * 75;
        _receptionistName = tier == 0 ? "Taylor Brooks" : tier == 1 ? "Morgan Hayes" : "Jordan Ellis";
        _receptionistHired = true;
        _receptionist.NextSpawnAttempt = DateTime.UtcNow.AddSeconds(2);
        // Never delete queued customers already scheduled by the former receptionist.
        _menuPage = MenuPage.ReceptionistManagement; _selection = 0;
        Save();
        Notify("Hired " + _receptionistName + ". Staff queue " + EffectiveQueueCapacity() +
            "/" + _customerQueueCapacity + "; wage $" + (_receptionistWage + _receptionistWageBonus) + "/period.");
    }

    private void ExecuteStaffRosterSelection()
    {
        if (_selection == 3) { NavigateBack(); return; }
        if (_selection >= _mechanics.Count) { Notify("No mechanic hired for that slot."); return; }
        _editingStaffSlot = _selection;
        _menuPage = MenuPage.StaffProfile;
        _selection = 0;
    }

    private void ExecuteStaffProfileSelection()
    {
        if (_selection == 5) { NavigateBack(); return; }
        if (_editingStaffSlot < 0 || _editingStaffSlot >= _mechanics.Count) { NavigateBack(); return; }
        Mechanic m = _mechanics[_editingStaffSlot];
        switch (_selection)
        {
            case 0:
                ShowStatusNotice("MECHANIC PROFILE",
                    m.Name + " | " + RankName(m) + " " + SpecialtyName(m.Specialty),
                    "Level " + MechanicLevel(m) + " | XP " + m.Xp +
                        " | Jobs completed " + m.JobsCompleted,
                    "Wage $" + m.UnpaidWages.ToString("N0") + " " +
                        PayrollDueText(m.WageUnpaidSeconds, m.UnpaidWages) +
                        " | Morale " + PayrollMorale(m.WageUnpaidSeconds, m.UnpaidWages),
                    "Assigned job " + (m.HasJob ? "YES" : "NO") +
                        " | Ped " + (m.Worker != null && m.Worker.Exists() ? "SPAWNED" : "OFFSCREEN"));
                break;
            case 1:
                if (_cash < TrainingCost) { Notify("Business cannot afford training ($750)."); break; }
                long billedTraining = ManagerAdjustedExpense(TrainingCost);
                _cash -= billedTraining;
                _trainingSpend += billedTraining;
                // Classroom XP is not counted as a completed customer order.
                m.Xp = Math.Min(100000, m.Xp + 40);
                Save();
                Notify(m.Name + " trained: +40 XP. Level " + MechanicLevel(m) + ".");
                break;
            case 2:
                if (MechanicLevel(m) < 2) { Notify("Specialization requires mechanic level 2."); break; }
                if (_cash < SpecialistTrainingCost) { Notify("Specialist certification costs $350."); break; }
                if (m.HasJob) { Notify("Cannot change specialty while assigned to a job."); break; }
                long billedSpecialistTraining = ManagerAdjustedExpense(SpecialistTrainingCost);
                _cash -= billedSpecialistTraining;
                _trainingSpend += billedSpecialistTraining;
                m.Specialty = (m.Specialty + 1) % 3;
                Save();
                Notify(m.Name + " now works as " + SpecialtyName(m.Specialty) + ".");
                break;
            case 3:
                // Raises persist, increase each payroll period, and cannot be reversed for free.
                if (m.WageBonus >= 1000) { Notify("Maximum wage bonus reached."); break; }
                m.WageBonus += 50;
                Save();
                Notify(m.Name + " wage is now $" + MechanicWage(m) + " per payroll period.");
                break;
            case 4: FireSelectedMechanic(); break;
        }
    }

    private void ExecuteEmployeeSettingsSelection()
    {
        switch (_selection)
        {
            case 0: _menuPage = MenuPage.EmployeePlacements; _selection = 0; break;
            case 1: _menuPage = MenuPage.EmployeePaths; _selection = 0; break;
            case 2: _menuPage = MenuPage.RouteRecovery; _selection = 0; break;
            case 3: _menuPage = MenuPage.PlacementMarkers; _selection = 0; break;
            case 4:
                _breakAnimationsEnabled = !_breakAnimationsEnabled;
                foreach (Mechanic m in _mechanics)
                {
                    m.WorkMode = -1;
                    m.BreakAnimationSeconds = 0;
                }
                Save();
                Notify("Break-time animations " + (_breakAnimationsEnabled ? "ON" : "OFF") + ".");
                break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteEmployeePlacementsSelection()
    {
        switch (_selection)
        {
            case 0: OpenPointEditor(_breakArea, "Staff Break Area", -1, false); break;
            case 1: _menuPage = MenuPage.MechanicBays; _selection = 0; break;
            case 2: OpenPointEditor(_receptionDesk, "Receptionist Desk", -1, false); break;
            case 3: OpenPointEditor(_receptionApproach, "Receptionist Desk Approach", -1, true); break;
            case 4: OpenPointEditor(_managerWorkstation, "Manager Workstation", -1, true); break;
            case 5: OpenPointEditor(_managerApproach, "Manager Final Workstation Approach", -1, true); break;
            case 6: OpenPointEditor(_employeeEntrance, "Staff Spawn / Entry", -1, false); break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteEmployeePathsSelection()
    {
        if (_selection == 0) { _mechanicPathsFromManagement = false; _menuPage = MenuPage.MechanicWalkingPaths; _selection = 0; }
        else if (_selection == 1) { _receptionPathsFromManagement = false; _menuPage = MenuPage.ReceptionWalkingPaths; _selection = 0; }
        else if (_selection == 2) { _managerPathsFromManagement = false; _menuPage = MenuPage.ManagerWalkingPaths; _selection = 0; }
        else NavigateBack();
    }

    private void ExecuteMechanicWalkingSelection()
    {
        switch (_selection)
        {
            case 0:
                if (_clockedIn) { Notify("Close shop before changing mechanic walking mode."); break; }
                _customEmployeeWalkingPaths = !_customEmployeeWalkingPaths;
                Save();
                Notify("Mechanic walking: " + (_customEmployeeWalkingPaths ?
                    "CUSTOM MLO (0-5 points)" : "AUTO DIRECT") +
                    ". Car pickup exit routing is separately configurable.");
                break;
            case 1:
                if (_customerOrder != null && _customerOrder.VehicleHandoff)
                { Notify("Change pickup routing after the current vehicle transfer."); break; }
                _pickupUsesArrivalExit = !_pickupUsesArrivalExit;
                Save();
                Notify("Car pickup exit route: " + (_pickupUsesArrivalExit ? "REVERSE SPAWN-BREAK" : "DIRECT") + ".");
                break;
            case 2: _menuPage = MenuPage.ArrivalWaypoints; _selection = 0; break;
            case 3: _menuPage = MenuPage.ServiceWaypoints; _selection = 0; break;
            case 4: _menuPage = MenuPage.BayApproaches; _selection = 0; break;
            case 5: OpenPointEditor(_walkCheckpoint, "Legacy Mechanic Arrival Checkpoint", -1, true); break;
            case 6: _menuPage = MenuPage.MechanicRouteRoster; _selection = 0; break;
            case 7: _smoothMechanicWaypoints = !_smoothMechanicWaypoints; Save();
                Notify("Smooth mechanic route transitions " + (_smoothMechanicWaypoints ? "ON" : "OFF") + "."); break;
            case 8: _safeTeleportOnStall = !_safeTeleportOnStall; Save();
                Notify("Safe stuck-ped teleport " + (_safeTeleportOnStall ? "ON" : "OFF") + "."); break;
            case 9:
                if (_clockedIn) { Notify("Close shop before changing job routes."); break; }
                _customMechanicJobRoutes = !_customMechanicJobRoutes;
                Save();
                Notify("Break / Bay routes: " + (_customMechanicJobRoutes ?
                    "SAVED WAYPOINTS" : "DIRECT") + ". Existing points and masks are preserved.");
                break;
            default: NavigateBack(); break;
        }
    }

    // Per-mechanic route masks select existing saved points; no new markers.
    // They are snapshot when the next route starts, so toggling mid-job cannot
    // steal or cancel a live customer's order.
    private static int CountMask(int mask)
    {
        int result = 0;
        for (int i = 0; i < 5; i++) if ((mask & (1 << i)) != 0) result++;
        return result;
    }

    private void ExecuteRouteRosterSelection()
    {
        if (_selection >= 3) { NavigateBack(); return; }
        if (_selection >= _mechanics.Count) { Notify("Hire that mechanic before configuring their route."); return; }
        _routeConfigSlot = _selection;
        _menuPage = MenuPage.MechanicRouteDirection;
        _selection = 0;
    }

    private void ExecuteRouteDirectionSelection()
    {
        if (_selection == 3) { NavigateBack(); return; }
        if (_routeConfigSlot >= _mechanics.Count) { NavigateBack(); return; }
        Mechanic staff = _mechanics[_routeConfigSlot];
        if (_selection == 2)
        {
            ShowStatusNotice("MECHANIC JOB ROUTE", staff.Name,
                "To job: " + CountMask(staff.JobOutboundMask) + "/5 enabled",
                "From job: " + CountMask(staff.JobReturnMask) + "/5 enabled",
                "Active only in CUSTOM MLO mode; new routes only.");
            return;
        }
        _routeConfigReturning = _selection == 1;
        _menuPage = MenuPage.MechanicRoutePoints;
        _selection = 0;
    }

    private void ExecuteRoutePointSelection()
    {
        if (_selection == 7) { NavigateBack(); return; }
        if (_routeConfigSlot >= _mechanics.Count) { NavigateBack(); return; }
        Mechanic staff = _mechanics[_routeConfigSlot];
        int mask = _routeConfigReturning ? staff.JobReturnMask : staff.JobOutboundMask;
        if (_selection < 5) mask ^= 1 << _selection;
        else mask = _selection == 5 ? 31 : 0;
        mask &= 31;
        if (_routeConfigReturning) staff.JobReturnMask = mask;
        else staff.JobOutboundMask = mask;
        Save();
        GTA.UI.Screen.ShowSubtitle(staff.Name + ": " + (_routeConfigReturning ? "return" : "job") +
            " route uses " + CountMask(mask) + "/5 points.", 1800);
    }

    private void ExecuteReceptionWalkingSelection()
    {
        switch (_selection)
        {
            case 0:
                if (_clockedIn) { Notify("Close shop before changing receptionist walking mode."); break; }
                _customReceptionWalkingPaths = !_customReceptionWalkingPaths;
                Save();
                Notify("Receptionist walking: " + (_customReceptionWalkingPaths ?
                    "CUSTOM MLO (0-5 points)" : "AUTO DIRECT") + ".");
                break;
            case 1: _menuPage = MenuPage.ReceptionWaypoints; _selection = 0; break;
            case 2: OpenPointEditor(_receptionApproach, "Receptionist Final Desk Approach", -1, true); break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteManagerWalkingSelection()
    {
        switch (_selection)
        {
            case 0:
                if (_clockedIn) { Notify("Close shop before changing manager walking mode."); return; }
                _customManagerWalkingPaths = !_customManagerWalkingPaths;
                Save(); Notify("Manager walking: " + (_customManagerWalkingPaths ? "CUSTOM MLO" : "AUTO DIRECT") + ".");
                break;
            case 1: _menuPage = MenuPage.ManagerWaypoints; _selection = 0; break;
            case 2: OpenPointEditor(_managerApproach, "Manager Final Workstation Approach", -1, true); break;
            default: NavigateBack(); break;
        }
    }

    private void ExecutePointListSelection()
    {
        int maxSlots = (_menuPage == MenuPage.ArrivalWaypoints ||
            _menuPage == MenuPage.ServiceWaypoints ||
            _menuPage == MenuPage.ReceptionWaypoints ||
            _menuPage == MenuPage.ManagerWaypoints) ? OptionalPathPointCount : VisualBayCount;
        if (_selection >= maxSlots) { NavigateBack(); return; }
        int slot = _selection;
        if (_menuPage == MenuPage.MechanicBays)
            OpenPointEditor(_bays[slot], "Mechanic Bay " + (slot + 1), slot, false);
        else if (_menuPage == MenuPage.ServiceVehicleBays)
            OpenPointEditor(_vehicleParkingBays[slot], "Vehicle Parking Bay " + (slot + 1), -1, false);
        else if (_menuPage == MenuPage.ArrivalWaypoints)
            OpenPointEditor(_arrivalRoute[slot], "Arrival: Stop " + (slot + 1) + "/5 (optional)", -1, true);
        else if (_menuPage == MenuPage.ServiceWaypoints)
            OpenPointEditor(_serviceRoute[slot], "Work Bay: Stop " + (slot + 1) + "/5 (optional)", -1, true);
        else if (_menuPage == MenuPage.ReceptionWaypoints)
            OpenPointEditor(_receptionRoute[slot], "Reception: Stop " + (slot + 1) + "/5 (optional)", -1, true);
        else if (_menuPage == MenuPage.ManagerWaypoints)
            OpenPointEditor(_managerRoute[slot], "Manager: Stop " + (slot + 1) + "/5 (optional)", -1, true);
        else if (_menuPage == MenuPage.BayApproaches)
            OpenPointEditor(_bayApproaches[slot], "Bay " + (slot + 1) + " Approach", -1, true);
    }

    private void ExecutePointEditorSelection()
    {
        if (_editingPoint == null) { NavigateBack(); return; }
        int vehicleBaySlot = EditingVehicleParkingSlot();
        switch (_selection)
        {
            case 0:
                if (vehicleBaySlot >= 0) ConfigureVehicleParkingBay(vehicleBaySlot);
                else if (_editingBayIndex >= 0) ConfigureWorkBay(_editingBayIndex);
                else if (_editingRoutePoint) ConfigureRoutePoint(_editingPoint, _editingPointLabel);
                else ConfigureArrivalPoint(_editingPoint, _editingPointLabel);
                if (_editingPoint.IsSet)
                    _showPlacementMarkers = true; // SET always enables saved-point preview
                // Pallet markers/boxes follow edited stockroom positions immediately.
                if (Object.ReferenceEquals(_editingPoint, _supplierPartsPallet) ||
                    Array.IndexOf(_extraStoragePallets, _editingPoint) >= 0)
                    DeleteStoragePalletVisuals();
                // Clearing and changing a desk destroys our OWN receptionist only.
                if (!_clockedIn &&
                    (_editingPoint == _receptionDesk || _editingPoint == _receptionApproach))
                    DismissReceptionist();
                break;
            case 1:
                if (vehicleBaySlot >= 0 && _customerOrder != null)
                { Notify("Finish the current customer order before clearing vehicle parking."); break; }
                if (vehicleBaySlot < 0 && _clockedIn)
                { Notify("Close the shop before clearing employee points."); break; }
                if (!_editingPoint.IsSet) { Notify(_editingPointLabel + " is already clear."); break; }
                if (_editingBayIndex >= 0) DismissWorkerForSlot(_editingBayIndex);
                if (_editingPoint == _receptionDesk || _editingPoint == _receptionApproach)
                    DismissReceptionist();
                _editingPoint.IsSet = false;
                if (Object.ReferenceEquals(_editingPoint, _supplierPartsPallet) ||
                    Array.IndexOf(_extraStoragePallets, _editingPoint) >= 0)
                    DeleteStoragePalletVisuals();
                Save();
                Notify(_editingPointLabel + " CLEARED.");
                break;
            case 2:
                _showPlacementMarkers = !_showPlacementMarkers;
                Notify("Saved placement markers " + (_showPlacementMarkers ? "VISIBLE" : "HIDDEN") + ".");
                break;
            case 3:
                Notify(_editingPointLabel + ": " + (_editingPoint.IsSet ?
                    FormatPosition(_editingPoint.Position) + " | heading " +
                    _editingPoint.Heading.ToString("0") : "NOT SET"));
                break;
            case 4: NavigateBack(); break;
        }
    }

    private void ExecuteRecoverySelection()
    {
        switch (_selection)
        {
            case 0:
                _routeRecoveryEnabled = !_routeRecoveryEnabled;
                Save();
                Notify("Blocked-route teleport recovery " + (_routeRecoveryEnabled ? "ON" : "OFF") + ".");
                break;
            case 1:
                ShowStatusNotice("PATH RECOVERY STATUS",
                    "Teleport recovery " + (_routeRecoveryEnabled ? "ON" : "OFF"),
                    "No-progress timer " + _stallSeconds + " seconds",
                    "Max saved-point teleports " + _recoveryLimit + " per route leg",
                    "Manual MLO routes can prevent wall collisions.");
                break;
            case 2: NavigateBack(); break;
        }
    }

    private void ExecuteMarkerSelection()
    {
        switch (_selection)
        {
            case 0:
                _showPlacementMarkers = !_showPlacementMarkers;
                Notify(_showPlacementMarkers && !_placed ? "Set the shop location first: no placement points can be previewed yet." :
                    "Placement markers " + (_showPlacementMarkers ? "ON" : "OFF") +
                    (_showPlacementMarkers ? ". Visible within 80m of your shop when points are SET." : "."));
                break;
            case 1:
                Notify("BLUE: mechanic bays | TEAL: customer vehicle parking | GREEN: break" +
                    " | PINK: reception | ORANGE: arrival stops | YELLOW: work stops.");
                break;
            case 2: NavigateBack(); break;
        }
    }

    private long TotalMechanicPayroll()
    {
        long amount = 0;
        foreach (Mechanic m in _mechanics) amount += MechanicWage(m);
        return amount;
    }

    // GTA's feed notifications support ~n~ for separate lines. Keep messages
    // concise and show them only on explicit user requests, not per game tick.
    private static void ShowStatusNotice(string heading, params string[] lines)
    {
        Notify("~y~" + heading + "~s~~n~" + string.Join("~n~", lines));
    }

    private int StagedCount()
    {
        return (_stagedCustomerOrder == null ? 0 : 1) +
            (_stagedThirdCustomerOrder == null ? 0 : 1);
    }

    private void ShowVehicleDrivingStatus()
    {
        ShowStatusNotice("VEHICLE DRIVING",
            "Drive-in " + (_driveInEnabled ? "ON" : "OFF") + " | Valet " + (_vehicleTransfersEnabled ? "ON" : "OFF") +
                " | Reverse " + (_valetReverseOut ? "ON" : "OFF"),
            "Parking " + (_automaticLotMode ? "AUTO LOT" : "CUSTOM MLO") +
                " | Car bays " + CountSet(_vehicleParkingBays) + "/3",
            "Auto lot " + (_autoLotRightSide ? "RIGHT" : "LEFT") + " " +
                _autoLotOffsetMeters.ToString("0") + "m | Road spawn " + Status(_customerDriveSpawn),
            "Active " + _activeCustomerOrders.Count + " | Roadside " + StagedCount() + " | Drop-off " +
                (DropoffOccupied(null) ? "OCCUPIED" : "CLEAR"));
    }

    private void ShowEmployeeStatus()
    {
        int waiting = 0, assigned = 0, working = 0, visible = 0;
        foreach (Mechanic employee in _mechanics)
        {
            if (employee.HasJob) assigned++; else waiting++;
            if (employee.ArrivalPhase == PhaseWorking) working++;
            if (employee.Worker != null && employee.Worker.Exists()) visible++;
        }
        ShowStatusNotice("EMPLOYEE STATUS",
            "Mechanics " + _mechanics.Count + " | Visible " + visible + " | Idle " + waiting,
            "Assigned " + assigned + " | Working " + working,
            "Reception " + (_receptionistHired ? (_receptionist.AtDesk ? "AT DESK" : "ON DUTY / EN ROUTE") : "NOT HIRED"),
            "Walk " + (_customEmployeeWalkingPaths ? "CUSTOM MLO" : "DIRECT") +
                " | Return via entrance " + (_pickupUsesArrivalExit ? "ON" : "OFF"));
    }

    private void ShowShopStatus()
    {
        int visible = 0;
        foreach (Mechanic mechanic in _mechanics)
            if (mechanic.Worker != null && mechanic.Worker.Exists()) visible++;
        ShowStatusNotice("SHOP STATUS",
            "Business " + (_clockedIn ? "OPEN" : "CLOSED") + " | Saved location " + StatusShopLocation(),
            "Staff " + _mechanics.Count + " (" + visible + " visible) | Bays " + CountConfiguredBays() + "/3",
            "Reception " + (_receptionistHired && _receptionist.AtDesk ? "READY" : "UNAVAILABLE") +
                " | Parts " + _parts,
            "Manager " + (_managerTier < 0 ? "NONE" : _managerName) +
                " | " + (_remoteMode ? "REMOTE LIVE" : "ONSITE") +
                " | Offsite jobs " + _remoteJobsCompleted,
            "Customer " + (_customerOrder == null ? "NONE" : _customerOrder.Stage.ToString()) +
                " | Save " + (File.Exists(SaveFile) ? "FOUND" : "MISSING"));
    }

    private string StatusShopLocation() { return _placed ? "SET" : "NOT SET"; }

    private int CountConfiguredBays()
    {
        int count = 0;
        foreach (WorkBay bay in _bays) if (bay.IsSet) count++;
        return count;
    }

    private void ConfigureWorkBay(int index)
    {
        if (!_placed) { Notify("Set the shop location before configuring a mechanic bay."); return; }
        if (Game.Player.Character == null) return;
        Vector3 here = Game.Player.Character.Position;
        if (DistanceSquared(here, _location) > 2500f)
        {
            Notify("Work bays must be within 50 meters of the saved shop location.");
            return;
        }
        // Player stands exactly where the worker should appear, facing the right direction.
        DismissWorkerForSlot(index);
        _bays[index].IsSet = true;
        _bays[index].Position = here;
        _bays[index].Heading = Game.Player.Character.Heading;
        Save();
        _visualRefreshClock = 1.0;
        _showPlacementMarkers = true;
        Notify("Mechanic Bay " + (index + 1) + " SAVED at " + FormatPosition(here) + ". Markers ON.");
    }

    // Customer vehicle spots are NOT employee arrival routes. Capturing them
    // during an open shift is safe only if no live order has a destination in use.
    // Use player heading to set the car's final intended orientation.
    private void ConfigureVehicleParkingBay(int index)
    {
        if (index < 0 || index >= VisualBayCount) return;
        if (!_placed) { Notify("Set the shop location before saving a vehicle bay."); return; }
        if (_customerOrder != null)
        {
            Notify("Vehicle bay not changed: finish the current customer order first.");
            return;
        }
        Ped player = Game.Player.Character;
        if (player == null) { Notify("Player position unavailable; try again in gameplay."); return; }
        Vector3 position = player.Position;
        if (DistanceSquared(position, _location) > 6400f)
        {
            Notify("Vehicle parking must be within 80 meters of the shop.");
            return;
        }
        WorkBay bay = _vehicleParkingBays[index];
        bay.IsSet = true;
        bay.Position = position;
        bay.Heading = player.Heading;
        _showPlacementMarkers = true;
        Save();
        Notify("Vehicle Bay " + (index + 1) + " parking SAVED: " +
            FormatPosition(position) + " | heading " + bay.Heading.ToString("0") +
            ". Teal marker now visible.");
    }

    // Configure while shop is closed. The checkpoint is optional, but can guide
    // workers around one corner/doorway in a custom MLO with no indoor navmesh.
    private void ConfigureArrivalPoint(WorkBay point, string name)
    {
        if (!_placed) { Notify("Set the shop location first."); return; }
        if (_clockedIn) { Notify("Close your shop before changing employee arrival routes."); return; }
        if (Game.Player.Character == null) return;
        Vector3 here = Game.Player.Character.Position;
        if (DistanceSquared(here, _location) > 6400f)
        {
            Notify("Arrival locations must be within 80 meters of your shop.");
            return;
        }
        if (point == _walkCheckpoint && point.IsSet && DistanceSquared(point.Position, here) < 2.25f)
        {
            point.IsSet = false;
            Save();
            Notify("Legacy Walk Checkpoint CLEARED; direct mode ignores it anyway.");
            return;
        }
        if (point == _employeeEntrance || point == _breakArea || point == _receptionDesk)
        {
            float minimum = point == _breakArea || point == _receptionDesk ? 1.2f : 6.0f;
            foreach (WorkBay bay in _bays)
            {
                if (bay.IsSet && DistanceSquared(bay.Position, here) < minimum * minimum)
                {
                    Notify(name + " too close to a mechanic bay. Move at least " + minimum + " meters away.");
                    return;
                }
            }
        }
        point.IsSet = true;
        point.Position = here;
        point.Heading = Game.Player.Character.Heading;
        Save();
        _warnedMissingEntrance = false;
        _showPlacementMarkers = true;
        Notify(name + " SAVED at " + FormatPosition(here) + ". Markers ON.");
    }

    // Optional route markers are captured at the player's standing position.
    // Re-select within 1.5m of the previous point to clear it. Configure while closed.
    private void ConfigureRoutePoint(WorkBay point, string label)
    {
        if (!_placed) { Notify("Set your shop location first."); return; }
        if (_clockedIn) { Notify("Close the shop before editing employee paths."); return; }
        if (Game.Player.Character == null) return;
        Vector3 here = Game.Player.Character.Position;
        if (DistanceSquared(here, _location) > 6400f)
        { Notify("Route markers must be within 80m of the shop."); return; }
        if (point.IsSet && DistanceSquared(here, point.Position) < 2.25f)
        {
            point.IsSet = false;
            Save();
            Notify(label + " CLEARED.");
            return;
        }
        point.IsSet = true;
        point.Position = here;
        point.Heading = Game.Player.Character.Heading;
        Save();
        _showPlacementMarkers = true;
        Notify(label + " SAVED: " + FormatPosition(here) + ". Markers ON.");
    }

    // The previews are ephemeral, drawn per-frame only when enabled.
    // They are NOT GTA objects and never persist in the save file.
    private void DrawPlacementMarkers()
    {
        if (!_placed || Game.Player.Character == null || Game.IsPaused) return;
        if (DistanceSquared(Game.Player.Character.Position, _location) > 6400f) return;
        Color blue = Color.FromArgb(190, 58, 151, 245);
        Color green = Color.FromArgb(190, 65, 216, 137);
        Color pink = Color.FromArgb(190, 235, 95, 188);
        Color orange = Color.FromArgb(190, 254, 161, 60);
        Color yellow = Color.FromArgb(185, 255, 202, 75);
        if (_showPlacementMarkers)
        {
            MarkPoint(_breakArea, green);
            for (int i = 0; i < VisualBayCount; i++) MarkPoint(_bays[i], blue);
            MarkPoint(_employeeEntrance, orange);
            // Avoid showing legacy MLO-only route markers on the outdoor AUTO DIRECT mode.
            if (_customEmployeeWalkingPaths)
            {
                MarkPoint(_walkCheckpoint, yellow);
                foreach (WorkBay point in _arrivalRoute)
                    MarkPoint(point, Color.FromArgb(185, 255, 155, 60));
                foreach (WorkBay point in _serviceRoute) MarkPoint(point, yellow);
                foreach (WorkBay point in _bayApproaches) MarkPoint(point, yellow);
            }
            if (_customReceptionWalkingPaths)
                foreach (WorkBay stop in _receptionRoute)
                    MarkPoint(stop, Color.FromArgb(185, 180, 135, 248));
            if (_customManagerWalkingPaths)
                foreach (WorkBay stop in _managerRoute)
                    MarkPoint(stop, Color.FromArgb(195, 92, 195, 255));
            MarkPoint(_managerApproach, Color.FromArgb(195, 115, 210, 248));
            MarkPoint(_receptionApproach, Color.FromArgb(190, 172, 110, 239));
            MarkPoint(_receptionDesk, pink);
            MarkPoint(_managerWorkstation, Color.FromArgb(195, 129, 195, 255));
            MarkPoint(_customerDriveSpawn, Color.FromArgb(220, 115, 208, 255));
            MarkParkingSquare(_customerDropoff, Color.FromArgb(215, 87, 210, 245));
            foreach (WorkBay point in _vehicleParkingBays)
                MarkParkingSquare(point, Color.FromArgb(210, 26, 225, 191));
            MarkPoint(_customerApproach, Color.FromArgb(200, 196, 156, 255));
            MarkPoint(_customerQueue, Color.FromArgb(200, 255, 135, 75));
            foreach (WorkBay point in _customerWalkingRoute)
                MarkPoint(point, Color.FromArgb(190, 235, 126, 236));
            MarkParkingSquare(_queueCar2, Color.FromArgb(220, 252, 185, 69));
            MarkParkingSquare(_queueCar3, Color.FromArgb(220, 245, 120, 54));
            MarkPoint(_supplierTruckSpawn, Color.FromArgb(210, 122, 210, 100));
            MarkPoint(_supplierTruckPark, Color.FromArgb(210, 222, 170, 50));
            MarkPoint(_supplierHandoff, Color.FromArgb(210, 185, 240, 70));
            MarkPoint(_supplierPartsPallet, Color.FromArgb(210, 110, 235, 120));
            foreach (WorkBay deliveryNode in _supplierDriverWaypoints)
                MarkPoint(deliveryNode, Color.FromArgb(195, 150, 220, 85));
        }
        // Preview is independent: works even with all normal placement markers OFF.
        if (_autoLotPreview && _automaticLotMode)
            for (int i = 0; i < VisualBayCount; i++)
                if (_bays[i].IsSet) MarkParkingSquare(ComputeAutomaticBay(i), Color.FromArgb(220, 110, 250, 180));
        // Emphasize the location currently being edited so similarly colored
        // bays/waypoints remain identifiable within the garage.
        if (_showMenu && _menuPage == MenuPage.PointEditor &&
            _editingPoint != null && _editingPoint.IsSet)
        {
            Vector3 location = _editingPoint.Position;
            World.DrawMarker((MarkerType)1,
                new Vector3(location.X, location.Y, location.Z - 0.025f),
                new Vector3(), new Vector3(), new Vector3(0.70f, 0.70f, 0.045f),
                Color.FromArgb(150, 255, 255, 255));
        }
    }

    private static Vector3[] ParkingOutlineVertices(Vector3 position, float heading)
    {
        // SHVDN/GTA: forward at 0 degrees is +Y, at 90 degrees is -X.
        // This is the inverse of Vector3.ToHeading, not a mirrored +X rotation.
        float radians = heading * (float)Math.PI / 180f;
        float sin = (float)Math.Sin(radians), cos = (float)Math.Cos(radians);
        Vector3[] points = new Vector3[7];
        for (int i = 0; i < points.Length; i++)
        {
            float length = i < 4 ? (i < 2 ? -2.55f : 2.55f) : (i == 4 ? 2.15f : 0.85f);
            float width = i < 4 ? (i == 0 || i == 3 ? -1.18f : 1.18f) :
                (i == 4 ? 0f : (i == 5 ? -0.60f : 0.60f));
            points[i] = new Vector3(position.X - length * sin + width * cos,
                position.Y + length * cos + width * sin, position.Z + 0.025f);
        }
        return points;
    }

    private static void MarkParkingSquare(WorkBay point, Color color)
    {
        if (point == null || !point.IsSet) return;
        Vector3[] points = ParkingOutlineVertices(point.Position, point.Heading);
        // Four outline edges, then two arms of a clear forward-facing arrow.
        // Existing placement-preview distance/toggles remain in the caller.
        for (int i = 0; i < 6; i++)
        {
            Vector3 a = i < 4 ? points[i] : points[4];
            Vector3 b = i < 4 ? points[(i + 1) % 4] : points[i + 1];
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE,
                a.X, a.Y, a.Z, b.X, b.Y, b.Z, (int)color.R, (int)color.G, (int)color.B, (int)color.A);
        }
    }

    private static void MarkPoint(WorkBay point, Color color)
    {
        if (point == null || !point.IsSet) return;
        Vector3 basePos = new Vector3(point.Position.X, point.Position.Y,
            point.Position.Z + 0.06f);
        World.DrawMarker((MarkerType)1, basePos, new Vector3(), new Vector3(),
            new Vector3(0.48f, 0.48f, 0.16f), color);
        // Heading remains saved for parking and ped facing, but debug previews
        // intentionally show ONLY the position circle to avoid visual clutter.
    }

    private static void LoadPoint(ScriptSettings save, string section, WorkBay point)
    {
        point.IsSet = save.GetValue(section, "Set", false);
        point.Position = new Vector3(
            save.GetValue(section, "X", 0.0f),
            save.GetValue(section, "Y", 0.0f),
            save.GetValue(section, "Z", 0.0f));
        point.Heading = save.GetValue(section, "Heading", 0.0f);
    }

    private static void SavePoint(ScriptSettings save, string section, WorkBay point)
    {
        save.SetValue(section, "Set", point.IsSet);
        save.SetValue(section, "X", point.Position.X);
        save.SetValue(section, "Y", point.Position.Y);
        save.SetValue(section, "Z", point.Position.Z);
        save.SetValue(section, "Heading", point.Heading);
    }

    private static float DistanceSquared(Vector3 a, Vector3 b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        float dz = a.Z - b.Z;
        return dx * dx + dy * dy + dz * dz;
    }

    private string FormatLocation()
    {
        return String.Format(System.Globalization.CultureInfo.InvariantCulture,
            "X={0:0.00}, Y={1:0.00}, Z={2:0.00}", _location.X, _location.Y, _location.Z);
    }

    private static string FormatPosition(Vector3 point)
    {
        return String.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0:0.00}, {1:0.00}, {2:0.00}", point.X, point.Y, point.Z);
    }

    // Workers are presentation entities, never the source of truth for payroll.
    // Route: off-camera entrance -> optional checkpoint -> break area -> bay
    // (only for assigned jobs) -> break area again. If blocked and recovery is
    // enabled, move only to the next USER-SAVED waypoint, never an invented point.
    private void UpdateVisualMechanics()
    {
        if (!_clockedIn || !_placed || Game.Player.Character == null ||
            DistanceSquared(Game.Player.Character.Position, _location) > _spawnDistance * _spawnDistance)
        {
            if (_clockedIn && CaptureStaffPosts()) Save();
            CleanupVisualMechanics();
            _workersPreviouslyInRange = false;
            return;
        }
        if (!_workersPreviouslyInRange)
        {
            _workersPreviouslyInRange = true;
            // Also stagger shift arrivals if the player returns from outside range.
            for (int i = 0; i < _mechanics.Count; i++)
                _mechanics[i].NextSpawnAttempt = DateTime.UtcNow.AddSeconds(i * _arrivalSpacingSeconds);
        }
        UpdateVisualManager(DateTime.UtcNow);
        if (!_employeeEntrance.IsSet)
        {
            CleanupVisualMechanics();
            if (!_warnedMissingEntrance)
            {
                Notify("Set Employee Entrance in " + MenuKeyLabel() + " > Employees > Employee Settings.");
                _warnedMissingEntrance = true;
            }
            return;
        }
        if (!_breakArea.IsSet)
        {
            // A receptionist can work independently of a mechanic break area.
            foreach (Mechanic m in _mechanics) DismissWorker(m);
            if (_mechanics.Count > 0 && !_warnedMissingBreakArea)
            {
                Notify("Set Break Area in " + MenuKeyLabel() + " > Employees > Employee Settings.");
                _warnedMissingBreakArea = true;
            }
            UpdateVisualReceptionist(DateTime.UtcNow);
            return;
        }
        if (!_mechanicModel.IsInCdImage || !_mechanicModel.IsPed)
        {
            if (!_warnedInvalidModel)
            {
                Log("Invalid mechanic model: " + _pedModelName, null);
                Notify("Invalid mechanic ped model in Settings.ini. Check Errors.log.");
                _warnedInvalidModel = true;
            }
            UpdateVisualReceptionist(DateTime.UtcNow);
            return;
        }

        bool needsVisualModel = false;
        for (int i = 0; i < _mechanics.Count && i < VisualBayCount; i++)
            if (_bays[i].IsSet) { needsVisualModel = true; break; }
        if (!needsVisualModel)
        {
            foreach (Mechanic m in _mechanics) DismissWorker(m);
            UpdateVisualReceptionist(DateTime.UtcNow);
            return;
        }
        if (!_mechanicModel.IsLoaded)
        {
            _mechanicModel.Request(); // nonblocking; check again next second
            UpdateVisualReceptionist(DateTime.UtcNow);
            return;
        }

        DateTime now = DateTime.UtcNow;
        // Receptionist is independent of bay count and repair job scheduler.
        UpdateVisualReceptionist(now);
        for (int i = 0; i < _mechanics.Count; i++)
        {
            Mechanic employee = _mechanics[i];
            if (i >= VisualBayCount || !_bays[i].IsSet)
            {
                DismissWorker(employee);
                continue;
            }
            if (employee.Worker != null && (!employee.Worker.Exists() || employee.Worker.IsDead))
            {
                DismissWorker(employee);
                employee.NextSpawnAttempt = now.AddSeconds(20);
            }
            if (employee.Worker == null && now >= employee.NextSpawnAttempt)
            {
                try
                {
                    // Wait for the player to look away from the entrance. Never
                    // deliberately create a ped in front of the active camera.
                    // Resume from their previous streamed post rather than replaying
                    // the entrance route every time the player returns.
                    bool resume = employee.SavedPost.IsSet && !employee.HasJob &&
                        DistanceSquared(employee.SavedPost.Position, _location) < 10000f;
                    Vector3 spawn = resume ? employee.SavedPost.Position : _employeeEntrance.Position;
                    float heading = resume ? employee.SavedPost.Heading : _employeeEntrance.Heading;
                    bool onCamera = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPHERE_VISIBLE,
                        spawn.X, spawn.Y, spawn.Z + 0.8f, 1.0f);
                    bool tooClose = DistanceSquared(Game.Player.Character.Position, spawn) < 100f;
                    if (onCamera || tooClose) continue;

                    employee.NextSpawnAttempt = now.AddSeconds(20);
                    Ped worker = World.CreatePed(_mechanicModel, spawn, heading);
                    if (worker == null || !worker.Exists())
                    {
                        Log("Failed to spawn employee from entrance for bay " + (i + 1), null);
                        continue;
                    }
                    worker.IsPersistent = true;
                    worker.BlockPermanentEvents = true;
                    worker.AlwaysKeepTask = true;
                    employee.Worker = worker;
                    employee.WorkMode = -1;
                    employee.ArrivalPhase = PhaseNone;
                    if (resume)
                    {
                        employee.SavedPost.IsSet = false; // consume only after spawn success
                        StartRouteLeg(employee, i, PhaseToBreak);
                    }
                    else StartRouteLeg(employee, i, _customEmployeeWalkingPaths &&
                        (CountSet(_arrivalRoute) > 0 || _walkCheckpoint.IsSet) ? PhaseToCheckpoint : PhaseToBreak);
                }
                catch (Exception ex)
                {
                    Log("Spawn arriving mechanic " + (i + 1), ex);
                    DismissWorker(employee);
                    employee.NextSpawnAttempt = now.AddSeconds(25);
                }
            }
            if (employee.Worker == null || !employee.Worker.Exists()) continue;
            try { UpdateEmployeeActivity(employee, i, now); }
            catch (Exception ex)
            {
                Log("Employee arrival path " + (i + 1), ex);
                DismissWorker(employee);
                employee.NextSpawnAttempt = now.AddSeconds(30);
            }
        }
    }

    private Vector3 GetBreakSpot(int slot)
    {
        // Give each worker a slightly different position around the user-defined
        // office marker, avoiding a stack of three NPCs on one coordinate.
        double radians = _breakArea.Heading * Math.PI / 180.0;
        float offset = (slot - 1) * _breakSpacing;
        return new Vector3(
            _breakArea.Position.X + (float)Math.Cos(radians) * offset,
            _breakArea.Position.Y + (float)Math.Sin(radians) * offset,
            _breakArea.Position.Z);
    }

    // AUTO DIRECT (default): employee walks from current location to the goal.
    // CUSTOM MLO: zero to five optional arrival or work-route points, in numeric order.
    // Unset gaps are skipped, so 0/5, 1/5, or 2/5 are all valid configurations.
    // The old single checkpoint is used only if no new arrival points are configured.
    private List<Vector3> BuildRoute(int slot, int phase)
    {
        List<Vector3> result = new List<Vector3>();
        if (phase == PhaseToCheckpoint)
        {
            if (_customEmployeeWalkingPaths)
            {
                if (CountSet(_arrivalRoute) > 0)
                {
                    foreach (WorkBay step in _arrivalRoute)
                        if (step.IsSet) result.Add(step.Position);
                }
                else if (_walkCheckpoint.IsSet) result.Add(_walkCheckpoint.Position);
            }
        }
        else if (phase == PhaseToBreak) result.Add(GetBreakSpot(slot));
        else if (phase == PhaseToBay)
        {
            if (_customMechanicJobRoutes)
            {
                int routeMask = slot < _mechanics.Count ? _mechanics[slot].JobOutboundMask : 31;
                for (int i = 0; i < _serviceRoute.Length; i++)
                    if (_serviceRoute[i].IsSet && (routeMask & (1 << i)) != 0)
                        result.Add(_serviceRoute[i].Position);
                if (_bayApproaches[slot].IsSet) result.Add(_bayApproaches[slot].Position);
            }
            result.Add(_bays[slot].Position);
        }
        else if (phase == PhaseReturningToBreak)
        {
            if (_customMechanicJobRoutes)
            {
                if (_bayApproaches[slot].IsSet) result.Add(_bayApproaches[slot].Position);
                int routeMask = slot < _mechanics.Count ? _mechanics[slot].JobReturnMask : 31;
                for (int i = _serviceRoute.Length - 1; i >= 0; i--)
                    if (_serviceRoute[i].IsSet && (routeMask & (1 << i)) != 0)
                        result.Add(_serviceRoute[i].Position);
            }
            result.Add(GetBreakSpot(slot));
        }
        return result;
    }

    private void IssueEmployeeWalkTask(Ped worker, Vector3 target, float heading, bool forceStraight = false, bool directJob = false)
    {
        if (worker == null || !worker.Exists()) return;
        if ((_customEmployeeWalkingPaths && !directJob) || forceStraight)
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_GO_STRAIGHT_TO_COORD,
                worker.Handle, target.X, target.Y, target.Z,
                _walkSpeed, -1, heading, 0.3f);
        else
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_FOLLOW_NAV_MESH_TO_COORD,
                worker.Handle, target.X, target.Y, target.Z,
                _walkSpeed, -1, 0.8f, 0, heading);
    }

    private void WalkCurrentNode(Mechanic employee, int slot, DateTime now)
    {
        if (employee.Worker == null || !employee.Worker.Exists()) return;
        if (employee.PathNodeIndex < 0 || employee.PathNodeIndex >= employee.PathNodes.Count)
            return;
        Vector3 target = employee.PathNodes[employee.PathNodeIndex];
        // Heading here is advisory. Workstation facing is set by the scenario task.
        float heading = employee.ArrivalPhase == PhaseToBay ? _bays[slot].Heading :
            employee.ArrivalPhase == PhaseToCheckpoint && _walkCheckpoint.IsSet ?
            _walkCheckpoint.Heading : _breakArea.Heading;
        GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, employee.Worker.Handle);
        IssueEmployeeWalkTask(employee.Worker, target, heading,
            employee.ReturningFromCustomerCar && _pickupUsesArrivalExit && CountSet(_arrivalRoute) > 0,
            !_customMechanicJobRoutes && (employee.ArrivalPhase == PhaseToBay || employee.ArrivalPhase == PhaseReturningToBreak));
        employee.RouteSince = now;
        employee.LastProgressAt = now;
        employee.LastDistanceSquared = DistanceSquared(employee.Worker.Position, target);
        employee.RouteRetries = 0;
        employee.WorkMode = -1;
    }

    private void StartRouteLeg(Mechanic employee, int slot, int phase)
    {
        employee.ArrivalPhase = phase;
        employee.ReturningFromCustomerCar = false;
        employee.PathNodes = BuildRoute(slot, phase);
        employee.PathNodeIndex = 0;
        employee.RouteTeleports = 0;
        if (employee.PathNodes.Count > 0) WalkCurrentNode(employee, slot, DateTime.UtcNow);
    }

    private bool RecoverBlockedWorker(Mechanic employee, int slot, Vector3 target, DateTime now)
    {
        if (!_routeRecoveryEnabled || employee.Worker == null || !employee.Worker.Exists() ||
            employee.RouteTeleports >= _recoveryLimit ||
            DistanceSquared(employee.Worker.Position, target) > 10000f)
            return false;
        try
        {
            // Use exactly the saved Z: custom MLO floors may not match ground queries.
            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, employee.Worker.Handle);
            employee.Worker.Position = target;
            employee.RouteTeleports++;
            employee.LastDistanceSquared = DistanceSquared(employee.Worker.Position, target);
            employee.LastProgressAt = now;
            employee.RouteSince = now;
            employee.RouteRetries = 0;
            Log("Mechanic " + (slot + 1) + " recovered at saved route waypoint " +
                (employee.PathNodeIndex + 1) + " " + FormatPosition(target), null);
            return true; // On next update the normal distance check advances the route.
        }
        catch (Exception ex) { Log("Mechanic route teleport recovery", ex); return false; }
    }

    private void PauseBlockedWorker(Mechanic employee, int slot, Vector3 target)
    {
        // Emergency visual recovery: route retry/teleport budget was exhausted.
        // Use the user's saved final destination without cancelling the repair.
        // Do NOT teleport when the destination overlaps the player.
        if (_safeTeleportOnStall && _routeRecoveryEnabled && employee.Worker != null &&
            employee.Worker.Exists())
        {
            Vector3 emergency = employee.HasJob && employee.ArrivalPhase == PhaseToBay &&
                slot < _bays.Length && _bays[slot].IsSet ? _bays[slot].Position : GetBreakSpot(slot);
            Ped player = Game.Player.Character;
            if ((_breakArea.IsSet || (employee.HasJob && slot < _bays.Length && _bays[slot].IsSet)) &&
                DistanceSquared(employee.Worker.Position, emergency) <= 10000f &&
                (player == null || !player.Exists() || DistanceSquared(player.Position, emergency) > 1.44f))
            {
                try
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, employee.Worker.Handle);
                    employee.Worker.Position = emergency;
                    employee.ArrivalPhase = employee.HasJob ? PhaseWorking : PhaseAtBreak;
                    employee.WorkMode = -1;
                    employee.PathNodes.Clear();
                    employee.PathNodeIndex = 0;
                    Log("Mechanic " + (slot + 1) + " emergency route recovery without cancelling job", null);
                    return;
                }
                catch (Exception ex) { Log("Emergency mechanic teleport", ex); }
            }
        }
        // Never keep endlessly shoving the NPC against a wall or respawn-loop.
        Log("Blocked employee " + (slot + 1) + " walking to " + FormatPosition(target) +
            " (route step " + (employee.PathNodeIndex + 1) + ")", null);
        // Keep HasJob and work clock intact. Route visuals may need correction,
        // but an unrelated customer repair must never be cancelled here.
        employee.ArrivalPhase = PhaseRouteBlocked;
        employee.WorkMode = -1;
        employee.PathNodes.Clear();
        if (employee.Worker != null && employee.Worker.Exists())
        {
            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, employee.Worker.Handle);
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_STAND_STILL, employee.Worker.Handle, -1);
        }
        Notify("Mechanic " + (slot + 1) + " path blocked. Check " + MenuKeyLabel() +
            " > Employees > Settings > Walking Paths; AUTO mode uses GTA navmesh.");
    }

    private void UpdateEmployeeActivity(Mechanic employee, int slot, DateTime now)
    {
        // Vehicle valet tasks own this ped until it exits the car and returns to a walking route.
        // Do not clear TASK_ENTER_VEHICLE or TASK_VEHICLE_DRIVE_TO_COORD from the employee tick.
        if (MechanicOnVehicleTask(slot)) return;
        // A path failure waits for manual correction instead of repeated spawn loops.
        if (employee.ArrivalPhase == PhaseRouteBlocked) return;

        if (employee.ArrivalPhase == PhaseAtBreak)
        {
            if (employee.HasJob) { StartRouteLeg(employee, slot, PhaseToBay); return; }
            employee.BreakAnimationSeconds += 1.0;
            if (employee.WorkMode != 0 || (_breakAnimationsEnabled && employee.BreakAnimationSeconds >= 55))
            {
                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, employee.Worker.Handle);
                if (_breakAnimationsEnabled)
                {
                    string scenario = _breakScenarios[(slot + employee.BreakAnimation) % _breakScenarios.Length];
                    GTA.Native.Function.Call(GTA.Native.Hash.TASK_START_SCENARIO_IN_PLACE,
                        employee.Worker.Handle, scenario, 0, true);
                }
                else
                    GTA.Native.Function.Call(GTA.Native.Hash.TASK_STAND_STILL, employee.Worker.Handle, -1);
                employee.BreakAnimation++;
                employee.BreakAnimationSeconds = 0;
                employee.WorkMode = 0;
            }
            return;
        }
        if (employee.ArrivalPhase == PhaseWorking)
        {
            if (!employee.HasJob) { StartRouteLeg(employee, slot, PhaseReturningToBreak); return; }
            if (employee.WorkMode != 1)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, employee.Worker.Handle);
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_START_SCENARIO_IN_PLACE,
                    employee.Worker.Handle, "WORLD_HUMAN_VEHICLE_MECHANIC", 0, true);
                employee.WorkMode = 1;
            }
            return;
        }
        if (employee.PathNodes == null || employee.PathNodes.Count == 0 ||
            employee.PathNodeIndex >= employee.PathNodes.Count) return;
        Vector3 target = employee.PathNodes[employee.PathNodeIndex];
        float distance2 = DistanceSquared(employee.Worker.Position, target);
        // Switch ~0.35m earlier at intermediate nodes. This avoids the stop-and-restart
        // at the exact waypoint, while the FINAL workstation/break goal keeps
        // the original 1.5m arrival accuracy.
        float approachRadiusSq = _smoothMechanicWaypoints &&
            employee.PathNodeIndex < employee.PathNodes.Count - 1 ? 3.61f : 2.25f;
        if (distance2 < approachRadiusSq)
        {
            employee.PathNodeIndex++;
            if (employee.PathNodeIndex < employee.PathNodes.Count)
            {
                WalkCurrentNode(employee, slot, now);
                return;
            }
            // All nodes in this leg reached. The next leg begins on next refresh.
            if (employee.ArrivalPhase == PhaseToCheckpoint)
                StartRouteLeg(employee, slot, PhaseToBreak);
            else if (employee.ArrivalPhase == PhaseToBay)
            {
                if (employee.HasJob) { employee.ArrivalPhase = PhaseWorking; employee.WorkMode = -1; }
                else StartRouteLeg(employee, slot, PhaseReturningToBreak);
            }
            else // PhaseToBreak and PhaseReturningToBreak
            {
                employee.ArrivalPhase = PhaseAtBreak;
                employee.ReturningFromCustomerCar = false;
                employee.WorkMode = -1;
            }
            return;
        }
        if (distance2 + 0.3f < employee.LastDistanceSquared)
        {
            employee.LastDistanceSquared = distance2;
            employee.LastProgressAt = now;
        }
        if ((now - employee.LastProgressAt).TotalSeconds >= _stallSeconds)
        {
            if (employee.RouteRetries >= 2 || (now - employee.RouteSince).TotalSeconds >= 50)
            {
                // Repeat a bounded, saved-coordinate teleport rather than
                // clearing the entire mechanic job when GTA MLO pathfinding fails.
                if (!_safeTeleportOnStall || !RecoverBlockedWorker(employee, slot, target, now))
                {
                    // On intermediate failures skip the unreachable marker and
                    // continue toward the next saved waypoint; NEVER reset HasJob.
                    if (employee.PathNodeIndex < employee.PathNodes.Count - 1)
                    {
                        employee.PathNodeIndex++;
                        WalkCurrentNode(employee, slot, now);
                    }
                    else PauseBlockedWorker(employee, slot, target);
                }
                return;
            }
            // Reissue the current node, NOT a direct route to the final bay.
            IssueEmployeeWalkTask(employee.Worker, target, employee.Worker.Heading,
                employee.ReturningFromCustomerCar && _pickupUsesArrivalExit && CountSet(_arrivalRoute) > 0,
            !_customMechanicJobRoutes && (employee.ArrivalPhase == PhaseToBay || employee.ArrivalPhase == PhaseReturningToBreak));
            employee.LastProgressAt = now;
            employee.RouteRetries++;
        }
    }

    // Receptionist uses the same off-camera entrance but walks to an independent desk.
    // They never take mechanic jobs or consume repair inventory.
    private void UpdateVisualReceptionist(DateTime now)
    {
        if (!_receptionistHired || !_receptionDesk.IsSet || !_employeeEntrance.IsSet)
        {
            DismissReceptionist();
            return;
        }
        if (!_receptionistModel.IsInCdImage || !_receptionistModel.IsPed)
        {
            if (!_warnedInvalidReceptionistModel)
            {
                Log("Invalid receptionist model: " + _receptionistModelName, null);
                Notify("Invalid receptionist ped model. Check Settings.ini.");
                _warnedInvalidReceptionistModel = true;
            }
            return;
        }
        if (!_receptionistModel.IsLoaded)
        {
            _receptionistModel.Request();
            return;
        }
        Receptionist staff = _receptionist;
        if (staff.Worker != null && (!staff.Worker.Exists() || staff.Worker.IsDead))
        {
            DismissReceptionist();
            staff.NextSpawnAttempt = now.AddSeconds(20);
        }
        if (staff.Worker == null && now >= staff.NextSpawnAttempt)
        {
            bool resume = staff.SavedPost.IsSet &&
                DistanceSquared(staff.SavedPost.Position, _location) < 10000f;
            Vector3 spawn = resume ? staff.SavedPost.Position : _employeeEntrance.Position;
            float heading = resume ? staff.SavedPost.Heading : _employeeEntrance.Heading;
            bool onCamera = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPHERE_VISIBLE,
                spawn.X, spawn.Y, spawn.Z + 0.8f, 1.0f);
            bool tooClose = DistanceSquared(Game.Player.Character.Position, spawn) < 100f;
            if (onCamera || tooClose) return;
            staff.NextSpawnAttempt = now.AddSeconds(25);
            try
            {
                Ped ped = World.CreatePed(_receptionistModel, spawn, heading);
                if (ped == null || !ped.Exists())
                {
                    Log("Receptionist failed to spawn", null);
                    return;
                }
                ped.IsPersistent = true;
                ped.BlockPermanentEvents = true;
                ped.AlwaysKeepTask = true;
                staff.Worker = ped;
                staff.AtDesk = false;
                staff.Blocked = false;
                staff.Route.Clear();
                if (_customReceptionWalkingPaths)
                {
                    if (CountSet(_receptionRoute) > 0)
                    {
                        foreach (WorkBay stop in _receptionRoute)
                            if (stop.IsSet) staff.Route.Add(stop.Position);
                    }
                    else if (_walkCheckpoint.IsSet) staff.Route.Add(_walkCheckpoint.Position);
                    if (_receptionApproach.IsSet) staff.Route.Add(_receptionApproach.Position);
                }
                staff.Route.Add(_receptionDesk.Position);
                if (resume)
                {
                    // Skip already travelled entrance waypoints; only return to desk.
                    staff.Route.Clear();
                    staff.Route.Add(_receptionDesk.Position);
                    staff.SavedPost.IsSet = false;
                }
                staff.NodeIndex = 0;
                staff.RouteTeleports = 0;
                WalkReceptionNode(now);
            }
            catch (Exception ex)
            {
                Log("Spawn receptionist", ex);
                DismissReceptionist();
                staff.NextSpawnAttempt = now.AddSeconds(25);
            }
        }
        if (staff.Worker == null || !staff.Worker.Exists() || staff.Blocked) return;
        try { UpdateReceptionRoute(now); }
        catch (Exception ex)
        {
            Log("Receptionist movement", ex);
            DismissReceptionist();
            staff.NextSpawnAttempt = now.AddSeconds(25);
        }
    }

    private void WalkReceptionNode(DateTime now)
    {
        Receptionist staff = _receptionist;
        if (staff.Worker == null || !staff.Worker.Exists() || staff.NodeIndex >= staff.Route.Count) return;
        Vector3 destination = staff.Route[staff.NodeIndex];
        GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, staff.Worker.Handle);
        if (_customReceptionWalkingPaths)
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_GO_STRAIGHT_TO_COORD,
                staff.Worker.Handle, destination.X, destination.Y, destination.Z,
                _walkSpeed, -1, _receptionDesk.Heading, 0.3f);
        else
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_FOLLOW_NAV_MESH_TO_COORD,
                staff.Worker.Handle, destination.X, destination.Y, destination.Z,
                _walkSpeed, -1, 0.3f, 0, _receptionDesk.Heading);
        staff.RouteSince = now;
        staff.LastProgressAt = now;
        staff.LastDistanceSquared = DistanceSquared(staff.Worker.Position, destination);
        staff.RouteRetries = 0;
    }

    private void UpdateReceptionRoute(DateTime now)
    {
        Receptionist staff = _receptionist;
        if (staff.AtDesk || staff.NodeIndex >= staff.Route.Count) return;
        Vector3 target = staff.Route[staff.NodeIndex];
        float distance2 = DistanceSquared(staff.Worker.Position, target);
        // Tight final-desk tolerance prevents treating the far side of a
        // counter as "arrived" when the receptionist is still 1.5m away.
        // A reachable approach point helps with doorways / desk corners.
        float arrivalTolerance2 = (staff.NodeIndex == staff.Route.Count - 1)
            ? 0.36f : 2.25f;
        if (distance2 < arrivalTolerance2)
        {
            staff.NodeIndex++;
            if (staff.NodeIndex < staff.Route.Count) WalkReceptionNode(now);
            else
            {
                staff.AtDesk = true;
                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, staff.Worker.Handle);
                // This native anchors the scenario to the exact saved desk point;
                // in-place tasks can leave the ped in front of the counter.
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_START_SCENARIO_AT_POSITION,
                    staff.Worker.Handle, "WORLD_HUMAN_CLIPBOARD",
                    _receptionDesk.Position.X, _receptionDesk.Position.Y, _receptionDesk.Position.Z,
                    _receptionDesk.Heading, -1, false, true);
            }
            return;
        }
        if (distance2 + 0.3f < staff.LastDistanceSquared)
        {
            staff.LastDistanceSquared = distance2;
            staff.LastProgressAt = now;
        }
        if ((now - staff.LastProgressAt).TotalSeconds < _stallSeconds) return;
        if (staff.RouteRetries < 2 && (now - staff.RouteSince).TotalSeconds < 50)
        {
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_GO_STRAIGHT_TO_COORD,
                staff.Worker.Handle, target.X, target.Y, target.Z,
                _walkSpeed, -1, _receptionDesk.Heading, 0.3f);
            staff.RouteRetries++;
            staff.LastProgressAt = now;
            return;
        }
        if (_routeRecoveryEnabled && staff.RouteTeleports < _recoveryLimit && distance2 <= 10000f)
        {
            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, staff.Worker.Handle);
            staff.Worker.Position = target;
            staff.RouteTeleports++;
            staff.RouteRetries = 0;
            staff.LastDistanceSquared = DistanceSquared(staff.Worker.Position, target);
            staff.LastProgressAt = now;
            staff.RouteSince = now;
            Log("Receptionist teleport recovery to " + FormatPosition(target), null);
        }
        else
        {
            staff.Blocked = true;
            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, staff.Worker.Handle);
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_STAND_STILL, staff.Worker.Handle, -1);
            Log("Receptionist route blocked at " + FormatPosition(target), null);
            Notify("Receptionist path blocked; check Employee Settings.");
        }
    }

    private void DismissReceptionist()
    {
        Receptionist staff = _receptionist;
        try { if (staff.Worker != null && staff.Worker.Exists()) staff.Worker.Delete(); }
        catch (Exception ex) { Log("Dismiss receptionist", ex); }
        finally
        {
            staff.Worker = null;
            staff.Route.Clear();
            staff.NodeIndex = 0;
            staff.AtDesk = false;
            staff.Blocked = false;
            staff.RouteTeleports = 0;
        }
    }

    private void DismissWorkerForSlot(int index)
    {
        if (index < _mechanics.Count) DismissWorker(_mechanics[index]);
    }

    private void DismissWorker(Mechanic employee)
    {
        if (employee.Worker == null)
        {
            employee.WorkMode = -1;
            employee.ArrivalPhase = PhaseNone;
            employee.PathNodes.Clear();
            employee.PathNodeIndex = 0;
            return;
        }
        try
        {
            // Delete ONLY the ped created by our own script, never ambient population.
            if (employee.Worker.Exists()) employee.Worker.Delete();
        }
        catch (Exception ex) { Log("Dismiss worker", ex); }
        finally
        {
            employee.Worker = null; employee.WorkMode = -1; employee.ArrivalPhase = PhaseNone;
            employee.PathNodes.Clear(); employee.PathNodeIndex = 0;
        }
    }

    private bool CaptureStaffPosts()
    {
        bool captured = false;
        foreach (Mechanic employee in _mechanics)
        {
            if (employee.Worker == null || !employee.Worker.Exists()) continue;
            // If a paid job moves to the remote ledger, preserve an idle staging
            // post instead of putting its mechanic inside an orphaned vehicle bay.
            employee.SavedPost.Position = employee.HasJob && _breakArea.IsSet ?
                _breakArea.Position : employee.Worker.Position;
            employee.SavedPost.Heading = employee.Worker.Heading;
            employee.SavedPost.IsSet = true;
            captured = true;
        }
        if (_receptionist.Worker != null && _receptionist.Worker.Exists())
        {
            _receptionist.SavedPost.Position = _receptionist.Worker.Position;
            _receptionist.SavedPost.Heading = _receptionist.Worker.Heading;
            _receptionist.SavedPost.IsSet = true;
            captured = true;
        }
        return captured;
    }

    private WorkBay CurrentManagerWorkstation()
    {
        if (_managerWorkstation.IsSet) return _managerWorkstation;
        if (_officeDeskPlaced && _officeDeskLocation.IsSet) return _officeDeskLocation;
        return _receptionDesk;
    }

    private void DismissVisualManager()
    {
        try { if (_managerWorker != null && _managerWorker.Exists()) _managerWorker.Delete(); }
        catch (Exception ex) { Log("Manager visual cleanup", ex); }
        _managerWorker = null;
        _managerAtPost = false;
        _managerPathNodes.Clear();
        _managerNodeIndex = 0;
        _managerRouteRetries = 0;
    }

    private void StartManagerRoute(DateTime now, bool resumed = false)
    {
        if (_managerWorker == null || !_managerWorker.Exists()) return;
        _managerPathNodes.Clear();
        if (_customManagerWalkingPaths && !resumed)
        {
            foreach (WorkBay point in _managerRoute)
                if (point.IsSet) _managerPathNodes.Add(point.Position);
            if (_managerApproach.IsSet) _managerPathNodes.Add(_managerApproach.Position);
        }
        WorkBay post = CurrentManagerWorkstation();
        _managerPathNodes.Add(post.Position);
        _managerNodeIndex = 0;
        _managerAtPost = false;
        _managerRouteRetries = 0;
        WalkManagerRouteNode(now);
    }

    private void WalkManagerRouteNode(DateTime now)
    {
        if (_managerWorker == null || !_managerWorker.Exists() ||
            _managerNodeIndex >= _managerPathNodes.Count) return;
        Vector3 target = _managerPathNodes[_managerNodeIndex];
        _managerNodeSince = now;
        _managerLastProgress = now;
        _managerLastDistanceSq = DistanceSquared(_managerWorker.Position, target);
        WorkBay post = CurrentManagerWorkstation();
        GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, _managerWorker.Handle);
        if (_customManagerWalkingPaths)
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_GO_STRAIGHT_TO_COORD,
                _managerWorker.Handle, target.X, target.Y, target.Z,
                _walkSpeed, -1, post.Heading, 0.3f);
        else
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_FOLLOW_NAV_MESH_TO_COORD,
                _managerWorker.Handle, target.X, target.Y, target.Z,
                _walkSpeed, -1, 0.3f, 0, post.Heading);
    }

    private void UpdateVisualManager(DateTime now)
    {
        WorkBay post = CurrentManagerWorkstation();
        Ped player = Game.Player.Character;
        if (!_clockedIn || _managerTier < 0 || !post.IsSet || player == null ||
            DistanceSquared(player.Position, _location) > _spawnDistance * _spawnDistance)
        { DismissVisualManager(); return; }
        if (_managerWorker != null && (!_managerWorker.Exists() || _managerWorker.IsDead))
            DismissVisualManager();
        if (_managerWorker == null)
        {
            if (now < _nextManagerSpawn || _managerPedModel == null ||
                !_managerPedModel.IsInCdImage || !_managerPedModel.IsPed) return;
            if (!_managerPedModel.IsLoaded) { _managerPedModel.Request(); return; }
            Vector3 spawn = _employeeEntrance.IsSet ? _employeeEntrance.Position : post.Position;
            if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPHERE_VISIBLE,
                spawn.X, spawn.Y, spawn.Z + 0.8f, 1f) ||
                DistanceSquared(player.Position, spawn) < 100f) return;
            _nextManagerSpawn = now.AddSeconds(20);
            try
            {
                _managerWorker = World.CreatePed(_managerPedModel, spawn,
                    _employeeEntrance.IsSet ? _employeeEntrance.Heading : post.Heading);
                if (_managerWorker == null || !_managerWorker.Exists()) return;
                _managerWorker.IsPersistent = true;
                _managerWorker.BlockPermanentEvents = true;
                _managerWorker.AlwaysKeepTask = true;
                StartManagerRoute(now);
            }
            catch (Exception ex) { Log("Manager spawn", ex); DismissVisualManager(); return; }
        }
        if (_managerAtPost || _managerPathNodes.Count == 0) return;
        try
        {
            Vector3 destination = _managerPathNodes[_managerNodeIndex];
            float remainingSq = DistanceSquared(_managerWorker.Position, destination);
            bool finalNode = _managerNodeIndex == _managerPathNodes.Count - 1;
            if (remainingSq < (finalNode ? 0.49f : 2.25f))
            {
                _managerNodeIndex++;
                _managerRouteRetries = 0;
                if (_managerNodeIndex < _managerPathNodes.Count)
                    WalkManagerRouteNode(now);
                else
                {
                    _managerAtPost = true;
                    GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, _managerWorker.Handle);
                    GTA.Native.Function.Call(GTA.Native.Hash.TASK_START_SCENARIO_AT_POSITION,
                        _managerWorker.Handle, "WORLD_HUMAN_CLIPBOARD",
                        post.Position.X, post.Position.Y, post.Position.Z,
                        post.Heading, -1, false, true);
                }
                return;
            }
            if (remainingSq + 0.09f < _managerLastDistanceSq)
            { _managerLastProgress = now; _managerLastDistanceSq = remainingSq; }
            // One retry per 10s; after 3 failed retries stop issuing tasks.
            // Never infinite-loop or teleport an employee across an indoor MLO.
            if ((now - _managerLastProgress).TotalSeconds >= Math.Max(8, _stallSeconds) &&
                (now - _managerNodeSince).TotalSeconds >= 8 && _managerRouteRetries < 3)
            {
                _managerRouteRetries++;
                WalkManagerRouteNode(now);
            }
        }
        catch (Exception ex) { Log("Manager route", ex); DismissVisualManager(); _nextManagerSpawn = now.AddSeconds(25); }
    }

    private void CleanupVisualMechanics()
    {
        foreach (Mechanic employee in _mechanics) DismissWorker(employee);
        DismissReceptionist();
        DismissVisualManager();
    }

    private void TransferPlayerMoney(bool deposit)
    {
        // SHVDN Player.Money works only for the three stock Story Mode protagonists.
        // MP freemode peds cannot safely use this API for cash transfers.
        if (Game.Player.Character == null) return;
        int modelHash = Game.Player.Character.Model.Hash;
        // Native GTA V protagonist model hashes (no PedHash enum dependency).
        const int MichaelModel = unchecked((int)0x0D7114C9);  // player_zero
        const int FranklinModel = unchecked((int)0x9B22DBAF); // player_one
        const int TrevorModel = unchecked((int)0x9B810FA2);   // player_two
        if (modelHash != MichaelModel &&
            modelHash != FranklinModel &&
            modelHash != TrevorModel)
        {
            Notify("Cash transfer unavailable with this ped model. Use business earnings for now.");
            return;
        }
        if (!_bootGameplayActivated || Game.IsPaused) return;
        int wallet = Game.Player.Money;
        if (deposit && wallet < 1000 || !deposit && (_cash < 1000 || wallet > int.MaxValue - 1000))
        { Notify("Transfer unavailable: insufficient funds or wallet limit."); return; }
        if (deposit && _cash > long.MaxValue - 1000) return;
        long cash = _cash, deposits = _financeOwnerDeposits, withdrawals = _financeOwnerWithdrawals;
        bool saved = false;
        try
        {
            Game.Player.Money = deposit ? wallet - 1000 : wallet + 1000;
            _cash += deposit ? 1000 : -1000;
            if (deposit) _financeOwnerDeposits += 1000; else _financeOwnerWithdrawals += 1000;
            saved = Save();
        }
        catch (Exception ex) { Log("Owner cash transfer", ex); }
        if (!saved)
        {
            _cash = cash; _financeOwnerDeposits = deposits; _financeOwnerWithdrawals = withdrawals;
            try { Game.Player.Money = wallet; } catch (Exception ex) { Log("Owner wallet rollback requires review", ex); }
            Notify("Transfer failed; business state restored. Check wallet if an external mod intervened."); return;
        }
        Notify(deposit ? "Owner deposit $1,000 committed (not sales)." : "Owner withdrawal $1,000 committed (not an expense).");
    }

    private bool CustomerConfigurationReady()
    {
        if (!_placed || !_customerDropoff.IsSet || !_customerQueue.IsSet ||
            !_receptionistHired || !_receptionDesk.IsSet || !_employeeEntrance.IsSet ||
            !_breakArea.IsSet || _mechanics.Count == 0) return false;
        for (int i = 0; i < _mechanics.Count && i < VisualBayCount; i++)
            if (_bays[i].IsSet) return true;
        return false;
    }

    // Include ambient/player/other mod vehicles; a customer's parking spot is not
    // clear merely because Glass Heroes has no active work order.
    // The active customer vehicle may be ignored ONLY for diagnostics; dispatch
    // always passes null so ALL occupied spots prevent new arrivals.
    private bool DropoffOccupied(Vehicle except)
    {
        if (!_customerDropoff.IsSet) return true;
        foreach (Vehicle car in World.GetNearbyVehicles(_customerDropoff.Position, _dropoffClearanceRadius))
        {
            if (car == null || !car.Exists()) continue;
            if (except != null && car.Handle == except.Handle) continue;
            if (DistanceSquared(car.Position, _customerDropoff.Position) <=
                _dropoffClearanceRadius * _dropoffClearanceRadius) return true;
        }
        return false;
    }

    private void RestoreCurrentOrder()
    {
        _customerOrder = _activeCustomerOrders.Count > 0 ? _activeCustomerOrders[0] : null;
    }

    private void AddActiveOrder(CustomerOrder order)
    {
        if (order == null || _activeCustomerOrders.Contains(order)) return;
        _activeCustomerOrders.Add(order);
        RestoreCurrentOrder();
    }

    private void CancelAllCustomerOrders(string reason)
    {
        foreach (CustomerOrder order in _activeCustomerOrders.ToArray())
        {
            _customerOrder = order;
            CancelCustomerOrder(reason, false);
        }
        RestoreCurrentOrder();
    }

    // Only one arriving vehicle may claim drop-off at a time. A paid vehicle
    // returning from a bay takes priority over future arrivals.
    private static bool NeedsDropoffSoon(CustomerOrder order)
    {
        if (order == null) return false;
        switch (order.Stage)
        {
            case CustomerStage.MechanicWalkingToReturnCar:
            case CustomerStage.MechanicEnteringForReturn:
            case CustomerStage.MechanicReversingFromBay:
            case CustomerStage.TeleportWaitingPickup:
            case CustomerStage.MechanicWaitingToReturn:
            case CustomerStage.MechanicDrivingToDropoff:
            case CustomerStage.MechanicParkingAtDropoff:
            case CustomerStage.MechanicExitingAtDropoff:
            case CustomerStage.ToExitApproach:
            case CustomerStage.ReturningToVehicle:
            case CustomerStage.DrivingAway:
                return true;
            default: return false;
        }
    }

    private static bool IntakeInProgress(CustomerOrder order)
    {
        if (order == null) return false;
        switch (order.Stage)
        {
            case CustomerStage.DrivingToDropoff:
            case CustomerStage.ParkingAtDropoff:
            case CustomerStage.ExitingVehicle:
            case CustomerStage.ToApproach:
            case CustomerStage.ToCounter:
            case CustomerStage.CheckingIn:
            case CustomerStage.WaitingForMechanic:
            case CustomerStage.TeleportWaitingBay:
            case CustomerStage.MechanicWalkingToPickup:
            case CustomerStage.MechanicEnteringVehicle:
                return true;
            default: return false;
        }
    }

    private int FreePartsForNewOrders() { return FreeStockForService(-1); }

    // Simulate reservations for all assigned jobs without prematurely spending stock.
    // Specific supplies are reserved first; universal stock covers any remainder.
    // Issue #27: estimates cover configured acquisition and billable repair labor.
    // Inventory has no historical lot costs; these are estimates, never a second expense.
    private sealed class RepairQuote
    {
        public int ServiceKind, DedicatedUnits, UniversalUnits, OrderId;
        public long Baseline, Premium, PartsEstimate, LaborEstimate, Total;
        public double BillableSeconds;
        public bool LegacyPromise, CommitRetryPending;
        public int LastCommitTryGameTime;
    }

    private sealed class SavedRepairPromise
    {
        public int Route, MechanicSlot; // 0 background, 1 physical, 2 remote
        public RepairQuote Quote;
        public double RemainingSeconds;
    }

    private static bool ValidRepairPromise(SavedRepairPromise promise)
    {
        if (promise == null || promise.Quote == null || promise.Route < 0 || promise.Route > 2 ||
            promise.MechanicSlot < -1 || promise.MechanicSlot >= VisualBayCount) return false;
        RepairQuote q = promise.Quote;
        long units = (long)q.DedicatedUnits + q.UniversalUnits;
        return q.OrderId > 0 && q.ServiceKind >= -1 && q.ServiceKind <= 2 &&
            q.DedicatedUnits >= 0 && q.UniversalUnits >= 0 && units >= 1 && units <= 2 &&
            (q.ServiceKind >= 0 || q.DedicatedUnits == 0) && q.Total >= 0 && q.Baseline >= 0 &&
            q.Premium >= 0 && q.PartsEstimate >= 0 && q.LaborEstimate >= 0 &&
            q.BillableSeconds > 0 && !double.IsNaN(q.BillableSeconds) && !double.IsInfinity(q.BillableSeconds) &&
            promise.RemainingSeconds >= 0 && !double.IsNaN(promise.RemainingSeconds) &&
            promise.RemainingSeconds <= Math.Max(15d, q.BillableSeconds);
    }

    private string RepairLedgerStamp()
    {
        // A prior build may preserve unknown journal keys while changing money/stock.
        // Detect that rollback/edit instead of replaying a stale accepted order.
        long[] values = { _cash, _revenue, _customerRevenue, _remoteSales, _parts,
            _specialStock[0], _specialStock[1], _specialStock[2], _completed, _customerCompleted,
            _remoteJobsCompleted, _remotePartsConsumed, _lastCustomerOrderId, _partsSalesRevenue, _partsSalesUnitsSold };
        string text = string.Join("|", Array.ConvertAll(values,
            value => value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        using (var hash = System.Security.Cryptography.SHA256.Create())
            return BitConverter.ToString(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-", "");
    }

    private void LoadRepairPromises(ScriptSettings data)
    {
        int count = data.GetValue("RepairPromisesV1", "Count", 0);
        int version = data.GetValue("RepairPromisesV1", "Version", 1);
        if (count < 0 || count > MaxSavedRepairPromises || (count > 0 && version != 1))
            throw new InvalidDataException("Unsupported repair promise ledger; original save left untouched.");
        if (count > 0 && !string.Equals(data.GetValue("RepairPromisesV1", "LedgerStamp", ""),
            RepairLedgerStamp(), StringComparison.Ordinal))
            throw new InvalidDataException("Saved repair journal does not match the current financial ledger. Restore a matching backup or request review; no save changed.");
        var ids = new HashSet<int>();
        for (int i = 0; i < count; i++)
        {
            string section = "RepairPromiseV1." + i;
            var q = new RepairQuote {
                OrderId = data.GetValue(section, "OrderId", -1), ServiceKind = data.GetValue(section, "ServiceKind", -2),
                DedicatedUnits = data.GetValue(section, "DedicatedUnits", -1), UniversalUnits = data.GetValue(section, "UniversalUnits", -1),
                Baseline = data.GetValue(section, "Baseline", -1L), Premium = data.GetValue(section, "Premium", -1L),
                PartsEstimate = data.GetValue(section, "PartsEstimate", -1L), LaborEstimate = data.GetValue(section, "LaborEstimate", -1L),
                Total = data.GetValue(section, "Total", -1L), BillableSeconds = data.GetValue(section, "BillableSeconds", -1d),
                LegacyPromise = data.GetValue(section, "LegacyPromise", false)
            };
            var promise = new SavedRepairPromise { Quote = q, Route = data.GetValue(section, "Route", -1),
                MechanicSlot = data.GetValue(section, "MechanicSlot", -2), RemainingSeconds = data.GetValue(section, "RemainingSeconds", -1d) };
            if (!ValidRepairPromise(promise) || !ids.Add(q.OrderId))
                throw new InvalidDataException("Invalid or duplicate saved repair quote; original save left untouched.");
            _pendingRepairPromises.Add(promise);
            _lastCustomerOrderId = Math.Max(_lastCustomerOrderId, q.OrderId);
        }
    }

    private List<SavedRepairPromise> CaptureRepairPromises()
    {
        var records = new Dictionary<int, SavedRepairPromise>();
        foreach (SavedRepairPromise p in _pendingRepairPromises) records[p.Quote.OrderId] = p;
        bool[] claimed = new bool[_mechanics.Count];
        foreach (CustomerOrder order in _activeCustomerOrders)
        {
            if (order.MechanicSlot >= 0 && order.MechanicSlot < claimed.Length) claimed[order.MechanicSlot] = true;
            if (order.Paid) { records.Remove(order.Id); continue; }
            if (order.Pricing == null) continue; // An unaccepted random request is not a promised quote.
            double worked = order.MechanicSlot >= 0 && order.MechanicSlot < _mechanics.Count ?
                _mechanics[order.MechanicSlot].WorkSeconds : order.ResumeWorkSeconds;
            records[order.Id] = new SavedRepairPromise { Route = 1, Quote = order.Pricing,
                MechanicSlot = order.MechanicSlot >= 0 ? order.MechanicSlot : order.ResumeMechanicSlot,
                RemainingSeconds = Math.Max(0, order.RepairSeconds - worked) };
        }
        foreach (RemoteJob job in _remoteJobs)
        {
            if (job.MechanicSlot >= 0 && job.MechanicSlot < claimed.Length) claimed[job.MechanicSlot] = true;
            if (job.Pricing == null) continue;
            records[job.OrderId] = new SavedRepairPromise { Route = 2, Quote = job.Pricing,
                MechanicSlot = job.MechanicSlot, RemainingSeconds = Math.Max(0, job.RemainingGameSeconds) };
        }
        for (int i = 0; i < _mechanics.Count; i++)
        {
            Mechanic m = _mechanics[i];
            if (claimed[i] || !m.HasJob || m.Pricing == null) continue;
            records[m.Pricing.OrderId] = new SavedRepairPromise { Route = 0, Quote = m.Pricing,
                MechanicSlot = i, RemainingSeconds = Math.Max(0, m.Pricing.BillableSeconds - m.WorkSeconds) };
        }
        var result = new List<SavedRepairPromise>(records.Values);
        if (result.Count > MaxSavedRepairPromises) throw new InvalidDataException("Repair promise capacity exceeded; no save replaced.");
        foreach (SavedRepairPromise p in result)
            if (!ValidRepairPromise(p)) throw new InvalidDataException("Invalid runtime repair promise; no save replaced.");
        return result;
    }

    private void WriteRepairPromises(ScriptSettings data)
    {
        List<SavedRepairPromise> records = CaptureRepairPromises();
        data.SetValue("RepairPromisesV1", "Version", 1);
        data.SetValue("RepairPromisesV1", "Count", records.Count);
        data.SetValue("RepairPromisesV1", "LedgerStamp", records.Count > 0 ? RepairLedgerStamp() : "");
        for (int i = 0; i < records.Count; i++)
        {
            SavedRepairPromise p = records[i]; RepairQuote q = p.Quote;
            string section = "RepairPromiseV1." + i;
            data.SetValue(section, "OrderId", q.OrderId); data.SetValue(section, "ServiceKind", q.ServiceKind);
            data.SetValue(section, "DedicatedUnits", q.DedicatedUnits); data.SetValue(section, "UniversalUnits", q.UniversalUnits);
            data.SetValue(section, "Baseline", q.Baseline); data.SetValue(section, "Premium", q.Premium);
            data.SetValue(section, "PartsEstimate", q.PartsEstimate); data.SetValue(section, "LaborEstimate", q.LaborEstimate);
            data.SetValue(section, "Total", q.Total); data.SetValue(section, "BillableSeconds", q.BillableSeconds);
            data.SetValue(section, "LegacyPromise", q.LegacyPromise); data.SetValue(section, "Route", p.Route);
            data.SetValue(section, "MechanicSlot", p.MechanicSlot); data.SetValue(section, "RemainingSeconds", p.RemainingSeconds);
        }
    }

    private void RememberCancelledRepair(CustomerOrder order)
    {
        if (order == null || order.Paid || order.Pricing == null ||
            _remoteJobs.Exists(j => j.OrderId == order.Id)) return;
        double worked = order.MechanicSlot >= 0 && order.MechanicSlot < _mechanics.Count ?
            _mechanics[order.MechanicSlot].WorkSeconds : order.ResumeWorkSeconds;
        var record = new SavedRepairPromise { Route = 1, Quote = order.Pricing,
            MechanicSlot = order.MechanicSlot >= 0 ? order.MechanicSlot : order.ResumeMechanicSlot,
            RemainingSeconds = Math.Max(0, order.RepairSeconds - worked) };
        int found = _pendingRepairPromises.FindIndex(p => p.Quote.OrderId == order.Id);
        if (found >= 0)
        {
            record.RemainingSeconds = Math.Min(record.RemainingSeconds, _pendingRepairPromises[found].RemainingSeconds);
            _pendingRepairPromises[found] = record;
        }
        else _pendingRepairPromises.Add(record);
    }

    private void ParkRuntimeRepairPromises()
    {
        List<SavedRepairPromise> snapshot = CaptureRepairPromises();
        _pendingRepairPromises.Clear();
        _pendingRepairPromises.AddRange(snapshot);
    }

    private bool RestoreSavedNonphysicalRepairs()
    {
        if (!_clockedIn || !_bootGameplayActivated || Game.IsPaused || _pendingRepairPromises.Count == 0) return false;
        bool changed = false;
        for (int pendingIndex = _pendingRepairPromises.Count - 1; pendingIndex >= 0; pendingIndex--)
        {
            SavedRepairPromise p = _pendingRepairPromises[pendingIndex];
            if (p.Route == 1 || (p.Route == 2 && _managerTier < 0)) continue;
            int slot = p.MechanicSlot;
            if (slot < 0 || slot >= _mechanics.Count)
            {
                slot = -1;
                for (int i = 0; i < _mechanics.Count; i++) if (!_mechanics[i].HasJob) { slot = i; break; }
            }
            if (slot < 0 || _mechanics[slot].HasJob) continue;
            Mechanic m = _mechanics[slot];
            m.HasJob = true; m.IdleSeconds = 0;
            if (p.Route == 0)
            { m.Pricing = p.Quote; m.WorkSeconds = Math.Max(0, p.Quote.BillableSeconds - p.RemainingSeconds); }
            else
            {
                m.WorkSeconds = 0;
                _remoteJobs.Add(new RemoteJob { MechanicSlot = slot, OrderId = p.Quote.OrderId,
                    ServiceKind = p.Quote.ServiceKind, PartsNeeded = p.Quote.DedicatedUnits + p.Quote.UniversalUnits,
                    Price = p.Quote.Total, Pricing = p.Quote, RemainingGameSeconds = p.RemainingSeconds });
            }
            _pendingRepairPromises.RemoveAt(pendingIndex);
            changed = true;
            Notify("Resumed saved repair #" + p.Quote.OrderId + " at its fixed quote $" + p.Quote.Total + "; no offline progress.");
        }
        return changed;
    }

    private static void ApplySavedCustomerPromise(CustomerOrder order, SavedRepairPromise promise)
    {
        RepairQuote q = promise.Quote;
        order.Id = q.OrderId; order.ServiceKind = q.ServiceKind;
        order.PartsNeeded = q.DedicatedUnits + q.UniversalUnits;
        order.Price = q.Total; order.Pricing = q; order.RepairSeconds = q.BillableSeconds;
        order.RestoredPromise = true; order.ResumeMechanicSlot = promise.MechanicSlot;
        order.ResumeWorkSeconds = Math.Max(0, q.BillableSeconds - promise.RemainingSeconds);
    }

    private bool RepairQuoteHasStock(RepairQuote quote)
    {
        if (quote == null) return false;
        int[] free = UnreservedRepairStock(quote);
        return free[0] >= quote.UniversalUnits && (quote.DedicatedUnits == 0 ||
            (quote.ServiceKind >= 0 && free[quote.ServiceKind + 1] >= quote.DedicatedUnits));
    }

    private bool TrySettleRepair(RepairQuote quote, Mechanic mechanic, CustomerOrder order, RemoteJob remote)
    {
        if (!_bootGameplayActivated || Game.IsPaused || quote == null || mechanic == null ||
            (order != null && (order.Paid || order.Price != quote.Total)) ||
            (remote != null && remote.Price != quote.Total)) return false;
        int now = Game.GameTime;
        if (quote.CommitRetryPending && unchecked((uint)(now - quote.LastCommitTryGameTime)) < 3000U) return false;
        bool customer = order != null || remote != null;
        if (!CanCreditRepair(quote.Total, customer, remote != null) || _completed == int.MaxValue ||
            (customer && _customerCompleted == int.MaxValue) || mechanic.JobsCompleted == int.MaxValue ||
            (remote != null && (_remoteJobsCompleted == long.MaxValue ||
                _remotePartsConsumed > long.MaxValue - remote.PartsNeeded))) return false;
        int parts = _parts; int[] special = (int[])_specialStock.Clone();
        if (!TryConsumeQuotedSupplies(quote)) return false;
        long cash = _cash, revenue = _revenue, customerRevenue = _customerRevenue, remoteRevenue = _remoteSales;
        long remoteCompleted = _remoteJobsCompleted, remoteParts = _remotePartsConsumed;
        int completed = _completed, customerCompleted = _customerCompleted, xp = mechanic.Xp, jobs = mechanic.JobsCompleted;
        bool hasJob = mechanic.HasJob; double work = mechanic.WorkSeconds, idle = mechanic.IdleSeconds;
        RepairQuote backgroundQuote = mechanic.Pricing; string outcome = _lastOrderOutcome;
        int remoteIndex = remote == null ? -1 : _remoteJobs.IndexOf(remote);
        if (remote != null && remoteIndex < 0)
        { _parts = parts; Array.Copy(special, _specialStock, 3); return false; }
        int oldLevel = MechanicLevel(mechanic);
        _cash += quote.Total; _revenue += quote.Total; _completed++;
        mechanic.WorkSeconds = 0; mechanic.IdleSeconds = 0;
        if (customer) { _customerRevenue += quote.Total; _customerCompleted++; }
        if (order != null)
        {
            order.Paid = true;
            if (!order.VehicleHandoff) mechanic.HasJob = false;
            _lastOrderOutcome = "#" + order.Id + " PAID $" + quote.Total.ToString("N0") + " / " + ServiceName(order.ServiceKind);
        }
        else mechanic.HasJob = false;
        if (!customer) mechanic.Pricing = null;
        if (remote != null)
        {
            _remoteSales += quote.Total; _remoteJobsCompleted++; _remotePartsConsumed += remote.PartsNeeded;
            _lastOrderOutcome = "REMOTE #" + remote.OrderId + " PAID $" + quote.Total;
            _remoteJobs.RemoveAt(remoteIndex);
        }
        int gain = order != null ? 25 + (SpecialistMatches(mechanic, order.ServiceKind) ? 10 : 0) : remote != null ? 25 : 15;
        mechanic.Xp = (int)Math.Min(100000L, (long)mechanic.Xp + gain); mechanic.JobsCompleted++;
        if (!Save())
        {
            _parts = parts; Array.Copy(special, _specialStock, 3);
            _cash = cash; _revenue = revenue; _customerRevenue = customerRevenue; _remoteSales = remoteRevenue;
            _remoteJobsCompleted = remoteCompleted; _remotePartsConsumed = remoteParts;
            _completed = completed; _customerCompleted = customerCompleted; _lastOrderOutcome = outcome;
            mechanic.HasJob = hasJob; mechanic.WorkSeconds = work; mechanic.IdleSeconds = idle;
            mechanic.Pricing = backgroundQuote; mechanic.Xp = xp; mechanic.JobsCompleted = jobs;
            if (order != null) order.Paid = false;
            if (remote != null) _remoteJobs.Insert(remoteIndex, remote);
            quote.CommitRetryPending = true; quote.LastCommitTryGameTime = now;
            return false;
        }
        quote.CommitRetryPending = false;
        if (MechanicLevel(mechanic) > oldLevel) Notify(mechanic.Name + " reached level " + MechanicLevel(mechanic) + "!");
        return true;
    }

    private void InitialiseRepairPricing()
    {
        string[] names = { "Universal", "Filters", "Brakes", "Diagnostics" };
        for (int bucket = 0; bucket < 4; bucket++)
        {
            long baseCost = bucket == 0 ? _partCost : _specialUnitCosts[bucket - 1];
            // Express is the highest existing procurement rate; manager discounts
            // can only reduce it. An explicit nonnegative estimate may override it.
            long fallback = (Math.Max(0L, baseCost) * 125L + 99L) / 100L;
            long configured = _configuration.GetValue("Pricing", "UnitCostEstimate" + names[bucket], -1L);
            _unitCostEstimates[bucket] = configured < 0 ? fallback : configured;
        }
    }

    private long EstimatedUnitAcquisitionCost(int kind)
    {
        return kind < -1 || kind > 2 ? 0L : _unitCostEstimates[kind + 1];
    }

    private static RepairQuote CalculateRepairQuote(long baseline, long premium,
        int kind, int dedicated, int universal, long specificUnitCost, long universalUnitCost,
        long wage, double wagePeriod, double billableSeconds)
    {
        if (kind < -1 || kind > 2 || dedicated < 0 || universal < 0 ||
            (kind == -1 && dedicated != 0) || specificUnitCost < 0 || universalUnitCost < 0 ||
            wage < 0 || wagePeriod <= 0 || billableSeconds < 0 ||
            double.IsNaN(wagePeriod) || double.IsInfinity(wagePeriod) ||
            double.IsNaN(billableSeconds) || double.IsInfinity(billableSeconds))
            throw new ArgumentOutOfRangeException("Repair quote inputs");
        checked
        {
            long parts = dedicated * specificUnitCost + universal * universalUnitCost;
            long labor = (long)Decimal.Ceiling((decimal)wage * (decimal)billableSeconds / (decimal)wagePeriod);
            return new RepairQuote { ServiceKind = kind, DedicatedUnits = dedicated,
                UniversalUnits = universal, Baseline = Math.Max(0L, baseline),
                Premium = Math.Max(0L, premium), PartsEstimate = parts, LaborEstimate = labor,
                BillableSeconds = billableSeconds,
                Total = Math.Max(Math.Max(0L, baseline) + Math.Max(0L, premium), parts + labor) };
        }
    }

    private static void ReserveRepairUnits(int[] available, int kind, int quantity)
    {
        int needed = Math.Max(0, quantity);
        int specific = kind >= 0 && kind <= 2 ? Math.Min(available[kind + 1], needed) : 0;
        if (kind >= 0 && kind <= 2) available[kind + 1] -= specific;
        available[0] = Math.Max(0, available[0] - (needed - specific));
    }

    private static void ReserveQuotedUnits(int[] available, RepairQuote quote)
    {
        if (quote.ServiceKind >= 0 && quote.ServiceKind <= 2)
            available[quote.ServiceKind + 1] = Math.Max(0,
                available[quote.ServiceKind + 1] - Math.Max(0, quote.DedicatedUnits));
        available[0] = Math.Max(0, available[0] - Math.Max(0, quote.UniversalUnits));
    }

    private int[] UnreservedRepairStock(RepairQuote excluded)
    {
        int[] available = { Math.Max(0, _parts), Math.Max(0, _specialStock[0]),
            Math.Max(0, _specialStock[1]), Math.Max(0, _specialStock[2]) };
        bool[] claimed = new bool[_mechanics.Count];
        var seen = new HashSet<RepairQuote>();
        foreach (CustomerOrder order in _activeCustomerOrders)
        {
            if (order.MechanicSlot < 0 && !order.RestoredPromise) continue;
            if (order.MechanicSlot >= 0 && order.MechanicSlot < claimed.Length) claimed[order.MechanicSlot] = true;
            if (order.Paid) continue;
            RepairQuote quote = order.Pricing;
            if (quote != null)
            {
                if (!Object.ReferenceEquals(quote, excluded) && seen.Add(quote))
                    ReserveQuotedUnits(available, quote);
            }
            else ReserveRepairUnits(available, order.ServiceKind, order.PartsNeeded);
        }
        foreach (RemoteJob job in _remoteJobs)
        {
            if (job.MechanicSlot >= 0 && job.MechanicSlot < claimed.Length) claimed[job.MechanicSlot] = true;
            if (job.Pricing != null)
            {
                if (!Object.ReferenceEquals(job.Pricing, excluded) && seen.Add(job.Pricing))
                    ReserveQuotedUnits(available, job.Pricing);
            }
            else ReserveRepairUnits(available, job.ServiceKind, job.PartsNeeded);
        }
        foreach (SavedRepairPromise promise in _pendingRepairPromises)
            if (!Object.ReferenceEquals(promise.Quote, excluded) && seen.Add(promise.Quote))
                ReserveQuotedUnits(available, promise.Quote);
        for (int i = 0; i < _mechanics.Count; i++)
        {
            Mechanic mechanic = _mechanics[i];
            if (!mechanic.HasJob || claimed[i]) continue;
            if (mechanic.Pricing != null)
            {
                if (!Object.ReferenceEquals(mechanic.Pricing, excluded) && seen.Add(mechanic.Pricing))
                    ReserveQuotedUnits(available, mechanic.Pricing);
            }
            else ReserveRepairUnits(available, -1, 1);
        }
        return available;
    }

    private bool TryCreateRepairQuote(int kind, int quantity, Mechanic mechanic,
        double duration, long baseline, long premium, out RepairQuote quote)
    {
        quote = null;
        if (_pendingRepairPromises.Count >= MaxSavedRepairPromises - VisualBayCount) return false;
        if (kind < -1 || kind > 2 || quantity <= 0 || mechanic == null) return false;
        int[] available = UnreservedRepairStock(null);
        int specific = kind >= 0 ? Math.Min(available[kind + 1], quantity) : 0;
        int universal = quantity - specific;
        if (available[0] < universal) return false;
        try
        {
            quote = CalculateRepairQuote(baseline, premium, kind, specific, universal,
                EstimatedUnitAcquisitionCost(kind), EstimatedUnitAcquisitionCost(-1),
                MechanicWage(mechanic), _wagePeriodSeconds, duration);
            return true;
        }
        catch (ArithmeticException ex) { Log("Repair quote exceeds supported money range", ex); return false; }
        catch (ArgumentOutOfRangeException ex) { Log("Invalid repair quote", ex); return false; }
    }

    private bool TryRetainLegacyRepairQuote(int kind, int quantity, Mechanic mechanic,
        double duration, long promised, int id, Action<RepairQuote> attach)
    {
        if (promised < 0) return false;
        // Temporarily mark the legacy owner's reservation so it is not counted
        // twice while calculating its dedicated/universal allocation.
        attach(new RepairQuote { ServiceKind = kind });
        RepairQuote quote;
        if (!TryCreateRepairQuote(kind, quantity, mechanic, duration, promised, 0, out quote))
        { attach(null); return false; }
        quote.Total = promised; // never silently increase an already-promised receipt
        quote.OrderId = id;
        quote.LegacyPromise = true;
        attach(quote);
        Notify("Prior repair quote retained at $" + promised + "; cost estimate shown separately.");
        return true;
    }

    private bool CanCreditRepair(long price, bool customer, bool remote)
    {
        return price >= 0 && _cash <= long.MaxValue - price && _revenue <= long.MaxValue - price &&
            (!customer || _customerRevenue <= long.MaxValue - price) &&
            (!remote || _remoteSales <= long.MaxValue - price);
    }

    private bool TryConsumeQuotedSupplies(RepairQuote quote)
    {
        if (quote == null || quote.ServiceKind < -1 || quote.ServiceKind > 2 ||
            quote.DedicatedUnits < 0 || quote.UniversalUnits < 0 ||
            (quote.ServiceKind == -1 && quote.DedicatedUnits != 0) ||
            (long)quote.DedicatedUnits + quote.UniversalUnits <= 0) return false;
        int[] available = UnreservedRepairStock(quote);
        if (available[0] < quote.UniversalUnits ||
            (quote.ServiceKind >= 0 && available[quote.ServiceKind + 1] < quote.DedicatedUnits)) return false;
        if (quote.ServiceKind >= 0) _specialStock[quote.ServiceKind] -= quote.DedicatedUnits;
        _parts -= quote.UniversalUnits;
        return true;
    }

    private long ServiceQuoteBaseline(int kind)
    {
        return (long)_jobRevenue + (kind == 1 ? 300L : kind == 2 ? 150L : 0L);
    }

    private string RepairQuoteBreakdown(RepairQuote quote)
    {
        return quote == null ? "Quote pending mechanic assignment" :
            "Parts est. $" + quote.PartsEstimate + " + labor $" + quote.LaborEstimate +
            " | service $" + quote.Baseline + " + expertise $" + quote.Premium +
            " | total $" + quote.Total + (quote.LegacyPromise ? " (prior promise retained)" : "");
    }

    private void RecordRepairPricing(RepairQuote quote, string route)
    {
        if (quote == null) return;
        try
        {
            string path = Path.Combine(DataDirectory, "RepairPricing.v1.csv");
            if (!File.Exists(path)) File.AppendAllText(path,
                "RecordedUTC,OrderID,Route,ServiceKind,DedicatedUnits,UniversalUnits,PartsEstimate,LaborEstimate,ServiceBaseline,ExpertisePremium,Total,BillableSeconds,LegacyPromise\r\n");
            File.AppendAllText(path, DateTime.UtcNow.ToString("s") + "," + quote.OrderId + "," + route + "," +
                quote.ServiceKind + "," + quote.DedicatedUnits + "," + quote.UniversalUnits + "," +
                quote.PartsEstimate + "," + quote.LaborEstimate + "," + quote.Baseline + "," +
                quote.Premium + "," + quote.Total + "," +
                quote.BillableSeconds.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "," +
                quote.LegacyPromise + "\r\n");
        }
        catch (Exception ex) { Log("Repair pricing audit", ex); }
    }

    private int FreeStockForService(int kind)
    {
        int[] available = UnreservedRepairStock(null);
        if (kind < 0) return available[0];
        return kind > 2 ? 0 : (int)Math.Min(int.MaxValue, (long)available[0] + available[kind + 1]);
    }

    private bool ConsumeRepairSupplies(CustomerOrder order)
    {
        if (order.Pricing == null && !TryRetainLegacyRepairQuote(order.ServiceKind, order.PartsNeeded,
            _mechanics[order.MechanicSlot], order.RepairSeconds, order.Price, order.Id, q => order.Pricing = q))
            return false;
        return TryConsumeQuotedSupplies(order.Pricing);
    }

    private bool CanStartNextIntake()
    {
        if (_remoteJobs.Count > 0 || _remoteMode) return false;
        if (_activeCustomerOrders.Count >= Math.Min(3, _mechanics.Count)) return false;
        // Reserve stock for every assigned job to avoid two repairs consuming
        // the same part. New walk-in orders may need up to two parts.
        int needed = _stagedCustomerOrder != null ? _stagedCustomerOrder.PartsNeeded :
            _stagedThirdCustomerOrder != null ? _stagedThirdCustomerOrder.PartsNeeded : 2;
        if (FreeStockForService(_stagedCustomerOrder != null ? _stagedCustomerOrder.ServiceKind :
                _stagedThirdCustomerOrder != null ? _stagedThirdCustomerOrder.ServiceKind : -1) < needed) return false;
        if (DropoffOccupied(null)) return false;
        foreach (CustomerOrder order in _activeCustomerOrders)
            if (NeedsDropoffSoon(order) || IntakeInProgress(order)) return false;
        // Do not bring in a second car if no other staffed bay can accept it.
        // This also avoids deadlock when only one mechanic is hired.
        foreach (Mechanic employee in _mechanics)
            if (!employee.HasJob && employee.Worker != null && employee.Worker.Exists() &&
                employee.ArrivalPhase == PhaseAtBreak) return true;
        return false;
    }

    private bool CanReturnCustomerCar(CustomerOrder order)
    {
        // Another incoming customer has exclusive use of the handoff area
        // until their car has been collected and driven away.
        if (DropoffOccupied(order.Vehicle)) return false;
        foreach (CustomerOrder other in _activeCustomerOrders)
        {
            if (other == order) continue;
            if (other.Stage == CustomerStage.DrivingToDropoff ||
                other.Stage == CustomerStage.ParkingAtDropoff ||
                other.Stage == CustomerStage.ExitingVehicle) return false;
            // Returning cars use the shared driveway one at a time.
            // First serviced customer has priority if multiple orders finish together.
            if (other.Paid && NeedsDropoffSoon(other) && other.Id < order.Id) return false;
        }
        return true;
    }

    private void StartReturnVehicleWalk(CustomerOrder order)
    {
        Mechanic mechanic = _mechanics[order.MechanicSlot];
        order.Stage = CustomerStage.MechanicWalkingToReturnCar;
        order.SecondsInStage = 0;
        List<Vector3> route = new List<Vector3>();
        route.Add(order.ResolvedParkingBay.Position);
        BeginVehicleWalk(order, mechanic, route);
        Notify(mechanic.Name + " returning serviced car to the drop-off.");
    }

    private void QueueDueCustomer(DateTime now)
    {
        if (_nextCustomerAt == DateTime.MinValue)
            _nextCustomerAt = now.AddSeconds(5);
        if (now < _nextCustomerAt) return;
        // Bounded backlog: do not create physical cars or accumulate indefinitely.
        if (_waitingCustomerRequests < EffectiveQueueCapacity())
            _waitingCustomerRequests++;
        _nextCustomerAt = now.AddSeconds(_customerIntervalSeconds);
    }

    private void UpdateCustomerVisuals()
    {
        if (Game.IsPaused) return;
        if (!_clockedIn || _remoteMode || !_customerServiceEnabled || !CustomerConfigurationReady() ||
            Game.Player.Character == null ||
            DistanceSquared(Game.Player.Character.Position, _location) >
                _spawnDistance * _spawnDistance)
        {
            // Manager takeover is deferred until the configured away threshold;
            // preserve unfinished customer state between entity streaming and
            // that threshold instead of silently cancelling the customer invoice.
            bool managerHolding = _managerTier >= 0 && _clockedIn && _placed &&
                Game.Player.Character != null &&
                DistanceSquared(Game.Player.Character.Position, _location) <=
                    _remoteDistanceMeters * _remoteDistanceMeters;
            if (!_remoteMode && !managerHolding)
                CancelAllCustomerOrders("Customer left active area");
            ClearStagedCustomers();
            if (!_clockedIn || !_customerServiceEnabled) _waitingCustomerRequests = 0;
            _waitingForDropoff = false;
            return;
        }
        DateTime now = DateTime.UtcNow;
        bool savedCustomer = _pendingRepairPromises.Exists(p => p.Route == 1);
        if (savedCustomer && _waitingCustomerRequests == 0) _waitingCustomerRequests = 1;
        QueueDueCustomer(now);
        ValidateStagedCustomer(ref _stagedCustomerOrder);
        ValidateStagedCustomer(ref _stagedThirdCustomerOrder);
        UpdateStagedQueueTravel(ref _stagedCustomerOrder);
        UpdateStagedQueueTravel(ref _stagedThirdCustomerOrder);
        if (!savedCustomer && _activeCustomerOrders.Count > 0 && _driveInEnabled && _waitingCustomerRequests > 0)
        {
            if (_stagedCustomerOrder == null)
                SpawnRoadsideWaitingCustomer(now, false);
            if (_stagedCustomerOrder != null && _stagedThirdCustomerOrder == null &&
                _waitingCustomerRequests > 1 && EffectiveQueueCapacity() > 1 && _queueCar3.IsSet)
                SpawnRoadsideWaitingCustomer(now, true);
        }
        bool canDispatch = CanStartNextIntake();
        _waitingForDropoff = _waitingCustomerRequests > 0 && !canDispatch;
        if (_waitingCustomerRequests > 0 && canDispatch &&
            _receptionist.Worker != null && _receptionist.Worker.Exists() && _receptionist.AtDesk)
        {
            if (_stagedCustomerOrder != null || _stagedThirdCustomerOrder != null)
                ActivateRoadsideWaitingCustomer();
            else
            {
                int before = _activeCustomerOrders.Count;
                SpawnCustomerOrder(now);
                if (_activeCustomerOrders.Count > before) _waitingCustomerRequests--;
            }
        }
        else if (_waitingForDropoff && DropoffOccupied(null) &&
            (now - _lastDropoffWarningAt).TotalSeconds >= 45)
        {
            _lastDropoffWarningAt = now;
            Notify("Queue paused: customer drop-off occupied. Waiting for space.");
        }

        // Per-order timer, AI and payment states are processed independently.
        // Snapshot protects the collection if an order ends during iteration.
        foreach (CustomerOrder order in _activeCustomerOrders.ToArray())
        {
            if (!_activeCustomerOrders.Contains(order)) continue;
            _customerOrder = order;
            UpdateOneCustomerOrder(order);
        }
        RestoreCurrentOrder();
    }

    private void UpdateOneCustomerOrder(CustomerOrder order)
    {
        if (order.Customer == null || !order.Customer.Exists() || order.Customer.IsDead ||
            order.Vehicle == null || !order.Vehicle.Exists() ||
            ((!_vehicleTransfersEnabled || order.VehicleTeleport) && order.Vehicle.IsDead))
        {
            CancelCustomerOrder("Missing customer entity", true);
            return;
        }
        if (order.VehicleTeleport && ProcessTeleportOrder(order)) return;
        order.SecondsInStage += 1.0;
        // Customer walk nodes are optional, independent of mechanic/receptionist routes.
        switch (order.Stage)
        {
            case CustomerStage.DrivingToDropoff:
                // The original code told customers to exit within 7m even while rolling.
                // Approach a wider radius first, then let GTA perform a parking maneuver.
                if (_fineParkingEnabled &&
                    DistanceSquared(order.Vehicle.Position, _customerDropoff.Position) < 196f)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_PARK,
                        order.Customer.Handle, order.Vehicle.Handle,
                        _customerDropoff.Position.X, _customerDropoff.Position.Y,
                        _customerDropoff.Position.Z, _customerDropoff.Heading,
                        1, _parkingRadius, false);
                    order.ParkingTaskIssued = true;
                    order.Stage = CustomerStage.ParkingAtDropoff;
                    order.SecondsInStage = 0;
                }
                else if (!_fineParkingEnabled &&
                    DistanceSquared(order.Vehicle.Position, _customerDropoff.Position) < 49f)
                    BeginCustomerExit(order);
                else if (order.SecondsInStage >= _driveArrivalTimeout)
                    CancelCustomerOrder("Road navigation failed; adjust road-side spawn/drop-off", true);
                break;
            case CustomerStage.ParkingAtDropoff:
                // Require proximity AND low speed for a natural parked exit.
                if (DistanceSquared(order.Vehicle.Position, _customerDropoff.Position) < 16f &&
                    order.Vehicle.Speed < 1.5f)
                    BeginCustomerExit(order);
                else if (order.SecondsInStage >= _parkingTimeoutSeconds)
                {
                    // Conservative fallback: the original 7m arrival window.
                    // Never warp a vehicle through props or MLO walls.
                    if (DistanceSquared(order.Vehicle.Position, _customerDropoff.Position) < 49f)
                    {
                        Notify("Customer parking imperfect; continuing check-in from nearby.");
                        BeginCustomerExit(order);
                    }
                    else CancelCustomerOrder("Parking timed out; adjust drop-off heading/position", true);
                }
                break;
            case CustomerStage.ExitingVehicle:
                if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_IN_ANY_VEHICLE,
                        order.Customer.Handle, false))
                {
                    StartCustomerWalkIn(order);
                }
                else if (order.SecondsInStage >= 20)
                    CancelCustomerOrder("Driver unable to exit vehicle", true);
                break;
            case CustomerStage.ToWalkWaypoint:
            case CustomerStage.ToPickupWaypoint:
                AdvanceCustomerWalkingNode(order);
                break;
            case CustomerStage.ToApproach:
                if (CustomerArrived(order, _customerApproach.Position))
                    WalkCustomer(order, _customerQueue.Position, CustomerStage.ToCounter);
                else if (order.SecondsInStage > 40) HandleCustomerStall(order, _customerApproach.Position);
                break;
            case CustomerStage.ToCounter:
                if (CustomerArrived(order, _customerQueue.Position))
                {
                    order.Stage = CustomerStage.CheckingIn;
                    order.SecondsInStage = 0;
                    GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, order.Customer.Handle);
                    GTA.Native.Function.Call(GTA.Native.Hash.TASK_START_SCENARIO_IN_PLACE,
                        order.Customer.Handle, "WORLD_HUMAN_CLIPBOARD", 0, true);
                    Notify("Reception: customer #" + order.Id + " checking in for " + ServiceName(order.ServiceKind) + ".");
                }
                else if (order.SecondsInStage > 40) HandleCustomerStall(order, _customerQueue.Position);
                break;
            case CustomerStage.CheckingIn:
                if (order.SecondsInStage >= 7)
                {
                    order.Stage = CustomerStage.WaitingForMechanic;
                    order.SecondsInStage = 0;
                    // v0.3.1.8 behavior: stay at reception in the check-in scenario
                    // throughout the repair. An early exit-to-exterior task added
                    // in v0.3.1.9 caused customers to wait beside their cars.
                    // Only ReleaseCustomerForPickup() sends them outside.
                }
                break;
            case CustomerStage.WaitingForMechanic:
                AssignCustomerMechanic(order);
                break;
            case CustomerStage.MechanicWalkingToPickup:
                if (ProgressVehicleWalk(order)) BeginEnterVehicle(order, false);
                break;
            case CustomerStage.MechanicEnteringVehicle:
                if (MechanicInCustomerCar(order)) StartValetDeparture(order, true);
                else if (order.SecondsInStage > 22) CancelCustomerOrder("Mechanic could not enter customer vehicle", true);
                break;
            case CustomerStage.MechanicReversingFromDropoff:
                if (order.SecondsInStage >= 2) BeginVehicleDrive(order, true);
                break;
            case CustomerStage.MechanicReversingFromBay:
                if (order.SecondsInStage >= 2) BeginVehicleDrive(order, false);
                break;
            case CustomerStage.MechanicDrivingToBay:
                UpdateVehicleApproach(order, true);
                break;
            case CustomerStage.MechanicParkingAtBay:
                UpdateVehicleParking(order, true);
                break;
            case CustomerStage.MechanicExitingAtBay:
                if (MechanicOutOfCar(order))
                {
                    Mechanic mechanic = _mechanics[order.MechanicSlot];
                    mechanic.ArrivalPhase = PhaseToBay;
                    mechanic.PathNodes = new List<Vector3>();
                    // Outdoor lot: walk from parked car to the work spot without an MLO detour.
                    if (_customEmployeeWalkingPaths && _bayApproaches[order.MechanicSlot].IsSet)
                        mechanic.PathNodes.Add(_bayApproaches[order.MechanicSlot].Position);
                    mechanic.PathNodes.Add(_bays[order.MechanicSlot].Position);
                    mechanic.PathNodeIndex = 0;
                    mechanic.RouteTeleports = 0;
                    WalkCurrentNode(mechanic, order.MechanicSlot, DateTime.UtcNow);
                    order.Stage = CustomerStage.Repairing;
                    order.SecondsInStage = 0;
                    Notify(mechanic.Name + " parked in Bay " + (order.MechanicSlot + 1) + ". Repair started.");
                }
                else if (order.SecondsInStage > 20)
                    CancelCustomerOrder("Mechanic could not leave car at bay", true);
                break;
            case CustomerStage.MechanicWalkingToReturnCar:
                if (ProgressVehicleWalk(order)) BeginEnterVehicle(order, true);
                break;
            case CustomerStage.MechanicEnteringForReturn:
                if (MechanicInCustomerCar(order))
                {
                    if (CanReturnCustomerCar(order)) StartValetDeparture(order, false);
                    else { order.Stage = CustomerStage.MechanicWaitingToReturn; order.ReturnDriverInCar = true; order.SecondsInStage = 0; }
                }
                else if (order.SecondsInStage > 22) CancelCustomerOrder("Mechanic could not re-enter serviced car", true);
                break;
            case CustomerStage.MechanicWaitingToReturn:
                if (CanReturnCustomerCar(order))
                {
                    if (order.ReturnDriverInCar) StartValetDeparture(order, false);
                    else StartReturnVehicleWalk(order);
                }
                else if (order.SecondsInStage >= 20 && ((int)order.SecondsInStage) % 30 == 20)
                    Notify("Mechanic waiting: customer drop-off occupied; return will resume automatically.");
                break;
            case CustomerStage.MechanicDrivingToDropoff:
                UpdateVehicleApproach(order, false);
                break;
            case CustomerStage.MechanicParkingAtDropoff:
                UpdateVehicleParking(order, false);
                break;
            case CustomerStage.MechanicExitingAtDropoff:
                if (MechanicOutOfCar(order))
                {
                    Mechanic mechanic = _mechanics[order.MechanicSlot];
                    mechanic.HasJob = false;
                    // The worker is now at DROP-OFF. Do not cut straight through
                    // the MLO: follow the saved arrival route in forward order.
                    // Pickup used arrival points in reverse; return uses them in forward order.
                    mechanic.ArrivalPhase = PhaseReturningToBreak;
                    mechanic.ReturningFromCustomerCar = true;
                    mechanic.PathNodes = BuildMechanicReturnFromDropoffRoute(order.MechanicSlot);
                    mechanic.PathNodeIndex = 0;
                    mechanic.RouteTeleports = 0;
                    WalkCurrentNode(mechanic, order.MechanicSlot, DateTime.UtcNow);
                    Notify("Serviced car returned to customer parking.");
                    ReleaseCustomerForPickup(order);
                }
                else if (order.SecondsInStage > 20)
                    CancelCustomerOrder("Mechanic could not exit after returning car", true);
                break;
            case CustomerStage.Repairing:
                // Repair clock and financial settlement run in ProcessEconomy.
                break;
            case CustomerStage.ToExitApproach:
                if (CustomerArrived(order, _customerApproach.Position))
                    WalkCustomer(order, _customerDropoff.Position, CustomerStage.ReturningToVehicle);
                else if (order.SecondsInStage > 40) HandleCustomerStall(order, _customerApproach.Position);
                break;
            case CustomerStage.ReturningToVehicle:
                if (CustomerArrived(order, _customerDropoff.Position) || order.SecondsInStage > 45)
                {
                    if (order.DroveIn)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.TASK_ENTER_VEHICLE,
                            order.Customer.Handle, order.Vehicle.Handle, 12000, -1, 1.0f, 1, 0);
                        order.Stage = CustomerStage.DrivingAway;
                        order.SecondsInStage = 0;
                    }
                    else CancelCustomerOrder("Customer collected vehicle", false);
                }
                break;
            case CustomerStage.DrivingAway:
                if (!order.DepartureIssued &&
                    GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_IN_VEHICLE,
                        order.Customer.Handle, order.Vehicle.Handle, false))
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_WANDER,
                        order.Customer.Handle, order.Vehicle.Handle, _driveSpeed, 786603);
                    order.DepartureIssued = true;
                }
                if (order.SecondsInStage > 25 || (order.DepartureIssued &&
                    DistanceSquared(order.Vehicle.Position, _customerDropoff.Position) > 1600f))
                    CancelCustomerOrder("Customer departed", false);
                break;
        }
    }

    // A mechanic only interacts with customer cars during these discrete transport states.
    // Every native task is issued once per state transition, never every update tick.
    private bool MechanicOnVehicleTask(int slot)
    {
        foreach (CustomerOrder order in _activeCustomerOrders)
        {
            if (!order.VehicleHandoff || order.MechanicSlot != slot) continue;
            switch (order.Stage)
            {
                case CustomerStage.MechanicWalkingToPickup:
                case CustomerStage.MechanicEnteringVehicle:
                case CustomerStage.MechanicReversingFromDropoff:
                case CustomerStage.MechanicDrivingToBay:
                case CustomerStage.MechanicParkingAtBay:
                case CustomerStage.MechanicExitingAtBay:
                case CustomerStage.MechanicWalkingToReturnCar:
                case CustomerStage.MechanicEnteringForReturn:
                case CustomerStage.MechanicReversingFromBay:
                case CustomerStage.MechanicWaitingToReturn:
                case CustomerStage.MechanicDrivingToDropoff:
                case CustomerStage.MechanicParkingAtDropoff:
                case CustomerStage.MechanicExitingAtDropoff:
                    return true;
            }
        }
        return false;
    }

    private Ped AssignedWorker(CustomerOrder order)
    {
        if (order.MechanicSlot < 0 || order.MechanicSlot >= _mechanics.Count) return null;
        Ped worker = _mechanics[order.MechanicSlot].Worker;
        return worker != null && worker.Exists() && !worker.IsDead ? worker : null;
    }

    // Mirrors the optional pickup exit route. From the OUTDOOR customer car,
    // go through arrival points in staff-entry order and finish at the break spot.
    // No route points set => retain direct navmesh walking for outdoor parking lots.
    private List<Vector3> BuildMechanicReturnFromDropoffRoute(int slot)
    {
        List<Vector3> route = new List<Vector3>();
        if (_pickupUsesArrivalExit)
        {
            foreach (WorkBay step in _arrivalRoute)
                if (step.IsSet) route.Add(step.Position);
        }
        route.Add(GetBreakSpot(slot));
        return route;
    }

    private List<Vector3> BuildMechanicPickupExitRoute(Vector3 vehiclePosition)
    {
        List<Vector3> route = new List<Vector3>();
        if (_pickupUsesArrivalExit)
        {
            // On the way IN: entrance, point 1, ... point 5, break room.
            // On the way OUT: break room, point 5, ... point 1, car.
            for (int i = _arrivalRoute.Length - 1; i >= 0; i--)
                if (_arrivalRoute[i].IsSet) route.Add(_arrivalRoute[i].Position);
            // An old generic checkpoint must NOT get silently inserted into pickup.
            // With 0/5 configured, pickup stays direct even for a custom MLO.
            // The optional staff entry is a doorway, not mandatory for exterior lots.
            // Do not automatically append it if the user has supplied their own route.
        }
        route.Add(vehiclePosition);
        return route;
    }

    private void BeginVehicleWalk(CustomerOrder order, Mechanic mechanic, List<Vector3> route)
    {
        order.VehicleWalkPath = route;
        order.VehicleWalkIndex = 0;
        order.VehicleWalkRecoveries = 0;
        order.VehicleWalkNodeSeconds = 0;
        order.SecondsInStage = 0;
        mechanic.WorkMode = -1;
        TaskVehicleWalkNode(order);
    }

    private void TaskVehicleWalkNode(CustomerOrder order, bool forceStraight = false)
    {
        Ped worker = AssignedWorker(order);
        if (worker == null || order.VehicleWalkIndex >= order.VehicleWalkPath.Count) return;
        Vector3 target = order.VehicleWalkPath[order.VehicleWalkIndex];
        GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, worker.Handle);
        // Always go DIRECTLY to the car from the mechanic's current location.
        // Default AUTO DIRECT uses navmesh for pedestrian obstacle avoidance.
        // CUSTOM MLO or a blocked navmesh path uses a straight task as fallback.
        bool scriptedExitNode = order.Stage == CustomerStage.MechanicWalkingToPickup &&
            _pickupUsesArrivalExit && order.VehicleWalkIndex < order.VehicleWalkPath.Count - 1;
        if (!_customEmployeeWalkingPaths && !forceStraight && !scriptedExitNode)
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_FOLLOW_NAV_MESH_TO_COORD,
                worker.Handle, target.X, target.Y, target.Z,
                _walkSpeed, -1, 1.0f, 0, worker.Heading);
        else
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_GO_STRAIGHT_TO_COORD,
                worker.Handle, target.X, target.Y, target.Z,
                _walkSpeed, -1, worker.Heading, 0.3f);
        order.VehicleWalkNodeSeconds = 0;
        order.VehicleWalkLastDistanceSq = DistanceSquared(worker.Position, target);
    }

    private bool ProgressVehicleWalk(CustomerOrder order)
    {
        Ped worker = AssignedWorker(order);
        if (worker == null) { CancelCustomerOrder("Mechanic entity lost during vehicle collection", true); return false; }
        if (order.VehicleWalkIndex >= order.VehicleWalkPath.Count) return true;
        Vector3 target = order.VehicleWalkPath[order.VehicleWalkIndex];
        float acceptanceSq = _smoothMechanicWaypoints &&
            order.VehicleWalkIndex < order.VehicleWalkPath.Count - 1 ? 5.29f : 4.0f;
        if (DistanceSquared(worker.Position, target) < acceptanceSq)
        {
            order.VehicleWalkIndex++;
            if (order.VehicleWalkIndex >= order.VehicleWalkPath.Count) return true;
            TaskVehicleWalkNode(order);
            return false;
        }
        float remaining = DistanceSquared(worker.Position, target);
        // Timeouts measure lack of progress, NOT total travel duration. Custom
        // MLO exit legs can be much longer than 18 seconds at walking speed.
        if (remaining + 0.3f < order.VehicleWalkLastDistanceSq)
        {
            order.VehicleWalkLastDistanceSq = remaining;
            order.VehicleWalkNodeSeconds = 0;
        }
        else order.VehicleWalkNodeSeconds++;
        if (order.VehicleWalkNodeSeconds > 18)
        {
            if (order.VehicleWalkRecoveries < 1)
            {
                // No teleporting onto car roofs or into walls. Try one straight fallback.
                order.VehicleWalkRecoveries++;
                TaskVehicleWalkNode(order, true);
                Log("Mechanic pedestrian task stalled: retry " +
                    FormatPosition(target), null);
            }
            else
            {
                // A stalled route is NOT a failed sale. Snap to the next saved
                // walkway coordinate (or beside the car for the final node),
                // then keep the existing assigned mechanic and customer order.
                if (_safeTeleportOnStall && _routeRecoveryEnabled && TryTeleportVehicleWalker(order, worker))
                    return false;
                // Recovery was unsafe or disabled. Leave the job intact and
                // retry the original target on a slow cadence instead of cancelling.
                if (order.VehicleWalkRecoveries < 4)
                {
                    order.VehicleWalkRecoveries++;
                    TaskVehicleWalkNode(order, true);
                }
                else
                {
                    order.VehicleWalkNodeSeconds = 0;
                    Log("Valet walking remains blocked (order kept): " + order.Id, null);
                }
            }
        }
        return false;
    }

    private bool TryTeleportVehicleWalker(CustomerOrder order, Ped worker)
    {
        if (order.VehicleWalkIndex >= order.VehicleWalkPath.Count ||
            worker == null || !worker.Exists()) return false;
        Vector3 target = order.VehicleWalkPath[order.VehicleWalkIndex];
        bool finalNode = order.VehicleWalkIndex == order.VehicleWalkPath.Count - 1;
        // Final target is a car position; do not teleport onto its roof.
        if (finalNode && order.Vehicle != null && order.Vehicle.Exists())
        {
            float angle = order.Vehicle.Heading * (float)Math.PI / 180f;
            target = new Vector3(target.X - (float)Math.Cos(angle) * 2.2f,
                target.Y + (float)Math.Sin(angle) * 2.2f, target.Z + 0.15f);
        }
        if (DistanceSquared(worker.Position, target) > 10000f) return false;
        Ped player = Game.Player.Character;
        if (player != null && player.Exists() && DistanceSquared(player.Position, target) < 1.44f)
            return false; // never land on the player
        try
        {
            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, worker.Handle);
            worker.Position = target; // saved MLO floor Z, no terrain query
            order.VehicleWalkRecoveries = 0;
            order.VehicleWalkNodeSeconds = 0;
            order.VehicleWalkLastDistanceSq = float.MaxValue;
            if (finalNode)
            {
                // ProgressVehicleWalk advances normally next tick (within 2m).
                // Car exterior offsets are not always inside 2m; avoid looping.
                order.VehicleWalkIndex = order.VehicleWalkPath.Count;
            }
            else
            {
                order.VehicleWalkIndex++;
                if (order.VehicleWalkIndex < order.VehicleWalkPath.Count) TaskVehicleWalkNode(order);
            }
            Log("Valet walking teleported to safe recovery point; order " + order.Id, null);
            return true;
        }
        catch (Exception ex) { Log("Valet walk teleport recovery", ex); return false; }
    }

    private void BeginEnterVehicle(CustomerOrder order, bool returning)
    {
        Ped worker = AssignedWorker(order);
        if (worker == null || order.Vehicle == null || !order.Vehicle.Exists())
        { CancelCustomerOrder("Mechanic or customer vehicle missing", true); return; }
        GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, worker.Handle);
        GTA.Native.Function.Call(GTA.Native.Hash.TASK_ENTER_VEHICLE,
            worker.Handle, order.Vehicle.Handle, 18000, -1, 1.0f, 1, 0);
        order.Stage = returning ? CustomerStage.MechanicEnteringForReturn : CustomerStage.MechanicEnteringVehicle;
        order.SecondsInStage = 0;
    }

    private bool MechanicInCustomerCar(CustomerOrder order)
    {
        Ped worker = AssignedWorker(order);
        return worker != null && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_IN_VEHICLE,
            worker.Handle, order.Vehicle.Handle, false);
    }

    private bool MechanicOutOfCar(CustomerOrder order)
    {
        Ped worker = AssignedWorker(order);
        return worker != null && !GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_IN_ANY_VEHICLE,
            worker.Handle, false);
    }

    // AUTO LOT uses only the saved mechanic workstation and its facing direction.
    // It DOES NOT place a vehicle at the NPC's feet: a user-configurable lateral
    // separation keeps the mechanic free to stand and work alongside the car.
    // No vehicle teleport, props bypass, route waypoint, or save-file placement.
    private WorkBay ComputeAutomaticBay(int slot)
    {
        WorkBay mechanicBay = _bays[slot];
        double radians = mechanicBay.Heading * Math.PI / 180.0;
        float sign = _autoLotRightSide ? 1f : -1f;
        float dx = (float)Math.Cos(radians) * _autoLotOffsetMeters * sign;
        float dy = -(float)Math.Sin(radians) * _autoLotOffsetMeters * sign;
        return new WorkBay { IsSet = true,
            Position = new Vector3(mechanicBay.Position.X + dx,
                mechanicBay.Position.Y + dy, mechanicBay.Position.Z),
            Heading = mechanicBay.Heading };
    }

    private WorkBay VehicleDestination(CustomerOrder order, bool toBay)
    {
        return toBay ? order.ResolvedParkingBay : _customerDropoff;
    }

    private bool IsAutoLotParkingBlocked(CustomerOrder order)
    {
        if (!order.AutomaticLot || order.ResolvedParkingBay == null) return false;
        Vector3 target = order.ResolvedParkingBay.Position;
        // Local clearance test primarily detects occupied parking spots (other cars).
        // GTA does not expose full off-road / custom-prop vehicle navigation reliably.
        return GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_POSITION_OCCUPIED,
            target.X, target.Y, target.Z + 0.5f, 2.0f,
            false, true, false, false, false,
            order.Vehicle == null ? 0 : order.Vehicle.Handle, false);
    }

    // OPTIONAL maneuver; GTA's temporary reverse action is not full parking AI.
    // Do not issue it if a nearby vehicle/ped occupies the space immediately behind.
    // It runs only once after entry; the long-range drive task starts on a later tick.
    private void StartValetDeparture(CustomerOrder order, bool toBay)
    {
        if (order == null || order.Vehicle == null || !order.Vehicle.Exists()) return;
        Ped worker = AssignedWorker(order);
        if (worker == null || !MechanicInCustomerCar(order)) return;
        if (_valetReverseOut && order.Vehicle.Speed < 1.5f)
        {
            Vector3 rear = order.Vehicle.Position - order.Vehicle.ForwardVector * 4.0f;
            bool occupied = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_POSITION_OCCUPIED,
                rear.X, rear.Y, rear.Z + 0.4f, 1.3f,
                false, true, true, false, false, order.Vehicle.Handle, false);
            if (!occupied)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.SET_DRIVER_ABILITY, worker.Handle, 1.0f);
                GTA.Native.Function.Call(GTA.Native.Hash.SET_DRIVER_AGGRESSIVENESS, worker.Handle, 0.0f);
                // eTempAction.TA_GO_IN_REVERSE = 22; 1100 milliseconds, not per-tick.
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_TEMP_ACTION,
                    worker.Handle, order.Vehicle.Handle, 22, 1100);
                order.Stage = toBay ? CustomerStage.MechanicReversingFromDropoff :
                    CustomerStage.MechanicReversingFromBay;
                order.SecondsInStage = 0;
                return;
            }
        }
        // No safe rear clearance, or feature OFF: original drive task.
        BeginVehicleDrive(order, toBay);
    }

    private void BeginVehicleDrive(CustomerOrder order, bool toBay)
    {
        Ped worker = AssignedWorker(order);
        if (worker == null) { CancelCustomerOrder("Mechanic disappeared while driving", true); return; }
        WorkBay destination = VehicleDestination(order, toBay);
        float speed = order.AutomaticLot ? 3.0f : _vehicleTransferSpeed;
        float stopRange = order.AutomaticLot ? 3.0f : 6.0f;
        // Best available native driver skill is set in BOTH parking modes;
        // it does not add navmesh or guarantee collisions are avoided.
        GTA.Native.Function.Call(GTA.Native.Hash.SET_DRIVER_ABILITY, worker.Handle, 1.0f);
        GTA.Native.Function.Call(GTA.Native.Hash.SET_DRIVER_AGGRESSIVENESS, worker.Handle, 0.0f);
        if (order.AutomaticLot)
        {
            if (toBay && IsAutoLotParkingBlocked(order))
            {
                // Do not start a task into an already occupied car space.
                CancelCustomerOrder("Auto bay occupied; change side/offset or use MLO mode", true);
                return;
            }
        }
        GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,
            worker.Handle, order.Vehicle.Handle, destination.Position.X, destination.Position.Y,
            destination.Position.Z, speed, CarefulDrivingStyle, stopRange);
        order.Stage = toBay ? CustomerStage.MechanicDrivingToBay : CustomerStage.MechanicDrivingToDropoff;
        order.SecondsInStage = 0;
        order.LastDrivePosition = order.Vehicle.Position;
        order.DriveNoProgressSeconds = 0;
        order.DriveRetryCount = 0;
        Notify(_mechanics[order.MechanicSlot].Name + (toBay ? " driving car into service." : " returning car to drop-off."));
    }

    private void UpdateVehicleApproach(CustomerOrder order, bool toBay)
    {
        Ped worker = AssignedWorker(order);
        if (worker == null || !MechanicInCustomerCar(order))
        { CancelCustomerOrder("Mechanic lost vehicle during transfer", true); return; }
        WorkBay dest = VehicleDestination(order, toBay);
        float parkingDistanceSquared = order.AutomaticLot ? 81f : 196f;
        if (DistanceSquared(order.Vehicle.Position, dest.Position) < parkingDistanceSquared)
        {
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_PARK,
                worker.Handle, order.Vehicle.Handle,
                dest.Position.X, dest.Position.Y, dest.Position.Z, dest.Heading,
                1, order.AutomaticLot ? 2.4f : 2.8f, false);
            order.Stage = toBay ? CustomerStage.MechanicParkingAtBay : CustomerStage.MechanicParkingAtDropoff;
            order.SecondsInStage = 0;
        }
        else if (order.AutomaticLot)
        {
            // Throttled progress check. One alternate avoid-traffic task; never
            // repeatedly spam natives, force acceleration, or warp an occupied car.
            if (DistanceSquared(order.Vehicle.Position, order.LastDrivePosition) >= 2.25f)
            {
                order.LastDrivePosition = order.Vehicle.Position;
                order.DriveNoProgressSeconds = 0;
            }
            else order.DriveNoProgressSeconds += 1.0;
            if (order.DriveNoProgressSeconds >= 14 && order.DriveRetryCount == 0)
            {
                order.DriveRetryCount++;
                order.DriveNoProgressSeconds = 0;
                order.LastDrivePosition = order.Vehicle.Position;
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,
                    worker.Handle, order.Vehicle.Handle, dest.Position.X, dest.Position.Y,
                    dest.Position.Z, 2.4f, RecoveryDrivingStyle, 3.0f);
                Notify("Valet rerouting around parking-lot obstruction (one retry).");
            }
            else if (order.DriveNoProgressSeconds >= 14 && order.DriveRetryCount > 0)
                CancelCustomerOrder("Valet stuck; try opposite auto side or CUSTOM MLO", true);
        }
        if (_activeCustomerOrders.Contains(order) && order.SecondsInStage > _vehicleTransferTimeout)
            CancelCustomerOrder("Vehicle could not reach " + (toBay ? "service bay" : "drop-off") +
                "; check driveable route or switch vehicle mode", true);
    }

    private void UpdateVehicleParking(CustomerOrder order, bool atBay)
    {
        WorkBay dest = VehicleDestination(order, atBay);
        if (DistanceSquared(order.Vehicle.Position, dest.Position) < 16f && order.Vehicle.Speed < 1.5f)
        {
            Ped worker = AssignedWorker(order);
            if (worker == null) { CancelCustomerOrder("Mechanic missing at parking", true); return; }
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE,
                worker.Handle, order.Vehicle.Handle, 0);
            order.Stage = atBay ? CustomerStage.MechanicExitingAtBay : CustomerStage.MechanicExitingAtDropoff;
            order.SecondsInStage = 0;
        }
        else if (order.SecondsInStage > _parkingTimeoutSeconds + 15)
            CancelCustomerOrder("Mechanic failed to park at saved point; revise heading", true);
    }

    private void ReleaseCustomerForPickup(CustomerOrder order)
    {
        BeginCustomerWalkingRoute(order, true);
    }

    private static void BeginCustomerExit(CustomerOrder order)
    {
        GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE,
            order.Customer.Handle, order.Vehicle.Handle, 0);
        order.Stage = CustomerStage.ExitingVehicle;
        order.SecondsInStage = 0;
    }

    private void StartCustomerWalkIn(CustomerOrder order)
    {
        BeginCustomerWalkingRoute(order, false);
    }

    private void BeginCustomerWalkingRoute(CustomerOrder order, bool returning)
    {
        order.CustomerRoute.Clear();
        order.CustomerRouteReturning = returning;
        order.CustomerRouteIndex = 0;
        if (returning && _customerApproach.IsSet)
            order.CustomerRoute.Add(_customerApproach.Position);
        for (int i = returning ? _customerWalkingRoute.Length - 1 : 0;
            returning ? i >= 0 : i < _customerWalkingRoute.Length;
            i += returning ? -1 : 1)
            if (_customerWalkingRoute[i].IsSet)
                order.CustomerRoute.Add(_customerWalkingRoute[i].Position);
        if (!returning && _customerApproach.IsSet)
            order.CustomerRoute.Add(_customerApproach.Position);
        order.CustomerRoute.Add(returning ? _customerDropoff.Position : _customerQueue.Position);
        IssueNextCustomerWalkingNode(order);
    }

    private void IssueNextCustomerWalkingNode(CustomerOrder order)
    {
        bool final = order.CustomerRouteIndex == order.CustomerRoute.Count - 1;
        WalkCustomer(order, order.CustomerRoute[order.CustomerRouteIndex],
            final ? (order.CustomerRouteReturning ? CustomerStage.ReturningToVehicle : CustomerStage.ToCounter)
                  : (order.CustomerRouteReturning ? CustomerStage.ToPickupWaypoint : CustomerStage.ToWalkWaypoint));
    }

    private void AdvanceCustomerWalkingNode(CustomerOrder order)
    {
        Vector3 destination = order.CustomerRoute[order.CustomerRouteIndex];
        if (CustomerArrived(order, destination))
        {
            order.CustomerRouteIndex++;
            if (order.CustomerRouteIndex < order.CustomerRoute.Count)
                IssueNextCustomerWalkingNode(order);
        }
        else if (order.SecondsInStage > 40)
            HandleCustomerStall(order, destination);
    }

    private void SyncCustomerBlip(CustomerOrder order)
    {
        if (order == null) return;
        try
        {
            if (order.CustomerBlip != null)
            {
                if (order.CustomerBlip.Exists()) order.CustomerBlip.Delete();
                order.CustomerBlip = null;
            }
            if (!_customerBlipsEnabled || order.Vehicle == null || !order.Vehicle.Exists()) return;
            order.CustomerBlip = order.Vehicle.AddBlip();
            if (order.CustomerBlip != null)
            {
                order.CustomerBlip.Color = BlipColor.Yellow;
                order.CustomerBlip.Scale = 0.75f;
                order.CustomerBlip.IsShortRange = true;
                order.CustomerBlip.Name = "Glass Heroes Customer #" + order.Id;
            }
        }
        catch (Exception ex) { Log("Customer blip", ex); }
    }

    private WorkBay GetQueueWaitingSpot(bool third)
    {
        return third ? _queueCar3 : (_queueCar2.IsSet ? _queueCar2 : _customerDriveSpawn);
    }

    private void ClearStagedCustomers()
    {
        if (_stagedCustomerOrder != null)
        {
            CancelCreatedCustomer(_stagedCustomerOrder);
            _stagedCustomerOrder = null;
        }
        if (_stagedThirdCustomerOrder != null)
        {
            CancelCreatedCustomer(_stagedThirdCustomerOrder);
            _stagedThirdCustomerOrder = null;
        }
    }

    private void ValidateStagedCustomer(ref CustomerOrder staged)
    {
        if (staged != null &&
            (staged.Customer == null || !staged.Customer.Exists() || staged.Customer.IsDead ||
             staged.Vehicle == null || !staged.Vehicle.Exists()))
        {
            CancelCreatedCustomer(staged);
            staged = null; // Retain request; stage a replacement when possible.
        }
    }

    private void UpdateStagedQueueTravel(ref CustomerOrder staged)
    {
        if (staged == null || staged.QueueDestination == null ||
            staged.Stage == CustomerStage.WaitingAtRoadside) return;
        staged.SecondsInStage += 1.0; // Customer visuals update at one-second intervals.
        WorkBay target = staged.QueueDestination;
        float distance = DistanceSquared(staged.Vehicle.Position, target.Position);
        if (staged.Stage == CustomerStage.DrivingToQueueSpace)
        {
            if (distance < 100f)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_PARK,
                    staged.Customer.Handle, staged.Vehicle.Handle,
                    target.Position.X, target.Position.Y, target.Position.Z,
                    target.Heading, 1, 2.5f, false);
                staged.Stage = CustomerStage.ParkingAtQueueSpace;
                staged.SecondsInStage = 0;
            }
            else if (staged.SecondsInStage > 85)
            {
                Log("Queued customer failed to reach waiting space; keeping request", null);
                CancelCreatedCustomer(staged);
                staged = null;
            }
        }
        else if (staged.Stage == CustomerStage.ParkingAtQueueSpace)
        {
            if (distance < 25f && staged.Vehicle.Speed < 1.5f)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, staged.Customer.Handle);
                staged.Stage = CustomerStage.WaitingAtRoadside;
                staged.SecondsInStage = 0;
            }
            else if (staged.SecondsInStage > 35)
            {
                if (distance < 49f)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, staged.Customer.Handle);
                    staged.Stage = CustomerStage.WaitingAtRoadside;
                    staged.SecondsInStage = 0;
                }
                else
                {
                    Log("Queued car unable to park; retaining pending customer", null);
                    CancelCreatedCustomer(staged);
                    staged = null;
                }
            }
        }
    }

    private bool QueueParkingOccupied(WorkBay spot)
    {
        if (spot == null || !spot.IsSet) return true;
        foreach (Vehicle vehicle in World.GetNearbyVehicles(spot.Position, 5f))
            if (vehicle != null && vehicle.Exists() &&
                DistanceSquared(vehicle.Position, spot.Position) < 25f) return true;
        return false;
    }

    // Vehicle slot 2 uses Queue Car 2 or the legacy road-spawn fallback.
    // Vehicle slot 3 is opt-in and needs a distinct saved Queue Car 3 spot.
    // Both are parked reservations; only the oldest can receive a live work order.
    private void SpawnRoadsideWaitingCustomer(DateTime now, bool third)
    {
        if (_activeCustomerOrders.Count == 0 || !_driveInEnabled || _waitingCustomerRequests <= (third ? 1 : 0) ||
            (third ? _stagedThirdCustomerOrder != null : _stagedCustomerOrder != null)) return;
        WorkBay spawn = GetQueueWaitingSpot(third);
        if (!spawn.IsSet || !_customerDropoff.IsSet) return;
        // Each waiting space must be distinct from drop-off and from other waiting vehicles.
        if (DistanceSquared(spawn.Position, _customerDropoff.Position) < 64f) return;
        WorkBay other = GetQueueWaitingSpot(!third);
        if (other.IsSet && DistanceSquared(spawn.Position, other.Position) < 49f) return;
        if (Game.Player.Character == null ||
            DistanceSquared(Game.Player.Character.Position, spawn.Position) <
                _queueSpawnSafetyMeters * _queueSpawnSafetyMeters) return;
        if (QueueParkingOccupied(spawn)) return;
        // Each queued car enters through the saved road spawn and drives to its
        // waiting spot. If the road spawn IS the waiting spot, stage parked there.
        bool driveToWaiting = _customerDriveSpawn.IsSet &&
            DistanceSquared(_customerDriveSpawn.Position, spawn.Position) > 64f;
        WorkBay entry = driveToWaiting ? _customerDriveSpawn : spawn;
        if (Game.Player.Character != null &&
            DistanceSquared(Game.Player.Character.Position, entry.Position) <
                _queueSpawnSafetyMeters * _queueSpawnSafetyMeters) return;
        if (driveToWaiting && QueueParkingOccupied(entry)) return;
        if (third && _stagedCustomerOrder != null &&
            _stagedCustomerOrder.Vehicle != null && _stagedCustomerOrder.Vehicle.Exists() &&
            DistanceSquared(_stagedCustomerOrder.Vehicle.Position, entry.Position) < 64f) return;
        if (!_customerPedModel.IsLoaded) { _customerPedModel.Request(); return; }
        if (!_customerVehicleModel.IsLoaded) { _customerVehicleModel.Request(); return; }
        CustomerOrder next = new CustomerOrder();
        next.Id = ++_lastCustomerOrderId;
        next.ServiceKind = _customerRandom.Next(0, 3);
        next.PartsNeeded = next.ServiceKind == 1 ? 2 : 1;
        next.Price = ServiceQuoteBaseline(next.ServiceKind);
        next.RepairSeconds = _jobSeconds * (next.ServiceKind == 1 ? 1.25 : 1.0);
        next.DroveIn = true;
        next.Stage = driveToWaiting ? CustomerStage.DrivingToQueueSpace : CustomerStage.WaitingAtRoadside;
        next.QueueDestination = spawn;
        try
        {
            next.Vehicle = World.CreateVehicle(_customerVehicleModel, entry.Position, entry.Heading);
            if (next.Vehicle == null || !next.Vehicle.Exists())
            { CancelCreatedCustomer(next); return; }
            next.Vehicle.IsPersistent = true;
            float radians = entry.Heading * (float)Math.PI / 180f;
            Vector3 roadsidePed = new Vector3(
                entry.Position.X - (float)Math.Cos(radians) * 2.2f,
                entry.Position.Y + (float)Math.Sin(radians) * 2.2f,
                entry.Position.Z + 0.1f);
            next.Customer = World.CreatePed(_customerPedModel, roadsidePed, entry.Heading);
            if (next.Customer == null || !next.Customer.Exists())
            { CancelCreatedCustomer(next); return; }
            next.Customer.IsPersistent = true;
            next.Customer.BlockPermanentEvents = true;
            next.Customer.AlwaysKeepTask = true;
            GTA.Native.Function.Call(GTA.Native.Hash.SET_PED_INTO_VEHICLE,
                next.Customer.Handle, next.Vehicle.Handle, -1);
            if (third) _stagedThirdCustomerOrder = next;
            else _stagedCustomerOrder = next;
            SyncCustomerBlip(next);
            if (driveToWaiting)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,
                    next.Customer.Handle, next.Vehicle.Handle,
                    spawn.Position.X, spawn.Position.Y, spawn.Position.Z,
                    Math.Min(_driveSpeed, 6f), CarefulDrivingStyle, 3.5f);
                Notify("Queued Car " + (third ? "3" : "2") + " driving from road spawn to its waiting space.");
            }
            else Notify("Queued Car " + (third ? "3" : "2") + " waiting at saved position (road spawn is the same spot).");
            Save();
        }
        catch (Exception ex)
        {
            Log("Stage customer in queue lane", ex);
            CancelCreatedCustomer(next);
            if (third) _stagedThirdCustomerOrder = null;
            else _stagedCustomerOrder = null;
        }
    }

    private string RoadsideQueueDiagnostic()
    {
        if (_waitingCustomerRequests == 0)
        {
            int seconds = _nextCustomerAt == DateTime.MinValue ? 0 :
                (int)Math.Max(0, (_nextCustomerAt - DateTime.UtcNow).TotalSeconds);
            return "No pending customers. Next scheduled request ~" + seconds + "s; use Schedule One for testing.";
        }
        if (!_driveInEnabled) return "Drive-in OFF: pending customers are virtual.";
        if (!_customerDriveSpawn.IsSet) return "Save Customer Road Spawn for driving arrival.";
        if (_stagedCustomerOrder != null || _stagedThirdCustomerOrder != null)
        {
            CustomerOrder next = _stagedCustomerOrder ?? _stagedThirdCustomerOrder;
            if (next.Stage == CustomerStage.DrivingToQueueSpace)
                return "Car " + next.Id + " is driving to its waiting position.";
            if (next.Stage == CustomerStage.ParkingAtQueueSpace)
                return "Car " + next.Id + " is parking in the waiting lane.";
            if (DropoffOccupied(null)) return "WAIT: Drop-off occupied by a vehicle.";
            if (FreePartsForNewOrders() < (_stagedCustomerOrder ?? _stagedThirdCustomerOrder).PartsNeeded)
                return "WAIT: Restock parts for the next queued service.";
            if (_activeCustomerOrders.Count >= Math.Min(3, _mechanics.Count))
                return "WAIT: All hired mechanic slots are handling live orders.";
            foreach (CustomerOrder order in _activeCustomerOrders)
            {
                if (NeedsDropoffSoon(order)) return "WAIT: Finished customer has priority to return their car.";
                if (IntakeInProgress(order)) return "WAIT: Prior car must be collected and clear drop-off.";
            }
            return "Staged car ready when another mechanic finishes walking to break.";
        }
        if (_customerOrder == null)
            return DropoffOccupied(null) ? "Drop-off blocked by a vehicle." : "Awaiting receptionist/spawn.";
        WorkBay spot = GetQueueWaitingSpot(false);
        if (!spot.IsSet) return "Save Road Spawn or Car 2 parking point.";
        if (DistanceSquared(spot.Position, _customerDropoff.Position) < 64f)
            return "Car 2 waiting space must be at least 8m from drop-off.";
        if (QueueParkingOccupied(spot)) return "Car 2 waiting lane is occupied; clear parked traffic.";
        if (Game.Player.Character != null &&
            DistanceSquared(Game.Player.Character.Position, spot.Position) <
                _queueSpawnSafetyMeters * _queueSpawnSafetyMeters)
            return "Player too close to car spawn: move " + _queueSpawnSafetyMeters.ToString("0") + "m away.";
        if (_waitingCustomerRequests >= 2 && !_queueCar3.IsSet)
            return "Save Car 3 position to stage a second waiting vehicle.";
        if (_waitingCustomerRequests >= 2 &&
            DistanceSquared(_queueCar3.Position, spot.Position) < 49f)
            return "Car 2 and Car 3 positions must be 7m apart.";
        if (_customerDriveSpawn.IsSet &&
            DistanceSquared(_customerDriveSpawn.Position, spot.Position) < 64f)
            return "Road spawn is beside Car 2: car will appear parked (move road spawn 8m away to drive in).";
        return "Queued car will drive from Road Spawn to waiting lane when entry is clear.";
    }

    private void ActivateRoadsideWaitingCustomer()
    {
        CustomerOrder next = _stagedCustomerOrder ?? _stagedThirdCustomerOrder;
        if (next == null || !CanStartNextIntake() ||
            next.Stage != CustomerStage.WaitingAtRoadside) return;
        if (next.Customer == null || !next.Customer.Exists() || next.Vehicle == null ||
            !next.Vehicle.Exists())
        {
            if (next == _stagedCustomerOrder) _stagedCustomerOrder = null;
            else _stagedThirdCustomerOrder = null;
            CancelCreatedCustomer(next);
            return;
        }
        // The third customer remains in their saved spot until it becomes their turn.
        if (next == _stagedCustomerOrder)
        {
            // Preserve FIFO: the third car becomes the oldest waiting reservation.
            // It remains physically parked at its original saved spot until released.
            _stagedCustomerOrder = _stagedThirdCustomerOrder;
            _stagedThirdCustomerOrder = null;
        }
        else _stagedThirdCustomerOrder = null;
        AddActiveOrder(next);
        _waitingCustomerRequests = Math.Max(0, _waitingCustomerRequests - 1);
        GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,
            next.Customer.Handle, next.Vehicle.Handle,
            _customerDropoff.Position.X, _customerDropoff.Position.Y, _customerDropoff.Position.Z,
            _driveSpeed, CarefulDrivingStyle, 5.0f);
        next.Stage = CustomerStage.DrivingToDropoff;
        next.SecondsInStage = 0;
        Notify("Queued customer #" + next.Id + " released toward drop-off.");
    }

    private void SpawnCustomerOrder(DateTime now)
    {
        if (!_customerPedModel.IsInCdImage || !_customerPedModel.IsPed ||
            !_customerVehicleModel.IsInCdImage || !_customerVehicleModel.IsVehicle)
        {
            Log("CustomerService: invalid ped/vehicle model", null);
            _nextCustomerAt = now.AddMinutes(2);
            return;
        }
        if (!_customerPedModel.IsLoaded) { _customerPedModel.Request(); return; }
        if (!_customerVehicleModel.IsLoaded) { _customerVehicleModel.Request(); return; }
        bool driveMode = _driveInEnabled && _customerDriveSpawn.IsSet;
        WorkBay spawn = driveMode ? _customerDriveSpawn : _customerDropoff;
        // Driving mode allows an observable approach: no camera-facing restriction.
        // The road-side point must still be reasonably far away to avoid pop-in.
        if (DistanceSquared(Game.Player.Character.Position, spawn.Position) <
            (driveMode ? 625f : 144f)) return;
        if (!driveMode && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPHERE_VISIBLE,
            spawn.Position.X, spawn.Position.Y, spawn.Position.Z + 1f, 2.0f)) return;

        CustomerOrder order = new CustomerOrder();
        order.Id = ++_lastCustomerOrderId;
        order.ServiceKind = _customerRandom.Next(0, 3);
        order.PartsNeeded = order.ServiceKind == 1 ? 2 : 1;
        order.Price = ServiceQuoteBaseline(order.ServiceKind);
        order.RepairSeconds = _jobSeconds * (order.ServiceKind == 1 ? 1.25 : 1.0);
        SavedRepairPromise recovered = _pendingRepairPromises.Find(p => p.Route == 1);
        if (recovered != null) ApplySavedCustomerPromise(order, recovered);
        order.DroveIn = driveMode;
        try
        {
            order.Vehicle = World.CreateVehicle(_customerVehicleModel, spawn.Position, spawn.Heading);
            if (order.Vehicle == null || !order.Vehicle.Exists())
            {
                CancelCreatedCustomer(order);
                _nextCustomerAt = now.AddSeconds(30);
                return;
            }
            order.Vehicle.IsPersistent = true;
            // In driving mode start the ped inside the car, not on the road.
            float heading = spawn.Heading * (float)Math.PI / 180f;
            Vector3 outside = new Vector3(spawn.Position.X - (float)Math.Cos(heading) * 2.2f,
                spawn.Position.Y + (float)Math.Sin(heading) * 2.2f, spawn.Position.Z + 0.1f);
            order.Customer = World.CreatePed(_customerPedModel, outside, spawn.Heading);
            if (order.Customer == null || !order.Customer.Exists())
            {
                CancelCreatedCustomer(order);
                _nextCustomerAt = now.AddSeconds(30);
                return;
            }
            order.Customer.IsPersistent = true;
            order.Customer.BlockPermanentEvents = true;
            order.Customer.AlwaysKeepTask = true;
            AddActiveOrder(order);
            if (recovered != null) _pendingRepairPromises.Remove(recovered);
            SyncCustomerBlip(order);
            if (driveMode)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.SET_PED_INTO_VEHICLE,
                    order.Customer.Handle, order.Vehicle.Handle, -1);
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,
                    order.Customer.Handle, order.Vehicle.Handle,
                    _customerDropoff.Position.X, _customerDropoff.Position.Y, _customerDropoff.Position.Z,
                    _driveSpeed, 786603, 5.0f);
                order.Stage = CustomerStage.DrivingToDropoff;
                Notify("Customer #" + order.Id + " driving to Glass Heroes.");
            }
            else
            {
                StartCustomerWalkIn(order);
                Notify("Customer #" + order.Id + " arrived: " + ServiceName(order.ServiceKind) + ".");
            }
            Save(); // Persist accepted data/IDs; actual entity handles are never saved.
        }
        catch (Exception ex)
        {
            Log("Spawn customer order", ex);
            CancelCreatedCustomer(order);
            if (recovered != null && !order.Paid && !_pendingRepairPromises.Contains(recovered))
                _pendingRepairPromises.Add(recovered);
            _activeCustomerOrders.Remove(order);
            RestoreCurrentOrder();
            _nextCustomerAt = now.AddSeconds(30);
        }
    }

    private static string ServiceName(int kind)
    {
        return kind == 1 ? "Brake Service" : kind == 2 ? "Engine Diagnostics" : "Routine Service";
    }

    private static bool CustomerArrived(CustomerOrder order, Vector3 point)
    {
        return DistanceSquared(order.Customer.Position, point) < 1.5f;
    }

    private void WalkCustomer(CustomerOrder order, Vector3 point, CustomerStage stage)
    {
        order.Stage = stage;
        order.SecondsInStage = 0;
        GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, order.Customer.Handle);
        GTA.Native.Function.Call(GTA.Native.Hash.TASK_GO_STRAIGHT_TO_COORD,
            order.Customer.Handle, point.X, point.Y, point.Z,
            _walkSpeed, -1, order.Customer.Heading, 0.35f);
    }

    private void HandleCustomerStall(CustomerOrder order, Vector3 destination)
    {
        // A stuck customer is not repeatedly rammed into the counter.
        if (_routeRecoveryEnabled && order.Customer != null && order.Customer.Exists())
        {
            order.Customer.Position = destination; // only user-saved coordinates
            order.SecondsInStage = 0;
            Log("Customer " + order.Id + " route recovered at " + FormatPosition(destination), null);
        }
        else CancelCustomerOrder("Customer path blocked", true);
    }

    // Issue #29: session-only transition data. Accepted quotes remain in RepairPromisesV1;
    // entities and deferred movement are never serialized or replayed after reload.
    private bool TeleportAttemptDue(CustomerOrder order)
    {
        if (Game.IsPaused || !_bootGameplayActivated) return false;
        int now = Game.GameTime;
        if (order.TeleportAttempted && unchecked((uint)(now - order.LastTeleportAttempt)) < 5000U) return false;
        order.TeleportAttempted = true;
        order.LastTeleportAttempt = now;
        return true;
    }

    private bool TeleportTargetAvailable(CustomerOrder order, WorkBay target, bool pickup)
    {
        try
        {
            if (target == null || !target.IsSet || order.Vehicle == null || !order.Vehicle.Exists() || order.Vehicle.IsDead) return false;
            Vector3 p = target.Position;
            if (float.IsNaN(p.X) || float.IsNaN(p.Y) || float.IsNaN(p.Z) ||
                float.IsInfinity(p.X) || float.IsInfinity(p.Y) || float.IsInfinity(p.Z) ||
                float.IsNaN(target.Heading) || float.IsInfinity(target.Heading)) return false;
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists() ||
                GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_IN_VEHICLE, player.Handle, order.Vehicle.Handle, false) ||
                DistanceSquared(player.Position, p) < 9f) return false;
            // Do not move an occupied car, even if another mod put an NPC inside it.
            if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_VEHICLE_SEAT_FREE, order.Vehicle.Handle, -1, false) ||
                GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_VEHICLE_NUMBER_OF_PASSENGERS, order.Vehicle.Handle) != 0) return false;
            foreach (CustomerOrder other in _activeCustomerOrders)
            {
                if (other == order || other.Vehicle == null || !other.Vehicle.Exists()) continue;
                if (DistanceSquared(other.Vehicle.Position, p) < 16f) return false;
                // Reserve coincident saved bays while another unpaid order is assigned,
                // even if that order's car has not reached its bay yet.
                if (!pickup && !other.Paid && other.MechanicSlot >= 0 && other.ResolvedParkingBay != null &&
                    other.ResolvedParkingBay.IsSet && DistanceSquared(other.ResolvedParkingBay.Position, p) < 16f) return false;
            }
            return !GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_POSITION_OCCUPIED,
                p.X, p.Y, p.Z + 0.5f, 2f, false, true, false, false, false, order.Vehicle.Handle, false);
        }
        catch (Exception ex) { Log("Customer transfer clearance #" + order.Id, ex); return false; }
    }

    private bool ProcessTeleportOrder(CustomerOrder order)
    {
        bool pickup = order.Stage == CustomerStage.TeleportWaitingPickup;
        if (order.Stage != CustomerStage.TeleportWaitingBay && !pickup) return false;
        if (!TeleportAttemptDue(order)) return true;
        try
        {
            if (order.Vehicle == null || !order.Vehicle.Exists() || order.Vehicle.IsDead)
            { CancelCustomerOrder("Customer vehicle unavailable during transfer", true); return true; }
            if ((pickup && !order.Paid) || (!pickup && order.Paid) || order.MechanicSlot < 0 || order.MechanicSlot >= _mechanics.Count)
                return true;
            if (!pickup && order.TeleportedToBay) { order.Stage = CustomerStage.Repairing; return true; }
            if (pickup && order.TeleportedToPickup) { ReleaseCustomerForPickup(order); return true; }
            WorkBay target = pickup ? _customerDropoff : order.ResolvedParkingBay;
            // Existing return ownership prevents intake/another return claiming drop-off.
            if ((pickup && !CanReturnCustomerCar(order)) || !TeleportTargetAvailable(order, target, pickup))
            {
                if (!order.TeleportWaitNotified)
                { order.TeleportWaitNotified = true; Notify("Order #" + order.Id + " awaiting clear, unoccupied " + (pickup ? "pickup" : "service bay") + ". Order retained."); }
                return true;
            }
            try
            {
                if (!order.TeleportPositionApplied)
                {
                    order.Vehicle.Velocity = new Vector3();
                    order.Vehicle.Position = target.Position;
                    order.TeleportPositionApplied = true;
                }
                order.Vehicle.Heading = target.Heading;
            }
            catch (Exception ex) { Log("Deferred customer vehicle transfer #" + order.Id, ex); return true; }
            order.TeleportPositionApplied = false;
            order.TeleportWaitNotified = false;
            order.SecondsInStage = 0;
            if (!pickup)
            {
                order.TeleportedToBay = true;
                order.Stage = CustomerStage.Repairing;
                // Existing employee activity walks to its separate workstation and starts
                // WORLD_HUMAN_VEHICLE_MECHANIC. Payment still waits for PhaseWorking.
            }
            else
            {
                order.TeleportedToPickup = true;
                ReleaseCustomerForPickup(order);
            }
            return true;
        }
        catch (Exception ex) { Log("Customer transfer transition #" + order.Id, ex); return true; }
    }

    private int _serviceBaySlot;
    private MenuPage _serviceBayParent;

    private void OpenServiceBayProfile()
    {
        if (_selection >= VisualBayCount) { NavigateBack(); return; }
        _serviceBaySlot = _selection;
        _serviceBayParent = _menuPage;
        _menuPage = MenuPage.ServiceBayProfile; _selection = 0;
    }

    private void ExecuteServiceBayProfile()
    {
        int slot = _serviceBaySlot;
        if (slot < 0 || slot >= VisualBayCount) { NavigateBack(); return; }
        if (_selection == 0) OpenPointEditor(_vehicleParkingBays[slot], "Service Bay " + (slot + 1) + " Vehicle Parking", -1, false);
        else if (_selection == 1) OpenPointEditor(_bays[slot], "Service Bay " + (slot + 1) + " Mechanic Work", slot, false);
        else if (_selection == 2)
            Notify("Bay " + (slot + 1) + " uses mechanic roster slot " + (slot + 1) + ": " +
                (slot < _mechanics.Count ? _mechanics[slot].Name : "not hired") + ". Existing assignments are preserved; manage staff through Mechanics.");
        else if (_selection == 3)
            ShowStatusNotice("SERVICE BAY " + (slot + 1),
                "Work: " + Status(_bays[slot]) + " " + FormatPosition(_bays[slot].Position),
                "Custom car: " + Status(_vehicleParkingBays[slot]) + " " + FormatPosition(_vehicleParkingBays[slot].Position),
                "Heading " + _vehicleParkingBays[slot].Heading.ToString("0") + " | " + (_automaticLotMode ? "AUTO LOT active: computed offset/heading" : "CUSTOM saved car target active"),
                "Mechanic slot " + (slot + 1) + " | driving " + (_vehicleTransfersEnabled ? "ON" : "OFF: teleport fallback"));
        else NavigateBack();
    }

    private void AssignCustomerMechanic(CustomerOrder order)
    {
        if (!_bootGameplayActivated || Game.IsPaused || order == null || order.Paid || order.MechanicSlot >= 0) return;
        if (order.Pricing != null ? !RepairQuoteHasStock(order.Pricing) :
            FreeStockForService(order.ServiceKind) < order.PartsNeeded) return; // No hidden stock creation.
        // Prefer a free specialist for the type of service, then any free generalist.
        // AUTO LOT accepts any configured mechanic station; CUSTOM MLO still
        // requires individual vehicle parking points where the user placed them.
        bool hasConfiguredCarBay = false;
        if (_vehicleTransfersEnabled && !_automaticLotMode)
            for (int j = 0; j < _mechanics.Count && j < VisualBayCount; j++)
                if (_vehicleParkingBays[j].IsSet) hasConfiguredCarBay = true;
        if (!_vehicleTransfersEnabled && !TeleportAttemptDue(order)) return;
        int chosen = -1;
        for (int i = 0; i < _mechanics.Count && i < VisualBayCount; i++)
        {
            Mechanic mechanic = _mechanics[i];
            if (!_bays[i].IsSet || mechanic.HasJob ||
                (!_automaticLotMode && hasConfiguredCarBay && !_vehicleParkingBays[i].IsSet) ||
                mechanic.Worker == null || !mechanic.Worker.Exists() ||
                mechanic.ArrivalPhase != PhaseAtBreak) continue;
            if (!_vehicleTransfersEnabled && !TeleportTargetAvailable(order,
                _automaticLotMode ? ComputeAutomaticBay(i) : _vehicleParkingBays[i], false)) continue;
            if (chosen < 0) chosen = i;
            if (SpecialistMatches(mechanic, order.ServiceKind)) { chosen = i; break; }
        }
        if (chosen < 0)
        {
            if (!_vehicleTransfersEnabled && !order.TeleportWaitNotified)
            { order.TeleportWaitNotified = true; Notify("Order #" + order.Id + " waiting for an available mechanic and clear configured service bay. Order retained."); }
            return;
        }
        Mechanic assigned = _mechanics[chosen];
        // Freeze the exact parts allocation and current billable duration BEFORE
        // reserving the worker. Existing service + expertise is a total floor.
        double multiplier = Math.Max(0.55, 1.0 - (MechanicLevel(assigned) - 1) * 0.035 -
            (SpecialistMatches(assigned, order.ServiceKind) ? 0.15 : 0));
        if (_toolSetPurchased) multiplier *= 0.93;
        if (_diagnosticScannerPurchased && order.ServiceKind == 2) multiplier *= 0.90;
        long premium = (MechanicLevel(assigned) - 1) * 20L +
            (SpecialistMatches(assigned, order.ServiceKind) ? 75L : 0L);
        RepairQuote quote = order.Pricing;
        if (quote == null && !TryCreateRepairQuote(order.ServiceKind, order.PartsNeeded, assigned,
            order.RepairSeconds * multiplier, order.Price, premium, out quote)) return;
        quote.OrderId = order.Id;
        long previousPrice = order.Price; double previousSeconds = order.RepairSeconds;
        RepairQuote previousQuote = order.Pricing; CustomerStage previousStage = order.Stage;
        double previousWork = assigned.WorkSeconds, previousIdle = assigned.IdleSeconds;
        bool previousHandoff = order.VehicleHandoff, previousAutomatic = order.AutomaticLot;
        WorkBay previousBay = order.ResolvedParkingBay; double previousStageSeconds = order.SecondsInStage;
        order.Pricing = quote; order.Price = quote.Total; order.RepairSeconds = quote.BillableSeconds;
        assigned.HasJob = true;
        assigned.WorkSeconds = order.RestoredPromise ? Math.Min(order.RepairSeconds, order.ResumeWorkSeconds) : 0;
        assigned.IdleSeconds = 0;
        order.MechanicSlot = chosen;
        order.AutomaticLot = _automaticLotMode;
        order.ResolvedParkingBay = order.AutomaticLot ? ComputeAutomaticBay(chosen) :
            _vehicleParkingBays[chosen];
        order.VehicleTeleport = !_vehicleTransfersEnabled;
        if (order.VehicleTeleport) order.ResolvedParkingBay = new WorkBay { IsSet = order.ResolvedParkingBay.IsSet,
            Position = order.ResolvedParkingBay.Position, Heading = order.ResolvedParkingBay.Heading };
        order.VehicleHandoff = _vehicleTransfersEnabled && order.ResolvedParkingBay.IsSet;
        order.Stage = order.VehicleTeleport ? CustomerStage.TeleportWaitingBay : order.VehicleHandoff ? CustomerStage.MechanicWalkingToPickup : CustomerStage.Repairing;
        order.SecondsInStage = 0;
        if (!Save())
        {
            assigned.HasJob = false; assigned.WorkSeconds = previousWork; assigned.IdleSeconds = previousIdle;
            order.MechanicSlot = -1; order.Price = previousPrice; order.RepairSeconds = previousSeconds;
            order.Pricing = previousQuote; order.Stage = previousStage; order.SecondsInStage = previousStageSeconds;
            order.VehicleTeleport = false;
            order.VehicleHandoff = previousHandoff; order.AutomaticLot = previousAutomatic; order.ResolvedParkingBay = previousBay;
            return; // No accepted promise or vehicle task before a durable snapshot.
        }
        order.TeleportWaitNotified = false;
        if (order.VehicleHandoff)
        {
            // Backtrack the SAME configurable Spawn -> Break route used on arrival.
            // Important for MLO doorways: direct current-position -> car cuts walls.
            // Default with 0/5 points is still a single direct target.
            List<Vector3> walkingRoute = BuildMechanicPickupExitRoute(order.Vehicle.Position);
            BeginVehicleWalk(order, assigned, walkingRoute);
            Notify(assigned.Name + " is collecting customer vehicle for Bay " + (chosen + 1) + ".");
        }
        else if (_vehicleTransfersEnabled)
            Notify("Custom MLO car bay " + (chosen + 1) + " not set; using legacy repair flow.");
        Notify(RepairQuoteBreakdown(order.Pricing));
        Notify("Order #" + order.Id + " -> " + assigned.Name + " (Lvl " + MechanicLevel(assigned) +
            ") | quote $" + order.Price + " | " + order.PartsNeeded + " parts.");
    }

    private void ProcessCustomerRepair(double seconds)
    {
        foreach (CustomerOrder order in _activeCustomerOrders.ToArray())
        {
            if (!_activeCustomerOrders.Contains(order)) continue;
            _customerOrder = order;
            ProcessOneCustomerRepair(order, seconds);
        }
        RestoreCurrentOrder();
    }

    private void ProcessOneCustomerRepair(CustomerOrder order, double seconds)
    {
        if (order == null || order.Paid || seconds <= 0 || double.IsNaN(seconds) || double.IsInfinity(seconds) || order.Stage != CustomerStage.Repairing ||
            order.MechanicSlot < 0 || order.MechanicSlot >= _mechanics.Count) return;
        Mechanic mechanic = _mechanics[order.MechanicSlot];
        if (!mechanic.HasJob || mechanic.ArrivalPhase == PhaseRouteBlocked)
        {
            CancelCustomerOrder("Assigned mechanic unavailable", true);
            return;
        }
        // Never pay for an order until the spawned mechanic reaches the bay.
        if (mechanic.Worker == null || !mechanic.Worker.Exists() ||
            mechanic.ArrivalPhase != PhaseWorking ||
            (order.VehicleHandoff && DistanceSquared(order.Vehicle.Position,
                order.ResolvedParkingBay.Position) > 25f)) return;
        mechanic.WorkSeconds = Math.Min(order.RepairSeconds, mechanic.WorkSeconds + seconds);
        if (mechanic.WorkSeconds < order.RepairSeconds) return;
        // OFF-mode entity/placement validation runs only at the completed-work
        // boundary, at most once per five game seconds if it must wait.
        if (order.VehicleTeleport && (!TeleportAttemptDue(order) || !order.TeleportedToBay ||
            order.Vehicle == null || !order.Vehicle.Exists() || order.Vehicle.IsDead ||
            DistanceSquared(order.Vehicle.Position, order.ResolvedParkingBay.Position) > 25f)) return;
        if (order.Pricing == null && !TryRetainLegacyRepairQuote(order.ServiceKind, order.PartsNeeded,
            mechanic, order.RepairSeconds, order.Price, order.Id, q => order.Pricing = q)) return;
        if (!TrySettleRepair(order.Pricing, mechanic, order, null)) return;
        RecordCustomerInvoice(order);
        RecordRepairPricing(order.Pricing, "CUSTOMER");
        Notify("Order #" + order.Id + " complete: +$" + order.Price +
            " | " + order.PartsNeeded + " parts used.");
        if (order.Customer == null || !order.Customer.Exists())
        { CancelCustomerOrder("Customer entity lost after payment", false); return; }
        if (order.VehicleTeleport)
        {
            order.Stage = CustomerStage.TeleportWaitingPickup;
            order.SecondsInStage = 0;
            order.TeleportAttempted = false;
            return;
        }
        if (order.VehicleHandoff)
        {
            // Other customers may now occupy drop-off. Hold at the service bay
            // until it clears, so returning cars never drive into the arrival line.
            if (CanReturnCustomerCar(order)) StartReturnVehicleWalk(order);
            else
            {
                order.Stage = CustomerStage.MechanicWaitingToReturn;
                order.SecondsInStage = 0;
                // Worker is still at the bay. Resume walking when the spot clears.
                Notify(mechanic.Name + " holding completed car until drop-off is clear.");
            }
        }
        else ReleaseCustomerForPickup(order);
    }

    private void RecordCustomerInvoice(CustomerOrder order)
    {
        try
        {
            if (!File.Exists(InvoiceFile))
                File.AppendAllText(InvoiceFile, "RecordedUTC,OrderID,Service,Mechanic,Revenue,PartsUsed\r\n");
            File.AppendAllText(InvoiceFile, DateTime.UtcNow.ToString("s") + "," +
                order.Id + "," + ServiceName(order.ServiceKind) + "," +
                (_mechanics[order.MechanicSlot].Name.Replace(",", " ")) + "," + order.Price + "," +
                order.PartsNeeded + "\r\n");
        }
        catch (Exception ex) { Log("Customer invoice write", ex); }
    }

    private static void CancelCreatedCustomer(CustomerOrder order)
    {
        if (order == null) return;
        try { if (order.CustomerBlip != null && order.CustomerBlip.Exists()) order.CustomerBlip.Delete(); }
        catch { }
        order.CustomerBlip = null;
        try { if (order.Customer != null && order.Customer.Exists()) order.Customer.Delete(); }
        catch { }
        try { if (order.Vehicle != null && order.Vehicle.Exists()) order.Vehicle.Delete(); }
        catch { }
    }

    private void CancelCustomerOrder(string reason, bool notify)
    {
        if (_customerOrder == null) return;
        CustomerOrder order = _customerOrder;
        RememberCancelledRepair(order);
        if (order.MechanicSlot >= 0 && order.MechanicSlot < _mechanics.Count)
        {
            Mechanic mechanic = _mechanics[order.MechanicSlot];
            mechanic.HasJob = false;
            mechanic.WorkSeconds = 0;
            mechanic.IdleSeconds = 0;
            // Keep a script-owned worker from remaining inside a car that is about to be deleted.
            if (order.VehicleHandoff && MechanicOnVehicleTask(order.MechanicSlot))
            {
                DismissWorker(mechanic); // Removes our own ped and its interrupted task safely.
                mechanic.NextSpawnAttempt = DateTime.UtcNow.AddSeconds(15);
            }
        }
        CancelCreatedCustomer(order);
        _activeCustomerOrders.Remove(order);
        RestoreCurrentOrder();
        // Queued requests can dispatch next update once the drop-off is clear.
        // Picking up a paid-for vehicle must not erase the PAID result.
        if (!order.Paid)
        {
            _lastOrderOutcome = "#" + order.Id + " canceled: " + reason;
            Save();
        }
        if (notify) Notify("Customer order canceled: " + reason);
        Log("Customer order canceled: " + reason, null);
    }

    private void ProcessEconomy(double seconds)
    {
        if (!_clockedIn || Game.IsPaused || seconds <= 0 || double.IsNaN(seconds) || double.IsInfinity(seconds)) return;
        bool restored = RestoreSavedNonphysicalRepairs();
        foreach (Mechanic mechanic in _mechanics)
        {
            if (mechanic.UnpaidWages <= 0) continue;
            mechanic.WageUnpaidSeconds += seconds;
            int slot = _mechanics.IndexOf(mechanic);
            if (mechanic.WageUnpaidSeconds >= _payrollDueSeconds &&
                !_mechanicOverdueNotified.Contains(slot))
            {
                _mechanicOverdueNotified.Add(slot);
                Notify(mechanic.Name + " has overdue wages. Check Business Management > Expenses & Bills.");
            }
        }
        if (_receptionWagesDue > 0)
        {
            _receptionWageUnpaidSeconds += seconds;
            if (_receptionWageUnpaidSeconds >= _payrollDueSeconds && !_receptionOverdueNotified)
            {
                _receptionOverdueNotified = true;
                Notify("Receptionist wages are overdue. See Expenses & Bills.");
            }
        }
        if (_managerWagesDue > 0) _managerWageAgeSeconds += seconds;
        if (_rentUtilitiesDue > 0) _rentUnpaidSeconds += seconds;
        _payrollClock += seconds;
        _overheadClock += seconds;
        bool changed = restored;
        if (_payrollClock >= _wagePeriodSeconds)
        {
            _payrollClock -= _wagePeriodSeconds;
            long expense = 0;
            if (_managerTier >= 0)
            {
                int wage = ManagerPeriodWage();
                _managerWagesDue += wage;
                expense += wage;
            }
            if (_receptionistHired)
            {
                _receptionWagesDue += _receptionistWage + _receptionistWageBonus;
                expense += _receptionistWage + _receptionistWageBonus;
            }
            foreach (Mechanic mechanic in _mechanics)
            {
                long wage = MechanicWage(mechanic);
                mechanic.UnpaidWages += wage;
                expense += wage;
            }
            changed = true;
            if (expense > 0) Notify("Payroll invoice accrued: $" + expense +
                " outstanding. Pay under Management > Expenses & Bills.");
        }
        if (_overheadClock >= _overheadPeriodSeconds)
        {
            _overheadClock -= _overheadPeriodSeconds;
            _rentUtilitiesDue += ManagerAdjustedExpense(_rentAndUtilities);
            changed = true;
            if (_rentAndUtilities > 0) Notify("Rent & utilities bill accrued: $" +
                _rentAndUtilities + " outstanding.");
        }

        // A manager runs distant work through the separate Game.GameTime ledger.
        // Do not also run physical repairs/background jobs in the same tick.
        // The normal customer visual radius is smaller than the manager's remote
        // takeover radius. Hold assigned repairs in this intermediate band so
        // ordinary offscreen cleanup does not cancel the invoice before takeover.
        bool holdingForManager = _managerTier >= 0 && !_remoteMode &&
            Game.Player.Character != null && _placed &&
            DistanceSquared(Game.Player.Character.Position, _location) >
                _spawnDistance * _spawnDistance;
        if (_remoteMode || _remoteJobs.Count > 0 || holdingForManager)
        { if (changed) Save(); return; }

        // Mutually exclusive scheduling: while customer mode is ON, do not
        // generate free-money background jobs in addition to customer invoices.
        if (_customerServiceEnabled)
        {
            ProcessCustomerRepair(seconds);
            if (changed) Save();
            return;
        }

        // A job is assigned only after a break between repairs. Active jobs
        // reserve a part so multiple mechanics cannot spend the same stock.
        int reservedParts = 0;
        foreach (Mechanic employee in _mechanics)
            if (employee.HasJob) reservedParts++;
        for (int i = 0; i < _mechanics.Count; i++)
        {
            Mechanic employee = _mechanics[i];
            if (employee.ArrivalPhase == PhaseRouteBlocked) continue;
            if (!employee.HasJob)
            {
                if (_parts <= reservedParts) { employee.IdleSeconds = 0; continue; }
                employee.IdleSeconds += seconds;
                // Slight staggering prevents all mechanics starting together.
                if (employee.IdleSeconds < _betweenJobsSeconds + i * 10) continue;
                RepairQuote quote;
                if (!TryCreateRepairQuote(-1, 1, employee, _jobSeconds, _jobRevenue, 0, out quote)) continue;
                quote.OrderId = ++_lastCustomerOrderId;
                employee.Pricing = quote;
                employee.HasJob = true;
                employee.WorkSeconds = 0;
                employee.IdleSeconds = 0;
                reservedParts++;
                changed = true; // Accepted background quote is captured by the save below.
                Notify("Mechanic " + (i + 1) + " assigned a new repair order.");
                continue;
            }

            // When an NPC is visibly traveling, the job timer waits until the
            // employee reaches the workstation and starts its repair scenario.
            bool traveling = employee.Worker != null && employee.Worker.Exists() &&
                employee.ArrivalPhase != PhaseWorking;
            if (traveling) continue;
            if (employee.Pricing == null && !TryRetainLegacyRepairQuote(-1, 1, employee, _jobSeconds,
                _jobRevenue, _lastCustomerOrderId, q => employee.Pricing = q)) continue;
            employee.WorkSeconds = Math.Min(employee.Pricing.BillableSeconds, employee.WorkSeconds + seconds);
            if (employee.WorkSeconds >= employee.Pricing.BillableSeconds)
            {
                RepairQuote paidQuote = employee.Pricing;
                if (!TrySettleRepair(paidQuote, employee, null, null)) continue;
                reservedParts--;
                RecordRepairPricing(paidQuote, "BACKGROUND");
                changed = true;
                Notify(employee.Name + " completed repair: +$" +
                    paidQuote.Total + " (1 part used) | " + RepairQuoteBreakdown(paidQuote));
            }
        }
        if (changed) Save();
    }


    // ===== v0.3.2.8 Manager & game-time-only remote workshop =====
    private int ManagerLevel() { return _managerTier < 0 ? 0 : Math.Min(10, _managerTier + 1 + _managerXp / 200); }
    private int ManagerDiscountPercent()
    { return _managerTier < 0 ? 0 : Math.Min(15, (_managerTier == 0 ? 3 : _managerTier == 1 ? 6 : 10) + _managerXp / 300); }
    private int ManagerCandidateCost(int tier) { return tier == 0 ? 800 : tier == 1 ? 1600 : 2800; }
    private int ManagerPeriodWage() { return _managerTier < 0 ? 0 : 200 + _managerTier * 125; }
    private long ManagerAdjustedExpense(long gross)
    {
        if (gross <= 0) return gross;
        long saved = gross * ManagerDiscountPercent() / 100;
        _managerSavings += saved;
        return gross - saved;
    }
    // Payroll remains fully owed to employees. Manager efficiency is a business
    // rebate at PAYMENT, never a reduction of the worker's contractual invoice.
    private void GiveManagerPayrollRebate(long gross)
    {
        if (_managerTier < 0 || gross <= 0) return;
        long credit = gross * ManagerDiscountPercent() / 100;
        _cash += credit;
        _financeRebateIncome += credit;
        _managerSavings += credit;
    }
    private void ExecuteManagerManagement()
    {
        switch (_selection)
        {
            case 0: _menuPage = MenuPage.ManagerProfile; _selection = 0; break;
            case 1: _menuPage = MenuPage.HireManagerRoster; _selection = 0; break;
            case 2: _managerPathsFromManagement = true; _menuPage = MenuPage.ManagerWalkingPaths; _selection = 0; break;
            default: NavigateBack(); break;
        }
    }
    private void ExecuteManagerProfile()
    {
        switch (_selection)
        {
            case 0:
                ShowStatusNotice("BUSINESS MANAGER",
                    _managerTier < 0 ? "No manager hired" : _managerName + " | level " + ManagerLevel(),
                    "Discount " + ManagerDiscountPercent() + "% | Pay/period $" + ManagerPeriodWage(),
                    "Manager wages due $" + _managerWagesDue.ToString("N0") +
                        " | Business savings $" + _managerSavings.ToString("N0"),
                    "Away mode " + (_remoteMode ? "ACTIVE" : "INACTIVE") +
                        " | Active remote orders " + _remoteJobs.Count,
                    "LIFETIME remote jobs " + _remoteJobsCompleted + " | gross sales $" + _remoteSales.ToString("N0") +
                        " | Parts used " + _remotePartsConsumed);
                break;
            case 1:
                if (_managerTier < 0) { Notify("Hire a manager first."); break; }
                long managerTrainingQuote = 750L * (100 - ManagerDiscountPercent()) / 100;
                if (_cash < managerTrainingQuote)
                { Notify("Manager training requires $" + managerTrainingQuote.ToString("N0") + "."); break; }
                long managerTrainingActual = ManagerAdjustedExpense(750);
                _cash -= managerTrainingActual; _trainingSpend += managerTrainingActual; _managerXp += 40;
                Save(); Notify(_managerName + " trained: level " + ManagerLevel() + ", " + ManagerDiscountPercent() + "% expense savings.");
                break;
            case 2: PayInvoice(ref _managerWagesDue, true, "Manager wages"); break;
            case 3:
                if (_managerTier < 0) { Notify("No manager employed."); break; }
                if (_remoteMode || _remoteJobs.Count > 0)
                { Notify("Return to the shop and complete remote jobs before dismissing the manager."); break; }
                _formerStaffWagesDue += _managerWagesDue; _managerWagesDue = 0;
                DismissVisualManager();
                _managerTier = -1; _managerXp = 0; _managerName = "";
                Save(); Notify("Manager dismissed. Any unpaid wages moved to former-staff bills."); break;
            default: NavigateBack(); break;
        }
    }
    private void ExecuteHireManagerRoster()
    {
        if (_selection >= 3) { NavigateBack(); return; }
        if (_managerTier >= 0) { Notify("Dismiss your current manager before hiring a replacement."); return; }
        int price = ManagerCandidateCost(_selection);
        if (_cash < price) { Notify("Hiring this manager requires $" + price); return; }
        _cash -= price; _hireSpend += price;
        _managerTier = _selection;
        _managerXp = _selection * 100;
        _managerName = _selection == 0 ? "Avery Price" : _selection == 1 ? "Blake Morgan" : "Casey Bennett";
        _menuPage = MenuPage.ManagerManagement; _selection = 0;
        Save(); Notify(_managerName + " hired. Discount " + ManagerDiscountPercent() +
            "%. Shop can operate while you are away and the game is running.");
    }
    private void EnterRemoteOperations()
    {
        if (_remoteMode || _managerTier < 0 || !_clockedIn) return;
        _remoteMode = true;
        _remoteIdleGameSeconds = 0;
        foreach (CustomerOrder live in _activeCustomerOrders.ToArray())
        {
            if (!live.Paid && live.MechanicSlot >= 0 && live.MechanicSlot < _mechanics.Count)
            {
                _remoteJobs.Add(new RemoteJob {
                    MechanicSlot = live.MechanicSlot, OrderId = live.Id,
                    ServiceKind = live.ServiceKind, PartsNeeded = live.PartsNeeded,
                    Price = live.Price, Pricing = live.Pricing,
                    RemainingGameSeconds = Math.Max(15d, live.RepairSeconds - _mechanics[live.MechanicSlot].WorkSeconds)
                });
            }
        }
        CancelAllCustomerOrders("Manager took over offsite");
        ClearStagedCustomers();
        _waitingCustomerRequests = 0;
        foreach (RemoteJob job in _remoteJobs)
            if (job.MechanicSlot >= 0 && job.MechanicSlot < _mechanics.Count)
                _mechanics[job.MechanicSlot].HasJob = true;
        _nextCustomerAt = DateTime.UtcNow.AddSeconds(_customerIntervalSeconds);
        Save(); Notify("Manager took over: remote operations enabled. Only in-game play advances jobs.");
    }
    private void LeaveRemoteOperations()
    {
        if (!_remoteMode) return;
        _remoteMode = false;
        Save(); Notify("Returned to Glass Heroes. LIFETIME manager report (not shift cash): " +
            _remoteJobsCompleted + " completed / $" + _remoteSales.ToString("N0") + " sales.");
    }
    private void ProcessRemoteOperations(double gameSeconds)
    {
        // Always run on GTA Game.GameTime deltas, never DateTime.UtcNow or save timestamps.
        if (Game.IsPaused || gameSeconds <= 0 || double.IsNaN(gameSeconds) || double.IsInfinity(gameSeconds) || _managerTier < 0 || !_clockedIn) return;
        bool changed = false;
        foreach (RemoteJob job in _remoteJobs.ToArray())
        {
            if (job.MechanicSlot < 0 || job.MechanicSlot >= _mechanics.Count)
            { _remoteJobs.Remove(job); continue; }
            job.RemainingGameSeconds = Math.Max(0, job.RemainingGameSeconds - gameSeconds);
            if (job.RemainingGameSeconds > 0) continue;
            Mechanic mechanic = _mechanics[job.MechanicSlot];
            if (job.Pricing == null && !TryRetainLegacyRepairQuote(job.ServiceKind, job.PartsNeeded,
                mechanic, Math.Max(0, _jobSeconds), job.Price, job.OrderId, q => job.Pricing = q)) continue;
            if (!TrySettleRepair(job.Pricing, mechanic, null, job)) continue;
            AppendRemoteInvoice(job, mechanic);
            RecordRepairPricing(job.Pricing, "REMOTE");
            changed = true;
        }
        if (_remoteMode && _remoteJobs.Count == 0 && _customerServiceEnabled &&
            _receptionistHired && _mechanics.Count > 0)
        {
            _remoteIdleGameSeconds += gameSeconds;
            int duration = Math.Max(55, _remoteJobGameSeconds - ManagerLevel() * 12);
            if (_remoteIdleGameSeconds >= duration)
            {
                _remoteIdleGameSeconds = 0;
                int kind = (int)(_remoteJobsCompleted % 3);
                int need = 1; // one virtual job at a time; avoids inventory overcommit.
                int slot = -1;
                for (int i = 0; i < _mechanics.Count; i++)
                    if (!_mechanics[i].HasJob) { slot = i; break; }
                if (slot >= 0 && (_specialStock[kind] + _parts) >= need)
                {
                    Mechanic staff = _mechanics[slot];
                    double repairSeconds = Math.Max(20, _jobSeconds -
                        (ManagerLevel() - 1) * 4 - (MechanicLevel(staff) - 1) * 3);
                    RepairQuote quote;
                    if (!TryCreateRepairQuote(kind, need, staff, repairSeconds,
                        _jobRevenue, (MechanicLevel(staff) - 1) * 20L, out quote))
                    { if (changed) Save(); return; }
                    staff.HasJob = true;
                    _lastCustomerOrderId++;
                    quote.OrderId = _lastCustomerOrderId;
                    _remoteJobs.Add(new RemoteJob {
                        MechanicSlot = slot, OrderId = _lastCustomerOrderId,
                        ServiceKind = kind, PartsNeeded = need,
                        Price = quote.Total, Pricing = quote,
                        RemainingGameSeconds = repairSeconds
                    });
                    changed = true;
                }
            }
        }
        if (changed) Save();
    }
    private void AppendRemoteInvoice(RemoteJob job, Mechanic mechanic)
    {
        try
        {
            if (!File.Exists(InvoiceFile))
                File.AppendAllText(InvoiceFile, "RecordedUTC,OrderID,Service,Mechanic,Revenue,PartsUsed\r\n");
            File.AppendAllText(InvoiceFile, DateTime.UtcNow.ToString("s") + "," + job.OrderId + "," +
                ServiceName(job.ServiceKind) + " REMOTE," + mechanic.Name.Replace(",", " ") + "," +
                job.Price + "," + job.PartsNeeded + "\r\n");
        }
        catch (Exception ex) { Log("Remote invoice", ex); }
    }

    // ===== Registered player wholesale cargo vehicle =====
    private bool HasWholesaleVehicle() { return _wholesaleVehicleModel != 0 && !String.IsNullOrWhiteSpace(_wholesaleVehiclePlate); }
    private static string WholesalePlate(Vehicle vehicle)
    {
        try { return GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, vehicle.Handle).Trim(); }
        catch { return ""; }
    }
    private bool MatchesWholesaleVehicle(Vehicle vehicle)
    {
        return vehicle != null && vehicle.Exists() && HasWholesaleVehicle() &&
            vehicle.Model.Hash == _wholesaleVehicleModel &&
            String.Equals(WholesalePlate(vehicle), _wholesaleVehiclePlate, StringComparison.OrdinalIgnoreCase);
    }
    private Vehicle FindAssignedWholesaleVehicle(Vector3 center, float radius)
    {
        if (!HasWholesaleVehicle()) return null;
        DateTime now = DateTime.UtcNow;
        // Model and vehicle handle are inexpensive; read its plate at most every 3 s.
        if (_cachedWholesaleVehicle != null && _cachedWholesaleVehicle.Exists() &&
            _cachedWholesaleVehicle.Model.Hash == _wholesaleVehicleModel &&
            DistanceSquared(_cachedWholesaleVehicle.Position, center) <= radius * radius)
        {
            if (now >= _nextWholesalePlateCheck)
            {
                _wholesaleCachedPlateValid = MatchesWholesaleVehicle(_cachedWholesaleVehicle);
                _nextWholesalePlateCheck = now.AddSeconds(3);
            }
            if (_wholesaleCachedPlateValid) return _cachedWholesaleVehicle;
        }
        // A different region must be searchable immediately, despite a previous miss.
        if (now < _nextWholesaleVehicleLookup &&
            DistanceSquared(center, _lastWholesaleLookupCenter) < 625f) return null;
        _nextWholesaleVehicleLookup = now.AddMilliseconds(750);
        _lastWholesaleLookupCenter = center;
        Ped player = Game.Player.Character;
        if (player != null && player.Exists() && player.IsInVehicle() &&
            MatchesWholesaleVehicle(player.CurrentVehicle) &&
            DistanceSquared(player.CurrentVehicle.Position, center) <= radius * radius)
        {
            _cachedWholesaleVehicle = player.CurrentVehicle;
            _wholesaleCachedPlateValid = true;
            _nextWholesalePlateCheck = now.AddSeconds(3);
            return _cachedWholesaleVehicle;
        }
        foreach (Vehicle vehicle in World.GetNearbyVehicles(center, radius))
        {
            if (!MatchesWholesaleVehicle(vehicle)) continue;
            _cachedWholesaleVehicle = vehicle;
            _wholesaleCachedPlateValid = true;
            _nextWholesalePlateCheck = now.AddSeconds(3);
            return vehicle;
        }
        _cachedWholesaleVehicle = null;
        _wholesaleCachedPlateValid = false;
        return null;
    }

    private void SupplierCheckpoint(string name)
    {
        // Only state transitions; not every frame. Stays useful after a native crash.
        DateTime now = DateTime.UtcNow;
        if ((now - _lastSupplierCheckpoint).TotalMilliseconds < 100) return;
        _lastSupplierCheckpoint = now;
        Log("Wholesale checkpoint: " + name + " | stage=" + _supplierVisualStage +
            " | loaded=" + _wholesaleCargoLoaded + " | carrying=" + _supplierPlayerCarrying, null);
    }

    private void AttachWholesaleBoxToVehicle(Vehicle vehicle)
    {
        if (vehicle == null || !vehicle.Exists()) return;
        if (_supplierCrate == null || !_supplierCrate.Exists())
            _supplierCrate = CreateFixedEquipmentProp("prop_cs_cardbox_01",
                vehicle.Position + new Vector3(0f, 0f, 1f), vehicle.Heading, false);
        if (_supplierCrate == null || !_supplierCrate.Exists()) return;
        // Visible cargo: for most cars the rear interior / luggage compartment.
        // Exact coordinates vary by vehicle model; a future vehicle-specific
        // offset table could improve the fit for pickups, vans and sedans.
        _supplierCrate.IsPositionFrozen = false;
        GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_COLLISION,
            _supplierCrate.Handle, false, false);
        GTA.Native.Function.Call(GTA.Native.Hash.ATTACH_ENTITY_TO_ENTITY,
            _supplierCrate.Handle, vehicle.Handle, 0,
            0f, -1.15f, 0.45f, 0f, 0f, 0f,
            false, false, false, false, 2, true);
    }

    private void RefreshWholesaleCargoVisual()
    {
        if (!_wholesaleCargoLoaded || _deliverySupplier != 0 || _deliveryKind == -2) return;
        Ped player = Game.Player.Character;
        if (player == null || !player.Exists()) return;
        Vehicle vehicle = FindAssignedWholesaleVehicle(player.Position, 55f);
        if (vehicle == null) { DeleteOwnedProp(ref _supplierCrate); return; }
        if (_supplierCrate == null || !_supplierCrate.Exists())
            AttachWholesaleBoxToVehicle(vehicle);
    }

    private Vector3 WarehousePackagePosition()
    {
        Vector3 floor = _wholesaleWarehousePallet != null && _wholesaleWarehousePallet.Exists() ?
            _wholesaleWarehousePallet.Position : _wholesaleWarehousePickup.Position;
        return floor + new Vector3(0f, 0f, 0.28f);
    }

    // Only one pallet spawned near the live warehouse pickup. Never persists.
    private void UpdateWholesalePalletVisual()
    {
        Ped player = Game.Player.Character;
        bool show = _deliveryKind != -2 && _deliverySupplier == 0 &&
            _deliveryRemainingGameSeconds <= 0 && _wholesaleWarehousePickup.IsSet &&
            _supplierVisualStage == 7 && !_wholesaleCargoLoaded && !_supplierPlayerCarrying &&
            player != null && player.Exists() &&
            DistanceSquared(player.Position, _wholesaleWarehousePickup.Position) < 6400f;
        if (!show)
        {
            DeleteOwnedProp(ref _wholesaleWarehousePallet);
            // This crate belongs to this pickup only; never delete carried,
            // vehicle-attached or non-wholesale delivery cargo here.
            if (_deliverySupplier == 0 && _supplierVisualStage == 7 &&
                !_wholesaleCargoLoaded && !_supplierPlayerCarrying)
                DeleteOwnedProp(ref _supplierCrate);
            _warehouseVisualAttempts = 0;
            _nextWarehouseVisualAttempt = DateTime.MinValue;
            return;
        }
        bool palletExists = _wholesaleWarehousePallet != null && _wholesaleWarehousePallet.Exists();
        bool boxExists = _supplierCrate != null && _supplierCrate.Exists();
        if (palletExists && boxExists) return;
        if (!TryBeginWarehouseVisualAttempt(DateTime.UtcNow)) return;
        if (!palletExists) _wholesaleWarehousePallet = CreateWarehousePallet();
        if (!boxExists && _wholesaleWarehousePallet != null && _wholesaleWarehousePallet.Exists())
        {
            _supplierCrate = CreateFixedEquipmentProp("prop_cs_cardbox_01",
                WarehousePackagePosition(), _wholesaleWarehousePickup.Heading, false);
            if (_supplierCrate != null && _supplierCrate.Exists())
            {
                GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_COLLISION,
                    _supplierCrate.Handle, false, false);
                _supplierCrate.IsPositionFrozen = true;
            }
        }
    }

    private bool TryBeginWarehouseVisualAttempt(DateTime now)
    {
        if (now < _nextWarehouseVisualAttempt) return false;
        // Five attempts per batch, then a longer cooldown. A transient model
        // failure must not permanently hide a ready package until stream-out.
        if (_warehouseVisualAttempts >= 5) _warehouseVisualAttempts = 0;
        _warehouseVisualAttempts++;
        _nextWarehouseVisualAttempt = now.AddSeconds(_warehouseVisualAttempts >= 5 ? 30 : 3);
        return true;
    }

    private Prop CreateWarehousePallet()
    {
        Prop pallet = CreateFixedEquipmentProp("prop_pallet_02a",
            _wholesaleWarehousePickup.Position, _wholesaleWarehousePickup.Heading, true);
        if (pallet == null)
            pallet = CreateFixedEquipmentProp("prop_pallet_01a",
                _wholesaleWarehousePickup.Position, _wholesaleWarehousePickup.Heading, true);
        return pallet;
    }

    private void DrawWholesalePackageMarker()
    {
        if (_deliveryKind == -2 || _deliverySupplier != 0 || _deliveryRemainingGameSeconds > 0 ||
            _supplierPlayerCarrying || _wholesaleCargoLoaded || _wholesaleCargoUnloaded ||
            !_wholesaleWarehousePickup.IsSet || _supplierVisualStage != 7) return;
        Ped player = Game.Player.Character;
        if (player == null || !player.Exists() ||
            DistanceSquared(player.Position, _wholesaleWarehousePickup.Position) > 2025f) return;
        Vector3 p = _wholesaleWarehousePickup.Position;
        // Small package locator on the ground, matching shop placement markers.
        World.DrawMarker((MarkerType)1, new Vector3(p.X,p.Y,p.Z - 0.035f),
            new Vector3(), new Vector3(), new Vector3(0.65f,0.65f,0.075f),
            Color.FromArgb(190, 255, 199, 64));
    }
    private void AssignWholesaleVehicle()
    {
        if (_deliveryKind != -2 || _wholesaleCargoLoaded)
        { Notify("Finish your pending supplier shipment before changing the assigned vehicle."); return; }
        Ped player = Game.Player.Character;
        if (player == null || !player.Exists() || !player.IsInVehicle())
        { Notify("Sit in the pickup vehicle you want to register, then select Assign Pickup Vehicle."); return; }
        Vehicle vehicle = player.CurrentVehicle;
        if (vehicle == null || !vehicle.Exists()) return;
        string plate = WholesalePlate(vehicle);
        if (String.IsNullOrWhiteSpace(plate)) { Notify("Vehicle plate unavailable; assignment failed."); return; }
        _wholesaleVehiclePlate = plate;
        _wholesaleVehicleModel = vehicle.Model.Hash;
        _cachedWholesaleVehicle = vehicle;
        _wholesaleCachedPlateValid = true;
        _nextWholesalePlateCheck = DateTime.UtcNow.AddSeconds(3);
        Save(); Notify("Wholesale pickup vehicle assigned: " + plate + ". Use THIS vehicle at the warehouse.");
    }

    // No Tick work: manual preview/confirmation only. No extra retail markup.
    private string PartsSalePreview(int kind)
    {
        if (kind < -1 || kind > 2) return "INVALID";
        int free = UnreservedRepairStock(null)[kind + 1];
        int total = kind < 0 ? Math.Max(0, _parts) : Math.Max(0, _specialStock[kind]);
        return free + "/" + total + " FREE | $" + EstimatedUnitAcquisitionCost(kind).ToString("N0") +
            (free <= 0 ? " UNAVAILABLE" : " EST.");
    }

    private bool TrySellPart(int kind)
    {
        if (!_bootGameplayActivated || Game.IsPaused || kind < -1 || kind > 2) return false;
        int now = Game.GameTime;
        if (_lastPartsSaleGameTime == now) return false; // Defense beyond the shared input gate.
        _lastPartsSaleGameTime = now;
        if (UnreservedRepairStock(null)[kind + 1] < 1)
        { Notify("No unreserved " + DeliveryItemName(kind) + " available to sell."); return false; }
        long price = EstimatedUnitAcquisitionCost(kind);
        if (price < 0 || _cash > long.MaxValue - price || _revenue > long.MaxValue - price ||
            _partsSalesRevenue > long.MaxValue - price || _partsSalesUnitsSold == long.MaxValue)
        { Notify("Parts sale cannot fit in the business ledger."); return false; }
        long cash = _cash, revenue = _revenue, sales = _partsSalesRevenue, units = _partsSalesUnitsSold;
        int stock = kind < 0 ? _parts : _specialStock[kind];
        if (kind < 0) _parts--; else _specialStock[kind]--;
        _cash += price; _revenue += price; _partsSalesRevenue += price; _partsSalesUnitsSold++;
        if (!Save())
        {
            if (kind < 0) _parts = stock; else _specialStock[kind] = stock;
            _cash = cash; _revenue = revenue; _partsSalesRevenue = sales; _partsSalesUnitsSold = units;
            Notify("Parts sale cancelled because its save did not commit.");
            return false;
        }
        // Successful stocking remains a historical earned-package visual, not a stock counter.
        try
        {
            string path = Path.Combine(DataDirectory, "PartsSales.v1.csv");
            if (!File.Exists(path)) File.AppendAllText(path, "Utc,Kind,Units,UnitEstimate,Total,GameTimeMs" + Environment.NewLine);
            File.AppendAllText(path, DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture) +
                "," + kind + ",1," + price + "," + price + "," + now + Environment.NewLine);
        }
        catch (Exception ex) { Log("Parts sales audit (ledger already committed)", ex); }
        Notify("Sold 1 " + DeliveryItemName(kind) + " for $" + price.ToString("N0") + " (fixed cost estimate).");
        return true;
    }

    // Same-volume atomic snapshot. Failure never replaces a previously valid save.
    private static bool TryCommitBusinessSave(string path, Action<string> writeSnapshot, Action<Exception> report)
    {
        string temporary = null;
        try
        {
            string absolute = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(absolute);
            Directory.CreateDirectory(directory);
            temporary = Path.Combine(directory, ".gh-save-" + Guid.NewGuid().ToString("N") + ".ini");
            bool existing = File.Exists(absolute);
            if (existing) File.Copy(absolute, temporary, false); // Retain unknown INI keys.
            writeSnapshot(temporary);
            if (!File.Exists(temporary)) throw new IOException("Snapshot writer did not create a save.");
            if (existing) File.Replace(temporary, absolute, null);
            else File.Move(temporary, absolute);
            temporary = null;
            return true;
        }
        catch (Exception ex) { report(ex); return false; }
        finally
        {
            if (temporary != null)
                try { File.Delete(temporary); } catch (Exception ex) { report(ex); }
        }
    }

    // Additive reporting journal. Existing business counters remain authoritative.
    private sealed class FinanceSnapshot
    {
        public const int Size = 18;
        public long[] V = new long[Size];
        public FinanceSnapshot CloneSnapshot() { return new FinanceSnapshot { V = (long[])V.Clone() }; }
        public static long Sales(FinanceSnapshot s) { return checked(s.V[1] + s.V[2] + s.V[3] + s.V[4]); }
        public static long Paid(FinanceSnapshot s) { long total = 0; for (int i = 5; i <= 10; i++) total = checked(total + s.V[i]); return total; }
        public static long Due(FinanceSnapshot s) { return checked(s.V[11] + s.V[12] + s.V[13]); }
        public static long Profit(FinanceSnapshot s) { return checked(Sales(s) - Paid(s) - Due(s) + s.V[16] + s.V[17]); }
        public static FinanceSnapshot Difference(FinanceSnapshot end, FinanceSnapshot start)
        {
            var result = new FinanceSnapshot();
            for (int i = 0; i < Size; i++) result.V[i] = checked(end.V[i] - start.V[i]);
            return result;
        }
    }

    private sealed class FinanceEntry
    {
        public long Id, ActiveMs, Amount, CashAfter, Shift;
        public int Category;
        public bool IsCash { get { return Category < 15; } }
    }

    private sealed class FinanceBook
    {
        public const int Retained = 256;
        public FinanceSnapshot Opening, Checkpoint, ShiftStart, ShiftEnd;
        public long NextId = 1, ActiveMs, ShiftNumber, HistoryOpeningCash, CashIn, CashOut;
        public bool ShiftOpen;
        public List<FinanceEntry> Entries = new List<FinanceEntry>();
        public static FinanceBook Create(FinanceSnapshot opening)
        {
            return new FinanceBook { Opening = opening.CloneSnapshot(), Checkpoint = opening.CloneSnapshot(),
                ShiftStart = opening.CloneSnapshot(), ShiftEnd = opening.CloneSnapshot(), HistoryOpeningCash = opening.V[0] };
        }
        public FinanceBook CloneBook()
        {
            return new FinanceBook { Opening = Opening.CloneSnapshot(), Checkpoint = Checkpoint.CloneSnapshot(),
                ShiftStart = ShiftStart.CloneSnapshot(), ShiftEnd = ShiftEnd.CloneSnapshot(), NextId = NextId,
                ActiveMs = ActiveMs, ShiftNumber = ShiftNumber, HistoryOpeningCash = HistoryOpeningCash,
                CashIn = CashIn, CashOut = CashOut, ShiftOpen = ShiftOpen, Entries = new List<FinanceEntry>(Entries) };
        }
        private void Add(int category, long amount, long time, ref long cash)
        {
            if (amount == 0) return;
            if (category < 15)
            {
                cash = checked(cash + amount);
                if (amount > 0) CashIn = checked(CashIn + amount); else CashOut = checked(CashOut - amount);
            }
            Entries.Add(new FinanceEntry { Id = NextId, ActiveMs = time, Category = category,
                Amount = amount, CashAfter = cash, Shift = ShiftOpen ? ShiftNumber : 0L });
            NextId = checked(NextId + 1);
            while (Entries.Count > Retained)
            { HistoryOpeningCash = Entries[0].CashAfter; Entries.RemoveAt(0); }
        }
        public void Observe(FinanceSnapshot current, long time, bool businessOpen)
        {
            if (time < ActiveMs) throw new InvalidDataException("Finance active clock moved backwards.");
            FinanceSnapshot delta = FinanceSnapshot.Difference(current, Checkpoint);
            long cash = Checkpoint.V[0];
            // Mutually exclusive sales: remote is a subset of the old customer total.
            for (int i = 1; i <= 4; i++) Add(i - 1, delta.V[i], time, ref cash);
            for (int i = 5; i <= 10; i++) Add(i - 1, checked(-delta.V[i]), time, ref cash);
            Add(10, delta.V[14], time, ref cash); Add(11, checked(-delta.V[15]), time, ref cash);
            Add(12, delta.V[16], time, ref cash); Add(13, delta.V[17], time, ref cash);
            Add(14, checked(current.V[0] - cash), time, ref cash); // Never silently invent a sale.
            // Expenses arise when owed, not a second time when the invoice is paid.
            Add(15, checked(delta.V[6] + delta.V[11]), time, ref cash);
            Add(16, checked(delta.V[7] + delta.V[12]), time, ref cash);
            Add(17, checked(delta.V[5] + delta.V[13]), time, ref cash);
            if (businessOpen && !ShiftOpen)
            { ShiftNumber = checked(ShiftNumber + 1); ShiftStart = current.CloneSnapshot(); }
            if (ShiftOpen || businessOpen) ShiftEnd = current.CloneSnapshot();
            ShiftOpen = businessOpen;
            Checkpoint = current.CloneSnapshot(); ActiveMs = time;
        }
    }

    private FinanceBook _financeBook;
    private long _financeActiveMs, _financeOwnerDeposits, _financeOwnerWithdrawals, _financeRebateIncome, _financeRefundIncome;
    private bool _financeLifetime = true;
    private int _financeHistoryPage;
    private static readonly string[] FinanceCategories = {
        "Physical repairs", "Remote repairs", "Autonomous jobs", "Parts retail", "Supplier payment",
        "Wage payment", "Rent payment", "Hiring", "Training", "Equipment", "Owner deposit", "Owner withdrawal",
        "Manager rebate", "Supplier refund", "Unclassified adjustment", "Wages accrued", "Rent accrued", "Supplies accrued" };

    private FinanceSnapshot CaptureFinanceSnapshot()
    {
        var s = new FinanceSnapshot();
        s.V[0] = _cash;
        s.V[1] = checked(_customerRevenue - _remoteSales); s.V[2] = _remoteSales;
        s.V[3] = checked(_revenue - _customerRevenue - _partsSalesRevenue); s.V[4] = _partsSalesRevenue;
        s.V[5] = _partsSpend; s.V[6] = _payrollSpend; s.V[7] = _overheadSpend;
        s.V[8] = _hireSpend; s.V[9] = _trainingSpend; s.V[10] = _equipmentSpend;
        s.V[11] = checked(MechanicWagesDue() + _receptionWagesDue + _managerWagesDue);
        s.V[12] = _rentUtilitiesDue; s.V[13] = _supplierInvoicesDue;
        s.V[14] = _financeOwnerDeposits; s.V[15] = _financeOwnerWithdrawals;
        s.V[16] = _financeRebateIncome; s.V[17] = _financeRefundIncome;
        return s;
    }

    private static void WriteFinanceSnapshot(ScriptSettings data, string section, FinanceSnapshot state)
    { for (int i = 0; i < FinanceSnapshot.Size; i++) data.SetValue(section, "V" + i, state.V[i]); }

    private static FinanceSnapshot ReadFinanceSnapshot(ScriptSettings data, string section)
    {
        var state = new FinanceSnapshot();
        for (int i = 0; i < FinanceSnapshot.Size; i++)
        {
            long value = data.GetValue(section, "V" + i, long.MinValue);
            if (value == long.MinValue) throw new InvalidDataException("Missing finance snapshot value.");
            state.V[i] = value;
        }
        return state;
    }

    private void WriteFinanceBook(ScriptSettings data, FinanceBook book)
    {
        const string section = "FinanceV1";
        data.SetValue(section, "Version", 1); data.SetValue(section, "Count", book.Entries.Count);
        data.SetValue(section, "NextId", book.NextId); data.SetValue(section, "ActiveMs", book.ActiveMs);
        data.SetValue(section, "Shift", book.ShiftNumber); data.SetValue(section, "ShiftOpen", book.ShiftOpen);
        data.SetValue(section, "HistoryOpeningCash", book.HistoryOpeningCash);
        data.SetValue(section, "CashIn", book.CashIn); data.SetValue(section, "CashOut", book.CashOut);
        data.SetValue(section, "Deposits", _financeOwnerDeposits); data.SetValue(section, "Withdrawals", _financeOwnerWithdrawals);
        data.SetValue(section, "Rebates", _financeRebateIncome); data.SetValue(section, "Refunds", _financeRefundIncome);
        WriteFinanceSnapshot(data, "FinanceOpeningV1", book.Opening); WriteFinanceSnapshot(data, "FinanceCheckpointV1", book.Checkpoint);
        WriteFinanceSnapshot(data, "FinanceShiftStartV1", book.ShiftStart); WriteFinanceSnapshot(data, "FinanceShiftEndV1", book.ShiftEnd);
        for (int i = 0; i < book.Entries.Count; i++)
        {
            FinanceEntry e = book.Entries[i]; string row = "FinanceEntryV1." + i;
            data.SetValue(row, "Id", e.Id); data.SetValue(row, "ActiveMs", e.ActiveMs);
            data.SetValue(row, "Category", e.Category); data.SetValue(row, "Amount", e.Amount);
            data.SetValue(row, "CashAfter", e.CashAfter); data.SetValue(row, "Shift", e.Shift);
        }
    }

    private void LoadFinanceBook(ScriptSettings data)
    {
        int version = data.GetValue("FinanceV1", "Version", 0);
        if (version == 0) { _financeBook = FinanceBook.Create(CaptureFinanceSnapshot()); return; }
        if (version != 1) throw new InvalidDataException("Unsupported finance journal version.");
        _financeOwnerDeposits = data.GetValue("FinanceV1", "Deposits", 0L);
        _financeOwnerWithdrawals = data.GetValue("FinanceV1", "Withdrawals", 0L);
        _financeRebateIncome = data.GetValue("FinanceV1", "Rebates", 0L);
        _financeRefundIncome = data.GetValue("FinanceV1", "Refunds", 0L);
        if (_financeOwnerDeposits < 0 || _financeOwnerWithdrawals < 0 || _financeRebateIncome < 0 || _financeRefundIncome < 0) throw new InvalidDataException("Negative recorded finance credit/transfer total.");
        var b = new FinanceBook { Opening = ReadFinanceSnapshot(data, "FinanceOpeningV1"),
            Checkpoint = ReadFinanceSnapshot(data, "FinanceCheckpointV1"), ShiftStart = ReadFinanceSnapshot(data, "FinanceShiftStartV1"),
            ShiftEnd = ReadFinanceSnapshot(data, "FinanceShiftEndV1"), NextId = data.GetValue("FinanceV1", "NextId", 0L),
            ActiveMs = data.GetValue("FinanceV1", "ActiveMs", -1L), ShiftNumber = data.GetValue("FinanceV1", "Shift", -1L),
            HistoryOpeningCash = data.GetValue("FinanceV1", "HistoryOpeningCash", long.MinValue),
            CashIn = data.GetValue("FinanceV1", "CashIn", -1L), CashOut = data.GetValue("FinanceV1", "CashOut", -1L) };
        int count = data.GetValue("FinanceV1", "Count", -1);
        if (count < 0 || count > FinanceBook.Retained || b.NextId <= 0 || b.ActiveMs < 0 || b.ShiftNumber < 0 ||
            b.CashIn < 0 || b.CashOut < 0 || b.HistoryOpeningCash == long.MinValue)
            throw new InvalidDataException("Malformed finance journal header.");
        long running = b.HistoryOpeningCash, lastId = 0, lastTime = 0;
        for (int i = 0; i < count; i++)
        {
            string row = "FinanceEntryV1." + i;
            var e = new FinanceEntry { Id = data.GetValue(row, "Id", 0L), ActiveMs = data.GetValue(row, "ActiveMs", -1L),
                Category = data.GetValue(row, "Category", -1), Amount = data.GetValue(row, "Amount", 0L),
                CashAfter = data.GetValue(row, "CashAfter", long.MinValue), Shift = data.GetValue(row, "Shift", -1L) };
            if (e.Id <= lastId || (i > 0 && e.Id != lastId + 1) || e.Id >= b.NextId || e.ActiveMs < lastTime || e.ActiveMs > b.ActiveMs ||
                e.Category < 0 || e.Category >= FinanceCategories.Length || e.Amount == 0 || e.Shift < 0 || e.Shift > b.ShiftNumber)
                throw new InvalidDataException("Malformed finance journal transaction.");
            if (e.IsCash) running = checked(running + e.Amount);
            if (running != e.CashAfter) throw new InvalidDataException("Finance transaction cash does not reconcile.");
            lastId = e.Id; lastTime = e.ActiveMs; b.Entries.Add(e);
        }
        if ((count == 0 && b.NextId != 1) || (count > 0 && lastId != b.NextId - 1)) throw new InvalidDataException("Missing finance transaction tail.");
        FinanceSnapshot current = CaptureFinanceSnapshot();
        for (int i = 0; i < FinanceSnapshot.Size; i++)
            if (current.V[i] != b.Checkpoint.V[i]) throw new InvalidDataException("Finance journal is stale. Preserve save and restore matching source/data.");
        if (running != current.V[0] || checked(b.Opening.V[0] + b.CashIn - b.CashOut) != current.V[0])
            throw new InvalidDataException("Finance opening/inflow/outflow bridge does not reconcile.");
        // Loading never opens the shop. The last saved shift remains inspectable.
        b.ShiftOpen = false; _financeBook = b; _financeActiveMs = b.ActiveMs;
    }

    private FinanceBook PrepareFinanceCommit()
    {
        FinanceBook candidate = (_financeBook ?? FinanceBook.Create(CaptureFinanceSnapshot())).CloneBook();
        candidate.Observe(CaptureFinanceSnapshot(), _financeActiveMs, _clockedIn);
        return candidate;
    }

    private bool Save()
    {
        FinanceBook candidate;
        try
        {
            candidate = PrepareFinanceCommit();
        }
        catch (Exception ex) { Log("Finance snapshot", ex); Notify("Finance snapshot failed; save not committed."); return false; }
        bool saved = TryCommitBusinessSave(SaveFile, snapshotPath =>
        {
            ScriptSettings data = ScriptSettings.Load(snapshotPath);
            data.SetValue("Business", "Cash", _cash);
            data.SetValue("Business", "Revenue", _revenue);
            data.SetValue("Business", "PartsSalesRevenue", _partsSalesRevenue);
            data.SetValue("Business", "PartsSalesUnitsSold", _partsSalesUnitsSold);
            data.SetValue("Business", "PartsSpent", _partsSpend);
            data.SetValue("Business", "PayrollSpent", _payrollSpend);
            data.SetValue("BusinessBills", "ReceptionistWagesDue", _receptionWagesDue);
            data.SetValue("BusinessBills", "FormerStaffWagesDue", _formerStaffWagesDue);
            data.SetValue("BusinessBills", "RentUtilitiesDue", _rentUtilitiesDue);
            data.SetValue("Management", "Tier", _managerTier);
            data.SetValue("Management", "XP", _managerXp);
            data.SetValue("Management", "Name", _managerName);
            SavePoint(data, "ManagerWorkstation", _managerWorkstation);
            SavePoint(data, "ManagerApproach", _managerApproach);
            data.SetValue("EmployeeSettings", "CustomManagerPaths", _customManagerWalkingPaths);
            for (int i = 0; i < OptionalPathPointCount; i++)
                SavePoint(data, "ManagerWaypoint" + (i + 1), _managerRoute[i]);
            SavePoint(data, "ReceptionistResumePost", _receptionist.SavedPost);
            data.SetValue("Management", "WagesDue", _managerWagesDue);
            data.SetValue("Management", "WageAgeSeconds", _managerWageAgeSeconds);
            data.SetValue("Management", "Savings", _managerSavings);
            data.SetValue("Management", "RemoteRevenue", _remoteSales);
            data.SetValue("Management", "RemoteCompleted", _remoteJobsCompleted);
            data.SetValue("Management", "RemotePartsConsumed", _remotePartsConsumed);
            data.SetValue("WholesaleVehicle", "Plate", _wholesaleVehiclePlate);
            data.SetValue("WholesaleVehicle", "Model", _wholesaleVehicleModel);
            data.SetValue("WholesaleVehicle", "CargoLoaded", _wholesaleCargoLoaded);
            data.SetValue("WholesaleVehicle", "CargoUnloaded", _wholesaleCargoUnloaded);
            data.SetValue("BusinessBills", "ReceptionAgeSeconds", _receptionWageUnpaidSeconds);
            data.SetValue("BusinessBills", "RentAgeSeconds", _rentUnpaidSeconds);
            data.SetValue("ShiftReport", "Ready", _shiftReportReady);
            data.SetValue("ShiftReport", "SalesAtOpen", _shiftSalesBaseline);
            data.SetValue("ShiftReport", "CostsAtOpen", _shiftCostsBaseline);
            data.SetValue("Business", "OverheadSpent", _overheadSpend);
            data.SetValue("Business", "HiringSpent", _hireSpend);
            data.SetValue("Business", "TrainingSpent", _trainingSpend);
            data.SetValue("Business", "Parts", _parts);
            for (int stock = 0; stock < 3; stock++)
                data.SetValue("DetailedInventory", "Stock" + stock, _specialStock[stock]);
            data.SetValue("Purchasing", "Supplier", _selectedSupplier);
            data.SetValue("Purchasing", "ReorderThreshold", _reorderThreshold);
            data.SetValue("Purchasing", "DeliveryKind", _deliveryKind);
            data.SetValue("Purchasing", "DeliveryQuantity", _deliveryQuantity);
            data.SetValue("Purchasing", "DeliverySupplier", _deliverySupplier);
            data.SetValue("Purchasing", "DeliveryCost", _deliveryCost);
            data.SetValue("Purchasing", "DeliveryPrepaid", _deliveryPrepaid);
            data.SetValue("BusinessBills", "SupplierInvoicesDue", _supplierInvoicesDue);
            data.SetValue("Storage", "OwnedPallets", _ownedStoragePallets);
            SavePoint(data, "SupplierWarehousePickup", _wholesaleWarehousePickup);
            for (int pallet = 0; pallet < MaxStoragePallets; pallet++)
            {
                if (pallet > 0) SavePoint(data, "SupplierStoragePallet" + (pallet + 1), _extraStoragePallets[pallet - 1]);
                data.SetValue("Storage", "BoxCount" + (pallet + 1), _storedDeliveryCrates[pallet]);
                data.SetValue("Storage", "DeliveredPackageMask" + (pallet + 1), _deliveredPackageMasks[pallet]);
            }
            data.SetValue("Purchasing", "DeliveryDueUtcTicks", _deliveryDueUtcTicks);
            data.SetValue("Purchasing", "DeliveryRemainingGameSeconds", _deliveryRemainingGameSeconds);
            data.SetValue("Purchasing", "CompletedDeliveries", _completedSupplierDeliveries);
            data.SetValue("SupplierVisuals", "PhysicalMuleDeliveries", _physicalMuleDeliveries);
            SavePoint(data, "SupplierTruckSpawn", _supplierTruckSpawn);
            SavePoint(data, "SupplierTruckPark", _supplierTruckPark);
            SavePoint(data, "SupplierDriverHandoff", _supplierHandoff);
            SavePoint(data, "SupplierPartsPallet", _supplierPartsPallet);
            for (int w = 0; w < 3; w++)
                SavePoint(data, "SupplierDriverWaypoint" + (w + 1), _supplierDriverWaypoints[w]);
            data.SetValue("Business", "EquipmentSpent", _equipmentSpend);
            data.SetValue("Equipment", "ToolSet", _toolSetPurchased);
            data.SetValue("Equipment", "Scanner", _diagnosticScannerPurchased);
            data.SetValue("Equipment", "OfficeDeskPurchased", _officeDeskPurchased);
            data.SetValue("Equipment", "OfficeDeskPlaced", _officeDeskPlaced);
            data.SetValue("Equipment", "ToolSetPlaced", _toolSetPlaced);
            data.SetValue("Equipment", "ScannerPlaced", _scannerPlaced);
            data.SetValue("Gamepad", "Enabled", _controllerEnabled);
            SavePoint(data, "OfficeDesk", _officeDeskLocation);
            SavePoint(data, "ToolSetPlacement", _toolSetLocation);
            SavePoint(data, "ScannerPlacement", _scannerLocation);
            data.SetValue("Business", "Mechanics", _mechanics.Count);
            for (int i = 0; i < _mechanics.Count; i++)
            {
                string section = "MechanicProfile" + (i + 1);
                Mechanic m = _mechanics[i];
                data.SetValue(section, "Name", m.Name);
                data.SetValue(section, "XP", m.Xp);
                data.SetValue(section, "Specialty", m.Specialty);
                data.SetValue(section, "WageBonus", m.WageBonus);
                data.SetValue(section, "CompletedJobs", m.JobsCompleted);
                data.SetValue(section, "WagesDue", m.UnpaidWages);
                data.SetValue(section, "WageAgeSeconds", m.WageUnpaidSeconds);
                data.SetValue(section, "JobOutboundMask", m.JobOutboundMask & 31);
                data.SetValue(section, "JobReturnMask", m.JobReturnMask & 31);
                SavePoint(data, "MechanicResumePost" + (i + 1), m.SavedPost);
            }
            data.SetValue("Business", "ReceptionistHired", _receptionistHired);
            data.SetValue("ReceptionistProfile", "Tier", _receptionistTier);
            data.SetValue("ReceptionistProfile", "Name", _receptionistName);
            data.SetValue("ReceptionistProfile", "WageBonus", _receptionistWageBonus);
            data.SetValue("CustomerService", "Enabled", _customerServiceEnabled);
            data.SetValue("CustomerService", "PendingCustomerLimit", _customerQueueCapacity);
            data.SetValue("CustomerService", "DriveInEnabled", _driveInEnabled);
            data.SetValue("CustomerService", "VehicleTransfersEnabled", _vehicleTransfersEnabled);
            data.SetValue("CustomerService", "AutomaticLotMode", _automaticLotMode);
            data.SetValue("CustomerService", "AutoLotRightSide", _autoLotRightSide);
            data.SetValue("CustomerService", "ValetReverseOut", _valetReverseOut);
            data.SetValue("CustomerService", "AutoLotOffsetMeters", _autoLotOffsetMeters);
            data.SetValue("CustomerService", "CustomerBlipsEnabled", _customerBlipsEnabled);
            data.SetValue("CustomerService", "CompletedOrders", _customerCompleted);
            data.SetValue("CustomerService", "LastOrderId", _lastCustomerOrderId);
            data.SetValue("CustomerService", "GrossRevenue", _customerRevenue);
            data.SetValue("CustomerService", "LastOutcome", _lastOrderOutcome);
            data.SetValue("EmployeeSettings", "TeleportOnBlockedRoute", _routeRecoveryEnabled);
            data.SetValue("EmployeeSettings", "BreakAnimationsEnabled", _breakAnimationsEnabled);
            data.SetValue("EmployeeSettings", "CustomWalkingPaths", _customEmployeeWalkingPaths);
            data.SetValue("EmployeeSettings", "CustomJobRoutes", _customMechanicJobRoutes);
            data.SetValue("EmployeeSettings", "PickupUseReverseArrivalWaypoints", _pickupUsesArrivalExit);
            data.SetValue("EmployeeSettings", "SmoothWaypointTransitions", _smoothMechanicWaypoints);
            data.SetValue("EmployeeSettings", "SafeTeleportOnStall", _safeTeleportOnStall);
            data.SetValue("EmployeeSettings", "CustomReceptionistPaths", _customReceptionWalkingPaths);
            data.SetValue("Business", "CompletedEmployeeJobs", _completed);
            data.SetValue("Shop", "Placed", _placed);
            data.SetValue("Shop", "X", _location.X);
            data.SetValue("Shop", "Y", _location.Y);
            data.SetValue("Shop", "Z", _location.Z);
            for (int i = 0; i < VisualBayCount; i++)
            {
                string section = "WorkBay" + (i + 1);
                data.SetValue(section, "Set", _bays[i].IsSet);
                data.SetValue(section, "X", _bays[i].Position.X);
                data.SetValue(section, "Y", _bays[i].Position.Y);
                data.SetValue(section, "Z", _bays[i].Position.Z);
                data.SetValue(section, "Heading", _bays[i].Heading);
            }
            SavePoint(data, "EmployeeEntrance", _employeeEntrance);
            SavePoint(data, "WalkCheckpoint", _walkCheckpoint);
            SavePoint(data, "EmployeeBreakArea", _breakArea);
            SavePoint(data, "ReceptionDesk", _receptionDesk);
            SavePoint(data, "ReceptionApproach", _receptionApproach);
            SavePoint(data, "CustomerDriveSpawn", _customerDriveSpawn);
            SavePoint(data, "CustomerDropoff", _customerDropoff);
            for (int i = 0; i < VisualBayCount; i++)
                SavePoint(data, "VehicleParkingBay" + (i + 1), _vehicleParkingBays[i]);
            SavePoint(data, "CustomerApproach", _customerApproach);
            SavePoint(data, "CustomerQueue", _customerQueue);
            for (int i = 0; i < _customerWalkingRoute.Length; i++)
                SavePoint(data, "CustomerWalkingPoint" + (i + 1), _customerWalkingRoute[i]);
            SavePoint(data, "CustomerQueueCar2", _queueCar2);
            SavePoint(data, "CustomerQueueCar3", _queueCar3);
            for (int i = 0; i < OptionalPathPointCount; i++)
            {
                SavePoint(data, "ArrivalWaypoint" + (i + 1), _arrivalRoute[i]);
                SavePoint(data, "ReceptionWaypoint" + (i + 1), _receptionRoute[i]);
                SavePoint(data, "ServiceWaypoint" + (i + 1), _serviceRoute[i]);
            }
            for (int i = 0; i < VisualBayCount; i++)
                SavePoint(data, "BayApproach" + (i + 1), _bayApproaches[i]);
            WriteFinanceBook(data, candidate);
            WriteSupplierOrderLines(data);
            WriteRepairPromises(data);
            if (!data.Save()) throw new IOException("ScriptSettings.Save returned false.");
        }, ex => Log("Saving", ex));
        if (saved) _financeBook = candidate;
        if (!saved) Notify("Business save failed. Check GlassHeroesBusiness/Errors.log.");
        return saved;
    }

    // Input map is intentionally opt-in to avoid clashing with other single-player mods.
    private GTA.Control ReadPadControl(string key, GTA.Control fallback)
    {
        string value = _configuration.GetValue("Gamepad", key, fallback.ToString());
        GTA.Control selected;
        if (!Enum.TryParse<GTA.Control>(value, true, out selected) ||
            !Enum.IsDefined(typeof(GTA.Control), selected))
        {
            Log("Gamepad mapping " + key, new ArgumentException("Invalid control '" + value + "'; using " + fallback));
            return fallback;
        }
        return selected;
    }

    // Read even if control was temporarily disabled while the F2 UI owns input.
    private static bool PadJustPressed(GTA.Control input)
    {
        // Issue #20: v3 Game helpers omit the group argument. Preserve R2's
        // gameplay (0) AND frontend (2), including disabled menu controls.
        return GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_JUST_PRESSED, 0, (int)input) ||
            GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, (int)input) ||
            GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_JUST_PRESSED, 2, (int)input) ||
            GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 2, (int)input);
    }

    private static bool PadPressed(GTA.Control input)
    {
        return GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_PRESSED, 0, (int)input) ||
            GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DISABLED_CONTROL_PRESSED, 0, (int)input) ||
            GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_PRESSED, 2, (int)input) ||
            GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DISABLED_CONTROL_PRESSED, 2, (int)input);
    }

    private bool _nativePauseYielded;

    private void HideBusinessMenuForPause()
    {
        _showMenu = false;
        _heldKeys.Clear();
        _menuInput.Reset();
        _lastControllerAction = DateTime.UtcNow;
    }

    private bool YieldMenuToNativePause()
    {
        if (!_showMenu) return Game.IsPaused;
        if (!Game.IsPaused && !PadJustPressed(GTA.Control.FrontendPause) &&
            !PadJustPressed(GTA.Control.FrontendPauseAlternate)) return false;
        HideBusinessMenuForPause();
        return true;
    }

    private void DisableControllerControlsWhileMenuOpen()
    {
        // Strict controller focus: walk / look only. Prevent firing, interacting,
        // vehicle entry, jumping, punching, weapon wheel, phone, etc. The menu
        // still reads D-pad / A / B with IsDisabledControlJustPressed.
        GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_ALL_CONTROL_ACTIONS, 0);
        GTA.Control[] permitted = {
            GTA.Control.MoveLeftRight, GTA.Control.MoveUpDown,
            GTA.Control.LookLeftRight, GTA.Control.LookUpDown
        };
        // Controller menu remains usable while driving: allow basic throttle,
        // braking, steering and camera only; prohibit weapons / vehicle entry.
        Ped player = Game.Player.Character;
        if (player != null && player.Exists() && player.IsInVehicle())
            permitted = new GTA.Control[] {
                GTA.Control.VehicleAccelerate, GTA.Control.VehicleBrake,
                GTA.Control.VehicleMoveLeftRight, GTA.Control.VehicleMoveUpDown,
                GTA.Control.VehicleHandbrake, GTA.Control.LookLeftRight,
                GTA.Control.LookUpDown
            };
        foreach (GTA.Control input in permitted)
            GTA.Native.Function.Call(GTA.Native.Hash.ENABLE_CONTROL_ACTION, 0, (int)input, true);
        // Keep GTA's native Pause/Start paths available even during custom focus.
        GTA.Native.Function.Call(GTA.Native.Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.FrontendPause, true);
        GTA.Native.Function.Call(GTA.Native.Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.FrontendPauseAlternate, true);
        GTA.Native.Function.Call(GTA.Native.Hash.ENABLE_CONTROL_ACTION, 2, (int)GTA.Control.FrontendPause, true);
        GTA.Native.Function.Call(GTA.Native.Hash.ENABLE_CONTROL_ACTION, 2, (int)GTA.Control.FrontendPauseAlternate, true);
    }

    private void PollController()
    {
        if (!_controllerEnabled || Game.IsPaused || _nativePauseYielded) return;
        DateTime now = DateTime.UtcNow;
        try
        {
        if ((now - _lastControllerAction).TotalMilliseconds < 180) return;
        if (PadPressed(_controllerOpenModifier) && PadJustPressed(_controllerOpenButton) &&
            _menuInput.TryController(4, now.Ticks))
        {
            _lastPadDetected = "OPEN COMBO";
            _lastPadDetectedAt = now;
            _showMenu = !_showMenu;
            if (_showMenu) { _menuPage = MenuPage.Business; _selection = 0; }
            _lastControllerAction = now;
            return;
        }
        if (!_showMenu) return;
        if (_menuPage == MenuPage.BuildSupplierOrder &&
            (PadJustPressed(GTA.Control.FrontendLeft) || PadJustPressed(GTA.Control.FrontendRight)))
        {
            int direction = PadJustPressed(GTA.Control.FrontendLeft) ? -1 : 1;
            if (_menuInput.TryController(direction < 0 ? 5 : 6, now.Ticks)) AdjustSupplierCart(direction);
            _lastControllerAction = now; return;
        }
        // Unlike keyboard KeyDown, gamepad edges must be polled during Tick.
        // Some frontend controls are used elsewhere, so suppress only when menu active.
        if (PadJustPressed(_controllerBack) && _menuInput.TryController(3, now.Ticks)) { _lastPadDetected = "BACK"; NavigateBack(); _lastControllerAction = now; }
        else if (PadJustPressed(_controllerUp) && _menuInput.TryController(0, now.Ticks))
        { _lastPadDetected = "UP"; _selection = (_selection - 1 + GetMenuItems().Length) % GetMenuItems().Length; _lastControllerAction = now; }
        else if (PadJustPressed(_controllerDown) && _menuInput.TryController(1, now.Ticks))
        { _lastPadDetected = "DOWN"; _selection = (_selection + 1) % GetMenuItems().Length; _lastControllerAction = now; }
        else if (PadJustPressed(_controllerAccept) && _menuInput.TryController(2, now.Ticks))
        { _lastPadDetected = "SELECT"; ExecuteSelection(); _lastControllerAction = now; }
        if (_lastPadDetected != "NONE") _lastPadDetectedAt = now;
        }
        catch (Exception ex)
        {
            // Input failure must not close the whole business or freeze payroll.
            if ((now - _lastPadReadErrorAt).TotalSeconds >= 10)
            {
                Log("Gamepad input", ex);
                _lastPadReadErrorAt = now;
            }
        }
    }

    private static string SupplierName(int index)
    { return index == 0 ? "WHOLESALE" : index == 1 ? "LOCAL" : "EXPRESS"; }

    private int SupplierDeliverySeconds(int supplier)
    { return supplier == 0 ? _supplierWholesaleSeconds : supplier == 1 ? _supplierLocalSeconds : _supplierExpressSeconds; }

    private long InventoryPurchaseCost(int kind, int quantity)
    {
        int unit = kind < 0 ? _partCost : _specialUnitCosts[kind];
        // Supplier pricing: wholesale -10%, local retail, express +25%.
        int percent = _selectedSupplier == 0 ? 90 : _selectedSupplier == 1 ? 100 : 125;
        long supplierPrice = ((long)unit * quantity * percent + 99) / 100;
        return supplierPrice * (100 - ManagerDiscountPercent()) / 100;
    }

    private string DeliveryItemName(int kind)
    { return kind < 0 ? "universal parts" : _stockNames[kind]; }

    private void ShowSupplierDeliveryStatus()
    {
        if (_deliveryKind == -2)
        {
            ShowStatusNotice("SUPPLIER DELIVERY", "No outstanding purchase orders.",
                "Preferred supplier: " + SupplierName(_selectedSupplier) +
                    " (" + SupplierDeliverySeconds(_selectedSupplier) + " sec lead time)",
                "Completed deliveries " + _completedSupplierDeliveries,
                "New orders are invoiced after arrival/stocking; pay under Expenses.");
            return;
        }
        double remain = Math.Max(0, _deliveryRemainingGameSeconds);
        ShowStatusNotice("SUPPLIER DELIVERY",
            _deliveryQuantity + " " + (_deliveryLines == null ? DeliveryItemName(_deliveryKind) : "mixed parts") + " | " + SupplierName(_deliverySupplier),
            (_deliveryPrepaid ? "PREPAID" : "BILL AFTER STOCKING") + " $" + _deliveryCost.ToString("N0") +
                " | ETA " + (int)Math.Ceiling(remain) + " sec",
            _deliverySupplier == 0 ? "WHOLESALE PICKUP " + Status(_wholesaleWarehousePickup) +
                " | " + MuleStageName() : "MULE " + MuleStageName() +
                " | " + (ManagerSupplierEligible() ? (SupplierPlayerAway() ? "MANAGER VIRTUAL RECEIPT" : "MANAGER PALLET DROP") : "PLAYER HANDOFF") +
                (AvailableSupplierPallet() < 0 ? " | WAITING FOR PALLET/CAPACITY" : ""),
            "Storage " + StoredPartsTotal() + "/" + TotalStorageCapacity() +
                " | Supplier bills $" + _supplierInvoicesDue.ToString("N0"));
    }

    private bool MuleRouteConfigured()
    {
        return _supplierTruckSpawn.IsSet && _supplierTruckPark.IsSet && _supplierHandoff.IsSet &&
            DistanceSquared(_supplierTruckSpawn.Position, _supplierTruckPark.Position) > 64f;
    }

    private string MuleStageName()
    {
        switch (_supplierVisualStage)
        {
            case 1: return "DRIVING TO UNLOAD";
            case 2: return "DRIVER EXITING";
            case 3: return "DRIVER WALKING TO HANDOFF";
            case 4: return _supplierPartsPallet.IsSet ? "AWAITING PLAYER PICKUP" : "DRIVER UNLOADING";
            case 5: return "DEPARTING";
            case 6: return "PLAYER CARRYING BOX";
            case 7: return "WAREHOUSE BOX READY";
            case 8: return "DRIVER AT REAR DOORS";
            case 9: return "WHOLESALE CARGO IN ASSIGNED VEHICLE";
            default: return "WAITING FOR ARRIVAL";
        }
    }

    private void DeleteSupplierVisuals()
    {
        try { if (_supplierCrate != null && _supplierCrate.Exists()) _supplierCrate.Delete(); } catch { }
        try { if (_supplierDriver != null && _supplierDriver.Exists()) _supplierDriver.Delete(); } catch { }
        try { if (_supplierTruck != null && _supplierTruck.Exists()) _supplierTruck.Delete(); } catch { }
        _supplierCrate = null;
        _warehouseVisualAttempts = 0;
        _nextWarehouseVisualAttempt = DateTime.MinValue;
        DeleteOwnedProp(ref _wholesaleWarehousePallet);
        _supplierPlayerCarrying = false;
        _supplierDriver = null;
        _supplierTruck = null;
        _supplierVisualStage = 0;
        _supplierExitIssued = false;
        _supplierDepartIssued = false;
        _supplierRouteIndex = 0;
        _supplierWalkingNodes.Clear();
        _supplierDirectToPlayer = false;
        _managerSupplierPallet = -1;
        _supplierPlayerRepaths = 0;
        _supplierBoxAttachedToDriver = false;
        _muleRearDoorsOpened = false;
    }

    // Session-only route clock. No wall-clock age or persisted/offline catch-up.
    private DateTime _supplierGameplayNow = new DateTime(2000, 1, 1);
    private int _managerSupplierPallet = -1;

    private bool StartMuleDelivery(DateTime now)
    {
        Ped nearbyPlayer = Game.Player.Character;
        if (nearbyPlayer == null || !nearbyPlayer.Exists() || !_placed ||
            !SupplierPhysicalRange(DistanceSquared(nearbyPlayer.Position, _location), _remoteDistanceMeters)) return false;
        if (_muleModel == null || !_muleModel.IsInCdImage || !_muleModel.IsVehicle ||
            _supplierDriverModel == null || !_supplierDriverModel.IsInCdImage || !_supplierDriverModel.IsPed)
        {
            Log("Invalid supplier mule/driver model; fallback automatic delivery", null);
            _physicalMuleDeliveries = false; // session fallback; no paid order is lost
            return false;
        }
        if (!_muleModel.IsLoaded) { _muleModel.Request(); return false; }
        if (!_supplierDriverModel.IsLoaded) { _supplierDriverModel.Request(); return false; }
        if (DistanceSquared(Game.Player.Character.Position, _supplierTruckSpawn.Position) < 25f) return false;
        if (World.GetNearbyVehicles(_supplierTruckSpawn.Position, 7f).Length > 0) return false;
        try
        {
            _supplierTruck = World.CreateVehicle(_muleModel, _supplierTruckSpawn.Position, _supplierTruckSpawn.Heading);
            if (_supplierTruck == null || !_supplierTruck.Exists()) { DeleteSupplierVisuals(); return false; }
            _supplierTruck.IsPersistent = true;
            // Source-specific fleet appearance. Livery indices depend on installed
            // Mule assets, so only apply an index supported by the current model.
            int fleet = Math.Max(0, Math.Min(2, _deliverySupplier));
            try
            {
                int liveryCount = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_VEHICLE_LIVERY_COUNT,
                    _supplierTruck.Handle);
                int liveryIndex = _muleLiveryBySupplier[fleet];
                if (liveryCount > 0 && liveryIndex >= 0 && liveryIndex < liveryCount)
                    GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_LIVERY,
                        _supplierTruck.Handle, liveryIndex);
                // Recognizable fallback fleet colors even without native liveries.
                int primary = fleet == 0 ? 64 : fleet == 1 ? 38 : 88;
                GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_COLOURS,
                    _supplierTruck.Handle, primary, 0);
            }
            catch (Exception liveryError) { Log("Supplier fleet appearance", liveryError); }
            Vector3 p = _supplierTruckSpawn.Position + new Vector3(2f, 2f, 0.15f);
            _supplierDriver = World.CreatePed(_supplierDriverModel, p, _supplierTruckSpawn.Heading);
            if (_supplierDriver == null || !_supplierDriver.Exists()) { DeleteSupplierVisuals(); return false; }
            _supplierDriver.IsPersistent = true;
            _supplierDriver.BlockPermanentEvents = true;
            _supplierDriver.AlwaysKeepTask = true;
            GTA.Native.Function.Call(GTA.Native.Hash.SET_PED_INTO_VEHICLE,
                _supplierDriver.Handle, _supplierTruck.Handle, -1);
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,
                _supplierDriver.Handle, _supplierTruck.Handle,
                _supplierTruckPark.Position.X, _supplierTruckPark.Position.Y, _supplierTruckPark.Position.Z,
                7.0f, CarefulDrivingStyle, 9f);
            _supplierVisualStage = 1;
            _supplierExitIssued = false;
            _supplierDepartIssued = false;
            _supplierStageAt = now;
            _supplierVisualOrderTicks = _deliveryDueUtcTicks;
            Notify("Mule supplier truck is approaching the unloading bay.");
            return true;
        }
        catch (Exception ex) { Log("Supplier Mule spawn", ex); DeleteSupplierVisuals(); return false; }
    }

    private void OpenMuleCargoDoors()
    {
        if (_supplierTruck == null || !_supplierTruck.Exists() || _muleRearDoorsOpened) return;
        _muleRearDoorsOpened = true;
        // Door indexes vary across stock/custom Mule variants. GTA natives silently
        // ignore unsupported indexes; try the rear cargo pair and boot slot.
        foreach (int door in new int[] { 2, 3, 5 })
            try { GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_DOOR_OPEN,
                _supplierTruck.Handle, door, false, false); } catch { }
    }

    private void CloseMuleCargoDoors()
    {
        if (_supplierTruck == null || !_supplierTruck.Exists()) return;
        foreach (int door in new int[] { 2, 3, 5 })
            try { GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_DOOR_SHUT,
                _supplierTruck.Handle, door, false); } catch { }
    }

    private static void EnsureBoxCarryAnimation(Ped carrier)
    {
        if (carrier == null || !carrier.Exists() || carrier.IsInVehicle()) return;
        const string dict = "anim@heists@box_carry@";
        GTA.Native.Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, dict);
        if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, dict)) return;
        if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_ENTITY_PLAYING_ANIM,
            carrier.Handle, dict, "idle", 3)) return;
        GTA.Native.Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, carrier.Handle,
            dict, "idle", 3f, 3f, -1, 49, 0f, false, false, false);
    }

    private void AttachSupplierBoxToPed(Ped ped)
    {
        if (ped == null || !ped.Exists() || _supplierCrate == null || !_supplierCrate.Exists()) return;
        _supplierCrate.IsPositionFrozen = false;
        GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_COLLISION, _supplierCrate.Handle, false, false);
        int hand = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_PED_BONE_INDEX, ped.Handle, 28422);
        GTA.Native.Function.Call(GTA.Native.Hash.ATTACH_ENTITY_TO_ENTITY,
            _supplierCrate.Handle, ped.Handle, hand,
            0.04f, 0.02f, -0.15f, 0f, 0f, 0f,
            false, false, false, false, 2, true);
        EnsureBoxCarryAnimation(ped);
    }

    private void BeginSupplierRearUnload(DateTime now)
    {
        OpenMuleCargoDoors();
        _supplierVisualStage = 8;
        _supplierStageAt = now;
        if (_supplierTruck != null && _supplierTruck.Exists() && _supplierDriver != null && _supplierDriver.Exists())
        {
            Vector3 rear = _supplierTruck.Position - _supplierTruck.ForwardVector * 3f;
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_FOLLOW_NAV_MESH_TO_COORD,
                _supplierDriver.Handle, rear.X, rear.Y, rear.Z, 1f, -1, 1f, 0, 0f);
        }
    }

    private void GiveBoxToDriver(DateTime now)
    {
        if (_supplierDriver == null || !_supplierDriver.Exists() || _supplierTruck == null || !_supplierTruck.Exists()) return;
        if (_supplierCrate == null || !_supplierCrate.Exists())
            _supplierCrate = CreateFixedEquipmentProp("prop_cs_cardbox_01",
                _supplierTruck.Position - _supplierTruck.ForwardVector * 2.5f + new Vector3(0f,0f,0.5f),
                _supplierTruck.Heading, false);
        if (_supplierCrate == null || !_supplierCrate.Exists()) return;
        AttachSupplierBoxToPed(_supplierDriver);
        _supplierBoxAttachedToDriver = true;
        BeginSupplierDriverWalk(now);
    }

    private void MaintainSupplierCarryPose(DateTime now)
    {
        if ((now - _lastCarrierAnimation).TotalSeconds < 2.5) return;
        _lastCarrierAnimation = now;
        if (_supplierPlayerCarrying) EnsureBoxCarryAnimation(Game.Player.Character);
        else if (_supplierBoxAttachedToDriver && _supplierVisualStage >= 3 && _supplierVisualStage <= 4)
            EnsureBoxCarryAnimation(_supplierDriver);
    }

    private void BeginSupplierDriverWalk(DateTime now)
    {
        _supplierWalkingNodes.Clear();
        _managerSupplierPallet = ManagerSupplierEligible() ? AvailableSupplierPallet() : -1;
        if (ManagerSupplierEligible() && _managerSupplierPallet < 0)
        { DeferSupplierPlayerHandoff(now); return; }
        _supplierDirectToPlayer = _managerSupplierPallet < 0 && _supplierPartsPallet.IsSet;
        if (_managerSupplierPallet >= 0)
        {
            foreach (WorkBay point in _supplierDriverWaypoints)
                if (point.IsSet) _supplierWalkingNodes.Add(point.Position);
            _supplierWalkingNodes.Add(StoragePalletPoint(_managerSupplierPallet).Position);
            _supplierRouteIndex = 0;
            _supplierVisualStage = 3;
            _supplierStageAt = now;
            TaskSupplierWalkingNode();
            return;
        }
        if (_supplierDirectToPlayer)
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists())
            { DeferSupplierPlayerHandoff(now); return; }
            _supplierWalkingNodes.Add(player.Position);
            _supplierPlayerApproachDeadline = now.AddSeconds(90);
            _nextSupplierPlayerRepath = now.AddSeconds(3);
            _supplierPlayerRepaths = 0;
        }
        else
        {
            // Regular automatic drop-off retains every configured route stop.
            foreach (WorkBay point in _supplierDriverWaypoints)
                if (point.IsSet) _supplierWalkingNodes.Add(point.Position);
            _supplierWalkingNodes.Add(_supplierHandoff.Position);
        }
        _supplierRouteIndex = 0;
        _supplierVisualStage = 3;
        _supplierStageAt = now;
        TaskSupplierWalkingNode();
    }

    private void UpdateSupplierPlayerApproach(DateTime now)
    {
        Ped player = Game.Player.Character;
        if (player == null || !player.Exists() || player.IsDead ||
            _supplierDriver == null || !_supplierDriver.Exists() ||
            _supplierCrate == null || !_supplierCrate.Exists() || !_supplierBoxAttachedToDriver)
        { DeferSupplierPlayerHandoff(now); return; }
        float distance = DistanceSquared(_supplierDriver.Position, player.Position);
        if (distance < 4f)
        {
            if (_supplierVisualStage != 4)
            { _supplierVisualStage = 4; _supplierStageAt = now; }
            return; // hold the attached box for the player's explicit action
        }
        if (now >= _supplierPlayerApproachDeadline || distance > 10000f || _supplierPlayerRepaths >= 15)
        { DeferSupplierPlayerHandoff(now); return; }
        _supplierVisualStage = 3;
        if (now < _nextSupplierPlayerRepath) return;
        _nextSupplierPlayerRepath = now.AddSeconds(3);
        _supplierPlayerRepaths++;
        _supplierWalkingNodes.Clear();
        _supplierWalkingNodes.Add(player.Position);
        _supplierRouteIndex = 0;
        TaskSupplierWalkingNode();
    }

    private void DeferSupplierPlayerHandoff(DateTime now)
    {
        // Cleanup owns visuals only. The paid/pending order and invoice ledger
        // remain authoritative, and respawn cannot spin on the following tick.
        DeleteSupplierVisuals();
        _nextSupplierVisualRetryUtc = now.AddSeconds(30);
        Log("Supplier player handoff deferred; pending order preserved", null);
    }

    private void TaskSupplierWalkingNode()
    {
        if (_supplierDriver == null || !_supplierDriver.Exists() ||
            _supplierRouteIndex >= _supplierWalkingNodes.Count) return;
        Vector3 at = _supplierWalkingNodes[_supplierRouteIndex];
        GTA.Native.Function.Call(GTA.Native.Hash.TASK_FOLLOW_NAV_MESH_TO_COORD,
            _supplierDriver.Handle, at.X, at.Y, at.Z, 1.0f, -1, 1f, 0, 0f);
    }

    private bool ManagerSupplierEligible()
    {
        return _deliveryKind != -2 && (_deliverySupplier == 1 || _deliverySupplier == 2) &&
            _managerTier >= 0 && _clockedIn && _placed;
    }

    private static bool SupplierPhysicalRange(float distanceSquared, float awayDistance)
    {
        return distanceSquared >= 0f && distanceSquared <= awayDistance * awayDistance;
    }

    private bool SupplierPlayerAway()
    {
        Ped player = Game.Player.Character;
        return player != null && player.Exists() &&
            !SupplierPhysicalRange(DistanceSquared(player.Position, _location), _remoteDistanceMeters);
    }

    private int AvailableSupplierPallet()
    {
        if (!CanStoreShipment(_deliveryQuantity)) return -1;
        for (int i = 0; i < _ownedStoragePallets; i++)
            if (StoragePalletPoint(i).IsSet && _storedDeliveryCrates[i] < CratesPerPallet) return i;
        return -1;
    }

    private void TryManagerSupplierDeposit(DateTime now)
    {
        int slot = _managerSupplierPallet;
        if (!ManagerSupplierEligible() || _supplierPlayerCarrying || slot < 0 ||
            slot >= _ownedStoragePallets || !StoragePalletPoint(slot).IsSet ||
            _supplierDriver == null || !_supplierDriver.Exists() || _supplierDriver.IsDead ||
            _supplierCrate == null || !_supplierCrate.Exists() || !_supplierBoxAttachedToDriver)
        { DeferSupplierPlayerHandoff(now); return; }
        if (DistanceSquared(_supplierDriver.Position, StoragePalletPoint(slot).Position) > 4f)
        { DeferSupplierPlayerHandoff(now); return; }
        // The grounded support/layout must exist before a nearby physical receipt.
        UpdateSupplierPalletVisual();
        Vector3 position; float heading;
        if (_storagePalletProps[slot] == null || !_storagePalletProps[slot].Exists() ||
            !TryStoredBoxTransform(slot, _storedDeliveryCrates[slot], true, out position, out heading))
        { DeferSupplierPlayerHandoff(now); return; }
        if (!CommitSupplierDelivery(slot, _supplierVisualOrderTicks))
        { DeferSupplierPlayerHandoff(now); return; }
        // Accounting is durable before visuals. Provenance reconstructs the box even
        // if a native/entity operation fails after this point.
        DeleteOwnedProp(ref _supplierCrate);
        _supplierBoxAttachedToDriver = false;
        RefreshStoragePalletAfterDeposit();
        BeginSupplierTruckDeparture(now);
    }

    private bool CommitSupplierDelivery(int palletSlot = -1, long expectedOrderTicks = 0L)
    {
        if (_deliveryKind == -2 || _deliveryQuantity <= 0 ||
            (expectedOrderTicks != 0L && expectedOrderTicks != _deliveryDueUtcTicks)) return false;
        if (!CanStoreShipment(_deliveryQuantity))
        { Notify("Delivery awaiting capacity: buy/place pallets before stocking the box."); return false; }
        if (palletSlot >= 0 && (palletSlot >= _ownedStoragePallets ||
            !StoragePalletPoint(palletSlot).IsSet || _storedDeliveryCrates[palletSlot] >= CratesPerPallet))
            return false;
        // Save atomically replaces one complete snapshot. Roll back memory on failure.
        long oldDue = _deliveryDueUtcTicks, oldCost = _deliveryCost;
        double oldRemaining = _deliveryRemainingGameSeconds, oldTimer = _supplierTimerSaveSeconds;
        bool oldPrepaid = _deliveryPrepaid, oldLoaded = _wholesaleCargoLoaded, oldUnloaded = _wholesaleCargoUnloaded;
        bool oldDirty = _palletVisualDirty;
        int oldMask = palletSlot >= 0 ? _deliveredPackageMasks[palletSlot] : 0;
        int kind = _deliveryKind;
        int quantity = _deliveryQuantity;
        int[] oldLines = _deliveryLines;
        long invoice = _deliveryPrepaid ? 0L : _deliveryCost;
        ApplySupplierInventory(kind, quantity, oldLines, 1);
        if (palletSlot >= 0)
        {
            _deliveredPackageMasks[palletSlot] |= 1 << _storedDeliveryCrates[palletSlot];
            _storedDeliveryCrates[palletSlot]++;
            _palletVisualDirty = true;
        }
        _supplierInvoicesDue += invoice;
        _completedSupplierDeliveries++;
        _deliveryKind = -2;
        _deliveryLines = null;
        _deliveryQuantity = 0;
        _deliveryDueUtcTicks = 0L;
        _deliveryRemainingGameSeconds = 0;
        _supplierTimerSaveSeconds = 0;
        _deliveryCost = 0;
        _deliveryPrepaid = false;
        _wholesaleCargoLoaded = false;
        _wholesaleCargoUnloaded = false;
        if (!Save())
        {
            ApplySupplierInventory(kind, quantity, oldLines, -1);
            _deliveryLines = oldLines;
            if (palletSlot >= 0)
            { _storedDeliveryCrates[palletSlot]--; _deliveredPackageMasks[palletSlot] = oldMask; }
            _palletVisualDirty = oldDirty;
            _supplierInvoicesDue -= invoice; _completedSupplierDeliveries--;
            _deliveryKind = kind; _deliveryQuantity = quantity; _deliveryCost = oldCost;
            _deliveryDueUtcTicks = oldDue; _deliveryRemainingGameSeconds = oldRemaining;
            _supplierTimerSaveSeconds = oldTimer; _deliveryPrepaid = oldPrepaid;
            _wholesaleCargoLoaded = oldLoaded; _wholesaleCargoUnloaded = oldUnloaded;
            return false;
        }
        Notify("STOCKED " + quantity + " " + (oldLines == null ? DeliveryItemName(kind) : "supplier order units") +
            (invoice > 0 ? "; supplier invoice $" + invoice.ToString("N0") + " now DUE." :
                "; previously paid purchase completed."));
        return true;
    }

    private void ProcessSupplierDelivery(DateTime now)
    {
        if (Game.IsPaused) return;
        if (_supplierVisualStage == 5)
        {
            if (!_supplierDepartIssued && _supplierDriver != null && _supplierDriver.Exists() &&
                _supplierTruck != null && _supplierTruck.Exists() && _supplierDriver.IsInVehicle())
            {
                _supplierDepartIssued = true;
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,
                    _supplierDriver.Handle, _supplierTruck.Handle,
                    _supplierTruckSpawn.Position.X, _supplierTruckSpawn.Position.Y, _supplierTruckSpawn.Position.Z,
                    9.0f, CarefulDrivingStyle, 8f);
            }
            if ((now - _supplierStageAt).TotalSeconds > 26 ||
                _supplierTruck == null || !_supplierTruck.Exists()) DeleteSupplierVisuals();
            return;
        }
        if (_deliveryKind == -2)
        { if (_supplierVisualStage > 0) DeleteSupplierVisuals(); DeleteOwnedProp(ref _wholesaleWarehousePallet); return; }
        if (_deliveryRemainingGameSeconds > 0) return;
        if (_deliveryQuantity <= 0) { _deliveryKind = -2; Save(); DeleteSupplierVisuals(); return; }
        MaintainSupplierCarryPose(now);
        if (_supplierVisualStage == 6 && _supplierPlayerCarrying) return; // No vehicle required during player handoff.
        if (ManagerSupplierEligible() && !_supplierPlayerCarrying &&
            !_wholesaleCargoLoaded && !_wholesaleCargoUnloaded && SupplierPlayerAway())
        {
            if (_supplierVisualStage > 0) DeleteSupplierVisuals();
            int slot = AvailableSupplierPallet();
            if (slot >= 0 && now >= _nextSupplierVisualRetryUtc)
            {
                _nextSupplierVisualRetryUtc = now.AddSeconds(10);
                if (CommitSupplierDelivery(slot))
                { _storageVisualAttempts = 0; _nextStorageVisualAttempt = DateTime.MinValue; }
            }
            return;
        }
        if (_managerSupplierPallet >= 0 && !ManagerSupplierEligible())
        { DeferSupplierPlayerHandoff(now); return; }
        if (_deliverySupplier == 0)
        {
            if (!_wholesaleWarehousePickup.IsSet) return;
            if (_wholesaleCargoLoaded) { _supplierVisualStage = 9; RefreshWholesaleCargoVisual(); return; }
            if (_wholesaleCargoUnloaded) { _supplierVisualStage = 6; return; }
            // Wholesale replaces the Mule with a physical pickup mission.
            _supplierVisualStage = 7;
            // Ready cargo is streamed by player distance, independently of
            // the assigned vehicle. Vehicle identity is checked when LOADING.
            UpdateWholesalePalletVisual();
            return;
        }
        if (_deliverySupplier != 0 && !_supplierPlayerCarrying && SupplierPlayerAway())
        { if (_supplierVisualStage > 0) DeferSupplierPlayerHandoff(now); return; }
        if (ManagerSupplierEligible() && (!_physicalMuleDeliveries || !MuleRouteConfigured())) return;
        if (!_physicalMuleDeliveries || !MuleRouteConfigured())
        {
            // Legacy fallback still requires capacity; no paid order is discarded.
            CommitSupplierDelivery();
            return;
        }
        if (_supplierVisualStage == 0)
        {
            if (now >= _nextSupplierVisualRetryUtc)
            { _nextSupplierVisualRetryUtc = now.AddSeconds(10); StartMuleDelivery(now); }
            return;
        }
        if (_supplierDriver == null || !_supplierDriver.Exists() ||
            _supplierTruck == null || !_supplierTruck.Exists())
        {
            DeferSupplierPlayerHandoff(now); // original order retained; bounded retry
            return;
        }
        if (_supplierDriver.IsDead)
        { DeferSupplierPlayerHandoff(now); return; }
        if (ManagerSupplierEligible() && _managerSupplierPallet < 0 &&
            (_supplierVisualStage == 3 || _supplierVisualStage == 4))
        { BeginSupplierDriverWalk(now); return; }
        double elapsed = (now - _supplierStageAt).TotalSeconds;
        if (_managerSupplierPallet >= 0 && _supplierVisualStage == 4)
        { TryManagerSupplierDeposit(now); return; }
        if (_supplierDirectToPlayer && (_supplierVisualStage == 3 || _supplierVisualStage == 4))
        {
            UpdateSupplierPlayerApproach(now);
            return;
        }
        if (_supplierVisualStage == 1)
        {
            if (DistanceSquared(_supplierTruck.Position, _supplierTruckPark.Position) <= 120f)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_PARK,
                    _supplierDriver.Handle, _supplierTruck.Handle,
                    _supplierTruckPark.Position.X, _supplierTruckPark.Position.Y, _supplierTruckPark.Position.Z,
                    _supplierTruckPark.Heading, 1, 14f, true);
                _supplierVisualStage = 2;
                _supplierStageAt = now;
            }
            else if (elapsed > 110) { DeferSupplierPlayerHandoff(now); }
        }
        else if (_supplierVisualStage == 2)
        {
            if (elapsed > 4 && !_supplierExitIssued)
            {
                _supplierExitIssued = true;
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE,
                    _supplierDriver.Handle, _supplierTruck.Handle, 0);
            }
            if (_supplierExitIssued && !_supplierDriver.IsInVehicle()) BeginSupplierRearUnload(now);
            else if (elapsed > 25) // blocked exit: warp driver, retain paid order
            {
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE,
                    _supplierDriver.Handle, _supplierTruck.Handle, 16);
                BeginSupplierRearUnload(now);
            }
        }
        else if (_supplierVisualStage == 8)
        {
            if (elapsed > 45) { DeferSupplierPlayerHandoff(now); return; }
            Vector3 rear = _supplierTruck.Position - _supplierTruck.ForwardVector * 3f;
            if (DistanceSquared(_supplierDriver.Position, rear) < 6.25f || elapsed > 16)
                GiveBoxToDriver(now);
        }
        else if (_supplierVisualStage == 3)
        {
            if (_supplierRouteIndex >= _supplierWalkingNodes.Count)
            { _supplierVisualStage = 4; _supplierStageAt = now; return; }
            Vector3 goal = _supplierWalkingNodes[_supplierRouteIndex];
            if (DistanceSquared(_supplierDriver.Position, goal) < 4f)
            {
                _supplierRouteIndex++;
                _supplierStageAt = now;
                if (_supplierRouteIndex < _supplierWalkingNodes.Count) TaskSupplierWalkingNode();
                else { _supplierVisualStage = 4; _supplierStageAt = now; }
            }
            else if (elapsed > 25)
            {
                // Safe user-saved walking marker recovery. Do not lose paid parts.
                if (_managerSupplierPallet < 0 && _routeRecoveryEnabled && _safeTeleportOnStall)
                {
                    _supplierDriver.Position = goal;
                    _supplierRouteIndex++;
                    _supplierStageAt = now;
                    if (_supplierRouteIndex < _supplierWalkingNodes.Count) TaskSupplierWalkingNode();
                    else { _supplierVisualStage = 4; _supplierStageAt = now; }
                }
                else { DeferSupplierPlayerHandoff(now); }
            }
        }
        else if (_supplierVisualStage == 4)
        {
            if (!_supplierBoxAttachedToDriver) GiveBoxToDriver(now);
            // Hold the crate with both hands (box-carry animation) until player receives it.
            if (!_supplierPartsPallet.IsSet && elapsed > 5 && CommitSupplierDelivery())
                BeginSupplierTruckDeparture(now);
        }
        else if (_supplierVisualStage == 6)
        {
            // Player has the physical box. Do NOT commit stock or timeout this
            // paid delivery until they carry it to the parts pallet.
            if (!_supplierPlayerCarrying) { _supplierVisualStage = 4; _supplierStageAt = now; }
        }
    }

    private void ExecuteSupplierDeliverySetup()
    {
        if (_selection == 0)
        {
            if (_supplierVisualStage > 0 && _deliveryKind != -2)
            { Notify("Complete the current Mule handoff before changing delivery mode."); return; }
            _physicalMuleDeliveries = !_physicalMuleDeliveries;
            Save(); Notify("Physical Mule supplier delivery " + (_physicalMuleDeliveries ? "ON" : "OFF") + ".");
        }
        else if (_selection == 1) OpenPointEditor(_supplierTruckSpawn, "Mule Truck Road Spawn", -1, true);
        else if (_selection == 2) OpenPointEditor(_supplierTruckPark, "Mule Unloading Parking", -1, true);
        else if (_selection == 3) OpenPointEditor(_supplierHandoff, "Driver Box Handoff", -1, true);
        else if (_selection == 4) { _menuPage = MenuPage.SupplierPalletStorage; _selection = 0; }
        else if (_selection == 5) { _menuPage = MenuPage.SupplierDriverWaypoints; _selection = 0; }
        else if (_selection == 6) ShowStatusNotice("FIXED WHOLESALE WAREHOUSE",
            "X -164.2800 | Y -2676.5900 | Z 6.0141",
            "Pickup happens only with a registered player vehicle.");
        else if (_selection == 7) AssignWholesaleVehicle();
        else if (_selection == 8) ShowStatusNotice("WHOLESALE PICKUP VEHICLE",
            "Plate " + (HasWholesaleVehicle() ? _wholesaleVehiclePlate : "NOT ASSIGNED"),
            "Vehicle model " + _wholesaleVehicleModel + " | Cargo " + (_wholesaleCargoLoaded ? "LOADED" : "EMPTY"),
            "Order " + (_deliverySupplier == 0 && _deliveryKind != -2 ? "WHOLESALE ACTIVE" : "NONE"),
            "Drive to warehouse, load box, return to shop, unload and stock.");
        else if (_selection == 10) RecoverSupplierShipment();
        else if (_selection == 11) CancelSupplierOrderWithConfirmation();
        else if (_selection == 9) ShowStatusNotice("SUPPLIER DELIVERY ROUTE",
            "Model " + _muleModelName + " | Physical " + (_physicalMuleDeliveries ? "ON" : "OFF"),
            "Truck spawn " + Status(_supplierTruckSpawn) + " | unload " + Status(_supplierTruckPark),
            "Handoff " + Status(_supplierHandoff) + " | Pallet " + Status(_supplierPartsPallet),
            "Walk stops " + CountSet(_supplierDriverWaypoints) + "/3" + " | " +
                (_supplierPartsPallet.IsSet ? "PLAYER HANDOFF" : "AUTO HANDOFF"),
            "Delivery " + (_deliveryKind == -2 ? "NONE" : MuleStageName()) +
                " | Warehouse " + Status(_wholesaleWarehousePickup) +
                " | Storage " + StoredPartsTotal() + "/" + TotalStorageCapacity());
        else NavigateBack();
    }

    // Recovery never credits inventory, pays an invoice or starts a second order.
    private void RecoverSupplierShipment()
    {
        if (_deliveryKind == -2) { Notify("No supplier shipment to recover."); return; }
        try
        {
            Ped player = Game.Player.Character;
            if (player != null && player.Exists() && _supplierPlayerCarrying)
                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_SECONDARY_TASK, player.Handle);
        }
        catch (Exception ex) { Log("Supplier recovery pose cleanup", ex); }
        DeleteSupplierVisuals();
        _wholesaleCargoLoaded = false;
        _wholesaleCargoUnloaded = false;
        _cachedWholesaleVehicle = null;
        _nextWholesaleVehicleLookup = DateTime.MinValue;
        _supplierVisualStage = 0;
        Save();
        SupplierCheckpoint("manual recover - original order retained");
        Notify(_deliverySupplier == 0 ?
            "Wholesale shipment reset. Collect original order at warehouse when ETA reaches zero." :
            "Mule shipment reset. Original order will be retried after ETA.");
    }

    // Two intentional presses required; no stock or invoices were booked yet.
    private void CancelSupplierOrderWithConfirmation()
    {
        if (_deliveryKind == -2) { Notify("No pending supplier order to cancel."); return; }
        DateTime now = DateTime.UtcNow;
        if (now > _cancelSupplierConfirmUntil)
        {
            _cancelSupplierConfirmUntil = now.AddSeconds(12);
            Notify("Press CANCEL PENDING ORDER again within 12 seconds. No stock will be added.");
            return;
        }
        _cancelSupplierConfirmUntil = DateTime.MinValue;
        bool wasCarrying = _supplierPlayerCarrying;
        bool prepaid = _deliveryPrepaid;
        if (!TryCancelSupplierOrder()) return;
        try
        {
            Ped player = Game.Player.Character;
            if (player != null && player.Exists() && wasCarrying)
                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_SECONDARY_TASK, player.Handle);
        }
        catch (Exception ex) { Log("Supplier cancellation pose cleanup", ex); }
        DeleteSupplierVisuals();
        _cachedWholesaleVehicle = null;
        SupplierCheckpoint("manual cancellation");
        Notify(prepaid ? "Order cancelled; prepaid amount refunded once." : "Order cancelled; no invoice or stock created.");
    }

    private bool TryCancelSupplierOrder()
    {
        if (Game.IsPaused || _deliveryKind == -2) return false;
        int kind = _deliveryKind, quantity = _deliveryQuantity;
        int[] lines = _deliveryLines;
        long cost = _deliveryCost, identity = _deliveryDueUtcTicks, cash = _cash, refunds = _financeRefundIncome;
        double remaining = _deliveryRemainingGameSeconds, timer = _supplierTimerSaveSeconds;
        bool prepaid = _deliveryPrepaid, loaded = _wholesaleCargoLoaded, unloaded = _wholesaleCargoUnloaded;
        _deliveryKind = -2; _deliveryQuantity = 0; _deliveryLines = null;
        _deliveryCost = 0; _deliveryDueUtcTicks = 0; _deliveryRemainingGameSeconds = 0;
        _supplierTimerSaveSeconds = 0; _deliveryPrepaid = false;
        _wholesaleCargoLoaded = false; _wholesaleCargoUnloaded = false;
        if (prepaid) { _cash += cost; _financeRefundIncome += cost; }
        if (Save()) return true;
        _deliveryKind = kind; _deliveryQuantity = quantity; _deliveryLines = lines;
        _deliveryCost = cost; _deliveryDueUtcTicks = identity; _cash = cash; _financeRefundIncome = refunds;
        _deliveryRemainingGameSeconds = remaining; _supplierTimerSaveSeconds = timer;
        _deliveryPrepaid = prepaid; _wholesaleCargoLoaded = loaded; _wholesaleCargoUnloaded = unloaded;
        return false;
    }

    private void ExecuteSupplierDriverWaypoints()
    {
        if (_selection >= 3) { NavigateBack(); return; }
        OpenPointEditor(_supplierDriverWaypoints[_selection], "Driver Walk " + (_selection + 1) + "/3", -1, true);
    }

    private WorkBay StoragePalletPoint(int slot)
    {
        return slot == 0 ? _supplierPartsPallet : _extraStoragePallets[slot - 1];
    }

    private int StoredPartsTotal()
    {
        return _parts + _specialStock[0] + _specialStock[1] + _specialStock[2];
    }

    private int TotalStorageCapacity()
    {
        int placed = 0;
        for (int i = 0; i < _ownedStoragePallets; i++)
            if (StoragePalletPoint(i).IsSet) placed++;
        return placed * PartsPerPallet;
    }

    private int DepositPalletNear(Vector3 location)
    {
        int selected = -1;
        float bestDistance = 8f; // sqrt(8) ~= 2.8m, the on-foot handoff range.
        for (int i = 0; i < _ownedStoragePallets; i++)
        {
            WorkBay point = StoragePalletPoint(i);
            if (!point.IsSet || _storedDeliveryCrates[i] >= CratesPerPallet) continue;
            float dist = DistanceSquared(location, point.Position);
            if (dist < bestDistance) { bestDistance = dist; selected = i; }
        }
        return selected;
    }

    private bool CanStoreShipment(int quantity)
    {
        if (quantity <= 0 || StoredPartsTotal() + quantity > TotalStorageCapacity()) return false;
        for (int i = 0; i < _ownedStoragePallets; i++)
            if (StoragePalletPoint(i).IsSet && _storedDeliveryCrates[i] < CratesPerPallet) return true;
        return false;
    }

    private void ExecuteSupplierPalletStorage()
    {
        if (_selection == 0)
        {
            ShowStatusNotice("PALLET INVENTORY CAPACITY",
                "Stored " + StoredPartsTotal() + "/" + TotalStorageCapacity() + " parts; " + _ownedStoragePallets + " purchased",
                "Pallet boxes: " + _storedDeliveryCrates[0] + ", " + _storedDeliveryCrates[1] +
                    ", " + _storedDeliveryCrates[2] + ", " + _storedDeliveryCrates[3] + " / 9 each",
                "Each placed pallet supports 25 units; previous excess stock is preserved.",
                "New supply orders require capacity for their entire quantity.");
        }
        else if (_selection == 1)
        {
            if (_ownedStoragePallets >= MaxStoragePallets) { Notify("Maximum four storage pallets owned."); return; }
            if (_cash < ExtraPalletPrice) { Notify("Another pallet costs $750; insufficient business cash."); return; }
            _cash -= ExtraPalletPrice;
            _equipmentSpend += ExtraPalletPrice;
            _ownedStoragePallets++;
            DeleteStoragePalletVisuals();
            Save();
            Notify("Purchased pallet " + _ownedStoragePallets + "/4. Place it to unlock capacity.");
        }
        else if (_selection >= 2 && _selection <= 5)
        {
            int slot = _selection - 2;
            if (slot >= _ownedStoragePallets) { Notify("Buy this pallet before placing it."); return; }
            OpenPointEditor(StoragePalletPoint(slot), "Storage Pallet " + (slot + 1), -1, true);
        }
        else NavigateBack();
    }

    private void ShowDetailedInventory()
    {
        ShowStatusNotice("PARTS INVENTORY",
            "Universal " + _parts + " | Filters " + _specialStock[0],
            "Brake kits " + _specialStock[1] + " | Diagnostics " + _specialStock[2],
            "Storage " + StoredPartsTotal() + "/" + TotalStorageCapacity() +
                " | Supplier invoices $" + _supplierInvoicesDue.ToString("N0"),
            "Order: " + (_deliveryKind == -2 ? "NONE" : _deliveryQuantity + " " + (_deliveryLines == null ? DeliveryItemName(_deliveryKind) : "mixed parts") + " PENDING"));
        Notify("Manual parts sales $" + _partsSalesRevenue.ToString("N0") + " | " +
            _partsSalesUnitsSold + " units; stock reserved for repairs cannot be sold.");
    }

    private int[] _deliveryLines; // Null means an unchanged legacy single-item order.
    private readonly int[] _supplierCart = new int[4]; // Unsubmitted draft; never stock.

    private void BuyInventory(int kind, int quantity)
    {
        if (kind < -1 || kind > 2) return;
        int[] lines = new int[4]; lines[kind + 1] = quantity;
        TryPlaceSupplierOrder(lines);
    }

    private static int SupplierLineTotal(int[] lines)
    {
        if (lines == null || lines.Length != 4) return -1;
        int total = 0;
        foreach (int quantity in lines)
        {
            if (quantity < 0 || quantity > 100) return -1;
            total += quantity;
        }
        return total <= 100 ? total : -1;
    }

    private long SupplierCartCost(int[] lines)
    {
        if (SupplierLineTotal(lines) < 0) return 0L;
        long cost = 0;
        for (int i = 0; i < 4; i++)
            if (lines[i] > 0) cost += InventoryPurchaseCost(i - 1, lines[i]);
        return cost;
    }

    private bool TryPlaceSupplierOrder(int[] requested)
    {
        int total = SupplierLineTotal(requested);
        if (Game.IsPaused || _deliveryKind != -2 || total <= 0)
        { Notify("Order not placed: unpause, clear the pending shipment, and choose at least one item (maximum 100 total)."); return false; }
        if (!CanStoreShipment(total))
        { Notify("Order exceeds available pallet capacity. No order or invoice created."); return false; }
        if (_selectedSupplier == 0 && !HasWholesaleVehicle())
        { Notify("Wholesale requires an assigned pickup vehicle in Supplier Settings."); return false; }
        int oldKind = _deliveryKind, oldQuantity = _deliveryQuantity, oldSupplier = _deliverySupplier;
        int[] oldLines = _deliveryLines;
        long oldCost = _deliveryCost, oldIdentity = _deliveryDueUtcTicks, oldSavings = _managerSavings;
        double oldRemaining = _deliveryRemainingGameSeconds, oldTimer = _supplierTimerSaveSeconds;
        bool oldPrepaid = _deliveryPrepaid, oldLoaded = _wholesaleCargoLoaded, oldUnloaded = _wholesaleCargoUnloaded;
        _deliveryLines = (int[])requested.Clone();
        _deliveryKind = 0;
        while (_deliveryLines[_deliveryKind] == 0) _deliveryKind++;
        _deliveryKind--; // Keep a valid legacy kind; the additive payload owns distribution.
        _deliveryQuantity = total;
        _deliverySupplier = _selectedSupplier;
        _deliveryCost = SupplierCartCost(_deliveryLines);
        long noManagerCost = 0;
        int pct = _selectedSupplier == 0 ? 90 : _selectedSupplier == 1 ? 100 : 125;
        for (int i = 0; i < 4; i++)
            if (_deliveryLines[i] > 0)
                noManagerCost += ((long)(i == 0 ? _partCost : _specialUnitCosts[i - 1]) * _deliveryLines[i] * pct + 99) / 100;
        _managerSavings += Math.Max(0L, noManagerCost - _deliveryCost);
        _deliveryPrepaid = false; _wholesaleCargoLoaded = false; _wholesaleCargoUnloaded = false;
        _deliveryDueUtcTicks = DateTime.UtcNow.Ticks; // Identity only; never elapsed progress.
        _deliveryRemainingGameSeconds = SupplierDeliverySeconds(_selectedSupplier);
        _supplierTimerSaveSeconds = 0;
        if (!Save())
        {
            _deliveryKind = oldKind; _deliveryQuantity = oldQuantity; _deliverySupplier = oldSupplier;
            _deliveryLines = oldLines; _deliveryCost = oldCost; _deliveryDueUtcTicks = oldIdentity;
            _managerSavings = oldSavings; _deliveryRemainingGameSeconds = oldRemaining;
            _supplierTimerSaveSeconds = oldTimer; _deliveryPrepaid = oldPrepaid;
            _wholesaleCargoLoaded = oldLoaded; _wholesaleCargoUnloaded = oldUnloaded;
            return false;
        }
        DeleteSupplierVisuals();
        _nextSupplierVisualRetryUtc = DateTime.MinValue;
        SetSupplierPickupWaypoint();
        Notify("Order saved: " + total + " units | " + SupplierName(_deliverySupplier) + " | $" + _deliveryCost.ToString("N0") +
            " invoice after stocking. " + (_deliverySupplier == 0 ? "Player warehouse pickup required." : "One shipment / one ETA."));
        return true;
    }

    private void SetSupplierPickupWaypoint()
    {
        if (_deliverySupplier != 0 || !_wholesaleWarehousePickup.IsSet) return;
        try { GTA.Native.Function.Call(GTA.Native.Hash.SET_NEW_WAYPOINT,
            _wholesaleWarehousePickup.Position.X, _wholesaleWarehousePickup.Position.Y); } catch { }
    }

    private void ApplySupplierInventory(int kind, int quantity, int[] lines, int direction)
    {
        if (lines == null)
        { if (kind < 0) _parts += quantity * direction; else _specialStock[kind] += quantity * direction; }
        else
        {
            _parts += lines[0] * direction;
            for (int i = 0; i < 3; i++) _specialStock[i] += lines[i + 1] * direction;
        }
    }

    private void LoadSupplierOrderLines(ScriptSettings data)
    {
        int version = data.GetValue("SupplierOrderV1", "Version", 0);
        if (version == 0) return; // Legacy single-item order, including prepaid.
        if (version != 1) throw new InvalidDataException("Unsupported supplier cart version. Preserve the save for review.");
        int[] lines = new int[4];
        for (int i = 0; i < 4; i++) lines[i] = data.GetValue("SupplierOrderV1", "Line" + i, 0);
        int total = SupplierLineTotal(lines);
        long identity = data.GetValue("SupplierOrderV1", "OrderTicks", 0L);
        if (total == 0 && identity == 0L) return; // Cleared cart or legacy order snapshot.
        if (total <= 0 || _deliveryKind == -2 || total != _deliveryQuantity || identity != _deliveryDueUtcTicks ||
            data.GetValue("SupplierOrderV1", "Cost", -1L) != _deliveryCost ||
            data.GetValue("SupplierOrderV1", "Supplier", -1) != _deliverySupplier)
            throw new InvalidDataException("Supplier cart does not match pending order. Restore matching source/data; do not delete keys.");
        int first = 0; while (lines[first] == 0) first++;
        if (first - 1 != _deliveryKind) throw new InvalidDataException("Supplier cart kind mismatch.");
        _deliveryLines = lines;
    }

    private void WriteSupplierOrderLines(ScriptSettings data)
    {
        data.SetValue("SupplierOrderV1", "Version", 1);
        for (int i = 0; i < 4; i++) data.SetValue("SupplierOrderV1", "Line" + i, _deliveryLines == null ? 0 : _deliveryLines[i]);
        data.SetValue("SupplierOrderV1", "OrderTicks", _deliveryLines == null ? 0L : _deliveryDueUtcTicks);
        data.SetValue("SupplierOrderV1", "Cost", _deliveryLines == null ? 0L : _deliveryCost);
        data.SetValue("SupplierOrderV1", "Supplier", _deliveryLines == null ? -1 : _deliverySupplier);
    }

    private void AdjustSupplierCart(int direction)
    {
        if (_menuPage != MenuPage.BuildSupplierOrder || _selection < 0 || _selection >= 4) return;
        _supplierCart[_selection] = Math.Max(0, Math.Min(100, _supplierCart[_selection] + direction));
    }

    private void ExecuteSupplierCart()
    {
        if (_selection < 4) AdjustSupplierCart(1);
        else if (_selection == 4) { _selectedSupplier = (_selectedSupplier + 1) % 3; Save(); }
        else if (_selection == 5)
        {
            if (SupplierLineTotal(_supplierCart) < 0) Notify("Cart exceeds the 100-unit limit. Reduce quantities before previewing or ordering.");
            else Notify("Cart " + SupplierLineTotal(_supplierCart) + " units | $" + SupplierCartCost(_supplierCart).ToString("N0") + " | free capacity " + Math.Max(0, TotalStorageCapacity() - StoredPartsTotal()));
        }
        else if (_selection == 6) { if (TryPlaceSupplierOrder(_supplierCart)) Array.Clear(_supplierCart, 0, 4); }
        else if (_selection == 7)
        { _supplierCart[0] = Math.Max(0, Math.Max(5, _reorderThreshold) - _parts); for (int i = 1; i < 4; i++) _supplierCart[i] = Math.Max(0, _reorderThreshold - _specialStock[i - 1]); }
        else if (_selection == 8) Array.Clear(_supplierCart, 0, 4);
        else NavigateBack();
    }

    private void ShowSupplierOrderContents()
    {
        if (_deliveryKind == -2) { Notify("No pending supplier order."); return; }
        int[] lines = _deliveryLines;
        if (lines == null) { lines = new int[4]; lines[_deliveryKind + 1] = _deliveryQuantity; }
        ShowStatusNotice("PENDING ORDER CONTENTS", "Universal " + lines[0] + " | Filters " + lines[1],
            "Brake kits " + lines[2] + " | Diagnostics " + lines[3],
            SupplierName(_deliverySupplier) + " | " + (int)Math.Ceiling(_deliveryRemainingGameSeconds) + " active seconds remaining",
            "$" + _deliveryCost.ToString("N0") + (_deliveryPrepaid ? " PREPAID" : " consolidated invoice on stocking"));
    }

    private void ShowReorderAdvice()
    {
        ShowStatusNotice("REORDER ADVICE", "Universal " + _parts + " | Filters " + _specialStock[0],
            "Brake kits " + _specialStock[1] + " | Diagnostics " + _specialStock[2],
            "Low stock threshold " + _reorderThreshold, "Pending cargo is excluded from stock on hand.");
    }

    private void ExecuteInventorySelection()
    {
        MenuPage[] destinations = { MenuPage.InventoryOverview, MenuPage.BuildSupplierOrder,
            MenuPage.ActiveDeliveries, MenuPage.PartsSales, MenuPage.SupplierPalletStorage, MenuPage.SupplierDeliverySetup };
        if (_selection >= destinations.Length) { NavigateBack(); return; }
        _menuPage = destinations[_selection]; _selection = 0;
    }

    private bool BuyEquipment(int price, string name)
    {
        long effectivePrice = price * (100 - ManagerDiscountPercent()) / 100;
        if (_cash < effectivePrice) { Notify(name + ": need $" + effectivePrice + ", available $" + _cash); return false; }
        _cash -= effectivePrice;
        _equipmentSpend += effectivePrice;
        _managerSavings += price - effectivePrice;
        Save();
        Notify("Purchased " + name + " for $" + price.ToString("N0"));
        return true;
    }

    private bool SetOwnedEquipmentPosition(bool owned, WorkBay point, string label)
    {
        if (!owned) { Notify("Purchase " + label + " before placing it."); return false; }
        if (_activeCustomerOrders.Count > 0)
        { Notify("Move equipment between active repairs to protect customer paths."); return false; }
        Ped player = Game.Player.Character;
        if (player == null || !player.Exists() || player.IsInVehicle())
        { Notify("Stand on foot at the intended equipment position."); return false; }
        // Stand where the player can see the intended location. Equipment is placed
        // FORWARD onto the same floor level, not at the pedestrian's body origin.
        // Save the player's current indoor Z; the grounded-spawn method aligns
        // the prop to MLO collision at creation, if the collision is available.
        Vector3 forward = player.ForwardVector;
        float flatLength = (float)Math.Sqrt(forward.X * forward.X + forward.Y * forward.Y);
        if (flatLength < 0.15f) { Notify("Face the spot where you want the equipment."); return false; }
        const float placeMeters = 2.2f;
        point.IsSet = true;
        point.Position = new Vector3(player.Position.X + forward.X / flatLength * placeMeters,
            player.Position.Y + forward.Y / flatLength * placeMeters, player.Position.Z);
        point.Heading = player.Heading;
        return true;
    }

    private void ExecuteEquipmentSelection()
    {
        switch (_selection)
        {
            case 0: ShowStatusNotice("SHOP EQUIPMENT",
                "Workshop tool set: " + (_toolSetPurchased ? (_toolSetPlaced ? "OWNED / PLACED" : "OWNED / NOT PLACED") : "NOT OWNED") + " (-7% all repairs)",
                "Diagnostic scanner: " + (_diagnosticScannerPurchased ? (_scannerPlaced ? "OWNED / PLACED" : "OWNED / NOT PLACED") : "NOT OWNED") + " (-10% diagnostics)",
                "Office computer: " + (_officeDeskPurchased ? (_officeDeskPlaced ? "PLACED" : "OWNED / NOT PLACED") : "NOT OWNED"),
                "Equipment spending: $" + _equipmentSpend.ToString("N0") + "; placement is cosmetic."); break;
            case 1: if (!_toolSetPurchased && BuyEquipment(1800, "workshop tool set")) { _toolSetPurchased = true; Save(); } break;
            case 2: if (!_diagnosticScannerPurchased && BuyEquipment(1600, "diagnostic scanner")) { _diagnosticScannerPurchased = true; Save(); } break;
            case 3: if (!_officeDeskPurchased && BuyEquipment(2400, "office computer desk")) { _officeDeskPurchased = true; Save(); } break;
            case 4:
                if (SetOwnedEquipmentPosition(_toolSetPurchased, _toolSetLocation, "the tool set"))
                { _toolSetPlaced = true; DeleteEquipmentProps(); UpdateEquipmentVisuals(); Save(); Notify("Workshop tool chest positioned 2.2m ahead on the floor."); }
                break;
            case 5:
                _toolSetPlaced = false; DeleteEquipmentProps(); Save();
                Notify("Tool chest hidden. Its purchased repair-time benefit remains active."); break;
            case 6:
                if (SetOwnedEquipmentPosition(_diagnosticScannerPurchased, _scannerLocation, "the diagnostic scanner"))
                { _scannerPlaced = true; DeleteEquipmentProps(); UpdateEquipmentVisuals(); Save(); Notify("Diagnostic trolley positioned 2.2m ahead on the floor."); }
                break;
            case 7:
                _scannerPlaced = false; DeleteEquipmentProps(); Save();
                Notify("Diagnostic workstation hidden. Its purchased repair bonus stays active."); break;
            case 8:
                if (SetOwnedEquipmentPosition(_officeDeskPurchased, _officeDeskLocation, "the office computer"))
                { _officeDeskPlaced = true; DeleteOfficeDeskProps(); UpdateOfficeDeskVisuals(); Save();
                  Notify("Office desk positioned ahead. Press E or D-pad RIGHT beside it."); }
                break;
            case 9: _officeDeskPlaced = false; DeleteOfficeDeskProps(); Save();
                Notify("Desk removed; purchase remains owned."); break;
            case 10: ShowStatusNotice("OFFICE COMPUTER",
                "Purchased: " + (_officeDeskPurchased ? "YES" : "NO"),
                "Placed: " + (_officeDeskPlaced ? "YES" : "NO"),
                "Near desk: E or D-pad RIGHT (independent of controller menu toggle)",
                "F2 always works. Controller menu: " + (_controllerEnabled ? "ON" : "OFF")); break;
            default: NavigateBack(); break;
        }
    }

    private void ExecuteControllerSelection()
    {
        switch (_selection)
        {
            case 0:
                // Both GTA frontend input and WinForms KeyDown may report one Enter/A press;
                // plus WinForms repeats held keys. Suppress cross-device/repeat toggles.
                DateTime toggleNow = DateTime.UtcNow;
                if ((toggleNow - _lastControllerToggle).TotalMilliseconds < 900) break;
                _lastControllerToggle = toggleNow;
                _lastControllerAction = toggleNow;
                _controllerEnabled = !_controllerEnabled;
                Save();
                GTA.UI.Screen.ShowSubtitle("Glass Heroes Controller: " + (_controllerEnabled ? "~g~ON" : "~r~OFF") +
                    "~s~ | Release ENTER/A before selecting again", 3600);
                break;
            case 1: ShowStatusNotice("CONTROLLER KEY MAP",
                "Menu: hold " + _controllerOpenModifier + " + tap " + _controllerOpenButton,
                "Move: " + _controllerUp + " / " + _controllerDown,
                "Select " + _controllerAccept + " | Back " + _controllerBack,
                "Office: D-pad RIGHT on foot near desk, even if menu controls OFF",
                "Alternate office input: E | Customize [Gamepad] in Settings.ini"); break;
            case 2: ShowStatusNotice("CONTROLLER DIAGNOSTICS",
                "Enabled: " + (_controllerEnabled ? "YES" : "NO") + " | Last detected: " + _lastPadDetected,
                "Modifier held: " + (PadPressed(_controllerOpenModifier) ? "YES" : "NO"),
                "Open button held: " + (PadPressed(_controllerOpenButton) ? "YES" : "NO"),
                "One press = one toggle; release ENTER/A before pressing again."); break;
            default: NavigateBack(); break;
        }
    }

    private bool IsPlayerAtOfficeDesk()
    {
        if (!_officeDeskPurchased || !_officeDeskPlaced || !_officeDeskLocation.IsSet) return false;
        Ped player = Game.Player.Character;
        return player != null && player.Exists() && !player.IsInVehicle() &&
            DistanceSquared(player.Position, _officeDeskLocation.Position) <= 6.25f;
    }

    private void OpenBusinessFromOffice()
    {
        _showMenu = true;
        _menuPage = MenuPage.BusinessManagement;
        _selection = 0;
        _lastControllerAction = DateTime.UtcNow;
    }

    // v0.3.2.6: Customer deliveries remain PAID but NOT STOCKED until the player
    // receives the box and sets it on the saved pallet. This is separate from UI.
    private sealed class PalletBoxLayout
    {
        public Vector3[][] Offsets;
        public float HeadingOffset;
    }

    // Model-local geometry only. Build once per model pair, never per-frame.
    private static PalletBoxLayout BuildPalletBoxLayout(Vector3 palletMin, Vector3 palletMax,
        Vector3 boxMin, Vector3 boxMax)
    {
        float[] bounds = { palletMin.X, palletMin.Y, palletMin.Z, palletMax.X, palletMax.Y,
            palletMax.Z, boxMin.X, boxMin.Y, boxMin.Z, boxMax.X, boxMax.Y, boxMax.Z };
        foreach (float value in bounds)
            if (float.IsNaN(value) || float.IsInfinity(value)) return null;
        float px = palletMax.X - palletMin.X, py = palletMax.Y - palletMin.Y;
        float bx = boxMax.X - boxMin.X, by = boxMax.Y - boxMin.Y, height = boxMax.Z - boxMin.Z;
        if (px <= 0 || py <= 0 || palletMax.Z <= palletMin.Z || bx <= 0 || by <= 0 || height <= 0) return null;
        const float edge = 0.015f, gap = 0.012f;
        int columns = 0, rows = 0, turn = 0;
        // Prefer 4-4-1, otherwise use the largest centered footprint that fits.
        int[,] grids = { {2,2}, {3,1}, {1,3}, {2,1}, {1,2}, {1,1} };
        for (int grid = 0; grid < grids.GetLength(0) && columns == 0; grid++)
            for (int rotation = 0; rotation < 2; rotation++)
            {
                float x = rotation == 0 ? bx : by, y = rotation == 0 ? by : bx;
                int c = grids[grid,0], r = grids[grid,1];
                if (c * x + (c - 1) * gap <= px - 2 * edge &&
                    r * y + (r - 1) * gap <= py - 2 * edge)
                { columns = c; rows = r; turn = rotation; break; }
            }
        if (columns == 0) return null; // Unsafe geometry is never spawned or billed.
        float width = turn == 0 ? bx : by, depth = turn == 0 ? by : bx;
        float cx = (boxMin.X + boxMax.X) * .5f, cy = (boxMin.Y + boxMax.Y) * .5f;
        float originX = turn == 0 ? cx : -cy, originY = turn == 0 ? cy : cx;
        float palletX = (palletMin.X + palletMax.X) * .5f, palletY = (palletMin.Y + palletMax.Y) * .5f;
        int perLayer = columns * rows;
        var layout = new PalletBoxLayout { Offsets = new Vector3[CratesPerPallet + 1][], HeadingOffset = turn * 90f };
        layout.Offsets[0] = new Vector3[0];
        for (int count = 1; count <= CratesPerPallet; count++)
        {
            layout.Offsets[count] = new Vector3[count];
            for (int index = 0; index < count; index++)
            {
                int layer = index / perLayer, inLayer = index % perLayer;
                int layerCount = Math.Min(perLayer, count - layer * perLayer);
                float x, y;
                if (columns == 2 && rows == 2)
                {
                    if (layerCount == 1) { x = 0; y = 0; }
                    else if (layerCount == 2) { x = (inLayer == 0 ? -.5f : .5f) * (width + gap); y = 0; }
                    else
                    {
                        x = (inLayer % 2 == 0 ? -.5f : .5f) * (width + gap);
                        y = (inLayer / 2 == 0 ? -.5f : .5f) * (depth + gap);
                        if (layerCount == 3 && inLayer == 2) x = 0;
                    }
                }
                else if (rows == 1) { x = (inLayer - (layerCount - 1) * .5f) * (width + gap); y = 0; }
                else { x = 0; y = (inLayer - (layerCount - 1) * .5f) * (depth + gap); }
                layout.Offsets[count][index] = new Vector3(palletX + x - originX, palletY + y - originY,
                    palletMax.Z - boxMin.Z + layer * height);
            }
        }
        return layout;
    }

    private PalletBoxLayout StoredPalletLayout(int palletSlot)
    {
        if (palletSlot < 0 || palletSlot >= _ownedStoragePallets ||
            _storagePalletProps[palletSlot] == null || !_storagePalletProps[palletSlot].Exists()) return null;
        Model pallet = _storagePalletProps[palletSlot].Model;
        Model box = new Model("prop_cs_cardbox_01");
        long key = ((long)(uint)pallet.Hash << 32) | (uint)box.Hash;
        PalletBoxLayout layout;
        if (_palletBoxLayouts.TryGetValue(key, out layout)) return layout;
        if (!RequestVisualModel(box)) return null;
        try
        {
            Vector3 pmin, pmax, bmin, bmax;
            var pd = pallet.Dimensions; var bd = box.Dimensions;
            pmin = pd.Item1; pmax = pd.Item2; bmin = bd.Item1; bmax = bd.Item2;
            layout = BuildPalletBoxLayout(pmin, pmax, bmin, bmax);
            if (layout != null)
            {
                if (_palletBoxLayouts.Count >= MaxStoragePallets * 2) _palletBoxLayouts.Clear();
                _palletBoxLayouts[key] = layout;
            }
            return layout;
        }
        finally { ReleaseVisualModel(box); }
    }

    private static Vector3 RotatePalletOffset(Vector3 origin, Vector3 local, float heading)
    {
        double theta = heading * Math.PI / 180.0;
        float cos = (float)Math.Cos(theta), sin = (float)Math.Sin(theta);
        return origin + new Vector3(local.X * cos - local.Y * sin, local.X * sin + local.Y * cos, local.Z);
    }

    private static int VisiblePackageCount(int boxCount, int mask)
    {
        int count = 0;
        for (int b = 0; b < Math.Min(CratesPerPallet, Math.Max(0, boxCount)); b++)
            if ((mask & (1 << b)) != 0) count++;
        return count;
    }

    private bool TryStoredBoxTransform(int palletSlot, int boxIndex, bool depositing, out Vector3 position, out float heading)
    {
        position = Vector3.Zero; heading = 0;
        if (boxIndex < 0 || boxIndex >= CratesPerPallet) return false;
        PalletBoxLayout layout = StoredPalletLayout(palletSlot);
        if (layout == null) return false;
        int mask = _deliveredPackageMasks[palletSlot];
        int rank = VisiblePackageCount(boxIndex, mask);
        int count = VisiblePackageCount(_storedDeliveryCrates[palletSlot], mask) + (depositing ? 1 : 0);
        if (rank >= count || count > CratesPerPallet) return false;
        WorkBay pallet = StoragePalletPoint(palletSlot);
        Vector3 palletGround = _storagePalletProps[palletSlot].Position;
        position = RotatePalletOffset(palletGround, layout.Offsets[count][rank], pallet.Heading);
        heading = pallet.Heading + layout.HeadingOffset;
        return true;
    }

    private Vector3 StoredBoxPosition(int palletSlot, int boxIndex)
    {
        Vector3 position; float heading;
        if (!TryStoredBoxTransform(palletSlot, boxIndex, false, out position, out heading))
            throw new InvalidOperationException("Pallet box geometry is not ready.");
        return position;
    }

    private bool TrySupplierPackageInteraction()
    {
        if (Game.IsPaused) return false;
        if (_showMenu || _deliveryKind == -2 ||
            Game.Player.Character == null || Game.Player.Character.IsInVehicle()) return false;
        Ped player = Game.Player.Character;
        bool atDriver = _supplierVisualStage == 4 && _supplierDriver != null &&
            _supplierDriver.Exists() && DistanceSquared(player.Position, _supplierDriver.Position) < 10f;
        bool atWarehouse = _supplierVisualStage == 7 && _deliverySupplier == 0 &&
            _wholesaleWarehousePickup.IsSet && !_wholesaleCargoLoaded &&
            DistanceSquared(player.Position, _wholesaleWarehousePickup.Position) < 10f;
        Vehicle assignedNearPlayer = FindAssignedWholesaleVehicle(player.Position, 8f);
        bool loadWholesale = _supplierVisualStage == 6 && _supplierPlayerCarrying &&
            _deliverySupplier == 0 && !_wholesaleCargoUnloaded && assignedNearPlayer != null;
        bool unloadWholesale = _supplierVisualStage == 9 && _wholesaleCargoLoaded &&
            _deliverySupplier == 0 && assignedNearPlayer != null && _placed &&
            DistanceSquared(player.Position, _location) <= 3600f;
        if (loadWholesale)
        {
            // The box stays as a REAL visible prop attached to the assigned vehicle.
            // Inventory and invoice remain untouched until pallet deposit.
            AttachWholesaleBoxToVehicle(assignedNearPlayer);
            if (_supplierCrate == null || !_supplierCrate.Exists())
            { Notify("Cargo model unavailable; loading not completed. Try again."); return true; }
            _wholesaleCargoLoaded = true;
            _supplierPlayerCarrying = false;
            _supplierVisualStage = 9;
            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_SECONDARY_TASK, player.Handle);
            Save(); SupplierCheckpoint("package loaded into assigned vehicle");
            Notify("Shipment LOADED into vehicle cargo area. Return to Glass Heroes to unload.");
            return true;
        }
        if (unloadWholesale)
        {
            // Reuse the vehicle-attached box, rather than spawning a replacement.
            if (_supplierCrate == null || !_supplierCrate.Exists())
                _supplierCrate = CreateFixedEquipmentProp("prop_cs_cardbox_01",
                    player.Position, player.Heading, false);
            if (_supplierCrate == null || !_supplierCrate.Exists())
            { Notify("Cargo box still loading; try unloading again."); return true; }
            AttachSupplierBoxToPed(player);
            _wholesaleCargoLoaded = false;
            _wholesaleCargoUnloaded = true;
            _supplierPlayerCarrying = true;
            _supplierVisualStage = 6;
            Save(); SupplierCheckpoint("box unloaded from assigned vehicle");
            Notify("Shipment UNLOADED. Carry it to any pallet with capacity.");
            return true;
        }
        if (atDriver || atWarehouse)
        {
            if (atWarehouse)
            {
                UpdateWholesalePalletVisual();
                if (_wholesaleWarehousePallet == null || !_wholesaleWarehousePallet.Exists())
                { Notify("Warehouse pallet is loading; try collecting the box again."); return true; }
            }
            if (_supplierCrate == null || !_supplierCrate.Exists())
                _supplierCrate = CreateFixedEquipmentProp("prop_cs_cardbox_01",
                    atWarehouse ? WarehousePackagePosition() : _supplierHandoff.Position,
                    atWarehouse ? _wholesaleWarehousePickup.Heading : _supplierHandoff.Heading, false);
            if (_supplierCrate == null || !_supplierCrate.Exists())
            { Notify("Supplier box model is loading. Try again in a moment."); return true; }
            try
            {
                if (atDriver && _supplierDriver != null && _supplierDriver.Exists())
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_SECONDARY_TASK, _supplierDriver.Handle);
                    _supplierBoxAttachedToDriver = false;
                }
                AttachSupplierBoxToPed(player);
                if (atWarehouse && _supplierPartsPallet.IsSet)
                    try { GTA.Native.Function.Call(GTA.Native.Hash.SET_NEW_WAYPOINT,
                        _supplierPartsPallet.Position.X, _supplierPartsPallet.Position.Y); } catch { }
                _supplierPlayerCarrying = true;
                _supplierVisualStage = 6;
                _supplierStageAt = _supplierGameplayNow;
                SupplierCheckpoint(atWarehouse ? "warehouse package collected" : "mule handoff collected");
                Notify(atWarehouse ? "Wholesale box collected. Load it into your assigned vehicle." :
                    "Delivery box collected. Carry it to a placed pallet.");
            }
            catch (Exception ex) { Log("Supplier box handoff", ex); Notify("Box handoff failed; try again."); }
            return true;
        }
        if (_supplierVisualStage == 6 && _supplierPlayerCarrying)
        {
            if (_deliverySupplier == 0 && !_wholesaleCargoUnloaded) return false;
            int slot = DepositPalletNear(player.Position);
            if (slot < 0) return false;
            if (!CanStoreShipment(_deliveryQuantity))
            { Notify("Pallet storage FULL. Purchase/place another pallet before stocking."); return true; }
            // The physical stockroom pallet must exist before we freeze a box in
            // the display stack; this prevents a saved-Z floating crate.
            if (_storagePalletProps[slot] == null || !_storagePalletProps[slot].Exists())
            {
                UpdateSupplierPalletVisual();
                if (_storagePalletProps[slot] == null || !_storagePalletProps[slot].Exists())
                { Notify("Stockroom pallet is loading. Try placing the box again."); return true; }
            }
            int nextBox = _storedDeliveryCrates[slot];
            Vector3 boxPosition; float boxHeading;
            if (!TryStoredBoxTransform(slot, nextBox, true, out boxPosition, out boxHeading))
            { Notify("Safe pallet box layout is not ready. No stock or invoice changed; try again shortly."); return true; }
            if (!CommitSupplierDelivery(slot)) return true;
            SupplierCheckpoint("package deposited on pallet / invoice issued");
            if (_supplierCrate != null && _supplierCrate.Exists())
            {
                GTA.Native.Function.Call(GTA.Native.Hash.DETACH_ENTITY, _supplierCrate.Handle, true, true);
                _supplierCrate.Position = boxPosition;
                _supplierCrate.Heading = boxHeading;
                _supplierCrate.IsPositionFrozen = true;
                GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_COLLISION, _supplierCrate.Handle, false, false);
                int packageIndex = slot * CratesPerPallet + nextBox;
                DeleteOwnedProp(ref _deliveredPackageProps[packageIndex]);
                _deliveredPackageProps[packageIndex] = _supplierCrate;
                _supplierCrate = null; // delivered parcel ownership is separate from obsolete displays
            }
            RefreshStoragePalletAfterDeposit();
            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_SECONDARY_TASK, player.Handle);
            _supplierPlayerCarrying = false;
            // Warehouse pickup has no delivery truck to send away.
            if (_supplierTruck != null && _supplierTruck.Exists()) BeginSupplierTruckDeparture(_supplierGameplayNow);
            else { _supplierVisualStage = 0; _supplierStageAt = _supplierGameplayNow; }
            return true;
        }
        return false;
    }

    private void HandleSupplierPackageInteraction()
    {
        if (_showMenu || Game.IsPaused || Game.Player.Character == null) return;
        if (_supplierVisualStage == 6 && _supplierPlayerCarrying && Game.Player.Character.IsDead)
        {
            // Interrupted cargo can be retrieved again; no inventory or invoice created.
            DeleteOwnedProp(ref _supplierCrate);
            _supplierPlayerCarrying = false;
            _supplierBoxAttachedToDriver = false;
            if (_deliverySupplier == 0)
            {
                _wholesaleCargoLoaded = false;
                _wholesaleCargoUnloaded = false;
                Save(); // recovery: recollect at warehouse, no duplicate invoice
            }
            _supplierVisualStage = _deliverySupplier == 0 ? 7 : 4;
            _supplierStageAt = _supplierGameplayNow;
            return;
        }
        bool nearDriver = _supplierVisualStage == 4 && _supplierDriver != null &&
            _supplierDriver.Exists() &&
            DistanceSquared(Game.Player.Character.Position, _supplierDriver.Position) < 10f;
        bool nearWarehouse = _supplierVisualStage == 7 && _wholesaleWarehousePickup.IsSet &&
            DistanceSquared(Game.Player.Character.Position, _wholesaleWarehousePickup.Position) < 10f;
        bool nearCargoVehicle = (_supplierVisualStage == 9 && _wholesaleCargoLoaded ||
            _supplierVisualStage == 6 && _supplierPlayerCarrying && _deliverySupplier == 0 && !_wholesaleCargoUnloaded) &&
            FindAssignedWholesaleVehicle(Game.Player.Character.Position, 8f) != null;
        bool nearPallet = _supplierVisualStage == 6 && _supplierPlayerCarrying &&
            DepositPalletNear(Game.Player.Character.Position) >= 0;
        if (!nearDriver && !nearPallet && !nearWarehouse && !nearCargoVehicle) return;
        if (PadJustPressed(GTA.Control.ScriptPadRight) || PadJustPressed(GTA.Control.FrontendRight))
        { TrySupplierPackageInteraction(); return; }
        DateTime now = DateTime.UtcNow;
        if (now < _nextSupplierInteractionHint) return;
        _nextSupplierInteractionHint = now.AddSeconds(2);
        string prompt = nearPallet ? "STOCK this box at the pallet." :
            nearCargoVehicle && _wholesaleCargoLoaded ? "UNLOAD the box from the assigned vehicle." :
            nearCargoVehicle && _supplierPlayerCarrying ? "LOAD the box into the assigned vehicle." :
            nearWarehouse ? "PICK UP the warehouse box; use your assigned vehicle to LOAD it." :
            "PICK UP the supplier package.";
        GTA.UI.Screen.ShowSubtitle("~b~Glass Heroes~s~: ~y~E~s~ / ~b~D-pad RIGHT~s~ to " + prompt, 1750);
    }

    private void BeginSupplierTruckDeparture(DateTime now)
    {
        CloseMuleCargoDoors();
        _supplierVisualStage = 5;
        _supplierStageAt = now;
        _supplierDepartIssued = false;
        if (_supplierDriver != null && _supplierDriver.Exists() &&
            _supplierTruck != null && _supplierTruck.Exists())
        {
            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, _supplierDriver.Handle);
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_ENTER_VEHICLE,
                _supplierDriver.Handle, _supplierTruck.Handle, 12000, -1, 1f, 1, 0);
        }
    }

    private static int CompatiblePackageVisualMask(int storedBoxes, int savedMask)
    {
        int count = Math.Max(0, Math.Min(CratesPerPallet, storedBoxes));
        return savedMask & ((1 << count) - 1);
    }

    private void DeleteLegacyStockroomDisplayBoxes()
    {
        // Only our obsolete display list. Never touch live cargo, delivered
        // parcel ownership, warehouse props, world/MLO entities or stock counts.
        foreach (Prop crate in _storedCrateProps)
            try { if (crate != null && crate.Exists()) crate.Delete(); } catch { }
        _storedCrateProps.Clear();
    }

    private void DeleteStoragePalletVisuals()
    {
        for (int i = 0; i < MaxStoragePallets; i++) DeleteOwnedProp(ref _storagePalletProps[i]);
        DeleteLegacyStockroomDisplayBoxes();
        for (int i = 0; i < _deliveredPackageProps.Length; i++)
            DeleteOwnedProp(ref _deliveredPackageProps[i]);
        _storageVisualAttempts = 0;
        _nextStorageVisualAttempt = DateTime.MinValue;
        _palletVisualDirty = true;
    }

    private void RefreshStoragePalletAfterDeposit()
    {
        // A new successful deposit gets a fresh bounded visual budget, even if
        // earlier model streaming exhausted retries while the player stayed nearby.
        _storageVisualAttempts = 0;
        _nextStorageVisualAttempt = DateTime.MinValue;
        UpdateSupplierPalletVisual();
    }

    private void HideUnsupportedPalletBoxes(int palletSlot, int firstMissingBox)
    {
        // Never show an upper parcel without its lower support after a spawn failure.
        // Only owned visuals change; delivery provenance and stock are untouched.
        for (int b = Math.Max(0, firstMissingBox); b < CratesPerPallet; b++)
            DeleteOwnedProp(ref _deliveredPackageProps[palletSlot * CratesPerPallet + b]);
    }

    private void UpdateSupplierPalletVisual()
    {
        Ped player = Game.Player.Character;
        bool nearby = false;
        for (int i = 0; i < _ownedStoragePallets; i++)
            if (StoragePalletPoint(i).IsSet && player != null && player.Exists() &&
                DistanceSquared(player.Position, StoragePalletPoint(i).Position) < 6400f) nearby = true;
        if (!nearby)
        {
            if (_storageVisualAttempts > 0 || !_palletVisualDirty || _storedCrateProps.Count > 0)
                DeleteStoragePalletVisuals();
            return;
        }
        DeleteLegacyStockroomDisplayBoxes();
        if (!_palletVisualDirty) return;
        DateTime now = DateTime.UtcNow;
        if (_storageVisualAttempts >= 5 || now < _nextStorageVisualAttempt) return;
        _storageVisualAttempts++;
        _nextStorageVisualAttempt = now.AddSeconds(3);
        bool complete = true;
        for (int i = 0; i < _ownedStoragePallets; i++)
        {
            WorkBay point = StoragePalletPoint(i);
            if (!point.IsSet) continue;
            if (_storagePalletProps[i] == null || !_storagePalletProps[i].Exists())
            {
                _storagePalletProps[i] = CreateFixedEquipmentProp("prop_pallet_02a",
                    point.Position, point.Heading, true);
                if (_storagePalletProps[i] == null)
                    _storagePalletProps[i] = CreateFixedEquipmentProp("prop_pallet_01a",
                        point.Position, point.Heading, true);
            }
            if (_storagePalletProps[i] == null) { complete = false; continue; }
            // BoxCount remains authoritative capacity data. Only parcels with
            // delivery provenance may be reconstructed, never legacy displays.
            for (int b = 0; b < Math.Min(CratesPerPallet, _storedDeliveryCrates[i]); b++)
            {
                if ((_deliveredPackageMasks[i] & (1 << b)) == 0) continue;
                int index = i * CratesPerPallet + b;
                Vector3 position; float heading;
                if (!TryStoredBoxTransform(i, b, false, out position, out heading))
                { HideUnsupportedPalletBoxes(i, b); complete = false; break; }
                Prop visual = _deliveredPackageProps[index];
                if (visual != null && visual.Exists())
                { visual.Position = position; visual.Heading = heading; continue; }
                visual = CreateFixedEquipmentProp("prop_cs_cardbox_01", position, heading, false);
                if (visual != null)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_COLLISION, visual.Handle, false, false);
                    _deliveredPackageProps[index] = visual;
                }
                else { HideUnsupportedPalletBoxes(i, b); complete = false; break; }
            }
        }
        _palletVisualDirty = !complete;
    }

    private void HandleOfficeInteraction()
    {
        if (_showMenu || _supplierPlayerCarrying || !IsPlayerAtOfficeDesk() || Game.IsPaused) return;
        DateTime now = DateTime.UtcNow;
        // D-pad RIGHT always works at the placed desk, independent of the full
        // controller menu toggle. Accept both gameplay ScriptPadRight and frontend Right.
        if ((now - _lastControllerAction).TotalMilliseconds >= 350 &&
            (PadJustPressed(GTA.Control.ScriptPadRight) || PadJustPressed(GTA.Control.FrontendRight)))
        { _lastPadDetected = "DESK DPAD RIGHT"; OpenBusinessFromOffice(); return; }
        // Preserve the optional remappable gamepad alternate.
        if (_controllerEnabled && PadJustPressed(_controllerInteract))
        { OpenBusinessFromOffice(); return; }
        if (now < _nextDeskHint) return;
        _nextDeskHint = now.AddSeconds(3);
        GTA.UI.Screen.ShowSubtitle("Glass Heroes office: ~y~E~s~ or ~b~D-pad RIGHT~s~ to manage business.", 2400);
    }

    private void DeleteOfficeDeskProps()
    {
        if (_officeLaptopProp != null)
        { try { if (_officeLaptopProp.Exists()) _officeLaptopProp.Delete(); } catch { } _officeLaptopProp = null; }
        if (_officeDeskProp != null)
        { try { if (_officeDeskProp.Exists()) _officeDeskProp.Delete(); } catch { } _officeDeskProp = null; }
    }

    private void UpdateOfficeDeskVisuals()
    {
        if (!_officeDeskPurchased || !_officeDeskPlaced || !_officeDeskLocation.IsSet ||
            Game.Player.Character == null ||
            DistanceSquared(Game.Player.Character.Position, _officeDeskLocation.Position) > 6400f)
        { DeleteOfficeDeskProps(); return; }
        if (_officeDeskProp != null && _officeDeskProp.Exists() &&
            _officeLaptopProp != null && _officeLaptopProp.Exists()) return;
        if (DateTime.UtcNow < _nextOfficeSpawn) return;
        _nextOfficeSpawn = DateTime.UtcNow.AddSeconds(3);
        DeleteOfficeDeskProps();
        Model deskModel = new Model("v_corp_desksetb");
        Model laptopModel = new Model("prop_laptop_01a");
        bool deskReady = RequestVisualModel(deskModel);
        bool laptopReady = RequestVisualModel(laptopModel);
        if (!deskReady || !laptopReady) return;
        try
        {
            _officeDeskProp = World.CreateProp(deskModel, _officeDeskLocation.Position, false, true);
            if (_officeDeskProp == null || !_officeDeskProp.Exists()) { Log("Desk creation", new Exception("Desk prop unavailable")); return; }
            _officeDeskProp.Heading = _officeDeskLocation.Heading;
            _officeDeskProp.IsPositionFrozen = true;
            Vector3 laptopPos = _officeDeskProp.Position + new Vector3(0f, 0f, 0.82f);
            _officeLaptopProp = World.CreateProp(laptopModel, laptopPos, false, false);
            if (_officeLaptopProp != null && _officeLaptopProp.Exists())
            { _officeLaptopProp.Heading = _officeDeskLocation.Heading; _officeLaptopProp.IsPositionFrozen = true; }
            else Log("Office computer", new Exception("Laptop prop not available; retrying."));
        }
        finally { ReleaseVisualModel(deskModel); ReleaseVisualModel(laptopModel); }
    }

    private static void DeleteOwnedProp(ref Prop prop)
    {
        if (prop != null)
        { try { if (prop.Exists()) prop.Delete(); } catch { } prop = null; }
    }

    private void DeleteEquipmentProps()
    {
        DeleteOwnedProp(ref _toolSetProp);
        DeleteOwnedProp(ref _scannerBenchProp);
        DeleteOwnedProp(ref _scannerTabletProp);
    }

    private Prop CreateFixedEquipmentProp(string modelName, Vector3 pos, float heading, bool placeOnGround = true)
    {
        Model model = new Model(modelName);
        if (!RequestVisualModel(model)) return null;
        try
        {
            Prop item = World.CreateProp(model, pos, false, placeOnGround);
            if (item != null && item.Exists())
            { item.Heading = heading; item.IsPositionFrozen = true; }
            return item;
        }
        finally { ReleaseVisualModel(model); }
    }

    // Do not yield into nested callbacks while a visual is waiting for a model.
    // Existing distance gates and retry clocks decide when this is called again.
    private bool RequestVisualModel(Model model)
    {
        _requestedVisualModels.Add(model);
        if (!model.IsLoaded) model.Request();
        return model.IsLoaded;
    }

    private void ReleaseVisualModel(Model model)
    {
        try
        {
            model.MarkAsNoLongerNeeded();
            _requestedVisualModels.Remove(model);
        }
        catch (Exception ex) { Log("Release v3 visual model", ex); }
    }

    private void ReleasePendingVisualModels()
    {
        // Snapshot because successful releases remove entries. Failed releases
        // retain ownership for another attempt while this instance is alive.
        foreach (Model model in new List<Model>(_requestedVisualModels))
            ReleaseVisualModel(model);
    }

    private static void RequireExclusiveBusinessInstallation(Func<string, bool> fileExists)
    {
        // Managed startup checks only. Never delete, rename or rewrite a conflict.
        if (fileExists(@"scripts\GlassHeroesBusiness.2.cs") ||
            fileExists(@"scripts\GlassHeroesBusiness.dll") ||
            fileExists(@"scripts\GlassHeroesBusiness_v0.3.3.0_RC1.2.cs") ||
            (fileExists(@"scripts\GlassHeroesBusiness.3.cs") &&
                fileExists(@"scripts\GlassHeroesBusiness.V3.Experimental.dll")))
            throw new InvalidOperationException("Enable only one Glass Heroes Business Manager generation/source. " +
                "Move conflicting business scripts outside scripts before loading this experimental v3 build; preserve business data.");
    }

    private void UpdateEquipmentVisuals()
    {
        Ped player = Game.Player.Character;
        if (player == null || !player.Exists()) { DeleteEquipmentProps(); return; }
        bool nearTools = _toolSetPurchased && _toolSetPlaced && _toolSetLocation.IsSet &&
            DistanceSquared(player.Position, _toolSetLocation.Position) < 6400f;
        bool nearScanner = _diagnosticScannerPurchased && _scannerPlaced && _scannerLocation.IsSet &&
            DistanceSquared(player.Position, _scannerLocation.Position) < 6400f;
        if (!nearTools) DeleteOwnedProp(ref _toolSetProp);
        if (!nearScanner)
        { DeleteOwnedProp(ref _scannerBenchProp); DeleteOwnedProp(ref _scannerTabletProp); }
        if (DateTime.UtcNow < _nextEquipmentSpawn) return;
        _nextEquipmentSpawn = DateTime.UtcNow.AddSeconds(3);
        try
        {
            if (nearTools && (_toolSetProp == null || !_toolSetProp.Exists()))
                _toolSetProp = CreateFixedEquipmentProp("prop_toolchest_01", _toolSetLocation.Position, _toolSetLocation.Heading);
            if (nearScanner)
            {
                if (_scannerBenchProp == null || !_scannerBenchProp.Exists())
                    _scannerBenchProp = CreateFixedEquipmentProp("p_cs_trolley_01_s", _scannerLocation.Position, _scannerLocation.Heading);
                // v0.3.2.5: trolley deliberately has no floating tablet overlay.
            }
        }
        catch (Exception ex)
        {
            // A missing prop/model must not disable the employee or financial tick loop.
            Log("Equipment prop loading", ex);
            DeleteEquipmentProps();
        }
    }

    private void OnAborted(object sender, EventArgs e)
    {
        Log("LIFECYCLE abort requested; last tick phase=" + _diagnosticTickStage, null);
        // Nothing has been spawned or billed before activation. Do not write
        // partially initialized defaults over the user's existing Save.ini.
        if (!_bootGameplayActivated)
        {
            Log("BOOT aborted before gameplay activation; saved business state preserved", null);
            return;
        }
        ParkRuntimeRepairPromises(); // Preserve fixed accepted data before entity cleanup.
        ReleasePendingVisualModels();
        CancelAllCustomerOrders("Script aborted");
        ClearStagedCustomers();
        CleanupVisualMechanics();
        DeleteSupplierVisuals();
        DeleteOwnedProp(ref _supplierPalletProp);
        DeleteStoragePalletVisuals();
        DeleteOfficeDeskProps();
        DeleteEquipmentProps();
        try { _customerPedModel.MarkAsNoLongerNeeded(); } catch { }
        try { _customerVehicleModel.MarkAsNoLongerNeeded(); } catch { }
        try { _receptionistModel.MarkAsNoLongerNeeded(); }
        catch (Exception ex) { Log("Release receptionist model", ex); }
        try { _mechanicModel.MarkAsNoLongerNeeded(); }
        catch (Exception ex) { Log("Release model", ex); }
        Save();
        Log("LIFECYCLE abort cleanup completed", null);
    }

    private static void Notify(string message)
    {
        // A concurrent three-order shift can generate dozens of ticker posts.
        // Suppress ONLY low-priority transfer narration; never drop reception,
        // quotes, completed invoices, errors, F2 actions or requested statuses.
        // Avoid THEFEED_FLUSH_QUEUE: that would delete OTHER MODS' notifications.
        if (_compactCustomerNotices && IsRoutineTransitNotice(message)) return;
        GTA.UI.Notification.Show("~b~Glass Heroes~s~: " + message, false);
    }

    private static bool IsRoutineTransitNotice(string message)
    {
        return message.StartsWith("Queued customer car ", StringComparison.Ordinal) ||
            message.StartsWith("Queued customer #", StringComparison.Ordinal) ||
            message.StartsWith("Valet rerouting ", StringComparison.Ordinal) ||
            message.StartsWith("Serviced car returned to customer parking", StringComparison.Ordinal) ||
            message.EndsWith(" driving car into service.", StringComparison.Ordinal) ||
            message.EndsWith(" returning car to drop-off.", StringComparison.Ordinal) ||
            message.EndsWith(" returning serviced car to the drop-off.", StringComparison.Ordinal);
    }

    // BEGIN ISSUE1 DIAGNOSTIC LOG (also exercised by the standalone test runner)
    private sealed class DiagnosticLog
    {
        private readonly Func<DateTime> _utcNow;
        private readonly Action<string> _append;
        private DateTime _nextErrorUtc = DateTime.MinValue;
        private DateTime _nextTransitionUtc = DateTime.MinValue;
        private DateTime _nextWriteRetryUtc = DateTime.MinValue;
        private int _suppressedErrors;
        private bool _writing;
        private bool _hasState;
        private long _lastState;

        internal DiagnosticLog(Func<DateTime> utcNow, Action<string> append)
        {
            _utcNow = utcNow;
            _append = append;
        }

        internal void RecordTransition(bool shift, bool remote, bool staffInRange,
            bool menu, int supplierStage, int customers, int remoteJobs)
        {
            // Read managed state only, once per existing visual refresh. No GTA
            // natives, entity scans, string allocations or writes on stable idle.
            long state = (shift ? 1L : 0L) | (remote ? 2L : 0L) |
                (staffInRange ? 4L : 0L) | (menu ? 8L : 0L) |
                ((long)supplierStage << 4) | ((long)customers << 8) |
                ((long)remoteJobs << 24);
            if (_hasState && state == _lastState) return;
            DateTime now = _utcNow();
            if (now < _nextTransitionUtc) return;
            // Coalesce rapid changes: this is the latest observed state, not a
            // complete event history. Never emit a per-frame/per-second heartbeat.
            _nextTransitionUtc = now.AddSeconds(10);
            if (Write("STATE shift=" + shift + " remote=" + remote +
                " staffInRange=" + staffInRange + " menu=" + menu +
                " supplierStage=" + supplierStage + " customers=" + customers +
                " remoteJobs=" + remoteJobs, null))
            {
                _lastState = state;
                _hasState = true;
            }
        }

        internal bool Write(string context, Exception ex)
        {
            // The writer can fail or re-enter. Diagnostics must never recurse or
            // escape into the gameplay loop, including when storage is unavailable.
            if (_writing) return false;
            _writing = true;
            try
            {
                DateTime now = _utcNow();
                if (now < _nextWriteRetryUtc) return false;
                if (ex != null && now < _nextErrorUtc)
                {
                    if (_suppressedErrors < int.MaxValue) _suppressedErrors++;
                    return false;
                }
                if (ex != null) _nextErrorUtc = now.AddSeconds(10);
                string suppressed = ex != null && _suppressedErrors > 0
                    ? " | suppressed errors=" + _suppressedErrors : "";
                _append(now.ToString("s") + "Z | " + context + " | " +
                    (ex == null ? "no exception" : ex.ToString()) + suppressed +
                    Environment.NewLine);
                if (ex != null) _suppressedErrors = 0;
                return true;
            }
            catch
            {
                // Back off disk failures as well as exception floods. This clock
                // governs logging only; no business or delivery timer is changed.
                try { _nextWriteRetryUtc = _utcNow().AddSeconds(30); }
                catch { }
                return false;
            }
            finally { _writing = false; }
        }
    }
    // END ISSUE1 DIAGNOSTIC LOG

    private static readonly DiagnosticLog _diagnosticLog = new DiagnosticLog(
        () => DateTime.UtcNow, AppendDiagnosticLine);

    private static void AppendDiagnosticLine(string line)
    {
        Directory.CreateDirectory(DataDirectory);
        File.AppendAllText(Path.Combine(DataDirectory, "Errors.log"), line);
    }

    private static void Log(string context, Exception ex)
    {
        _diagnosticLog.Write(context, ex);
    }
}
