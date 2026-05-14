using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.Features.Customers.DTOs;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.Features.Customers.Queries.GetCustomers;
using MechanicShop.Contracts.Requests.Customers;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Authorize]
[Route("api/customers")]
[ApiVersion("1.0")]
public sealed class CustomerController(ISender sender) : ApiController
{ 
  [HttpGet]
  [ProducesResponseType(typeof(List<CustomerDto>) , StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails) , StatusCodes.Status500InternalServerError)]
  [EndpointName("GetAllCustomers")]
  [EndpointSummary("Get All Customers Available In The System.")]
  [EndpointDescription("Returns All Customers Associated With The Current User.")]
  [Tags("customers")]
  [OutputCache(Duration = 60)]
  public async Task<IActionResult> GetAll(CancellationToken ct)
  {
    var result = await sender.Send(new GetCustomersQuery() , ct);

    return result.Match(
      Ok,
      ProblemDetailsHandler
    );
  }

  [HttpGet("{customerId:guid}" , Name = "GetCustomerById")]
  [ProducesResponseType(typeof(List<CustomerDto>) , StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(List<CustomerDto>) , StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails) , StatusCodes.Status500InternalServerError)]
  [EndpointName("GetCustomerById")]
  [EndpointSummary("Get A Customer Using Requested Id.")]
  [EndpointDescription("Returns Detailed Information About The Specified Customer If Found.")]
  [Tags("customers")]
  [OutputCache(Duration = 60)]
  public async Task<IActionResult> GetById(Guid customerId , CancellationToken ct)
  {
    var result = await sender.Send(new GetCustomerByIdQuery(customerId) , ct);

    return result.Match(
      Ok,
      ProblemDetailsHandler
    );
  }
  
  [HttpPost]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(typeof(CustomerDto) , StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails) , StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails) , StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ProblemDetails) , StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
  [EndpointName("CreateCustomer")]
  [EndpointSummary("Creates A New Customer")]
  [EndpointDescription("Adds A New Customer To The System.")]
  [ProducesDefaultResponseType(typeof(ProblemDetails))]
  [Tags("customers")]
  public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request , CancellationToken ct)
  {
    var vehicles = request.Vehicles
      .ConvertAll(
        vehicle => 
          new CreateVehicleCommand(
            vehicle.Make , 
            vehicle.Model , 
            vehicle.LicensePlate , 
            vehicle.Year
          )
        );
    
    var result = await sender.Send(
      new CreateCustomerCommand(
        request.Name,
        request.PhoneNumber,
        request.Email,
        vehicles
      ),
      ct
    );

    return result.Match(
        response => CreatedAtRoute(
          routeName: "GetCustomerById",
          routeValues: new
          {
              version = HttpContext.GetRequestedApiVersion()?.ToString(),
              customerId = response.CustomerId
          },
          value: response
        ),
        ProblemDetailsHandler
      );
  }

  [HttpPut("{customerId:guid}")]
  [Authorize(Roles = nameof(Role.Manager))]
  [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails) , StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails) , StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ProblemDetails) , StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
  [EndpointSummary("Updates An Existing customer.")]
  [EndpointDescription("Updates A Customer And Its Associated Vehicle.")]
  [EndpointName("UpdateCustomer")]
  [Tags("customers")]
  public async Task<IActionResult> Update(Guid customerId , [FromBody] UpdateCustomerRequest request , CancellationToken ct)
  {
    var vehicles  = request.Vehicles
      .ConvertAll(
        vehicle => 
          new UpdateVehicleCommand(
            vehicle.VehicLeId , 
            vehicle.Make , 
            vehicle.Model , 
            vehicle.LicensePlate , 
            vehicle.Year
          )
      );
    
    var result = await sender.Send(
      new UpdateCustomerCommand(
        customerId,
        request.Name,
        request.PhoneNumber,
        request.Email,
        vehicles
      ),
      ct
    );

    return result.Match(
      response => Ok(response),
      ProblemDetailsHandler
    );
  }

  [HttpDelete]
  [Authorize(Roles = nameof(Role.Manager))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointSummary("Removes A Customer From The System.")]
  [EndpointDescription("Deletes The Specified Customer From The System.")]
  [EndpointName("RemoveCustomer")]
  [Tags("customers")]

  public async Task<IActionResult> Delete(Guid customerId , CancellationToken ct)
  {
    var result = await sender.Send(new RemoveCustomerCommand(customerId) , ct);

    return result.Match(
      _ => NoContent(),
      ProblemDetailsHandler
    );
  }
}