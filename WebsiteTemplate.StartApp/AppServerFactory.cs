using WebsiteTemplate.Core.Data;
using WebsiteTemplate.Core.Extensions;
using Microsoft.AspNetCore.Builder;

namespace WebsiteTemplate.StartApp;

public static class AppServerFactory
{
    public static WebApplication Create(bool validate = true)
    {
        var builder = WebApplication.CreateBuilder();

        if (validate)
        {
            builder.Validate();
        }

        builder.AddAppServer<AppDbContext>();

        return builder.Build();
    }

    public static WebApplication CreateForDatabaseMigration()
    {
        return Create(false);
    }
}