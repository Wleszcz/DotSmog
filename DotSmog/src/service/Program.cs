using System.Net.WebSockets;
using System.Numerics;
using DotSmog;
using DotSmog.service;
using DotSmog.src;
using MongoDB.Bson;

var dbConnector = DBConnector.Instance;


var queueConnector = new QueueConnector();
var tokenService = new TokenService();
using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true; // Prevent the process from terminating.
    cancellationTokenSource.Cancel();
};

queueConnector.StartReceiving(cancellationTokenSource.Token);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseWebSockets();

app.MapGet("/socket", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        WebSocketConnectionManager.AddSocket(webSocket);

        Console.WriteLine("WebSocket connected");
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});


app.MapGet("/api/readings", async (string? type, DateTime? date, string? stationId) =>
    {
        var documents = await dbConnector.GetDataAsync(QueueConnector.collectionName, type, date, stationId);
        Messages messages = new Messages
        {
            SensorMessages = documents
        };
        return Results.Ok(messages);
    })
    .WithName("readings")
    .WithOpenApi();


app.MapGet("/api/balance/{accountId}", async (string accountId) =>
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Results.BadRequest("AccountId cannot be empty.");
        }
        
        try
        {
            var balance = await tokenService.GetBalance(accountId);
            return Results.Ok(balance);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        
    })
    .WithName("balance")
    .WithOpenApi();

app.Run();
