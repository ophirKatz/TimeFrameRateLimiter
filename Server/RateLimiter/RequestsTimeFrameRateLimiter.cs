using System.Collections.Concurrent;

namespace Server.RateLimiter;

// Using record structs for immutability and easy-copying
public readonly record struct RequestsTimeFrame(DateTimeOffset StartTime, int Count);

public class RequestsTimeFrameRateLimiter : IRateLimiter
{
    private readonly RateLimiterOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<string, RequestsTimeFrame> _timeFrames = new();

    public RequestsTimeFrameRateLimiter(RateLimiterOptions options, TimeProvider timeProvider)
    {
        _options = options;
        _timeProvider = timeProvider;
    }

    public bool LimitClientRequestsById(string clientId)
    {
        var isLimitExceeded = false;
        _timeFrames.AddOrUpdate(
            clientId,
            new RequestsTimeFrame(_timeProvider.GetUtcNow(), 1),
            (_, timeFrame) =>
            {
                var currentTime = _timeProvider.GetUtcNow();
                if (currentTime - _options.TimeFrame > timeFrame.StartTime)
                {
                    // Start a new time frame
                    return new RequestsTimeFrame(currentTime, 1);
                }

                var frameWithRequest = timeFrame with { Count = timeFrame.Count + 1 };
                isLimitExceeded = frameWithRequest.Count >= _options.RequestLimit;
                return frameWithRequest;
            }
        );

        return isLimitExceeded;
    }
}