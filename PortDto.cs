using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto
{
    public class PortDto
    {

        public int Id { get; set; }

        public string AssetId { get; set; }

        [Required]
        public string PortNumber { get; set; }
        [Required]
        public string ProcessId { get; set; }
        [Required]
        public string ProcessName { get; set; }
        public string OrgId { get; set; }

    }
}
