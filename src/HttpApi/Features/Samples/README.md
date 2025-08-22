# Sample Secure Endpoints

This directory contains sample endpoints demonstrating OpenIddict validation and authentication.

## Authentication Setup

The API uses OpenIddict validation with introspection to validate tokens issued by the Identity Server.

### Configuration

The following settings are configured in `appsettings.json`:

```json
{
  "OpenIddict": {
    "Issuer": "https://localhost:5001/",
    "ClientId": "certmanager-api",
    "ClientSecret": "certmanager-api-secret-2024"
  }
}
```

### How It Works

1. **Token Validation**: When a request comes with a Bearer token, the API validates it using introspection
2. **Introspection**: The API calls the Identity Server's introspection endpoint to verify the token
3. **Claims**: Valid tokens provide user claims including roles and permissions

## Sample Endpoints

### 1. Public Health Check
- **Endpoint**: `GET /api/secure/health`
- **Authentication**: None required
- **Description**: Public endpoint for health monitoring

### 2. User Information
- **Endpoint**: `GET /api/secure/userinfo`
- **Authentication**: Required (any authenticated user)
- **Description**: Returns current user's claims and information

### 3. Admin Only
- **Endpoint**: `GET /api/secure/admin`
- **Authentication**: Required with Admin role
- **Description**: Demonstrates role-based authorization

## Testing the Endpoints

### 1. Get an Access Token

First, authenticate with the Identity Server to get an access token:

```bash
# Using the Angular app (recommended)
# 1. Navigate to http://localhost:4200
# 2. Click Login
# 3. Authenticate with Identity Server
# 4. Token is automatically stored and used

# Or use direct API call (for testing)
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=certmanager-api&client_secret=certmanager-api-secret-2024&scope=certmanager-api"
```

### 2. Test Public Endpoint

```bash
curl https://localhost:5002/api/secure/health
```

### 3. Test Protected Endpoint

```bash
curl https://localhost:5002/api/secure/userinfo \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 4. Test Admin Endpoint

```bash
curl https://localhost:5002/api/secure/admin \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

## Creating Your Own Secure Endpoints

### Basic Protected Endpoint

```csharp
public class MyProtectedEndpoint : EndpointWithoutRequest<MyResponse>
{
    public override void Configure()
    {
        Get("/api/my-endpoint");
        RequireAuthorization(); // Requires authentication
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var username = HttpContext.User.Identity?.Name;
        // Your logic here
    }
}
```

### Role-Based Endpoint

```csharp
[Authorize(Roles = "Manager,Admin")]
public class ManagerEndpoint : EndpointWithoutRequest<MyResponse>
{
    public override void Configure()
    {
        Get("/api/manager-only");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Only users with Manager or Admin role can access
    }
}
```

### Policy-Based Endpoint

```csharp
[Authorize(Policy = "RequireEmailVerified")]
public class VerifiedEmailEndpoint : EndpointWithoutRequest<MyResponse>
{
    public override void Configure()
    {
        Get("/api/verified-only");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Only users with verified email can access
    }
}
```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**: Token is missing, expired, or invalid
2. **403 Forbidden**: User authenticated but lacks required role/permission
3. **500 Internal Server Error**: Check if Identity Server is running and accessible

### Debug Tips

1. Enable debug logging in `appsettings.Development.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "OpenIddict": "Debug"
      }
    }
  }
}
```

2. Check Identity Server is running on `https://localhost:5001`
3. Verify client credentials match between API and Identity Server
4. Ensure HTTPS is used in production