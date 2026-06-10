namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_.ITAM_EA
{
    public class ITAMEADeviceDTO
    {
        public string SystemName { get; set; } // system_name
        public string LoginUser { get; set; } // login_user
        public string Domain { get; set; } // domain
        public string Privileges { get; set; } // privileges
        public string Manufacturer { get; set; } // manufacturer
        public string OS { get; set; } // os
        public string PublicIP { get; set; } // public_ip
        public bool InITAM { get; set; } // in_itam
        public bool IsApproved { get; set; } // is_approved
        public string ID { get; set; } // asset_unique_id
        public string? OrgID { get; set; } // org_id
        public string? BIOS { get; set; } // bios_sn
        public DateTime? LastSyncDate { get; set; } // last_sync_time
        public DateTime? Created { get; set; } // created_at

    }

    public class DevicesDTO
    {
        public string AssetID { get; set; }
        public string SystemName { get; set; } // system_name
        public string LoginUser { get; set; } // login_user
        public string Domain { get; set; } // domain
        public string Privileges { get; set; } // privileges
        public string Manufacturer { get; set; } // manufacturer
        public string OS { get; set; } // os
        public string PublicIP { get; set; } // public_ip
        public bool InITAM { get; set; } // in_itam
        public bool IsApproved { get; set; } // is_approved
        public string ID { get; set; } // asset_unique_id
        public string? OrgID { get; set; } // org_id
        public string? BIOS { get; set; } // bios_sn
        public DateTime? LastSyncDate { get; set; } // last_sync_time
        public DateTime? Created { get; set; } // created_at

    }
}
