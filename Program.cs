using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StaticFileSecureCall;
using StaticFileSecureCall.DataManagement;
using StaticFileSecureCall.Services;
using System;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("FileConnection");

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IKeyGenerator, KeyMaster>();
        builder.Services.AddDbContextPool<AppDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("OtherConnection")));
        builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))); //autodetect version
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddLogging();
        IConfiguration configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .Build();
        var authorizedIpAddresses = configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>(); //register authorized Ip addreses

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

        app.UseAuthorization();

        //app.UseEndpoints();

        app.MapControllers();

        app.Run();
    }
}