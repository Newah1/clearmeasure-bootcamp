## Context

The ChurchBulletin work order system follows a state machine pattern where each state transition is modeled as a `StateCommandBase`-derived record. Commands declare their begin/end statuses, authorization rules, and any side effects in their `Execute()` override. A single `StateCommandHandler` in the DataAccess layer persists the result. The `StateCommandList` acts as a registry, returning all commands and filtering to valid ones based on the work order's current status and the requesting user's role.

Currently, `Complete` is a terminal state with no outbound transitions. The `CanReassign()` method on `WorkOrder` only returns `true` for `Draft` status, which controls whether the assignee dropdown is editable in the UI.

## Goals / Non-Goals

**Goals:**
- Allow the creator of a completed work order to reassign it to a (potentially different) employee, transitioning it back to Assigned status
- Follow the existing state command pattern exactly — no new abstractions or handler changes
- Preserve work order history (number, title, description, attachments, creator) across the reassignment
- Clear `CompletedDate` when transitioning back from Complete to Assigned

**Non-Goals:**
- Allowing reassignment by anyone other than the creator
- Allowing reassignment from Cancelled status (Cancelled remains terminal)
- Adding new roles or permissions beyond the existing `CanCreateWorkOrder` / `CanFulfillWorkOrder` model
- Modifying the `StateCommandHandler` or `StateCommandBase` abstractions
- Adding new database columns or migrations

## Decisions

### Decision 1: Model as a new `CompleteToAssignedCommand` state command

**Rationale:** Every state transition in the system is a distinct `StateCommandBase`-derived record. Adding `CompleteToAssignedCommand` follows this established pattern exactly. The command declares `Complete` as its begin status, `Assigned` as its end status, restricts execution to the creator, and clears `CompletedDate` in its `Execute()` override.

**Alternatives considered:**
- Reusing `DraftToAssignedCommand` with a modified begin status: Would break the single-responsibility of that command and its validation logic
- A generic "ReassignWorkOrderCommand" that works from multiple statuses: Over-engineers the solution; the feature proposal 012 describes Assigned/InProgress reassignment separately — this feature is specifically about Complete-to-Assigned

### Decision 2: Update `CanReassign()` to include Complete status

**Rationale:** The `CanReassign()` method on `WorkOrder` controls the assignee dropdown's `disabled` attribute in the Razor view. Adding `Status == WorkOrderStatus.Complete` to the condition is the minimal change needed to enable the UI for this new transition. No Razor template changes are required.

**Alternatives considered:**
- Adding a separate `CanReassignFromComplete()` method: Unnecessary complexity; `CanReassign()` is the single control point and extending it is cleaner

### Decision 3: Clear `CompletedDate` on reassignment

**Rationale:** When a completed work order is moved back to Assigned, the previous completion is no longer valid. Clearing `CompletedDate` is consistent with how `AssignedToCancelledCommand` clears `AssignedDate` and `Assignee` when cancelling. The `CompletedDate` will be set again when the work order is eventually re-completed.

### Decision 4: Use "Reassign" as the transition verb

**Rationale:** The verb "Reassign" clearly communicates the action to the user and distinguishes this command from "Assign" (Draft-to-Assigned). This appears as the button label in the UI and is used by `StateCommandList.GetMatchingCommand()` to resolve the command by name.

## Risks / Trade-offs

- **[State complexity]** Adding an outbound transition from Complete introduces a cycle in the state machine (Complete -> Assigned -> InProgress -> Complete). This is intentional and mirrors how `InProgressToAssignedCommand` (Shelve) already creates a cycle between InProgress and Assigned.
- **[CompletedDate loss]** Clearing `CompletedDate` means the original completion timestamp is lost. This is acceptable because the work order is no longer complete — the completion date will be set again on re-completion.
- **[No audit trail]** The system does not currently maintain an audit log of state transitions. This is a pre-existing limitation, not introduced by this change.

## Open Questions

- Should there be a confirmation dialog in the UI before reassigning a completed work order, given this is a more significant action than other transitions?
