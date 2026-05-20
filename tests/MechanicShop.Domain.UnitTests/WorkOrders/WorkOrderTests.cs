using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Tests.Common.WorkOrderGenerator;

namespace MechanicShop.Domain.UnitTests.WorkOrders;

public class WorkOrderTests
{
  [Fact]
  public void Create_ValidData_ReturnsWorkOrder()
  {
    var result = WorkOrderFactory.Create();

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Create_IdIsEmptyGuid_ReturnsWorkOrderIdRequiredError()
  {
    var result = WorkOrderFactory.Create(id: Guid.Empty);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_VehicleIdIsEmptyGuid_ReturnsVehicleIdRequiredError()
  {
    var result = WorkOrderFactory.Create(vehicleId: Guid.Empty);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.VehicleIdRequired.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_RepairTasksIsEmpty_ReturnsRepairTasksRequiredError()
  {
    var result = WorkOrderFactory.Create(repairTasks: []);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.RepairTasksRequired.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_LaborIdIsEmptyGuid_ReturnsLaborIdRequiredError()
  {
    var result = WorkOrderFactory.Create(laborId: Guid.Empty);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.LaborIdRequired.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_EndAtIsBeforeStartAt_ReturnsInvalidTimingError()
  {
    var start = DateTimeOffset.UtcNow;
    var end   = start.AddMinutes(-30);

    var result = WorkOrderFactory.Create(startAt: start, endAt: end);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_EndAtEqualsStartAt_ReturnsInvalidTimingError()
  {
    var at = DateTimeOffset.UtcNow;

    var result = WorkOrderFactory.Create(startAt: at, endAt: at);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_SpotIsInvalid_ReturnsSpotInvalidError()
  {
    var result = WorkOrderFactory.Create(spot: (Spot)99);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.SpotInvalid.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_ValidData_SetsStateToScheduled()
  {
    var workOrder = WorkOrderFactory.Create().Value;

    Assert.Equal(WorkOrderState.Scheduled, workOrder.State);
  }


  [Fact]
  public void AddRepairTask_ValidTask_ReturnsUpdated()
  {
    var workOrder  = WorkOrderFactory.Create().Value;
    var repairTask = RepairTaskFactory.Create().Value;

    var result = workOrder.AddRepairTask(repairTask);

    Assert.True(result.IsSuccess);
    Assert.Contains(workOrder.RepairTasks, rt => rt.Id == repairTask.Id);
  }

  [Fact]
  public void AddRepairTask_DuplicateTask_ReturnsRepairTaskAlreadyAddedError()
  {
    var repairTask = RepairTaskFactory.Create().Value;
    var workOrder  = WorkOrderFactory.Create(repairTasks: [repairTask]).Value;

    var result = workOrder.AddRepairTask(repairTask);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.RepairTaskAlreadyAdded.Code, result.TopError.Code);
  }

  [Fact]
  public void AddRepairTask_WorkOrderNotEditable_ReturnsReadonlyError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);

    var result = workOrder.AddRepairTask(RepairTaskFactory.Create().Value);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
  }


  [Fact]
  public void UpdateTiming_ValidTiming_ReturnsUpdated()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    var newStart  = DateTimeOffset.UtcNow.AddHours(1);
    var newEnd    = newStart.AddHours(2);

    var result = workOrder.UpdateTiming(newStart, newEnd);

    Assert.True(result.IsSuccess);
    Assert.Equal(newStart, workOrder.StartAtUtc);
    Assert.Equal(newEnd,   workOrder.EndAtUtc);
  }

  [Fact]
  public void UpdateTiming_EndAtBeforeStartAt_ReturnsInvalidTimingError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    var start     = DateTimeOffset.UtcNow.AddHours(2);
    var end       = start.AddMinutes(-30);

    var result = workOrder.UpdateTiming(start, end);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateTiming_EndAtEqualsStartAt_ReturnsInvalidTimingError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    var at        = DateTimeOffset.UtcNow.AddHours(1);

    var result = workOrder.UpdateTiming(at, at);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateTiming_WorkOrderNotEditable_ReturnsTimingReadonlyError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);

    var result = workOrder.UpdateTiming(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.TimingReadonly(workOrder.Id.ToString(), workOrder.State).Code, result.TopError.Code);
  }


  [Fact]
  public void UpdateLabor_ValidLaborId_ReturnsUpdated()
  {
    var workOrder   = WorkOrderFactory.Create().Value;
    var newLaborId  = Guid.NewGuid();

    var result = workOrder.UpdateLabor(newLaborId);

    Assert.True(result.IsSuccess);
    Assert.Equal(newLaborId, workOrder.LaborId);
  }

  [Fact]
  public void UpdateLabor_EmptyGuid_ReturnsLaborIdEmptyError()
  {
    var workOrder = WorkOrderFactory.Create().Value;

    var result = workOrder.UpdateLabor(Guid.Empty);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.LaborIdEmpty(Guid.Empty.ToString()).Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateLabor_WorkOrderNotEditable_ReturnsTimingReadonlyError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);

    var result = workOrder.UpdateLabor(Guid.NewGuid());

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.TimingReadonly(workOrder.Id.ToString(), workOrder.State).Code, result.TopError.Code);
  }


  [Fact]
  public void UpdateState_ScheduledToInProgress_ReturnsUpdated()
  {
    var workOrder = WorkOrderFactory.Create().Value;

    var result = workOrder.UpdateState(WorkOrderState.InProgress);

    Assert.True(result.IsSuccess);
    Assert.Equal(WorkOrderState.InProgress, workOrder.State);
  }

  [Fact]
  public void UpdateState_InProgressToCompleted_ReturnsUpdated()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);

    var result = workOrder.UpdateState(WorkOrderState.Completed);

    Assert.True(result.IsSuccess);
    Assert.Equal(WorkOrderState.Completed, workOrder.State);
  }

  [Fact]
  public void UpdateState_ScheduledToCancelled_ReturnsUpdated()
  {
    var workOrder = WorkOrderFactory.Create().Value;

    var result = workOrder.UpdateState(WorkOrderState.Cancelled);

    Assert.True(result.IsSuccess);
    Assert.Equal(WorkOrderState.Cancelled, workOrder.State);
  }

  [Fact]
  public void UpdateState_InProgressToCancelled_ReturnsUpdated()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);

    var result = workOrder.UpdateState(WorkOrderState.Cancelled);

    Assert.True(result.IsSuccess);
    Assert.Equal(WorkOrderState.Cancelled, workOrder.State);
  }

  [Fact]
  public void UpdateState_CompletedToCancelled_ReturnsInvalidStateTransitionError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);
    workOrder.UpdateState(WorkOrderState.Completed);

    var result = workOrder.UpdateState(WorkOrderState.Cancelled);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.InvalidStateTransition(WorkOrderState.Completed, WorkOrderState.Cancelled).Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateState_ScheduledToCompleted_ReturnsInvalidStateTransitionError()
  {
    var workOrder = WorkOrderFactory.Create().Value;

    var result = workOrder.UpdateState(WorkOrderState.Completed);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.InvalidStateTransition(WorkOrderState.Scheduled, WorkOrderState.Completed).Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateState_CancelledToAnyState_ReturnsInvalidStateTransitionError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.Cancelled);

    var result = workOrder.UpdateState(WorkOrderState.InProgress);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.InvalidStateTransition(WorkOrderState.Cancelled, WorkOrderState.InProgress).Code, result.TopError.Code);
  }


  [Fact]
  public void Cancel_ScheduledWorkOrder_ReturnsUpdated()
  {
    var workOrder = WorkOrderFactory.Create().Value;

    var result = workOrder.Cancel();

    Assert.True(result.IsSuccess);
    Assert.Equal(WorkOrderState.Cancelled, workOrder.State);
  }

  [Fact]
  public void Cancel_InProgressWorkOrder_ReturnsUpdated()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);

    var result = workOrder.Cancel();

    Assert.True(result.IsSuccess);
    Assert.Equal(WorkOrderState.Cancelled, workOrder.State);
  }

  [Fact]
  public void Cancel_CompletedWorkOrder_ReturnsInvalidStateTransitionError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);
    workOrder.UpdateState(WorkOrderState.Completed);

    var result = workOrder.Cancel();

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.InvalidStateTransition(WorkOrderState.Completed, WorkOrderState.Cancelled).Code, result.TopError.Code);
  }

  [Fact]
  public void Cancel_AlreadyCancelledWorkOrder_ReturnsInvalidStateTransitionError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.Cancel();

    var result = workOrder.Cancel();

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void ClearRepairTasks_EditableWorkOrder_ReturnsUpdatedAndEmptiesTasks()
  {
    var workOrder = WorkOrderFactory.Create().Value;

    var result = workOrder.ClearRepairTasks();

    Assert.True(result.IsSuccess);
    Assert.Empty(workOrder.RepairTasks);
  }

  [Fact]
  public void ClearRepairTasks_WorkOrderNotEditable_ReturnsReadonlyError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);

    var result = workOrder.ClearRepairTasks();

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateSpot_ValidSpot_ReturnsUpdated()
  {
    var workOrder = WorkOrderFactory.Create(spot: Spot.A).Value;

    var result = workOrder.UpdateSpot(Spot.C);

    Assert.True(result.IsSuccess);
    Assert.Equal(Spot.C, workOrder.Spot);
  }

  [Fact]
  public void UpdateSpot_InvalidSpot_ReturnsSpotInvalidError()
  {
    var workOrder = WorkOrderFactory.Create().Value;

    var result = workOrder.UpdateSpot((Spot)99);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.SpotInvalid.Code, result.TopError.Code);
  }

  [Fact]
  public void UpdateSpot_WorkOrderNotEditable_ReturnsReadonlyError()
  {
    var workOrder = WorkOrderFactory.Create().Value;
    workOrder.UpdateState(WorkOrderState.InProgress);

    var result = workOrder.UpdateSpot(Spot.B);

    Assert.False(result.IsSuccess);
    Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
  }
}