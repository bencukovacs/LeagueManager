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
    git clone https://github.com/bencukovacs/LeagueManager.git
    ```

3. **Configure Local Secrets:**
    * In the project's root directory, create a copy of the `.env.example` file and name it `.env`.
    * Open this new `.env` file and fill in your desired local secrets for the database password, JWT key, and admin password.
4. **Run Application:**
    * Navigate to the project's root directory in your terminal.
    * Run the following command. This will build the API image and start both the API and database containers.

        ```bash
        docker compose up --build
        ```

    * The API will be available at `http://localhost:8080/swagger`.

---

## Contributing

We welcome contributions! Please read our [**Contributing Guide**](docs/CONTRIBUTING.md) to learn about our development process, coding standards, and how to submit a pull request.

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
