using FoodHub.Menu.Domain.Exceptions;

namespace FoodHub.Menu.Domain.ValueObjects;

public record Price
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Price(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainException("Price amount cannot be negative.");
        }
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency cannot be empty.");
        }

        Amount = amount;
        Currency = currency;
    }
}
