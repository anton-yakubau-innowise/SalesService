namespace SalesService.API.Configuration;

public class CookieSettings
{
    public required CookieSetting LastCustomerId { get; set; }
}

public class CookieSetting
{
    public string Key { get; set; } = string.Empty;
    public double ExpirationHours { get; set; }
}