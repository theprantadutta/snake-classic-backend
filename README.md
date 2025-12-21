# Snake Classic Backend

A .NET 10 Web API backend for the Snake Classic mobile game, built with Clean Architecture principles.

## Overview

This backend provides RESTful APIs and real-time communication for the Snake Classic game, supporting features like user authentication, leaderboards, achievements, multiplayer gaming, tournaments, battle pass, and in-app purchases.

## Architecture

The project follows **Clean Architecture** with four layers:

```
snake-classic-backend/
├── src/
│   ├── SnakeClassic.Domain/           # Entities, Enums, Interfaces (innermost)
│   ├── SnakeClassic.Application/      # CQRS handlers, DTOs, Business logic
│   ├── SnakeClassic.Infrastructure/   # EF Core, Firebase, External services
│   └── SnakeClassic.Api/              # Controllers, SignalR Hubs, Middleware
├── snake-classic-backend.sln
├── Dockerfile
└── compose.yaml
```

## Tech Stack

- **.NET 10** - Runtime and SDK
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM with PostgreSQL
- **MediatR** - CQRS pattern implementation
- **SignalR** - Real-time multiplayer communication
- **Hangfire** - Background job processing
- **Firebase Admin SDK** - Authentication and push notifications
- **Serilog** - Structured logging
- **Scalar** - API documentation

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)
- Firebase project with Admin SDK credentials

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-username/snake-classic-backend.git
cd snake-classic-backend
```

### 2. Configure environment variables

Create a `.env` file in the root directory:

```bash
# Database
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=snake_classic
DATABASE_USERNAME=postgres
DATABASE_PASSWORD=your_password

# JWT
JWT_SECRET_KEY=your-256-bit-secret-key-minimum-32-characters

# Firebase
FIREBASE_PROJECT_ID=your-firebase-project-id
GOOGLE_APPLICATION_CREDENTIALS=firebase-admin-sdk.json

# API
ASPNETCORE_ENVIRONMENT=Development
API_PORT=8393
ALLOWED_ORIGINS=*
```

### 3. Add Firebase credentials

Place your Firebase Admin SDK JSON file as `firebase-admin-sdk.json` in the root directory.

### 4. Run database migrations

```bash
cd src/SnakeClassic.Infrastructure
dotnet ef database update -s ../SnakeClassic.Api
```

### 5. Run the application

```bash
# From solution root
dotnet run --project src/SnakeClassic.Api

# Or with hot reload
dotnet watch run --project src/SnakeClassic.Api
```

The API will be available at `http://localhost:8393`

## Docker Deployment

### Build and run with Docker Compose

```bash
docker compose up --build
```

### Build image only

```bash
docker build -t snake-classic-backend .
```

### Run container

```bash
docker run -p 8393:8393 --env-file .env snake-classic-backend
```

## API Documentation

Once running, access the interactive API documentation:

- **Scalar UI**: http://localhost:8393/scalar/v1
- **OpenAPI JSON**: http://localhost:8393/openapi/v1.json
- **Health Check**: http://localhost:8393/health

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/firebase` | Authenticate with Firebase token |
| GET | `/api/v1/auth/me` | Get current user |
| POST | `/api/v1/auth/logout` | Logout |
| POST | `/api/v1/auth/refresh` | Refresh token |

### Users
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/users/me` | Get current user profile |
| PUT | `/api/v1/users/me` | Update profile |
| POST | `/api/v1/users/username/check` | Check username availability |
| PUT | `/api/v1/users/username` | Set username |
| GET | `/api/v1/users/{id}` | Get user by ID |
| GET | `/api/v1/users/search` | Search users |
| POST | `/api/v1/users/register-token` | Register FCM token |

### Scores
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/scores` | Submit score |
| GET | `/api/v1/scores/me` | Get my scores |
| GET | `/api/v1/scores/me/stats` | Get my statistics |
| POST | `/api/v1/scores/batch` | Batch submit scores |

### Leaderboards
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/leaderboard/global` | Global leaderboard |
| GET | `/api/v1/leaderboard/weekly` | Weekly leaderboard |
| GET | `/api/v1/leaderboard/daily` | Daily leaderboard |
| GET | `/api/v1/leaderboard/friends` | Friends leaderboard |

### Achievements
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/achievements` | Get all achievements |
| GET | `/api/v1/achievements/me` | Get my achievements |
| POST | `/api/v1/achievements/progress` | Update progress |
| POST | `/api/v1/achievements/claim` | Claim reward |

### Social
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/social/friends` | Get friends list |
| GET | `/api/v1/social/requests` | Get friend requests |
| POST | `/api/v1/social/friends/request` | Send friend request |
| POST | `/api/v1/social/friends/accept/{id}` | Accept request |
| POST | `/api/v1/social/friends/reject/{id}` | Reject request |
| DELETE | `/api/v1/social/friends/{id}` | Remove friend |

### Tournaments
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/tournaments` | Get tournaments |
| GET | `/api/v1/tournaments/active` | Get active tournaments |
| GET | `/api/v1/tournaments/{id}` | Get tournament details |
| POST | `/api/v1/tournaments/{id}/join` | Join tournament |
| POST | `/api/v1/tournaments/{id}/score` | Submit score |
| GET | `/api/v1/tournaments/{id}/leaderboard` | Tournament leaderboard |

### Multiplayer
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/multiplayer/create` | Create game |
| POST | `/api/v1/multiplayer/join` | Join game |
| GET | `/api/v1/multiplayer/game/{id}` | Get game state |
| POST | `/api/v1/multiplayer/game/{id}/leave` | Leave game |
| GET | `/api/v1/multiplayer/current` | Get current game |

### Battle Pass
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/battlepass/current-season` | Get current season |
| GET | `/api/v1/battlepass/progress` | Get my progress |
| POST | `/api/v1/battlepass/add-xp` | Add XP |
| POST | `/api/v1/battlepass/claim-reward` | Claim reward |
| POST | `/api/v1/battlepass/purchase-premium` | Purchase premium |

### Purchases
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/purchases/verify` | Verify purchase |
| POST | `/api/v1/purchases/restore` | Restore purchases |
| GET | `/api/v1/purchases/premium-content` | Get premium content |

### Notifications
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/notifications/topics/subscribe` | Subscribe to topic |
| POST | `/api/v1/notifications/topics/unsubscribe` | Unsubscribe |
| POST | `/api/v1/notifications/send-individual` | Send to user |
| POST | `/api/v1/notifications/send-topic` | Send to topic |

## Real-time Communication (SignalR)

The multiplayer hub is available at `/hubs/game`

### Client Events (send to server)
- `JoinRoom` - Join a game room
- `LeaveRoom` - Leave a game room
- `SetReady` - Set player ready status
- `StartGame` - Start the game (host only)
- `SendMove` - Send player movement
- `PlayerDied` - Notify player death
- `GameOver` - Notify game over
- `UpdateGameState` - Update game state (host only)

### Server Events (receive from server)
- `PlayerJoined` - Player joined the room
- `PlayerLeft` - Player left the room
- `PlayerReady` - Player ready status changed
- `GameStarting` - Game is starting (countdown)
- `GameStarted` - Game has started
- `PlayerMoved` - Player moved
- `GameStateUpdated` - Game state updated
- `PlayerDied` - Player died
- `GameEnded` - Game ended

## Project Structure

```
src/
├── SnakeClassic.Domain/
│   ├── Entities/           # User, Score, Achievement, Tournament, etc.
│   ├── Enums/              # GameMode, FriendshipStatus, etc.
│   └── Common/             # Base entities
│
├── SnakeClassic.Application/
│   ├── Common/             # Result, Interfaces
│   ├── Features/
│   │   ├── Auth/           # Commands & Queries
│   │   ├── Users/
│   │   ├── Scores/
│   │   ├── Leaderboards/
│   │   ├── Achievements/
│   │   ├── Social/
│   │   ├── Tournaments/
│   │   ├── Multiplayer/
│   │   ├── BattlePass/
│   │   ├── Purchases/
│   │   └── Notifications/
│   └── DependencyInjection.cs
│
├── SnakeClassic.Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/  # Entity configurations
│   │   └── Migrations/
│   ├── Services/
│   │   ├── FirebaseAuthService.cs
│   │   ├── FirebaseMessagingService.cs
│   │   ├── JwtService.cs
│   │   └── DateTimeService.cs
│   └── DependencyInjection.cs
│
└── SnakeClassic.Api/
    ├── Controllers/V1/      # API controllers
    ├── Hubs/                # SignalR hubs
    ├── Services/            # CurrentUserService
    └── Program.cs           # Application entry point
```

## Database Migrations

### Create a new migration

```bash
cd src/SnakeClassic.Infrastructure
dotnet ef migrations add MigrationName -s ../SnakeClassic.Api -o Persistence/Migrations
```

### Apply migrations

```bash
dotnet ef database update -s ../SnakeClassic.Api
```

### Remove last migration

```bash
dotnet ef migrations remove -s ../SnakeClassic.Api
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=snake_classic;..."
  },
  "JwtSettings": {
    "SecretKey": "",
    "Issuer": "SnakeClassicApi",
    "Audience": "SnakeClassicApp",
    "ExpiryMinutes": 10080
  },
  "Firebase": {
    "ProjectId": "snake-classic-2a376"
  }
}
```

Configuration is loaded from:
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Environment variables
4. `.env` file (auto-loaded at startup)

## Hangfire Dashboard

In development mode, the Hangfire dashboard is available at:

http://localhost:8393/hangfire

## Logging

Logs are written to:
- Console (development)
- `logs/snake-classic-{date}.log` (file)

Log levels can be configured via `LOG_LEVEL` environment variable.

## Health Check

```bash
curl http://localhost:8393/health
```

Response:
```json
{
  "status": "healthy",
  "timestamp": "2025-01-15T12:00:00Z",
  "version": "1.0.0"
}
```

## License

This project is proprietary software. All rights reserved.
