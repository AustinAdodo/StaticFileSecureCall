using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.AspNetCore.Hosting;
using Amazon.SimpleEmail;
using System.IO;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
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
using Microsoft.Extensions.FileProviders;
using System.Security;
using Microsoft.Data.SqlClient;
using AspNetCore.Authentication.ApiKey;

internal class Program
{
    /// <summary>
    /// ***********NB:IMiddleware Interface was not implmented because that will require 
    /// ***********the inheriting middleware to be registered as transient service.
    /// ************ For load-balanced API Cache to handle rate limiting when app becomes large use Redis.
    /// ************ static file Middleware must be placed before app.UserRouting().
    /// </summary>
    /// <param name="args"></param>

    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //CreateHostBuilder(args).Build().Run();

        //setup configuration and Environment
        var CurrentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{CurrentEnvironment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        string connectionString = string.Empty;
        if (CurrentEnvironment == Environments.Development)
        {
            var authorizedIpAddresses = configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>();
            string? pass = Environment.GetEnvironmentVariable("DB_PASSWORD").ToString();
            var devConnection = builder.Configuration.GetConnectionString("FileConnection").ToString();
            SqlConnectionStringBuilder ConnStrbuilder = new SqlConnectionStringBuilder(devConnection);
            ConnStrbuilder.Password = pass;
            var securePassword = new SecureString();
            if (ConnStrbuilder != null) connectionString = ConnStrbuilder.ToString();
        }

        //AWS ConnectionString Configurations options.
        if (CurrentEnvironment == Environments.Production)
        {
            try
            {
                var awsOptions = configuration.GetAWSOptions();
                using var secretsManagerClient = new AmazonSecretsManagerClient(awsOptions.Region);
                var secretName = "StaticFileSecureCall";
                var request = new GetSecretValueRequest
                {
                    SecretId = secretName
                };
                var response = await secretsManagerClient.GetSecretValueAsync(request);
                if (response.SecretString == null) throw new Exception("Secret string is empty or null.");
                var secret = response.SecretString; ///
                var secretObject = JsonSerializer.Deserialize<Dictionary<string, string>>(secret);
                var password = secretObject["password"];
                //Update the connection string in IConfiguration
                configuration["ConnectionStrings:FileConnection"] = configuration["ConnectionStrings:FileConnection"]?.Replace("__PASSWORD__", password);
                connectionString = configuration["ConnectionStrings:FileConnection"].ToString();
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
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = ApiKeyDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = ApiKeyDefaults.AuthenticationScheme;
        })
         .AddApiKeyInHeaderOrQueryParams<ApiKeyProvider>(options =>
        {
        options.Realm = "Your API";
        options.KeyName = "x-api-key"; // The header or query parameter name for the API key
        });
    
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiKeyPolicy", policy =>
            {
                policy.AddAuthenticationSchemes(ApiKeyDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });
        });
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

        app.UseStaticFiles(); //The default directory is {content root}/wwwroot, but it can be changed with the UseWebRoot method

        app.UseCors();

        app.UseRouting();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        //app.UseEndpoints(endpoints =>{ endpoints.MapControllers(); });

        app.MapControllers();

        app.Run();
    }
}

//Configure Web Host builder.
//    public static IHostBuilder CreateHostBuilder(string[] args) =>
//       Host.CreateDefaultBuilder(args)
//           .ConfigureWebHostDefaults(webBuilder =>
//           {
//               webBuilder.Configure(app =>
//               {
//                   // Configure the host settings, services, and hosting environment

//               });
//           });
//}




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

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//           Path.Combine(builder.Environment.ContentRootPath, "MyStaticFiles")),
//    RequestPath = "/StaticFiles"
//});
