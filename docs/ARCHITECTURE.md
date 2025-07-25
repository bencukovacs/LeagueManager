# Architecture Documentation

This document outlines the high-level technical architecture and key design decisions for the League Manager backend application.

---

## 1. Core Architecture: Clean Architecture

The solution is structured following the principles of **Clean Architecture** (also known as Onion Architecture). This creates a clear separation of concerns and ensures the core business logic is independent of external frameworks and technologies.

The dependency rule is central: dependencies only flow inwards. The `Domain` is the center and has no dependencies.

### Project Structure

* **`LeagueManager.Domain`**: The core of the application. Contains only the domain models (e.g., `Team`, `Player`) and core business rules. It is completely independent.
* **`LeagueManager.Application`**: The application logic layer. Contains service interfaces (`ITeamService`), DTOs, AutoMapper profiles, and validation logic. It defines the application's use cases.
* **`LeagueManager.Infrastructure`**: Contains implementations for external concerns. This includes the Entity Framework `DbContext`, database migrations, and concrete service implementations that access the database.
* **`LeagueManager.API`**: The presentation layer. A thin layer containing only the API controllers and startup configuration. Its job is to handle HTTP requests and delegate work to the `Application` layer.

---

## 2. Design Patterns & Practices

* **Service Layer Pattern:** All business logic is encapsulated in services (e.g., `TeamService`, `AuthService`). Controllers remain thin and only orchestrate the flow of data.
* **Data Transfer Objects (DTOs):** DTOs are used for all API inputs and outputs. This decouples the public API contract from the internal database models.
* **AutoMapper:** The AutoMapper library is used to handle the mapping between domain models and DTOs, reducing boilerplate code.
* **Dependency Injection (DI):** Heavily used throughout the application to provide loose coupling. Services are injected into controllers, and the `DbContext` and `IMapper` are injected into services.

---

## 3. Database & Persistence

* **Database:** **PostgreSQL**, running in a Docker container for a consistent development environment.
* **ORM:** **Entity Framework Core** using a **Code-First** approach. The database schema is defined by the C# models in the `Domain` layer and managed via EF Core migrations.
* **Seasonal Model:** To support historical records and multiple seasons, a **many-to-many relationship** will be used. `Team` and `Player` are master records, linked to a `Season` via junction tables (e.g., `SeasonTeam`, `SeasonPlayer`).

---

## 4. Authentication & Authorization

* **Framework:** **ASP.NET Core Identity** is used as the foundation for user and role management.
* **Authentication Method:** **JSON Web Tokens (JWT)**. A stateless approach where a signed token is issued upon login and validated on subsequent requests.
* **Authorization Strategy:**
  * **Policy-Based Authorization:** Defines high-level rules (e.g., `"CanManageTeam"`).
  * **Resource-Based Authorization:** Permissions are checked against specific resources. A user's right to edit a team depends on their role *for that specific team*, which is managed in a `TeamMembership` table.
  * **Custom Claims:** The JWT will contain custom claims (e.g., `TeamLeaderOf: 5`) to optimize certain permission checks.

---

## 5. Error Handling

* **Global Exception Middleware:** A custom middleware catches all unhandled exceptions. This ensures that the application always returns a consistent, standardized `500 Internal Server Error` response without crashing, and all unexpected errors are logged.
* **Specific Exceptions:** Controllers handle specific, expected exceptions (like `ArgumentException` or `KeyNotFoundException`) to return meaningful `400 Bad Request` or `404 Not Found` responses to the client.

---

## 6. Testing Strategy

* **Framework:** **xUnit** is used for all tests.
* **Unit Tests:**
  * **Controllers:** Tested in isolation using **Moq** to mock service dependencies. These tests verify routing logic and correct HTTP response generation.
  * **Services:** Tested with a real database provider (**SQLite In-Memory**) to verify complex business logic and data interactions. This provides a high-fidelity testing environment.
