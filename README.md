# 🏗️ DDD Pattern Template

A comprehensive Domain-Driven Design (DDD) pattern template for building scalable, maintainable enterprise applications with .NET 9 and Angular 19.

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 9">
  <img src="https://img.shields.io/badge/Angular-19-DD0031?style=for-the-badge&logo=angular" alt="Angular 19">
  <img src="https://img.shields.io/badge/TypeScript-5.6-3178C6?style=for-the-badge&logo=typescript" alt="TypeScript">
  <img src="https://img.shields.io/badge/License-MIT%20with%20Restrictions-green?style=for-the-badge" alt="License">
</p>

## 📑 Table of Contents

- [📜 License](#-license)
- [🎯 Overview](#-overview)
  - [Key Highlights](#key-highlights)
- [🚀 Features](#-features)
  - [Backend (.NET 9)](#backend-net-9)
  - [Frontend (Angular 19)](#frontend-angular-19)
- [📁 Project Structure](#-project-structure)
- [🛠️ Technology Stack](#️-technology-stack)
- [🚦 Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
- [📦 Template Installation](#-template-installation)
  - [Install as .NET Template](#install-as-net-template)
  - [Create New Project from Template](#create-new-project-from-template)
  - [Template Parameters](#template-parameters)
  - [Template Usage Examples](#template-usage-examples)
  - [Post-Installation Steps](#post-installation-steps)
  - [Installation](#installation)
  - [Development](#development)
  - [Configuration](#configuration)
- [📖 Usage](#-usage)
  - [Creating a New Entity](#creating-a-new-entity)
  - [Using Role-Based Directives](#using-role-based-directives)
- [🧪 Testing](#-testing)
- [📊 Health Checks](#-health-checks)
- [🔒 Security](#-security)
  - [Authentication Flow](#authentication-flow)
  - [Security Best Practices](#security-best-practices)
- [🤝 Contributing](#-contributing)
- [📚 Documentation](#-documentation)
- [📝 Roadmap](#-roadmap)
- [👤 Credits](#-credits)
- [🙏 Acknowledgments](#-acknowledgments)

## 📜 License

This template is **FREE to use** for your projects, but **commercial resale is restricted**. See [LICENSE.md](LICENSE.md) for details.

✅ **You CAN**: Use it for commercial projects, client work, modify it freely  
❌ **You CANNOT**: Sell or resell this template without permission

## 🎯 Overview

This template provides a production-ready foundation implementing Domain-Driven Design patterns with clean architecture principles. It combines the power of .NET 9 backend with Angular 19 frontend, offering a complete full-stack solution for enterprise applications.

### Key Highlights

- **Clean Architecture** with clear separation of concerns
- **OpenIddict** OAuth2/OpenID Connect authentication
- **Entity Framework Core** with SQL Server
- **Angular 19** with standalone components
- **Serilog** structured logging
- **Health Checks** for production monitoring
- **CORS** support for cross-origin requests
- **Role-based** access control directives

## 🚀 Features

### Backend (.NET 9)

#### Domain Layer
- ✅ Entities and Value Objects
- ✅ Aggregates and Aggregate Roots
- ✅ Domain Events with Light Event Bus
- ✅ Domain Services
- ✅ Repository Interfaces
- ❌ Specifications Pattern (Planned)

#### Application Layer
- ✅ Application Services
- ✅ DTOs and Mapping
- ❌ CQRS Pattern (Planned)
- ✅ FluentValidation Integration
- ✅ Event Handlers
- ✅ Use Case Implementation

#### Infrastructure Layer
- ✅ Entity Framework Core 9
- ✅ Generic Repository Pattern
- ✅ Unit of Work Pattern
- ✅ Database Migrations
- ✅ SQL Server Integration
- ✅ External Service Integration

#### Identity & Security
- ✅ OpenIddict Integration
- ✅ OAuth2/OpenID Connect
- ✅ JWT Token Authentication
- ✅ Role-based Authorization
- ✅ CORS Configuration
- ✅ Secure Token Storage

#### API Layer
- ✅ RESTful API Design
- ✅ Swagger/OpenAPI Documentation
- ✅ Health Check Endpoints
- ✅ Structured Logging with Serilog
- ✅ Global Exception Handling
- ✅ API Versioning Ready

### Frontend (Angular 19)

#### Core Features
- ❌ Standalone Components (Using Modules)
- ✅ OAuth2/OIDC Authentication
- ✅ Auth Guards and Interceptors
- ✅ Role-based Directives
- ✅ Reactive Forms
- ✅ State Management Ready

#### UI Components
- ✅ Responsive Landing Page
- ✅ Dashboard with Role-based Access
- ✅ Authentication Callback Handler
- ✅ Navigation with Smooth Scrolling
- ✅ Modern UI with SCSS
- ✅ Mobile-responsive Design

#### Security & Performance
- ✅ Token Refresh Management
- ✅ Silent Token Renewal
- ✅ Lazy Loading Ready
- ✅ Production Build Optimization
- ✅ PWA Ready Structure

## 📁 Project Structure

```
ddd-pattern-template/
├── 📄 LICENSE                        # MIT License with Commercial Resale Restriction
├── 📄 LICENSE.md                     # Detailed license information
├── 📄 README.md                      # Project documentation
├── 📄 DddPatternTemplate.sln         # Solution file
│
├── 📂 src/                           # Source code
│   ├── 📂 Domain/                    # Domain Layer (Core Business Logic)
│   │   ├── Entities/                 # Domain entities
│   │   ├── Events/                   # Domain events
│   │   ├── Interfaces/               # Repository and service interfaces
│   │   ├── Services/                 # Domain services
│   │   ├── Specifications/           # Specification pattern
│   │   └── ValueObjects/             # Value objects
│   │
│   ├── 📂 Domain.Shared/             # Shared Domain Constants
│   │   ├── Constants/                # Domain constants
│   │   └── Enums/                    # Domain enumerations
│   │
│   ├── 📂 Application/               # Application Layer
│   │   ├── EventHandlers/            # Domain event handlers
│   │   ├── Services/                 # Application services
│   │   └── ApplicationModule.cs      # DI configuration
│   │
│   ├── 📂 Application.Contracts/     # Application Contracts
│   │   ├── Dtos/                     # Data Transfer Objects
│   │   ├── Services/                 # Service interfaces
│   │   └── Validators/               # FluentValidation validators
│   │
│   ├── 📂 Infrastructure/            # Infrastructure Layer
│   │   ├── Repositories/             # Repository implementations
│   │   ├── Services/                 # External service implementations
│   │   └── InfrastructureModule.cs   # DI configuration
│   │
│   ├── 📂 EfCore/                    # Entity Framework Core
│   │   ├── Configurations/           # Entity configurations
│   │   ├── Migrations/               # Database migrations
│   │   ├── Repositories/             # EF repository implementations
│   │   ├── Schemas/                  # Database schemas
│   │   ├── ApplicationDataContext.cs # Main DbContext
│   │   ├── IdentityDataContext.cs    # Identity DbContext
│   │   └── EfCoreModule.cs           # DI configuration
│   │
│   ├── 📂 Identity/                  # Identity Server
│   │   ├── wwwroot/                  # Static files
│   │   ├── Program.cs                # Identity server configuration
│   │   ├── appsettings.json          # Configuration
│   │   └── appsettings.Development.json
│   │
│   ├── 📂 HttpApi/                   # HTTP API Layer
│   │   ├── Controllers/              # API controllers
│   │   ├── Filters/                  # Action filters
│   │   ├── Middleware/               # Custom middleware
│   │   ├── Services/                 # API services
│   │   └── HttpApiExtensions.cs      # API configuration
│   │
│   └── 📂 HttpApi.Host/              # API Host Project
│       ├── HealthChecks/             # Health check implementations
│       ├── ClientApp/                # Angular Application
│       │   ├── src/
│       │   │   ├── app/
│       │   │   │   ├── core/         # Core module
│       │   │   │   │   ├── services/ # Auth, API services
│       │   │   │   │   ├── guards/   # Route guards
│       │   │   │   │   └── interceptors/
│       │   │   │   ├── shared/       # Shared module
│       │   │   │   │   ├── components/
│       │   │   │   │   ├── directives/ # Role-based directives
│       │   │   │   │   └── pipes/
│       │   │   │   ├── features/     # Feature modules
│       │   │   │   │   ├── home/     # Landing page
│       │   │   │   │   ├── dashboard/ # Dashboard
│       │   │   │   │   └── auth-callback/
│       │   │   │   └── layout/       # Layout components
│       │   │   ├── assets/           # Static assets
│       │   │   ├── environments/     # Environment configs
│       │   │   └── styles/           # Global styles
│       │   ├── angular.json          # Angular configuration
│       │   ├── package.json          # NPM dependencies
│       │   └── tsconfig.json         # TypeScript configuration
│       ├── Program.cs                # Main entry point
│       ├── appsettings.json          # API configuration
│       └── appsettings.Development.json
│
└── 📂 tests/                         # Test Projects
    ├── Domain.Tests/                 # Domain unit tests
    ├── Application.Tests/            # Application unit tests
    └── HttpApi.Tests/                # API integration tests

```

## 🛠️ Technology Stack

### Backend
- **.NET 9.0** - Latest .NET framework
- **Entity Framework Core 9** - ORM
- **OpenIddict** - OAuth2/OpenID Connect server
- **SQL Server** - Database
- **Serilog** - Structured logging
- **FluentValidation** - Input validation
- **Fast Endpoints** - RESTful API Engine


### Frontend
- **Angular 19** - Frontend framework
- **TypeScript 5.6** - Type-safe JavaScript
- **RxJS** - Reactive programming
- **SCSS** - Styling
- ❌ **Design System** (Not implemented. use your preferred design system)
- **OIDC Client** - OAuth2/OIDC authentication

### DevOps & Tools
- **Docker** - Containerization ready
- ❌ **GitHub Actions** (Not implemented)
- **Swagger/OpenAPI** - API documentation
- **Scaler** - OpenAPI Explorer
- **Health Checks** - Production monitoring

## 🚦 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server) or [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Angular CLI](https://angular.io/cli) `npm install -g @angular/cli`

## 📦 Template Installation

### Install as .NET Template

1. **Install from NuGet (When Published)**
   ```bash
   dotnet new install DDD.Pattern.Template
   ```

2. **Install from Local Source**
   ```bash
   # Clone the repository
   git clone https://github.com/EngRslan/ddd-pattern-template.git
   cd ddd-pattern-template
   
   # Install the template locally
   dotnet new install .
   ```

### Create New Project from Template

```bash
# Create with default settings
dotnet new ddd-template -n YourProjectName

# Create with custom options
dotnet new ddd-template -n YourProjectName \
  --UseAngular true \
  --UseDocker true \
  --IncludeSampleCode true \
  --UseIdentity true \
  --EnableHealthChecks true \
  --skipRestore false
```

### Template Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `--UseAngular` | bool | `true` | Include Angular frontend application with OIDC authentication, dashboard, and role-based directives |
| `--UseDocker` | bool | `true` | Include Docker support with Dockerfile and docker-compose configurations |
| `--IncludeSampleCode` | bool | `true` | Include sample entities, services, DTOs, and validators to demonstrate DDD patterns |
| `--UseIdentity` | bool | `true` | Include OpenIddict Identity Server for OAuth2/OpenID Connect authentication |
| `--EnableHealthChecks` | bool | `true` | Enable health check endpoints for production monitoring |
| `--skipRestore` | bool | `true` | Skip automatic restore of NuGet and npm packages during project creation |

### Template Usage Examples

#### Full-Featured Application (Default)
```bash
dotnet new ddd-template -n MyApp
```
Creates a complete DDD application with Angular frontend, Identity Server, Docker support, sample code, and health checks.

#### API-Only (No Frontend)
```bash
dotnet new ddd-template -n MyApi --UseAngular false
```
Creates a backend-only API without the Angular frontend.

#### Clean Template (No Samples)
```bash
dotnet new ddd-template -n MyApp --IncludeSampleCode false
```
Creates the template structure without sample entities and services.

#### Minimal API (No Identity, No Docker)
```bash
dotnet new ddd-template -n MyMinimalApi \
  --UseAngular false \
  --UseDocker false \
  --UseIdentity false \
  --IncludeSampleCode false
```
Creates a minimal DDD API without authentication, frontend, or containerization.

#### Auto-Restore Dependencies
```bash
dotnet new ddd-template -n MyApp --skipRestore false
```
Automatically restores NuGet packages and runs npm install after project creation.

### Post-Installation Steps

After creating a project from the template:

1. **Update the namespace** - The template will rename `CertManager` to your project name automatically
2. **Configure connection strings** - Update database connections in appsettings.json files
3. **Run database migrations** - Apply EF Core migrations to create the database schema
4. **Configure CORS** - Update allowed origins for your environment
5. **Start development** - Run Identity Server and API Host projects

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/EngRslan/ddd-pattern-template.git
   cd ddd-pattern-template
   ```

2. **Setup the database**
   ```bash
   # Update connection strings in appsettings.json files
   # Run migrations for Application DbContext
   cd src/HttpApi.Host
   dotnet ef database update -c ApplicationDataContext
   
   # Run migrations for Identity DbContext
   cd src/Identity
   dotnet ef database update -c IdentityDataContext
   ```

3. **Install backend dependencies**
   ```bash
   dotnet restore
   ```

4. **Install frontend dependencies**
   ```bash
   cd src/HttpApi.Host/ClientApp
   npm install
   ```

5. **Run the Identity Server**
   ```bash
   cd src/Identity
   dotnet run
   # Identity server will run on https://localhost:{{generated}}
   ```

6. **Run the API Host**
   ```bash
   cd src/HttpApi.Host
   dotnet run
   # API will run on https://localhost:{{generated}}
   # Angular app will run on http://localhost:4200
   ```

### Development

#### Backend Development
```bash
# Run with hot reload
dotnet watch run --project src/HttpApi.Host

# Run tests
dotnet test

# Add migration
dotnet ef migrations add MigrationName -c ApplicationDataContext -p src/EfCore -s src/HttpApi.Host

# Update database
dotnet ef database update -c ApplicationDataContext -p src/EfCore -s src/HttpApi.Host
```

#### Frontend Development
```bash
cd src/HttpApi.Host/ClientApp

# Development server
npm start

# Run tests
npm test

# Build for production
npm run build

# Lint
npm run lint
```

### Configuration

#### Connection Strings
Update connection strings in:
- `src/HttpApi.Host/appsettings.json`
- `src/Identity/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DddTemplate;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

#### CORS Settings
Configure allowed origins in `appsettings.json`:
```json
{
  "AllowedOrigins": ["http://localhost:4200", "https://yourdomain.com"]
}
```

## 📖 Usage

### Creating a New Entity

1. **Define the entity in Domain layer**
```csharp
public class Product : Entity<Guid>
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    
    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
```

2. **Create repository interface**
```csharp
public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product> GetByNameAsync(string name);
}
```

3. **Implement repository in Infrastructure layer**
```csharp
public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    public async Task<Product> GetByNameAsync(string name)
    {
        return await FirstOrDefaultAsync(p => p.Name == name);
    }
}
```

### Using Role-Based Directives

```html
<!-- Show only for Admin role -->
<div *appHasRole="'Admin'">
  Admin only content
</div>

<!-- Show for multiple roles -->
<button *appHasAnyRole="['Admin', 'Manager']">
  Management Action
</button>

<!-- Hide from specific roles -->
<div *appHasNotRole="'Guest'">
  Premium content
</div>
```

## 🧪 Testing

### Unit Tests
```bash
dotnet test tests/Domain.Tests
dotnet test tests/Application.Tests
```

### Integration Tests
```bash
dotnet test tests/HttpApi.Tests
```

### Frontend Tests
```bash
cd src/HttpApi.Host/ClientApp
npm test
npm run test:coverage
```

## 📊 Health Checks

The application includes comprehensive health checks:

- `/health` - Overall health status
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

Access health check UI at: `https://localhost:5002/health`

## 🔒 Security

### Authentication Flow
1. User clicks login → Redirected to Identity Server
2. User authenticates → Identity Server issues tokens
3. Tokens stored in browser → Used for API calls
4. Silent renewal → Tokens refreshed automatically

### Security Best Practices
- ✅ HTTPS enforced in production
- ✅ CORS properly configured
- ✅ JWT tokens with expiration
- ✅ Role-based authorization
- ✅ Input validation with FluentValidation
- ✅ SQL injection protection with EF Core
- ✅ XSS protection in Angular

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### How to Contribute

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines

- Follow DDD principles and clean architecture
- Write unit tests for new features
- Update documentation as needed 
- Follow existing code style and conventions
- Ensure all tests pass before submitting PR

## 📚 Documentation

- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [OpenIddict Documentation](https://documentation.openiddict.com/)
- [Angular Documentation](https://angular.io/docs)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

## 📝 Roadmap

- [ ] Add GraphQL support
- [ ] Implement event sourcing
- [ ] Add message queue integration (RabbitMQ/Azure Service Bus)
- [ ] Add Blazor WebAssembly alternative frontend
- [ ] Implement caching with Redis
- [ ] Add Docker Compose configuration
- [ ] Create CI/CD pipelines
- [ ] Add multi-tenancy support

## 👤 Credits

**Developed by Muhammad Raslan**

- GitHub: [@EngRslan](https://github.com/EngRslan)
- Email: [eng.m.rslan@hotmail.com]

## 📄 License

This project is licensed under the **MIT License with Commercial Resale Restriction**.

- ✅ **Free to use** for personal and commercial projects
- ✅ **Free to modify** and customize as needed  
- ✅ **Free to use** for client projects
- ❌ **Cannot be resold** as a template or product without permission

See the [LICENSE](LICENSE) file for full details.

## 🙏 Acknowledgments

### Architecture Inspiration
- **File organization and structure inspired by [ABP Framework](https://abp.io/)** - A complete infrastructure to create modern web applications
- Clean Architecture principles by Robert C. Martin (Uncle Bob)
- Domain-Driven Design concepts by Eric Evans

### External Dependencies

#### Backend Libraries
- **[OpenIddict](https://github.com/openiddict/openiddict-core)** - Flexible OpenID Connect server framework
- **[Entity Framework Core](https://github.com/dotnet/efcore)** - Modern object-database mapper for .NET
- **[Serilog](https://github.com/serilog/serilog)** - Simple .NET logging with structured events
- **[FluentValidation](https://github.com/FluentValidation/FluentValidation)** - Popular .NET validation library

#### Frontend Libraries
- **[Angular](https://github.com/angular/angular)** - Platform for building mobile and desktop apps
- **[angular-oauth2-oidc](https://github.com/manfredsteyer/angular-oauth2-oidc)** - OAuth2 and OpenID Connect (OIDC) client
- **[RxJS](https://github.com/ReactiveX/rxjs)** - Reactive Extensions Library for JavaScript
- **[TypeScript](https://github.com/microsoft/TypeScript)** - Typed superset of JavaScript

### Special Thanks
- Thanks to all contributors who have helped shape this template
- The .NET and Angular communities for continuous support and inspiration
- Built with ❤️ for the developer community

---

<p align="center">
  Made with ❤️ by Muhammad Raslan
</p>

<p align="center">
  <a href="#-ddd-pattern-template">Back to top ↑</a>
</p>