using ErrorOr;

namespace Engrslan.Domain.Shared.Errors;

public static class DomainErrors
{
    public static class General
    {
        public static Error NotFound(string entity, object id) =>
            Error.NotFound("General.NotFound", $"{entity} with id '{id}' was not found");

        public static Error Unauthorized() =>
            Error.Unauthorized("General.Unauthorized", "You are not authorized to perform this action");

        public static Error Forbidden() =>
            Error.Forbidden("General.Forbidden", "Access to this resource is forbidden");
    }

    public static class Validation
    {
        public static Error InvalidInput(string field, string reason) =>
            Error.Validation("Validation.InvalidInput", $"'{field}' is invalid: {reason}");

        public static Error Required(string field) =>
            Error.Validation("Validation.Required", $"'{field}' is required");

        public static Error MaxLength(string field, int maxLength) =>
            Error.Validation("Validation.MaxLength", $"'{field}' must not exceed {maxLength} characters");

        public static Error MinLength(string field, int minLength) =>
            Error.Validation("Validation.MinLength", $"'{field}' must be at least {minLength} characters");

        public static Error InvalidEmail() =>
            Error.Validation("Validation.InvalidEmail", "Email address is not valid");

        public static Error InvalidPhoneNumber() =>
            Error.Validation("Validation.InvalidPhoneNumber", "Phone number is not valid");
    }

    public static class Business
    {
        public static Error DuplicateEntity(string entity, string field, object value) =>
            Error.Conflict("Business.DuplicateEntity", $"{entity} with {field} '{value}' already exists");

        public static Error InvalidOperation(string reason) =>
            Error.Failure("Business.InvalidOperation", reason);

        public static Error ConcurrencyConflict() =>
            Error.Conflict("Business.ConcurrencyConflict", "The record has been modified by another user");
    }
}