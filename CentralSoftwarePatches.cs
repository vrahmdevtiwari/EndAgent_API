using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TEST_WebApiOsDetails.Models
{
    [BsonIgnoreExtraElements] // safety layer
    public class CentralSoftwarePatches
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }

        // Unique internal update identifier
        [BsonElement("update_id")]
        public string UpdateId { get; set; }

        // Software Name (Chrome, SQL Server, Java, etc.)
        [BsonElement("software_name")]
        public string SoftwareName { get; set; }

        // Vendor (Microsoft, Oracle, Google, Adobe)
        [BsonElement("vendor")]
        public string? Vendor { get; set; }

        // Patch / Advisory / Bulletin Number
        [BsonElement("patch_number")]
        public string? PatchNumber { get; set; }

        // Patch Description / Summary
        [BsonElement("patch_description")]
        public string? PatchDescription { get; set; }

        // Security / Bug Fix / Feature Update
        [BsonElement("classification")]
        public string? Classification { get; set; }

        // Critical / High / Medium / Low
        [BsonElement("severity")]
        public string? Severity { get; set; }

        // Software Version
        [BsonElement("version")]
        public string? Version { get; set; }

        // 32-bit / 64-bit
        [BsonElement("bit_rate")]
        public string? BitRate { get; set; }

        // Windows / Linux / Mac
        [BsonElement("platform")]
        public string? Platform { get; set; }

        // Build Number (Optional)
        [BsonElement("build_number")]
        public string? BuildNumber { get; set; }

        // File Size
        [BsonElement("size")]
        public string? Size { get; set; }

        // Related Articles / CVE references
        [BsonElement("articles")]
        public string? Articles { get; set; }

        // Release Date
        [BsonElement("release_date")]
        public string? ReleaseDate { get; set; }
        [BsonElement("file_name")]
        public string? FileName { get; set; }

        // Physical file path
        [BsonElement("patch_path")]
        public string? PatchPath { get; set; }

        // Product Family (Dropdown FK)
        [BsonElement("product_family")]
        public string? ProductFamily { get; set; }

        // Soft delete flag
        [BsonElement("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // Audit Fields
        [BsonElement("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [BsonElement("modified_date")]
        public DateTime? ModifiedDate { get; set; }
        public string OrgId { get; set; }
        [BsonElement("installed_parameters")]
        public string? InstalledParaters { get; set; }
        [BsonElement("category")]
        public string? Category { get; set; }
        [BsonElement("is_stale")]
        public bool? IsStale { get; set; } = false;
    }
}
