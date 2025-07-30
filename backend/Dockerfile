# Stage 1: Build Environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Use full paths from the repository root for all COPY commands
COPY ["backend/LeagueManager.sln", "./"]
COPY ["backend/LeagueManager.API/*.csproj", "./LeagueManager.API/"]
COPY ["backend/LeagueManager.Application/*.csproj", "./LeagueManager.Application/"]
COPY ["backend/LeagueManager.Domain/*.csproj", "./LeagueManager.Domain/"]
COPY ["backend/LeagueManager.Infrastructure/*.csproj", "./LeagueManager.Infrastructure/"]
COPY ["backend/LeagueManager.Tests/*.csproj", "./LeagueManager.Tests/"]
RUN dotnet restore "LeagueManager.sln"

# Copy the entire backend source code
COPY ./backend .

WORKDIR "/src/LeagueManager.API"
RUN dotnet publish "LeagueManager.API.csproj" -c Release -o /app/publish


# Stage 2: Final Production Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LeagueManager.API.dll"]