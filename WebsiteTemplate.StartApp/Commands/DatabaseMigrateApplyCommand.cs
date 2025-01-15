using System.ComponentModel;
using WebsiteTemplate.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace WebsiteTemplate.StartApp.Commands;

[Description("Apply the database migration.")]
public class DatabaseMigrateApplyCommand : AsyncCommand<DatabaseMigrateApplyCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("The environment to apply the database migration.")]
        [CommandOption("--env <ENV>")]
        [DefaultValue("Production")]
        public required string Environment { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var env = settings.Environment;
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", env);

            var envColor = env switch
            {
                "Development" => Color.Yellow,
                "Staging" => Color.Aqua,
                "Production" => Color.Green,
                _ => Color.Default
            };
            var table = new Table();
            table.AddColumn("Environment")
                .AddColumn(new TableColumn(new Text(env, new Style(envColor)).Centered()));

            AnsiConsole.Write(table);

            var app = AppServerFactory.CreateForDatabaseMigration();

            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
            if (!pendingMigrations.Any())
            {
                AnsiConsole.MarkupLine("[bold green]沒有待轉移的 Migration[/]");
                return 0;
            }

            AnsiConsole.MarkupLine("[bold]待轉移的 Migration:[/]");
            foreach (var pending in pendingMigrations)
            {
                var txt = new Text(pending);
                var padText = new Padder(txt).PadLeft(4).PadBottom(0).PadTop(0);
                AnsiConsole.Write(padText);
            }

            await dbContext.Database.MigrateAsync();
        }
        finally
        {
            AnsiConsole.MarkupLine("[bold green]Migration 結束[/]");
        }

        return 0;
    }
}