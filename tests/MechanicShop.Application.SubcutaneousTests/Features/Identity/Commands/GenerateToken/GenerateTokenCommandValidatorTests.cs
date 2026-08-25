using FluentValidation.TestHelper;
using MechanicShop.Application.Features.Identity.Commands.GenerateToken;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Commands.GenerateToken;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GenerateTokenCommandValidatorTests
{
    private readonly GenerateTokenCommandValidator _validator;

    public GenerateTokenCommandValidatorTests()
    {
        _validator = new GenerateTokenCommandValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_WhenEmailIsNullOrEmpty_ShouldHaveValidationError(string? email)
    {
        // Given
        var command = new GenerateTokenCommand(email!, "SomePassword123!");

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("invalid-email@")]
    [InlineData("invalid-email.com")]
    public void Constructor_WhenEmailFormatIsInvalid_ShouldHaveValidationError(string email)
    {
        // Given
        var command = new GenerateTokenCommand(email, "SomePassword123!");

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Email);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenEmailIsValid_ShouldNotHaveValidationErrorForEmail()
    {
        // Given
        var command = new GenerateTokenCommand("johndoe@example.com", "SomePassword123!");

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_WhenPasswordIsNullOrEmpty_ShouldHaveValidationError(string? password)
    {
        // Given
        var command = new GenerateTokenCommand("johndoe@example.com", password!);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Password);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenPasswordIsProvided_ShouldNotHaveValidationErrorForPassword()
    {
        // Given
        var command = new GenerateTokenCommand("johndoe@example.com", "SomePassword123!");

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var command = new GenerateTokenCommand("johndoe@example.com", "SomePassword123!");

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}