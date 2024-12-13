using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VaultSharp;
using CatalogServiceAPI.Models;
using CatalogServiceAPI.Services;
using CatalogServiceAPI.Interfaces;
using CatalogServiceAPI.Data;
using NLog;
using NLog.Web;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    // Konfiguration af Vault Service
    var vaultService = new VaultService(configuration);
    string mySecret = await vaultService.GetSecretAsync("secrets", "SecretKey") ?? "????";
    string myIssuer = await vaultService.GetSecretAsync("secrets", "IssuerKey") ?? "????";
    string myConnectionString = await vaultService.GetSecretAsync("secrets", "MongoConnectionString") ?? "????";

    // Indsæt hemmeligheder og forbindelsesoplysninger i konfigurationen
    configuration["SecretKey"] = mySecret;
    configuration["IssuerKey"] = myIssuer;
    configuration["MongoConnectionString"] = myConnectionString;

    Console.WriteLine("Issuer: " + myIssuer);
    Console.WriteLine("Secret: " + mySecret);
    Console.WriteLine("MongoConnectionString: " + myConnectionString);

    if (string.IsNullOrEmpty(myConnectionString))
    {
        logger.Error("ConnectionString not found in environment variables.");
        throw new Exception("ConnectionString not found in environment variables.");
    }
    else
    {
        logger.Info("ConnectionString: {0}", myConnectionString);
    }

    // Tilføj services til containeren
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddControllers();
    builder.Services.AddTransient<VaultService>();
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(myConnectionString));
    builder.Services.AddSingleton<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(configuration["DatabaseName"]);
    });

    builder.Services.AddSingleton<ICatalogInterface, CatalogMongoDBService>();

    // Konfiguration af JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = myIssuer,
            ValidAudience = "http://localhost",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mySecret)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                    logger.Error("Token expired: {0}", context.Exception.Message);
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // Seed data til MongoDB
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var mongoDBContext = services.GetRequiredService<MongoDBContext>();

        try
        {
            logger.Info("Seeding data to Catalog database...");
            await mongoDBContext.SeedDataAsync();
            logger.Info("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error occurred during data seeding.");
        }
    }

    // Konfigurer HTTP-pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapControllers();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception.");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
