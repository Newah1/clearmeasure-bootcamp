## Context

The work order workflow currently supports transitions: Draft → Assigned → InProgress → Complete, with Cancelled reachable from Assigned. The `AssignedToCancelledCommand` allows a creator to cancel an assigned work order, which clears the Assignee and AssignedDate but sets the status to Cancelled — a terminal state. There is no reverse path from Assigned back to Draft.

The `CanReassign()` method on `WorkOrder` already returns `true` only when the status is Draft, confirming the domain intent that reassignment requires the work order to be in Draft status first.

## Goals / Non-Goals

**Goals:**
- Allow the creator to unassign an assigned work order, returning it to Draft status
- Clear the Assignee and AssignedDate when unassigning, so the work order can be reassigned
- Follow the established state command pattern (`StateCommandBase`, `IStateCommand`)
- Maintain the existing authorization model: only the Creator can unassign

**Non-Goals:**
- Allowing the Assignee to unassign themselves (the Assignee has the "Shelve" command for pausing work)
- Allowing unassign from InProgress or Complete status (only from Assigned)
- UI changes beyond what the existing `StateCommandList` mechanism provides automatically

## Decisions

### Decision 1: New `AssignedToDraftCommand` following the `StateCommandBase` pattern

**Rationale:** Every state transition in the system is modeled as a record extending `StateCommandBase`. This new transition is no different. The command handles its own authorization (`UserCanExecute`) and side effects (`Execute`).

**Alternatives considered:**
- Extending `AssignedToCancelledCommand` to optionally return to Draft: Violates single responsibility and muddies the semantics of cancellation.
- Adding an "unassign" method directly on `WorkOrder`: Bypasses the state command pattern and authorization model.

### Decision 2: Creator-only authorization

**Rationale:** The creator is the authority over assignment. `DraftToAssignedCommand` requires the creator to assign, so `AssignedToDraftCommand` mirrors this by requiring the creator to unassign. This is consistent with `AssignedToCancelledCommand`, which also requires the creator.

### Decision 3: Clear Assignee and AssignedDate on unassign

**Rationale:** Returning to Draft means the work order is ready for reassignment. The `AssignedToCancelledCommand` already clears both fields when cancelling. The same cleanup applies when returning to Draft, ensuring `CanReassign()` returns `true` and the UI shows the assignee dropdown.

### Decision 4: Transition verb "Unassign"

**Rationale:** "Unassign" clearly communicates the action to the user. It is the inverse of "Assign" and is unambiguous in the UI button context. The past tense "Unassigned" works naturally for audit trail messages.

## Risks / Trade-offs

- **[Workflow complexity]** Adding a reverse path increases the number of state transitions. → Mitigation: The transition is simple (Assigned → Draft) with well-understood semantics. No new intermediate states are introduced.
- **[Assignee notification]** The assignee is not notified when unassigned. → Mitigation: Notification is out of scope for the initial implementation, consistent with how `AssignedToCancelledCommand` does not notify the assignee.

## Open Questions

- Should there be an audit log entry or event published when a work order is unassigned? (Currently out of scope — matches existing commands that do not publish events unless targeting a bot assignee.)
