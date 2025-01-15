using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.IdentityModel.Logging;
using Serilog;
using WebsiteTemplate.Core.Authentication;
using WebsiteTemplate.Core.Config;
using WebsiteTemplate.Core.Data;
using WebsiteTemplate.Core.Middlewares;
using WebsiteTemplate.Core.Models;

namespace WebsiteTemplate.Core.Extensions;

public static class AppBuilder
{
    public static WebApplicationBuilder Validate(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder.Configuration["Jwt:Key"], "Jwt:Key");
        var jwtKeyLength = builder.Configuration["Jwt:Key"]!.Length * 8;
        if (jwtKeyLength < 256)
        {
            throw new ArgumentException("Jwt:Key length must be at least 256 bits (32 bytes)");
        }

        return builder;
    }

    public static WebApplication AddAppServer<TDbContext>(this WebApplicationBuilder builder,
        Action<AppServerDbContextOptions>? dbContextOptionsAction = null)
        where TDbContext : AppDbContext
    {
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        builder.Services.AddOptions<JwtOption>()
            .BindConfiguration("Jwt");

        builder.Services.AddSerilog();

        var gameServerDbContextOptions = new AppServerDbContextOptions();
        dbContextOptionsAction?.Invoke(gameServerDbContextOptions);
        builder.Services.AddDbContext<TDbContext>(
            gameServerDbContextOptions.OptionsAction,
            gameServerDbContextOptions.ContextLifetime,
            gameServerDbContextOptions.OptionsLifetime);

        builder.Services
            .AddIdentity<AppIdentityUser, AppIdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        builder.Services.AddAuthenticationJwtBearer(
                signingOptions =>
                {
                    signingOptions.SigningKey = builder.Configuration["Jwt:Key"];
                },
                jwtBearerOptions =>
                {
                    jwtBearerOptions.TokenValidationParameters.ValidateIssuer = true;
                    jwtBearerOptions.TokenValidationParameters.ValidateAudience = false;
                    jwtBearerOptions.TokenValidationParameters.ValidateLifetime = true;
                })
            .AddAuthorization()
            .AddFastEndpoints();

        builder.Services.AddSingleton<JwtHelper>();
        builder.Services.AddScoped<JwtValidateMiddleware>();
        builder.Services.AddScoped<RefreshTokenService>();
        builder.Services.AddScoped<TokenService>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

#if NET8_0
        builder.Services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.CustomSchemaIds(x => x.FullName);
        });
#endif

        return builder.Build();
    }

    public static WebApplication UseAppServer(this WebApplication app)
    {
        IdentityModelEventSource.ShowPII = true;

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            IdentityModelEventSource.LogCompleteSecurityArtifact = true;
#if NET8_0
            app.UseSwagger();
            app.UseSwaggerUI();
#elif NET9_0_OR_GREATER
#endif
        }

        // app.UseHttpsRedirection();
        // app.UseStaticFiles();

        // app.UseCors()

        app.UseMiddleware<JwtValidateMiddleware>();

        app.UseAuthentication()
            .UseAuthorization()
            .UseFastEndpoints(config =>
            {
                config.Endpoints.RoutePrefix = "api";
            });

#if NET8_0
        app.UseSwagger();
#endif

        return app;
    }
}