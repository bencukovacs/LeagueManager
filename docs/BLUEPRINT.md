# Project Blueprint: League Manager

This document outlines the complete plan for the League Manager application, including user roles, features by phase, and detailed workflows.

---

## 1. User Roles

* **Guest (Unregistered):** Can view public-facing information like the league table, top scorers, and upcoming fixtures.
* **Player (Registered `User`):** Optional role. A registered user linked to a `Player` profile. They can sign up for upcoming matches and view team-specific information.
* **Team Leader (`User`):** Can manage their team's roster, submit match results, enter Man of the Match votes, and manage their team's attendance for a match. A team can have a maximum of two Team Leaders.
* **League Admin (`User`):** Has full control. Can create/manage teams, players, locations, and fixtures. They approve match results, handle disputes, and manage the overall league settings.

---

## 2. Phased Development Plan

### **Phase 1: The Core Foundation (Current)**
* **Goal:** Build the basic data structure and APIs for managing the league's core entities.
* **Features:**
    * Data models for `Team`, `Player`, `Fixture`, `Location`.
    * Full CRUD API endpoints for managing these entities.
    * Basic service to calculate a league table from submitted results (logic placeholder).
    * Database setup with PostgreSQL and Entity Framework.
    * Containerize the database with Docker.

### **Phase 2: Management & Results**
* **Goal:** Implement the full workflow for match result submission and approval.
* **Features:**
    * `Result` data model to store scores.
    * `Goal` data model linked to `Player` and `Fixture`.
    * `User` model with roles (`Admin`, `Team Leader`).
    * Secure endpoints for `Team Leader`s to submit match results (scores and goal scorers).
    * Workflow for `League Admin` to approve, dispute, or edit submitted results.
    * Functional league standings and top scorer tables based on approved results.

### **Phase 3: Engagement & Logistics**
* **Goal:** Add features for player interaction and scheduling complexity.
* **Features:**
    * **Match Attendance:**
        * `Team Leader`s and registered `Player`s can mark attendance for an upcoming fixture.
        * Automated notification to `League Admin` if a team lacks sufficient players 24 hours before a match.
    * **Man of the Match (MOM) Voting:**
        * `MOMVote` data model.
        * A `Team Leader` can submit their team's collective vote for their own MOM and the opponent's MOM.
    * **Automated Fixture Drafting:**
        * An admin tool to automatically generate a round-robin schedule for the season.
        * Considers team availability preferences (`TeamAvailability` model).

### **Phase 4: Advanced Features & Integrations**
* **Goal:** Add advanced league formats and external integrations.
* **Features:**
    * Logic for splitting the league into upper and lower house finals after the main season.
    * Ability for admins to manage partial parking lot closures for specific dates.
    * (Optional) Integration with Microsoft Teams for notifications or attendance polling.

---

## 3. Detailed Workflows

### **Result Submission Workflow**
1.  After a match, the `Team Leader` from either team accesses the "Submit Result" page.
2.  They enter the final score and select the goal scorers from a player list.
3.  The result is saved with a `PendingApproval` status.
4.  The `League Admin` receives a notification.
5.  The Admin reviews the result. They can `Approve` it, making it official, or `Dispute` it, which may require manual correction. The league table only updates with `Approved` results.

### **Match Attendance Workflow**
1.  A fixture is scheduled.
2.  Up until 24 hours before kickoff, registered `Player`s can mark themselves as "Attending" or "Not Attending."
3.  `Team Leader`s can view their team's attendance list and can manually mark unregistered players as attending.
4.  24 hours before the match, if a team has fewer than the required number of players signed up, a notification is automatically sent to the `League Admin`.
