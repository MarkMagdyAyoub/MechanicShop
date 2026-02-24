using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.ReassignLabor;

public sealed record ReassignLaborCommand(
  Guid WorkOrderId , 
  Guid LaborId
) : IRequest<Result<Updated>>;