# DDD Pattern Template

A .NET template for creating Domain-Driven Design (DDD) pattern applications with Clean Architecture.

## Features

- ✅ Clean Architecture with DDD patterns
- ✅ ASP.NET Core Web API
- ✅ Entity Framework Core with multiple database provider support
- ✅ Angular frontend (optional)
- ✅ Docker support (optional)
- ✅ FluentValidation for request validation
- ✅ Serilog for structured logging
- ✅ Repository and Unit of Work patterns
- ✅ CQRS ready structure

## Installation

### Install from local source
```bash
dotnet new install .
```

### Install from NuGet (after publishing)
```bash
dotnet new install DDD.Pattern.Template
```

## Usage

### Create a new project with default settings
```bash
dotnet new ddd-template -n MyProject
```

### Create a project with specific options
```bash
dotnet new ddd-template -n MyProject --Framework net8.0 --DatabaseProvider PostgreSQL --UseAngular false
```

### Available Parameters

| Parameter | Description | Default | Options |
|-----------|-------------|---------|---------|
| `--Framework` | Target .NET framework | net9.0 | net9.0, net8.0, net7.0, net6.0 |
| `--UseAngular` | Include Angular frontend | true | true, false |
| `--UseDocker` | Include Docker support | true | true, false |
| `--UseEfCore` | Include Entity Framework Core | true | true, false |
| `--DatabaseProvider` | Database provider to use | SqlServer | SqlServer, PostgreSQL, MySql, Sqlite |
| `--IncludeSampleCode` | Include sample entities and services | false | true, false |
| `--skipRestore` | Skip package restore on creation | false | true, false |

## Project Structure

```
YourProjectName/
├── YourProjectName.Domain/              # Domain layer (Entities, Value Objects, Domain Services)
├── YourProjectName.Domain.Shared/       # Shared domain concerns
├── YourProjectName.Application/         # Application layer (Use Cases, Application Services)
├── YourProjectName.Application.Contracts/ # DTOs and Application interfaces
├── YourProjectName.EfCore/             # Infrastructure layer (EF Core implementation)
├── YourProjectName.HttpApi/            # HTTP API layer (Controllers, Middleware)
├── YourProjectName.Host/               # Host application (Program.cs, Startup)
│   └── ClientApp/                      # Angular frontend (if enabled)
└── YourProjectName.sln                 # Solution file
```

## Building the NuGet Package

To create a NuGet package for distribution:

```bash
# Pack the template
nuget pack DDD.Pattern.Template.nuspec

# Or using dotnet CLI
dotnet pack
```

## Publishing to NuGet

```bash
dotnet nuget push DDD.Pattern.Template.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Uninstalling the Template

```bash
dotnet new uninstall DDD.Pattern.Template
```

## Development

### Testing the Template Locally

1. Install the template from the current directory:
   ```bash
   dotnet new install .
   ```

2. Create a test project:
   ```bash
   dotnet new ddd-template -n TestProject
   ```

3. Verify the generated project:
   ```bash
   cd TestProject
   dotnet build
   ```

### Updating the Template

After making changes to the template:

1. Uninstall the old version:
   ```bash
   dotnet new uninstall .
   ```

2. Install the updated version:
   ```bash
   dotnet new install .
   ```

## License

MIT

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.