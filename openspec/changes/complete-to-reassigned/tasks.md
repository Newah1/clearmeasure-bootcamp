## 1. Core Domain Changes

- [ ] 1.1 Create `CompleteToAssignedCommand` in `src/Core/Model/StateCommands/` following the `StateCommandBase` pattern
- [ ] 1.2 Update `WorkOrder.CanReassign()` in `src/Core/Model/WorkOrder.cs` to return `true` for Complete status
- [ ] 1.3 Register `CompleteToAssignedCommand` in `StateCommandList.GetAllStateCommands()` in `src/Core/Services/Impl/StateCommandList.cs`

## 2. Unit Tests

- [ ] 2.1 Add unit tests for `CompleteToAssignedCommand` validation and execution in `src/UnitTests/`
- [ ] 2.2 Add unit tests for updated `WorkOrder.CanReassign()` in `src/UnitTests/Core/Model/WorkOrderTests.cs`
- [ ] 2.3 Update `StateCommandListTests` to include `CompleteToAssignedCommand` in the expected command list

## 3. Integration Tests

- [ ] 3.1 Add `StateCommandHandlerForReassignTests.cs` in `src/IntegrationTests/DataAccess/Handlers/` testing Complete-to-Assigned persistence

## 4. Architecture Documentation

- [ ] 4.1 Update `arch/arch-state-workorder.md` state diagram to include the Complete-to-Assigned transition
