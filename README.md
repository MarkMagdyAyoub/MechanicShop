# MechanicShop Management System

MechanicShop is a full-stack web application for running the day-to-day work of a mechanic shop. It covers work orders, customers, vehicles, scheduling, billing, and shop dashboards through an ASP.NET Core API and an Angular single-page app.

The project is structured around Clean Architecture, with some Domain-Driven Design and CQRS ideas where they fit. The goal is to keep the domain model separate from infrastructure details, make application behavior easier to test, and keep the API and client from owning business rules.

## Architecture

The solution is split into a few focused projects:

- `MechanicShop.Domain` contains the core business models, value objects, and domain events. It has no dependencies on the other layers.
- `MechanicShop.Application` contains the application use cases. Commands and queries are handled through MediatR, with validation and other application behaviors around them.
- `MechanicShop.Infrastructure` contains the technical implementations: Entity Framework Core with PostgreSQL, Identity authentication, HybridCache, Redis support, Twilio, MailKit, real-time notifications, background work, and other integrations.
- `MechanicShop.Api` is the ASP.NET Core Web API. It exposes the application through versioned REST endpoints.
- `MechanicShop.Client` is the Angular 21 single-page application.
- `MechanicShop.Contracts` contains the DTOs shared between the API and client.

## What It Does

The main workflow starts with work orders. A work order can move through its lifecycle from creation to completion, with real-time updates sent through SignalR. Customers and vehicles are tracked separately so vehicle history stays tied to the right customer record.

Scheduling is calendar-based and supports appointment management and technician assignment. Billing can generate PDF invoices with QuestPDF from labor and parts. The dashboard gives a live view of shop activity and key metrics.

Authentication uses JWT, role-based access control, and rate limiting. Background processing handles automated tasks such as monitoring overdue work orders and sending notifications.

## Technology Stack

Backend:

- .NET 10 and ASP.NET Core
- PostgreSQL with Entity Framework Core
- HybridCache and Redis
- MediatR for in-process messaging and CQRS
- SignalR for real-time updates
- FluentValidation
- Serilog with a Seq sink

Frontend:

- Angular 21
- TypeScript
- RxJS reactive patterns

Observability and DevOps:

- OpenTelemetry for metrics and traces
- Prometheus and Grafana for metrics
- Seq for log management
- Docker and Docker Compose

## Getting Started

You will need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), [Node.js and npm](https://nodejs.org/), and [Docker Desktop](https://www.docker.com/products/docker-desktop).

The quickest way to start the stack is Docker Compose:

```bash
docker-compose up -d
```

That starts the API, database, and monitoring services. Once the containers are running, these endpoints are available:

- API: [http://localhost:5196](http://localhost:5196)
- OpenAPI JSON: [http://localhost:5196/openapi/v1.json](http://localhost:5196/openapi/v1.json)
- SwaggerUI: [http://localhost:5196/swagger/index.html](http://localhost:5196/swagger/index.html)
- Seq logs: [http://localhost:8081](http://localhost:8081)
- Prometheus: [http://localhost:9090](http://localhost:9090)
- Grafana: [http://localhost:3000](http://localhost:3000), using `admin` / `admin`

For local backend development, run the API project directly:

```bash
cd src/MechanicShop.Api
dotnet run
```

Make sure PostgreSQL is running and update `appsettings.Development.json` for your local database settings.

For local frontend development, install the client dependencies and start Angular:

```bash
cd src/MechanicShop.Client
npm install
npm start
```

The client runs at [http://localhost:4200](http://localhost:4200).

## Observability

The project includes the three usual pieces of observability:

- Logs are written as structured Serilog events and centralized in Seq.
- Metrics are collected by Prometheus and visualized in Grafana.
- Traces are emitted through OpenTelemetry so request flow and bottlenecks can be inspected.
