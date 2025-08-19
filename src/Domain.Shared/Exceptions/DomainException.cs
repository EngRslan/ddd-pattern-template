namespace CertManager.Domain.Shared.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class ValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors) 
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message) 
        : base($"Validation error: {message}")
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { message } } };
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key) 
        : base($"{entity} with id '{key}' was not found.")
    {
    }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException() : base("Unauthorized access.")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }
}

public class ForbiddenException : DomainException
{
    public ForbiddenException() : base("Access forbidden.")
    {
    }

    public ForbiddenException(string message) : base(message)
    {
    }
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }
}