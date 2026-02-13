using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks;

public static class RepairTaskErrors
{
    public static Error NameRequired =>
        Error.Validation("RepairTask.Name.Required", "Name Is Required");

    public static Error LaborCostInvalid =>
        Error.Validation("RepairTask.LaborCost.Invalid", "Labor Cost Must Be Between 1 And 10,000");

    public static Error DurationInvalid =>
        Error.Validation("RepairTask.Duration.Invalid", "Invalid Duration Selected");

    public static Error PartsRequired =>
        Error.Validation("RepairTask.Parts.Required", "At Least One Part Is Required");

    public static Error PartNameRequired =>
        Error.Validation("RepairTask.Parts.Name.Required", "All Parts Must Have A Name");

    public static Error AtLeastOneRepairTaskIsRequired =>
        Error.Validation("RepairTask.Required", "At Least One Repair Task Must Be Specified");

    public static Error InUse =>
        Error.Conflict("RepairTask.InUse", "Cannot Delete A Repair Task That Is Used In Work Orders");

    public static Error DuplicateName =>
        Error.Conflict("RepairTaskPart.Duplicate", "A Part With The Same Name Already Exists In This Repair Task");
}