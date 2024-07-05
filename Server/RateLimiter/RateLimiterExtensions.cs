namespace Server.RateLimiter;

public static class RateLimiterExtensions
{
    public static void AddRequestsTimeFrameRateLimiter(this IServiceCollection services,
        Action<RateLimiterOptions> optionsBuilder)
    {
        var options = new RateLimiterOptions();
        optionsBuilder(options);
        services.AddSingleton<IRateLimiter>(new RequestsTimeFrameRateLimiter(options));
    }

    public static void UseRequestsTimeFrameRateLimiter(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var clientId = context.Request.Query["clientId"];
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var rateLimiter = context.RequestServices.GetRequiredService<IRateLimiter>();
                context.Response.StatusCode = rateLimiter.LimitClientRequestsById(clientId!)
                    ? StatusCodes.Status503ServiceUnavailable
                    : StatusCodes.Status200OK;
                await context.Response.WriteAsync($"Client {clientId} Request Handled");
            }

            // Naturally calling next operations in the pipeline which will return a 404 since none exist
            await next(context);
        });
    }
}