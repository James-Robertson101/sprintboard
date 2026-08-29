using System.Net;
using System.Text.Json;
using SprintBoard.Api.Exceptions;

namespace SprintBoard.Api.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions anywhere in the pipeline and returns
    /// a consistent JSON error response instead of leaking stack traces.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var statusCode = ex switch
            {
                NotFoundException => HttpStatusCode.NotFound,
                ForbiddenException => HttpStatusCode.Forbidden,
                ArgumentException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = context.Response.StatusCode,
                message = statusCode == HttpStatusCode.InternalServerError
                    ? "An unexpected error occurred."
                    : ex.Message,
                // Only include details in development to avoid leaking internals in production
                detail = _env.IsDevelopment() ? ex.ToString() : null
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }

    // Convenience extension so it reads nicely in Program.cs:
    // app.UseGlobalExceptionMiddleware();
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}