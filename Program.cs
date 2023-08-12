using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleEmail;
using Microsoft.Extensions.Configuration;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
using StaticFileSecureCall;
using StaticFileSecureCall.DataManagement;
using StaticFileSecureCall.Services;
using System;
 
internal class Program
{
    /// <summary>
    /// ***********NB:IMiddleware Interface was not implmented because that will require 
    /// ***********the inheriting middleware to be registered as transient service.
    /// </summary>
    /// <param name="args"></param>
    
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("FileConnection");

        //setup configuration and Environment
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        var authorizedIpAddresses = configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>(); //register authorized Ip addreses

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRateLimiter(LimitPolicy => //Rate Limiting Policy Name : Caution
        {
            LimitPolicy.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            LimitPolicy.AddFixedWindowLimiter("caution", window => { window.Window = TimeSpan.FromSeconds(1800); window.PermitLimit = 6; });
        });
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IKeyGenerator, KeyMaster>();
        builder.Services.AddScoped<IPersistence, PersistenceService>();
        builder.Services.AddScoped<IMailDeliveryService, MailManager>();
        builder.Services.AddDbContextPool<AppDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("OtherConnection")));
        builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))); //autodetect version
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddLogging();
        // Configure AWS services
        builder.Services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonSimpleEmailService>();
       
        //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //.AddJwtBearer(options =>
        //{
        //    // Configure the JWT authentication settings
        //    // For example: options.TokenValidationParameters = ...
        //});

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<IpAuthorizationMiddleware>();//custom Middleware registered.

        app.UseRouting();

        app.UseStaticFiles();//static files included.

        app.UseHttpsRedirection();

        //app.UseRateLimiter();

        app.UseAuthorization();

        //app.UseEndpoints();

        app.MapControllers();//.RequireRateLimiting("Caution"); //Rate Limit policy Integrated.

        app.Run();
    }
}