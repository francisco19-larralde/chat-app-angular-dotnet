using ChatApp.Application.Interfaces;
using System.Collections.Concurrent;

namespace ChatApp.Infrastructure.Services;


public class InMemoryUserConnectionTracker : IUserConnectionTracker
{

    private readonly ConcurrentDictionary<int, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    public bool AddConnection(int userId, string connectionId)
    {
        lock (_lock)
        {
            var isFirstConnection = !_connections.ContainsKey(userId);

            if (!_connections.TryGetValue(userId, out var set))
            {
                set = new HashSet<string>();
                _connections[userId] = set;
            }

            set.Add(connectionId);
            return isFirstConnection;
        }
    }

    public bool RemoveConnection(int userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out var set))
                return false;

            set.Remove(connectionId);

            if (set.Count == 0)
            {
                _connections.TryRemove(userId, out _);
                return true;
            }

            return false;
        }
    }

    public bool IsUserOnline(int userId)
    {
        return _connections.ContainsKey(userId);
    }
}