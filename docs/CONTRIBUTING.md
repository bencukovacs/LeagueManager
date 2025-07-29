# How to Contribute to League Manager

Thank you for your interest in contributing! This document outlines the process for setting up your development environment, our coding standards, and the workflow for submitting changes.

---

## 1. Setting Up Your Development Environment

To get the project running locally, please follow the instructions in the main [**`README.md`**](../README.md) file. This will guide you through setting up Docker and running the application with a single command.

---

## 2. Git Workflow

We follow a standard feature-branch workflow. **Never commit directly to the `main` branch.**

1. **Create a Branch:** Before starting work on a new feature or bugfix, create a new branch from the latest version of `main`. Please use the following naming convention:
    * For features: `feature/add-player-join-requests`
    * For bugfixes: `fix/incorrect-point-calculation`

2. **Commit Your Changes:** Make small, logical commits. Write clear and concise commit messages that explain the "why" behind your change.

3. **Open a Pull Request (PR):** When your feature is complete and tested, push your branch to GitHub and open a Pull Request against the `main` branch.
    * In the PR description, please describe the changes you made and reference any relevant issues.

---

## 3. Coding Standards & Best Practices

* **Clean Architecture:** All new code must follow the established Clean Architecture pattern. Business logic belongs in services, data models in the domain, and controllers should remain thin.
* **Testing:** Any new feature or bugfix **must** be accompanied by unit tests.
  * New service logic should be tested with service-level tests (using the SQLite provider).
  * New controller endpoints should be tested with controller-level tests (using Moq).
* **Code Quality:** Run the code through SonarLint in your IDE to catch any quality or security issues before committing. All issues reported by the SonarCloud scan in the Pull Request must be addressed.

---

## 4. Running Tests

To run the full test suite, navigate to the root directory of the solution and run the following command:

```bash
dotnet test
```

All tests must be passing before a Pull Request will be considered for merging.