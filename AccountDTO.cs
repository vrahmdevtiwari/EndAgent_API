using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class AccountDTO
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
        public string OrgId { get; set; }
        public string Name { get; set; }
        public string Caption { get; set; }
        public string AccountType { get; set; }
        public string SID { get; set; }
        public string Domain { get; set; }
       
    }
}
