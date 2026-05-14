using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.Data;

public class DbContextInitializer(
  ILogger<DbContextInitializer> logger,
  AppDbContext context,
  UserManager<ApplicationUser> userManager,
  RoleManager<IdentityRole<Guid>> roleManager
)
{
  private readonly ILogger<DbContextInitializer> _logger = logger;
  private readonly AppDbContext _context = context;
  private readonly UserManager<ApplicationUser> _userManager = userManager;
  private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;

  public async Task InitializeAsync()
  {
    try
    {
      await _context.Database.EnsureCreatedAsync();
    }
    catch(Exception ex)
    {
      _logger.LogError(ex , $"An Error Occurred While Initializing The Database");
      throw;
    }
  }

  public async Task SeedAsync()
  {
      try
      {
          await TrySeedAsync();
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "An error occurred while seeding the database.");
          throw;
      }
  }

  private async Task TrySeedAsync()
  {
    // roles
    var managerRole = new IdentityRole<Guid>(nameof(Role.Manager));
    var laborRole = new IdentityRole<Guid>(nameof(Role.Labor));

    if(_roleManager.Roles.All(role => role.Name != managerRole.Name))
    {
      await _roleManager.CreateAsync(managerRole);
    }

    if(_roleManager.Roles.All(role => role.Name != laborRole.Name))
    {
      await _roleManager.CreateAsync(laborRole);
    }

    // users
    var managers = new List<ApplicationUser>
    {
      new ApplicationUser
      {
          Id = Guid.Parse("8d7f2f44-8c91-4f7f-a5f2-1d9c3b8a4e11"),
          Email = "mark@gmail.com",
          UserName = "MarkMagdy",
          EmailConfirmed = true
      },
      new ApplicationUser
      {
          Id = Guid.Parse("1b2e9d63-6f84-4db6-b4a5-7e3c1a92f220"),
          Email = "john@gmail.com",
          UserName = "John",
          EmailConfirmed = true
      }
    };

    var labors = new List<ApplicationUser>
    {
      new ApplicationUser
      {
          Id = Guid.Parse("c4a8f9d1-3e52-47d0-9d61-2f7b6c5a8d33"),
          Email = "alice@gmail.com",
          UserName = "Alice",
          EmailConfirmed = true
      },
      new ApplicationUser
      {
          Id = Guid.Parse("5f1c7b82-9a65-4ed4-8c27-4a9d1e6b7f44"),
          Email = "bob@gmail.com",
          UserName = "Bob",
          EmailConfirmed = true
      },
      new ApplicationUser
      {
          Id = Guid.Parse("9e6d3a55-2b48-41f1-a3d8-8b2c7f9d5e55"),
          Email = "kevin@gmail.com",
          UserName = "Kevin",
          EmailConfirmed = true
      }
    };

    foreach(var manager in managers)
    {
      if(_userManager.Users.All(m => m.Email != manager.Email))
      {
        await _userManager.CreateAsync(manager , "Manager@1234!");
        if(!string.IsNullOrWhiteSpace(managerRole.Name))
          await _userManager.AddToRoleAsync(manager , managerRole.Name);
      }
    }

    foreach(var labor in labors)
    {
      if(_userManager.Users.All(l => l.Email != labor.Email))
      {
        await _userManager.CreateAsync(labor , "Labor@1234!");

        if(!string.IsNullOrWhiteSpace(laborRole.Name))
          await _userManager.AddToRoleAsync(labor , laborRole.Name);
      }
    }

    // employees
    if (!_context.Employees.Any())
    {
      _context.Employees.AddRange([
          Employee.Create(
              id: managers[0].Id,
              firstName: "Mark",
              lastName: "Magdy",
              fullName: "Mark Magdy",
              role: Role.Manager
          ).Value,

          Employee.Create(
              id: managers[1].Id,
              firstName: "John",
              lastName: "Doe",
              fullName: "John Doe",
              role: Role.Manager
          ).Value,

          Employee.Create(
              id: labors[0].Id,
              firstName: "Alice",
              lastName: "Johnson",
              fullName: "Alice Johnson",
              role: Role.Labor
          ).Value,

          Employee.Create(
              id: labors[1].Id,
              firstName: "Bob",
              lastName: "Smith",
              fullName: "Bob Smith",
              role: Role.Labor
          ).Value,

          Employee.Create(
              id: labors[2].Id,
              firstName: "Kevin",
              lastName: "Brown",
              fullName: "Kevin Brown",
              role: Role.Labor
          ).Value
      ]);
    }

    // customers
    if (!_context.Vehicles.Any())
    {
      List<Vehicle> customerOneVehicles = [
          Vehicle.Create(id: Guid.Parse("1d7f9b1e-2b7d-4d0f-8c8a-1f2b9c7a1001"), make: "Toyota", model: "Corolla", year: 2020, licensePlate: "ABC-123").Value,
          Vehicle.Create(id: Guid.Parse("2e8a0c2f-3c8e-4e1a-9d9b-2a3c0d8b2002"), make: "Honda", model: "Civic", year: 2021, licensePlate: "XYZ-456").Value,
      ];

      List<Vehicle> customerTwoVehicles = [
          Vehicle.Create(id: Guid.Parse("3f9b1d3a-4d9f-5f2b-a0ac-3b4d1e9c3003"), make: "BMW", model: "X5", year: 2022, licensePlate: "BMW-789").Value,
          Vehicle.Create(id: Guid.Parse("4a0c2e4b-5e0a-6a3c-b1bd-4c5e2f0d4004"), make: "Mercedes", model: "C200", year: 2023, licensePlate: "MER-101").Value,
      ];

      await _context.Customers.AddRangeAsync([
        Customer.Create(
            id: Guid.Parse("5b1d3f5c-6f1b-7b4d-c2ce-5d6f3a1e5005"),
            name: "Ahmed Hassan",
            phoneNumber: "01012345678",
            email: "ahmed@gmail.com",
            vehicles: customerOneVehicles
        ).Value,

        Customer.Create(
            id: Guid.Parse("6c2e4a6d-7a2c-8c5e-d3df-6e7a4b2f6006"),
            name: "Sara Ali",
            phoneNumber: "01198765432",
            email: "sara@gmail.com",
            vehicles: customerTwoVehicles
        ).Value
      ]);
    }

    // repair tasks
    if (!_context.RepairTasks.Any())
    {
      List<Part> oilChangeParts = [
          Part.Create(id: Guid.Parse("a1f3b2c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c"), name: "Engine Oil", cost: 25.00m, quantity: 1).Value,
          Part.Create(id: Guid.Parse("b2e4c6d8-e9f0-4a1b-9c2d-3e4f5a6b7c8d"), name: "Oil Filter", cost: 10.00m, quantity: 1).Value
      ];

      List<Part> brakeParts = [
          Part.Create(id: Guid.Parse("c3f5d7e9-f0a1-4b2c-ad3e-4f5a6b7c8d9e"), name: "Brake Pads", cost: 45.00m, quantity: 4).Value
      ];

      List<Part> batteryParts = [
          Part.Create(id: Guid.Parse("d4a6e8f0-a1b2-4c3d-be4f-5a6b7c8d9e0f"), name: "Car Battery", cost: 120.00m, quantity: 1).Value
      ];

      List<Part> tireParts = [
          Part.Create(id: Guid.Parse("e5b7f9a1-b2c3-4d4e-cf50-6b7c8d9e0f1a"), name: "Tire", cost: 80.00m, quantity: 2).Value
      ];

      List<Part> sparkPlugParts = [
          Part.Create(id: Guid.Parse("f6c8a0b2-c3d4-4e5f-d061-7c8d9e0f1a2b"), name: "Spark Plug", cost: 15.00m, quantity: 4).Value
      ];

      List<Part> airFilterParts = [
          Part.Create(id: Guid.Parse("a7d9b1c3-e4f5-4a6b-8c7d-9e0f1a2b3c4d"), name: "Air Filter", cost: 20.00m, quantity: 1).Value
      ];

      List<Part> wheelAlignmentParts = [
          Part.Create(id: Guid.Parse("b8e0c2d4-f5a6-4b7c-9d8e-0f1a2b3c4d5e"), name: "Alignment Shim", cost: 5.00m, quantity: 4).Value,
          Part.Create(id: Guid.Parse("c9f1d3e5-a6b7-4c8d-ae9f-1a2b3c4d5e6f"), name: "Tie Rod End", cost: 35.00m, quantity: 2).Value
      ];

      List<Part> transmissionParts = [
          Part.Create(id: Guid.Parse("d0a2e4f6-b7c8-4d9e-bf0a-2b3c4d5e6f7a"), name: "Transmission Fluid", cost: 30.00m, quantity: 2).Value,
          Part.Create(id: Guid.Parse("e1b3f5a7-c8d9-4e0f-c01b-3c4d5e6f7a8b"), name: "Transmission Filter", cost: 25.00m, quantity: 1).Value,
          Part.Create(id: Guid.Parse("f2c4a6b8-d9e0-4f1a-d12c-4d5e6f7a8b9c"), name: "Gasket Set", cost: 40.00m, quantity: 1).Value
      ];

      List<Part> radiatorParts = [
          Part.Create(id: Guid.Parse("a3d5b7c9-e0f1-4a2b-e23d-5e6f7a8b9c0d"), name: "Coolant", cost: 20.00m, quantity: 2).Value,
          Part.Create(id: Guid.Parse("b4e6c8d0-f1a2-4b3c-f34e-6f7a8b9c0d1e"), name: "Radiator Hose", cost: 30.00m, quantity: 2).Value,
          Part.Create(id: Guid.Parse("c5f7d9e1-a2b3-4c4d-a45f-7a8b9c0d1e2f"), name: "Thermostat", cost: 15.00m, quantity: 1).Value
      ];

      List<Part> acParts = [
          Part.Create(id: Guid.Parse("d6a8e0f2-b3c4-4d5e-b560-8b9c0d1e2f3a"), name: "Refrigerant", cost: 50.00m, quantity: 1).Value,
          Part.Create(id: Guid.Parse("e7b9f1a3-c4d5-4e6f-c671-9c0d1e2f3a4b"), name: "AC Filter", cost: 20.00m, quantity: 1).Value,
          Part.Create(id: Guid.Parse("f8c0a2b4-d5e6-4f7a-d782-0d1e2f3a4b5c"), name: "Compressor Oil", cost: 15.00m, quantity: 1).Value
      ];

      await _context.RepairTasks.AddRangeAsync([
        RepairTask.Create(id: Guid.Parse("1a2b3c4d-5e6f-4a7b-8c9d-0e1f2a3b4c5d"), name: "Oil Change", laborCost: 50.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._60, parts: oilChangeParts).Value,
        RepairTask.Create(id: Guid.Parse("2b3c4d5e-6f7a-4b8c-9d0e-1f2a3b4c5d6e"), name: "Brake Replacement", laborCost: 120.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._120, parts: brakeParts).Value,
        RepairTask.Create(id: Guid.Parse("3c4d5e6f-7a8b-4c9d-ae1f-2a3b4c5d6e7f"), name: "Battery Replacement", laborCost: 40.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._30, parts: batteryParts).Value,
        RepairTask.Create(id: Guid.Parse("4d5e6f7a-8b9c-4d0e-bf20-3b4c5d6e7f8a"), name: "Tire Replacement", laborCost: 70.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._90, parts: tireParts).Value,
        RepairTask.Create(id: Guid.Parse("5e6f7a8b-9c0d-4e1f-c031-4c5d6e7f8a9b"), name: "Spark Plug Replacement", laborCost: 55.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._60, parts: sparkPlugParts).Value,
        RepairTask.Create(id: Guid.Parse("6f7a8b9c-0d1e-4f2a-d142-5d6e7f8a9b0c"), name: "Air Filter Replacement", laborCost: 35.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._30, parts: airFilterParts).Value,
        RepairTask.Create(id: Guid.Parse("7a8b9c0d-1e2f-4a3b-e253-6e7f8a9b0c1d"), name: "Wheel Alignment", laborCost: 65.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._60, parts: wheelAlignmentParts).Value,
        RepairTask.Create(id: Guid.Parse("8b9c0d1e-2f3a-4b4c-f364-7f8a9b0c1d2e"), name: "Transmission Inspection", laborCost: 150.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._180, parts: transmissionParts).Value,
        RepairTask.Create(id: Guid.Parse("9c0d1e2f-3a4b-4c5d-a475-8a9b0c1d2e3f"), name: "Radiator Repair", laborCost: 130.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._120, parts: radiatorParts).Value,
        RepairTask.Create(id: Guid.Parse("0d1e2f3a-4b5c-4d6e-b586-9b0c1d2e3f4a"), name: "AC Maintenance", laborCost: 90.00m, estimatedDurationInMins: Domain.RepairTasks.Enums.RepairDurationInMinutes._90, parts: acParts).Value
      ]);
    }

    await _context.SaveChangesAsync(new CancellationToken());

    if (!_context.WorkOrders.Any())
    {
      var repairTasks = _context.RepairTasks.ToList();
      var vehicles = _context.Vehicles.ToList();
      Guid[] laborsIds = [labors[0].Id , labors[1].Id , labors[2].Id];
      Spot[] spots = [Spot.A , Spot.B , Spot.C , Spot.D];

      var generatedWorkOrders = new List<WorkOrder>();
      Random random = new();
      // start from tomorrow
      DateTimeOffset startDate = new DateTimeOffset(DateTimeOffset.UtcNow.UtcDateTime.Date.AddDays(1), TimeSpan.Zero);
      // generate work orders for next month
      DateTimeOffset endDate = startDate.AddMonths(1);
      TimeSpan openTime = TimeSpan.FromHours(12);
      TimeSpan closeTime = TimeSpan.FromHours(23);
      int totalMinutes = (int)(closeTime - openTime).TotalMinutes;

      while(startDate < endDate)
      {
        foreach(var spot in spots)
        {
          int occupiedMinutes = 0;
          int minOccupancy = (int)(totalMinutes * 0.6); 
          int maxOccupancy = (int)(totalMinutes * 0.8); 
          List<WorkOrder> spotWorkOrders = [];
          DateTimeOffset currentTime = startDate.Add(openTime);

          while (occupiedMinutes < minOccupancy && currentTime.TimeOfDay < closeTime)
          {
            var selectedTask = repairTasks
                              .DistinctBy(t => t.Id)
                              .OrderBy(_ => Guid.NewGuid())
                              .Take(Random.Shared.Next(1, Math.Min(4, repairTasks.Select(t => t.Id).Distinct().Count())))
                              .ToList();
            var laborId = labors[random.Next(labors.Count)].Id;
            var duration = selectedTask.Sum(rt => (int)rt.EstimatedDurationInMins);

            if (occupiedMinutes + duration > maxOccupancy)
            {
              break;
            }

            DateTimeOffset startAt = currentTime;
            DateTimeOffset endAt = startAt.AddMinutes(duration);

            var availableVehicle = vehicles
                                  .Where(v => !generatedWorkOrders.Any(w =>
                                      w.VehicleId == v.Id &&
                                      w.StartAtUtc.Date == startAt.Date &&
                                      w.StartAtUtc < endAt &&
                                      w.EndAtUtc > startAt))
                                  .OrderBy(_ => Guid.NewGuid())
                                  .FirstOrDefault();

            if (availableVehicle == null)
            {
                break;
            }

            if (endAt.TimeOfDay > closeTime)
            {
                break;
            }

            var workOrder = WorkOrder.Create(
                  Guid.NewGuid(),
                  availableVehicle.Id,
                  startAt,
                  endAt,
                  laborId,
                  spot,
                  selectedTask
                );

            spotWorkOrders.Add(workOrder.Value);
            occupiedMinutes += duration;

            currentTime = startDate.Add(openTime).AddMinutes(occupiedMinutes);
          }

          if (occupiedMinutes >= minOccupancy)
          {
              generatedWorkOrders.AddRange(spotWorkOrders);
          }
        }
        startDate = startDate.AddDays(1);
      }

      var repairTasksForFirstOrder = _context.RepairTasks
                                    .OrderBy(_ => Guid.NewGuid())
                                    .Take(2)
                                    .ToList();

      var utcNow = DateTimeOffset.UtcNow;

      var floored = new DateTimeOffset(
            utcNow.Year,
            utcNow.Month,
            utcNow.Day,
            utcNow.Hour,
            utcNow.Minute - (utcNow.Minute % 15),
            0,
            TimeSpan.Zero);

      var startTimeFirstOrder = floored;

      var workOrderStartingNow = WorkOrder.Create(
          Guid.NewGuid(),
          _context.Vehicles.OrderBy(_ => Guid.NewGuid()).First().Id,
          startTimeFirstOrder,
          startTimeFirstOrder.AddMinutes(repairTasksForFirstOrder.Sum(rt => (int)rt.EstimatedDurationInMins)),
          laborsIds[0],
          Spot.A,
          repairTasksForFirstOrder).Value;

      workOrderStartingNow.UpdateState(WorkOrderState.InProgress);

      var repairTasksEndingNow = _context.RepairTasks
      .First(rt => rt.EstimatedDurationInMins == Domain.RepairTasks.Enums.RepairDurationInMinutes._60);

      var startedAgo = utcNow.AddMinutes(-45);
      var roundedStart = new DateTimeOffset(
          startedAgo.Year,
          startedAgo.Month,
          startedAgo.Day,
          startedAgo.Hour,
          startedAgo.Minute - (startedAgo.Minute % 15),
          0,
          TimeSpan.Zero);

      var endTimeSecondOrder = roundedStart.AddMinutes((int)repairTasksEndingNow.EstimatedDurationInMins);

      WorkOrder value = WorkOrder.Create(
          Guid.NewGuid(),
          _context.Vehicles.OrderBy(_ => Guid.NewGuid()).First().Id,
          roundedStart,
          endTimeSecondOrder,
          laborsIds[1],
          Spot.B,
          [repairTasksEndingNow])
      .Value;
      var workOrderEndingNow = value;

      workOrderEndingNow.UpdateState(WorkOrderState.InProgress);

      generatedWorkOrders.AddRange(workOrderStartingNow, workOrderEndingNow);

      _context.WorkOrders.AddRange(generatedWorkOrders);

      await _context.SaveChangesAsync(new CancellationToken());
    }
  }
}

public static class DbInitializerExtension
{
  public static async Task InitializeDatabaseAsync(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DbContextInitializer>();
    await initializer.InitializeAsync();
    await initializer.SeedAsync();
  }
}