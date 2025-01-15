using System.ComponentModel;
using WebsiteTemplate.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace WebsiteTemplate.StartApp.Commands;

[Description("Reset the database.")]
public class DatabaseResetCommand : AsyncCommand<DatabaseResetCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("The environment to reset the database.")]
        [CommandOption("--env <ENV>")]
        [DefaultValue("Production")]
        public required string Environment { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
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

        AnsiConsole.MarkupLine("[bold red]資料庫重置會刪除所有資料![/]");
        var doReset = AnsiConsole.Confirm("確定要重置資料庫嗎?", false);

        if (!doReset)
        {
            AnsiConsole.MarkupLine("[bold]取消重置資料庫[/]");
            return 0;
        }

        var app = AppServerFactory.CreateForDatabaseMigration();

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        AnsiConsole.MarkupLine("[bold yellow]開始重置資料庫[/]");
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
        AnsiConsole.MarkupLine("[bold green]資料庫重置完成[/]");

        return 0;
    }
}