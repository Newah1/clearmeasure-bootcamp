using ClearMeasure.Bootcamp.Core.Model;
using ClearMeasure.Bootcamp.Core.Model.Constants;
using ClearMeasure.Bootcamp.Core.Model.Events;
using ClearMeasure.Bootcamp.Core.Model.StateCommands;
using ClearMeasure.Bootcamp.Core.Services;

namespace ClearMeasure.Bootcamp.UnitTests.Core.Model.StateCommands;

[TestFixture]
public class CompleteToAssignedCommandTests : StateCommandBaseTests
{
    [Test]
    public void ShouldNotBeValidInWrongStatus()
    {
        var order = new WorkOrder();
        order.Status = WorkOrderStatus.Assigned;
        var employee = new Employee();
        order.Creator = employee;

        var command = new CompleteToAssignedCommand(order, employee);
        Assert.That(command.IsValid(), Is.False);
    }

    [Test]
    public void ShouldNotBeValidWithWrongEmployee()
    {
        var order = new WorkOrder();
        order.Status = WorkOrderStatus.Complete;
        var employee = new Employee();
        var differentEmployee = new Employee();
        order.Creator = employee;

        var command = new CompleteToAssignedCommand(order, differentEmployee);
        Assert.That(command.IsValid(), Is.False);
    }

    [Test]
    public void ShouldBeValid()
    {
        var order = new WorkOrder();
        order.Status = WorkOrderStatus.Complete;
        var employee = new Employee();
        order.Creator = employee;

        var command = new CompleteToAssignedCommand(order, employee);
        Assert.That(command.IsValid(), Is.True);
    }

    [Test]
    public void ShouldTransitionStateProperly()
    {
        var order = new WorkOrder();
        order.Number = "123";
        order.Status = WorkOrderStatus.Complete;
        order.CompletedDate = new DateTime(2026, 1, 15);
        var employee = new Employee();
        order.Creator = employee;

        var command = new CompleteToAssignedCommand(order, employee);
        command.Execute(new StateCommandContext());

        Assert.That(order.Status, Is.EqualTo(WorkOrderStatus.Assigned));
        Assert.That(order.CompletedDate, Is.Null);
    }

    [Test]
    public void ShouldHaveCorrectTransitionVerbs()
    {
        var order = new WorkOrder();
        var employee = new Employee();

        var command = new CompleteToAssignedCommand(order, employee);

        Assert.That(command.TransitionVerbPresentTense, Is.EqualTo("Reassign"));
        Assert.That(command.TransitionVerbPastTense, Is.EqualTo("Reassigned"));
    }

    [Test]
    public void ShouldEmitBotEventWhenReassignedToBot()
    {
        var order = new WorkOrder();
        order.Number = "456";
        order.Status = WorkOrderStatus.Complete;
        order.CompletedDate = new DateTime(2026, 1, 15);
        var creator = new Employee();
        order.Creator = creator;

        var botEmployee = new Employee();
        botEmployee.AddRole(new Role(Roles.Bot, false, true));
        order.Assignee = botEmployee;

        var command = new CompleteToAssignedCommand(order, creator);
        command.Execute(new StateCommandContext());

        Assert.That(command.StateTransitionEvent, Is.InstanceOf<WorkOrderAssignedToBotEvent>());
    }

    [Test]
    public void ShouldNotEmitBotEventWhenReassignedToNonBot()
    {
        var order = new WorkOrder();
        order.Number = "789";
        order.Status = WorkOrderStatus.Complete;
        order.CompletedDate = new DateTime(2026, 1, 15);
        var creator = new Employee();
        order.Creator = creator;

        var regularEmployee = new Employee();
        order.Assignee = regularEmployee;

        var command = new CompleteToAssignedCommand(order, creator);
        command.Execute(new StateCommandContext());

        Assert.That(command.StateTransitionEvent, Is.Null);
    }

    protected override StateCommandBase GetStateCommand(WorkOrder order, Employee employee)
    {
        return new CompleteToAssignedCommand(order, employee);
    }
}
