using Microsoft.AspNetCore.Mvc;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = ex switch
        {
            UnauthorizedAccessException => 401,
            InvalidOperationException => 400,
            KeyNotFoundException => 404,
            _ => 500,
        };

        var problem = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = GetTitle(ex),
            Detail = ex.Message,
        };

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static string GetTitle(Exception ex) =>
        ex switch
        {
            UnauthorizedAccessException => "Unauthorized",
            InvalidOperationException => "Bad request",
            KeyNotFoundException => "Not found",
            _ => "Server error",
        };
}
