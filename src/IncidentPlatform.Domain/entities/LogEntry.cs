using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IncidentPlatform.Domain.Entities;

public class LogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid IncidentId { get; set; }

    public string Service { get; set; } = string.Empty;

    public string LogLevel { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
