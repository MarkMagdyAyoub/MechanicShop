using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace MechanicShop.Infrastructure.Data;


public sealed class AppDbContext(IMediator mediator) : DbContext ,IAppDbContext 
{
  private readonly IMediator _mediator = mediator;

  public DbSet<Customer> Customers => Set<Customer>();

  public DbSet<Part> Parts => Set<Part>();

  public DbSet<RepairTask> RepairTasks => Set<RepairTask>();

  public DbSet<Vehicle> Vehicles => Set<Vehicle>();

  public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

  public DbSet<Employee> Employees => Set<Employee>();

  public DbSet<Invoice> Invoices => Set<Invoice>();

  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
  {
    await DispatchDomainEventAsync(cancellationToken);
    return await base.SaveChangesAsync(cancellationToken); 
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
  }

  public async Task DispatchDomainEventAsync(CancellationToken cancellationToken)
  {
    var domainEntities = ChangeTracker.Entries()
                                      .Where(e => e.Entity is Entity baseEntity && baseEntity.DomainEvents.Count != 0)
                                      .Select(e => (Entity)e.Entity)
                                      .ToList();
    
    var domainEvents = domainEntities.SelectMany(de => de.DomainEvents);

    foreach (var domainEvent in domainEvents)
    {
      await _mediator.Publish(domainEvent , cancellationToken);
    }

    foreach(var domainEvent in domainEntities)
    {
      domainEvent.ClearDomainEvents();
    }
  }
}