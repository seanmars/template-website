namespace WebsiteTemplate.Core.Models;

public class BlockedRefreshToken
{
    public long Id { get; set; }
    public required string Token { get; set; }
    public required DateTime ExpiryDate { get; set; }
}