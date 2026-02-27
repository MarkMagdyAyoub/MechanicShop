<h1>
  <img src="./output.png" alt="Logo" width="100" height="100" style="vertical-align: middle;">
  MechanicShop Management System
  <img src="./output.png" alt="Logo" width="100" height="100" style="vertical-align: middle;">
</h1>

A comprehensive, full-stack web application designed to streamline the daily operations of a modern mechanic shop. This system provides tools for managing work orders, customers, billing, and scheduling, all through a clean and intuitive user interface.

## ⚠️ Project Status: Under Development ⚠️

This project is currently in active development.

*   **Completed Layers:**
    *   `Domain`: The core business logic, entities, and rules are defined.
    *   `Application`: The use cases (features), command/query logic, and application-level services are implemented.
*   **In-Progress Layers:**
    *   `Infrastructure`: Database integration, external services, and other technical implementations are being built.
    *   `Api`: The RESTful API endpoints are being developed.
    *   `Client`: The Angular frontend application is under construction.

## 🏛️ Architectural Overview

This project is built using **Clean Architecture** principles to create a decoupled, maintainable, and testable system. It also incorporates concepts from **Domain-Driven Design (DDD)** and **Command Query Responsibility Segregation (CQRS)**.

The solution is divided into several distinct projects, each with a clear responsibility:

*   **`MechanicShop.Domain`**: The heart of the application. It contains the core business models (Entities) and logic, and has no dependencies on any other layer.
*   **`MechanicShop.Application`**: Orchestrates the business logic using a CQRS pattern. It defines all the application's features (use cases) through Commands (for writing data) and Queries (for reading data).
*   **`MechanicShop.Infrastructure`**: Implements the interfaces defined in the Application layer. This is where technical details like database access (using Entity Framework Core), authentication, and third-party service integrations reside.
*   **`MechanicShop.Api`**: The presentation layer for the backend. It's an ASP.NET Core Web API that exposes the application's functionality to the client via RESTful HTTP endpoints.
*   **`MechanicShop.Client`**: A Single-Page Application (SPA) built with Angular. It provides the user interface and consumes the backend API.
*   **`MechanicShop.Contracts`**: A shared library containing Data Transfer Objects (DTOs) used for communication between the API and the Client.

## ✨ Features

The system is designed to support a wide range of features required for a mechanic shop:

*   **Customer Management**: Maintain a database of customers and their vehicles.
*   **Work Order Management**: Create, update, and track the status of work orders from initiation to completion.
*   **Scheduling**: A calendar-based view for scheduling appointments and assigning technicians.
*   **Repair Tasks**: A catalog of predefined repair tasks and their costs.
*   **Billing & Invoicing**: Generate invoices from completed work orders.
*   **Identity & Access Control**: User authentication and role-based permissions.
*   **Dashboard**: A dashboard providing key metrics and an overview of shop activity.

## 💻 Technology Stack

*   **Backend**:
    *   .NET 10
    *   ASP.NET Core
    *   Entity Framework Core (for data access)
    *   MediatR (for implementing the CQRS pattern)
    *   xUnit (for testing)
*   **Frontend**:
    *   Angular 21+
    *   TypeScript
*   **Database**:
    *   PostgreSQL
    *   Redis for caching

## 🚀 Getting Started

> **Note:** Full setup instructions will be provided once the `Infrastructure` and `Api` layers are more complete.