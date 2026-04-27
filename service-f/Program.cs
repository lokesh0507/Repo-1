using Confluent.Kafka;             // ← fixes Null, ProducerBuilder, Message errors
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var producerConfig = new ProducerConfig
{
    BootstrapServers = "127.0.0.1:9092",
    ClientId         = "service-f"
};

app.MapGet("/", () => "Hello World!");

app.MapPost("/publish", async () =>
{
    using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();

    var eventData = new TopicAEvent    // ← fixed: class defined at bottom
    {
        EventType = "eventTypeX",
        OrderId   = Guid.NewGuid().ToString(),
        CreatedBy = "service-f",
        CreatedAt = DateTime.UtcNow
    };

    var message = new Message<Null, string>
    {
        Value = JsonSerializer.Serialize(eventData)
    };

    await producer.ProduceAsync("topic-A", message);

    return Results.Ok("Published TopicAEvent to topic-A");
});

app.MapGet("/status", () =>
{
    return Results.Ok("service-f is healthy!");
});

app.Run();


// ──────────────────────────────────────────────
// ✅ REQUIRED — Add this class at bottom of file
// This fixes "TopicAEvent could not be found" error
// ──────────────────────────────────────────────

public class TopicAEvent
{
    public string   EventType { get; set; } = string.Empty;
    public string   OrderId   { get; set; } = string.Empty;
    public string   CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}