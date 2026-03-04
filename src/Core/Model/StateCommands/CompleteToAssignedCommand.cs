using ClearMeasure.Bootcamp.Core.Model.Constants;
using ClearMeasure.Bootcamp.Core.Model.Events;
using ClearMeasure.Bootcamp.Core.Services;

namespace ClearMeasure.Bootcamp.Core.Model.StateCommands;

public record CompleteToAssignedCommand(WorkOrder WorkOrder, Employee CurrentUser) : StateCommandBase(WorkOrder,
    CurrentUser)
{
    public const string Name = "Reopen";
    public override string TransitionVerbPresentTense => Name;

    public override string TransitionVerbPastTense => "Reopened";

    public override WorkOrderStatus GetBeginStatus()
    {
        return WorkOrderStatus.Complete;
    }

    public override WorkOrderStatus GetEndStatus()
    {
        return WorkOrderStatus.Assigned;
    }

    protected override bool UserCanExecute(Employee currentUser)
    {
        return currentUser == WorkOrder.Creator;
    }

    public override void Execute(StateCommandContext context)
    {
        WorkOrder.CompletedDate = null;
        base.Execute(context);

        var assignedToAiBot = WorkOrder.Assignee?.Roles
            .Any(x => x.Name == Roles.Bot) ?? false;

        if (assignedToAiBot)
        {
            StateTransitionEvent = new WorkOrderAssignedToBotEvent(WorkOrder.Number ?? string.Empty, WorkOrder.Assignee!.Id);
        }
    }
}
