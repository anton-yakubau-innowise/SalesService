using FluentAssertions;
using SalesService.Domain.Common;

namespace SalesService.UnitTests.Domain;

public class GuardTests
{
    [Fact]
    public void AgainstEmptyGuid_WhenValueIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        Guid emptyGuid = Guid.Empty;

        // Act
        Action act = () => Guard.AgainstEmptyGuid(emptyGuid, nameof(emptyGuid));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AgainstEmptyGuid_WhenValueIsNotEmpty_DoesNotThrow()
    {
        // Arrange
        Guid nonEmptyGuid = Guid.NewGuid();

        // Act
        Action act = () => Guard.AgainstEmptyGuid(nonEmptyGuid, nameof(nonEmptyGuid));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AgainstNull_WhenValueIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        object? value = null;

        // Act
        Action act = () => Guard.AgainstNull(value, nameof(value));

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AgainstNull_WhenValueIsNotNull_DoesNotThrow()
    {
        // Arrange
        object value = new();

        // Act
        Action act = () => Guard.AgainstNull(value, nameof(value));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AgainstStringLength_WhenValueIsNotExactLength_ThrowsArgumentException()
    {
        // Arrange
        string value = "test";
        int exactLength = 5;

        // Act
        Action act = () => Guard.AgainstStringLength(value, exactLength, nameof(value));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AgainstStringLength_WhenValueIsExactLength_DoesNotThrow()
    {
        // Arrange
        string value = "test";
        int exactLength = 4;

        // Act
        Action act = () => Guard.AgainstStringLength(value, exactLength, nameof(value));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AgainstStringLength_WhenValueIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;
        int exactLength = 4;

        // Act
        Action act = () => Guard.AgainstStringLength(value!, exactLength, nameof(value));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1, 0, 10)]
    [InlineData(11, 0, 10)]
    public void AgainstOutOfRange_Int_WhenArgumentIsOutOfRange_ThrowsArgumentOutOfRangeException(int value, int from, int to)
    {
        // Arrange
        string paramName = nameof(value);

        // Act
        Action act = () => Guard.AgainstOutOfRange(value, from, to);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be(paramName);
    }

    [Theory]
    [InlineData(0, 0, 10)] 
    [InlineData(10, 0, 10)]
    [InlineData(5, 0, 10)]
    public void AgainstOutOfRange_Int_WhenArgumentIsInRange_DoesNotThrow(int value, int from, int to)
    {
        // Arrange

        // Act
        Action act = () => Guard.AgainstOutOfRange(value, from, to);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AgainstNegative_Int_WhenArgumentIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int negativeValue = -10;
        string paramName = nameof(negativeValue);

        // Act
        Action act = () => Guard.AgainstNegative(negativeValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be(paramName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void AgainstNegative_Int_WhenArgumentIsZeroOrPositive_DoesNotThrow(int nonNegativeValue)
    {
        // Arrange

        // Act
        Action act = () => Guard.AgainstNegative(nonNegativeValue);

        // Assert
        act.Should().NotThrow();
    }


    [Fact]
    public void AgainstNegative_Decimal_WhenArgumentIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        decimal negativeValue = -25.5m;
        string paramName = nameof(negativeValue);

        // Act
        Action act = () => Guard.AgainstNegative(negativeValue);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be(paramName);
    }

    [Theory]
    [InlineData("0.0")]
    [InlineData("0.1")]
    [InlineData("12345.67")]
    public void AgainstNegative_Decimal_WhenArgumentIsZeroOrPositive_DoesNotThrow(string nonNegativeValueStr)
    {
        // Arrange
        var nonNegativeValue = decimal.Parse(nonNegativeValueStr, System.Globalization.CultureInfo.InvariantCulture);

        // Act
        Action act = () => Guard.AgainstNegative(nonNegativeValue);

        // Assert
        act.Should().NotThrow();
    }

    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("US")]
    [InlineData("USDE")]
    [InlineData("usd")]
    [InlineData("U$D")]
    public void AgainstInvalidCurrencyCodeFormat_WithInvalidFormat_ThrowsArgumentException(string invalidCurrencyCode)
    {
        // Arrange
        string paramName = nameof(invalidCurrencyCode);

        // Act
        Action act = () => Guard.AgainstInvalidCurrencyCodeFormat(invalidCurrencyCode);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be(paramName);
    }

    [Fact]
    public void AgainstInvalidCurrencyCodeFormat_WithValidFormat_DoesNotThrow()
    {
        // Arrange
        var validCurrencyCode = "USD";

        // Act
        Action act = () => Guard.AgainstInvalidCurrencyCodeFormat(validCurrencyCode);

        // Assert
        act.Should().NotThrow();
    }
}