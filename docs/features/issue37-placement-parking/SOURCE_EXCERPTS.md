# Issue37 before/after excerpts

Complete exact patch: source.diff. Base and code checkpoint are identified in the developer guide.

## UpdateVehicleParking

Before

```csharp
    private void UpdateVehicleParking(CustomerOrder order, bool atBay)
    {
        WorkBay dest = VehicleDestination(order, atBay);
        if (DistanceSquared(order.Vehicle.Position, dest.Position) < 16f && order.Vehicle.Speed < 1.5f)
        {
            Ped worker = AssignedWorker(order);
            if (worker == null) { FailVehicleTransfer(order, atBay, "Mechanic missing at parking"); return; }
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE,
                worker.Handle, order.Vehicle.Handle, 0);
            order.Stage = atBay ? CustomerStage.MechanicExitingAtBay : CustomerStage.MechanicExitingAtDropoff;
            order.SecondsInStage = 0;
        }
        else if (order.SecondsInStage > _parkingTimeoutSeconds + 15)
        {
            Ped worker = AssignedWorker(order);
            if (order.ParkingRetries == 0 && worker != null && MechanicInCustomerCar(order))
            {
                order.ParkingRetries++; order.SecondsInStage = 0;
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_PARK, worker.Handle, order.Vehicle.Handle,
                    dest.Position.X, dest.Position.Y, dest.Position.Z, dest.Heading, 1, 2.8f, false);
                Log("Parking retry #" + order.Id + " direction=" + (atBay ? "BAY" : "PICKUP") + " paid=" + order.Paid, null);
            }
            else FailVehicleTransfer(order, atBay, "Parking failed at saved point; review heading/clearance");
        }
    }
```

After

```csharp
    private void UpdateVehicleParking(CustomerOrder order, bool atBay)
    {
        if (!atBay) { UpdateDirectionFreeHandoff(order, true); return; }
        WorkBay dest = VehicleDestination(order, atBay);
        if (DistanceSquared(order.Vehicle.Position, dest.Position) < 16f && order.Vehicle.Speed < 1.5f)
        {
            Ped worker = AssignedWorker(order);
            if (worker == null) { FailVehicleTransfer(order, atBay, "Mechanic missing at parking"); return; }
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE,
                worker.Handle, order.Vehicle.Handle, 0);
            order.Stage = atBay ? CustomerStage.MechanicExitingAtBay : CustomerStage.MechanicExitingAtDropoff;
            order.SecondsInStage = 0;
        }
        else if (order.SecondsInStage > _parkingTimeoutSeconds + 15)
        {
            Ped worker = AssignedWorker(order);
            if (order.ParkingRetries == 0 && worker != null && MechanicInCustomerCar(order))
            {
                order.ParkingRetries++; order.SecondsInStage = 0;
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_PARK, worker.Handle, order.Vehicle.Handle,
                    dest.Position.X, dest.Position.Y, dest.Position.Z, dest.Heading, 1, 2.8f, false);
                Log("Parking retry #" + order.Id + " direction=" + (atBay ? "BAY" : "PICKUP") + " paid=" + order.Paid, null);
            }
            else FailVehicleTransfer(order, atBay, "Parking failed at saved point; review heading/clearance");
        }
    }
```

## ConfigureRoutePoint

Before

```csharp
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
```

After

```csharp
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
        Notify(label + " SAVED: " + FormatPosition(here) + ".");
    }
```

## SetShopLocationFromPlayer

Before

```csharp
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
```

After

```csharp
    private void SetShopLocationFromPlayer()
    {
        string blocked = PlacementOperationBlock();
        if (blocked != null) { Notify(blocked); return; }
        if (Game.Player.Character == null) return;
        Vector3 newLocation = Game.Player.Character.Position;
        if (_placed && DistanceSquared(_location, newLocation) > 400f && PlacementScope(6).Exists(point => point.Target.IsSet))
        { Notify("Moving this configured shop would invalidate placements. Use Reset & Restore with the optional shop anchor first; nothing cleared."); return; }
        Vector3 previous = _location; bool placed = _placed;
        _location = newLocation; _placed = true;
        if (!Save()) { _location = previous; _placed = placed; return; }
        Notify("Shop location SET: " + FormatLocation() + ". Existing placements preserved.");
    }
```

## New helper: IssueDirectionFreeHandoff

Before: no corresponding helper. After:

```csharp
    private void IssueDirectionFreeHandoff(CustomerOrder order, Ped driver)
    {
        Vector3 p = _customerDropoff.Position;
        // Position-only, slow, traffic-aware task. No saved-facing parameter.
        GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,
            driver.Handle, order.Vehicle.Handle, p.X, p.Y, p.Z, 2.4f, CarefulDrivingStyle, 1.75f);
        order.Handoff.Issued = true;
        order.ParkingTaskIssued = true;
        order.Handoff.LastPosition = order.Vehicle.Position;
        order.Handoff.NoProgress = 0;
    }
```

## New helper: PlacementCategoryVisible

Before: no corresponding helper. After:

```csharp
    private static bool PlacementCategoryVisible(PlacementDisplayMode mode, int mask, int category, bool focused)
    {
        if (mode == PlacementDisplayMode.Hidden) return false;
        if (mode == PlacementDisplayMode.Focused) return focused;
        return mode == PlacementDisplayMode.All || (category >= 0 && category < 5 && (mask & (1 << category)) != 0);
    }
```

## New helper: TryCommitPlacementBytes

Before: no corresponding helper. After:

```csharp
    private static bool TryCommitPlacementBytes(string path, string expectedHash, byte[] candidate, Action<string, string> replace, Action<Exception> report)
    {
        string temporary = null;
        try
        {
            path = Path.GetFullPath(path); RejectPlacementReparsePath(path);
            temporary = Path.Combine(Path.GetDirectoryName(path), ".gh-placement-" + Guid.NewGuid().ToString("N") + ".ini");
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { output.Write(candidate, 0, candidate.Length); output.Flush(true); }
            if (!File.ReadAllBytes(temporary).SequenceEqual(candidate) || PlacementHash(path) != expectedHash) throw new IOException("Save changed while preparing placements; operation aborted.");
            replace(temporary, path); temporary = null; return true;
        }
        catch (Exception error) { try { report(error); } catch { } return false; }
        finally { if (temporary != null) try { File.Delete(temporary); } catch (Exception error) { try { report(error); } catch { } } }
    }
```
