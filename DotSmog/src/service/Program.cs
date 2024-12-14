using System.Net.WebSockets;
using System.Numerics;
using System.Text.Json;
using DotSmog;
using DotSmog.service;
using DotSmog.src;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;

var dbConnector = DBConnector.Instance;



var tokenService = new TokenService();
using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true; // Prevent the process from terminating.
    cancellationTokenSource.Cancel();
};



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ServiceRealTime>();

var app = builder.Build();
var queueConnector = new QueueConnector(app.Services.GetRequiredService<ServiceRealTime>());
queueConnector.StartReceiving(cancellationTokenSource.Token);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseWebSockets();


app.MapGet("/sse", async (HttpContext ctx, ServiceRealTime sensorService) =>
{
    ctx.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
    ctx.Response.Headers.Append("Cache-Control", "no-cache");
    ctx.Response.Headers.Append("Connection", "keep-alive");

    try
    {
        while (!ctx.RequestAborted.IsCancellationRequested)
        {
            if (sensorService.DataQueue.TryTake(out var data, TimeSpan.FromSeconds(5)))
            {
                await ctx.Response.WriteAsync($"data: ");
                await JsonSerializer.SerializeAsync(ctx.Response.Body, data);
                await ctx.Response.WriteAsync("\n\n");
                await ctx.Response.Body.FlushAsync();
            }
            else
            {
                // Pauza, jeśli brak danych
                await Task.Delay(100);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"SSE connection error: {ex.Message}");
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
            BigInteger balance = await tokenService.GetBalance(accountId);
            int balanceValue = (int)balance;
            var response = new
            {
                stationId = accountId,
                value = balanceValue
            };
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        
    })
    .WithName("balance")
    .WithOpenApi();

app.MapGet("/api/data", async (
        string? stationType, 
        DateTime? date, 
        string? stationId, 
        string? limit, 
        string? sortBy, 
        string? sortOrder, 
        string? export) =>
    {
        try
        {
            var data = await dbConnector.GetDataAsync(
                QueueConnector.collectionName, 
                stationType, 
                date, 
                stationId, 
                limit, 
                sortBy, 
                sortOrder);

            // Obsługa eksportu
            if (!string.IsNullOrEmpty(export))
            {
                if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
                {
                    var csvStream = await dbConnector.ExportToCsvAsync(data);
                    return Results.File(csvStream, "text/csv", "data.csv");
                }

                if (string.Equals(export, "json", StringComparison.OrdinalIgnoreCase))
                {
                    var jsonStream = await dbConnector.ExportToJsonAsync(data);
                    return Results.File(jsonStream, "application/json", "data.json");
                }

                return Results.BadRequest("Invalid export type. Please use 'csv' or 'json'.");
            }
            return Results.Ok(data);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    })
    .WithName("data")
    .WithOpenApi();

app.Run();
