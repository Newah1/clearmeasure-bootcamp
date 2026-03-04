## Why

Completed work orders are currently a terminal state with no outbound transitions. If a creator discovers that a completed work order needs further attention — for example, the work was incomplete or a different team member needs to redo it — the only option is to create an entirely new work order, losing the original's history, number, and attachments. Allowing the creator to reassign a completed work order preserves continuity and avoids duplication.

## What Changes

- Add `CompleteToAssignedCommand` to `src/Core/Model/StateCommands/` that transitions a work order from Complete back to Assigned with a new assignee
- The command validates that only the work order's creator can execute it
- The command clears the `CompletedDate` when moving back to Assigned
- Update `WorkOrder.CanReassign()` to return `true` for Complete status in addition to Draft
- Register the new command in `StateCommandList`
- Update the `WorkOrderManage` page so the assignee dropdown is editable when the work order is in Complete status and the current user is the creator
- Update the state diagram in `arch/arch-state-workorder.md` to include the new transition

## Capabilities

### New Capabilities
- Creators can reassign a completed work order to a different employee, transitioning it back to Assigned status
- The assignee dropdown is enabled on the WorkOrderManage page for completed work orders when the current user is the creator

### Modified Capabilities
- `WorkOrder.CanReassign()` returns `true` for both Draft and Complete statuses
- `StateCommandList.GetAllStateCommands()` includes the new `CompleteToAssignedCommand`
- The state diagram includes the Complete-to-Assigned transition

## Impact

- **Core** — New `CompleteToAssignedCommand` state command; updated `CanReassign()` method on `WorkOrder`
- **Core (Services)** — `StateCommandList` updated to register the new command
- **UI.Shared** — No template changes needed; the existing `CanReassign()` check on the assignee dropdown and the `ValidCommands` loop will automatically pick up the new command
- **Architecture docs** — State diagram updated with the new transition
- **Database** — No schema changes required
- **No new NuGet packages required**
