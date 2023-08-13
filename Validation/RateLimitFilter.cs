using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;


namespace StaticFileSecureCall.Validation
{
    public class RateLimitFilter : IActionFilter
    {
        private readonly IIpPolicyStore _ipPolicyStore;
        private readonly IRateLimitCounterStore _counterStore;
        private readonly ILogger<RateLimitFilter> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPAddress? _remoteIpAddress;
        private readonly string ipAddress;

        public RateLimitFilter(
            IIpPolicyStore ipPolicyStore,
            IRateLimitCounterStore counterStore,
            ILogger<RateLimitFilter> logger,
            IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            _ipPolicyStore = ipPolicyStore ?? throw new ArgumentNullException(nameof(ipPolicyStore));
            _counterStore = counterStore ?? throw new ArgumentNullException(nameof(counterStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _remoteIpAddress = _contextAccessor.HttpContext?.Connection.RemoteIpAddress;
            ipAddress = _remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
           ? _remoteIpAddress.MapToIPv4().ToString()
           : _remoteIpAddress.ToString();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var endpoint = context.ActionDescriptor.EndpointMetadata
                .OfType<ControllerActionDescriptor>()
                .FirstOrDefault();
            if (endpoint == null)
            {
                return;
            }
            var policy = _ipPolicyStore.GetAsync($"{endpoint.ControllerName}_{endpoint.ActionName}");
            if (policy != null)
            {
                var counter = _counterStore.GetAsync(ipAddress, policy);
                if (counter == null)
                {
                    _counterStore.SetAsync(ipAddress, policy, TimeSpan.FromSeconds(policy.PeriodInSeconds));
                }
                else if (counter.Timestamp + TimeSpan.FromSeconds(policy.PeriodInSeconds) < DateTime.UtcNow)
                {
                    _counterStore.SetAsync(ipAddress, policy, TimeSpan.FromSeconds(policy.PeriodInSeconds));
                }
                else if (counter.TotalRequests > policy.Limit)
                {
                    _logger.LogInformation($"Rate limit exceeded for IP address {ipAddress}. Limit: {policy.Limit}");
                    httpContext.Response.StatusCode = policy.HttpStatusCode;
                    httpContext.Response.Headers["Retry-After"] = counter.Timestamp.Add(TimeSpan.FromSeconds(policy.PeriodInSeconds)).ToString("r");
                    context.Result = new ContentResult
                    {
                        Content = "Rate limit exceeded. Please try again later.",
                        StatusCode = policy.HttpStatusCode
                    };
                }
            }
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var httpContext = context.HttpContext;
            var endpoint = context.ActionDescriptor.EndpointMetadata
                .OfType<ControllerActionDescriptor>()
                .FirstOrDefault();
            if (endpoint == null)
            {
                return;
            }
            var policy = _ipPolicyStore.GetPolicy($"{endpoint.ControllerName}_{endpoint.ActionName}");
            if (policy != null)
            {
                _counterStore.Increment(ipAddress, policy);
            }
        }
    }

}
