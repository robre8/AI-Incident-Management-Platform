using System.ComponentModel.DataAnnotations;
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

    [Required]
    [StringLength(200)]
    public string Service { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LogLevel { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
