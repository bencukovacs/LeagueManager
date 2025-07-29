# Infrastructure & DevOps Documentation

This document describes the current state of the infrastructure used for building, testing, deploying, and hosting the League Manager application.

---

## I. Local Development Environment

* **Containerization:** The local development environment is fully containerized using **Docker Compose**.
* **Process:** A single command, `docker compose up`, builds the API's Docker image and starts both the API and a PostgreSQL database container.
* **Configuration:** Local secrets (database password, JWT key, etc.) are managed via a `.env` file at the project root, which is ignored by Git.

---

## II. Code Quality & Security

* **Automated Analysis:** **SonarCloud** is integrated into the CI/CD pipeline. It performs a deep static analysis (SAST) on every push and pull request, checking for bugs, security vulnerabilities, and code smells.
* **Real-time Feedback:** It is highly recommended to use the **SonarLint** extension for your IDE (e.g., VS Code). By connecting it to the SonarCloud project, you get instant feedback and analysis directly in your editor as you write code.

---

## III. CI/CD (Continuous Integration / Continuous Deployment)

* **Platform:** **GitHub Actions**.
* **Trigger:** The CI/CD workflow runs on every `git push` to the `main` branch and on every `pull_request` targeting `main`.
* **CI Pipeline Stages:**
    1. **Build:** Checks out the code and builds the entire .NET solution.
    2. **Test:** Runs the complete unit test suite.
    3. **Security Scan:** Runs the SonarCloud analysis.
    4. **Build & Push Image:** (On pushes to `main` only) Builds the final, optimized Docker image for the API and pushes it to the **GitHub Container Registry**.

---

## IV. Cloud Hosting & Deployment

* **Application Hosting:** The API is hosted on **Fly.io**.
  * **Platform:** It runs as a container on the Fly.io Machines platform.
  * **Configuration:** The application is defined by a `fly.toml` file. Secrets are managed through the `flyctl secrets` command.
  * **Deployment:** Deployments are triggered manually by running `flyctl deploy`. The application is configured to run its own database migrations on startup.
* **Database Hosting:** The PostgreSQL database is hosted on **Supabase**.
  * **Tier:** It uses the "always free" tier, which provides a robust and permanent database solution for the project.
  * **Connection:** The application connects to the Supabase database over the public internet using a connection string stored as a secret in Fly.io.
