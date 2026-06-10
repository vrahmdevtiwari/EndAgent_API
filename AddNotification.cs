using System.Text;
using TEST_WebApiOsDetails.Models;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_;
using TEST_WebApiOsDetails.Models.Notifications;

namespace TEST_WebApiOsDetails.data
{
    public class AddNotification
    {

        public List<Notification>? GenerateRAMChangeNotification(List<RAMDetailDTO> oldRAMDetails, List<RAMDetailDTO> newRAMDetails, string AssetId, string OrgId, string DevType, string DeviceId)
        {
            try 
            {
                StringBuilder notificationMessage = new StringBuilder();
                List<Notification> notifications = new List<Notification>();
                // Check for removed RAM
                foreach (var oldRAM in oldRAMDetails)
                {
                    if (!newRAMDetails.Any(newRAM => newRAM.PartNumber.Trim() == oldRAM.PartNumber.Trim()))
                    {
                        notificationMessage.AppendLine($"RAM \"{oldRAM.PartNumber}\" has been removed.\n");
                    }
                    if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                    {
                        Notification notification = new()
                        {
                            Header = "RAM Configurational Change",
                            Body = notificationMessage.ToString(),
                            AssetID = AssetId,
                            OrgID = OrgId,
                            CreatedAt = DateTime.Now,
                            DevType = DevType,
                            Message = "",
                            User = DeviceId,
                            IsRead = false
                        };
                        notifications.Add(notification);
                        notificationMessage.Clear();
                    }
                }

                // Check for added or changed RAM
                foreach (var newRAM in newRAMDetails)
                {
                    var oldRAM = oldRAMDetails.FirstOrDefault(old => old.PartNumber.Trim() == newRAM.PartNumber.Trim());
                    if (oldRAM == null)
                    {
                        string capacity = "";
                        if (!String.IsNullOrEmpty(newRAM.Capacity) && long.TryParse(newRAM.Capacity, out long sizeBytes))
                        {
                            
                            double totalSizeBytes = sizeBytes;
                            double totalSizeKB = totalSizeBytes / 1024.0;
                            double totalSizeMB = totalSizeKB / 1024.0;
                            double totalSizeGB = totalSizeMB / 1024.0;
                            double totalSizeTB = totalSizeGB / 1024.0;

                            if (totalSizeTB >= 1)
                            {
                                capacity = $"{totalSizeTB:0.##} TB";
                            }
                            else if (totalSizeGB >= 1)
                            {
                                capacity = $"{totalSizeGB:0.##} GB";
                            }
                            else if (totalSizeMB >= 1)
                            {
                                capacity = $"{totalSizeMB:0.##} MB";
                            }
                            else if (totalSizeKB >= 1)
                            {
                                capacity = $"{totalSizeKB:0.##} KB";
                            }
                        }
                        else
                        {
                            // Handle or log the case where parsing fails, if necessary.
                            // For now, you can skip the current iteration using continue.
                            continue;
                        }

                        var memType = newRAM.MemoryType;
                        switch (memType)
                        {
                            case "0": memType = "Unknown"; break;
                            case "1": memType = "Other"; break;
                            case "20": memType = "DDR"; break;
                            case "21": memType = "DDR2"; break;
                            case "22": memType = "DDR2 FB-DIMM "; break;
                            case "24": memType = "DDR3"; break;
                            case "26": memType = "DDR4"; break;
                        }


                        notificationMessage.AppendLine($"RAM \"{newRAM.PartNumber}\" has been added.\n");
                        notificationMessage.AppendLine($"Specifications: \n");
                        notificationMessage.AppendLine($"\tBank Label: {newRAM.BankLabel}\n");
                        notificationMessage.AppendLine($"\tCapacity: {capacity}\n");
                        notificationMessage.AppendLine($"\tDescription: {newRAM.Description}\n");
                        notificationMessage.AppendLine($"\tManufacturer: {newRAM.Manufacturer}\n");
                        notificationMessage.AppendLine($"\tMemoryType: {memType}\n");
                        notificationMessage.AppendLine($"\tSerialNumber: {newRAM.SerialNumber}\n");
                        notificationMessage.AppendLine($"\tSpeed: {newRAM.Speed}\n");
                        notificationMessage.AppendLine($"\tSMBIOS Memory Type: {newRAM.SMBIOSMemoryType}\n\n");

                        if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                        {
                            Notification notification = new()
                            {
                                Header = "RAM Configurational Change",
                                Body = notificationMessage.ToString(),
                                AssetID = AssetId,
                                OrgID = OrgId,
                                CreatedAt = DateTime.Now,
                                DevType = DevType,
                                Message = "",
                                User = DeviceId,
                                IsRead = false
                            };
                            notifications.Add(notification);
                            notificationMessage.Clear();
                        }
                    }
                    else
                    {
                        //// Compare properties
                        //if (oldRAM.Capacity.Trim() != newRAM.Capacity.Trim())
                        //{
                        //    notificationMessage.AppendLine($"Capacity of RAM \"{newRAM.PartNumber}\" has been changed from {oldRAM.Capacity} to {newRAM.Capacity}.\n");
                        //}

                        //if (oldRAM.BankLabel.Trim() != newRAM.BankLabel.Trim())
                        //{
                        //    notificationMessage.AppendLine($"Bank Label of RAM \"{newRAM.PartNumber}\" has been changed from {oldRAM.BankLabel} to {newRAM.BankLabel}.\n");
                        //}

                        //if (oldRAM.Description.Trim() != newRAM.Description.Trim())
                        //{
                        //    notificationMessage.AppendLine($"Description of RAM \"{newRAM.PartNumber}\" has been changed from {oldRAM.Description} to {newRAM.Description}.\n");
                        //}

                        //if (oldRAM.Manufacturer.Trim() != newRAM.Manufacturer.Trim())
                        //{
                        //    notificationMessage.AppendLine($"Manufacturer of RAM \"{newRAM.PartNumber}\" has been changed from {oldRAM.Manufacturer} to {newRAM.Manufacturer}.\n");
                        //}

                        //if (oldRAM.MemoryType.Trim() != newRAM.MemoryType.Trim())
                        //{
                        //    notificationMessage.AppendLine($"Memory Type of RAM \"{newRAM.PartNumber}\" has been changed from {oldRAM.MemoryType} to {newRAM.MemoryType}.\n");
                        //}

                        //if (oldRAM.SerialNumber.Trim() != newRAM.SerialNumber.Trim())
                        //{
                        //    notificationMessage.AppendLine($"Serial Number of RAM \"{newRAM.PartNumber}\" has been changed from {oldRAM.SerialNumber} to {newRAM.SerialNumber}.\n");
                        //}

                        //if (oldRAM.Speed.Trim() != newRAM.Speed.Trim())
                        //{
                        //    notificationMessage.AppendLine($"Speed of RAM \"{newRAM.PartNumber}\" has been changed from {oldRAM.Speed} to {newRAM.Speed}.\n");
                        //}

                        //if (oldRAM.SMBIOSMemoryType.Trim() != newRAM.SMBIOSMemoryType.Trim())
                        //{
                        //    notificationMessage.AppendLine($"SMBIOS Memory Type of RAM \"{newRAM.PartNumber}\" has been changed from {oldRAM.SMBIOSMemoryType} to {newRAM.SMBIOSMemoryType}.\n");
                        //}

                        //if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                        //{
                        //    Notification notification = new()
                        //    {
                        //        Header = "RAM Configurational Change",
                        //        Body = notificationMessage.ToString(),
                        //        AssetID = AssetId,
                        //        OrgID = OrgId,
                        //        CreatedAt = DateTime.Now,
                        //        DevType = DevType,
                        //        Message = "",
                        //        User = DeviceId,
                        //        IsRead = false
                        //    };
                        //    notifications.Add(notification);
                        //    notificationMessage.Clear();
                        //}
                    }
                }

                if (notifications.Count > 0)
                {
                    return notifications;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex) 
            {
                throw;
            }   
        }

        public List<Notification>? GenerateNetworkAdapterChangeNotification(List<NetworkAdapterDTO> oldNetworkAdapters, List<NetworkAdapterDTO> newNetworkAdapters, string AssetId, string OrgId, string DevType, string DeviceId)
        {
            try
            {
                StringBuilder notificationMessage = new StringBuilder();
                var notifications = new List<Notification>();
                // Check for removed network adapters
                foreach (var oldAdapter in oldNetworkAdapters)
                {
                    if (!newNetworkAdapters.Any(newAdapter => newAdapter.Name.Trim() == oldAdapter.Name.Trim()))
                    {
                        notificationMessage.AppendLine($"Network adapter \"{oldAdapter.Name}\" has been removed.");
                    }
                    if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                    {
                        Notification notification = new()
                        {
                            Header = "Network Adapters Configurational Change",
                            Body = notificationMessage.ToString(),
                            AssetID = AssetId,
                            OrgID = OrgId,
                            CreatedAt = DateTime.Now,
                            DevType = DevType,
                            Message = "",
                            User = DeviceId,
                            IsRead = false
                        };
                        notifications.Add(notification);
                        notificationMessage.Clear();
                    }
                }

                // Check for added or changed network adapters
                foreach (var newAdapter in newNetworkAdapters)
                {
                    var oldAdapter = oldNetworkAdapters.FirstOrDefault(old => old.Name.Trim() == newAdapter.Name.Trim());
                    if (oldAdapter == null)
                    {
                        notificationMessage.AppendLine($"Network adapter \"{newAdapter.Name}\" has been added.\n");
                        if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                        {
                            Notification notification = new()
                            {
                                Header = "Network Adapters Configurational Change",
                                Body = notificationMessage.ToString(),
                                AssetID = AssetId,
                                OrgID = OrgId,
                                CreatedAt = DateTime.Now,
                                DevType = DevType,
                                Message = "",
                                User = DeviceId,
                                IsRead = false
                            };
                            notifications.Add(notification);
                            notificationMessage.Clear();
                        }
                    }
                    else
                    {
                        // Compare properties
                        //if (oldAdapter.Description != newAdapter.Description)
                        //{
                        //    notificationMessage.AppendLine($"Description of network adapter \"{newAdapter.Name}\" has been changed from {oldAdapter.Description} to {newAdapter.Description}.\n");
                        //}

                        //if (oldAdapter.Status != newAdapter.Status)
                        //{
                        //    notificationMessage.AppendLine($"Status of network adapter \"{newAdapter.Name}\" has been changed from {oldAdapter.Status} to {newAdapter.Status}.\n");
                        //}

                        //if (oldAdapter.MACAddress != newAdapter.MACAddress)
                        //{
                        //    notificationMessage.AppendLine($"MAC Address of network adapter \"{newAdapter.Name}\" has been changed from {oldAdapter.MACAddress} to {newAdapter.MACAddress}.\n");
                        //}

                        ////if (oldAdapter.Speed != newAdapter.Speed)
                        ////{
                        ////    notificationMessage.AppendLine($"Speed of network adapter \"{newAdapter.Name}\" has been changed from {oldAdapter.Speed} to {newAdapter.Speed}.\n");
                        ////}

                        //if (oldAdapter.InterfaceIndex != newAdapter.InterfaceIndex)
                        //{
                        //    notificationMessage.AppendLine($"Interface Index of network adapter \"{newAdapter.Name}\" has been changed from {oldAdapter.InterfaceIndex} to {newAdapter.InterfaceIndex}.\n");
                        //}

                        //if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                        //{
                        //    Notification notification = new()
                        //    {
                        //        Header = "Network Adapters Configurational Change",
                        //        Body = notificationMessage.ToString(),
                        //        AssetID = AssetId,
                        //        OrgID = OrgId,
                        //        CreatedAt = DateTime.Now,
                        //        DevType = DevType,
                        //        Message = "",
                        //        User = DeviceId,
                        //        IsRead = false
                        //    };
                        //    notifications.Add(notification);
                        //    notificationMessage.Clear();
                        //}
                    }
                }

                if (notifications != null)
                {
                    return notifications;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public List<Notification>? GenerateDiskChangeNotification(List<DiskDetailDTO> oldDisks, List<DiskDetailDTO> newDisks, string AssetId, string OrgId, string DevType, string DeviceId)
        {
            try
            {
                StringBuilder notificationMessage = new StringBuilder();
                var notifications = new List<Notification>();
                // Check for removed disks
                foreach (var oldDisk in oldDisks)
                {
                    if (!newDisks.Any(newDisk => newDisk.Model == oldDisk.Model && newDisk.DeviceID == oldDisk.DeviceID))
                    {
                        notificationMessage.AppendLine($"Disk with Model: {oldDisk.Model} and Device ID: {oldDisk.DeviceID} has been removed.\n");
                    }
                    if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                    {
                        Notification notification = new()
                        {
                            Header = "Physical Drives Configurational Change",
                            Body = notificationMessage.ToString(),
                            AssetID = AssetId,
                            OrgID = OrgId,
                            CreatedAt = DateTime.Now,
                            DevType = DevType,
                            Message = "",
                            User = DeviceId,
                            IsRead = false
                        };
                        notifications.Add(notification);
                        notificationMessage.Clear();
                    }
                }

                // Check for added or changed disks
                foreach (var newDisk in newDisks)
                {
                    var oldDisk = oldDisks.FirstOrDefault(old => old.Model == newDisk.Model && old.DeviceID == newDisk.DeviceID);
                    if (oldDisk == null)
                    {
                        notificationMessage.AppendLine($"Disk with Model: \"{newDisk.Model}\" and Device ID: \"{newDisk.DeviceID}\" has been added.\n");
                        notificationMessage.AppendLine($"Specifications:\n");
                        notificationMessage.AppendLine($"\tManufacturer: {newDisk.Manufacturer}\n");
                        notificationMessage.AppendLine($"\tMedia Type: {newDisk.MediaType}\n");
                        notificationMessage.AppendLine($"\tSerial Number: {newDisk.SerialNumber}\n");
                        notificationMessage.AppendLine($"\tFirmware Revision: {newDisk.FirmwareRevision}\n");
                        notificationMessage.AppendLine($"\tCapacity: {newDisk.Capacity}");
                        notificationMessage.AppendLine($"\tPartitions: {newDisk.Partitions.Count()}\n");
                        notificationMessage.AppendLine($"\tInterface Type: {newDisk.InterfaceType}\n\n");

                        if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                        {
                            Notification notification = new()
                            {
                                Header = "Physical Drives Configurational Change",
                                Body = notificationMessage.ToString(),
                                AssetID = AssetId,
                                OrgID = OrgId,
                                CreatedAt = DateTime.Now,
                                DevType = DevType,
                                Message = "",
                                User = DeviceId,
                                IsRead = false
                            };
                            notifications.Add(notification);
                            notificationMessage.Clear();
                        }
                    }
                    else
                    {
                        // Compare properties for changed disks
                        //if (oldDisk.Model != newDisk.Model)
                        //{
                        //    notificationMessage.AppendLine($"Model of disk with Asset ID: \"{newDisk.AssetId}\" and Device ID: \"{newDisk.DeviceID}\" has been changed from {oldDisk.Model} to {newDisk.Model}.\n");
                        //}

                        //if (oldDisk.Manufacturer != newDisk.Manufacturer)
                        //{
                        //    notificationMessage.AppendLine($"Manufacturer of disk with Asset ID: \"{newDisk.AssetId}\" and Device ID: \"{newDisk.DeviceID}\" has been changed from {oldDisk.Manufacturer} to {newDisk.Manufacturer}.\n");
                        //}

                        //if (oldDisk.MediaType != newDisk.MediaType)
                        //{
                        //    notificationMessage.AppendLine($"Media Type of disk with Asset ID: \"{newDisk.AssetId}\" and Device ID: \"{newDisk.DeviceID}\" has been changed from {oldDisk.MediaType} to {newDisk.MediaType}.\n");
                        //}

                        //if (oldDisk.SerialNumber != newDisk.SerialNumber)
                        //{
                        //    notificationMessage.AppendLine($"Serial Number of disk with Asset ID: \"{newDisk.AssetId}\" and Device ID: \"{newDisk.DeviceID}\" has been changed from {oldDisk.SerialNumber} to {newDisk.SerialNumber}.\n");
                        //}

                        //if (oldDisk.FirmwareRevision != newDisk.FirmwareRevision)
                        //{
                        //    notificationMessage.AppendLine($"Firmware Revision of disk with Asset ID: \"{newDisk.AssetId}\" and Device ID: \"{newDisk.DeviceID}\" has been changed from {oldDisk.FirmwareRevision} to {newDisk.FirmwareRevision}.\n");
                        //}

                        //if (oldDisk.Capacity != newDisk.Capacity)
                        //{
                        //    notificationMessage.AppendLine($"Capacity of disk with Asset ID: \"{newDisk.AssetId}\" and Device ID: \"{newDisk.DeviceID}\" has been changed from {oldDisk.Capacity} to {newDisk.Capacity}.\n");
                        //}

                        //if (oldDisk.Partitions.Count() != newDisk.Partitions.Count())
                        //{
                        //    notificationMessage.AppendLine($"Number of partitions of disk with Asset ID: \"{newDisk.AssetId}\" and Device ID: \"{newDisk.DeviceID}\" has been changed from {oldDisk.Partitions.Count()} to {newDisk.Partitions.Count()}.\n");
                        //}

                        //if (oldDisk.InterfaceType != newDisk.InterfaceType)
                        //{
                        //    notificationMessage.AppendLine($"Interface Type of disk with Asset ID: \"{newDisk.AssetId}\" and Device ID: \"{newDisk.DeviceID}\" has been changed from {oldDisk.InterfaceType} to {newDisk.InterfaceType}.\n");
                        //}

                        //// Add comparisons for other properties as needed

                        //if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                        //{
                        //    Notification notification = new()
                        //    {
                        //        Header = "Physical Drives Configurational Change",
                        //        Body = notificationMessage.ToString(),
                        //        AssetID = AssetId,
                        //        OrgID = OrgId,
                        //        CreatedAt = DateTime.Now,
                        //        DevType = DevType,
                        //        Message = "",
                        //        User = DeviceId,
                        //        IsRead = false
                        //    };
                        //    notifications.Add(notification);
                        //    notificationMessage.Clear();
                        //}
                    }
                }

                if (notifications.Count > 0)
                {
                    return notifications;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Notification GenerateOSorBIOSNotification(string OSorBIOS, string oldDetail, string newDetail, string AssetId, string OrgId, string DevType, string DeviceId)
        {
            try
            {
                StringBuilder notificationMessage = new StringBuilder();
                if (string.IsNullOrEmpty(oldDetail) && !string.IsNullOrEmpty(newDetail))
                {
                    if (OSorBIOS == "OS")
                    {
                        notificationMessage.AppendLine($"Operating system \"{newDetail}\" has been installed.\n");
                    }
                    else if (OSorBIOS == "BIOS")
                    {
                        notificationMessage.AppendLine($"New BIOS \"{newDetail}\". has been recognized\n");
                    }


                    if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                    {
                        Notification notification = new()
                        {
                            Header = OSorBIOS == "OS" ? "Operating System Change" : (OSorBIOS == "BIOS" ? "BIOS Change" : "BIOS Change"),
                            Body = notificationMessage.ToString(),
                            AssetID = AssetId,
                            OrgID = OrgId,
                            CreatedAt = DateTime.Now,
                            DevType = DevType,
                            Message = "",
                            User = DeviceId,
                            IsRead = false
                        };
                        return notification;
                    }
                    return null;
                }
                else if (newDetail != oldDetail)
                {
                    if (OSorBIOS == "OS")
                    {
                        notificationMessage.AppendLine($"Operating System for \"{AssetId}\" has been changed from \"{oldDetail}\" to \"{newDetail}\".\n");
                    }
                    else if (OSorBIOS == "BIOS")
                    {
                        notificationMessage.AppendLine($"BIOS for \"{AssetId}\" has been changed from \"{oldDetail}\" to \"{newDetail}\".\n");
                    }


                    if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                    {
                        Notification notification = new()
                        {
                            Header = OSorBIOS == "OS" ? "Operating System Change" : (OSorBIOS == "BIOS" ? "BIOS Change" : "BIOS Change"),
                            Body = notificationMessage.ToString(),
                            AssetID = AssetId,
                            OrgID = OrgId,
                            CreatedAt = DateTime.Now,
                            DevType = DevType,
                            Message = "",
                            User = DeviceId,
                            IsRead = false
                        };
                        return notification;
                    }
                    return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Notification>? GenerateProcessorChangeNotification(List<ProcessorDTO> oldProcessors, List<ProcessorDTO> newProcessors, string AssetId, string OrgId, string DevType, string DeviceId)
        {
            try
            {
                StringBuilder notificationMessage = new StringBuilder();
                var notifications = new List<Notification>();

                // Check for removed processors
                foreach (var oldProcessor in oldProcessors)
                {
                    if (!newProcessors.Any(newProcessor => newProcessor.Name.Trim() == oldProcessor.Name.Trim()))
                    {
                        notificationMessage.AppendLine($"Processor \"{oldProcessor.Name}\" has been removed.");
                    }
                    if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                    {
                        Notification notification = new()
                        {
                            Header = "Processor Configurational Change",
                            Body = notificationMessage.ToString(),
                            AssetID = AssetId,
                            OrgID = OrgId,
                            CreatedAt = DateTime.Now,
                            DevType = DevType,
                            Message = "",
                            User = DeviceId,
                            IsRead = false
                        };
                        notifications.Add(notification);
                        notificationMessage.Clear();
                    }
                }

                // Check for added or changed processors
                foreach (var newProcessor in newProcessors)
                {
                    var oldProcessor = oldProcessors.FirstOrDefault(old => old.Name.Trim() == newProcessor.Name.Trim());
                    if (oldProcessor == null)
                    {
                        notificationMessage.AppendLine($"Processor \"{newProcessor.Name}\" has been added.\n");
                        if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                        {
                            Notification notification = new()
                            {
                                Header = "Processor Configurational Change",
                                Body = notificationMessage.ToString(),
                                AssetID = AssetId,
                                OrgID = OrgId,
                                CreatedAt = DateTime.Now,
                                DevType = DevType,
                                Message = "",
                                User = DeviceId,
                                IsRead = false
                            };
                            notifications.Add(notification);
                            notificationMessage.Clear();
                        }
                    }
                    else
                    {
                        // Compare properties
                        //if (oldProcessor.Manufacturer != newProcessor.Manufacturer)
                        //{
                        //    notificationMessage.AppendLine($"Manufacturer of processor \"{newProcessor.Name}\" has been changed from {oldProcessor.Manufacturer} to {newProcessor.Manufacturer}.\n");
                        //}

                        //if (oldProcessor.MaxClockSpeed != newProcessor.MaxClockSpeed)
                        //{
                        //    notificationMessage.AppendLine($"Max Clock Speed of processor \"{newProcessor.Name}\" has been changed from {oldProcessor.MaxClockSpeed} to {newProcessor.MaxClockSpeed}.\n");
                        //}

                        //if (oldProcessor.Cores != newProcessor.Cores)
                        //{
                        //    notificationMessage.AppendLine($"Number of Cores of processor \"{newProcessor.Name}\" has been changed from {oldProcessor.Cores} to {newProcessor.Cores}.\n");
                        //}

                        //if (oldProcessor.LogicalProcessors != newProcessor.LogicalProcessors)
                        //{
                        //    notificationMessage.AppendLine($"Number of Logical Processors of processor \"{newProcessor.Name}\" has been changed from {oldProcessor.LogicalProcessors} to {newProcessor.LogicalProcessors}.\n");
                        //}

                        //if (oldProcessor.ProcessorId != newProcessor.ProcessorId)
                        //{
                        //    notificationMessage.AppendLine($"Processor ID of processor \"{newProcessor.Name}\" has been changed from {oldProcessor.ProcessorId} to {newProcessor.ProcessorId}.\n");
                        //}

                        //if (!string.IsNullOrEmpty(notificationMessage.ToString()))
                        //{
                        //    Notification notification = new()
                        //    {
                        //        Header = "Processor Configurational Change",
                        //        Body = notificationMessage.ToString(),
                        //        AssetID = AssetId,
                        //        OrgID = OrgId,
                        //        CreatedAt = DateTime.Now,
                        //        DevType = DevType,
                        //        Message = "",
                        //        User = DeviceId,
                        //        IsRead = false
                        //    };
                        //    notifications.Add(notification);
                        //    notificationMessage.Clear();
                        //}
                    }
                }

                if (notifications.Count > 0)
                {
                    return notifications;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
