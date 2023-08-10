using Microsoft.Extensions.Configuration;
using StaticFileSecureCall;
using StaticFileSecureCall.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        IConfiguration configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .Build();
        var authorizedIpAddresses = configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>(); //register authorized Ip addreses
        builder.Services.AddScoped<IKeyGenerator, KeyMaster>();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<IpAuthorizationMiddleware>();//custom Middlewar registered.

        app.UseRouting();

        app.UseStaticFiles();//static files included.

        app.UseHttpsRedirection();

        app.UseAuthorization();

        //app.UseEndpoints();

        app.MapControllers();

        app.Run();
    }
}