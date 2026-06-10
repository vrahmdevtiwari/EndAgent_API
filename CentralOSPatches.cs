using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TEST_WebApiOsDetails.Models
{
    [BsonIgnoreExtraElements] // safety layer
    public class CentralOSPatches
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("update_id")]
        public string UpdateId { get; set; }

        [BsonElement("update_os")]
        public string UpdateOS { get; set; }

        [BsonElement("os_version")]
        public string? OSVersion { get; set; }

        [BsonElement("bit_rate")]
        public string BitRate { get; set; }

        [BsonElement("title")]
        public string? Title { get; set; }

        [BsonElement("product")]
        public string? Product { get; set; }

        [BsonElement("classification")]
        public string? Classification { get; set; }

        [BsonElement("kb_number")]
        public string KBNumber { get; set; }

        [BsonElement("kb_description")]
        public string? KBNumberDescription { get; set; }

        [BsonElement("product_family")]
        public string? ProductFamily { get; set; }

        [BsonElement("platform")]
        public string? Platform { get; set; }

        [BsonElement("version")]
        public string? Version { get; set; }

        [BsonElement("size")]
        public string? Size { get; set; }

        [BsonElement("build_number")]
        public string? BuildNumber { get; set; }

        [BsonElement("article")]
        public string? Articles { get; set; }

        [BsonElement("release_date")]
        public string? ReleaseDate { get; set; }
        [BsonElement("file_name")]
        public string? FileName { get; set; }

        [BsonElement("patch_path")]
        public string? PatchPath { get; set; }

        // ✅ ADD THESE (VERY IMPORTANT)

        [BsonElement("created_at")]
        public DateTime? CreatedAt { get; set; }

        [BsonElement("modified_at")]
        public DateTime? ModifiedAt { get; set; }

        [BsonElement("is_deleted")]
        public bool IsDeleted { get; set; }
        public string OrgId { get; set; }
        [BsonElement("vendor")]
        public string? Vendor { get; set; }
        [BsonElement("category")]
        public string? Category { get; set; }
        [BsonElement("is_stale")]
        public bool? IsStale { get; set; } = false;
    }
}
