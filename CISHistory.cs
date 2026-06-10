using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EndAgent_API.Models
{
    [BsonIgnoreExtraElements]
    public class CISHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("cis_type")]
        public string CISType { get; set; }
        [BsonElement("update_id")]
        public string? UpdateId { get; set; }
        [BsonElement("name")]
        public string? Name { get; set; } = "";
        [BsonElement("kb_number")]
        public string? KBNumber { get; set; } = "";
        [BsonElement("bit_rate")]
        public string? BitRate { get; set; } = "";
        [BsonElement("platform")]
        public string? Platform { get; set; } = "";
        [BsonElement("version")]
        public string? Version { get; set; } = "";
        [BsonElement("vendor")]
        public string? Vendor { get; set; } = "";
        [BsonElement("category")]
        public string? Category { get; set; } = "";
        [BsonElement("file_name")]
        public string? FileName { get; set; } = "";
        [BsonElement("file_path")]
        public string? FilePath { get; set; } = "";
        [BsonElement("id_deleted")]
        public bool IsDeleted { get; set; } = true;
        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
