namespace WebsiteTemplate.Core.Models;

public class BlockedAccessToken
{
    public required string Token { get; set; }
    public required DateTime ExpiryDate { get; set; }
}