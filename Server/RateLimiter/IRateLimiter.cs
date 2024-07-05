namespace Server.RateLimiter;

public interface IRateLimiter
{
    /// <summary>
    /// Limits client requests by clientId.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns>Returns true if request limit was exceeded. false otherwise</returns>
    bool LimitClientRequestsById(string clientId);
}