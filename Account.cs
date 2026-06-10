using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TEST_WebApiOsDetails.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SystemId { get; set; }

        [ForeignKey("SystemId")]
        public EASpecification EASpecification { get; set; }
        public string Name { get; set; }
        public string Caption { get; set; }
        public string AccountType { get; set; }
        public string SID { get; set; }
        public string Domain { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
