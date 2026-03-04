## MODIFIED Requirements

### Requirement: State diagram includes Complete-to-Assigned transition
The Mermaid state diagram in `arch/arch-state-workorder.md` SHALL be updated to include the new `CompleteToAssignedCommand` transition from `Complete` to `Assigned`.

#### Scenario: Diagram shows Complete-to-Assigned arrow
- **WHEN** the state diagram is rendered
- **THEN** it SHALL show a transition arrow from `Complete` to `Assigned` labeled `CompleteToAssignedCommand`
- **AND** `Complete` SHALL no longer transition directly to `[*]` (it is no longer a terminal state)

### Constraints
- The diagram SHALL remain in Mermaid `stateDiagram-v2` format
- Only the `Complete --> [*]` line is removed and replaced with `Complete --> Assigned : CompleteToAssignedCommand`
