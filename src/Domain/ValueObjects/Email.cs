using System.Text.RegularExpressions;
using Engrslan.Domain.Shared.Errors;
using ErrorOr;

namespace Engrslan.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ErrorOr<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return DomainErrors.Validation.Required(nameof(Email));

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(email))
            return DomainErrors.Validation.InvalidEmail();

        if (email.Length > 255)
            return DomainErrors.Validation.MaxLength(nameof(Email), 255);

        return new Email(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}