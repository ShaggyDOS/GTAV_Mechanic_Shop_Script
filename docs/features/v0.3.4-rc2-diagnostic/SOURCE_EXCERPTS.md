# RC2 before/after source excerpts

Full exact unified diff: source.diff. Gameplay bodies are preserved inside new diagnostic scopes.

## BeginEnterVehicle

Before (verified RC1)

```csharp
    private void BeginEnterVehicle(CustomerOrder order, bool returning)
    {
        Ped worker = AssignedWorker(order);
        if (worker == null || order.Vehicle == null || !order.Vehicle.Exists())
        { FailVehicleTransfer(order, !returning, "Mechanic or customer vehicle missing"); return; }
        GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, worker.Handle);
        GTA.Native.Function.Call(GTA.Native.Hash.TASK_ENTER_VEHICLE,
            worker.Handle, order.Vehicle.Handle, 18000, -1, 1.0f, 1, 0);
        order.Stage = returning ? CustomerStage.MechanicEnteringForReturn : CustomerStage.MechanicEnteringVehicle;
        order.SecondsInStage = 0;
    }
```

After (diagnostic RC2)

```csharp
    private void BeginEnterVehicle(CustomerOrder order, bool returning)
    {
        long trace = BeginCustomerVehicleTrace(order, 3);
        try
        {
            Ped worker = AssignedWorker(order);
            if (worker == null || order.Vehicle == null || !order.Vehicle.Exists())
            { FailVehicleTransfer(order, !returning, "Mechanic or customer vehicle missing"); return; }
            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS, worker.Handle);
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_ENTER_VEHICLE,
                worker.Handle, order.Vehicle.Handle, 18000, -1, 1.0f, 1, 0);
            order.Stage = returning ? CustomerStage.MechanicEnteringForReturn : CustomerStage.MechanicEnteringVehicle;
            order.SecondsInStage = 0;
        }
        finally { EndCustomerVehicleTrace(order, trace); }
    }
```

## BeginVehicleDrive

Before (verified RC1)

```csharp
    private void BeginVehicleDrive(CustomerOrder order, bool toBay)
    {
        Ped worker = AssignedWorker(order);
        if (worker == null) { FailVehicleTransfer(order, toBay, "Mechanic disappeared while driving"); return; }
        WorkBay destination = VehicleDestination(order, toBay);
        order.ParkingRetries = 0;
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
```

After (diagnostic RC2)

```csharp
    private void BeginVehicleDrive(CustomerOrder order, bool toBay)
    {
        long trace = BeginCustomerVehicleTrace(order, 5);
        try
        {
            Ped worker = AssignedWorker(order);
            if (worker == null) { FailVehicleTransfer(order, toBay, "Mechanic disappeared while driving"); return; }
            WorkBay destination = VehicleDestination(order, toBay);
            order.ParkingRetries = 0;
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
        finally { EndCustomerVehicleTrace(order, trace); }
    }
```
