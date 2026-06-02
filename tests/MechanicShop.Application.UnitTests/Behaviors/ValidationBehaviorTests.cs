using FluentValidation;
using FluentValidation.Results;
using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.DTOs;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Tests.Common.VehicleGenerator;
using MediatR;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
  private readonly IValidator<CreateVehicleCommand> _validator;
  private readonly ValidationBehavior<CreateVehicleCommand , Result<VehicleDto>> _sub;
  private readonly RequestHandlerDelegate<Result<VehicleDto>> _next;

  public ValidationBehaviorTests()
  {
    _validator = Substitute.For<IValidator<CreateVehicleCommand>>();
    
    _next = Substitute.For<RequestHandlerDelegate<Result<VehicleDto>>>();
    
    _sub = new ValidationBehavior<CreateVehicleCommand , Result<VehicleDto>>(_validator);
  }

  [Fact]
  public async Task Handle_ValidationIsValid_ReturnTheNextFunction()
  {
    var request = VehicleFactory.CreateCommand();
    var response = VehicleFactory.Create().Value.ToDto();
    
    _next.Invoke().Returns(response);

    _validator.ValidateAsync(
      request,
      CancellationToken.None
    )
    .Returns(new ValidationResult());


    var result = await _sub.Handle(
      request , 
      _next, 
      CancellationToken.None
    );
  
    Assert.True(result.IsSuccess);
    Assert.Equal(response , result.Value);
  }

  [Fact]
  public async Task Handle_ValidationIsNotValid_ReturnListOfErrors()
  {
    var request = VehicleFactory.CreateCommand();
    var response = VehicleFactory.Create().Value.ToDto();
    
    IEnumerable<ValidationFailure> failures = [
      new ValidationFailure("Model" , "failure"),
      new ValidationFailure("Make" , "failure"),
    ];
    
    _next.Invoke().Returns(response);

    _validator.ValidateAsync(
      request,
      CancellationToken.None
    )
    .Returns(new ValidationResult(failures));


    var result = await _sub.Handle(
      request , 
      _next, 
      CancellationToken.None
    );
  
    Assert.False(result.IsSuccess);
    Assert.Equal(failures.First().PropertyName , result.TopError.Code);
    Assert.Equal(failures.First().ErrorMessage , result.TopError.Description);
  }

  [Fact]
  public async Task Handle_ValidatorIsNull_ReturnTheResultOfNextFunction()
  {
    var sub = new ValidationBehavior<CreateVehicleCommand, Result<VehicleDto>>();
    var request = VehicleFactory.CreateCommand();
    var response = VehicleFactory.Create().Value.ToDto();

    _next.Invoke().Returns(response);

    var result = await sub.Handle(request, _next, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(response , result.Value);
    await _next.Received(1).Invoke(Arg.Any<CancellationToken>());
    await _validator.DidNotReceive()
      .ValidateAsync(
        Arg.Any<CreateVehicleCommand>() , 
        Arg.Any<CancellationToken>()
      );
  }
}