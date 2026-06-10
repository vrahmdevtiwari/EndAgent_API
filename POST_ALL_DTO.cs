using System.ComponentModel.DataAnnotations;
using TEST_WebApiOsDetails.Models.Dto;

namespace TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_
{
    public class POST_ALL_DTO
    {
        // EASpecificationDto
        public EASpecificationDto? EASpecificationDTO { get; set; }
        public List<InstalledAppDto>? InstalledApps { get; set; }
        public List<UpdateDto>? Updates { get; set; }
        public List<PortDto>? Ports { get; set; }
        public LocationDTO? LocationDTO { get; set; }
        public List<ServiceDTO>? Services { get; set; }
        public List<ProcessDTO>? Processes { get; set; }
        public List<ProcessorDTO>? Processors { get; set; }
        public List<RAMDetailDTO>? RAMDetails { get; set; }
        public List<ScheduledTaskDTO>? ScheduledTasks { get; set; }
        public List<StorageVolumeDTO>? StorageVolumes { get; set; }
        public List<GraphicCardDTO>? GraphicCards { get; set; }
        public List<RAIDControllerDTO>? RAIDControllers { get; set; }
        public List<NetworkAdapterDTO>? NetworkAdapters { get; set; }
        public List<PhysicalDriveDTO>? PhysicalDrives { get; set; }
        public OtherSpecificationDTO? OtherSpecifications { get; set; }
        public List<AccountDTO>? Accounts { get; set; }
        public List<ActivePortDTO>? ActivePorts { get; set; }
        public List<ActiveNetworkDetailDTO>? ActiveNetworkDetails { get; set; }
        public ResourceUtilDTO? ResourceUtils { get; set; }
        public List<DiskDetailDTO>? DiskDetails { get; set; }
        public TaskManagerDTO? TaskManager { get; set; }
        public string ObjectID { get; set; }
        public string OrgID { get; set; }

    }
}
