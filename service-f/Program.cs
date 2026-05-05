using SharedKafka.Extensions;
using SharedKafka.Producer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

// ✅ FIXED — AddSharedKafka BEFORE builder.Build()
builder.Services.AddSharedKafka(options =>
{
    options.BootstrapServers = "127.0.0.1:9092";
    options.ClientId         = "service-f";
    options.GroupId          = "service-f-group";
});

// ✅ Build AFTER registering services
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/publish", async (IKafkaProducer kafkaProducer) =>
{
    var eventData = new TopicAEvent
    {
        EventType = "eventTypeX",
        OrderId   = Guid.NewGuid().ToString(),
        CreatedBy = "service-f",
        CreatedAt = DateTime.UtcNow
    };

    // ✅ FIXED — use kafkaProducer (instance) not KafkaProducer (class)
    await kafkaProducer.ProduceAsync("topic-A", eventData);

    return Results.Ok("✅ Published TopicAEvent to topic-A");
});

app.MapGet("/status", () =>
{
    return Results.Ok("✅ service-f is healthy!");
});

app.Run();

// ✅ Event class
public class TopicAEvent
{
    public string   EventType { get; set; } = string.Empty;
    public string   OrderId   { get; set; } = string.Empty;
    public string   CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}