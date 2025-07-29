# Architecture Documentation

This document outlines the high-level technical architecture and key design decisions for the League Manager backend application.

---

## 1. Core Architecture: Clean Architecture

The solution is structured following the principles of **Clean Architecture**. This creates a clear separation of concerns and ensures the core business logic is independent of external frameworks and technologies. The dependency rule is central: dependencies only flow inwards.

### Project Structure

* **`LeagueManager.Domain`**: The core of the application. Contains only the domain models (e.g., `Team`, `Player`) and core enums. It is completely independent of other layers.
* **`LeagueManager.Application`**: The application logic layer. Contains service interfaces (`ITeamService`), Data Transfer Objects (DTOs), AutoMapper profiles, and FluentValidation rules. It defines the application's use cases.
* **`LeagueManager.Infrastructure`**: Contains implementations for external concerns. This includes the Entity Framework `DbContext`, database migrations, and concrete service implementations that access the database.
* **`LeagueManager.API`**: The presentation layer. A thin layer containing only the API controllers, middleware, and startup configuration (`Program.cs`). Its job is to handle HTTP requests and delegate work to the `Application` layer.

---

## 2. Design Patterns & Practices

* **Service Layer Pattern:** All business logic is encapsulated in services (e.g., `TeamService`, `AuthService`). Controllers remain thin and only orchestrate the flow of data.
* **Data Transfer Objects (DTOs):** DTOs are used for all API inputs and outputs. This decouples the public API contract from the internal database models.
* **AutoMapper:** The AutoMapper library is used to handle the mapping between domain models and DTOs, reducing boilerplate code.
* **FluentValidation:** The FluentValidation library is used to define and handle all validation rules for incoming DTOs, keeping validation logic separate from the models themselves.
* **Structured Logging:** **Serilog** is configured to write all application logs in a structured **JSON** format, which is essential for effective monitoring and analysis in a production environment.

---

## 3. Database & Persistence

* **Database:** **PostgreSQL**.
* **ORM:** **Entity Framework Core** using a **Code-First** approach. The database schema is defined by the C# models in the `Domain` layer and managed via EF Core migrations.
* **Seasonal Model (Future):** To support historical records, a many-to-many relationship will be used. `Team` and `Player` will be master records, linked to a `Season` via junction tables (e.g., `SeasonTeam`).

---

## 4. Authentication & Authorization

* **Framework:** **ASP.NET Core Identity** is used as the foundation for user and role management.
* **Authentication Method:** **JSON Web Tokens (JWT)**. A stateless approach where a signed token is issued upon login and validated on subsequent requests.
* **Authorization Strategy:**
  * **Policy-Based Authorization:** Defines high-level rules (e.g., `"CanManageTeam"`, `"CanUpdatePlayer"`).
  * **Resource-Based Authorization:** Permissions are checked against specific resources. A user's right to edit a team depends on their role *for that specific team*, which is managed in the `TeamMembership` table. This is implemented via custom `IAuthorizationHandler` classes.

---

## 5. Error Handling

* **Global Exception Middleware:** A custom middleware catches all unhandled exceptions. This ensures that the application always returns a consistent, standardized `500 Internal Server Error` response without crashing, and all unexpected errors are logged.
* **Specific Exceptions:** Controllers handle specific, expected exceptions (like `ArgumentException` or `KeyNotFoundException` from the service layer) to return meaningful `400 Bad Request` or `404 Not Found` responses to the client.

---

## 6. Testing Strategy

* **Framework:** **xUnit** is used for all tests.
* **Unit Tests (Controllers):** Tested in isolation using **Moq** to mock service dependencies. These tests verify routing logic and correct HTTP response generation.
* **Service Tests:** Tested with a real relational database provider (**SQLite In-Memory**) to verify complex business logic and data interactions, including transactions. This provides a high-fidelity testing environment.

---

## 7. CI/CD & Deployment

* **Local Development:** The environment is fully containerized using **Docker Compose**. Secrets are managed locally via a `.env` file.
* **Continuous Integration (CI):** A **GitHub Actions** workflow runs on every push and pull request. The pipeline builds the solution, runs the full test suite, and performs a static code analysis.
* **Code Quality:** **SonarCloud** is integrated into the CI pipeline to scan for bugs, vulnerabilities, and code smells.
* **Application Hosting:** The API is deployed as a Docker container to **Fly.io**.
* **Database Hosting:** The production PostgreSQL database is hosted on **Supabase**'s free tier.
* **Deployment Process:** Deployments are triggered manually via the `flyctl deploy` command. The application is configured to run its own database migrations automatically on startup.
