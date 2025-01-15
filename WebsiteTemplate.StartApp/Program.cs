using WebsiteTemplate.StartApp.Commands;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console.Cli;

Log.Logger = new LoggerConfiguration()
    .Destructure.ToMaximumDepth(3)
    .WriteTo.Console()
    .CreateLogger();

try
{
    var cliApp = new CommandApp();
    cliApp.Configure(config =>
    {
        config.SetApplicationName("SausageApp");

        config.AddBranch("serve", cmd =>
        {
            cmd.SetDescription("The Server command");

            cmd.AddCommand<AppServerCreateConfigCommand>("config");
            cmd.AddCommand<AppServerStartCommand>("start");
        });

        config.AddBranch("db", cmd =>
        {
            cmd.SetDescription("The Database command");

            cmd.AddCommand<DatabaseMigrateApplyCommand>("migrate");
            cmd.AddCommand<DatabaseResetCommand>("reset");
        });
    });

    await cliApp.RunAsync(args);
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