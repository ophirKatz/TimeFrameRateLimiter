using Server.RateLimiter;

namespace Server;

// Using Startup because it is reused in tests
public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var section = _configuration.GetSection(nameof(RateLimiterOptions));
        services.AddRequestsTimeFrameRateLimiter(options => section.Bind(options));
        services.AddSingleton(TimeProvider.System);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRequestsTimeFrameRateLimiter();
    }
}