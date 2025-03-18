namespace automatizador_cards_anki.api.Middleware
{
    public class LogHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogHandlingMiddleware> _logger;

        public LogHandlingMiddleware(RequestDelegate next,
            ILogger<LogHandlingMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Request information path {body}", context.Request.Path);

            await _next(context);

            _logger.LogInformation("Completed request {Result}", context.Response);
        }        
    }

    public static class RequestLogHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLog(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogHandlingMiddleware>();
        }
    }
}
