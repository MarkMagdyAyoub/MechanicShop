using MechanicShop.Api.Services;
using MechanicShop.Application;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure;
using MechanicShop.Infrastructure.RealTime;

var builder = WebApplication.CreateBuilder(args);

builder.Services
  .AddApplication()
  .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapHub<WorkOrderHub>(WorkOrderHub.HUB_URL);

app.Run();
