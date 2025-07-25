# Infrastructure & DevOps Plan

This document outlines the plan for building, testing, deploying, and hosting the League Manager application using modern DevOps practices.

---

## I. Local Development Environment

* **Containerization:** The entire application stack (API and Database) will be defined in a **`docker-compose.yml`** file.
* **Goal:** A developer should be able to get the entire application running locally with a single command: `docker-compose up`.
* **Components:**
  * `api`: A service built from a `Dockerfile` in the `LeagueManager.API` project.
  * `db`: A service using the official `postgres` image, with a persistent volume to store data between sessions.

---

## II. CI/CD (Continuous Integration / Continuous Deployment)

* **Platform:** **GitHub Actions**.
* **Trigger:** The CI/CD workflow will run on every `git push` to the `main` branch.
* **CI Pipeline Stages:**
    1. **Build:** Check out the code and build the entire .NET solution (`dotnet build`).
    2. **Test:** Run the complete unit test suite (`dotnet test`). The build will fail if any test fails.
    3. **Security Scan (DevSecOps):**
        * **Static Analysis (SAST):** Use a tool like **SonarCloud** or **GitHub CodeQL** to scan the C# code for security vulnerabilities and code quality issues.
        * **Container Scan:** Use a tool like **Trivy** to scan the final Docker image for known vulnerabilities in its base layers and dependencies.
    4. **Build & Push Image:** Build the final, optimized Docker image for the API and push it to a container registry (e.g., **Docker Hub**, **GitHub Container Registry**).

---

## III. Infrastructure as Code (IaC)

* **Platform:** **Terraform**.
* **Goal:** The entire cloud infrastructure will be defined as code, allowing it to be versioned, reviewed, and deployed repeatably.
* **Cloud Provider:** To be decided (e.g., AWS, Azure, or GCP).
* **Provisioned Resources:**
  * **VPC:** A dedicated virtual network for the application.
  * **Managed Kubernetes Cluster:** The core of the orchestration (e.g., **AWS EKS**, **Azure AKS**, **GCP GKE**).
  * **Managed PostgreSQL Database:** A production-grade database instance (e.g., **AWS RDS**, **Azure Database for PostgreSQL**).
  * **Secrets Management:** A secure vault for storing sensitive information like the production database password and JWT secret key (e.g., **AWS Secrets Manager**, **Azure Key Vault**).

---

## IV. Deployment & Orchestration

* **Platform:** **Kubernetes (K8s)**.
* **Manifests:** The application's runtime state will be defined in Kubernetes YAML files (`deployment.yaml`, `service.yaml`, `ingress.yaml`).
* **CD Pipeline Stage:**
    1. **Deploy:** The final stage of the GitHub Actions workflow will connect to the Kubernetes cluster and apply the manifest files (`kubectl apply -f .`). This will trigger a rolling update to deploy the new version of the API with zero downtime.
* **Observability:** The Kubernetes cluster will be configured with a monitoring and logging stack.
  * **Metrics:** **Prometheus** will scrape metrics from the API.
  * **Logs:** **Loki** or **Fluentd** will aggregate logs from all running containers.
  * **Dashboards:** **Grafana** will be used to visualize metrics and logs, providing a complete overview of the application's health.
