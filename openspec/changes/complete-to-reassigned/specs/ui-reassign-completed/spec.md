## MODIFIED Requirements

### Requirement: UI enables reassignment for completed work orders
The `WorkOrderManage` page SHALL allow the creator to reassign a completed work order by displaying the "Reassign" action button and enabling the assignee dropdown. No Razor template changes are required — the existing data bindings automatically support this through the updated `CanReassign()` method and the `ValidCommands` list populated by `StateCommandList`.

#### Scenario: Creator sees Reassign button on completed work order
- **GIVEN** a work order in `Complete` status
- **AND** the current user is the work order's creator
- **WHEN** the WorkOrderManage page loads
- **THEN** the page SHALL NOT display "This work order is read-only for you at this time."
- **AND** a "Reassign" button SHALL be visible in the action buttons section
- **AND** the assignee dropdown SHALL be enabled (not disabled)

#### Scenario: Non-creator sees read-only view for completed work order
- **GIVEN** a work order in `Complete` status
- **AND** the current user is NOT the work order's creator
- **WHEN** the WorkOrderManage page loads
- **THEN** the page SHALL display "This work order is read-only for you at this time."
- **AND** no action buttons SHALL be visible

#### Scenario: Creator can select a new assignee and reassign
- **GIVEN** a work order in `Complete` status displayed on the WorkOrderManage page
- **AND** the current user is the work order's creator
- **WHEN** the creator selects a different employee from the assignee dropdown
- **AND** clicks the "Reassign" button
- **THEN** the `CompleteToAssignedCommand` SHALL execute via `IBus`
- **AND** the work order status SHALL change to `Assigned`
- **AND** the user SHALL be navigated to the work order search page

### Requirement: Integration test for Complete-to-Assigned persistence
An integration test SHALL be added in `src/IntegrationTests/DataAccess/Handlers/StateCommandHandlerForReassignTests.cs` following the existing `IntegratedTestBase` pattern.

#### Scenario: ShouldReassignCompletedWorkOrder
- **GIVEN** a work order persisted in `Complete` status with a `CompletedDate`, a creator, and an assignee
- **AND** the test creates a `CompleteToAssignedCommand` with the creator as current user
- **WHEN** the command is sent via `IBus`
- **THEN** the rehydrated work order SHALL have `Status = WorkOrderStatus.Assigned`
- **AND** `CompletedDate` SHALL be `null`
- **AND** the `Assignee` SHALL be the employee set on the work order before the command was sent

### Constraints
- No changes to `WorkOrderManage.razor` template are needed
- No changes to `WorkOrderManage.razor.cs` code-behind are needed
- The existing `CanReassign()` binding and `ValidCommands` loop handle the new command automatically
- Follow existing integration test patterns: extend `IntegratedTestBase`, use `new DatabaseTests().Clean()`, use `TestHost` for services
