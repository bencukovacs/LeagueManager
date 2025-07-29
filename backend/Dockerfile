# Stage 1: Build Environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies for caching
COPY ["*.sln", "./"]
COPY ["LeagueManager.API/*.csproj", "./LeagueManager.API/"]
COPY ["LeagueManager.Application/*.csproj", "./LeagueManager.Application/"]
COPY ["LeagueManager.Domain/*.csproj", "./LeagueManager.Domain/"]
COPY ["LeagueManager.Infrastructure/*.csproj", "./LeagueManager.Infrastructure/"]
COPY ["LeagueManager.Tests/*.csproj", "./LeagueManager.Tests/"]
RUN dotnet restore "LeagueManager.sln"

# Copy the rest of the source code and publish the API
COPY . .
WORKDIR "/src/LeagueManager.API"
RUN dotnet publish "LeagueManager.API.csproj" -c Release -o /app/publish

# Stage 2: Final Production Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LeagueManager.API.dll"]