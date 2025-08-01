# League Manager: Project Blueprint

This document outlines the vision, features, and technical architecture for a comprehensive football league management platform.

---

## I. Core Concepts & Entities

* **League / Season:** The top-level container.
* **LeagueConfiguration:** A table to store league-specific rules, linked to a Season.
  * **Properties:** `SeasonId`, `MinPlayersPerTeam`, `MatchLengthMinutes`, `MidSeasonTransferLimit`, `RosterLockDate`.
* **Team:** Represents a team in the league.
  * **Properties:** `Name`, `PrimaryColor`, `SecondaryColor`, `Status` (e.g., `PendingApproval`, `Approved`), `SendAttendanceReminders` (boolean).
* **Player:** A roster member. Can exist without a User account.
  * **Properties:** `Name`. Linked to a `Team`.
* **User:** A person who can log in for authentication and authorization.
  * **Properties:** `Email`, `PasswordHash`. Can be optionally linked to a `Player` entity. Has a `Role`.
* **TeamMembership:** Links a `User` to a `Team` with a specific role.
  * **Properties:** `UserId`, `TeamId`, `Role` (e.g., `Leader`, `AssistantLeader`, `Member`).
* **Location:** A venue for matches.
* **Fixture / Match:** A scheduled game.
* **Result:** The outcome of a completed match.
* **Goal:** A goal scored in a match.
  * **Properties:** Linked to a `Player` and `Fixture`, `MinuteOfGoal` (for live tracking).
* **MOMVote:** A team's collective "Man of the Match" vote for a fixture.
* **TeamAvailability:** A record of when a team is available to play.
* **Attendance:** A player's confirmation for a specific match.
* **JoinRequest:** A user's request to join a team.
  * **Properties:** `UserId`, `TeamId`, `Status` (`Pending`, `Approved`, `Rejected`).
* **TransferAppeal:** A Team Leader's request to sign a player beyond the mid-season limit.
  * **Properties:** `TeamId`, `PlayerName`, `Justification`, `Status` (`Pending`, `Approved`, `Rejected`).

---

## II. User Roles & Permissions

* **Guest (Anonymous User):** Can view public league data (standings, fixtures, etc.).
* **RegisteredUser:** Everything a Guest can do, plus:
  * Create a new team (which starts as `PendingApproval`).
  * Request to join an existing team.
* **Team Member (via `TeamMembership`):** Everything a RegisteredUser can do, plus:
  * Sign up for their own team's matches.
  * Contribute to live match updates (e.g., log a goal for their team).
* **Team Leader / AssistantLeader (via `TeamMembership`):** Everything a Team Member can do, plus:
  * Manage their team's roster (add/remove player names, send invites).
  * Submit final match results.
  * Manage attendance for all players on their roster.
  * Approve live match events submitted by their team members.
  * Submit transfer appeals.
  * Configure team settings (like opting out of email reminders).
* **League Admin:** Full control over the system.
  * Manage seasons and their settings in the `LeagueConfiguration` table.
  * Approve pending teams after checking the "onboarding checklist."
  * Approve match results and transfer appeals.
  * Send broadcast messages to all Team Leaders.

---

## III. Feature Roadmap: A Phased Approach

### Phase 1: Core MVP (Complete)

* **Functionality:** Standings table, goal scorers table, and basic CRUD for all core entities. All data is entered by an Admin.
* **Architecture:** Clean Architecture with separate Domain, Application, Infrastructure, and API projects. Full unit test suite for all services and controllers.

### Phase 2: Management & Authentication Layer

* **Goal:** Empower users to register, log in, and manage their own teams.
* **Features:**
  * **User Authentication:** Full registration and JWT-based login system.
  * **Team Creation Workflow:** Registered users can create teams, which enter a `PendingApproval` state.
  * **Team-Specific Roles:** Implement the `TeamMembership` system.
  * **Result Submission:** Team Leaders can submit match results, including **MOM Votes**.
  * **Admin Approval Queues:** A dashboard for the League Admin to view and approve pending teams and results.
  * **League Configuration:** An admin page to set season-specific rules in the `LeagueConfiguration` table.
  * **Team Onboarding Checklist:** The `ApproveTeam` logic will read from the `LeagueConfiguration` table to check rules (e.g., minimum players).

### Phase 3: Advanced Scheduling & Roster Management

* **Goal:** Add complex, high-value features.
* **Features:**
  * **Automated Fixture Drafting:** An admin tool to generate a full season schedule based on team availability.
  * **Player Attendance Sign-up:** Players and leaders can manage attendance.
  * **Team Leader Invites & Player Join Requests:** User-driven roster management.
  * **Roster Lock & Transfers:** Admins can set a roster lock date. Team Leaders can use their mid-season transfer allowance and submit appeals.

### Phase 4: Live Features & Notifications

* **Goal:** Add real-time engagement and automated communication.
* **Features:**
  * **Live Match Tracking:** A page for team members to log goals and other events in real-time. Includes a dual-approval workflow for team leaders to confirm events.
  * **Live Match Display:** A public page for anyone to follow the score and key events of a running match.
  * **Email Notification System:**
    * Automated attendance reminders (with team-level opt-out).
    * Insufficient player warnings to leaders and admins.
    * Broadcast messaging from admins to all team leaders.
  * **MS Teams Integration:** (Future) Post match reminders via webhooks.
