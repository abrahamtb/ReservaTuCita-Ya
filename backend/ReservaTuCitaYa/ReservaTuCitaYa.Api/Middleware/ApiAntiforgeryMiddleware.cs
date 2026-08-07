using Microsoft.AspNetCore.Antiforgery;

namespace ReservaTuCitaYa.Api.Middleware;

public sealed class ApiAntiforgeryMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> SafeMethods =
        new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "OPTIONS", "TRACE" };

    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        if (context.Request.Path.StartsWithSegments("/api") &&
            !SafeMethods.Contains(context.Request.Method))
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                await Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Token antiforgery inválido",
                    detail: "Solicita un token antiforgery válido y envíalo en X-XSRF-TOKEN.",
                    instance: context.Request.Path)
                    .ExecuteAsync(context);
                return;
            }
        }

        await next(context);
    }
}
