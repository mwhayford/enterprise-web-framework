# Database Setup and Management

This document describes how to set up and manage the PostgreSQL database for the Core application.

## Prerequisites

- Docker and Docker Compose installed
- .NET 9.0 SDK installed
- PowerShell (for Windows) or Bash (for Linux/macOS)

## Quick Start

### 1. Start Database Services

```powershell
# Windows PowerShell
.\scripts\database.ps1 start

# Linux/macOS Bash
./scripts/database.sh start
```

This will start PostgreSQL and Redis containers with the following configuration:
- **PostgreSQL**: Port 5433, Database: `CoreDb`, User: `postgres`, Password: `password`
- **Redis**: Port 6380

### 2. Run Database Migrations

```powershell
# Windows PowerShell
.\scripts\database.ps1 migrate

# Linux/macOS Bash
./scripts/database.sh migrate
```

This will apply all Entity Framework migrations to create the database schema.

## Database Management Commands

### Available Actions

| Action | Description |
|--------|-------------|
| `start` | Start database services (PostgreSQL + Redis) |
| `stop` | Stop database services |
| `restart` | Restart database services |
| `status` | Show status of database services |
| `logs` | Show logs from database services |
| `migrate` | Run Entity Framework migrations |
| `seed` | Seed database with initial data |

### Examples

```powershell
# Start services
.\scripts\database.ps1 start

# Check status
.\scripts\database.ps1 status

# View logs
.\scripts\database.ps1 logs

# Run migrations
.\scripts\database.ps1 migrate

# Stop services
.\scripts\database.ps1 stop
```

## Manual Database Operations

### Using Docker Compose Directly

```bash
# Start services
docker-compose -f docker-compose.dev.yml up -d

# Stop services
docker-compose -f docker-compose.dev.yml down

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Check status
docker-compose -f docker-compose.dev.yml ps
```

### Using Entity Framework CLI

```bash
# Navigate to API project
cd src/backend/RentalManager.API

# Add new migration
dotnet ef migrations add MigrationName --project ../RentalManager.Infrastructure

# Update database
dotnet ef database update --project ../RentalManager.Infrastructure

# Remove last migration
dotnet ef migrations remove --project ../RentalManager.Infrastructure

# Generate SQL script
dotnet ef migrations script --project ../RentalManager.Infrastructure
```

## Database Schema

The application uses the following main entities:

- **AspNetUsers** (Identity users with custom properties)
- **AspNetRoles** (Identity roles)
- **AspNetUserRoles** (User-role relationships)
- **Payments** (Payment transactions)
- **Subscriptions** (Subscription management)
- **PaymentMethods** (Payment method storage)

### Key Features

- **ASP.NET Core Identity** integration
- **Money value objects** with proper decimal precision
- **Audit fields** (CreatedAt, UpdatedAt)
- **Soft delete** support
- **Indexes** for performance optimization

## Connection Strings

### Development
```
Host=localhost;Port=5433;Database=CoreDb;Username=postgres;Password=password
```

### Production
```
Host=your-rds-endpoint;Database=CoreDb;Username=your-username;Password=your-password;SSL Mode=Require
```

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure ports 5432 and 6379 are not in use
2. **Permission issues**: Run PowerShell as Administrator on Windows
3. **Docker not running**: Ensure Docker Desktop is running
4. **Migration failures**: Check database connectivity and permissions

### Reset Database

```powershell
# Stop services and remove volumes
.\scripts\database.ps1 stop
docker-compose -f docker-compose.dev.yml down -v

# Start fresh
.\scripts\database.ps1 start
.\scripts\database.ps1 migrate
```

### Health Checks

The containers include health checks to ensure services are ready:

- **PostgreSQL**: `pg_isready -U postgres`
- **Redis**: `redis-cli ping`

## Production Considerations

### Security
- Use strong passwords
- Enable SSL/TLS connections
- Restrict network access
- Regular security updates

### Performance
- Configure connection pooling
- Optimize indexes
- Monitor query performance
- Set up database monitoring

### Backup Strategy
- Regular automated backups
- Point-in-time recovery
- Cross-region replication
- Disaster recovery planning

## Monitoring

### Database Metrics
- Connection count
- Query performance
- Storage usage
- Replication lag

### Application Metrics
- Entity Framework query performance
- Connection pool usage
- Migration execution time
- Error rates

## Next Steps

1. **Set up monitoring** with OpenTelemetry
2. **Configure connection pooling** for production
3. **Implement backup strategy**
4. **Set up database migrations in CI/CD**
5. **Add database health checks** to the API

