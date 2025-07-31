using System.Net.WebSockets;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WebAppMR.Controller;
using WebAppMR.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<WebSocketController>();
builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<WebSocketMessageDispatcher>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


// Enable WebSockets
app.UseWebSockets();

app.MapControllers();


app.MapWhen(
    context => context.Request.Path == "/ws",
    appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var wsManager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();
                var userId = Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(userId))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Missing userId");
                    return;
                }
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine($"WebSocket connection established for user: {userId} + State: {socket.State}");
                wsManager.AddSocket(userId, socket);

                // Tạo dispatcher riêng cho từng userId
                var dispatcher = new WebAppMR.Services.WebSocketMessageDispatcher();
                wsManager.AddDispatcher(userId, dispatcher);

                // Đợi client đóng kết nối
                var buffer = new byte[1024 * 4];
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        wsManager.RemoveSocket(userId);
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received from client: {message}");
                        dispatcher.SetResponse(message);
                    }
                }
            }
            else
            {
                Console.WriteLine("WebSocket connection required.");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });
    });
// app.UseHttpsRedirection();

app.Run();


