using Confluent.Kafka;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var producerConfig = new ProducerConfig
{
    BootstrapServers = "127.0.0.1:9092",
    ClientId = "service-f"
};

app.MapGet("/", () => "Hello World!");
app.MapPost("/publish", async () =>
{
    
    using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
 
    var eventData = new
    {
        EventType = "eventTypeX",
        OrderId = Guid.NewGuid().ToString(),
        CreatedBy = "service-f",
        CreatedAt = DateTime.UtcNow
    };
 
    var message = new Message<Null, string>
    {
        Value = JsonSerializer.Serialize(eventData)
    };
 
    await producer.ProduceAsync("topic-A", message);
 
    return Results.Ok("Published eventTypeX to topic-A");
});

// ✅ Endpoint to trigger POST to service-a
app.MapPost("/send-to-a", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();

    var payload = new
    {
        Message = "Hello from Service-G",
        Count = 5
    };

    var response = await client.PostAsJsonAsync(
        "http://localhost:5000/receive-from-f",
        payload
    );

    var result = await response.Content.ReadAsStringAsync();
    return Results.Ok(result);
});

// ✅ POST API to receive data from service-g
app.MapPost("/receive-from-f", (ServiceGPayload payload) =>
{
    Console.WriteLine($"Received from Service-G: {payload.Message}, Count: {payload.Count}");
    return Results.Ok("✅ Data received by Service-A");
});

    

app.Run();
