using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

public class ExceptionMiddleware
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _env = env;
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            await handleSpecificException(context, ex, (int)HttpStatusCode.Forbidden);
        }
        catch (BadHttpRequestException ex)
        {
            await handleSpecificException(context, ex, (int)HttpStatusCode.BadRequest);
        }
        catch (KeyNotFoundException ex)
        {
            await handleSpecificException(context, ex, (int)HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private async Task handleSpecificException(HttpContext context, Exception ex, int StatusCode)
    {
        _logger.LogError(ex, "{ExceptionType} exception occurred: {Message}", ex.GetType().ToString(), ex.Message);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCode;

        var response = new CustomException(context.Response.StatusCode, ex.Message, ex.GetType().ToString());
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }


    private async Task HandleException(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "An unexpected exception occurred: {Message}", ex.Message);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = _env.IsDevelopment()
        ? new CustomException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
        : new CustomException(context.Response.StatusCode, ex.Message, "Internal Server Error");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }

}