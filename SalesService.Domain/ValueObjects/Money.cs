using SalesService.Domain.Common;

namespace SalesService.Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money()
        {
            Amount = 0;
            Currency = string.Empty;
        }

        public Money(decimal amount, string currency)
        {
            Guard.AgainstNegative(amount);
            Guard.AgainstInvalidCurrencyCodeFormat(currency);

            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }

        public override bool Equals(object? obj)
        {
            if (obj is null || GetType() != obj.GetType())
                return false;
            Money other = (Money)obj;
            return Amount == other.Amount && Currency == other.Currency;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public override string ToString()
        {
            return $"{Amount} {Currency}";
        }

        public Money Add(Money other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other), "Cannot add null Money object.");
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot add Money with different currencies.");

            return new Money(Amount + other.Amount, Currency);
        }
    }
}