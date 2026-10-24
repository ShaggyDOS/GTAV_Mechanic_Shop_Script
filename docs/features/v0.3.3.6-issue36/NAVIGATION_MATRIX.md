# Issue #36 production navigation inventory

Generated from the actual final navigation run. Destinations/callers and row labels are source-derived; in-game navigation remains NOT RUN.

## Business — GLASS HEROES

Harness return: Business, row 0.

- 0: Open Business
- 1: Save Business
- 2: Business Status
- 3: Business Management >

## BusinessManagement — BUSINESS MANAGEMENT

Harness return: Business, row 3.

- 0: Employees >
- 1: Customers >
- 2: Inventory & Suppliers >
- 3: Finances & Accounting >
- 4: Placement Settings >
- 5: Set Shop Location
- 6: < Back to Main

## Employees — EMPLOYEES

Harness return: BusinessManagement, row 0.

- 0: Employee Status
- 1: Mechanics >
- 2: Receptionist >
- 3: Business Manager >
- 4: Employee Settings >
- 5: < Back to Business Management

## EmployeeSettings — EMPLOYEE SETTINGS

Harness return: Employees, row 4.

- 0: Placement Settings >
- 1: Employee Walking Paths >
- 2: Blocked Path Recovery >
- 3: Placement Markers >
- 4: Break-Time Animations
- 5: < Back to Employees

## EmployeePaths — EMPLOYEE WALKING PATHS

Harness return: EmployeeSettings, row 1.

- 0: Mechanic Walking Paths >
- 1: Receptionist Walk Route >
- 2: Manager Walk Route >
- 3: < Back

## ArrivalWaypoints — SHARED ENTRY CORRIDOR

Harness return: MechanicWalkingPaths, row 2.

- 0: Entry Corridor Stop 1 >
- 1: Entry Corridor Stop 2 >
- 2: Entry Corridor Stop 3 >
- 3: Entry Corridor Stop 4 >
- 4: Entry Corridor Stop 5 >
- 5: < Back

## MechanicBays — SERVICE BAYS

Harness return: EmployeePlacements, row 1.

- 0: Service Bay 1 >
- 1: Service Bay 2 >
- 2: Service Bay 3 >
- 3: < Back

## ServiceWaypoints — SHARED WORK STOPS (0-5)

Harness return: MechanicWalkingPaths, row 3.

- 0: Shared Work Stop 1 >
- 1: Shared Work Stop 2 >
- 2: Shared Work Stop 3 >
- 3: Shared Work Stop 4 >
- 4: Shared Work Stop 5 >
- 5: < Back

## BayApproaches — WORKSTATION APPROACHES

Harness return: MechanicWalkingPaths, row 4.

- 0: Workstation 1 Approach >
- 1: Workstation 2 Approach >
- 2: Workstation 3 Approach >
- 3: < Back

## ServiceVehicleBays — SERVICE VEHICLE PARKING

Harness return: CustomerPlacements, row 6.

- 0: Service Bay 1 >
- 1: Service Bay 2 >
- 2: Service Bay 3 >
- 3: < Back

## ServiceBayProfile — SERVICE BAY 1

Harness return: Business, row 0.

- 0: Set Vehicle Position (CUSTOM)
- 1: Set Mechanic Work Position
- 2: Mechanic Assignment
- 3: View Current Configuration
- 4: < Back to Service Bays

## PointEditor — TEST POINT

Harness return: Business, row 0.

- 0: Set / Save Position + Facing
- 1: Clear Saved Position
- 2: Marker Preview
- 3: Show Saved Coordinates
- 4: < Back

## RouteRecovery — ROUTE RECOVERY

Harness return: EmployeeSettings, row 2.

- 0: Blocked-Route Teleport
- 1: Recovery Status
- 2: < Back

## PlacementMarkers — PLACEMENT MARKERS

Harness return: EmployeeSettings, row 3.

- 0: Placement Markers
- 1: Show Marker Color Legend
- 2: < Back

## Customers — CUSTOMER SERVICE

Harness return: BusinessManagement, row 1.

- 0: Customer Order Status
- 1: Customer Queue >
- 2: Placement Settings >
- 3: Vehicle Driving Options >
- 4: Customer Arrivals
- 5: Schedule Next Customer
- 6: Customer Blips
- 7: Placement Markers
- 8: < Back to Business Management

## Finances — FINANCES & ACCOUNTING

Harness return: BusinessManagement, row 3.

- 0: Financial Overview >
- 1: Income & Sales >
- 2: Expenses & Bills >
- 3: Profit & Loss >
- 4: Transaction History >
- 5: Owner Cash Transfers >
- 6: Reporting Period: Toggle
- 7: < Back to Management

## FinancialOverview — FINANCIAL OVERVIEW

Harness return: Finances, row 0.

- 0: Reporting Period: Toggle
- 1: Period Closing Cash
- 2: Gross Revenue
- 3: Cash Flow Reconciliation
- 4: Outstanding Bills at Close
- 5: Save / Audit State
- 6: < Back to Accounting

## IncomeSales — INCOME & SALES

Harness return: Finances, row 1.

- 0: Reporting Period: Toggle
- 1: Physical Customer Repairs
- 2: Manager Remote Repairs
- 3: Autonomous Mechanic Jobs
- 4: Parts Retail Sales
- 5: Gross Revenue (No Overlap)
- 6: Known Rebates / Refunds
- 7: < Back to Accounting

## ProfitLoss — PROFIT & LOSS

Harness return: Finances, row 3.

- 0: Reporting Period: Toggle
- 1: Gross Revenue
- 2: Cash Expense Payments
- 3: Expenses Incurred
- 4: Accounting Profit
- 5: Accounting Basis / Notes
- 6: < Back to Accounting

## TransactionHistory — TRANSACTION HISTORY

Harness return: Finances, row 4.

- 0: #1 test history
- 1: Older Transactions >
- 2: Newer Transactions >
- 3: < Back to Accounting

## OwnerTransfers — OWNER CASH TRANSFERS

Harness return: Finances, row 5.

- 0: Transfer Amount: Change
- 1: Review Owner Deposit >
- 2: Review Owner Withdrawal >
- 3: Recorded Owner Deposits
- 4: Recorded Owner Withdrawals
- 5: Reporting Period: Toggle
- 6: < Back to Accounting

## StaffRoster — MECHANIC ROSTER

Harness return: MechanicManagement, row 0.

- 0: Mechanic 1 >
- 1: Mechanic 2 >
- 2: Mechanic 3 >
- 3: < Back to Mechanics

## StaffProfile — MECHANIC PROFILE

Harness return: StaffRoster, row 0.

- 0: Profile / Performance
- 1: Training: +40 XP
- 2: Change Specialization
- 3: Raise Period Wage (+$50)
- 4: Fire This Mechanic
- 5: < Back to Roster

## VehicleDriveOptions — VEHICLE DRIVING

Harness return: Customers, row 3.

- 0: Drive-in Customer Cars
- 1: Mechanic Vehicle Transfers
- 2: Service Parking Mode
- 3: Auto Lot Settings >
- 4: Custom MLO Parking Bays >
- 5: Reverse Before Driving
- 6: Driving Status / Diagnostics
- 7: Retry Held Paid Car Return
- 8: < Back to Customers

## CustomerPlacements — CUSTOMER PLACEMENTS

Harness return: Customers, row 2.

- 0: Customer Drop-off Parking >
- 1: Customer Road Spawn >
- 2: Customer Reception Approach >
- 3: Customer Check-in Position >
- 4: Waiting Car 2 Position >
- 5: Waiting Car 3 Position >
- 6: Service Vehicle Parking Bays >
- 7: Customer Walking Route (0-3) >
- 8: < Back

## EmployeePlacements — EMPLOYEE PLACEMENTS

Harness return: EmployeeSettings, row 0.

- 0: Mechanic Break Area >
- 1: Service Bays >
- 2: Receptionist Desk >
- 3: Receptionist Desk Approach >
- 4: Manager Workstation >
- 5: Manager Workstation Approach >
- 6: Staff Spawn / Entry >
- 7: < Back

## MechanicWalkingPaths — MECHANIC WALKING

Harness return: EmployeePaths, row 0.

- 0: Mechanic Walking Mode
- 1: Car Walks: Entry Corridor
- 2: Shared Entry Corridor >
- 3: Shared Work Stops >
- 4: Workstation Approaches >
- 5: Legacy Staff Checkpoint (MLO) >
- 6: Selections by Mechanic >
- 7: Smooth Through-Waypoints
- 8: Teleport Stuck Pedestrians
- 9: Custom Work Stop Routes
- 10: < Back

## ReceptionWalkingPaths — RECEPTIONIST WALK ROUTE

Harness return: EmployeePaths, row 1.

- 0: Receptionist Walking Mode
- 1: Entry-to-Desk Stops >
- 2: Final Desk Approach >
- 3: < Back

## ReceptionWaypoints — RECEPTION ROUTE (0-5)

Harness return: ReceptionWalkingPaths, row 1.

- 0: Reception Stop 1 / 5 >
- 1: Reception Stop 2 / 5 >
- 2: Reception Stop 3 / 5 >
- 3: Reception Stop 4 / 5 >
- 4: Reception Stop 5 / 5 >
- 5: < Back

## CustomerQueueSettings — CUSTOMER QUEUE

Harness return: Customers, row 1.

- 0: Pending Customer Requests
- 1: Maximum Queue Capacity
- 2: Schedule One Customer
- 3: Clear Pending Requests
- 4: < Back to Customers

## PartsSales — BUILD PARTS RETAIL SALE

Harness return: InventoryManagement, row 3.

- 0: Universal Parts: Quantity
- 1: Service Filters: Quantity
- 2: Brake Kits: Quantity
- 3: Diagnostic Supplies: Quantity
- 4: Review Sale >
- 5: Clear Sale Draft
- 6: < Back to Inventory

## VehicleAutoLotOptions — AUTO LOT PARKING

Harness return: VehicleDriveOptions, row 3.

- 0: Car Parking Side
- 1: Car Offset from Mechanic Bay
- 2: Auto Parking Preview
- 3: < Back to Driving

## BusinessExpenses — EXPENSES & BILLS

Harness return: Finances, row 2.

- 0: Outstanding Bills
- 1: Mechanic Wage Invoices >
- 2: Pay Receptionist Wages
- 3: Pay Rent & Utilities
- 4: Pay Supplier Invoices
- 5: Pay All Bills
- 6: Order 10 Universal Parts
- 7: Equipment & Office >
- 8: Financial Reports >
- 9: Pay Manager Wages
- 10: < Back to Business Management

## MechanicInvoices — MECHANIC WAGES

Harness return: BusinessExpenses, row 1.

- 0: Mechanic 1 Wage Invoice
- 1: Mechanic 2 Wage Invoice
- 2: Mechanic 3 Wage Invoice
- 3: Pay Former Employee Wages
- 4: Pay All Mechanic Wages
- 5: < Back to Expenses

## CustomerWalkingRoutes — CUSTOMER WALK (0-3)

Harness return: CustomerPlacements, row 7.

- 0: Customer Walking Stop 1 / 3 >
- 1: Customer Walking Stop 2 / 3 >
- 2: Customer Walking Stop 3 / 3 >
- 3: < Back

## InventoryManagement — INVENTORY & SUPPLIERS

Harness return: BusinessManagement, row 2.

- 0: Inventory Overview >
- 1: Build Supplier Order >
- 2: Active Deliveries >
- 3: Parts Sales >
- 4: Stockroom & Pallets >
- 5: Supplier Settings >
- 6: < Back to Management

## InventoryOverview — INVENTORY OVERVIEW

Harness return: InventoryManagement, row 0.

- 0: Universal Parts
- 1: Service Filters
- 2: Brake Kits
- 3: Diagnostic Supplies
- 4: Storage Capacity
- 5: Reorder Advice
- 6: Low-stock Threshold
- 7: < Back to Inventory

## BuildSupplierOrder — BUILD SUPPLIER ORDER

Harness return: InventoryManagement, row 1.

- 0: Universal Parts
- 1: Service Filters
- 2: Brake Kits
- 3: Diagnostic Supplies
- 4: Supplier: Change
- 5: Cost / Capacity Preview
- 6: Place Purchase Order
- 7: Fill Low-stock Cart
- 8: Clear Cart
- 9: < Back to Inventory

## ActiveDeliveries — ACTIVE DELIVERIES

Harness return: InventoryManagement, row 2.

- 0: Delivery Status
- 1: Order Contents
- 2: Recover Original Order
- 3: Cancel Pending Order
- 4: Set Warehouse GPS
- 5: < Back to Inventory

## EquipmentManagement — SHOP EQUIPMENT

Harness return: BusinessExpenses, row 7.

- 0: Equipment Status
- 1: Buy Tool Set
- 2: Buy Diagnostic Scanner
- 3: Buy Desk + Laptop
- 4: Place Tool Chest
- 5: Remove Tool Chest
- 6: Place Diagnostic Trolley
- 7: Remove Diagnostic Trolley
- 8: Place Desk in Front
- 9: Remove Placed Desk
- 10: Office PC Status
- 11: < Back

## MechanicRouteRoster — MECHANIC WORK STOP USE

Harness return: MechanicWalkingPaths, row 6.

- 0: Mechanic 1 Work Stops >
- 1: Mechanic 2 Work Stops >
- 2: Mechanic 3 Work Stops >
- 3: < Back to Mechanic Paths

## MechanicRouteDirection — WORK STOP USE / NO MECHANIC

Harness return: MechanicRouteRoster, row 0.

- 0: To Workstation >
- 1: From Workstation >
- 2: View Selected Routes
- 3: < Back to Mechanic List

## MechanicRoutePoints — TO WORKSTATION / NO MECHANIC

Harness return: MechanicRouteDirection, row 0.

- 0: Shared Work Stop 1
- 1: Shared Work Stop 2
- 2: Shared Work Stop 3
- 3: Shared Work Stop 4
- 4: Shared Work Stop 5
- 5: Select All Shared Stops
- 6: Deselect All Shared Stops
- 7: < Back to Route Direction

## SupplierDeliverySetup — SUPPLIER SETTINGS

Harness return: InventoryManagement, row 5.

- 0: Physical Mule Delivery
- 1: Mule Road Spawn >
- 2: Truck Unloading Spot >
- 3: Driver Box Handoff >
- 4: Pallet Storage & Capacity >
- 5: Driver Walk Stops (0-3) >
- 6: Wholesale Warehouse: Fixed
- 7: Assign Current Pickup Vehicle
- 8: Assigned Vehicle / Cargo Status
- 9: Delivery Route / Stock Status
- 10: Recover Stuck Shipment
- 11: Cancel Pending Order (press twice)
- 12: < Back to Inventory

## SupplierDriverWaypoints — DRIVER WALK (0-3)

Harness return: SupplierDeliverySetup, row 5.

- 0: Driver Stop 1 / 3 >
- 1: Driver Stop 2 / 3 >
- 2: Driver Stop 3 / 3 >
- 3: < Back

## HireMechanicRoster — HIRE A MECHANIC

Harness return: MechanicManagement, row 1.

- 0: Junior General Mechanic
- 1: Skilled Brake Technician
- 2: Experienced Diagnostic Technician
- 3: < Back to Mechanics

## HireReceptionistRoster — HIRE RECEPTIONIST

Harness return: ReceptionistManagement, row 1.

- 0: Entry Receptionist (1 queue)
- 1: Experienced Receptionist (2 queue)
- 2: Senior Service Advisor (3 queue)
- 3: < Back to Receptionist

## MechanicManagement — MECHANIC MANAGEMENT

Harness return: Employees, row 1.

- 0: Mechanic Roster >
- 1: Hire Mechanic >
- 2: Work Stop Selections >
- 3: Mechanic Walking Paths >
- 4: < Back to Employees

## ReceptionistManagement — RECEPTION MANAGEMENT

Harness return: Employees, row 2.

- 0: Receptionist Profile >
- 1: Hire Receptionist >
- 2: Receptionist Walk Route >
- 3: < Back to Employees

## ReceptionistProfile — RECEPTIONIST PROFILE

Harness return: ReceptionistManagement, row 0.

- 0: Profile & Queue Skill
- 1: Train Receptionist (+1 Capacity)
- 2: Increase Period Wage (+$25)
- 3: Dismiss Receptionist
- 4: < Back to Receptionists

## SupplierPalletStorage — STOCKROOM & PALLETS

Harness return: InventoryManagement, row 4.

- 0: Storage Capacity Status
- 1: Purchase Additional Pallet ($750)
- 2: Place / Move Pallet 1 >
- 3: Place / Move Pallet 2 >
- 4: Place / Move Pallet 3 >
- 5: Place / Move Pallet 4 >
- 6: < Back

## ManagerManagement — BUSINESS MANAGER

Harness return: Employees, row 3.

- 0: Manager Profile >
- 1: Hire Business Manager >
- 2: Manager Walk Route >
- 3: < Back to Employees

## HireManagerRoster — MANAGEMENT CANDIDATES

Harness return: ManagerManagement, row 1.

- 0: Assistant Manager — 3% discount
- 1: Experienced Manager — 6% discount
- 2: General Manager — 10% discount
- 3: < Back to Management

## ManagerProfile — MANAGER PROFILE

Harness return: ManagerManagement, row 0.

- 0: Profile & Business Reports
- 1: Train Manager (+40 XP)
- 2: Pay Manager Wage Invoice
- 3: Dismiss Manager
- 4: < Back to Manager

## ManagerWalkingPaths — MANAGER WALK ROUTE

Harness return: EmployeePaths, row 2.

- 0: Manager Walking Mode
- 1: Entry-to-Workstation Stops >
- 2: Final Workstation Approach >
- 3: < Back

## ManagerWaypoints — MANAGER ROUTE (0-5)

Harness return: ManagerWalkingPaths, row 1.

- 0: Manager Stop 1 / 5 >
- 1: Manager Stop 2 / 5 >
- 2: Manager Stop 3 / 5 >
- 3: Manager Stop 4 / 5 >
- 4: Manager Stop 5 / 5 >
- 5: < Back

## PlacementSettings — PLACEMENT SETTINGS

Harness return: PlacementSettings, row 0.

- 0: Customer Locations >
- 1: Employee Locations >
- 2: Service Bays >
- 3: Delivery & Stockroom >
- 4: Walking Routes & Approaches >
- 5: Marker Display Settings >
- 6: < Back to Management

## DeliveryPlacements — DELIVERY & STOCKROOM LOCATIONS

Harness return: DeliveryPlacements, row 0.

- 0: Mule Road Spawn >
- 1: Truck Unloading Spot >
- 2: Driver Box Handoff >
- 3: Pallet Storage & Placement >
- 4: Supplier Driver Stops >
- 5: Equipment Placement & Upgrades >
- 6: < Back to Placement Settings

## PlacementWalkingRoutes — WALKING ROUTES & APPROACHES

Harness return: PlacementWalkingRoutes, row 0.

- 0: Mechanics >
- 1: Receptionist >
- 2: Manager >
- 3: Customers >
- 4: Supplier Driver >
- 5: < Back to Placement Settings

## MechanicPlacementRoutes — MECHANIC WALKING ROUTES

Harness return: MechanicPlacementRoutes, row 0.

- 0: Shared Entry Corridor >
- 1: Shared Work Stops >
- 2: Workstation Approaches >
- 3: Selections by Mechanic >
- 4: Legacy Mechanic Checkpoint >
- 5: Walking Mode & Recovery >
- 6: < Back to Roles

## ReceptionPlacementRoutes — RECEPTIONIST WALKING ROUTES

Harness return: ReceptionPlacementRoutes, row 0.

- 0: Spawn -> Desk Stops >
- 1: Final Desk Approach >
- 2: Walking Mode >
- 3: < Back to Roles

## ManagerPlacementRoutes — MANAGER WALKING ROUTES

Harness return: ManagerPlacementRoutes, row 0.

- 0: Spawn -> Workstation Stops >
- 1: Final Workstation Approach >
- 2: Walking Mode >
- 3: < Back to Roles

## CustomerPlacementRoutes — CUSTOMER WALKING ROUTES

Harness return: CustomerPlacementRoutes, row 0.

- 0: Customer Walking Stops >
- 1: Reception Approach >
- 2: < Back to Roles

## SupplierPlacementRoutes — SUPPLIER DRIVER WALKING ROUTES

Harness return: SupplierPlacementRoutes, row 0.

- 0: Supplier Driver Stops >
- 1: Driver Box Handoff >
- 2: < Back to Roles

## OwnerTransferReview — CONFIRM OWNER WITHDRAWAL

Harness return: OwnerTransfers, row 2.

- 0: Review Amount / Business Cash
- 1: Confirm Transfer
- 2: < Back to Owner Transfers

## PartsSaleReview — CONFIRM PARTS RETAIL SALE

Harness return: PartsSales, row 4.

- 0: Review Items / Proceeds
- 1: Confirm Sale
- 2: < Back to Parts Sales

## EmployeeStatus — EMPLOYEE STATUS SNAPSHOT

Harness return: Employees, row 0.

- 0: Mechanic 1 (vacant)
- 1: Mechanic 2 (vacant)
- 2: Mechanic 3 (vacant)
- 3: Receptionist
- 4: Manager
- 5: Refresh / Full Status
- 6: < Back to Employees

