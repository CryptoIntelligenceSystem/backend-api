using System.Net;
using System.Text.Json;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;

namespace CryptoIntelligence.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException exception)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (ClickHouseException exception)
        {
            logger.LogError(exception, "ClickHouse request failed");
            await WriteErrorAsync(
                context,
                HttpStatusCode.ServiceUnavailable,
                "ClickHouse không khả dụng hoặc truy vấn thất bại.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API exception");
            await WriteErrorAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Đã xảy ra lỗi ngoài dự kiến.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var body = new
        {
            error = new
            {
                statusCode = (int)statusCode,
                message,
                traceId = context.TraceIdentifier
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
