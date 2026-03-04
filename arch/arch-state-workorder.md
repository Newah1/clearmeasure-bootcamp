# Work Order State Transitions

```mermaid
stateDiagram-v2
    direction LR

    [*] --> Draft : Create

    Draft --> Draft : SaveDraftCommand
    Draft --> Assigned : DraftToAssignedCommand

    Assigned --> InProgress : AssignedToInProgressCommand
    Assigned --> Cancelled : AssignedToCancelledCommand

    InProgress --> Complete : InProgressToCompleteCommand
    InProgress --> Assigned : InProgressToAssignedCommand

    Complete --> Assigned : CompleteToAssignedCommand
```