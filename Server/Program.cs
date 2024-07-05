using Server.RateLimiter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// TODO : Consider reading from appsettings
builder.Services.AddRequestsTimeFrameRateLimiter(options =>
{
    options.RequestLimit = 5;
    options.TimeFrame = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

app.UseRequestsTimeFrameRateLimiter();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
