# v0.3.4 production menu navigation matrix

Source SHA256: `fbc8f7f963bbffa3e31c097bf34250fef5807d28dd5de3b7d75463146beb3251`. Extracted from the passing production routing adapter, not GTA input. 68 menu pages; 184 navigation checks total.

| Page | Title | Back destination | Restored row | Items |
|---|---|---|---|---|
| Business | GLASS HEROES | Business | 0 | Open Business; Save Business; Business Status; Business Management > |
| BusinessManagement | BUSINESS MANAGEMENT | Business | 3 | Employees >; Customers >; Inventory & Suppliers >; Finances & Accounting >; Placement Settings >; Set Shop Location; < Back to Main |
| Employees | EMPLOYEES | BusinessManagement | 0 | Employee Status; Mechanics >; Receptionist >; Business Manager >; Employee Settings >; < Back to Business Management |
| EmployeeSettings | EMPLOYEE SETTINGS | Employees | 4 | Placement Settings >; Employee Walking Paths >; Blocked Path Recovery >; Placement Markers >; Break-Time Animations; < Back to Employees |
| EmployeePaths | EMPLOYEE WALKING PATHS | EmployeeSettings | 1 | Mechanic Walking Paths >; Receptionist Walking Paths >; Manager Walking Paths >; < Back |
| ArrivalWaypoints | SPAWN -> BREAK (0-5) | MechanicWalkingPaths | 2 | Arrival Point 1 / 5 >; Arrival Point 2 / 5 >; Arrival Point 3 / 5 >; Arrival Point 4 / 5 >; Arrival Point 5 / 5 >; < Back |
| MechanicBays | SERVICE BAYS | EmployeePlacements | 1 | Service Bay 1 >; Service Bay 2 >; Service Bay 3 >; < Back |
| ServiceWaypoints | SHARED WORK ACCESS (0-5) | MechanicWalkingPaths | 3 | Work Route 1 / 5 >; Work Route 2 / 5 >; Work Route 3 / 5 >; Work Route 4 / 5 >; Work Route 5 / 5 >; < Back |
| BayApproaches | BAY APPROACHES | MechanicWalkingPaths | 4 | Bay 1 Final Turn >; Bay 2 Final Turn >; Bay 3 Final Turn >; < Back |
| ServiceVehicleBays | SERVICE VEHICLE PARKING | CustomerPlacements | 6 | Service Bay 1 >; Service Bay 2 >; Service Bay 3 >; < Back |
| ServiceBayProfile | SERVICE BAY 1 | Business | 0 | Set Vehicle Position (CUSTOM); Set Mechanic Work Position; Mechanic Assignment; View Current Configuration; < Back to Service Bays |
| PointEditor | TEST POINT | Business | 0 | Set / Save Position + Facing; Clear Saved Position; Marker Preview; Show Saved Coordinates; < Back |
| RouteRecovery | ROUTE RECOVERY | EmployeeSettings | 2 | Blocked-Route Teleport; Recovery Status; < Back |
| PlacementMarkers | PLACEMENT MARKERS | EmployeeSettings | 3 | Placement Markers; Show Marker Color Legend; < Back |
| Customers | CUSTOMER SERVICE | BusinessManagement | 1 | Customer Order Status; Customer Queue >; Placement Settings >; Vehicle Driving Options >; Customer Arrivals; Schedule Next Customer; Customer Blips; Placement Markers; < Back to Business Management |
| Finances | FINANCES & ACCOUNTING | BusinessManagement | 3 | Financial Overview >; Income & Sales >; Expenses & Bills >; Profit & Loss >; Transaction History >; Owner Cash Transfers >; Reporting Period: Toggle; < Back to Management |
| FinancialOverview | FINANCIAL OVERVIEW | Finances | 0 | Reporting Period: Toggle; Period Closing Cash; Gross Revenue; Cash Flow Reconciliation; Outstanding Bills at Close; Save / Audit State; < Back to Accounting |
| IncomeSales | INCOME & SALES | Finances | 1 | Reporting Period: Toggle; Physical Customer Repairs; Manager Remote Repairs; Autonomous Mechanic Jobs; Parts Retail Sales; Gross Revenue (No Overlap); Known Rebates / Refunds; < Back to Accounting |
| ProfitLoss | PROFIT & LOSS | Finances | 3 | Reporting Period: Toggle; Gross Revenue; Cash Expense Payments; Expenses Incurred; Accounting Profit; Accounting Basis / Notes; < Back to Accounting |
| TransactionHistory | TRANSACTION HISTORY | Finances | 4 | #1 test history; Older Transactions >; Newer Transactions >; < Back to Accounting |
| OwnerTransfers | OWNER CASH TRANSFERS | Finances | 5 | Transfer Amount: Change; Review Owner Deposit >; Review Owner Withdrawal >; Recorded Owner Deposits; Recorded Owner Withdrawals; Reporting Period: Toggle; < Back to Accounting |
| StaffRoster | MECHANIC ROSTER | MechanicManagement | 0 | Mechanic 1 >; Mechanic 2 >; Mechanic 3 >; < Back to Mechanics |
| StaffProfile | MECHANIC PROFILE | StaffRoster | 0 | Profile / Performance; Training: +40 XP; Change Specialization; Raise Period Wage (+$50); Fire This Mechanic; < Back to Roster |
| VehicleDriveOptions | VEHICLE DRIVING | Customers | 3 | Drive-in Customer Cars; Mechanic Vehicle Transfers; Service Parking Mode; Auto Lot Settings >; Custom MLO Parking Bays >; Reverse Before Driving; Driving Status / Diagnostics; Retry Held Paid Car Return; < Back to Customers |
| CustomerPlacements | CUSTOMER PLACEMENTS | Customers | 2 | Customer Drop-off Parking >; Customer Road Spawn >; Customer Reception Approach >; Customer Check-in Position >; Waiting Car 2 Position >; Waiting Car 3 Position >; Service Vehicle Parking Bays >; Customer Walking Route (0-3) >; < Back |
| EmployeePlacements | EMPLOYEE PLACEMENTS | EmployeeSettings | 0 | Mechanic Break Area >; Service Bays >; Receptionist Desk >; Receptionist Desk Approach >; Manager Workstation >; Manager Workstation Approach >; Staff Spawn / Entry >; < Back |
| MechanicWalkingPaths | MECHANIC WALKING | EmployeePaths | 0 | Mechanic Walking Mode; Pickup + Return Exit Route; Spawn -> Break Waypoints >; Workstation Access Stops >; Final Bay Approaches >; Legacy Staff Checkpoint (MLO) >; Choose Job Waypoints by Mechanic >; Smooth Through-Waypoints; Teleport Stuck Pedestrians; Advanced Break <-> Bay Routes; < Back |
| ReceptionWalkingPaths | RECEPTION WALKING | EmployeePaths | 1 | Receptionist Walking Mode; Spawn -> Desk Waypoints >; Final Desk Approach >; < Back |
| ReceptionWaypoints | RECEPTION ROUTE (0-5) | ReceptionWalkingPaths | 1 | Reception Stop 1 / 5 >; Reception Stop 2 / 5 >; Reception Stop 3 / 5 >; Reception Stop 4 / 5 >; Reception Stop 5 / 5 >; < Back |
| CustomerQueueSettings | CUSTOMER QUEUE | Customers | 1 | Pending Customer Requests; Maximum Queue Capacity; Schedule One Customer; Clear Pending Requests; < Back to Customers |
| PartsSales | BUILD PARTS RETAIL SALE | InventoryManagement | 3 | Universal Parts: Quantity; Service Filters: Quantity; Brake Kits: Quantity; Diagnostic Supplies: Quantity; Review Sale >; Clear Sale Draft; < Back to Inventory |
| VehicleAutoLotOptions | AUTO LOT PARKING | VehicleDriveOptions | 3 | Car Parking Side; Car Offset from Mechanic Bay; Auto Parking Preview; < Back to Driving |
| BusinessExpenses | EXPENSES & BILLS | Finances | 2 | Outstanding Bills; Mechanic Wage Invoices >; Pay Receptionist Wages; Pay Rent & Utilities; Pay Supplier Invoices; Pay All Bills; Order 10 Universal Parts; Equipment & Office >; Financial Reports >; Pay Manager Wages; < Back to Business Management |
| MechanicInvoices | MECHANIC WAGES | BusinessExpenses | 1 | Mechanic 1 Wage Invoice; Mechanic 2 Wage Invoice; Mechanic 3 Wage Invoice; Pay Former Employee Wages; Pay All Mechanic Wages; < Back to Expenses |
| CustomerWalkingRoutes | CUSTOMER WALK (0-3) | CustomerPlacements | 7 | Customer Walking Stop 1 / 3 >; Customer Walking Stop 2 / 3 >; Customer Walking Stop 3 / 3 >; < Back |
| InventoryManagement | INVENTORY & SUPPLIERS | BusinessManagement | 2 | Inventory Overview >; Build Supplier Order >; Active Deliveries >; Parts Sales >; Stockroom & Pallets >; Supplier Settings >; < Back to Management |
| InventoryOverview | INVENTORY OVERVIEW | InventoryManagement | 0 | Universal Parts; Service Filters; Brake Kits; Diagnostic Supplies; Storage Capacity; Reorder Advice; Low-stock Threshold; < Back to Inventory |
| BuildSupplierOrder | BUILD SUPPLIER ORDER | InventoryManagement | 1 | Universal Parts; Service Filters; Brake Kits; Diagnostic Supplies; Supplier: Change; Cost / Capacity Preview; Review Purchase Order >; Fill Low-stock Cart; Clear Cart; < Back to Inventory |
| SupplierOrderReview | CONFIRM SUPPLIER ORDER | BuildSupplierOrder | 6 | Review Order Details; Confirm Purchase Order; < Back to Supplier Cart |
| ActiveDeliveries | ACTIVE DELIVERIES | InventoryManagement | 2 | Delivery Status; Order / Last Receipt; Recover Original Order; Cancel Pending Order; < Back to Inventory |
| EquipmentManagement | SHOP EQUIPMENT | BusinessExpenses | 7 | Equipment Status; Buy Tool Set; Buy Diagnostic Scanner; Buy Desk + Laptop; Place Tool Chest; Remove Tool Chest; Place Diagnostic Trolley; Remove Diagnostic Trolley; Place Desk in Front; Remove Placed Desk; Office PC Status; < Back |
| MechanicRouteRoster | MECHANIC JOB ROUTES | MechanicWalkingPaths | 6 | Mechanic 1 Job Route >; Mechanic 2 Job Route >; Mechanic 3 Job Route >; < Back to Mechanic Paths |
| MechanicRouteDirection | SELECT WALK DIRECTION | MechanicRouteRoster | 0 | Break -> Job Waypoint Selection >; Job -> Break Waypoint Selection >; Route Summary; < Back to Mechanic List |
| MechanicRoutePoints | JOB WALK WAYPOINTS | MechanicRouteDirection | 0 | Use Work Waypoint 1; Use Work Waypoint 2; Use Work Waypoint 3; Use Work Waypoint 4; Use Work Waypoint 5; All Waypoints ON; All Waypoints OFF; < Back to Route Direction |
| SupplierDeliverySetup | SUPPLIER SETTINGS | InventoryManagement | 5 | Physical Mule Delivery; Mule Road Spawn >; Truck Unloading Spot >; Driver Box Handoff >; Pallet Storage & Capacity >; Driver Walk Stops (0-3) >; Wholesale Warehouse: Fixed; Assign Current Pickup Vehicle; Assigned Vehicle / Cargo Status; Delivery Route / Stock Status; Recover Stuck Shipment; Cancel Pending Order (press twice); < Back to Inventory |
| SupplierDriverWaypoints | DRIVER WALK (0-3) | SupplierDeliverySetup | 5 | Driver Stop 1 / 3 >; Driver Stop 2 / 3 >; Driver Stop 3 / 3 >; < Back |
| HireMechanicRoster | HIRE A MECHANIC | MechanicManagement | 1 | Junior General Mechanic; Skilled Brake Technician; Experienced Diagnostic Technician; < Back to Mechanics |
| HireReceptionistRoster | HIRE RECEPTIONIST | ReceptionistManagement | 1 | Entry Receptionist (1 queue); Experienced Receptionist (2 queue); Senior Service Advisor (3 queue); < Back to Receptionist |
| MechanicManagement | MECHANIC MANAGEMENT | Employees | 1 | Mechanic Roster >; Hire Mechanic >; Mechanic Job Waypoints >; Mechanic Walking Paths >; < Back to Employees |
| ReceptionistManagement | RECEPTION MANAGEMENT | Employees | 2 | Receptionist Profile >; Hire Receptionist >; Receptionist Walking Paths >; < Back to Employees |
| ReceptionistProfile | RECEPTIONIST PROFILE | ReceptionistManagement | 0 | Profile & Queue Skill; Train Receptionist (+1 Capacity); Increase Period Wage (+$25); Dismiss Receptionist; < Back to Receptionists |
| SupplierPalletStorage | STOCKROOM & PALLETS | InventoryManagement | 4 | Storage Capacity Status; Purchase Additional Pallet ($750); Place / Move Pallet 1 >; Place / Move Pallet 2 >; Place / Move Pallet 3 >; Place / Move Pallet 4 >; < Back |
| ManagerManagement | BUSINESS MANAGER | Employees | 3 | Manager Profile >; Hire Business Manager >; Manager Walking Paths >; < Back to Employees |
| HireManagerRoster | MANAGEMENT CANDIDATES | ManagerManagement | 1 | Assistant Manager — 3% discount; Experienced Manager — 6% discount; General Manager — 10% discount; < Back to Management |
| ManagerProfile | MANAGER PROFILE | ManagerManagement | 0 | Profile & Business Reports; Train Manager (+40 XP); Pay Manager Wage Invoice; Dismiss Manager; < Back to Manager |
| ManagerWalkingPaths | MANAGER WALKING | EmployeePaths | 2 | Manager Walking Mode; Spawn -> Workstation Waypoints >; Final Workstation Approach >; < Back |
| ManagerWaypoints | MANAGER ROUTE (0-5) | ManagerWalkingPaths | 1 | Manager Stop 1 / 5 >; Manager Stop 2 / 5 >; Manager Stop 3 / 5 >; Manager Stop 4 / 5 >; Manager Stop 5 / 5 >; < Back |
| PlacementSettings | PLACEMENT SETTINGS | PlacementSettings | 0 | Customer Locations >; Employee Locations >; Service Bays >; Delivery & Stockroom >; Walking Routes & Approaches >; Marker Display Settings >; < Back to Management |
| DeliveryPlacements | DELIVERY & STOCKROOM LOCATIONS | DeliveryPlacements | 0 | Mule Road Spawn >; Truck Unloading Spot >; Driver Box Handoff >; Pallet Storage & Placement >; Supplier Driver Stops >; Equipment Placement & Upgrades >; < Back to Placement Settings |
| PlacementWalkingRoutes | WALKING ROUTES & APPROACHES | PlacementWalkingRoutes | 0 | Mechanics >; Receptionist >; Manager >; Customers >; Supplier Driver >; < Back to Placement Settings |
| MechanicPlacementRoutes | MECHANIC WALKING ROUTES | MechanicPlacementRoutes | 0 | Spawn -> Mechanic Break Area >; Workstation Access Stops >; Final Bay Approaches >; Job Waypoint Selection by Mechanic >; Legacy Mechanic Checkpoint >; Walking Mode & Recovery >; < Back to Roles |
| ReceptionPlacementRoutes | RECEPTIONIST WALKING ROUTES | ReceptionPlacementRoutes | 0 | Spawn -> Desk Stops >; Final Desk Approach >; Walking Mode >; < Back to Roles |
| ManagerPlacementRoutes | MANAGER WALKING ROUTES | ManagerPlacementRoutes | 0 | Spawn -> Workstation Stops >; Final Workstation Approach >; Walking Mode >; < Back to Roles |
| CustomerPlacementRoutes | CUSTOMER WALKING ROUTES | CustomerPlacementRoutes | 0 | Customer Walking Stops >; Reception Approach >; < Back to Roles |
| SupplierPlacementRoutes | SUPPLIER DRIVER WALKING ROUTES | SupplierPlacementRoutes | 0 | Supplier Driver Stops >; Driver Box Handoff >; < Back to Roles |
| OwnerTransferReview | CONFIRM OWNER WITHDRAWAL | OwnerTransfers | 2 | Review Amount / Business Cash; Confirm Transfer; < Back to Owner Transfers |
| PartsSaleReview | CONFIRM PARTS RETAIL SALE | PartsSales | 4 | Review Items / Proceeds; Confirm Sale; < Back to Parts Sales |
| EmployeeStatus | EMPLOYEE STATUS SNAPSHOT | Employees | 0 | Mechanic 1 (vacant); Mechanic 2 (vacant); Mechanic 3 (vacant); Receptionist; Manager; Refresh / Full Status; < Back to Employees |
