using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Domain.WorkOrders;

public static class WorkOrderErrors
{
    public static Error WorkOrderIdRequired => Error.Validation(
        code: "WorkOrder.WorkOrderId.Required",
        description: "WorkOrder Id Is Required");

    public static Error VehicleIdRequired => Error.Validation(
        code: "WorkOrder.VehicleId.Required",
        description: "Vehicle Id Is Required");

    public static Error RepairTasksRequired => Error.Validation(
        code: "WorkOrder.RepairTasks.Required",
        description: "At least one repair task is required");

    public static Error LaborIdRequired => Error.Validation(
        code: "WorkOrder.LaborId.Required",
        description: "Labor Id is required");

    public static Error InvalidTiming => Error.Conflict(
        code: "WorkOrder.InvalidTiming",
        description: "End time must be after start time.");

    public static Error SpotInvalid => Error.Validation(
        code: "WorkOrder.SpotInvalid",
        description: "The provided spot is invalid");

    public static Error Readonly => Error.Conflict(
        code: "WorkOrder.Readonly",
        description: "WorkOrder is Read-Only.");

    public static Error TimingReadonly(string id, WorkOrderState state) => Error.Conflict(
        code: "WorkOrder.TimingReadonly",
        description: $"WorkOrder '{id}': Can't Modify Timing When WorkOrder Status Is '{state}'");

    public static Error LaborIdEmpty(string id) => Error.Validation(
        code: "WorkOrder.LaborId.Empty",
        description: $"WorkOrder '{id}': Labor Id is empty");

    public static Error StateTransitionNotAllowed(DateTimeOffset startAtUtc) => Error.Conflict(
        code: "WorkOrder.StateTransition.NotAllowed",
        description: $"State Transition Is Not Allowed Before The Work Order’s Scheduled Start Time `{startAtUtc:yyyy-MM-dd HH:mm}` UTC.");

    public static Error InvalidStateTransition(WorkOrderState current, WorkOrderState next) => Error.Conflict(
        code: "WorkOrder.InvalidStateTransition",
        description: $"WorkOrder Invalid State Transition From '{current}' To '{next}'");

    public static Error RepairTaskAlreadyAdded => Error.Conflict(
        code: "WorkOrder.RepairTask.AlreadyAdded",
        description: "Repair Task Already Exists");

    public static Error InvalidStateTransitionTime => Error.Conflict(
        code: "WorkOrder.InvalidStateTransitionTime",
        description: "State transition is not allowed before the work order’s scheduled start time.");
}