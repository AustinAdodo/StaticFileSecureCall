using Microsoft.EntityFrameworkCore;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.AspNetCore.Hosting;
using Amazon.SimpleEmail;
using System.IO;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
using StaticFileSecureCall;
using StaticFileSecureCall.DataManagement;
using StaticFileSecureCall.Services;
using System;
using AspNetCoreRateLimit;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.ComponentModel.Design;
using System.Data.Entity;

internal class Program
{
    /// <summary>
    /// ***********NB:IMiddleware Interface was not implmented because that will require 
    /// ***********the inheriting middleware to be registered as transient service.
    /// ************ For load-balanced API Cache to handle rate limiting when app becomes large use Redis.
    /// </summary>
    /// <param name="args"></param>

    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //setup configuration and Environment
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        var CurrentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = string.Empty;
        if (CurrentEnvironment == Environments.Development)
        {
        var authorizedIpAddresses = configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>();
            string? dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")?.ToString();//.Trim('"')
        var devConnection= builder.Configuration.GetConnectionString("FileConnection")?.Replace("__DB_PASSWORD__", dbPassword);
        if (devConnection != null) connectionString = devConnection;
        }

        //AWS Configurations options.
        if (CurrentEnvironment == Environments.Production)
        {
        try
        {
            var awsOptions = configuration.GetAWSOptions();
            using var secretsManagerClient = new AmazonSecretsManagerClient(awsOptions.Region);
            var secretName = "my-secret-name";
            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };
            var response = await secretsManagerClient.GetSecretValueAsync(request);
            if (response.SecretString == null) throw new Exception("Secret string is empty or null.");
            var secret = response.SecretString; ///
            var secretObject = JsonSerializer.Deserialize<Dictionary<string, string>>(secret);
            var password = secretObject["password"];
            // Update the connection string in IConfiguration
            //configuration["ConnectionStrings:FileConnection"] = configuration["ConnectionStrings:FileConnection"]?.Replace("__PASSWORD__", password);
        }
        catch (AmazonSecretsManagerException ex)
        {
            Console.WriteLine($"AWS Secrets Manager exception: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        }

        // Add services to the container. Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        //Configure Rate Limiting Policy and  rate limiting options
        builder.Services.AddOptions();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        builder.Services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        builder.Services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        builder.Services.AddInMemoryRateLimiting();
        builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

        //Add Swagger and other Services to Pipeline.
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache(); 
        builder.Services.AddTransient<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();
        builder.Services.AddScoped<ICredentialService, CredentialManager>();
        builder.Services.AddScoped<IKeyGenerator, KeyMaster>();
        builder.Services.AddScoped<IPersistence, PersistenceService>();
        builder.Services.AddScoped<IMailDeliveryService, MailManager>();
        builder.Services.AddDbContextPool<AppDbContext>(options =>
   options.UseSqlServer(connectionString));
        builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect("OtherConnection")));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddLogging();

        // Configure AWS services
        builder.Services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonSimpleEmailService>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<IpAuthorizationMiddleware>();

        app.UseCors();

        app.UseRouting();

        app.UseStaticFiles();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        //app.UseEndpoints(endpoints =>{ endpoints.MapControllers(); });

        app.MapControllers();

        app.MapControllers();

        app.Run();
    }
}




//[In Consideration]
//services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
// ...
//builder.Services.AddRateLimiter(LimitPolicy => //Rate Limiting Policy Name : Caution
//{
//    LimitPolicy.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
//    LimitPolicy.AddFixedWindowLimiter("caution", window => { window.Window = TimeSpan.FromSeconds(1800); window.PermitLimit = 6; });
//});

// Configure json Web Tokens
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//.AddJwtBearer(options =>
//{
//    // Configure the JWT authentication settings
//    // For example: options.TokenValidationParameters = ...
//});
