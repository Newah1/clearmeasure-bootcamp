using ClearMeasure.Bootcamp.Core.Model;
using ClearMeasure.Bootcamp.Core.Model.StateCommands;
using ClearMeasure.Bootcamp.Core.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearMeasure.Bootcamp.Core;

namespace ClearMeasure.Bootcamp.DataAccess.Handlers;

public class StateCommandHandler(DbContext dbContext, TimeProvider time, IDistributedBus distributedBus, ILogger<StateCommandHandler> logger)
    : IRequestHandler<StateCommandBase, StateCommandResult>
{
    public async Task<StateCommandResult> Handle(StateCommandBase request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing");
        request.Execute(new StateCommandContext { CurrentDateTime = time.GetUtcNow().DateTime });

        var order = request.WorkOrder;
        if (order.Assignee == order.Creator)
        {
            order.Assignee = order.Creator; //EFCore reference checking
        }

        DeduplicateRoleReferences(order);

        if (order.Id == Guid.Empty)
        {
            dbContext.Attach(order);
            dbContext.Add(order);
        }
        else
        {
            dbContext.Attach(order);
            dbContext.Update(order);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var loweredTransitionVerb = request.TransitionVerbPastTense.ToLower();
        var workOrderNumber = order.Number;
        var fullName = request.CurrentUser.GetFullName();

        var debugMessage = string.Format("{0} has {1} work order {2}", fullName, loweredTransitionVerb, workOrderNumber);
        logger.LogDebug(debugMessage);
        logger.LogInformation("Executed");

        var result = new StateCommandResult(order, request.TransitionVerbPresentTense, debugMessage);

        await distributedBus.PublishAsync(request.StateTransitionEvent, cancellationToken);
        return result;
    }

    /// <summary>
    /// Ensures that Role instances shared across Creator and Assignee use the same
    /// object reference, preventing EF Core from tracking duplicate entities with the same key.
    /// </summary>
    private static void DeduplicateRoleReferences(WorkOrder order)
    {
        if (order.Creator?.Roles == null || order.Assignee?.Roles == null)
            return;

        if (ReferenceEquals(order.Creator, order.Assignee))
            return;

        var roleMap = new Dictionary<Guid, Role>();
        foreach (var role in order.Creator.Roles)
        {
            roleMap[role.Id] = role;
        }

        var deduplicatedRoles = new HashSet<Role>();
        foreach (var role in order.Assignee.Roles)
        {
            if (roleMap.TryGetValue(role.Id, out var existingRole))
            {
                deduplicatedRoles.Add(existingRole);
            }
            else
            {
                deduplicatedRoles.Add(role);
            }
        }

        order.Assignee.Roles.Clear();
        foreach (var role in deduplicatedRoles)
        {
            order.Assignee.Roles.Add(role);
        }
    }
}