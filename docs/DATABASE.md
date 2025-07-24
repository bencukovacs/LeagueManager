# Database Schema Documentation

This document illustrates the database structure for the League Manager application. It shows the current tables and relationships, as well as the planned future additions.

---

## 1. Current Schema (Phase 1)

This is the core structure we have built so far. It's focused on scheduling matches and defining the basic entities of the league.

```mermaid
erDiagram
    Team {
        int Id PK
        string Name
        string PrimaryColor
        string SecondaryColor
    }

    Player {
        int Id PK
        string Name
        int TeamId FK
    }

    Location {
        int Id PK
        string Name
        string Address
        string PitchNumber
    }

    Fixture {
        int Id PK
        int HomeTeamId FK
        int AwayTeamId FK
        datetime KickOffDateTime
        int Status
        int LocationId FK
    }

    Team ||--o{ Player : "has"
    Team ||--o{ Fixture : "is home team"
    Team ||--o{ Fixture : "is away team"
    Location ||--o{ Fixture : "hosts"