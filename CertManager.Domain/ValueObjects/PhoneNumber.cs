using System.Text.RegularExpressions;
using CertManager.Domain.Shared.Errors;
using ErrorOr;

namespace CertManager.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ErrorOr<PhoneNumber> Create(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return DomainErrors.Validation.Required(nameof(PhoneNumber));

        phoneNumber = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (!PhoneRegex.IsMatch(phoneNumber))
            return DomainErrors.Validation.InvalidPhoneNumber();

        if (phoneNumber.Length > 15)
            return DomainErrors.Validation.MaxLength(nameof(PhoneNumber), 15);

        return new PhoneNumber(phoneNumber);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}