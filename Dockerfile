# Use the full .NET SDK image, which includes the 'dotnet ef' tools
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install the dotnet-ef global tool
RUN dotnet tool install --global dotnet-ef
# Add the tool to the PATH so the shell can find it
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /src

# Copy all source code into the image
COPY . .

# Restore all dependencies for the solution
RUN dotnet restore "LeagueManager.sln"

# Install the postgresql-client needed for the startup script
RUN apt-get update && apt-get install -y --no-install-recommends postgresql-client

# Make our entrypoint script executable
RUN chmod +x ./LeagueManager.API/entrypoint.sh

# Set the entrypoint to our script. This will be the first thing that runs.
ENTRYPOINT ["/src/LeagueManager.API/entrypoint.sh"]