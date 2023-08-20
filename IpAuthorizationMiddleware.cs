namespace StaticFileSecureCall
{
    public class IpAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public IpAuthorizationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            string[]? authorizedIpAddresses = _configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>();
            var remoteIpAddress = context.Connection.RemoteIpAddress;
            string? formattedIpAddress = remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6
                ? remoteIpAddress.MapToIPv4().ToString() // Convert IPv6 to IPv4 format
                : remoteIpAddress.ToString();
            if (authorizedIpAddresses.Contains(formattedIpAddress))
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = 403; // Forbidden
            }
        }
    }
}
