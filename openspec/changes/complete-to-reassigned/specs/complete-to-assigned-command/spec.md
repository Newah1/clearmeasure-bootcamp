## ADDED Requirements

### Requirement: CompleteToAssignedCommand transitions a completed work order back to Assigned
The system SHALL include a `CompleteToAssignedCommand` record in `src/Core/Model/StateCommands/` that extends `StateCommandBase`. The command SHALL transition a work order from `Complete` status to `Assigned` status, enabling the creator to reassign completed work to a different (or the same) employee.

#### Scenario: Valid reassignment by creator
- **GIVEN** a work order in `Complete` status with a creator and an assignee
- **AND** the current user is the work order's creator
- **WHEN** `CompleteToAssignedCommand.Execute()` is called
- **THEN** the work order's status SHALL be `Assigned`
- **AND** the work order's `CompletedDate` SHALL be `null`

#### Scenario: Command reports correct begin and end statuses
- **WHEN** `GetBeginStatus()` is called on `CompleteToAssignedCommand`
- **THEN** it SHALL return `WorkOrderStatus.Complete`
- **WHEN** `GetEndStatus()` is called on `CompleteToAssignedCommand`
- **THEN** it SHALL return `WorkOrderStatus.Assigned`

#### Scenario: Only the creator can execute the command
- **GIVEN** a work order in `Complete` status
- **AND** the current user is NOT the work order's creator
- **WHEN** `IsValid()` is called on `CompleteToAssignedCommand`
- **THEN** it SHALL return `false`

#### Scenario: Command is invalid when work order is not in Complete status
- **GIVEN** a work order in `Assigned` status (or any status other than `Complete`)
- **AND** the current user is the work order's creator
- **WHEN** `IsValid()` is called on `CompleteToAssignedCommand`
- **THEN** it SHALL return `false`

#### Scenario: Command uses "Reassign" as transition verb
- **WHEN** `TransitionVerbPresentTense` is accessed on `CompleteToAssignedCommand`
- **THEN** it SHALL return `"Reassign"`
- **WHEN** `TransitionVerbPastTense` is accessed
- **THEN** it SHALL return `"Reassigned"`

### Requirement: CompleteToAssignedCommand is registered in StateCommandList
The `StateCommandList.GetAllStateCommands()` method in `src/Core/Services/Impl/StateCommandList.cs` SHALL include `CompleteToAssignedCommand` in the list of all state commands.

#### Scenario: Command appears in the full command list
- **WHEN** `GetAllStateCommands()` is called with any work order and employee
- **THEN** the returned array SHALL contain an instance of `CompleteToAssignedCommand`
- **AND** it SHALL appear after `AssignedToCancelledCommand` in the list

#### Scenario: Command appears in valid commands for completed work order with creator
- **GIVEN** a work order in `Complete` status
- **AND** the current user is the work order's creator
- **WHEN** `GetValidStateCommands()` is called
- **THEN** the returned array SHALL contain a `CompleteToAssignedCommand`

### Requirement: Unit tests for CompleteToAssignedCommand
Unit tests SHALL be added in `src/UnitTests/Core/Model/StateCommands/CompleteToAssignedCommandTests.cs` using NUnit 4.x and Shouldly assertions. Tests SHALL follow the AAA pattern and existing naming conventions.

#### Scenario: ShouldBeValidWhenCompleteAndCreator
- **GIVEN** a work order with `Status = WorkOrderStatus.Complete` and `Creator = employee`
- **WHEN** `IsValid()` is called with the creator as current user
- **THEN** it SHALL return `true`

#### Scenario: ShouldBeInvalidWhenNotComplete
- **GIVEN** a work order with `Status = WorkOrderStatus.Assigned` and `Creator = employee`
- **WHEN** `IsValid()` is called with the creator as current user
- **THEN** it SHALL return `false`

#### Scenario: ShouldBeInvalidWhenNotCreator
- **GIVEN** a work order with `Status = WorkOrderStatus.Complete` and `Creator = employeeA`
- **WHEN** `IsValid()` is called with `employeeB` (not the creator) as current user
- **THEN** it SHALL return `false`

#### Scenario: ShouldTransitionToAssigned
- **GIVEN** a work order with `Status = WorkOrderStatus.Complete`
- **WHEN** `Execute()` is called
- **THEN** `WorkOrder.Status` SHALL be `WorkOrderStatus.Assigned`

#### Scenario: ShouldClearCompletedDate
- **GIVEN** a work order with `Status = WorkOrderStatus.Complete` and `CompletedDate = someDate`
- **WHEN** `Execute()` is called
- **THEN** `WorkOrder.CompletedDate` SHALL be `null`

#### Scenario: ShouldHaveCorrectTransitionVerbs
- **WHEN** `TransitionVerbPresentTense` is accessed
- **THEN** it SHALL be `"Reassign"`
- **WHEN** `TransitionVerbPastTense` is accessed
- **THEN** it SHALL be `"Reassigned"`

### Requirement: StateCommandListTests updated
The existing test `ShouldReturnAllStateCommandsInCorrectOrder` in `src/UnitTests/Core/Services/StateCommandListTests.cs` SHALL be updated to expect 7 commands (adding `CompleteToAssignedCommand` at the end).

### Constraints
- `CompleteToAssignedCommand` SHALL be a `record` extending `StateCommandBase(WorkOrder, Employee)`, consistent with all other state commands
- The command SHALL NOT modify `AssignedDate` — it retains the original assigned date from when the work order was first assigned
- The command SHALL be in the `ClearMeasure.Bootcamp.Core.Model.StateCommands` namespace
- No changes to `StateCommandBase` or `StateCommandHandler` are permitted
