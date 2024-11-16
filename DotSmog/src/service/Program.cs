using DotSmog;
using DotSmog.src;
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
builder.Services.AddCors(options =>
{
     options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");

app.MapGet("/api/readings", async (string? type, DateTime? date ,
    Guid? stationUUID) =>
    {
        var documents = await dbConnector.GetDataAsync(QueueConnector.collectionName, type, date, stationUUID);
        Messages messages = new Messages();
        messages.SensorMessages = documents; 
        return Results.Ok(messages);
    })
    .WithName("readings")
    .WithOpenApi();

app.Run();