using MechanicShop.Application;
using MechanicShop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
  .AddApplication()
  .AddInfrastructure(builder.Configuration);

var app = builder.Build();


app.Run();
