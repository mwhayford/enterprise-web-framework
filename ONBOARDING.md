# Developer Onboarding Guide

Welcome to the RentalManager project! This guide will help you get set up for local development.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Initial Setup](#initial-setup)
3. [Configuration](#configuration)
4. [Running the Application](#running-the-application)
5. [Running Tests](#running-tests)
6. [Development Workflow](#development-workflow)
7. [Troubleshooting](#troubleshooting)
8. [Next Steps](#next-steps)

---

## Prerequisites

Before starting, ensure you have the following installed:

### Required Tools

- **.NET SDK 9.0** or later
  - Download from [.NET Downloads](https://dotnet.microsoft.com/download)
  - Verify: `dotnet --version`

- **Node.js 18** or later and npm
  - Download from [Node.js Downloads](https://nodejs.org/)
  - Verify: `node --version` and `npm --version`

- **Docker Desktop**
  - Download from [Docker Desktop](https://www.docker.com/products/docker-desktop)
  - Verify: `docker --version` and `docker-compose --version`

- **Git**
  - Download from [Git Downloads](https://git-scm.com/downloads)
  - Verify: `git --version`

### Recommended Tools

- **Visual Studio Code** or **Visual Studio**
  - VS Code: [Download](https://code.visualstudio.com/)
  - Visual Studio: [Download](https://visualstudio.microsoft.com/)

- **Postman** or **Insomnia** (for API testing)

- **Database Client** (optional)
  - pgAdmin or DBeaver for PostgreSQL
  - Redis Commander for Redis

---

## Initial Setup

### Step 1: Clone the Repository

```bash
# Clone your fork (if contributing) or the main repository
git clone https://github.com/your-org/RentalManager.git
cd RentalManager
```

### Step 2: Start Infrastructure Services

Start the required services using Docker Compose:

```bash
# Navigate to the project root
cd C:\src\RentalManager

# Start all infrastructure services
docker-compose up -d postgres redis elasticsearch kafka

# Verify services are running
docker-compose ps
```

Expected services:
- PostgreSQL (port 5433)
- Redis (port 6380)
- Elasticsearch (port 9200)
- Apache Kafka (port 9092)

### Step 3: Install Backend Dependencies

```bash
cd src/backend

# Restore NuGet packages
dotnet restore RentalManager.sln

# Verify the solution builds
dotnet build RentalManager.sln
```

### Step 4: Install Frontend Dependencies

```bash
cd src/frontend

# Install npm packages
npm install

# Verify installation
npm run build
```

---

## Configuration

### Backend Configuration

1. **Copy the configuration template:**

```powershell
cd src/backend/RentalManager.API
Copy-Item appsettings.Development.json.template appsettings.Development.json
```

2. **Edit `appsettings.Development.json`** and configure the following:

**Required Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=RentalManagerDb_Dev;Username=postgres;Password=password"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  },
  "JwtSettings": {
    "SecretKey": "YOUR_JWT_SECRET_KEY_MIN_32_CHARACTERS_LONG"
  },
  "StripeSettings": {
    "PublishableKey": "pk_test_YOUR_PUBLISHABLE_KEY",
    "SecretKey": "sk_test_YOUR_SECRET_KEY"
  }
}
```

**Getting Credentials:**

- **Google OAuth**: See [Google Cloud Console Setup Guide](docs/authentication-flow.md)
- **JWT Secret Key**: Generate a random 32+ character string
  ```powershell
  -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
  ```
- **Stripe Test Keys**: Get from [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys)

### Frontend Configuration

1. **Copy the environment template:**

```powershell
cd src/frontend
Copy-Item env.example .env.local
```

2. **Edit `.env.local`** with your configuration:

```env
VITE_API_BASE_URL=https://localhost:7001/api
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_your_stripe_publishable_key
```

### Database Setup

Run Entity Framework migrations:

```bash
cd src/backend/RentalManager.API

# Create the database
dotnet ef database update --project ../RentalManager.Infrastructure --startup-project .
```

Or use the included script:

```powershell
cd scripts
.\database.ps1
```

---

## Running the Application

### Option 1: Docker Compose (Recommended)

Run everything with Docker Compose:

```bash
cd C:\src\RentalManager
docker-compose up
```

This starts:
- Backend API on http://localhost:5111
- Frontend on http://localhost:5173
- All infrastructure services

### Option 2: Manual Development

**Backend:**

```bash
cd src/backend
dotnet run --project RentalManager.API
```

Backend will be available at:
- API: https://localhost:7001
- Swagger UI: https://localhost:7001/swagger
- Hangfire Dashboard: https://localhost:7001/hangfire

**Frontend:**

```bash
cd src/frontend
npm run dev
```

Frontend will be available at: http://localhost:3000

---

## Running Tests

### Backend Tests

**Unit Tests:**
```bash
cd tests/RentalManager.UnitTests
dotnet test
```

**Integration Tests:**
```bash
cd tests/RentalManager.IntegrationTests
dotnet test
```

**All Tests:**
```bash
cd src/backend
dotnet test
```

### Frontend Tests

```bash
cd src/frontend
npm test
```

### End-to-End Tests

```bash
cd tests/RentalManager.E2ETests
npm install
npx playwright test
```

### Test Coverage

Generate test coverage reports:

```bash
# Backend
cd src/backend
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Frontend
cd src/frontend
npm run test:coverage
```

---

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout main
git pull origin main
git checkout -b feature/your-feature-name
```

### 2. Make Your Changes

- Write clean, well-documented code
- Follow the [coding standards](CONTRIBUTING.md#coding-standards)
- Add tests for new functionality
- Update documentation as needed

### 3. Test Your Changes

```bash
# Run all tests
dotnet test
npm test
npx playwright test

# Ensure no linting errors
dotnet build
npm run lint
```

### 4. Commit Your Changes

Follow [conventional commit](CONTRIBUTING.md#commit-message-conventions) format:

```bash
git add .
git commit -m "feat(auth): add Google OAuth login"
```

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub.

---

## Troubleshooting

### Common Issues

**1. Database Connection Failed**

```bash
# Check PostgreSQL is running
docker-compose ps

# Restart PostgreSQL
docker-compose restart postgres

# Check connection string in appsettings.Development.json
```

**2. Port Already in Use**

```bash
# Find process using port 7001
netstat -ano | findstr :7001

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

**3. Frontend Build Errors**

```bash
# Clear node_modules and reinstall
cd src/frontend
Remove-Item -Recurse -Force node_modules
npm install

# Clear build cache
Remove-Item -Recurse -Force dist
```

**4. Backend Build Errors**

```bash
# Clean and rebuild
cd src/backend
dotnet clean
dotnet restore
dotnet build

# Clear NuGet cache if needed
dotnet nuget locals all --clear
```

**5. Docker Services Not Starting**

```bash
# Stop all containers
docker-compose down

# Remove volumes
docker-compose down -v

# Start fresh
docker-compose up -d
```

### Getting Help

- Check existing [documentation](docs/)
- Search [GitHub Issues](https://github.com/your-org/RentalManager/issues)
- Ask in [GitHub Discussions](https://github.com/your-org/RentalManager/discussions)
- Review [troubleshooting guide](docs/troubleshooting.md) (coming soon)

---

## Next Steps

Now that you're set up, here are recommended next steps:

1. **Explore the Codebase**
   - Review [architecture documentation](docs/architecture.md)
   - Explore the [API documentation](docs/api-documentation.md)
   - Check out [existing tests](tests/)

2. **Contribute**
   - Read [CONTRIBUTING.md](CONTRIBUTING.md)
   - Find [good first issues](https://github.com/your-org/RentalManager/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22)
   - Join our [development discussions](https://github.com/your-org/RentalManager/discussions)

3. **Learn the Domain**
   - Review [domain documentation](docs/database-schema.md)
   - Understand the rental management workflow
   - Explore business logic in [Domain layer](src/backend/RentalManager.Domain/)

4. **Development Resources**
   - [Testing Guide](docs/unit-testing.md)
   - [Database Setup](docs/database-setup.md)
   - [CI/CD Documentation](docs/ci-cd.md)
   - [Deployment Guide](docs/deployment.md)

---

## Additional Resources

- **Project README**: [README.md](README.md)
- **Contributing Guidelines**: [CONTRIBUTING.md](CONTRIBUTING.md)
- **API Documentation**: [docs/api-documentation.md](docs/api-documentation.md)
- **Architecture Overview**: [docs/architecture.md](docs/architecture.md)
- **Troubleshooting**: Contact the team via GitHub Discussions

---

**Happy coding! 🎉**

If you have any questions or need help, don't hesitate to reach out to the team.

