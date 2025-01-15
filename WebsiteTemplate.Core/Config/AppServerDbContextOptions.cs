using Microsoft.EntityFrameworkCore;

namespace WebsiteTemplate.Core.Config;

public class AppServerDbContextOptions
{
    public Action<DbContextOptionsBuilder>? OptionsAction { get; set; } = null;
    public ServiceLifetime ContextLifetime { get; set; } = ServiceLifetime.Scoped;
    public ServiceLifetime OptionsLifetime { get; set; } = ServiceLifetime.Scoped;
}