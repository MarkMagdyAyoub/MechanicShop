using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Tests.Common.PartGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;

namespace MechanicShop.Domain.UnitTests.RepairTask;

public class RepairTaskTests
{
  [Fact]
  public void Create_ValidData_ReturnRepairTaskInstance()
  {
    var result = RepairTaskFactory.Create();

    Assert.True(result.IsSuccess);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_NameIsNullOrWhiteSpace_ReturnNameRequiredError(string name)
  {
    var result = RepairTaskFactory.Create(name: name);

    Assert.False(result.IsSuccess);
    Assert.Equal(RepairTaskErrors.NameRequired.Code, result.TopError.Code);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(0)]
  public void Create_LaborCostIsInvalid_ReturnLaborCostInvalidError(decimal laborCost)
  {
    var result = RepairTaskFactory.Create(laborCost: laborCost);

    Assert.False(result.IsSuccess);
    Assert.Equal(RepairTaskErrors.LaborCostInvalid.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_DurationIsInvalid_ReturnDurationInvalidError()
  {
    var result = RepairTaskFactory.Create(estimatedDurationInMins: (RepairDurationInMinutes)9999);

    Assert.False(result.IsSuccess);
    Assert.Equal(RepairTaskErrors.DurationInvalid.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_ValidData_NameIsTrimmed()
  {
    var result = RepairTaskFactory.Create(name: "  Oil Change  ");

    Assert.True(result.IsSuccess);
    Assert.Equal("Oil Change", result.Value.Name);
  }

  [Fact]
  public void Create_ValidData_TotalCostEqualsLaborCostPlusPartsCost()
  {
    var part = PartFactory.Create(cost: 30m , quantity: 1).Value;
    var result = RepairTaskFactory.Create(laborCost: 50.00m, parts: [part]);

    Assert.True(result.IsSuccess);
    Assert.Equal(80.00m, result.Value.TotalCost);
  }

  [Fact]
  public void Update_ValidData_UpdateRepairTaskInstance()
  {
    var repairTask = RepairTaskFactory.Create().Value;

    var result = repairTask.Update("Brake Replacement", 120.00m, RepairDurationInMinutes._120);

    Assert.True(result.IsSuccess);
    Assert.Equal("Brake Replacement", repairTask.Name);
    Assert.Equal(120.00m, repairTask.LaborCost);
    Assert.Equal(RepairDurationInMinutes._120, repairTask.EstimatedDurationInMins);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Update_NameIsNullOrWhiteSpace_ReturnNameRequiredError(string name)
  {
    var repairTask = RepairTaskFactory.Create().Value;

    var result = repairTask.Update(name, repairTask.LaborCost, repairTask.EstimatedDurationInMins);

    Assert.False(result.IsSuccess);
    Assert.Equal(RepairTaskErrors.NameRequired.Code, result.TopError.Code);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(0)]
  [InlineData(10001)]
  public void Update_LaborCostIsInvalid_ReturnLaborCostInvalidError(decimal laborCost)
  {
    var repairTask = RepairTaskFactory.Create().Value;

    var result = repairTask.Update(repairTask.Name, laborCost, repairTask.EstimatedDurationInMins);

    Assert.False(result.IsSuccess);
    Assert.Equal(RepairTaskErrors.LaborCostInvalid.Code, result.TopError.Code);
  }

  [Fact]
  public void Update_DurationIsInvalid_ReturnDurationInvalidError()
  {
    var repairTask = RepairTaskFactory.Create().Value;

    var result = repairTask.Update(repairTask.Name, repairTask.LaborCost, (RepairDurationInMinutes)9999);

    Assert.False(result.IsSuccess);
    Assert.Equal(RepairTaskErrors.DurationInvalid.Code, result.TopError.Code);
  }

  [Fact]
  public void Update_ValidData_NameIsTrimmed()
  {
    var repairTask = RepairTaskFactory.Create().Value;

    var result = repairTask.Update("  Brake Replacement  ", 120.00m, RepairDurationInMinutes._120);

    Assert.True(result.IsSuccess);
    Assert.Equal("Brake Replacement", repairTask.Name);
  }

  [Fact]
  public void Update_ValidData_DoesNotMutateParts()
  {
    var part = PartFactory.Create(Guid.NewGuid(), "Engine Oil", 25.00m, 1).Value;
    var repairTask = RepairTaskFactory.Create(parts: [part]).Value;

    repairTask.Update("Brake Replacement", 120.00m, RepairDurationInMinutes._120);

    Assert.Single(repairTask.Parts);
  }

  [Fact]
  public void UpsertParts_NewPart_AddsPartToRepairTask()
  {
    var repairTask = RepairTaskFactory.Create().Value;
    var initialCount = repairTask.Parts.Count();
    var newPart = PartFactory.Create(Guid.NewGuid(), "Brake Pads", 45.00m, 2).Value;

    var result = repairTask.UpsertParts([..repairTask.Parts , newPart]);

    Assert.True(result.IsSuccess);
    Assert.Equal(initialCount + 1, repairTask.Parts.Count());
    Assert.Contains(repairTask.Parts, p => p.Id == newPart.Id);
  }

  [Fact]
  public void UpsertParts_ExistingPartId_DoesNotDuplicatePart()
  {
    var existingPart = PartFactory.Create(Guid.NewGuid(), "Engine Oil", 25.00m, 1).Value;
    var repairTask = RepairTaskFactory.Create(parts: [existingPart]).Value;

    var incomingWithSameId = PartFactory.Create(existingPart.Id, "Engine Oil Synthetic", 35.00m, 2).Value;
    var result = repairTask.UpsertParts([incomingWithSameId]);

    Assert.True(result.IsSuccess);
    Assert.Single(repairTask.Parts);
  }

  [Fact]
  public void UpsertParts_PartAbsentFromIncomingList_RemovesPart()
  {
    var partToKeep = PartFactory.Create(Guid.NewGuid(), "Engine Oil", 25.00m, 1).Value;
    var partToRemove = PartFactory.Create(Guid.NewGuid(), "Brake Pads", 45.00m, 2).Value;
    var repairTask = RepairTaskFactory.Create(parts: [partToKeep, partToRemove]).Value;

    var result = repairTask.UpsertParts([partToKeep]);

    Assert.True(result.IsSuccess);
    Assert.Single(repairTask.Parts);
    Assert.DoesNotContain(repairTask.Parts, p => p.Id == partToRemove.Id);
  }

  [Fact]
  public void UpsertParts_EmptyList_RemovesAllParts()
  {
    var existingPart = PartFactory.Create(Guid.NewGuid(), "Engine Oil", 25.00m, 1).Value;
    var repairTask = RepairTaskFactory.Create(parts: [existingPart]).Value;

    var result = repairTask.UpsertParts([]);

    Assert.True(result.IsSuccess);
    Assert.Empty(repairTask.Parts);
  }

  [Fact]
  public void UpsertParts_ValidData_TotalCostRecalculatesCorrectly()
  {
    var initialPart = PartFactory.Create(Guid.NewGuid(), "Engine Oil", 25.00m, 1).Value;
    var repairTask = RepairTaskFactory.Create(laborCost: 50.00m, parts: [initialPart]).Value;

    var newPart = PartFactory.Create(Guid.NewGuid(), "Brake Pads", 100.00m, 1).Value;
    repairTask.UpsertParts([newPart]);

    Assert.Equal(150.00m, repairTask.TotalCost);
  }

  [Fact]
  public void UpsertParts_ValidData_CalculateNewAndOldPartsCost()
  {
    var initialPart = PartFactory.Create(Guid.NewGuid(), "Engine Oil", 25.00m, 1).Value;
    var repairTask = RepairTaskFactory.Create(laborCost: 50.00m, parts: [initialPart]).Value;

    var newPart = PartFactory.Create(Guid.NewGuid(), "Brake Pads", 100.00m, 1).Value;
    repairTask.UpsertParts([initialPart , newPart]);

    Assert.Equal(175.00m, repairTask.TotalCost);
  }
}