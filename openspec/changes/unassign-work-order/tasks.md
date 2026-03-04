## 1. State Command Implementation

- [x] 1.1 Create `AssignedToDraftCommand.cs` in `src/Core/Model/StateCommands/` extending `StateCommandBase`
- [x] 1.2 Implement `GetBeginStatus()` returning `WorkOrderStatus.Assigned`
- [x] 1.3 Implement `GetEndStatus()` returning `WorkOrderStatus.Draft`
- [x] 1.4 Implement `UserCanExecute()` checking `currentUser == WorkOrder.Creator`
- [x] 1.5 Implement `Execute()` clearing `Assignee` and `AssignedDate`, then calling `base.Execute()`

## 2. StateCommandList Registration

- [x] 2.1 Add `new AssignedToDraftCommand(workOrder, currentUser)` to `GetAllStateCommands()` in `src/Core/Services/Impl/StateCommandList.cs`

## 3. Unit Tests

- [x] 3.1 Create `AssignedToDraftCommandTests.cs` in `src/UnitTests/Core/Model/StateCommands/` extending `StateCommandBaseTests`
- [x] 3.2 Test: `ShouldNotBeValidInWrongStatus` — verify invalid when work order is not in Assigned status
- [x] 3.3 Test: `ShouldNotBeValidWithWrongEmployee` — verify invalid when current user is not the Creator
- [x] 3.4 Test: `ShouldBeValid` — verify valid when status is Assigned and current user is Creator
- [x] 3.5 Test: `ShouldTransitionStateProperly` — verify status changes to Draft, Assignee is null, AssignedDate is null
- [x] 3.6 Update `StateCommandListTests.ShouldReturnAllStateCommandsInCorrectOrder` to expect 7 commands and include `AssignedToDraftCommand`

## 4. Build Verification

- [x] 4.1 Verify the solution builds with `dotnet build src/ChurchBulletin.sln --configuration Release`
- [x] 4.2 Verify all unit tests pass with `dotnet test src/UnitTests --configuration Release`
