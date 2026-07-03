using Application.Constants;

namespace WebApi.Middleware;

public class ForcePasswordChangeMiddleware
{
    private readonly RequestDelegate _next;

    public ForcePasswordChangeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var requiresPasswordChange = user.HasClaim(c =>
                c.Type == CustomClaimType.ForceChangePassword && c.Value == "true");

            if (requiresPasswordChange)
            {
                var path = context.Request.Path.Value?.ToLower();
                var isAllowedPath = path != null &&
                    (path.Contains("/api/auth/change-password") ||
                     path.Contains("/api/auth/logout"));

                if (!isAllowedPath)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"PasswordUpdateRequired\", \"message\": \"You must change your password to continue.\"}");

                    return;
                }
            }
        }

        await _next(context);
    }
}
