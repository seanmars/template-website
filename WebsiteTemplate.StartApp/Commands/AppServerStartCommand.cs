// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace WebsiteTemplate.StartApp.Commands;

[Description("Start the game server.")]
public sealed class AppServerStartCommand : AsyncCommand<AppServerStartCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("The environment to run the game server.")]
        [CommandOption("--env <ENV>")]
        [DefaultValue("Production")]
        public required string Environment { get; init; }

        [Description("The port to run the game server.")]
        [CommandOption("-p|--port <PORT>")]
        [DefaultValue(5000)]
        public required int Port { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var env = settings.Environment;
        var port = settings.Port.ToString();

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", env);
        // 因為 appsettings 有可能會直接設定 http_port，故使用 HTTP_PORTS 來設定
        Environment.SetEnvironmentVariable("HTTP_PORTS", port);

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
        table.AddRow("Port", port);

        AnsiConsole.Write(table);

        var app = AppServerFactory.Create();

        await app.RunAsync();

        // Start the game server
        return 0;
    }
}