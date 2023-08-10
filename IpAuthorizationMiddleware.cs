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
            var authorizedIpAddresses = _configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>();

            var remoteIpAddress = context.Connection.RemoteIpAddress;
            if (authorizedIpAddresses.Contains(remoteIpAddress.ToString()))
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
