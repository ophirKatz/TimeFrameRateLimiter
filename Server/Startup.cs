using Server.RateLimiter;

namespace Server;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // TODO : Consider reading from appsettings
        services.AddRequestsTimeFrameRateLimiter(options =>
        {
            options.RequestLimit = 5;
            options.TimeFrame = TimeSpan.FromSeconds(5);
        });

        services.AddSingleton(TimeProvider.System);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRequestsTimeFrameRateLimiter();
    }
}