using CertManager.Domain.Shared.Errors;
using ErrorOr;

namespace CertManager.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static ErrorOr<Money> Create(decimal amount, string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return DomainErrors.Validation.Required(nameof(Currency));

        if (currency.Length != 3)
            return DomainErrors.Validation.InvalidInput(nameof(Currency), "Currency must be a 3-letter ISO code");

        if (amount < 0)
            return DomainErrors.Validation.InvalidInput(nameof(Amount), "Amount cannot be negative");

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency) => new(0, currency.ToUpperInvariant());

    public ErrorOr<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return DomainErrors.Business.InvalidOperation($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public ErrorOr<Money> Subtract(Money other)
    {
        if (Currency != other.Currency)
            return DomainErrors.Business.InvalidOperation($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        if (Amount < other.Amount)
            return DomainErrors.Business.InvalidOperation("Insufficient funds");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        return left.Amount <= right.Amount;
    }
}