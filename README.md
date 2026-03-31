# Distributed Order Processing Platform
## Full Stack Development - Assignment 2 2026
**Student:** Adeniyi Emmanuel | **Student ID:** 71739

---

## System Architecture

This platform extends the SportsStore MVC application into a distributed, event-driven order processing system built on .NET 10.
```
Customer Portal (Blazor)          Admin Dashboard (React)
         │                                  │
         └──────────────┬───────────────────┘
                        │
              Order Management API
              (.NET 10 · CQRS · MediatR)
                        │
                   RabbitMQ
              (Event Bus / Message Broker)
                        │
        ┌───────────────┼───────────────┐
        │               │               │
  Inventory         Payment         Shipping
  Service           Service         Service
```

---

## Event Flow

1. Customer places order via Blazor portal
2. `OrderManagement.API` saves order to database and publishes `OrderSubmittedEvent` to RabbitMQ
3. `InventoryService` consumes the event, validates stock, publishes `InventoryConfirmedEvent` or `InventoryFailedEvent`
4. `PaymentService` consumes inventory confirmation, simulates payment, publishes `PaymentApprovedEvent` or `PaymentRejectedEvent`
5. `ShippingService` consumes payment approval, generates shipment reference, publishes `ShippingCreatedEvent`
6. `OrderManagement.API` listens for all result events and updates order status in the database

---

## Service Responsibilities

| Service | Responsibility |
|---|---|
| `OrderManagement.API` | Central API — creates orders, publishes events, tracks status |
| `InventoryService` | Validates stock availability (90% success simulation) |
| `PaymentService` | Processes payments (85% approval simulation) |
| `ShippingService` | Generates shipment references and dispatch dates |
| `CustomerPortal` | Blazor UI — product browsing, cart, checkout, order tracking |
| `AdminDashboard` | React UI — order monitoring, filtering, details |
| `Shared.Contracts` | Shared message contracts, events, DTOs and enums |

---

## Tech Stack

- **.NET 10** — All backend services
- **MediatR** — CQRS pattern implementation
- **Entity Framework Core + SQLite** — Data persistence
- **RabbitMQ** — Asynchronous message broker
- **Serilog** — Structured logging across all services
- **AutoMapper** — Object mapping between entities and DTOs
- **Blazor Server** — Customer-facing frontend
- **React** — Admin dashboard frontend
- **Docker + Docker Compose** — Containerisation
- **GitHub Actions** — CI/CD pipeline

---

## How to Run

### Prerequisites
- Docker Desktop installed and running
- .NET 10 SDK

### Run with Docker Compose
```bash
git clone https://github.com/workwithmanny/fs-assignment-2026-2-order-processing-71739.git
cd fs-assignment-2026-2-order-processing-71739
docker-compose up
```

### Services will be available at:
| Service | URL |
|---|---|
| Order Management API | http://localhost:5001 |
| API Swagger UI | http://localhost:5001/swagger |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

### Run locally without Docker
```bash
# Terminal 1 - Start RabbitMQ
docker run -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Terminal 2 - Start Order API
cd OrderManagement.API
dotnet run

# Terminal 3 - Start Inventory Service
cd InventoryService
dotnet run

# Terminal 4 - Start Payment Service
cd PaymentService
dotnet run

# Terminal 5 - Start Shipping Service
cd ShippingService
dotnet run
```

---

## Assumptions and Limitations

- Customer authentication is not implemented — a hardcoded customer ID is used for demonstration
- Payment processing is simulated with an 85% approval rate
- Inventory validation is simulated with a 90% success rate
- SQLite is used instead of SQL Server for Mac compatibility
- Product data is seeded manually for demonstration purposes

---

## GitHub Actions CI

The CI pipeline runs on every push and pull request to master. It restores dependencies, builds the full solution and runs all tests. Build badges and test results are available in the Actions tab.
