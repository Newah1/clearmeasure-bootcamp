## MODIFIED Requirements

### Requirement: WorkOrder.CanReassign() supports Complete status
The `CanReassign()` method on `WorkOrder` in `src/Core/Model/WorkOrder.cs` SHALL return `true` when the work order's status is `Draft` OR `Complete`. This controls whether the assignee dropdown is editable in the WorkOrderManage page.

#### Scenario: CanReassign returns true for Draft
- **GIVEN** a work order with `Status = WorkOrderStatus.Draft`
- **WHEN** `CanReassign()` is called
- **THEN** it SHALL return `true`

#### Scenario: CanReassign returns true for Complete
- **GIVEN** a work order with `Status = WorkOrderStatus.Complete`
- **WHEN** `CanReassign()` is called
- **THEN** it SHALL return `true`

#### Scenario: CanReassign returns false for Assigned
- **GIVEN** a work order with `Status = WorkOrderStatus.Assigned`
- **WHEN** `CanReassign()` is called
- **THEN** it SHALL return `false`

#### Scenario: CanReassign returns false for InProgress
- **GIVEN** a work order with `Status = WorkOrderStatus.InProgress`
- **WHEN** `CanReassign()` is called
- **THEN** it SHALL return `false`

#### Scenario: CanReassign returns false for Cancelled
- **GIVEN** a work order with `Status = WorkOrderStatus.Cancelled`
- **WHEN** `CanReassign()` is called
- **THEN** it SHALL return `false`

### Requirement: Unit tests for CanReassign
Unit tests SHALL be added to the existing `src/UnitTests/Core/Model/WorkOrderTests.cs` file. Tests SHALL follow the existing naming convention and use NUnit 4.x assertions or Shouldly.

#### Scenario: CanReassignShouldReturnTrueForDraft
- **GIVEN** test method `CanReassignShouldReturnTrueForDraft` in `WorkOrderTests.cs`
- **WHEN** a work order has `Status = WorkOrderStatus.Draft`
- **THEN** `CanReassign()` SHALL return `true`

#### Scenario: CanReassignShouldReturnTrueForComplete
- **GIVEN** test method `CanReassignShouldReturnTrueForComplete` in `WorkOrderTests.cs`
- **WHEN** a work order has `Status = WorkOrderStatus.Complete`
- **THEN** `CanReassign()` SHALL return `true`

#### Scenario: CanReassignShouldReturnFalseForAssigned
- **GIVEN** test method `CanReassignShouldReturnFalseForAssigned` in `WorkOrderTests.cs`
- **WHEN** a work order has `Status = WorkOrderStatus.Assigned`
- **THEN** `CanReassign()` SHALL return `false`

#### Scenario: CanReassignShouldReturnFalseForInProgress
- **GIVEN** test method `CanReassignShouldReturnFalseForInProgress` in `WorkOrderTests.cs`
- **WHEN** a work order has `Status = WorkOrderStatus.InProgress`
- **THEN** `CanReassign()` SHALL return `false`

### Constraints
- The `CanReassign()` method SHALL remain on the `WorkOrder` class (no new interfaces or services)
- The method SHALL NOT take parameters — it uses the work order's own `Status` property
- No changes to the Razor template `WorkOrderManage.razor` are needed; the existing `disabled="@(Model.WorkOrder != null && !Model.WorkOrder.CanReassign())"` binding will automatically reflect the updated logic
