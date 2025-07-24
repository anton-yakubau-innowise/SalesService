using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SalesService.Domain.Common
{
    public static class Guard
    {
        public static void AgainstEmptyGuid(Guid argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument == Guid.Empty)
            {
                throw new ArgumentException("Identifier cannot be empty.", paramName);
            }
        }

        public static void AgainstNull<T>(T? argument, [CallerArgumentExpression("argument")] string? paramName = null) where T : class
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName, "Parameter cannot be null.");
            }
        }

        public static void AgainstNullOrWhiteSpace(string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException("String parameter cannot be null or whitespace.", paramName);
            }
        }

        public static void AgainstStringLength(string argument, int exactLength, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            AgainstNullOrWhiteSpace(argument, paramName);
            if (argument.Length != exactLength)
            {
                throw new ArgumentException($"Parameter must be exactly {exactLength} characters long.", paramName);
            }
        }
        
        public static void AgainstOutOfRange(int argument, int min, int max, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument < min || argument > max)
            {
                throw new ArgumentOutOfRangeException(paramName, $"Parameter is out of valid range ({min}-{max}).");
            }
        }

        public static void AgainstNegative(int argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, "Parameter cannot be negative.");
            }
        }

        public static void AgainstNegative(decimal argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, "Parameter cannot be negative.");
            }
        }

        public static void AgainstInvalidCurrencyCodeFormat(string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            AgainstNullOrWhiteSpace(argument, paramName);
            if (argument!.Length != 3 || !Regex.IsMatch(argument, @"^[A-Z]{3}$"))
            {
                throw new ArgumentException("Currency code must be 3 uppercase letters (e.g., USD, EUR).", paramName);
            }
        }
    }
}