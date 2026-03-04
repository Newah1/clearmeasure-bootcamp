## ADDED Requirements

### Requirement: AssignedToDraftCommand state command
The system SHALL provide an `AssignedToDraftCommand` state command in `src/Core/Model/StateCommands/AssignedToDraftCommand.cs` that transitions a work order from Assigned status back to Draft status. The command SHALL extend `StateCommandBase` and follow the same patterns as existing state commands (e.g., `AssignedToCancelledCommand`, `DraftToAssignedCommand`).

#### Scenario: Valid unassign by creator
- **GIVEN** a work order in Assigned status with a Creator and an Assignee
- **AND** the current user is the Creator of the work order
- **WHEN** the `AssignedToDraftCommand` is evaluated via `IsValid()`
- **THEN** the command SHALL return `true`

#### Scenario: Invalid unassign when not in Assigned status
- **GIVEN** a work order in a status other than Assigned (e.g., Draft, InProgress, Complete, Cancelled)
- **WHEN** the `AssignedToDraftCommand` is evaluated via `IsValid()`
- **THEN** the command SHALL return `false`

#### Scenario: Invalid unassign by non-creator
- **GIVEN** a work order in Assigned status
- **AND** the current user is NOT the Creator of the work order
- **WHEN** the `AssignedToDraftCommand` is evaluated via `IsValid()`
- **THEN** the command SHALL return `false`

#### Scenario: Execute unassign clears assignment and returns to Draft
- **GIVEN** a work order in Assigned status with a Creator, an Assignee, and an AssignedDate
- **AND** the current user is the Creator
- **WHEN** the `AssignedToDraftCommand` is executed
- **THEN** the work order Status SHALL be `WorkOrderStatus.Draft`
- **AND** the work order Assignee SHALL be `null`
- **AND** the work order AssignedDate SHALL be `null`

### Requirement: Command metadata
The `AssignedToDraftCommand` SHALL define the following metadata:
- `Name` constant: `"Unassign"`
- `TransitionVerbPresentTense`: `"Unassign"`
- `TransitionVerbPastTense`: `"Unassigned"`
- `GetBeginStatus()`: `WorkOrderStatus.Assigned`
- `GetEndStatus()`: `WorkOrderStatus.Draft`

## MODIFIED Requirements

### Requirement: StateCommandList includes AssignedToDraftCommand
The `StateCommandList.GetAllStateCommands()` method in `src/Core/Services/Impl/StateCommandList.cs` SHALL include `AssignedToDraftCommand` in its list of returned commands.

#### Scenario: All state commands returned
- **WHEN** `GetAllStateCommands()` is called
- **THEN** the returned array SHALL contain an `AssignedToDraftCommand` instance
- **AND** the total command count SHALL be 7

### Requirement: Unit tests for AssignedToDraftCommand
Unit tests SHALL be added in `src/UnitTests/Core/Model/StateCommands/AssignedToDraftCommandTests.cs` extending `StateCommandBaseTests`. Tests SHALL follow the established 4-test pattern used by all other state command test classes and use `Assert.That(x, Is.EqualTo(y))` assertions.

#### Scenario: ShouldNotBeValidInWrongStatus
- **GIVEN** test method `ShouldNotBeValidInWrongStatus` exists in `AssignedToDraftCommandTests`
- **AND** a work order with Status set to `WorkOrderStatus.Draft` (not Assigned)
- **AND** the current user is the Creator
- **WHEN** `command.IsValid()` is called
- **THEN** `Assert.That(command.IsValid(), Is.False)` SHALL pass

#### Scenario: ShouldNotBeValidWithWrongEmployee
- **GIVEN** test method `ShouldNotBeValidWithWrongEmployee` exists in `AssignedToDraftCommandTests`
- **AND** a work order with Status set to `WorkOrderStatus.Assigned`
- **AND** the current user is NOT the Creator
- **WHEN** `command.IsValid()` is called
- **THEN** `Assert.That(command.IsValid(), Is.False)` SHALL pass

#### Scenario: ShouldBeValid
- **GIVEN** test method `ShouldBeValid` exists in `AssignedToDraftCommandTests`
- **AND** a work order with Status set to `WorkOrderStatus.Assigned`
- **AND** the current user is the Creator
- **WHEN** `command.IsValid()` is called
- **THEN** `Assert.That(command.IsValid(), Is.True)` SHALL pass

#### Scenario: ShouldTransitionStateProperly
- **GIVEN** test method `ShouldTransitionStateProperly` exists in `AssignedToDraftCommandTests`
- **AND** a work order with Status set to `WorkOrderStatus.Assigned`, an Assignee set, and an AssignedDate set
- **AND** the current user is the Creator
- **WHEN** `command.Execute(new StateCommandContext())` is called
- **THEN** `Assert.That(order.Status, Is.EqualTo(WorkOrderStatus.Draft))` SHALL pass
- **AND** `Assert.That(order.Assignee, Is.Null)` SHALL pass
- **AND** `Assert.That(order.AssignedDate, Is.Null)` SHALL pass

#### Scenario: StateCommandListTests updated
- **GIVEN** the existing test `ShouldReturnAllStateCommandsInCorrectOrder` in `StateCommandListTests`
- **WHEN** the test is updated
- **THEN** `Assert.That(commands.Length, Is.EqualTo(7))` SHALL pass
- **AND** one of the returned commands SHALL be an instance of `AssignedToDraftCommand`

### Constraints
- The `AssignedToDraftCommand` SHALL be a `record` extending `StateCommandBase(WorkOrder, CurrentUser)`, consistent with all other state commands
- The command SHALL be placed in `src/Core/Model/StateCommands/AssignedToDraftCommand.cs` within the `ClearMeasure.Bootcamp.Core.Model.StateCommands` namespace
- The `Execute()` method SHALL clear `Assignee` and `AssignedDate` before calling `base.Execute()`, mirroring the pattern in `AssignedToCancelledCommand`
- No database migration is required — the transition uses existing columns
- No new NuGet packages are required
- Maintain onion architecture: all changes are in Core and UnitTests projects only
- Follow AAA test pattern without section comments
- Test naming follows `[Scenario]` convention prefixed with `Should` or `When`
