using MechanicShop.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services
  .AddApplication();

var app = builder.Build();


app.Run();
