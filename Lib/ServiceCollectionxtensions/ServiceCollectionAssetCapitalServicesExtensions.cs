using System.Configuration;

namespace StaticFileSecureCall.Lib.ServiceCollectionxtensions
{
    public static class ServiceCollectionAssetCapitalServicesExtensions
    {
        public static IServiceCollection AddAssetCapitalBusinessServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IPersistence, PersistenceService>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddInMemoryRateLimiting();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddTransient<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();
            services.AddTransient<ICredentialService, CredentialManager>();
            services.AddTransient<IKeyGenerator, KeyMaster>();
            services.AddScoped<IPersistence, PersistenceService>();
            services.AddScoped<IMailDeliveryService, MailManager>();
            return services;
        }
    }
}
