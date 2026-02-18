using MechanicShop.Application.Features.Customers.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery : IRequest<Result<List<CustomerDto>>>;