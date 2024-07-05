namespace Server.RateLimiter;

public class RateLimiterOptions
{
    public int RequestLimit { get; set; }
    public TimeSpan TimeFrame { get; set; }
}