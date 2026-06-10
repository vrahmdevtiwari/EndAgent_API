using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EndAgent_API.Models
{
    public class PatchRequestDto
    {
        public string FamilyIds { get; set; }
        public List<OSPatchesUpdateIsStaleDTO> ExistingPatches { get; set; } = new();
        public List<OSPatchesUpdateIsStaleDTO> ExistingSoftwares { get; set; } = new();
    }


    public class OSPatchesUpdateIsStaleDTO
    {
        [BsonElement("update_id")]
        public string UpdateId { get; set; }
        [BsonElement("is_stale")]
        public bool IsStale { get; set; }
    }
}
