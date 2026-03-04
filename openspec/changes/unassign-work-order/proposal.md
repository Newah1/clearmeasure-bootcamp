## Why

When a creator assigns a work order, the only way to change the assignee is to cancel the work order entirely and create a new one. This is disruptive because it loses the original work order number, description, and audit trail. Creators need the ability to "unassign" a work order — returning it to Draft status — so they can reassign it to a different employee without cancelling and recreating it.

## What Changes

- New state command `AssignedToDraftCommand` that transitions a work order from Assigned back to Draft
- The command clears the Assignee and AssignedDate, restoring the work order to an editable Draft state
- Only the Creator of the work order can execute this command
- The command is registered in `StateCommandList` alongside existing state commands
- Unit tests following the established 4-test pattern for state commands
- `StateCommandListTests` updated to reflect the new command count

## Capabilities

### New Capabilities
- `assigned-to-draft-command`: A state command allowing the work order creator to unassign an assigned work order, returning it to Draft status

### Modified Capabilities
- `StateCommandList` is updated to include the new `AssignedToDraftCommand` in its command registry

## Impact

- **Core project**: New `AssignedToDraftCommand` record added to `src/Core/Model/StateCommands/`
- **StateCommandList**: One additional command registered in `GetAllStateCommands()`
- **Database**: No schema changes — status transitions use existing columns
- **UI**: The new command button appears automatically via the existing `StateCommandList`/`GetValidStateCommands()` mechanism used by the WorkOrderManage page
- **CI/CD**: No pipeline changes needed
