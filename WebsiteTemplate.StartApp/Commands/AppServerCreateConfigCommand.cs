// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using System.Text.Json;
using WebsiteTemplate.Core.Authentication;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebsiteTemplate.StartApp.Commands;

[Description("Create a game server configuration file.")]
public class AppServerCreateConfigCommand : AsyncCommand<AppServerCreateConfigCommand.Settings>
{
    public sealed class ConnectionStrings
    {
        public required string DefaultConnection { get; set; }
    }

    public sealed class Config
    {
        public required ConnectionStrings ConnectionStrings { get; set; }
        public required JwtOption Jwt { get; set; }
    }

    public class Settings : CommandSettings
    {
        [Description("Prints configuration to the console.")]
        [CommandOption("--print")]
        [DefaultValue(false)]
        public bool Print { get; init; }
    }

    public async Task<bool> CheckDatabaseConnection(string connectionString)
    {
        try
        {
            throw new NotImplementedException();
            // var conn = new MySqlConnection(connectionString);
            // await conn.OpenAsync();
            return true;
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Error: {e.Message}[/]");
            return false;
        }
    }

    private async Task<string> RequireDatabaseInfo()
    {
        var isValid = false;
        var connectionString = string.Empty;
        while (!isValid)
        {
            var dbServer = AnsiConsole.Ask("請輸入資料庫伺服器位置:", "127.0.0.1");
            var dbPort = AnsiConsole.Ask("請輸入資料庫連接埠:", 3306);
            var dbName = AnsiConsole.Ask("請輸入資料庫名稱:", "sausage_gs");
            var dbUser = AnsiConsole.Ask("請輸入資料庫使用者名稱:", "root");
            var dbPassword = AnsiConsole.Prompt(
                new TextPrompt<string>("請輸入資料庫密碼:")
                    .Secret('*'));

            connectionString = $"Server={dbServer};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPassword};";
            var doCheck = AnsiConsole.Confirm("是否確認資料庫連線資訊?", false);
            if (!doCheck)
            {
                break;
            }

            isValid = await CheckDatabaseConnection(connectionString);
            if (isValid)
            {
                AnsiConsole.MarkupLine("[green]測試資料庫連線成功[/]");
            }
        }

        return connectionString;
    }

    private JwtOption RequireJwtOption()
    {
        var issuer = AnsiConsole.Ask("請輸入 JWT 發行者:", "The issuer");
        var key = AnsiConsole.Prompt(
            new TextPrompt<string>("請輸入 JWT 金鑰(最少 32 個字元)[[空白會自動產生]]:")
                .AllowEmpty()
        );
        var expiryDays = AnsiConsole.Ask("請輸入 JWT 過期天數:", 14);
        var refreshExpiryDays = AnsiConsole.Ask("請輸入 JWT 的 Refresh Token 過期天數:", 10);

        if (string.IsNullOrWhiteSpace(key))
        {
            key = JwtHelper.GenerateJwtSecret();
        }

        return new JwtOption
        {
            Key = key,
            Issuer = issuer,
            ExpiryDays = expiryDays,
            RefreshExpiryDays = refreshExpiryDays
        };
    }

    private void PrintConfig(Config config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        AnsiConsole.Write(
            new Panel(new JsonText(json))
                .Header("設定檔")
                .Collapse()
                .RoundedBorder()
                .BorderColor(color: Color.Yellow)
        );
    }

    private Task SaveConfig(Config config, string path)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return File.WriteAllTextAsync(path, json);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var availableEnvironments = new[]
        {
            Environments.Development,
            Environments.Staging,
            Environments.Production
        };

        var selectedEnv = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("請選擇環境")
                .AddChoices(availableEnvironments)
        );

        var connectionStrings = new ConnectionStrings
        {
            DefaultConnection = await RequireDatabaseInfo()
        };

        var jwtOption = RequireJwtOption();

        var config = new Config
        {
            ConnectionStrings = connectionStrings,
            Jwt = jwtOption,
        };

        if (settings.Print)
        {
            PrintConfig(config);
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{selectedEnv}.json");
        if (Path.Exists(path))
        {
            var overwrite = AnsiConsole.Confirm("設定檔已存在，是否覆蓋?", false);
            if (!overwrite)
            {
                return 0;
            }
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Default)
            .StartAsync("正在儲存設定檔...", async _ =>
            {
                await SaveConfig(config, path);
                AnsiConsole.MarkupLine("[green]設定檔儲存成功[/]");
            });

        return 0;
    }
}