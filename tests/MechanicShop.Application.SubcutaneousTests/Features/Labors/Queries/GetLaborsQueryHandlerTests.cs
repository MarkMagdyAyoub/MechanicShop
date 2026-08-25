using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labor.Queries;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.EmployeeGenerator;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Queries.GetLabors;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetLaborsQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
  private readonly IAppDbContext _context = factory.CreateDbContext();
  private readonly ISender _sender = factory.CreateSender();

  [Fact]
  public async Task Handler_WhenLaborsExist_ShouldReturnListOfLaborsDto()
  {
    // Given
    var employee = EmployeeFactory.Create().Value;
    _context.Employees.Add(employee);
    await _context.SaveChangesAsync(CancellationToken.None);
  
    // When
    var query = new GetLaborsQuery();
    var result = await _sender.Send(query);
  
    // Then
    Assert.True(result.IsSuccess);
    Assert.Contains(result.Value , l => l.Id == employee.Id);
  }

  [Fact]
  public async Task Handler_WhenLaborsNotExist_ShouldReturnEmptyList()
  {
    // When
    var query = new GetLaborsQuery();
    var result = await _sender.Send(query);
  
    // Then
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value);
  }

  public Task DisposeAsync() => factory.ResetDatabaseAsync();

  public Task InitializeAsync() => factory.ResetDatabaseAsync();
}