using DotSmog;
using MongoDB.Bson;

var dbConnector = DBConnector.Instance;

var queueConnector = new QueueConnector();
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

app.MapGet("/api/readings", async () =>
    {
        var documents = await dbConnector.GetDataAsync("users");
        var result = string.Join("\n", documents);
        return result;
    })
    .WithName("readings")
    .WithOpenApi();

app.Run();