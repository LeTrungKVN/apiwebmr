using System.Net.WebSockets;
using System.Text;

namespace WebAppMR.Controller
{
    public class WebSocketController
    {
        public async Task Handle(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await HandleWebSocketAsync(context, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("WebSocket connection required.");
            }
        }

        private async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
        {
            // Gửi thông tin chào mừng về phía client khi kết nối thành công
            string welcome = "WebSocket connected. Send 'ping' or 'echo: <msg>'";
            var welcomeBytes = Encoding.UTF8.GetBytes(welcome);
            await webSocket.SendAsync(new ArraySegment<byte>(welcomeBytes), WebSocketMessageType.Text, true, CancellationToken.None);

            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var start = DateTime.UtcNow;
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                string response = ProcessWebSocketCommand(message);
                var responseBytes = Encoding.UTF8.GetBytes(response);
                var sendStart = DateTime.UtcNow;
                await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), result.MessageType, result.EndOfMessage, CancellationToken.None);
                var sendEnd = DateTime.UtcNow;

                // Tính tốc độ truyền và độ trễ
                double latencyMs = (sendStart - start).TotalMilliseconds;
                double bytesPerSec = responseBytes.Length / ((sendEnd - sendStart).TotalSeconds + 0.0001); // tránh chia cho 0

                string info = $"[Server] Latency: {latencyMs:F2} ms, Speed: {bytesPerSec:F2} bytes/s";
                var infoBytes = Encoding.UTF8.GetBytes(info);
                await webSocket.SendAsync(new ArraySegment<byte>(infoBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private string ProcessWebSocketCommand(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Empty message";
            if (message.Equals("ping", StringComparison.OrdinalIgnoreCase))
                return "pong";
            if (message.StartsWith("echo:", StringComparison.OrdinalIgnoreCase))
                return message[5..].Trim();
            return $"Unknown command: {message}";
        }
    }
}
