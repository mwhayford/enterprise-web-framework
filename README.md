# Enterprise Web Application Framework

A comprehensive, production-ready web application framework built with modern technologies and enterprise-grade architecture patterns.

## 🏗️ Architecture Overview

This project implements a **Clean Architecture** pattern with **CQRS** (Command Query Responsibility Segregation) using **MediatR**, providing a scalable and maintainable foundation for enterprise applications.

### Technology Stack

**Backend:**
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core 8.0** - ORM with PostgreSQL
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **ASP.NET Core Identity** - Authentication & Authorization
- **JWT Bearer Tokens** - Stateless authentication
- **Google OAuth 2.0** - Social authentication
- **Stripe.NET** - Payment processing
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation

**Frontend:**
- **Vite 5.x** - Build tool and dev server
- **React 18.x** - UI framework
- **TypeScript 5.x** - Type safety
- **Tailwind CSS 3.x** - Utility-first CSS
- **React Router 6.x** - Client-side routing
- **React Query (TanStack Query)** - Server state management
- **React Hook Form** - Form handling
- **Zod** - Schema validation
- **Axios** - HTTP client
- **Stripe.js** - Payment integration

**Infrastructure:**
- **PostgreSQL 15+** - Primary database
- **Redis** - Caching and session storage
- **Elasticsearch 8.x** - Full-text search
- **Apache Kafka** - Event streaming and message queuing
- **Snowflake** - Data warehouse (planned)
- **Docker** - Containerization
- **Kubernetes (EKS)** - Container orchestration
- **Terraform** - Infrastructure as Code

**DevOps & Quality:**
- **NUnit** - Unit testing
- **TestContainers** - Integration testing
- **Playwright** - End-to-end testing
- **ESLint & Prettier** - Code quality
- **StyleCop & Roslynator** - .NET code analysis
- **GitHub Actions** - CI/CD pipeline

## 🚀 Quick Start

### Prerequisites

- **.NET 8.0 SDK**
- **Node.js 18+** and npm
- **Docker** and Docker Compose
- **PostgreSQL** (or use Docker)
- **Git**

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Core
   ```

2. **Start infrastructure services**
   ```bash
   cd docker
   docker-compose up -d postgres redis elasticsearch kafka
   ```

3. **Configure environment variables**
   ```bash
   # Backend
   cp src/backend/Core.API/appsettings.Development.json.example src/backend/Core.API/appsettings.Development.json
   
   # Frontend
   cp src/frontend/env.example src/frontend/.env.development
   ```

4. **Start the backend**
   ```bash
   cd src/backend
   dotnet restore
   dotnet run --project Core.API
   ```

5. **Start the frontend**
   ```bash
   cd src/frontend
   npm install
   npm run dev
   ```

6. **Access the application**
   - Frontend: http://localhost:3000
   - Backend API: https://localhost:7001
   - Swagger UI: https://localhost:7001/swagger
   - Database Admin: http://localhost:8080 (Adminer)

### Docker Development

For a fully containerized development environment:

```bash
cd docker
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

## 📁 Project Structure

```
Core/
├── src/
│   ├── backend/                 # .NET Backend
│   │   ├── Core.API/           # Web API layer
│   │   ├── Core.Application/   # CQRS handlers, DTOs, validators
│   │   ├── Core.Domain/        # Entities, value objects, domain events
│   │   ├── Core.Infrastructure/ # EF Core, external services
│   │   └── Core.Database/      # Database schema management
│   └── frontend/               # React Frontend
│       ├── src/
│       │   ├── components/     # Reusable UI components
│       │   ├── pages/          # Page components
│       │   ├── hooks/          # Custom React hooks
│       │   ├── services/       # API services
│       │   ├── types/          # TypeScript type definitions
│       │   ├── utils/          # Utility functions
│       │   └── contexts/       # React contexts
├── tests/                      # Test projects
│   ├── Core.UnitTests/         # NUnit unit tests
│   ├── Core.IntegrationTests/  # Integration tests
│   └── Core.E2ETests/          # Playwright E2E tests
├── docker/                     # Docker configurations
├── kubernetes/                 # K8s manifests
├── terraform/                  # Infrastructure as Code
└── docs/                       # Documentation
```

## 🔧 Key Features

### Authentication & Authorization
- **Google OAuth 2.0** integration
- **JWT token** authentication with refresh tokens
- **Role-based** and **policy-based** authorization
- **Secure password** policies

### Payment Processing
- **Stripe integration** for credit/debit cards and ACH
- **Subscription billing** with prorated changes
- **One-time payments** with idempotency
- **Webhook handling** for payment events
- **Payment method** management
- **Refund processing**

### User Management
- **User registration** and profile management
- **Account verification** and email notifications
- **User preferences** and settings
- **Admin user** management

### Search & Analytics
- **Elasticsearch** full-text search
- **Event streaming** with Apache Kafka
- **Data warehouse** integration (Snowflake)
- **Real-time** search indexing

### Background Processing
- **Hangfire** for scheduled jobs
- **Kafka consumers** for event processing
- **Async workflows** for underwriting
- **Email notifications** and reporting

## 🧪 Testing

### Unit Tests
```bash
cd tests/Core.UnitTests
dotnet test
```

### Integration Tests
```bash
cd tests/Core.IntegrationTests
dotnet test
```

### End-to-End Tests
```bash
cd tests/Core.E2ETests
npm install
npx playwright test
```

## 🚀 Deployment

### Docker Production Build
```bash
cd docker
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Kubernetes Deployment
```bash
cd kubernetes
kubectl apply -f .
```

### Terraform Infrastructure
```bash
cd terraform
terraform init
terraform plan
terraform apply
```

## 📚 Documentation

- [Architecture Overview](docs/architecture.md)
- [API Documentation](docs/api-documentation.md)
- [Authentication Guide](docs/authentication.md)
- [Payment Processing](docs/payment-processing.md)
- [Deployment Guide](docs/deployment.md)
- [Contributing Guidelines](CONTRIBUTING.md)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: Check the [docs](docs/) folder
- **Issues**: Create an issue on GitHub
- **Discussions**: Use GitHub Discussions for questions

## 🗺️ Roadmap

- [ ] **Phase 1**: Core authentication and user management
- [ ] **Phase 2**: Payment processing and subscriptions
- [ ] **Phase 3**: Search and analytics capabilities
- [ ] **Phase 4**: Advanced features and optimizations
- [ ] **Phase 5**: Mobile app integration

---

**Built with ❤️ using modern web technologies**
