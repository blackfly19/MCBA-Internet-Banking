namespace MCBA_AdminWebApi.Services;

public static class TokenStore
{
    // Thread-safe collection to store valid tokens
    private static readonly HashSet<string> ValidTokens = new();
    private static readonly object Lock = new();

    public static void AddToken(string token)
    {
        lock (Lock)
        {
            ValidTokens.Add(token);
        }
    }

    public static bool IsValidToken(string token)
    {
        lock (Lock)
        {
            return ValidTokens.Contains(token);
        }
    }

    public static void RemoveToken(string token)
    {
        lock (Lock)
        {
            ValidTokens.Remove(token);
        }
    }
}