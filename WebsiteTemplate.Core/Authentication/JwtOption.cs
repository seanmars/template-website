namespace WebsiteTemplate.Core.Authentication;

public class JwtOption
{
    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public int ExpiryDays { get; set; }
    public int RefreshExpiryDays { get; set; }
}