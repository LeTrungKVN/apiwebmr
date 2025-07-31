using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace WebAppMR.Services
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
        private readonly ConcurrentDictionary<string, WebSocketMessageDispatcher> _dispatchers = new();

        public void AddSocket(string userId, WebSocket socket)
        {
            _sockets[userId] = socket;
        }

        public void AddDispatcher(string userId, WebSocketMessageDispatcher dispatcher)
        {
            _dispatchers[userId] = dispatcher;
        }

        public void RemoveDispatcher(string userId)
        {
            _dispatchers.TryRemove(userId, out _);
        }

        public WebSocketMessageDispatcher? GetDispatcher(string userId)
        {
            _dispatchers.TryGetValue(userId, out var dispatcher);
            return dispatcher;
        }

        public WebSocket? GetSocket(string userId)
        {
            _sockets.TryGetValue(userId, out var socket);
            return socket;
        }
        public WebSocket? GetSocketFirstOrDefault()
        {
            return _sockets.Values.FirstOrDefault();
        }
        public IEnumerable<WebSocket> GetAllSockets() => _sockets.Values;
        public void RemoveSocket(string userId)
        {
            _sockets.TryRemove(userId, out _);
            RemoveDispatcher(userId);
        }

        public IEnumerable<string> GetAllUserIds() => _sockets.Keys;
    }
}