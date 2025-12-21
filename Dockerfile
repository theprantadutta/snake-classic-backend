# Snake Classic Backend - .NET 10 Clean Architecture
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y libkrb5-3 curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8393

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files
COPY ["snake-classic-backend.sln", "./"]
COPY ["src/SnakeClassic.Domain/SnakeClassic.Domain.csproj", "src/SnakeClassic.Domain/"]
COPY ["src/SnakeClassic.Application/SnakeClassic.Application.csproj", "src/SnakeClassic.Application/"]
COPY ["src/SnakeClassic.Infrastructure/SnakeClassic.Infrastructure.csproj", "src/SnakeClassic.Infrastructure/"]
COPY ["src/SnakeClassic.Api/SnakeClassic.Api.csproj", "src/SnakeClassic.Api/"]

# Restore dependencies
RUN dotnet restore "snake-classic-backend.sln"

# Copy all source code
COPY . .

# Build the solution
WORKDIR "/src/src/SnakeClassic.Api"
RUN dotnet build "SnakeClassic.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SnakeClassic.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8393
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SnakeClassic.Api.dll"]
