# League Manager API

This is the backend API for a web application designed to manage an amateur or company football league. It handles everything from scheduling fixtures and tracking results to managing player stats and team logistics.

---

## Core Features

* Automated league standings table
* Top goal scorer tracking
* Match scheduling and result management
* Role-based access for Admins and Team Leaders
* Player attendance tracking and Man of the Match voting

---

## Getting Started

1. **Prerequisites:** Make sure you have the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed.
2. **Clone Repository:**

    ```bash
    git clone https://github.com/bencukovacs/LeagueManager/tree/main
    ```

3. **Configure Database:**
    * Navigate to the project root in your terminal.
    * Start the PostgreSQL database container:

        ```bash
        docker run --name league-db -e POSTG-RES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5433:5432 -d postgres
        ```

    * Ensure the connection string in `appsettings.Development.json` matches the user, password, and port (`5433`).
4. **Run Application:**
    * Navigate to the `LeagueManager` directory.
    * Run `dotnet run` to start the API.
    * The API will be available at the URLs specified in the launch profile (e.g., `http://localhost:5166`).

---

## Tech Stack

* **Backend:** .NET 8, ASP.NET Core Web API
* **Database:** PostgreSQL
* **Containerization:** Docker
* **Architecture:** RESTful API, Entity Framework Core

---

## Documentation

* **Project Features & Roadmap:** [Project Blueprint](docs/BLUEPRINT.md)
* **Database Schema:** [Database Documentation](docs/DATABASE.md)
* **Frontend Plan:** [Frontend Documentation](docs/FRONTEND.md)
* **Infrastructure & DevOps:** [Infrastructure Documentation](docs/INFRASTRUCTURE.md)
