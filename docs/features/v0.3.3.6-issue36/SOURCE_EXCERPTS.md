# Issue #36 before/after source excerpts

Full exact diff: source.diff. Base: c8617aed5cda492214bbbe1907042bcafbfa0dc1.

## A footer layout — DrawMenu

Before:

```csharp
        string hint = GetSelectedDescription();
        string[] wrapped = WrapMenuDescription(hint, 53);
        Label(wrapped[0], left + 13, footerTop + 10, 0.30f, subtle);
        if (wrapped[1].Length > 0) Label(wrapped[1], left + 13, footerTop + 40, 0.30f, subtle);
        Label("ENTER SELECT  BACKSPACE BACK  ESC PAUSE", left + 13,
            footerTop + 86, 0.27f, white);
        Label((_selection + 1) + "/" + menuCount, left + 367, footerTop + 86, 0.27f, white);
    }
```

After:

```csharp
        string hint = GetSelectedDescription();
        string[] wrapped = WrapMenuDescription(hint, width - 26);
        int descriptionY = footerTop + (wrapped[1].Length == 0 ? 28 : 16);
        Label(wrapped[0], left + 13, descriptionY, 0.30f, subtle);
        if (wrapped[1].Length > 0) Label(wrapped[1], left + 13, descriptionY + 23, 0.30f, subtle);
        Label("ENTER SELECT  BACKSPACE BACK  ESC PAUSE", left + 13,
            footerTop + 86, 0.27f, white);
        Label((_selection + 1) + "/" + menuCount, left + 367, footerTop + 86, 0.27f, white);
    }
```

## B work-stop labels — GetMenuItems

Before:

```csharp
                    "Manager Walking Paths >", "< Back" };
            case MenuPage.MechanicWalkingPaths:
                return new string[] { "Mechanic Walking Mode", "Pickup + Return Exit Route",
                    "Spawn -> Break Waypoints >", "Workstation Access Stops >",
                    "Final Bay Approaches >", "Legacy Staff Checkpoint (MLO) >",
                    "Choose Job Waypoints by Mechanic >", "Smooth Through-Waypoints",
                    "Teleport Stuck Pedestrians", "Advanced Break <-> Bay Routes", "< Back" };
            case MenuPage.ReceptionWalkingPaths:
                return new string[] { "Receptionist Walking Mode", "Spawn -> Desk Waypoints >",
                    "Final Desk Approach >", "< Back" };
            case MenuPage.ReceptionWaypoints:
```

After:

```csharp
                    "Manager Walk Route >", "< Back" };
            case MenuPage.MechanicWalkingPaths:
                return new string[] { "Mechanic Walking Mode", "Car Walks: Entry Corridor",
                    "Shared Entry Corridor >", "Shared Work Stops >",
                    "Workstation Approaches >", "Legacy Staff Checkpoint (MLO) >",
                    "Selections by Mechanic >", "Smooth Through-Waypoints",
                    "Teleport Stuck Pedestrians", "Custom Work Stop Routes", "< Back" };
            case MenuPage.ReceptionWalkingPaths:
                return new string[] { "Receptionist Walking Mode", "Entry-to-Desk Stops >",
                    "Final Desk Approach >", "< Back" };
            case MenuPage.ReceptionWaypoints:
```

## C explicit warehouse GPS — SetSupplierPickupWaypoint

Before:

```csharp
    private void SetSupplierPickupWaypoint()
    {
        if (_deliverySupplier != 0 || !_wholesaleWarehousePickup.IsSet) return;
        try { GTA.Native.Function.Call(GTA.Native.Hash.SET_NEW_WAYPOINT,
            _wholesaleWarehousePickup.Position.X, _wholesaleWarehousePickup.Position.Y); } catch { }
    }
```

After:

```csharp
    private void SetSupplierPickupWaypoint()
    {
        if (!_bootGameplayActivated || Game.IsPaused) return;
        if (!WholesalePickupLocatorEligible())
        {
            Notify(!WholesalePickupLocationValid() ? "Warehouse pickup location unavailable. Check Supplier Settings." :
                "Warehouse GPS is available only for READY wholesale cargo awaiting collection.");
            return;
        }
        SyncWholesalePickupBlip(_supplierGameplayNow);
        if (_wholesalePickupBlip == null || !_pickupLocatorConfigured)
        { Notify("Pickup map locator is loading. Try GPS again shortly."); return; }
        try
        {
            PickupWaypointSnapshot waypoint = ReadPickupWaypoint();
            _wholesalePickupBlip.ShowRoute = true;
            _pickupWaypointAtGps = waypoint; _pickupGpsActive = true;
            Notify("Warehouse GPS enabled. Your existing waypoint is unchanged; choosing another yields this route.");
        }
        catch (Exception error) { NotePickupLocatorFailure(error); DeleteWholesalePickupBlip(); }
    }
```

## D canonical staff route — ExecuteReceptionistManagement

Before:

```csharp
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
```

After:

```csharp
    private void ExecuteReceptionistManagement()
    {
        switch (_selection)
        {
            case 0: _menuPage = MenuPage.ReceptionistProfile; _selection = 0; break;
            case 1: _menuPage = MenuPage.HireReceptionistRoster; _selection = 0; break;
            case 2: OpenPlacementMenu(MenuPage.ReceptionWalkingPaths); break;
            default: NavigateBack(); break;
        }
    }
```
