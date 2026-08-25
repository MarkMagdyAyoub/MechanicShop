using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.UpdateCustomer;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class UpdateVehicleCommandValidatorTests
{
    private readonly UpdateVehicleCommandValidator _validator;

    public UpdateVehicleCommandValidatorTests()
    {
        _validator = new UpdateVehicleCommandValidator();
    }

    [Fact]
    public void Constructor_WhenMakeIsEmpty_ReturnVehicleError()
    {
        var command = new UpdateVehicleCommand(
            VehicleId: Guid.NewGuid() ,
            Make: "" ,
            Model: "Model 5" ,
            LicensePlate: "ABC123" ,
            Year: 2020
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors , e => e.PropertyName == nameof(UpdateVehicleCommand.Make));
    }

    [Fact]
    public void Constructor_WhenModelIsEmpty_ReturnVehicleError()
    {
        var command = new UpdateVehicleCommand(
            VehicleId: Guid.NewGuid() ,
            Make: "Tesla" ,
            Model: "" ,
            LicensePlate: "ABC123" ,
            Year: 2020
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors , e => e.PropertyName == nameof(UpdateVehicleCommand.Model));
    }

    [Fact]
    public void Constructor_WhenLicensePlateIsEmpty_ReturnVehicleError()
    {
        var command = new UpdateVehicleCommand(
            VehicleId: Guid.NewGuid() ,
            Make: "Tesla" ,
            Model: "Model 5" ,
            LicensePlate: "" ,
            Year: 2020
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors , e => e.PropertyName == nameof(UpdateVehicleCommand.LicensePlate));
    }
}
