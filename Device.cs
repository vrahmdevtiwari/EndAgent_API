using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class Device
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string SystemName { get; set; }
        public string LoginUser { get; set; }
        public string Domain { get; set; }
        public string Privileges { get; set; }
        public string Manufacturer { get; set; }
        public string OS { get; set; }
        public string PublicIP { get; set; }
        public string LastSyncedTime { get; set; }
        public bool InITAM { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public int OrgID { get; set; }
        public Guid? ObjectID { get; set; }
        public string BIOS_SN { get; set; }
        public DateTime CreatedAt { get; set; }

        public Device()
        {
            ObjectID = Guid.NewGuid(); // Generate a new GUID when a new Device instance is created.
        }

        public override string ToString()
        {
            return $"Device: {SystemName}, ID: {Id}, OS: {OS}, Last Synced Time: {LastSyncedTime}, In ITAM: {InITAM}, Is Approved: {IsApproved}, Created At: {CreatedAt}";
        }
    }


}
