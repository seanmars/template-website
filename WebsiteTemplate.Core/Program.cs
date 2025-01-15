using Microsoft.EntityFrameworkCore;
using Serilog;
using WebsiteTemplate.Core.Data;
using WebsiteTemplate.Core.Extensions;

static WebApplication CreateWebApp(bool validate = true)
{
    var builder = WebApplication.CreateBuilder();

    if (validate)
    {
        builder.Validate();
    }

    builder.AddAppServer<AppDbContext>(options =>
    {
        options.OptionsAction = (optionsBuilder) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlite(connectionString);
        };
    });

    return builder.Build();
}

Log.Logger = new LoggerConfiguration()
    .Destructure.ToMaximumDepth(3)
    .WriteTo.Console()
    .CreateLogger();


try
{
    var app = CreateWebApp();
    app.UseAppServer();

    await app.RunAsync();
}
catch (HostAbortedException)
{
    // Ignore
    // Throw HostAbortedException when using EF CLI.
    // see more details: https://github.com/dotnet/efcore/issues/28478
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}