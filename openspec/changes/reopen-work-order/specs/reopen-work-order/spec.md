## ADDED Requirements

### Requirement: CompleteToAssignedCommand state command
A new state command `CompleteToAssignedCommand` SHALL be created in `src/Core/Model/StateCommands/CompleteToAssignedCommand.cs` that transitions a work order from `Complete` back to `Assigned` status. This allows the Creator to reopen a work order that was accidentally marked as complete by the Assignee.

The command SHALL be a `record` extending `StateCommandBase(WorkOrder, CurrentUser)` following the same pattern as `AssignedToCancelledCommand` and `InProgressToAssignedCommand`.

- `GetBeginStatus()` SHALL return `WorkOrderStatus.Complete`
- `GetEndStatus()` SHALL return `WorkOrderStatus.Assigned`
- `UserCanExecute(Employee)` SHALL return `true` only when `currentUser == WorkOrder.Creator`
- `TransitionVerbPresentTense` SHALL be `"Reopen"` (exposed as `const string Name = "Reopen"`)
- `TransitionVerbPastTense` SHALL be `"Reopened"`
- `Execute(StateCommandContext)` SHALL clear `CompletedDate` (set to `null`) and then call `base.Execute(context)`

#### Scenario: Command is valid when work order is Complete and current user is Creator
- **GIVEN** a `WorkOrder` with `Status` equal to `WorkOrderStatus.Complete`
- **AND** the `Creator` is set to an `Employee`
- **WHEN** `IsValid()` is called on `CompleteToAssignedCommand` with the Creator as current user
- **THEN** it SHALL return `true`

#### Scenario: Command is invalid when work order is not in Complete status
- **GIVEN** a `WorkOrder` with `Status` equal to `WorkOrderStatus.Assigned`
- **AND** the `Creator` is set to an `Employee`
- **WHEN** `IsValid()` is called on `CompleteToAssignedCommand` with the Creator as current user
- **THEN** it SHALL return `false`

#### Scenario: Command is invalid when current user is not the Creator
- **GIVEN** a `WorkOrder` with `Status` equal to `WorkOrderStatus.Complete`
- **AND** the `Creator` is set to an `Employee`
- **WHEN** `IsValid()` is called on `CompleteToAssignedCommand` with a different `Employee` as current user
- **THEN** it SHALL return `false`

#### Scenario: Executing the command transitions status and clears CompletedDate
- **GIVEN** a `WorkOrder` with `Status` equal to `WorkOrderStatus.Complete` and a non-null `CompletedDate`
- **AND** the `Creator` is set to an `Employee`
- **WHEN** `Execute(StateCommandContext)` is called on `CompleteToAssignedCommand`
- **THEN** `WorkOrder.Status` SHALL be `WorkOrderStatus.Assigned`
- **AND** `WorkOrder.CompletedDate` SHALL be `null`

### Requirement: Register CompleteToAssignedCommand in StateCommandList
The `StateCommandList` class in `src/Core/Services/Impl/StateCommandList.cs` SHALL include `CompleteToAssignedCommand` in its `GetAllStateCommands` method. The new command SHALL be added after `AssignedToCancelledCommand` (at the end of the list).

#### Scenario: GetAllStateCommands includes CompleteToAssignedCommand
- **WHEN** `GetAllStateCommands` is called
- **THEN** the returned array SHALL contain 7 commands (up from 6)
- **AND** the last command SHALL be an instance of `CompleteToAssignedCommand`

### Requirement: Unit tests for CompleteToAssignedCommand
Unit tests SHALL be added in a new file `src/UnitTests/Core/Model/StateCommands/CompleteToAssignedCommandTests.cs`. The test class SHALL extend `StateCommandBaseTests` and follow the same patterns as `InProgressToCompleteCommandTests` and `AssignedToCancelledCommandTests`.

#### Scenario: ShouldNotBeValidInWrongStatus
- **GIVEN** test method `ShouldNotBeValidInWrongStatus` exists in `src/UnitTests/Core/Model/StateCommands/CompleteToAssignedCommandTests.cs`
- **AND** a `WorkOrder` with `Status` set to `WorkOrderStatus.Assigned` (wrong status)
- **AND** the `Creator` set to an `Employee`
- **WHEN** `IsValid()` is called on `CompleteToAssignedCommand` with the Creator as current user
- **THEN** `Assert.That(command.IsValid(), Is.False)` SHALL pass

#### Scenario: ShouldNotBeValidWithWrongEmployee
- **GIVEN** test method `ShouldNotBeValidWithWrongEmployee` exists in `src/UnitTests/Core/Model/StateCommands/CompleteToAssignedCommandTests.cs`
- **AND** a `WorkOrder` with `Status` set to `WorkOrderStatus.Complete`
- **AND** the `Creator` set to an `Employee`
- **WHEN** `IsValid()` is called on `CompleteToAssignedCommand` with a different `new Employee()` as current user
- **THEN** `Assert.That(command.IsValid(), Is.False)` SHALL pass

#### Scenario: ShouldBeValid
- **GIVEN** test method `ShouldBeValid` exists in `src/UnitTests/Core/Model/StateCommands/CompleteToAssignedCommandTests.cs`
- **AND** a `WorkOrder` with `Status` set to `WorkOrderStatus.Complete`
- **AND** the `Creator` set to an `Employee`
- **WHEN** `IsValid()` is called on `CompleteToAssignedCommand` with the Creator as current user
- **THEN** `Assert.That(command.IsValid(), Is.True)` SHALL pass

#### Scenario: ShouldTransitionStateProperly
- **GIVEN** test method `ShouldTransitionStateProperly` exists in `src/UnitTests/Core/Model/StateCommands/CompleteToAssignedCommandTests.cs`
- **AND** a `WorkOrder` with `Number` set to `"123"`, `Status` set to `WorkOrderStatus.Complete`, and `CompletedDate` set to a non-null value
- **AND** the `Creator` set to an `Employee`
- **WHEN** `Execute(new StateCommandContext())` is called on `CompleteToAssignedCommand`
- **THEN** `Assert.That(order.Status, Is.EqualTo(WorkOrderStatus.Assigned))` SHALL pass
- **AND** `Assert.That(order.CompletedDate, Is.Null)` SHALL pass

### Requirement: Update StateCommandListTests for new command count
The existing test `ShouldReturnAllStateCommandsInCorrectOrder` in `src/UnitTests/Core/Services/StateCommandListTests.cs` SHALL be updated to expect 7 commands (up from 6) and SHALL assert that `commands[6]` is an instance of `CompleteToAssignedCommand`.

#### Scenario: StateCommandListTests reflects new command
- **GIVEN** the test `ShouldReturnAllStateCommandsInCorrectOrder` in `src/UnitTests/Core/Services/StateCommandListTests.cs`
- **WHEN** the test is executed
- **THEN** `Assert.That(commands.Length, Is.EqualTo(7))` SHALL pass
- **AND** `Assert.That(commands[6], Is.InstanceOf(typeof(CompleteToAssignedCommand)))` SHALL pass

### Requirement: Integration test for CompleteToAssignedCommand persistence
An integration test SHALL be added in a new file `src/IntegrationTests/DataAccess/Handlers/StateCommandHandlerForReopenTests.cs` that verifies the reopen command persists correctly through the database round-trip. The test SHALL follow the same pattern as `StateCommandHandlerForCompleteTests`.

#### Scenario: ShouldReopenWorkOrder
- **GIVEN** test method `ShouldReopenWorkOrder` exists in `src/IntegrationTests/DataAccess/Handlers/StateCommandHandlerForReopenTests.cs`
- **AND** the test extends `IntegratedTestBase`
- **AND** the test calls `new DatabaseTests().Clean()` at the start
- **AND** a `WorkOrder` is created with `Status` set to `WorkOrderStatus.Complete`, a `Creator`, an `Assignee`, and a non-null `CompletedDate`
- **AND** the work order is persisted using `TestHost.GetRequiredService<DbContext>()`
- **WHEN** a `CompleteToAssignedCommand` is created with the work order and the Creator as current user
- **AND** the command is serialized/deserialized via `RemotableRequestTests.SimulateRemoteObject(command)`
- **AND** the command is handled by `TestHost.GetRequiredService<StateCommandHandler>()`
- **THEN** the work order retrieved from the database SHALL have `Status` equal to `WorkOrderStatus.Assigned`
- **AND** `CompletedDate` SHALL be `null`
- **AND** `Creator` and `Assignee` SHALL be preserved

### Requirement: Update state diagram documentation
The state diagram in `arch/arch-state-workorder.md` SHALL be updated to include the new `Complete --> Assigned` transition.

#### Scenario: State diagram includes Reopen transition
- **GIVEN** the state diagram in `arch/arch-state-workorder.md`
- **WHEN** the diagram is rendered
- **THEN** it SHALL show a transition from `Complete` to `Assigned` labeled `CompleteToAssignedCommand`
- **AND** the `Complete --> [*]` terminal transition SHALL be removed (Complete is no longer always a terminal state)

### Constraints
- The `CompleteToAssignedCommand` SHALL be added to the `src/Core/Model/StateCommands/` directory (Core project, no new project references)
- The command SHALL follow the existing record-based pattern: `public record CompleteToAssignedCommand(WorkOrder WorkOrder, Employee CurrentUser) : StateCommandBase(WorkOrder, CurrentUser)`
- Only the **Creator** (not the Assignee) SHALL be able to execute this command, since the use case is the Creator reopening a work order that was accidentally completed
- `CompletedDate` SHALL be cleared when the work order is reopened (reverting the side effect of `InProgressToCompleteCommand`)
- Unit tests SHALL extend `StateCommandBaseTests` and follow the exact test naming and assertion patterns of `InProgressToCompleteCommandTests` and `AssignedToCancelledCommandTests`
- Integration tests SHALL follow the exact pattern of `StateCommandHandlerForCompleteTests` including `DatabaseTests().Clean()`, `Faker<T>()`, `RemotableRequestTests.SimulateRemoteObject()`, and Shouldly assertions
- Maintain onion architecture (Core has no project references)
- Follow existing code style: file-scoped namespaces, NUnit `[TestFixture]`/`[Test]` attributes, `Assert.That()` for unit tests, `ShouldBe()` for integration tests
