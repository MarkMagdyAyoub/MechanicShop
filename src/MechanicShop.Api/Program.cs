// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using MechanicShop.Api;
using MechanicShop.Application;
using MechanicShop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
  .AddApplication()
  .AddInfrastructure(builder.Configuration)
  .AddPresentation(builder.Configuration, builder.Host, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  await app.AddDevelopmentDependencies();
}

app.UseApplicationMiddlewares();

app.MapEndpoints();

app.Run();
