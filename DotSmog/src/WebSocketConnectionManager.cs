using System.Net.WebSockets;
using System.Text;
using DotSmog.src;

namespace DotSmog;

public class WebSocketConnectionManager
{
    private static readonly List<WebSocket> sockets = new();
    
    public async static void AddSocket(WebSocket socket)
    {
        sockets.Add(socket);
        await Receive(socket);
    }
    
    private async static Task Receive(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            try
            {   
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    // sockets.Remove(webSocket);
                }
            }
            catch (Exception ex)
            {
                // Log exception or handle the error gracefully
                Console.WriteLine($"Error receiving data: {ex.Message}");
                break;
            }
        }
    }
    
    public static void RemoveSocket(WebSocket socket)
    {
        sockets.Remove(socket);
    }
    
    public static IEnumerable<WebSocket> GetAllSockets()
    {
        return sockets.Where(s => s.State == WebSocketState.Open);
    }
    
    public static async Task SendToWebSockets(String sensorMessage)
    {
        var buffer = Encoding.UTF8.GetBytes(sensorMessage);
        foreach (var socket in sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine($"Sent socket: {sensorMessage}");
            }
        }
    }
}