#!/bin/bash

# Exit immediately if a command exits with a non-zero status.
set -e

# Wait for the database to be ready
until pg_isready -h db -p 5432 -U postgres; do
  >&2 echo "Postgres is unavailable - sleeping"
  sleep 1
done

>&2 echo "Postgres is up - running migrations..."

# Run database migrations from the source code context
dotnet ef database update --project ./LeagueManager.Infrastructure --startup-project ./LeagueManager.API

>&2 echo "Migrations complete - starting application..."

# Start the application, telling it to listen on the correct URL
dotnet run --project ./LeagueManager.API --urls http://+:8080