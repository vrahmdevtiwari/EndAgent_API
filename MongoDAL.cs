using MongoDB.Bson;
using MongoDB.Driver;
using TEST_WebApiOsDetails.Models;
using TEST_WebApiOsDetails.Models.Dto;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_;
using TEST_WebApiOsDetails.Models.Dto__Data_Tranfer_Objects_.ITAM_EA;
using EndAgent_API.Models.Dto__Data_Tranfer_Objects_;
using EndAgent_API.Models;
using System.Text.RegularExpressions;
using EndAgent_API.Models.ViewModel;
using System.IO;

namespace EndAgent_API.MongoDB
{
    public class MongoDAL
    {
        private MongoClient MongodbClient { get; set; }
        private IMongoDatabase MongoDatabase { get; set; }
        public IMongoCollection<AppInstallerDTO> AppInstallers => MongoDatabase.GetCollection<AppInstallerDTO>("AppInstallers");
        private IMongoCollection<BsonDocument> DevicesCollections { get; set; }
        private IMongoCollection<BsonDocument> EASpecificationCollection { get; set; }
        private IMongoCollection<BsonDocument> DevicesInfoCollections { get; set; }
        private IMongoCollection<BsonDocument> FileUploadCollections { get; set; }
        private IMongoCollection<BsonDocument> PatchQueueCollections { get; set; }
        private IMongoCollection<BsonDocument> UpdatePatchQueueCollections { get; set; }
        private IMongoCollection<BsonDocument> NotificationCollections { get; set; }
        private IMongoCollection<BsonDocument> ErrorLogsCollections { get; set; }
        private IMongoCollection<BsonDocument> BlackListSoftwareCollections { get; set; }
        private IMongoCollection<BsonDocument> TaskManagerCollections { get; set; }
        private IMongoCollection<BsonDocument> LoginActivityStatusCollections { get; set; }
        private IMongoCollection<BsonDocument> PatchHistoryCollections { get; set; }
        private IMongoCollection<BsonDocument> CISOptionsCollections { get; set; }
        private IMongoCollection<BsonDocument> CentralOSPatchesCollections { get; set; }
        private IMongoCollection<BsonDocument> CentralSoftwarePatchesCollections { get; set; }
        private IMongoCollection<BsonDocument> WhiteListSoftwareCollections { get; set; }
        private IMongoCollection<BsonDocument> EndpointGroupCollections { get; set; }

        public MongoDAL(string _dbConn, string _dbDatabase)
        {
            MongodbClient = new MongoClient(_dbConn);
            MongoDatabase = MongodbClient.GetDatabase(_dbDatabase);

            DevicesCollections = MongoDatabase.GetCollection<BsonDocument>("Devices");
            EASpecificationCollection = MongoDatabase.GetCollection<BsonDocument>("EASpecification");
            DevicesInfoCollections = MongoDatabase.GetCollection<BsonDocument>("DevicesInfo");
            FileUploadCollections = MongoDatabase.GetCollection<BsonDocument>("FileUpload");
            PatchQueueCollections = MongoDatabase.GetCollection<BsonDocument>("PatchQueue");
            UpdatePatchQueueCollections = MongoDatabase.GetCollection<BsonDocument>("UpdatePatchQueue");
            NotificationCollections = MongoDatabase.GetCollection<BsonDocument>("Notification");
            ErrorLogsCollections = MongoDatabase.GetCollection<BsonDocument>("ErrorLogs");
            BlackListSoftwareCollections = MongoDatabase.GetCollection<BsonDocument>("BlackListSoftware");
            TaskManagerCollections = MongoDatabase.GetCollection<BsonDocument>("TaskManagerOverAllUtilization");
            LoginActivityStatusCollections = MongoDatabase.GetCollection<BsonDocument>("LoginActivityStatus");
            PatchHistoryCollections = MongoDatabase.GetCollection<BsonDocument>("PatchHistory");
            CISOptionsCollections = MongoDatabase.GetCollection<BsonDocument>("CISOptions");
            CentralOSPatchesCollections = MongoDatabase.GetCollection<BsonDocument>("OSCentralCollection");
            CentralSoftwarePatchesCollections = MongoDatabase.GetCollection<BsonDocument>("SoftwareCentralCollection");
            WhiteListSoftwareCollections = MongoDatabase.GetCollection<BsonDocument>("WhiteListSoftware");
            EndpointGroupCollections = MongoDatabase.GetCollection<BsonDocument>("EndpointGroup");
        }

        //Dev: Srikanth Erukulla: 30-06-2025 - MongoDB Database Integration
        #region Devices Collection
        public async Task<bool> CheckDevices(string _objectId, string _orgId)
        {
            var _filter = new BsonDocument { { "asset_unique_id", _objectId }, { "org_id", Convert.ToInt32(_orgId) } };
            var _deviceList = await DevicesCollections.Find(_filter).ToListAsync();
            if (_deviceList != null && _deviceList.Count > 0)
                return true;

            return false;
        }

        public async Task<bool> CheckDevicesApprovedOrNot(string _objectId, string _orgId)
        {
            var _filter = new BsonDocument { { "asset_unique_id", _objectId }, { "org_id", Convert.ToInt32(_orgId) } };
            var _deviceList = await DevicesCollections.Find(_filter).ToListAsync();
            if (_deviceList != null && _deviceList.Count > 0)
            {
                var isApprovedField = _deviceList[0].GetValue("is_approved", BsonBoolean.False); // default false
                return isApprovedField.AsBoolean;
            }

            return false;
        }

        //AssetID = ObjectID
        public async Task<ITAMEADeviceDTO> GetDeviceByAssetId(string _objectId)
        {
            var _filter = new BsonDocument { { "asset_unique_id", _objectId } };
            var _device = await DevicesCollections.Find(_filter).FirstOrDefaultAsync();
            if (_device != null)
            {
                return new ITAMEADeviceDTO
                {
                    SystemName = _device.GetValue("system_name", "").AsString,
                    LoginUser = _device.GetValue("login_user", "").AsString,
                    Domain = _device.GetValue("domain", "").AsString,
                    Privileges = _device.GetValue("privileges", "").AsString,
                    Manufacturer = _device.GetValue("manufacturer", "").AsString,
                    OS = _device.GetValue("os", "").AsString,
                    PublicIP = _device.GetValue("public_ip", "").AsString,
                    ID = _device.GetValue("asset_unique_id", "").AsString,
                    IsApproved = _device.GetValue("is_approved", false).ToBoolean(),
                    InITAM = _device.GetValue("in_itam", false).ToBoolean(),
                    BIOS = _device.GetValue("bios_sn", "").AsString,
                    OrgID = _device.GetValue("org_id").AsInt32.ToString(),
                    LastSyncDate = _device.Contains("last_sync_time") && DateTime.TryParse(_device["last_sync_time"].ToString(), out var syncDate) ? syncDate : (DateTime?)null,
                    Created = _device.Contains("created_at") && DateTime.TryParse(_device["created_at"].ToString(), out var createdDate) ? createdDate : (DateTime?)null
                };
            }

            return null;
        }
        public async Task<List<DevicesDTO>> GetDeviceByOrgId(string orgId)
        {
            var devices = new List<DevicesDTO>();

            var filter = Builders<BsonDocument>.Filter.Eq("org_id", Convert.ToInt32(orgId));

            var projection = Builders<BsonDocument>.Projection
                .Include("asset_unique_id")
                .Include("system_name")
                .Include("login_user")
                .Include("domain")
                .Include("privileges")
                .Include("manufacturer")
                .Include("os")
                .Include("public_ip")
                .Include("in_itam")
                .Include("is_approved")
                .Include("bios_sn")
                .Include("org_id")
                .Include("last_sync_time")
                .Include("created_at")
                .Exclude("_id");

            var deviceList = await DevicesCollections
                .Find(filter)
                .Project(projection)
                .ToListAsync();

            if (deviceList == null || !deviceList.Any())
                return devices;

            foreach (var device in deviceList)
            {
                devices.Add(new DevicesDTO
                {
                    ID = device.GetValue("asset_unique_id", "").AsString,
                    SystemName = device.GetValue("system_name", "").AsString,
                    LoginUser = device.GetValue("login_user", "").AsString,
                    Domain = device.GetValue("domain", "").AsString,
                    Privileges = device.GetValue("privileges", "").AsString,
                    Manufacturer = device.GetValue("manufacturer", "").AsString,
                    OS = device.GetValue("os", "").AsString,
                    PublicIP = device.GetValue("public_ip", "").AsString,
                    IsApproved = device.GetValue("is_approved", false).ToBoolean(),
                    InITAM = device.GetValue("in_itam", false).ToBoolean(),
                    BIOS = device.GetValue("bios_sn", "").AsString,
                    OrgID = device.GetValue("org_id").AsInt32.ToString(),
                    LastSyncDate = device.Contains("last_sync_time") &&
                                   DateTime.TryParse(device["last_sync_time"].ToString(), out var syncDate)
                                   ? syncDate : (DateTime?)null,
                    Created = device.Contains("created_at") &&
                                   DateTime.TryParse(device["created_at"].ToString(), out var createdDate)
                                   ? createdDate : (DateTime?)null
                });
            }

            return devices;
        }

        public async Task<ITAMEADeviceDTO> GetDevice(string _objectId, string _orgId)
        {
            var _filter = new BsonDocument { { "asset_unique_id", _objectId }, { "org_id", Convert.ToInt32(_orgId) } };
            var _device = await DevicesCollections.Find(_filter).FirstOrDefaultAsync();
            if (_device != null)
            {
                return new ITAMEADeviceDTO
                {
                    SystemName = _device.GetValue("system_name", "").AsString,
                    LoginUser = _device.GetValue("login_user", "").AsString,
                    Domain = _device.GetValue("domain", "").AsString,
                    Privileges = _device.GetValue("privileges", "").AsString,
                    Manufacturer = _device.GetValue("manufacturer", "").AsString,
                    OS = _device.GetValue("os", "").AsString,
                    PublicIP = _device.GetValue("public_ip", "").AsString,
                    ID = _device.GetValue("asset_unique_id", "").AsString,
                    IsApproved = _device.GetValue("is_approved", false).ToBoolean(),
                    InITAM = _device.GetValue("in_itam", false).ToBoolean(),
                    BIOS = _device.GetValue("bios_sn", "").AsString,
                    LastSyncDate = _device.Contains("last_sync_time") && DateTime.TryParse(_device["last_sync_time"].ToString(), out var syncDate) ? syncDate : (DateTime?)null,
                    Created = _device.Contains("created_at") && DateTime.TryParse(_device["created_at"].ToString(), out var createdDate) ? createdDate : (DateTime?)null
                };
            }

            return null;
        }

        public async Task<List<ITAMEADeviceDTO>> GetDevicesByApproved(bool isApproved, string _orgId)
        {
            var _filter = new BsonDocument { { "is_approved", isApproved }, { "org_id", Convert.ToInt32(_orgId) } };
            var _deviceList = await DevicesCollections.Find(_filter).ToListAsync();
            if (_deviceList != null)
            {
                var _devices = new List<ITAMEADeviceDTO>();
                foreach (var _device in _deviceList)
                {
                    var device = new ITAMEADeviceDTO
                    {
                        SystemName = _device.GetValue("system_name", "").AsString,
                        LoginUser = _device.GetValue("login_user", "").AsString,
                        Domain = _device.GetValue("domain", "").AsString,
                        Privileges = _device.GetValue("privileges", "").AsString,
                        Manufacturer = _device.GetValue("manufacturer", "").AsString,
                        OS = _device.GetValue("os", "").AsString,
                        PublicIP = _device.GetValue("public_ip", "").AsString,
                        ID = _device.GetValue("asset_unique_id", "").AsString,
                        IsApproved = _device.GetValue("is_approved", false).ToBoolean(),
                        InITAM = _device.GetValue("in_itam", false).ToBoolean(),
                        BIOS = _device.GetValue("bios_sn", "").AsString,
                        OrgID = _device.GetValue("org_id").AsInt32.ToString(),
                        LastSyncDate = _device.Contains("last_sync_time") && DateTime.TryParse(_device["last_sync_time"].ToString(), out var syncDate) ? syncDate : (DateTime?)null,
                        Created = _device.Contains("created_at") && DateTime.TryParse(_device["created_at"].ToString(), out var createdDate) ? createdDate : (DateTime?)null
                    };
                    _devices.Add(device);
                }

                return _devices;
            }

            return null;
        }

        public async Task<List<ITAMEADeviceDTO>> GetAllDevicesByOrgId(string _orgId)
        {
            var _filter = new BsonDocument { { "org_id", Convert.ToInt32(_orgId) } };
            var _deviceList = await DevicesCollections.Find(_filter).ToListAsync();
            if (_deviceList != null && _deviceList.Count > 0)
            {
                List<ITAMEADeviceDTO> _itamDevicesList = new List<ITAMEADeviceDTO>();
                foreach (var d in _deviceList)
                {
                    ITAMEADeviceDTO _itam = new ITAMEADeviceDTO()
                    {
                        SystemName = d["system_name"].ToString() ?? string.Empty,
                        LoginUser = d["login_user"].ToString() ?? string.Empty,
                        Domain = d["domain"].ToString() ?? string.Empty,
                        Privileges = d["privileges"].ToString() ?? string.Empty,
                        Manufacturer = d["manufacturer"].ToString() ?? string.Empty,
                        OS = d["os"].ToString() ?? string.Empty,
                        PublicIP = d["public_ip"].ToString() ?? string.Empty,
                        ID = d["asset_unique_id"].ToString() ?? string.Empty,
                        IsApproved = Convert.ToBoolean(d["is_approved"].ToString()),
                        InITAM = Convert.ToBoolean(d["in_itam"].ToString()),
                        BIOS = d["bios_sn"].ToString() ?? string.Empty,
                        LastSyncDate = !string.IsNullOrEmpty(d["last_sync_time"].ToString()) ? Convert.ToDateTime(d["last_sync_time"].ToString()) : (DateTime?)null,
                        Created = !string.IsNullOrEmpty(d["created_at"].ToString()) ? Convert.ToDateTime(d["created_at"].ToString()) : (DateTime?)null
                    };

                    _itamDevicesList.Add(_itam);
                }

                return _itamDevicesList;
            }

            return new List<ITAMEADeviceDTO>();
        }

        public async Task<string> AddDevices(DeviceDTO model, bool _itamFlag)
        {
            
            var filter = new BsonDocument { { "bios_sn", model.BIOS_SN } };
            var _deviceList = await DevicesCollections.Find(filter).ToListAsync();

            if (_deviceList.Any())
            {
                var _device = _deviceList.FirstOrDefault(x => x["asset_unique_id"].AsString == model.ObjectID);

                if (_device != null)
                {
                    if (!_device.GetValue("is_approved", false).AsBoolean)
                        return $"Device {model.SystemName} already exists. Waiting for approval.";

                    return "Approved";
                }

                var _deviceSingle = _deviceList.FirstOrDefault();
                string _assetUnique = _deviceSingle["asset_unique_id"].AsString;
                return _assetUnique;

            }
            else
            {
                if (string.IsNullOrEmpty(model.ObjectID)) model.ObjectID = Guid.NewGuid().ToString();
                var _idfilter = new BsonDocument { { "asset_unique_id", model.ObjectID } };
                var _device = await DevicesCollections.Find(_idfilter).FirstOrDefaultAsync();
                if (_device == null)
                {
                    //Add new device info details
                    var _addDocument = new BsonDocument
                    {
                        { "asset_unique_id", model.ObjectID },
                        { "org_id",model.OrgID},
                        {"system_name",model.SystemName },
                        { "domain",model.Domain},
                        {"privileges",model.Privileges },
                        {"login_user",model.LoginUser },
                        {"manufacturer",model.Manufacturer },
                        {"os",model.OS },
                        {"public_ip",model.PublicIP },
                        { "bios_sn",model.BIOS_SN},
                        {"in_itam",_itamFlag},
                        {"is_approved",false },
                        {"last_sync_time",DateTime.Today },
                        {"created_at",DateTime.UtcNow }
                    };

                    await DevicesCollections.InsertOneAsync(_addDocument);
                }
                else
                {
                    var updateDefinition = Builders<BsonDocument>.Update
                        .Set("system_name", model.SystemName)
                        .Set("domain", model.Domain)
                        .Set("privileges", model.Privileges)
                        .Set("login_user", model.LoginUser)
                        .Set("manufacturer", model.Manufacturer)
                        .Set("os", model.OS)
                        .Set("public_ip", model.PublicIP)
                        .Set("bios_sn", model.BIOS_SN)
                        .Set("in_itam", _itamFlag)
                        .Set("last_sync_time", DateTime.Today);

                    await DevicesCollections.UpdateOneAsync(_idfilter, updateDefinition);
                }

                return $"{model.ObjectID}";
            }

        }

        public async Task<MongoError> ApproveDeviceByObjectId(string _objectId)
        {
            try
            {
                var _filter = new BsonDocument { { "asset_unique_id", _objectId } };
                var _update = Builders<BsonDocument>.Update.Set("is_approved", true);
                await DevicesCollections.UpdateOneAsync(_filter, _update);
                return new MongoError() { Status = true, Message = "Success" };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }

        public async Task<MongoError> DeleteDeviceById(string _objectId)
        {
            var _filter = new BsonDocument { { "asset_unique_id", _objectId } };
            var _result = await DevicesCollections.DeleteOneAsync(_filter);
            return new MongoError()
            {
                Status = _result.DeletedCount > 0,
                Message = _result.DeletedCount > 0 ? "Success" : "Failed"
            };
        }

        #endregion

        #region EASpecification Collection
        public async Task<bool> CheckEASpecificationByAssetUniqueId(string _assetUniqueId, string _orgId)
        {
            var _filter = new BsonDocument { { "asset_unique_id", _assetUniqueId }, { "org_id", Convert.ToInt32(_orgId) } };
            var _eaSpecify = await EASpecificationCollection.Find(_filter).ToListAsync();
            if (_eaSpecify != null && _eaSpecify.Count > 0)
                return true;

            return false;
        }

        public async Task<List<string>> GetEASpecificationIDs()
        {
            var documents = await EASpecificationCollection
                                .Find(Builders<BsonDocument>.Filter.Empty as FilterDefinition<BsonDocument>)
                                .Project(Builders<BsonDocument>.Projection.Include("asset_unique_id"))
                                .ToListAsync();

            return documents.Select(d => d["asset_unique_id"].ToString()).ToList();
        }

        public async Task<List<EASpecificationDto>> GetEASpecifications(string _orgId)
        {
            var _eaFilter = new BsonDocument { { "org_id", Convert.ToInt32(_orgId) } };
            var _eaList = await EASpecificationCollection.Find(_eaFilter).ToListAsync();
            if (_eaList != null)
            {
                var eaList = _eaList.Select(e => new EASpecificationDto
                {
                    ID = e["_id"].ToString() ?? string.Empty,
                    AssetID = e["asset_unique_id"].ToString() ?? string.Empty,
                    SystemName = e["system_name"].ToString() ?? string.Empty,
                    SystemStatus = e["system_status"].ToString() ?? string.Empty,
                    OperatingSystem = e["os"].ToString() ?? string.Empty,
                    OSVersion = e["os_version"].ToString() ?? string.Empty,
                    OSBuildVersion = e["os_build_version"].ToString() ?? string.Empty,
                    LoginUser = e["login_user"].ToString() ?? string.Empty,
                    LastActive = e["last_active"].ToString() ?? string.Empty,
                    Domain = e["domain"].ToString() ?? string.Empty,
                    Privileges = e["privileges"].ToString() ?? string.Empty,
                    NetworkAdapter = e["network_adapter"].ToString() ?? string.Empty,
                    IPv4Address = e["ipv4_address"].ToString() ?? string.Empty,
                    IPv6Address = e["ipv6_address"].ToString() ?? string.Empty,
                    Gateway = e["gateway"].ToString() ?? string.Empty,
                    SubnetMask = e["subnet_mask"].ToString() ?? string.Empty,
                    CreatedAt = e["created_at"].ToString() != null ? Convert.ToDateTime(e["created_at"].ToLocalTime()) : new DateTime()
                }).ToList();

                return eaList;
            }

            return new List<EASpecificationDto>();
        }

        public async Task<EASpecificationDto> GetEASpecificationByAssetId(string _assetUniqueId)
        {
            var _eaFilter = new BsonDocument { { "asset_unique_id", _assetUniqueId } };
            var _eaList = await EASpecificationCollection.Find(_eaFilter).FirstOrDefaultAsync();
            if (_eaList != null)
            {
                EASpecificationDto eASpecification = new EASpecificationDto()
                {
                    ID = _eaList["_id"].ToString() ?? string.Empty,
                    SystemName = _eaList["system_name"].ToString() ?? string.Empty,
                    SystemStatus = _eaList["system_status"].ToString() ?? string.Empty,
                    OperatingSystem = _eaList["os"].ToString() ?? string.Empty,
                    OSVersion = _eaList["os_version"].ToString() ?? string.Empty,
                    OSBuildVersion = _eaList["os_build_version"].ToString() ?? string.Empty,
                    LoginUser = _eaList["login_user"].ToString() ?? string.Empty,
                    LastActive = _eaList["last_active"].ToString() ?? string.Empty,
                    Domain = _eaList["domain"].ToString() ?? string.Empty,
                    Privileges = _eaList["privileges"].ToString() ?? string.Empty,
                    NetworkAdapter = _eaList["network_adapter"].ToString() ?? string.Empty,
                    IPv4Address = _eaList["ipv4_address"].ToString() ?? string.Empty,
                    IPv6Address = _eaList["ipv6_address"].ToString() ?? string.Empty,
                    Gateway = _eaList["gateway"].ToString() ?? string.Empty,
                    SubnetMask = _eaList["subnet_mask"].ToString() ?? string.Empty,
                    OrgID = _eaList["org_id"].ToString() ?? string.Empty,
                    CreatedAt = _eaList["created_at"].ToString() != null ? Convert.ToDateTime(_eaList["created_at"].ToLocalTime()) : new DateTime()
                };

                return eASpecification;
            }

            return new EASpecificationDto();
        }
        public async Task<EASpecificationDto> GetEASpecificationByAssetId(string _assetUniqueId, string _orgId)
        {
            var _eaFilter = new BsonDocument { { "asset_unique_id", _assetUniqueId }, { "org_id", Convert.ToInt32(_orgId) } };
            var _eaList = await EASpecificationCollection.Find(_eaFilter).FirstOrDefaultAsync();
            if (_eaList != null)
            {
                EASpecificationDto eASpecification = new EASpecificationDto()
                {
                    ID = _eaList["_id"].ToString() ?? string.Empty,
                    SystemName = _eaList["system_name"].ToString() ?? string.Empty,
                    SystemStatus = _eaList["system_status"].ToString() ?? string.Empty,
                    OperatingSystem = _eaList["os"].ToString() ?? string.Empty,
                    OSVersion = _eaList["os_version"].ToString() ?? string.Empty,
                    OSBuildVersion = _eaList["os_build_version"].ToString() ?? string.Empty,
                    LoginUser = _eaList["login_user"].ToString() ?? string.Empty,
                    LastActive = _eaList["last_active"].ToString() ?? string.Empty,
                    Domain = _eaList["domain"].ToString() ?? string.Empty,
                    Privileges = _eaList["privileges"].ToString() ?? string.Empty,
                    NetworkAdapter = _eaList["network_adapter"].ToString() ?? string.Empty,
                    IPv4Address = _eaList["ipv4_address"].ToString() ?? string.Empty,
                    IPv6Address = _eaList["ipv6_address"].ToString() ?? string.Empty,
                    Gateway = _eaList["gateway"].ToString() ?? string.Empty,
                    SubnetMask = _eaList["subnet_mask"].ToString() ?? string.Empty,
                    CreatedAt = _eaList["created_at"].ToString() != null ? Convert.ToDateTime(_eaList["created_at"].ToLocalTime()) : new DateTime()
                };

                return eASpecification;
            }

            return new EASpecificationDto();
        }

        public async Task<EASpecificationDto> GetEASpecificationByUniqueId(string _uniqueId)
        {
            var _eaFilter = new BsonDocument { { "ea_unique_id", _uniqueId } };
            var _eaList = await EASpecificationCollection.Find(_eaFilter).FirstOrDefaultAsync();
            if (_eaList != null)
            {
                EASpecificationDto eASpecification = new EASpecificationDto()
                {
                    SystemName = _eaList["system_name"].ToString() ?? string.Empty,
                    SystemStatus = _eaList["system_status"].ToString() ?? string.Empty,
                    OperatingSystem = _eaList["os"].ToString() ?? string.Empty,
                    OSVersion = _eaList["os_version"].ToString() ?? string.Empty,
                    OSBuildVersion = _eaList["os_build_version"].ToString() ?? string.Empty,
                    LoginUser = _eaList["login_user"].ToString() ?? string.Empty,
                    LastActive = _eaList["last_active"].ToString() ?? string.Empty,
                    Domain = _eaList["domain"].ToString() ?? string.Empty,
                    Privileges = _eaList["privileges"].ToString() ?? string.Empty,
                    NetworkAdapter = _eaList["network_adapter"].ToString() ?? string.Empty,
                    IPv4Address = _eaList["ipv4_address"].ToString() ?? string.Empty,
                    IPv6Address = _eaList["ipv6_address"].ToString() ?? string.Empty,
                    Gateway = _eaList["gateway"].ToString() ?? string.Empty,
                    SubnetMask = _eaList["subnet_mask"].ToString() ?? string.Empty,
                    CreatedAt = _eaList["created_at"].ToString() != null ? Convert.ToDateTime(_eaList["created_at"].ToString()) : new DateTime()
                };

                return eASpecification;
            }

            return new EASpecificationDto();
        }

        public async Task<EASpecificationDto> GetEASpecificationByUniqueId(string _uniqueId, string _orgId)
        {
            var _eaFilter = new BsonDocument { { "ea_unique_id", _uniqueId }, { "org_id", Convert.ToInt32(_orgId) } };
            var _eaList = await EASpecificationCollection.Find(_eaFilter).FirstOrDefaultAsync();
            if (_eaList != null)
            {
                EASpecificationDto eASpecification = new EASpecificationDto()
                {
                    SystemName = _eaList["system_name"].ToString() ?? string.Empty,
                    SystemStatus = _eaList["system_status"].ToString() ?? string.Empty,
                    OperatingSystem = _eaList["os"].ToString() ?? string.Empty,
                    OSVersion = _eaList["os_version"].ToString() ?? string.Empty,
                    OSBuildVersion = _eaList["os_build_version"].ToString() ?? string.Empty,
                    LoginUser = _eaList["login_user"].ToString() ?? string.Empty,
                    LastActive = _eaList["last_active"].ToString() ?? string.Empty,
                    Domain = _eaList["domain"].ToString() ?? string.Empty,
                    Privileges = _eaList["privileges"].ToString() ?? string.Empty,
                    NetworkAdapter = _eaList["network_adapter"].ToString() ?? string.Empty,
                    IPv4Address = _eaList["ipv4_address"].ToString() ?? string.Empty,
                    IPv6Address = _eaList["ipv6_address"].ToString() ?? string.Empty,
                    Gateway = _eaList["gateway"].ToString() ?? string.Empty,
                    SubnetMask = _eaList["subnet_mask"].ToString() ?? string.Empty,
                    CreatedAt = _eaList["created_at"].ToString() != null ? Convert.ToDateTime(_eaList["created_at"].ToString()) : new DateTime()
                };

                return eASpecification;
            }

            return new EASpecificationDto();
        }

        public async Task<MongoError> AddEASpecification(string _assetUniqueId, EASpecificationDto _eASpecification)
        {
            try
            {
                var _eaSpec = new BsonDocument
                {
                    {"ea_unique_id", Guid.NewGuid().ToString()}, //system_id
                    {"asset_unique_id", _assetUniqueId},
                    {"org_id", _eASpecification.OrgID },
                    {"system_name",_eASpecification.SystemName },
                    {"system_status",_eASpecification.SystemStatus },
                    {"os",_eASpecification.OperatingSystem },
                    {"os_version",_eASpecification.OSVersion },
                    {"os_build_version",_eASpecification.OSBuildVersion },
                    {"login_user",_eASpecification.LoginUser },
                    {"last_active",_eASpecification.LastActive },
                    {"domain",_eASpecification.Domain },
                    {"privileges",_eASpecification.Privileges },
                    {"network_adapter",_eASpecification.NetworkAdapter },
                    {"ipv4_address",_eASpecification.IPv4Address },
                    {"ipv6_address",_eASpecification.IPv6Address },
                    {"gateway",_eASpecification.Gateway },
                    {"subnet_mask",_eASpecification.SubnetMask },
                    {"created_at",DateTime.UtcNow }
                };

                await EASpecificationCollection.InsertOneAsync(_eaSpec);
                return new MongoError() { Status = true, Message = "Success" };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }

        public async Task<MongoError> UpdateEASpecification(string _assetUniqueId, EASpecificationDto _eASpecification)
        {
            try
            {
                var _eafilter = new BsonDocument { { "asset_unique_id", _assetUniqueId }, { "org_id", Convert.ToInt32(_eASpecification.OrgID) } };
                var _eaupdate = Builders<BsonDocument>.Update.Set("system_name", _eASpecification.SystemName)
                                                           .Set("system_status", _eASpecification.SystemStatus)
                                                           .Set("os", _eASpecification.OperatingSystem)
                                                           .Set("os_version", _eASpecification.OSVersion)
                                                           .Set("os_build_version", _eASpecification.OSBuildVersion)
                                                           .Set("login_user", _eASpecification.LoginUser)
                                                           .Set("last_active", _eASpecification.LastActive)
                                                           .Set("domain", _eASpecification.Domain)
                                                           .Set("privileges", _eASpecification.Privileges)
                                                           .Set("network_adapter", _eASpecification.NetworkAdapter)
                                                           .Set("ipv4_address", _eASpecification.IPv4Address)
                                                           .Set("ipv6_address", _eASpecification.IPv6Address)
                                                           .Set("gateway", _eASpecification.Gateway)
                                                           .Set("subnet_mask", _eASpecification.SubnetMask)
                                                           .Set("created_at", DateTime.UtcNow);

                var _eaResult = await EASpecificationCollection.UpdateOneAsync(_eafilter, _eaupdate, new UpdateOptions { IsUpsert = true });
                return new MongoError() { Status = _eaResult.ModifiedCount > 0, Message = _eaResult.ModifiedCount > 0 ? "Success" : "Fail" };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }

        public async Task<bool?> UpdateLastActiveInEASpecificationAsync(string orgId, string assetUniqueId)
        {
            try
            {
                string lastActive = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("org_id", Convert.ToInt32(orgId)),
                    Builders<BsonDocument>.Filter.Eq("asset_unique_id", assetUniqueId)
                );

                var update = Builders<BsonDocument>.Update
                    .Set("created_at", DateTime.UtcNow)
                    .Set("last_active", lastActive);

                var result = await EASpecificationCollection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region Devices Information

        public async Task<MongoError> AddDevicesInfo(POST_ALL_DTO model)
        {
            try
            {
                // 1. Location
                var _location = new BsonDocument
                {
                    { "_id", Guid.NewGuid().ToString() },
                    { "latitude", model.LocationDTO?.Latitude ?? "0.0" },
                    { "longitude", model.LocationDTO?.Longitude ?? "0.0" }
                };
                //    // 2. Add EASpecificationDto
                //    var _eaSpec = new BsonDocument
                //{
                //    {"system_name",model.EASpecificationDTO.SystemName },
                //    {"system_status",model.EASpecificationDTO.SystemStatus },
                //    {"os",model.EASpecificationDTO.OperatingSystem },
                //    {"login_user",model.EASpecificationDTO.LoginUser },
                //    {"last_active",model.EASpecificationDTO.LastActive },
                //    {"domain",model.EASpecificationDTO.Domain },
                //    {"privileges",model.EASpecificationDTO.Privileges },
                //    {"network_adapter",model.EASpecificationDTO.NetworkAdapter },
                //    {"ipv4_address",model.EASpecificationDTO.IPv4Address },
                //    {"ipv6_address",model.EASpecificationDTO.IPv6Address },
                //    {"gateway",model.EASpecificationDTO.Gateway },
                //    {"subnet_mask",model.EASpecificationDTO.SubnetMask },
                //    {"created_at",DateTime.UtcNow }
                //};
                // 3. InstalledAppDto
                var _installedApps = model.InstalledApps.Select(x => new BsonDocument
                {
                    { "_id", Guid.NewGuid().ToString() },
                    {"app_name",x.AppName },
                    {"provider",x.Provider },
                    {"size",x.Size },
                    {"installed_on",x.InstalledOn },
                    {"version",x.Version },
                    {"created_at",DateTime.UtcNow }
                }).ToArray<BsonDocument>();
                // 4. UpdateDto
                var _updates = model.Updates.Select(x => new BsonDocument
            {
                    { "_id", Guid.NewGuid().ToString() },
                {"patch",x.Patch },
                {"title",x.Title },
                {"description",x.Description },
                {"installed_on",x.InstalledOn },
                {"version",x.Version },
                {"created_at",DateTime.UtcNow }
            }).ToList<BsonDocument>();
                // 5. PortDto
                var _ports = model.Ports.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"port_number",x.PortNumber },
                {"process_id",x.ProcessId },
                {"process_name",x.ProcessName },
                {"created_at",DateTime.UtcNow }
            }).ToList<BsonDocument>();
                // 6. Services
                var _service = model.Services.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"service_name",x.ServiceName },
                {"display_name",x.DisplayName },
                {"service_type",x.ServiceType },
                {"service_status",x.ServiceStatus },
                {"start_type",x.StartType },
                {"pid",x.PID },
                {"created_at",DateTime.UtcNow }
            }).ToList<BsonDocument>();
                // 7. ProcessDTO
                var _process = model.Processes.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"process_name",x.ProcessName },
                {"process_id",x.ProcessId },
                {"status",x.Status },
                {"username",x.Username },
                {"memory_usage",x.MemoryUsage },
                {"description",x.Description },
                {"path",x.Path },
                {"created_at",DateTime.UtcNow }
            }).ToList<BsonDocument>();
                // 8. ProcessorDTO
                var _processor = model.Processors.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"device_id",x.DeviceID },
                {"name",x.Name },
                {"manufacturer",x.Manufacturer },
                {"max_clock_speed",x.MaxClockSpeed },
                {"cores",x.Cores },
                {"logical_processors",x.LogicalProcessors },
                {"processor_id",x.ProcessorId },
                {"created_at",DateTime.UtcNow }
            }).ToList<BsonDocument>();
                // 9. RAM
                var _ram = model.RAMDetails.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"bank_label",x.BankLabel },
                {"capacity",x.Capacity},
                {"description",x.Description },
                {"manufacturer",x.Manufacturer },
                {"memory_type",x.MemoryType },
                {"part_number",x.PartNumber },
                {"serial_number",x.SerialNumber },
                {"speed",x.Speed},
                {"sm_bios_memory_type",x.SMBIOSMemoryType },
                {"created_at",DateTime.UtcNow }
            }).ToList<BsonDocument>();
                // 10. ScheduledTask
                var _scheduled_task = model.ScheduledTasks.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"name",x.Name},
                {"status",x.Status},
                {"author",x.Author},
                {"path",x.Path},
                {"trigger",x.Trigger},
                {"last_run_result",x.LastRunResult},
                {"last_run_time",x.LastRunTime },
                {"next_run_time",x.NextRunTime},
                {"created_at",DateTime.UtcNow}
            }).ToList<BsonDocument>();
                // 11. StorageVolumne
                var _storage_volumne = model.StorageVolumes.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"boot_volumne",x.BootVolume},
                {"capacity",x.Capacity },
                {"drive_letter",x.DriveLetter},
                {"file_system",x.FileSystem },
                {"free_system",x.FreeSpace},
                {"label",x.Label},
                {"system_volumne",x.SystemVolume},
                {"created_at",DateTime.UtcNow}
            }).ToList<BsonDocument>();
                // 12. Graphic Card
                var _graphic_card = model.GraphicCards.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"adapter_compatibility",x.AdapterCompatibility},
                {"adapter_ram",x.AdapterRAM },
                {"caption",x.Caption},
                {"device_id",x.DeviceID},
                {"video_processor",x.VideoProcessor},
                {"created_at",DateTime.UtcNow}
            }).ToList<BsonDocument>();
                // 13. RAID
                var _raid = model.RAIDControllers.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"name",x.Name},
                {"status",x.Status},
                {"pnp_device_id",x.PNPDeviceID},
                {"caption",x.Caption },
                {"system_creation_class_name",x.SystemCreationClassName},
                {"description",x.Description},
                {"manufacturer",x.Manufacturer },
                {"system_name",x.SystemName},
                {"config_manager_error_code",x.ConfigManagerErrorCode},
                {"config_manager_user_config",x.ConfigManagerUserConfig},
                {"created_at",DateTime.UtcNow},

            }).ToList<BsonDocument>();
                // 14. Network Adapter
                var _network_adapter = model.NetworkAdapters.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"name",x.Name},
                {"status",x.Status},
                {"description",x.Description },
                {"interface_index",x.InterfaceIndex},
                {"mac_address",x.MACAddress},
                {"speed",x.Speed},
                {"created_at",DateTime.UtcNow}
            }).ToList<BsonDocument>();
                // 15. Physical Drivers
                var _physical_drivers = model.PhysicalDrives.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"device_id",x.DeviceID},
                {"firmware_revision",x.FirmwareRevision},
                {"index",x.Index},
                {"interface_type",x.InterfaceType},
                {"media_type",x.MediaType },
                {"model",x.Model},
                {"partitions",x.Partitions },
                {"serial_number",x.SerialNumber },
                {"created_at",DateTime.UtcNow}
            }).ToList<BsonDocument>();
                // 16. Other Specification
                var _other_specification = new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"anti_virus",model.OtherSpecifications.Antivirus },
                {"mac_address",model.OtherSpecifications.MACAAddress },
                {"installed_ram",model.OtherSpecifications.InstalledRAM },
                {"bios_version",model.OtherSpecifications.BIOSVersion},
                {"cpu_name",model.OtherSpecifications.CPUName },
                {"system_uptime",model.OtherSpecifications.SystemUptime},
                {"system_model",model.OtherSpecifications.SystemModel },
                {"system_manufacturer",model.OtherSpecifications.SystemManufacturer },
                {"os_version",model.OtherSpecifications.OSVersion},
                {"os_build_version",model.OtherSpecifications.OSBuildVersion },
                {"serial_number",model.OtherSpecifications.SerialNumber},
                {"created_at",DateTime.UtcNow}
            };
                // 17. Account Details
                var _account = model.Accounts.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"name",x.Name},
                {"account_type",x.AccountType},
                {"caption",x.Caption},
                {"domain",x.Domain },
                {"sid",x.SID},
                {"created_at",DateTime.UtcNow}
            }).ToList<BsonDocument>();
                // 18. Active Ports
                var _active_ports = model.ActivePorts.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"pid",x.PID},
                {"proto",x.Proto},
                {"forign_address",x.ForeignAddress},
                {"local_address",x.LocalAddress},
                {"state",x.State},
                {"task_name",x.TaskName},
                {"created_at",DateTime.UtcNow}
            }).ToList<BsonDocument>();
                // 19. Active Network Details
                var _active_network_details = model.ActiveNetworkDetails.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"ip_address",x.IpAddress},
                {"mac_address",x.MacAddress},
                {"default_gateway",x.DefaultGateway},
                {"description",x.Description },
                {"dhcp_enabled",x.DhcpEnabled },
                {"dns_server",x.DnsServers},
                {"subnet_mask",x.SubnetMask},
                {"created_at",DateTime.UtcNow}
            }).ToList<BsonDocument>();
                // 20. ResourceUtil
                var _resource_util = new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"byte_sent",model.ResourceUtils.BytesSent },
                {"byte_received",model.ResourceUtils.BytesReceived},
                {"cpu_usage",model.ResourceUtils.CPUUsage },
                {"memory_usage",model.ResourceUtils.MemoryUsage},
                {"physical_disk_usage",model.ResourceUtils.PhysicalDiskUsage},
                {"gpu_usage",model.ResourceUtils.GPUUsage},
                {"created_at",DateTime.UtcNow}
            };
                // 21. Disk Details
                var _disk_details = model.DiskDetails.Select(x => new BsonDocument
            {{ "_id", Guid.NewGuid().ToString() },
                {"capacity",x.Capacity},
                {"device_id",x.DeviceID },
                {"firmware_revision",x.FirmwareRevision},
                {"index",x.Index},
                {"installed_date",x.InstallDate},
                {"interface_type",x.InterfaceType},
                {"manufacturer",x.Manufacturer},
                {"model",x.Model},
                {"media_type",x.MediaType},
                {"serial_number",x.SerialNumber },
                {"status",x.Status},
                {"created_at",DateTime.UtcNow},
                {"partitions",x.Partitions },
                {
                    "partition_details",
                    x.PartitionDetails!=null? new BsonArray(x.PartitionDetails.Select(p => new BsonDocument
                    {
                        { "_id", Guid.NewGuid().ToString() },
                                                        {"index", p.Index},
                                                        {"disk_index", p.DiskIndex},
                                                        {"bootable", p.Bootable},
                                                        {"boot_partition", p.BootPartition},
                                                        {"primary_partition", p.PrimaryPartition},
                                                        {"size", p.Size},
                                                        {"state", p.State},
                                                        {"drive_letter", p.DriveLetter},
                                                        {"file_system", p.FileSystem},
                                                        {"free_space", p.FreeSpace},
                                                        {"used_space", p.UsedSpace},
                                                        {"description", p.Description},
                                                        {"volumne_name", p.VolumeName}
                    })) : new BsonArray()
                }

            }).ToList<BsonDocument>();

                var eaSpecifyFlag = await UpdateEASpecification(model.ObjectID, model.EASpecificationDTO);
                if (!eaSpecifyFlag.Status)
                    return new MongoError() { Status = false, Message = "failed to update ea specification" };

                var tmFlag = await AddTaskManager(model.TaskManager, model.EASpecificationDTO.SystemName, model.ObjectID, model.EASpecificationDTO.OrgID);
                if (!tmFlag.Status)
                    return new MongoError() { Status = false, Message = "failed to insert task manager utilization" };

                var newDoc = new BsonDocument
                {
                    { "asset_unique_id", model.ObjectID },
                    {"org_id",Convert.ToInt32(model.OrgID)},
                    { "created_at", DateTime.UtcNow },
                    { "location", _location },
                    { "installed_apps", new BsonArray(_installedApps) },
                    { "updates", new BsonArray(_updates) },
                    { "ports", new BsonArray(_ports) },
                    { "services", new BsonArray(_service) },
                    { "processes", new BsonArray(_process) },
                    { "processors", new BsonArray(_processor) },
                    { "rams", new BsonArray(_ram) },
                    { "scheduled_tasks", new BsonArray(_scheduled_task) },
                    { "storage_volumns", new BsonArray(_storage_volumne) },
                    { "graphic_cards", new BsonArray(_graphic_card) },
                    { "raids", new BsonArray(_raid) },
                    { "network_adapters", new BsonArray(_network_adapter) },
                    { "physical_drivers", new BsonArray(_physical_drivers) },
                    { "other_specification", _other_specification },
                    { "accounts", new BsonArray(_account) },
                    { "active_ports", new BsonArray(_active_ports) },
                    { "active_network_ports", new BsonArray(_active_network_details) },
                    { "resource_util", _resource_util },
                    { "disk_details", new BsonArray(_disk_details) }
                };

                await DevicesInfoCollections.InsertOneAsync(newDoc);


                return new MongoError() { Status = true, Message = "Success" };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }

        public async Task<POST_ALL_DTO> GetDevicesInformation(string _objectId, string _orgId)
        {
            try
            {
                POST_ALL_DTO _deviceInfo = null;
                var _filter = new BsonDocument { { "asset_unique_id", _objectId }, { "org_id", Convert.ToInt32(_orgId) } };
                // Sort by 'created_at' in descending order to get the latest document
                var _sort = Builders<BsonDocument>.Sort.Descending("created_at");
                var _result = await DevicesInfoCollections.Find(_filter).Sort(_sort).FirstOrDefaultAsync();

                if (_result != null)
                {
                    _deviceInfo = new POST_ALL_DTO();

                    //Date
                    DateTime _date = Convert.ToDateTime(_result["created_at"]).ToLocalTime();

                    // 1. Location
                    var location = _result["location"].AsBsonDocument;
                    _deviceInfo.LocationDTO = new LocationDTO()
                    {
                        Latitude = location.Contains("latitude") ? location["latitude"].ToString() : "",
                        Longitude = location.Contains("longitude") ? location["longitude"].ToString() : "",
                    };
                    // 2. EASpecification
                    _deviceInfo.EASpecificationDTO = await GetEASpecificationByAssetId(_objectId, _orgId);
                    // 3. Installed Apps
                    var _installedApps = _result["installed_apps"].AsBsonArray;
                    _deviceInfo.InstalledApps = new List<TEST_WebApiOsDetails.Models.Dto.InstalledAppDto>();
                    foreach (var app in _installedApps)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.InstalledApps.Add(new TEST_WebApiOsDetails.Models.Dto.InstalledAppDto()
                        {
                            AppName = _appObj["app_name"].ToString(),
                            Provider = _appObj["provider"].ToString(),
                            Size = _appObj["size"].ToString(),
                            InstalledOn = _appObj["installed_on"].ToString(),
                            Version = _appObj["version"].ToString()
                        });
                    }
                    // 4. updates
                    var _updates = _result["updates"].AsBsonArray;
                    _deviceInfo.Updates = new List<TEST_WebApiOsDetails.Models.Dto.UpdateDto>();
                    foreach (var app in _updates)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.Updates.Add(new TEST_WebApiOsDetails.Models.Dto.UpdateDto()
                        {
                            Patch = _appObj["patch"].ToString(),
                            Title = _appObj["title"].ToString(),
                            Description = _appObj["description"].ToString(),
                            InstalledOn = _appObj["installed_on"].ToString(),
                            Version = _appObj["version"].ToString()
                        });
                    }
                    // 5. ports
                    var _ports = _result["ports"].AsBsonArray;
                    _deviceInfo.Ports = new List<TEST_WebApiOsDetails.Models.Dto.PortDto>();
                    foreach (var app in _ports)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.Ports.Add(new TEST_WebApiOsDetails.Models.Dto.PortDto()
                        {
                            PortNumber = _appObj["port_number"].ToString(),
                            ProcessId = _appObj["process_id"].ToString(),
                            ProcessName = _appObj["process_name"].ToString()
                        });
                    }
                    // 6. services
                    var _services = _result["services"].AsBsonArray;
                    _deviceInfo.Services = new List<ServiceDTO>();
                    foreach (var app in _services)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.Services.Add(new ServiceDTO()
                        {
                            ServiceName = _appObj["service_name"].ToString(),
                            DisplayName = _appObj["display_name"].ToString(),
                            ServiceType = _appObj["service_type"].ToString(),
                            ServiceStatus = _appObj["service_status"].ToString(),
                            StartType = _appObj["start_type"].ToString(),
                            PID = _appObj["pid"].ToString(),
                        });
                    }
                    // 7. processes
                    var _processes = _result["processes"].AsBsonArray;
                    _deviceInfo.Processes = new List<ProcessDTO>();
                    foreach (var app in _processes)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.Processes.Add(new ProcessDTO()
                        {
                            ProcessName = _appObj["process_name"].ToString(),
                            ProcessId = _appObj["process_id"].ToString(),
                            Status = _appObj["status"].ToString(),
                            Username = _appObj["username"].ToString(),
                            MemoryUsage = _appObj["memory_usage"].ToString(),
                            Description = _appObj["description"].ToString(),
                            Path = _appObj["path"].ToString()
                        });
                    }
                    // 8. processors
                    var _processors = _result["processors"].AsBsonArray;
                    _deviceInfo.Processors = new List<ProcessorDTO>();
                    foreach (var app in _processors)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.Processors.Add(new ProcessorDTO()
                        {
                            DeviceID = _appObj["device_id"].ToString(),
                            Name = _appObj["name"].ToString(),
                            Manufacturer = _appObj["manufacturer"].ToString(),
                            MaxClockSpeed = _appObj["max_clock_speed"].ToString(),
                            Cores = _appObj["cores"].ToString(),
                            LogicalProcessors = _appObj["logical_processors"].ToString(),
                            ProcessorId = _appObj["processor_id"].ToString()
                        });
                    }
                    // 9. rams
                    var _rams = _result["rams"].AsBsonArray;
                    _deviceInfo.RAMDetails = new List<RAMDetailDTO>();
                    foreach (var app in _rams)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.RAMDetails.Add(new RAMDetailDTO()
                        {
                            BankLabel = _appObj["bank_label"].ToString(),
                            Capacity = _appObj["capacity"].ToString(),
                            Manufacturer = _appObj["manufacturer"].ToString(),
                            Description = _appObj["description"].ToString(),
                            MemoryType = _appObj["memory_type"].ToString(),
                            PartNumber = _appObj["part_number"].ToString(),
                            SerialNumber = _appObj["serial_number"].ToString(),
                            Speed = _appObj["speed"].ToString(),
                            SMBIOSMemoryType = _appObj["sm_bios_memory_type"].ToString()
                        });
                    }
                    // 10. scheduled_tasks
                    var _scheduled_tasks = _result["scheduled_tasks"].AsBsonArray;
                    _deviceInfo.ScheduledTasks = new List<ScheduledTaskDTO>();
                    foreach (var app in _scheduled_tasks)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.ScheduledTasks.Add(new ScheduledTaskDTO()
                        {
                            Name = _appObj["name"].ToString(),
                            Status = _appObj["status"].ToString(),
                            Author = _appObj["author"].ToString(),
                            Path = _appObj["path"].ToString(),
                            Trigger = _appObj["trigger"].ToString(),
                            LastRunResult = _appObj["last_run_result"].ToString(),
                            LastRunTime = _appObj["last_run_time"].ToString(),
                            NextRunTime = _appObj["next_run_time"].ToString(),
                            CreatedDate = _appObj["created_at"].ToString()
                        });
                    }
                    // 11. storage_volumns
                    var _storage_volumns = _result["storage_volumns"].AsBsonArray;
                    _deviceInfo.StorageVolumes = new List<StorageVolumeDTO>();
                    foreach (var app in _storage_volumns)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.StorageVolumes.Add(new StorageVolumeDTO()
                        {
                            BootVolume = _appObj["boot_volumne"].ToString(),
                            Capacity = _appObj["capacity"].ToString(),
                            DriveLetter = _appObj["drive_letter"].ToString(),
                            FileSystem = _appObj["file_system"].ToString(),
                            FreeSpace = _appObj["free_system"].ToString(),
                            Label = _appObj["label"].ToString(),
                            SystemVolume = _appObj["system_volumne"].ToString()
                        });
                    }
                    // 12. graphic_cards
                    var _graphic_cards = _result["graphic_cards"].AsBsonArray;
                    _deviceInfo.GraphicCards = new List<GraphicCardDTO>();
                    foreach (var app in _graphic_cards)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.GraphicCards.Add(new GraphicCardDTO()
                        {
                            AdapterCompatibility = _appObj["adapter_compatibility"].ToString(),
                            AdapterRAM = _appObj["adapter_ram"].ToString(),
                            Caption = _appObj["caption"].ToString(),
                            DeviceID = _appObj["device_id"].ToString(),
                            VideoProcessor = _appObj["video_processor"].ToString()
                        });
                    }
                    // 13. raids
                    var _raids = _result["raids"].AsBsonArray;
                    _deviceInfo.RAIDControllers = new List<RAIDControllerDTO>();
                    foreach (var app in _raids)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.RAIDControllers.Add(new RAIDControllerDTO()
                        {
                            //AdapterCompatibility = _appObj["adapter_compatibility"].ToString(),
                            //AdapterRAM = _appObj["adapter_ram"].ToString(),
                            //Caption = _appObj["caption"].ToString(),
                            //DeviceID = _appObj["device_id"].ToString(),
                            //VideoProcessor = _appObj["video_processor"].ToString()
                        });
                    }
                    // 14. network_adapters
                    var _network_adapters = _result["network_adapters"].AsBsonArray;
                    _deviceInfo.NetworkAdapters = new List<NetworkAdapterDTO>();
                    foreach (var app in _network_adapters)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.NetworkAdapters.Add(new NetworkAdapterDTO()
                        {
                            Name = _appObj["name"].ToString(),
                            Status = _appObj["status"].ToString(),
                            Description = _appObj["description"].ToString(),
                            InterfaceIndex = _appObj["interface_index"].ToString(),
                            MACAddress = _appObj["mac_address"].ToString(),
                            Speed = _appObj["speed"].ToString()
                        });
                    }
                    // 15. physical_drivers
                    var _physical_drivers = _result["physical_drivers"].AsBsonArray;
                    _deviceInfo.PhysicalDrives = new List<PhysicalDriveDTO>();
                    foreach (var app in _physical_drivers)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.PhysicalDrives.Add(new PhysicalDriveDTO()
                        {
                            DeviceID = _appObj["device_id"].ToString(),
                            FirmwareRevision = _appObj["firmware_revision"].ToString(),
                            Index = _appObj["index"].ToString(),
                            InterfaceType = _appObj["interface_type"].ToString(),
                            MediaType = _appObj["media_type"].ToString(),
                            Model = _appObj["model"].ToString(),
                            Partitions = _appObj["partitions"].ToString(),
                            SerialNumber = _appObj["serial_number"].ToString()
                        });
                    }
                    // 16. other_specification
                    var _other_specification = _result["other_specification"].AsBsonDocument;
                    _deviceInfo.OtherSpecifications = new OtherSpecificationDTO()
                    {
                        Antivirus = _other_specification["anti_virus"].ToString(),
                        MACAAddress = _other_specification["mac_address"].ToString(),
                        InstalledRAM = _other_specification["installed_ram"].ToString(),
                        BIOSVersion = _other_specification["bios_version"].ToString(),
                        CPUName = _other_specification["cpu_name"].ToString(),
                        SystemUptime = _other_specification["system_uptime"].ToString(),
                        SystemModel = _other_specification["system_model"].ToString(),
                        SystemManufacturer = _other_specification["system_manufacturer"].ToString(),
                        OSVersion = _other_specification["os_version"].ToString(),
                        OSBuildVersion = _other_specification["os_build_version"].ToString(),
                        SerialNumber = _other_specification["serial_number"].ToString()
                    };
                    // 17. accounts
                    var _accounts = _result["accounts"].AsBsonArray;
                    _deviceInfo.Accounts = new List<AccountDTO>();
                    foreach (var app in _accounts)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.Accounts.Add(new AccountDTO()
                        {
                            Name = _appObj["name"].ToString(),
                            AccountType = _appObj["account_type"].ToString(),
                            Caption = _appObj["caption"].ToString(),
                            Domain = _appObj["domain"].ToString(),
                            SID = _appObj["sid"].ToString()
                        });
                    }
                    // 18. active_ports
                    var _active_ports = _result["active_ports"].AsBsonArray;
                    _deviceInfo.ActivePorts = new List<ActivePortDTO>();
                    foreach (var app in _active_ports)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.ActivePorts.Add(new ActivePortDTO()
                        {
                            PID = _appObj["pid"].ToString(),
                            Proto = _appObj["proto"].ToString(),
                            ForeignAddress = _appObj["forign_address"].ToString(),
                            LocalAddress = _appObj["local_address"].ToString(),
                            State = _appObj["state"].ToString(),
                            TaskName = _appObj["task_name"].ToString()
                        });
                    }
                    // 19. active_network_ports
                    var _active_network_ports = _result["active_network_ports"].AsBsonArray;
                    _deviceInfo.ActiveNetworkDetails = new List<ActiveNetworkDetailDTO>();
                    foreach (var app in _active_network_ports)
                    {
                        var _appObj = app.AsBsonDocument;
                        _deviceInfo.ActiveNetworkDetails.Add(new ActiveNetworkDetailDTO()
                        {
                            IpAddress = _appObj["ip_address"].ToString(),
                            MacAddress = _appObj["mac_address"].ToString(),
                            DefaultGateway = _appObj["default_gateway"].ToString(),
                            Description = _appObj["description"].ToString(),
                            DhcpEnabled = Convert.ToBoolean(_appObj["dhcp_enabled"].ToString()),
                            DnsServers = _appObj["dns_server"].ToString(),
                            SubnetMask = _appObj["subnet_mask"].ToString()
                        });
                    }
                    // 20. resource_util   
                    var _resource_util = _result["resource_util"].AsBsonDocument;
                    _deviceInfo.ResourceUtils = new ResourceUtilDTO()
                    {
                        BytesSent = _resource_util["byte_sent"].ToString() != "-" ? Convert.ToInt64(_resource_util["byte_sent"].ToString()) : 0,
                        BytesReceived = _resource_util["byte_received"].ToString() != "-" ? Convert.ToInt64(_resource_util["byte_received"].ToString()) : 0,
                    };
                    float _cpuUsage = 0.0f, _memoryUsage = 0.0f, _physicalDiskUsage = 0.0f, _gpuUsage = 0.0f;

                    if (_resource_util.Contains("cpu_usage") &&
                        float.TryParse(_resource_util["cpu_usage"].ToString(), out float parsedCpu))
                    {
                        _cpuUsage = parsedCpu;
                    }
                    if (_resource_util.Contains("memory_usage") &&
                        float.TryParse(_resource_util["memory_usage"].ToString(), out float parsedmemory))
                    {
                        _memoryUsage = parsedmemory;
                    }
                    if (_resource_util.Contains("physical_disk_usage") &&
                        float.TryParse(_resource_util["physical_disk_usage"].ToString(), out float parsedphysicaldisk))
                    {
                        _physicalDiskUsage = parsedphysicaldisk;
                    }
                    if (_resource_util.Contains("gpu_usage") &&
                        float.TryParse(_resource_util["gpu_usage"].ToString(), out float parsegpu))
                    {
                        _gpuUsage = parsegpu;
                    }

                    _deviceInfo.ResourceUtils.CPUUsage = _cpuUsage;
                    _deviceInfo.ResourceUtils.MemoryUsage = _memoryUsage;
                    _deviceInfo.ResourceUtils.PhysicalDiskUsage = _physicalDiskUsage;
                    _deviceInfo.ResourceUtils.GPUUsage = _gpuUsage;
                    // 21. disk_details
                    var _disk_details = _result["disk_details"].AsBsonArray;
                    _deviceInfo.DiskDetails = new List<DiskDetailDTO>();
                    foreach (var app in _disk_details)
                    {
                        var _appObj = app.AsBsonDocument;
                        var _partitionList = _appObj["partition_details"].AsBsonArray;
                        var partitionList = new List<PartitionDetailDTO>();
                        if (_partitionList != null)
                        {
                            foreach (var _app in _partitionList)
                            {
                                var _partitionDetailsObj = _app.AsBsonDocument;
                                partitionList.Add(new PartitionDetailDTO()
                                {
                                    Index = _partitionDetailsObj["index"].ToString(),
                                    DiskIndex = _partitionDetailsObj["disk_index"].ToString(),
                                    Bootable = _partitionDetailsObj["bootable"].ToString(),
                                    BootPartition = _partitionDetailsObj["boot_partition"].ToString(),
                                    PrimaryPartition = _partitionDetailsObj["primary_partition"].ToString(),
                                    Size = _partitionDetailsObj["size"].ToString(),
                                    State = _partitionDetailsObj["state"].ToString(),
                                    DriveLetter = _partitionDetailsObj["drive_letter"].ToString(),
                                    FileSystem = _partitionDetailsObj["file_system"].ToString(),
                                    FreeSpace = _partitionDetailsObj["free_space"].ToString(),
                                    UsedSpace = _partitionDetailsObj["used_space"].ToString(),
                                    Description = _partitionDetailsObj["description"].ToString(),
                                    VolumeName = _partitionDetailsObj["volumne_name"].ToString(),
                                });
                            }
                        }
                        _deviceInfo.DiskDetails.Add(new DiskDetailDTO()
                        {
                            Capacity = _appObj["capacity"].ToString(),
                            DeviceID = _appObj["device_id"].ToString(),
                            FirmwareRevision = _appObj["firmware_revision"].ToString(),
                            Index = _appObj["index"].ToString(),
                            InstallDate = _appObj["installed_date"].ToString(),
                            Manufacturer = _appObj["manufacturer"].ToString(),
                            Model = _appObj["model"].ToString(),
                            MediaType = _appObj["media_type"].ToString(),
                            SerialNumber = _appObj["serial_number"].ToString(),
                            Status = _appObj["status"].ToString(),
                            Partitions = _appObj["partitions"].ToString(),
                            PartitionDetails = partitionList
                        });

                    }

                }

                return _deviceInfo;
            }
            catch (Exception ex)
            {
                return null;
                throw;
            }
        }

        public async Task<List<UpdateDto>> GetUpdates(string _objectId, string _orgId)
        {
            var _getAllDevicesInfo = await GetDevicesInformation(_objectId, _orgId);
            if (_getAllDevicesInfo != null)
                return _getAllDevicesInfo.Updates;

            return new List<UpdateDto>();
        }
        public async Task<List<UpdateDto>> GetDevicesUpdates(string assetId, string orgId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("asset_unique_id", assetId),
                Builders<BsonDocument>.Filter.Eq("org_id", Convert.ToInt32(orgId))
            );

            // ✅ Only fetch installed_apps — avoids transferring the huge document
            var projection = Builders<BsonDocument>.Projection
                .Include("updates")
                .Include("created_at")
                .Exclude("_id");

            var document = await DevicesInfoCollections
                .Find(filter)
                .SortByDescending(d => d["created_at"])
                .Project(projection)
                .FirstOrDefaultAsync();

            if (document == null || !document.Contains("updates"))
                return new List<UpdateDto>();

            var apps = new List<UpdateDto>();

            foreach (var item in document["updates"].AsBsonArray)
            {
                var appDoc = item.AsBsonDocument;
                apps.Add(new UpdateDto
                {
                    Patch = appDoc["patch"].ToString(),
                    Title = appDoc["title"].ToString(),
                    Description = appDoc["description"].ToString(),
                    InstalledOn = appDoc["installed_on"].ToString(),
                    Version = appDoc["version"].ToString()
                });
            }

            return apps;
        }

        public async Task<List<InstalledAppDto>> GetInstalledApps(string _objectId, string _orgId)
        {
            var _getAllApps = await GetDevicesInformation(_objectId, _orgId);
            if (_getAllApps != null)
                return _getAllApps.InstalledApps;

            return new List<InstalledAppDto>();
        }

        public async Task<List<InstalledAppDto>> GetDevicesInstalledApps(string assetId, string orgId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("asset_unique_id", assetId),
                Builders<BsonDocument>.Filter.Eq("org_id", Convert.ToInt32(orgId))
            );

            // ✅ Only fetch installed_apps — avoids transferring the huge document
            var projection = Builders<BsonDocument>.Projection
                .Include("installed_apps")
                .Include("created_at")
                .Exclude("_id");

            var document = await DevicesInfoCollections
                .Find(filter)
                .SortByDescending(d => d["created_at"])
                .Project(projection)
                .FirstOrDefaultAsync();

            if (document == null || !document.Contains("installed_apps"))
                return new List<InstalledAppDto>();

            var apps = new List<InstalledAppDto>();

            foreach (var item in document["installed_apps"].AsBsonArray)
            {
                var appDoc = item.AsBsonDocument;
                apps.Add(new InstalledAppDto
                {
                    AppName = appDoc.GetValue("app_name", "").AsString,
                    Provider = appDoc.GetValue("provider", "").AsString,
                    Size = appDoc.GetValue("size", "").AsString,
                    InstalledOn = appDoc.GetValue("installed_on", "").AsString,
                    Version = appDoc.GetValue("version", "").AsString,
                    AssetId = assetId,
                    OrgId = orgId
                });
            }

            return apps;
        }

        public async Task<List<InstalledAppDto>> GetInstalledApps(string _orgId)
        {
            POST_ALL_DTO _deviceInfo = null;
            var _filter = new BsonDocument { { "org_id", Convert.ToInt32(_orgId) } };
            // Sort by 'created_at' in descending order to get the latest document
            var _sort = Builders<BsonDocument>.Sort.Descending("created_at");
            var _result = await DevicesInfoCollections.Find(_filter).Sort(_sort).FirstOrDefaultAsync();
            string _objectId = string.Empty;
            if (_result != null)
            {
                _objectId = _result["asset_unique_id"].AsString;
                return await GetInstalledApps(_objectId, _orgId);
            }

            return null;

        }

        #endregion

        #region FileUpload
        public async Task<List<string>> GetFileUploadIDs()
        {
            var documents = await CentralOSPatchesCollections
                .Find(Builders<BsonDocument>.Filter.Empty)
                .Project(Builders<BsonDocument>.Projection.Include("_id"))
                .ToListAsync();

            return documents
                .Where(d => d.Contains("_id") && d["_id"].IsObjectId)
                .Select(d => d["_id"].AsObjectId.ToString())
                .ToList();
        }

        public async Task<EPTPatchDataDTO> GetFileUploadeById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(id));

            var patch = await CentralOSPatchesCollections
                .Find(filter)
                .FirstOrDefaultAsync();

            if (patch == null)
                return new EPTPatchDataDTO();

            return new EPTPatchDataDTO
            {
                PatchID = patch["_id"].AsObjectId.ToString(),
                PatchName = patch.Contains("title") ? patch["title"].ToString() : "",
                KBNumber = patch.Contains("kb_number") ? patch["kb_number"].ToString() : "",
                KBNumberDescription = patch.Contains("kb_description") ? patch["kb_description"].ToString() : "",
                PatchOS = patch.Contains("update_os") ? patch["update_os"].ToString() : "",
                PatchBitRate = patch.Contains("bit_rate") ? patch["bit_rate"].ToString() : "",
                PatchStatus = "Available",
                PatchFilePath = patch.Contains("patch_path") ? patch["patch_path"].ToString() : "",
            };
        }

        public async Task<List<FileUploadDTO>> GetFileUploades(string _orgId)
        {
            var _filter = new BsonDocument { { "org_id", Convert.ToInt32(_orgId) } };
            var _result = await FileUploadCollections.Find(_filter).ToListAsync();
            if (_result != null)
            {
                var _files = new List<FileUploadDTO>();
                foreach (var _file in _result)
                {
                    FileUploadDTO file = new FileUploadDTO()
                    {
                        Id = _file["_id"].ToString(),
                        OrgID = _file["org_id"].ToString(),
                        UpdateID = _file["kb_updateId"].ToString(),
                        UpdateName = _file["update_name"].ToString(),
                        UpdateOS = _file["update_os"].ToString(),
                        KBNumber = _file["kb_number"].ToString(),
                        KBNumberDescription = _file.TryGetValue("kb_number_description", out var desc) ? desc.AsString : string.Empty,
                        UpdateBitRate = _file["update_bit_rate"].ToString(),
                        UploadedFileBase64 = string.Empty
                    };

                    _files.Add(file);
                }

                return _files;
            }

            return new List<FileUploadDTO>();
        }

        public async Task<string> GetPatchIdFromFileUpload(string patchName, string kbnumber)
        {
            // First check OS patches
            var osFilter = new BsonDocument
            {
                { "update_name", patchName },
                { "kb_number", kbnumber }
            };

            var osResult = await CentralOSPatchesCollections
                                .Find(osFilter)
                                .FirstOrDefaultAsync();

            if (osResult != null)
                return osResult["_id"].ToString();

            // If not found, check software patches
            var softwareFilter = new BsonDocument
            {
                { "software_name", patchName },
                { "patch_number", kbnumber },
                { "is_deleted", false }
            };

            var softwareResult = await CentralSoftwarePatchesCollections
                                        .Find(softwareFilter)
                                        .FirstOrDefaultAsync();

            if (softwareResult != null)
                return softwareResult["_id"].ToString();

            return null;
        }

        public async Task<MongoError> AddFileUpload(FileUploadDTO model, string normalizedFilePath, string _orgId)
        {
            try
            {
                var _file = new BsonDocument
            {
                { "org_id",Convert.ToInt32(_orgId)},
                {"kb_updateId",model.UpdateID },
                {"update_name", model.UpdateName },
                {"update_os",model.UpdateOS },
                {"update_bit_rate",model.UpdateBitRate },
                {"kb_number",model.KBNumber },
                {"kb_number_description",model.KBNumberDescription??"" },
                {"Install_parameters",model.Parameters??"" },
                {"file_path", normalizedFilePath},
                {"created_at", DateTime.UtcNow }
            };
                await FileUploadCollections.InsertOneAsync(_file);
                return new MongoError() { Status = true, Message = "Success" };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }

        #endregion

        #region Software
        public async Task<List<string>> GetSoftwareFileUploadIDs()
        {
            var documents = await CentralSoftwarePatchesCollections
                .Find(Builders<BsonDocument>.Filter.Empty)
                .Project(Builders<BsonDocument>.Projection.Include("_id"))
                .ToListAsync();

            return documents
                .Where(d => d.Contains("_id") && d["_id"].IsObjectId)
                .Select(d => d["_id"].AsObjectId.ToString())
                .ToList();
        }

        public async Task<EPTPatchDataDTO> GetSoftwareFileUploadeById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(id));

            var patch = await CentralSoftwarePatchesCollections
                .Find(filter)
                .FirstOrDefaultAsync();

            if (patch == null)
                return new EPTPatchDataDTO();

            return new EPTPatchDataDTO
            {
                PatchID = patch["_id"].AsObjectId.ToString(),
                PatchName = patch.Contains("vendor") ? patch["vendor"].ToString() : "",
                KBNumber = patch.Contains("patch_number") ? patch["patch_number"].ToString() : "",
                KBNumberDescription = patch.Contains("patch_description") ? patch["patch_description"].ToString() : "",
                PatchOS = patch.Contains("platform") ? patch["platform"].ToString() : "",
                PatchBitRate = patch.Contains("bit_rate") ? patch["bit_rate"].ToString() : "",
                PatchStatus = "Available",
                PatchFilePath = patch.Contains("patch_path") ? patch["patch_path"].ToString() : "",
            };
        }

        #endregion


        #region PatchQueue

        public async Task<PatchQueueDTO> GetPatchQueueByDeviceId(string _deviceId)
        {
            var _patchQueueilter = new BsonDocument { { "device_id", _deviceId } };
            var _patchQueue = await PatchQueueCollections.Find(_patchQueueilter).FirstOrDefaultAsync();
            if (_patchQueue != null)
            {
                var patchQueue = new PatchQueueDTO()
                {
                    ObjID = _patchQueue["_id"].AsString,
                    DeviceID = _patchQueue["device_id"].AsString,
                    Status = _patchQueue["status"].AsString,
                    CurrentVersion = _patchQueue["current_version"].AsString,
                    InQueueVersion = _patchQueue["inqueue_version"].AsString,
                    InQueue = Convert.ToBoolean(_patchQueue["inqueue"].AsBoolean),
                    OrgId = _patchQueue["org_id"].AsString,
                };

                return patchQueue;
            }

            return null;
        }
        public async Task<MongoError> PatchQueue(PatchQueueDTO value, string _deviceId)
        {
            try
            {
                var _patchQueueilter = new BsonDocument { { "device_id", _deviceId }, { "org_id", value.OrgId } };
                var _patchQueue = await PatchQueueCollections.Find(_patchQueueilter).FirstOrDefaultAsync();
                if (_patchQueue == null)
                {
                    var _file = new BsonDocument
                    {
                        {"_id", Guid.NewGuid().ToString() },
                        {"created_at", DateTime.UtcNow },
                        {"updated_at", DateTime.UtcNow },
                        {"current_version", value.CurrentVersion },
                        {"inqueue", value.InQueue },
                        {"inqueue_version", value.InQueueVersion },
                        {"status", value.Status },
                        {"device_id", _deviceId },
                        {"org_id", value.OrgId}
                    };
                    await PatchQueueCollections.InsertOneAsync(_file);
                    return new MongoError() { Status = true, Message = "Success" };
                }
                else
                {
                    string _currentVersion = _patchQueue["current_version"].ToString() ?? string.Empty;
                    if (value.CurrentVersion == _currentVersion)
                        return new MongoError() { Status = true, Message = "Success" };

                    var _patchQueueupdate = Builders<BsonDocument>.Update.Set("updated_at", DateTime.Now)
                                                           .Set("current_version", value.CurrentVersion)
                                                           .Set("inqueue", value.InQueue)
                                                           .Set("inqueue_version", value.InQueueVersion)
                                                           .Set("status", value.Status);

                    await PatchQueueCollections.UpdateOneAsync(_patchQueueilter, _patchQueueupdate, new UpdateOptions { IsUpsert = true });
                    return new MongoError() { Status = true, Message = "Success" };
                }
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }

        #endregion

        #region Update Patch Queue

        public async Task<string> GetUpdatePatchQueueStatusOnly(string systemId, string patchName, string kbNumber)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("system_id", systemId),   // ✅ FIXED
                Builders<BsonDocument>.Filter.Or(
                    Builders<BsonDocument>.Filter.Eq("update_name", patchName),
                    Builders<BsonDocument>.Filter.Eq("update_kb_number", kbNumber)
                )
            );

            var projection = Builders<BsonDocument>.Projection
                .Include("reason")
                .Include("status")
                .Exclude("_id");

            var document = await UpdatePatchQueueCollections
                .Find(filter)
                .Project(projection)
                .FirstOrDefaultAsync();

            if (document == null || document.TryGetValue("status", out var _status) && _status == "0")
                return "available";

            // business rule: return reason only if status != "1"
            if (document.TryGetValue("status", out var status) && status != "1")
            {
                return document.GetValue("reason", "").ToString();
            }

            return document.GetValue("reason", "").ToString();
        }


        public async Task<MongoError> UpdatePatchQueue(TEST_WebApiOsDetails.Models.UpdatePatchQueue value)
        {
            try
            {
                var doc = new BsonDocument
                {
                    { "_id", Guid.NewGuid().ToString() },
                    {"org_id",value.OrgId },
                    { "patch_id", value.PatchId },
                    { "system_id", value.SystemID },
                    { "patch_file_name", value.PatchFileName },
                    { "patch_file_path", value.PatchFilePath },
                    { "update_kb_number", value.UpdateKBNumber },
                    { "status", value.Status },
                    { "reason", value.Reason },
                    {"scheduled_time",value.ScheduledTime },
                    { "created_at", value.CreatedAt }

                };

                await UpdatePatchQueueCollections.InsertOneAsync(doc);
                return new MongoError { Status = true, Message = "Success" };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }

        public bool CheckUpdatePatchQueueAny(string _deviceId, string _patchName)
        {
            var _patchFilter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("device_id", _deviceId), Builders<BsonDocument>.Filter.Eq("update_name", _patchName));
            var exists = UpdatePatchQueueCollections.Find(_patchFilter).Any();
            return exists;
        }
        public bool CheckUpdatePatchQueueAny(string _deviceId, string _patchName, string _kbNumber)
        {
            var _patchFilter = Builders<BsonDocument>.Filter.And(
                                                            Builders<BsonDocument>.Filter.Eq("device_id", _deviceId),
                                                            Builders<BsonDocument>.Filter.Or(
                                                                Builders<BsonDocument>.Filter.Eq("update_name", _patchName),
                                                                Builders<BsonDocument>.Filter.Eq("update_kb_number", _kbNumber),
                                                                Builders<BsonDocument>.Filter.Eq("status", "1")
                                                            )
                                                        );
            var exists = UpdatePatchQueueCollections.Find(_patchFilter).Any();

            return exists;

        }

        public async Task<List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>> GetUpdatePatchQueues(string system_id, string status = null)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("system_id", system_id);

            if (!string.IsNullOrEmpty(status))
            {
                filter &= Builders<BsonDocument>.Filter.Eq("status", status);
            }

            var docs = await UpdatePatchQueueCollections.Find(filter).ToListAsync();
            var result = new List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>();

            foreach (var doc in docs)
            {
                result.Add(new TEST_WebApiOsDetails.Models.UpdatePatchQueue
                {
                    ID = doc.GetValue("_id", "").AsString,
                    SystemID = doc.GetValue("system_id", "").AsString,
                    PatchId = doc.GetValue("patch_id").AsString,
                    PatchFileName = doc.GetValue("patch_file_name", "").AsString,
                    UpdateKBNumber = doc.GetValue("update_kb_number", "").AsString,
                    Status = doc.GetValue("status", "").AsString,
                    Reason = doc.GetValue("reason", "").AsString,
                    CreatedAt = doc.GetValue("created_at", BsonNull.Value).ToUniversalTime(),
                    PatchFilePath = doc.GetValue("patch_file_path", "").AsString,
                    ScheduledTime = doc.GetValue("scheduled_time", (DateTime?)null).AsNullableLocalTime
                });
            }

            return result;
        }

        public async Task<List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>> GetUpdatePatchQueuesStatus(string system_id, string status = null)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("system_id", system_id);

            if (!string.IsNullOrEmpty(status))
            {
                filter &= Builders<BsonDocument>.Filter.Eq("status", status);
            }

            // Current UTC time
            var currentTime = DateTime.UtcNow;

            // 5 minutes before
            var startTime = currentTime.AddMinutes(-5);

            //// 30 minutes before
            //var endTime = currentTime.AddMinutes(-10);

            var timeFilter = Builders<BsonDocument>.Filter.Gte("created_at", startTime) &
                             Builders<BsonDocument>.Filter.Lte("created_at", currentTime);

            filter &= timeFilter;

            var docs = await UpdatePatchQueueCollections
                .Find(filter)
                .Sort(Builders<BsonDocument>.Sort.Descending("created_at"))
                .ToListAsync();

            var result = new List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>();

            foreach (var doc in docs)
            {
                result.Add(new TEST_WebApiOsDetails.Models.UpdatePatchQueue
                {
                    ID = doc.GetValue("_id", "").AsString,
                    SystemID = doc.GetValue("system_id", "").AsString,
                    PatchId = doc.GetValue("patch_id", "").AsString,
                    PatchFileName = doc.GetValue("patch_file_name", "").AsString,
                    UpdateKBNumber = doc.GetValue("update_kb_number", "").AsString,
                    Status = doc.GetValue("status", "").AsString,
                    Reason = doc.GetValue("reason", "").AsString,
                    CreatedAt = doc.GetValue("created_at", BsonNull.Value).ToUniversalTime(),
                    PatchFilePath = doc.GetValue("patch_file_path", "").AsString,
                    ScheduledTime = doc.GetValue("scheduled_time", BsonNull.Value).ToNullableUniversalTime()
                });
            }

            return result;
        }

        //public async Task<List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>> GetUpdatePatchQueueBySystemId(string system_id)
        //{
        //    var _patchQueueFilter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("system_id", system_id));
        //    var _patchQueueList = await UpdatePatchQueueCollections.Find(_patchQueueFilter).ToListAsync();
        //    var _pqList = new List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>();
        //    if (_patchQueueList != null)
        //    {
        //        foreach (var _pq in _patchQueueList)
        //        {
        //            TEST_WebApiOsDetails.Models.UpdatePatchQueue patchQueue = new TEST_WebApiOsDetails.Models.UpdatePatchQueue()
        //            {
        //                SystemID = _pq["system_id"].ToString(),
        //                UpdateName = _pq["update_name"].ToString(),
        //                UpdateKBNumber = _pq["update_kb_number"].ToString(),
        //                Status = _pq["status"].ToString(),
        //                Reason = _pq["reason"].ToString(),
        //                CreatedAt = Convert.ToDateTime(_pq["created_at"].ToString())
        //            };

        //            _pqList.Add(patchQueue);
        //        }

        //        return _pqList;
        //    }

        //    return new List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>();
        //}

        //public async Task<List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>> GetUpdatePatchQueues(string system_id, string _status)
        //{
        //    var _patchQueueFilter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("system_id", system_id), Builders<BsonDocument>.Filter.Eq("status", _status));
        //    var _patchQueueList = await UpdatePatchQueueCollections.Find(_patchQueueFilter).ToListAsync();
        //    var _pqList = new List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>();
        //    if (_patchQueueList != null)
        //    {
        //        foreach (var _pq in _patchQueueList)
        //        {
        //            TEST_WebApiOsDetails.Models.UpdatePatchQueue patchQueue = new TEST_WebApiOsDetails.Models.UpdatePatchQueue()
        //            {
        //                SystemID = _pq["system_id"].ToString(),
        //                UpdateName = _pq["update_name"].ToString(),
        //                UpdateKBNumber = _pq["update_kb_number"].ToString(),
        //                Status = _pq["status"].ToString(),
        //                Reason = _pq["reason"].ToString(),
        //                CreatedAt = Convert.ToDateTime(_pq["created_at"].ToString())
        //            };

        //            _pqList.Add(patchQueue);
        //        }

        //        return _pqList;
        //    }

        //    return new List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>();
        //}

        public async Task<List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>> GetUpdatePatchQueuesbyStatus(string _status)
        {
            var _patchQueueFilter = Builders<BsonDocument>.Filter.Ne("status", _status);
            var _patchQueueList = await UpdatePatchQueueCollections.Find(_patchQueueFilter).ToListAsync();
            var _pqList = new List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>();
            if (_patchQueueList != null)
            {
                foreach (var _pq in _patchQueueList)
                {
                    TEST_WebApiOsDetails.Models.UpdatePatchQueue patchQueue = new TEST_WebApiOsDetails.Models.UpdatePatchQueue()
                    {
                        SystemID = _pq["system_id"].ToString(),
                        UpdateName = _pq["update_name"].ToString(),
                        UpdateKBNumber = _pq["update_kb_number"].ToString(),
                        Status = _pq["status"].ToString(),
                        Reason = _pq["reason"].ToString(),
                        CreatedAt = Convert.ToDateTime(_pq["created_at"].ToString())
                    };

                    _pqList.Add(patchQueue);
                }

                return _pqList;
            }

            return new List<TEST_WebApiOsDetails.Models.UpdatePatchQueue>();
        }

        public async Task<List<string>> GetSuccessfulPatchIds(string systemId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("system_id", systemId),
                Builders<BsonDocument>.Filter.Eq("status", "1")
            );

            var result = await UpdatePatchQueueCollections
                .Find(filter)
                .Project(Builders<BsonDocument>.Projection.Include("patch_id"))
                .ToListAsync();

            return result.Select(x => x["patch_id"].AsString).ToList();
        }
        public async Task<List<string>> GetQueuedPatchIds(string assetId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("system_id", assetId),
                Builders<BsonDocument>.Filter.In("status", new[] { "1", "2" })
            );

            var result = await UpdatePatchQueueCollections
                .Find(filter)
                .Project(Builders<BsonDocument>.Projection.Include("patch_id"))
                .ToListAsync();

            return result
                .Select(x => x.GetValue("patch_id", "").AsString)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }
        public async Task<List<string>> GetPendingQueuedPatchIds(string assetId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("system_id", assetId),
                Builders<BsonDocument>.Filter.Eq("status", "0")
            );

            var result = await UpdatePatchQueueCollections
                .Find(filter)
                .Project(Builders<BsonDocument>.Projection.Include("patch_id"))
                .ToListAsync();

            return result
                .Select(x => x.GetValue("patch_id", "").AsString)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }
        //Update
        public async Task<MongoError> UpdateUpdatePatchQueue(string _systemId, string updateName, string _status, string _reason)
        {
            var _patchQueuefilter = new BsonDocument { { "system_id", _systemId }, { "update_name", updateName } };
            var _patchQueueUpdate = Builders<BsonDocument>.Update
                                                       .Set("status", _status)
                                                       .Set("reason", _reason);

            var result = await UpdatePatchQueueCollections.UpdateOneAsync(_patchQueuefilter, _patchQueueUpdate, new UpdateOptions { IsUpsert = true });
            return new MongoError() { Status = result.ModifiedCount > 0, Message = result.ModifiedCount > 0 ? "Success" : "Failed" };


        }
        public async Task<MongoError> UpdateUpdatePatchQueue(EditupdatePatchQueueDTO model)
        {
            try
            {
                // Filter by PatchId and SystemID to match exactly one patch
                var filter = Builders<BsonDocument>.Filter.Eq("patch_file_name", model.UpdateName)
                             & Builders<BsonDocument>.Filter.Eq("system_id", model.ObjID)
                             & Builders<BsonDocument>.Filter.Eq("status", "0");

                // Only update these fields
                var update = Builders<BsonDocument>.Update
                    .Set("status", model.Status)
                    .Set("reason", model.Reason ?? string.Empty)
                    .Set("patch_file_name", model.UpdateName);

                // Do NOT upsert — only update existing record
                var result = await UpdatePatchQueueCollections.UpdateOneAsync(
                    filter,
                    update,
                    new UpdateOptions { IsUpsert = false } // critical to avoid new insert
                );

                bool success = result.ModifiedCount > 0 || result.MatchedCount > 0;

                return new MongoError
                {
                    Status = success,
                    Message = success ? "Patch updated successfully" : "Patch not found"
                };
            }
            catch (Exception ex)
            {
                return new MongoError
                {
                    Status = false,
                    Message = $"Error updating patch: {ex.Message}"
                };
            }
        }
        public async Task CleanupPatchQueue(string systemId)

        {

            var filter = Builders<BsonDocument>.Filter.And(

                Builders<BsonDocument>.Filter.Eq("system_id", systemId),

                Builders<BsonDocument>.Filter.In("status", new[] { "2", "4" })

            );

            await UpdatePatchQueueCollections.DeleteManyAsync(filter);

        }

        // INSERT installer policy (ADMIN ONLY)
        public async Task<bool> InsertAppInstallerAsync(AppInstallerDTO model)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            model.Enabled = true;

            await AppInstallers.InsertOneAsync(model);
            return true;
        }

        // READ installer policy (RUNTIME / API)
        public async Task<string> GetAppInstallerByExeAsync(string exeName)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                 Builders<BsonDocument>.Filter.Regex("patch_path",
                     new BsonRegularExpression($"{exeName}$", "i")),
                 Builders<BsonDocument>.Filter.Eq("is_deleted", false)
             );

            var result = await CentralSoftwarePatchesCollections
                .Find(filter)
                .Project(Builders<BsonDocument>.Projection.Include("installed_parameters").Exclude("_id"))
                .FirstOrDefaultAsync();

            return result?["installed_parameters"]?.AsString;
        }
        // UPDATE AppInstaller entry
        public async Task<bool> UpdateAppInstallerAsync(AppInstallerDTO model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.ExecutableName))
                return false;

            model.UpdatedAt = DateTime.UtcNow;

            var filter = Builders<AppInstallerDTO>.Filter.Eq(x => x.ExecutableName, model.ExecutableName);
            var updateResult = await AppInstallers.ReplaceOneAsync(filter, model);

            return updateResult.IsAcknowledged;
        }
        public async Task<bool> HasPendingPatch(string systemId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("system_id", systemId),
                Builders<BsonDocument>.Filter.Eq("status", "0")
            );

            return await UpdatePatchQueueCollections
                .Find(filter)
                .AnyAsync();
        }

        //added by vijaya
        #endregion

        #region KPI Graph, Missing Patches By Platform, Vendor, Category

        public async Task<PatchStatusSummary> GetPatchQueueSummaryAsync(string orgId)
        {
            var _deviceCount = await GetAllDevicesByOrgId(orgId);
            var filter = Builders<BsonDocument>.Filter.Eq("org_id", orgId) & Builders<BsonDocument>.Filter.Ne("update_kb_number", "app");
            var osPatchCount = await CentralOSPatchesCollections.Find(Builders<BsonDocument>.Filter.Eq("OrgId", orgId)).ToListAsync();
            var allDocs = await UpdatePatchQueueCollections.Find(filter).ToListAsync();
            var result = new WindowsPatchGraphViewModel();
            //Devices Count            
            if (_deviceCount.Any())
            {
                result.StatusSummary.ReportedDevices = _deviceCount.Count();
                result.StatusSummary.ApprovedDevices = _deviceCount.Count(x => x.IsApproved == true);
                result.StatusSummary.UnApprovedDevices = _deviceCount.Count(x => x.IsApproved == false);
            }

            if (allDocs.Count > 0)
            {
                foreach (var doc in allDocs)
                {
                    string status = doc.GetValue("status", "").AsString;
                    string reason = doc.GetValue("reason", "").AsString;
                    string updateKbNumber = doc.GetValue("update_kb_number", "").AsString;

                    bool isWindowsPatch = !string.Equals(updateKbNumber, "app",
                        StringComparison.OrdinalIgnoreCase);

                    // ── Missing (status = "0" OR status = "") ──────────────────────
                    if (status == "0" || status == "")
                    {
                        result.MissingPatches.Add(doc);

                        if (isWindowsPatch)
                            result.WindowsMissingPatches.Add(doc);
                    }
                    // ── Failed (status = "2" or anything other than 0/1) ───────────
                    else if (status != "1")
                    {
                        result.FailedInstalls.Add(doc);

                        if (isWindowsPatch)
                            result.WindowsFailedInstalls.Add(doc);

                        result.StatusSummary.Failed++;
                    }
                    // ── status = "1" → Success or Already Installed ────────────────
                    else
                    {
                        bool isAlreadyInstalled = reason.Contains("already installed", StringComparison.OrdinalIgnoreCase);

                        if (isAlreadyInstalled)
                        {
                            result.AlreadyInstalledPatches.Add(doc);
                            result.StatusSummary.AlreadyInstalled++;
                        }
                        else
                        {
                            result.SuccessfulInstalls.Add(doc);
                            result.StatusSummary.Success++;
                        }
                    }
                }

                // ── Totals ─────────────────────────────────────────────────────
                result.StatusSummary.Missing = result.MissingPatches.Count;
                result.StatusSummary.Total = osPatchCount.Count;
            }
            else
            {
                result.StatusSummary.Total = 0;
                result.StatusSummary.Success = 0;
                result.StatusSummary.Failed = 0;
                result.StatusSummary.Missing = 0;
                result.StatusSummary.AlreadyInstalled = 0;
            }

            return result.StatusSummary;
        }

        public async Task<MissingPatchesSummary> GetMissingPatchesBreakdownAsync(string orgId)

        {

            var result = new MissingPatchesSummary();

            // Step 1: Fetch _id, platform, vendor, category from CentralOSPatches

            var centralFilter = Builders<BsonDocument>.Filter.And(

                Builders<BsonDocument>.Filter.Eq("OrgId", orgId),

                Builders<BsonDocument>.Filter.Eq("is_deleted", false)

            );

            var centralDocs = await CentralOSPatchesCollections

                .Find(centralFilter)

                .Project(Builders<BsonDocument>.Projection

                    .Include("_id")

                    .Include("bit_rate")

                    .Include("os_version")

                    .Include("platform")

                    .Include("vendor")

                    .Include("category"))

                .ToListAsync();

            if (centralDocs.Count == 0)

                return result;

            // Step 2: Build lookup dictionary from central docs

            var centralLookup = centralDocs

                .Where(d => d.Contains("_id") && d["_id"].BsonType != BsonType.Null)

                .GroupBy(d => d["_id"].ToString())

                .ToDictionary(

                    g => g.Key,

                    g =>

                    {

                        var d = g.First();

                        return new

                        {

                            Platform = d.Contains("platform")
        && d["platform"].BsonType != BsonType.Null
        && !string.IsNullOrWhiteSpace(d["platform"].AsString)

                                            ? d["platform"].AsString.Trim() : null,

                            Vendor = d.Contains("vendor")
        && d["vendor"].BsonType != BsonType.Null
        && !string.IsNullOrWhiteSpace(d["vendor"].AsString)

                                            ? d["vendor"].AsString.Trim() : null,

                            Category = d.Contains("category")
        && d["category"].BsonType != BsonType.Null
        && !string.IsNullOrWhiteSpace(d["category"].AsString)

                                            ? d["category"].AsString.Trim() : null,

                            OSVersion = d.Contains("os_version")
        && d["os_version"].BsonType != BsonType.Null
        && !string.IsNullOrWhiteSpace(d["os_version"].AsString)

                                            ? d["os_version"].AsString.Trim() : null,

                            Architecture = d.Contains("bit_rate")
        && d["bit_rate"].BsonType != BsonType.Null
        && !string.IsNullOrWhiteSpace(d["bit_rate"].AsString)

                                            ? d["bit_rate"].AsString.Trim() : null

                        };

                    }

                );

            if (centralLookup.Count == 0)

                return result;

            // Step 3: Fetch ALL patch_ids that EXIST in UpdatePatchQueue for this org

            //         (any status — we just want to know which patch_ids are present)

            var queueFilter = Builders<BsonDocument>.Filter.And(

                Builders<BsonDocument>.Filter.Eq("org_id", orgId),

                Builders<BsonDocument>.Filter.In("patch_id", centralLookup.Keys.ToList())

            );

            var queueDocs = await UpdatePatchQueueCollections

                .Find(queueFilter)

                .Project(Builders<BsonDocument>.Projection

                    .Include("patch_id")

                    .Exclude("_id"))

                .ToListAsync();

            // Step 4: Build a HashSet of patch_ids that ARE matched in the queue

            var matchedPatchIds = queueDocs

                .Select(q => q.GetValue("patch_id", BsonString.Empty).AsString)

                .Where(id => !string.IsNullOrWhiteSpace(id))

                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Step 5: Keep only central records whose _id is NOT in the queue

            var unmatched = centralLookup

                .Where(kv => !matchedPatchIds.Contains(kv.Key))

                .Select(kv => kv.Value)

                .ToList();

            if (unmatched.Count == 0)

                return result;

            // Step 6: Group by Platform

            result.PlatformList = unmatched

                .Where(c => !string.IsNullOrWhiteSpace(c.Platform))

                .GroupBy(c => c.Platform)

                .Select(g => new GroupCount { Name = g.Key, Count = g.Count() })

                .OrderByDescending(x => x.Count)

                .ToList();

            // Step 7: Group by Vendor

            result.VendorList = unmatched

                .Where(c => !string.IsNullOrWhiteSpace(c.Vendor))

                .GroupBy(c => c.Vendor)

                .Select(g => new GroupCount { Name = g.Key, Count = g.Count() })

                .OrderByDescending(x => x.Count)

                .ToList();

            // Step 8: Group by Category

            result.CategoryList = unmatched

                .Where(c => !string.IsNullOrWhiteSpace(c.Category))

                .GroupBy(c => c.Category)

                .Select(g => new GroupCount { Name = g.Key, Count = g.Count() })

                .OrderByDescending(x => x.Count)

                .ToList();

            // Step 9: Group by OSVersion

            result.OSVersionList = unmatched

                .Where(c => !string.IsNullOrWhiteSpace(c.OSVersion))

                .GroupBy(c => c.OSVersion)

                .Select(g => new GroupCount { Name = g.Key, Count = g.Count() })

                .OrderByDescending(x => x.Count)

                .ToList();

            // Step 10: Group by Architecture

            result.ArchitectureList = unmatched

                .Where(c => !string.IsNullOrWhiteSpace(c.Architecture))

                .GroupBy(c => c.Architecture)

                .Select(g => new GroupCount { Name = g.Key, Count = g.Count() })

                .OrderByDescending(x => x.Count)

                .ToList();


            return result;

        }


        //public async Task<MissingPatchesSummary> GetMissingPatchesBreakdownAsync(string orgId)
        //{
        //    var result = new MissingPatchesSummary();

        //    // Step 1: Get all missing patch_ids from UpdatePatchQueue
        //    //         where org_id = orgId AND (status = "0" OR status = "")
        //    //var queueFilter = Builders<BsonDocument>.Filter.And(
        //    //    Builders<BsonDocument>.Filter.Eq("org_id", orgId),
        //    //    Builders<BsonDocument>.Filter.Or(
        //    //        Builders<BsonDocument>.Filter.Eq("status", "0"),
        //    //        Builders<BsonDocument>.Filter.Eq("status", "")
        //    //    )
        //    //);
        //    var queueFilter = Builders<BsonDocument>.Filter.Eq("org_id", orgId);

        //    var missingQueueDocs = await UpdatePatchQueueCollections
        //        .Find(queueFilter)
        //        .Project(Builders<BsonDocument>.Projection.Include("patch_id").Include("status").Include("reason").Exclude("_id"))
        //        .ToListAsync();

        //    if (missingQueueDocs.Count == 0)
        //        return result;

        //    // Step 2: Extract patch_ids and convert to ObjectId for lookup
        //    var patchIds = missingQueueDocs
        //        .Select(d => d.GetValue("patch_id", "").AsString)
        //        .Where(id => !string.IsNullOrEmpty(id) && ObjectId.TryParse(id, out _))
        //        .Select(id => new ObjectId(id))
        //        .Distinct()
        //        .ToList();

        //    if (patchIds.Count == 0)
        //        return result;

        //    // Step 3: Lookup matching records in OSCentralCollection
        //    var centralFilter = Builders<BsonDocument>.Filter.And(
        //        Builders<BsonDocument>.Filter.In("_id", patchIds),
        //        Builders<BsonDocument>.Filter.Eq("OrgId", orgId),
        //        Builders<BsonDocument>.Filter.Eq("is_deleted", false)
        //    );

        //    var centralDocs = await CentralOSPatchesCollections
        //        .Find(centralFilter)
        //        .Project(Builders<BsonDocument>.Projection
        //            .Include("platform")
        //            .Include("vendor")
        //            .Include("category")
        //            .Exclude("_id"))
        //        .ToListAsync();

        //    if (centralDocs.Count == 0)
        //        return result;

        //    // Step 4: Group by Platform, Vendor, Category (skip null/empty values)

        //    result.PlatformList = centralDocs
        //        .Where(d => d.Contains("platform")
        //                 && d["platform"].BsonType != BsonType.Null
        //                 && !string.IsNullOrWhiteSpace(d["platform"].AsString))
        //        .GroupBy(d => d["platform"].AsString.Trim())
        //        .Select(g => new GroupCount { Name = g.Key, Count = g.Count() })
        //        .OrderByDescending(x => x.Count)
        //        .ToList();

        //    result.VendorList = centralDocs
        //        .Where(d => d.Contains("vendor")
        //                 && d["vendor"].BsonType != BsonType.Null
        //                 && !string.IsNullOrWhiteSpace(d["vendor"].AsString))
        //        .GroupBy(d => d["vendor"].AsString.Trim())
        //        .Select(g => new GroupCount { Name = g.Key, Count = g.Count() })
        //        .OrderByDescending(x => x.Count)
        //        .ToList();

        //    result.CategoryList = centralDocs
        //        .Where(d => d.Contains("category")
        //                 && d["category"].BsonType != BsonType.Null
        //                 && !string.IsNullOrWhiteSpace(d["category"].AsString))
        //        .GroupBy(d => d["category"].AsString.Trim())
        //        .Select(g => new GroupCount { Name = g.Key, Count = g.Count() })
        //        .OrderByDescending(x => x.Count)
        //        .ToList();

        //    return result;
        //}

        #endregion

        #region Notification
        public async Task<List<TEST_WebApiOsDetails.Models.Notifications.ITAMEANotificationDTO>> GetNotificationsByOrgId(string _orgId)
        {
            var _notificationFilter = new BsonDocument { { "org_id", _orgId } };
            var _notifications = await NotificationCollections.Find(_notificationFilter).ToListAsync();
            if (_notifications != null)
            {
                var _notifyList = new List<TEST_WebApiOsDetails.Models.Notifications.ITAMEANotificationDTO>();
                foreach (var _n in _notifications)
                {
                    var _notify = new TEST_WebApiOsDetails.Models.Notifications.ITAMEANotificationDTO()
                    {
                        Id = _n["_id"].ToString() ?? string.Empty,
                        User = _n["user"].ToString(),
                        Header = _n["header"].ToString(),
                        Body = _n["body"].ToString(),
                        IsRead = Convert.ToBoolean(_n["is_read"].ToString()),
                        Message = _n["message"].ToString(),
                        OrgID = _n["org_id"].ToString() ?? string.Empty,
                        AssetID = _n["asset_id"].ToString() ?? string.Empty,
                        DevType = _n["dev_type"].ToString() ?? string.Empty,
                        CreatedAt = Convert.ToDateTime(_n["created_at"].ToString())
                    };

                    _notifyList.Add(_notify);
                }

                return _notifyList;
            }

            return new List<TEST_WebApiOsDetails.Models.Notifications.ITAMEANotificationDTO>();
        }

        public async Task<TEST_WebApiOsDetails.Models.Notifications.ITAMEANotificationDTO> GetNotificationById(string _id, string _orgId)
        {
            var _notificationFilter = new BsonDocument { { "_id", ObjectId.Parse(_id) }, { "org_id", _orgId } };
            var _notification = await NotificationCollections.Find(_notificationFilter).FirstOrDefaultAsync();
            if (_notification != null)
            {
                var _notify = new TEST_WebApiOsDetails.Models.Notifications.ITAMEANotificationDTO()
                {
                    Id = _notification["_id"].ToString() ?? string.Empty,
                    User = _notification["user"].ToString(),
                    Header = _notification["header"].ToString(),
                    Body = _notification["body"].ToString(),
                    IsRead = Convert.ToBoolean(_notification["is_read"].ToString()),
                    Message = _notification["message"].ToString(),
                    OrgID = _notification["org_id"].ToString() ?? string.Empty,
                    AssetID = _notification["asset_id"].ToString() ?? string.Empty,
                    DevType = _notification["dev_type"].ToString() ?? string.Empty,
                    CreatedAt = Convert.ToDateTime(_notification["created_at"].ToString())
                };

                return _notify;
            }

            return new TEST_WebApiOsDetails.Models.Notifications.ITAMEANotificationDTO();
        }
        public async Task<MongoError> NotificationStatusUpdateById(string _id, string _orgId)
        {
            var _notifyFilter = new BsonDocument { { "_id", ObjectId.Parse(_id) }, { "org_id", _orgId } };
            var _notifyUpdate = Builders<BsonDocument>.Update.Set("is_read", true);
            await NotificationCollections.UpdateOneAsync(_notifyFilter, _notifyUpdate, new UpdateOptions { IsUpsert = true });
            return new MongoError() { Status = true, Message = "Success" };
        }

        public async Task<MongoError> NotificationStatusUpdate_MarkAllRead_ByOrgId(string _orgId)
        {
            var _notifyFilter = Builders<BsonDocument>.Filter.Eq("org_id", _orgId);
            var _notifyUpdate = Builders<BsonDocument>.Update.Set("is_read", true);
            var result = await NotificationCollections.UpdateManyAsync(_notifyFilter, _notifyUpdate);

            return new MongoError()
            {
                Status = result.ModifiedCount > 0,
                Message = result.ModifiedCount > 0 ?
                    $"Updated {result.ModifiedCount} notifications." :
                    "No notifications found for this organization id."
            };
        }

        public async Task<MongoError> NotificationDeleteById(string _id, string _orgId)
        {
            // Filter all documents with given org_id
            var _notifyFilter = Builders<BsonDocument>.Filter.And(
                                                                    Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(_id)),
                                                                    Builders<BsonDocument>.Filter.Eq("org_id", _orgId)
                                                                );


            // Delete all matching documents
            var result = await NotificationCollections.DeleteManyAsync(_notifyFilter);

            return new MongoError()
            {
                Status = result.DeletedCount > 0,
                Message = result.DeletedCount > 0
                    ? $"Deleted {result.DeletedCount} notifications for organization id: {_orgId}."
                    : "No notifications found for this organization id."
            };
        }

        #endregion

        #region ErrorLogs
        public async Task<List<ErrorLogDto>> GetErrorLogs()
        {
            var errors = await ErrorLogsCollections.Find(_ => true).ToListAsync();
            if (errors != null)
            {
                var _errors = new List<ErrorLogDto>();
                foreach (var error in errors)
                {
                    _errors.Add(new ErrorLogDto()
                    {
                        Id = error["_id"].ToString(),
                        EndPointName = error["end_point_name"].ToString(),
                        Error = error["error"].ToString(),
                        CreatedAt = Convert.ToDateTime(error["created_at"].ToString())
                    });
                }

                return _errors;
            }

            return null;
        }
        public async Task<MongoError> AddErrorLogs(ErrorLogDto model)
        {
            var _errorLogs = new BsonDocument
                    {
                        {"end_point_name",model.EndPointName },
                        {"error",model.Error },
                        {"created_at",DateTime.UtcNow }
                    };

            await ErrorLogsCollections.InsertOneAsync(_errorLogs);
            return new MongoError() { Status = true, Message = "Success" };

        }
        #endregion

        #region Black List Software
        public async Task<MongoError> AddBlackListSoftware(BLsoftwareListDTO model)
        {
            try
            {
                var _blackListSoftware = new BsonDocument
            {
                {"name", model.Name },
                {"description",model.Description },
                {"publisher",model.Publisher },
                {"created_at",DateTime.UtcNow}
            };
                BlackListSoftwareCollections.InsertOneAsync(_blackListSoftware);
                return new MongoError() { Status = true, Message = "Success" };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = "Fail" };
            }
        }

        public async Task<List<BLsoftwareListDTO>> GetBlackListSoftwares()
        {
            var _blslist = await BlackListSoftwareCollections.Find(_ => true).ToListAsync();
            if (_blslist != null)
            {
                var blslist = new List<BLsoftwareListDTO>();
                foreach (var bls in _blslist)
                {
                    blslist.Add(new BLsoftwareListDTO()
                    {
                        Id = bls["_id"].AsString,
                        Name = bls["name"].AsString,
                        Publisher = bls["publisher"].AsString,
                        CreatedDate = Convert.ToDateTime(bls["created_at"].AsString)
                    });
                }

                return blslist;
            }

            return null;
        }

        #endregion

        #region Task Manager | CPU Utilization
        public async Task<List<TaskManagerDTO>> GetTaskManagers(string _objectId, string _orgId)
        {
            var _taskManagerfilter = new BsonDocument { { "asset_unique_id", _objectId }, { "org_id", Convert.ToInt32(_orgId) } };
            var _taskManagerList = await TaskManagerCollections.Find(_taskManagerfilter).ToListAsync();
            if (_taskManagerList != null)
            {
                var _taskManagers = new List<TaskManagerDTO>();
                foreach (var _tm in _taskManagerList)
                {
                    _taskManagers.Add(new TaskManagerDTO()
                    {
                        Id = _tm["_id"].AsString,
                        CPUUsage = _tm["cpu_usage"].AsDouble,
                        MemoryUsage = _tm["memory_usage"].AsDouble,
                        DiskUsage = _tm["disk_usage"].AsDouble,
                        NetworkUsage = _tm["network_usage"].AsDouble,
                        ObjectId = _tm["asset_unique_id"].AsString,
                        SystemName = _tm["system_name"].AsString,
                        OrgId = _tm["org_id"].AsString,
                        CreatedAt = ((DateTime)_tm["created_at"])
                    });
                }

                return _taskManagers;
            }

            return null;
        }

        public async Task<MongoError> AddTaskManager(TaskManagerDTO _taskManager, string systemame, string _objectId, string _orgId)
        {
            try
            {
                BsonDocument taskManager = null;
                if (_taskManager != null)
                {
                    taskManager = new BsonDocument
                    {
                        {"cpu_usage",_taskManager.CPUUsage},
                        {"memory_usage",_taskManager.MemoryUsage },
                        {"disk_usage",_taskManager.DiskUsage },
                        {"network_usage",_taskManager.NetworkUsage },
                        {"asset_unique_id",_objectId },
                        {"system_name",systemame },
                        {"org_id",_orgId },
                        {"created_at",DateTime.UtcNow }
                    };
                }
                else
                {
                    taskManager = new BsonDocument
                    {
                        {"cpu_usage",0},
                        {"memory_usage",0 },
                        {"disk_usage",0 },
                        {"network_usage",0 },
                        {"asset_unique_id",_objectId },
                        {"system_name",systemame },
                        {"org_id",_orgId },
                        {"created_at",DateTime.UtcNow }
                    };
                }

                await TaskManagerCollections.InsertOneAsync(taskManager);
                return new MongoError() { Status = true, Message = "Success" };
            }
            catch
            {
                return new MongoError() { Status = false, Message = "Fail" };
            }
        }

        #endregion

        #region Login Activity |  , , , , asset_unique_id, org_id
        public async Task<MongoError> AddLoginActivity(LoginLogoutActivity model)
        {
            var _loginActivity = new BsonDocument
                    {
                        {"activity_type",model.ActivityType },
                        {"activity_date_time",model.ActivityTime },
                        {"login_user", model.LoginUser },
                        {"system_name",model.SystemName },
                        {"asset_unique_id",model.ObjectId },
                        {"org_id", model.OrgId },
                        {"created_at",DateTime.UtcNow }
                    };

            await LoginActivityStatusCollections.InsertOneAsync(_loginActivity);
            return new MongoError() { Status = true, Message = "Success" };

        }
        #endregion

        #region Patch History
        public async Task<MongoError> AddPatchEntry(TEST_WebApiOsDetails.Models.UpdatePatchQueue patchHistory)
        {
            string _user = string.Empty, orgid = string.Empty, _kbnumberdescription = string.Empty, _patch_os = string.Empty;
            try
            {
                // this is used to fetch systemname and orgid
                var _easpecification = await GetEASpecificationByAssetId(patchHistory.SystemID);
                if (_easpecification != null)
                    _user = _easpecification.SystemName;

                var _fileuploader = await GetFileUploades(_easpecification.OrgID);
                if (_fileuploader.Any() && _fileuploader.Count() > 0)
                {
                    //_kbnumberdescription = _fileuploader?.FirstOrDefault(x => x.KBNumber == patchHistory.UpdateKBNumber && x.UpdateName ==)?.KBNumberDescription ?? "";
                    var _patch_file_uploader_object = _fileuploader?.FirstOrDefault(x => x.KBNumber == patchHistory.UpdateKBNumber && x.UpdateName == patchHistory.UpdateName);
                    if (_patch_file_uploader_object != null)
                    {
                        _patch_os = _patch_file_uploader_object.UpdateOS;
                        _kbnumberdescription = _patch_file_uploader_object?.KBNumberDescription;
                    }
                }


                var _patchHistory = new BsonDocument
                {
                    {"org_id",_easpecification.OrgID },
                    {"user",_easpecification.SystemName},
                    {"system_id",patchHistory.SystemID },
                    {"patch_id",patchHistory.PatchId },
                    {"update_name",patchHistory.UpdateName },
                    {"os",_patch_os },
                    {"kb_number",patchHistory.UpdateKBNumber },
                    {"kb_number_description",_kbnumberdescription},
                    {"status",patchHistory.Status},
                    {"status_remarks",patchHistory.Reason },
                    {"created_at",DateTime.UtcNow}
                };
                await PatchHistoryCollections.InsertOneAsync(_patchHistory);
                return new MongoError() { Status = true, Message = "Success" };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = "Fail" };
            }
        }
        #endregion

        #region CIS Options
        public async Task<MongoError> AddCISOptions(CISDTO model)
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(model.OrgId.ToString()))
                    return new MongoError() { Status = false, Message = "organization id should not be null" };
                if (string.IsNullOrEmpty(model.UserId))
                    return new MongoError() { Status = false, Message = "user id should not be null" };
                if (string.IsNullOrEmpty(model.OsPatchesSelected) && string.IsNullOrEmpty(model.SoftwarePatchesSelected))
                    return new MongoError() { Status = false, Message = "at least select one option" };

                // Check if already exists
                var _cisfilter = new BsonDocument { { "OrgId", Convert.ToInt32(model.OrgId) } };
                var _cisObject = await CISOptionsCollections.Find(_cisfilter).ToListAsync();

                if (_cisObject == null || _cisObject.Count == 0)
                {
                    // Add New Record
                    var _options = new BsonDocument
                    {
                        { "OrgId", Convert.ToInt32(model.OrgId) },
                        { "user_id", model.UserId },
                        { "os_product_family_list_id", model.OsPatchesSelected ?? "" },
                        { "os_product_family_list_name", "" },
                        { "software_family_list_id", model.SoftwarePatchesSelected ?? "" },
                        { "software_family_list_name", "" },
                        { "created_at", DateTime.UtcNow }
                    };
                    await CISOptionsCollections.InsertOneAsync(_options);
                    return new MongoError() { Status = true, Message = "Add CIS Options Successfully..." };
                }
                else
                {
                    // ✅ FIX: Changed from DevicesCollections to CISOptionsCollections
                    var _update = Builders<BsonDocument>.Update
                        .Set("os_product_family_list_id", model.OsPatchesSelected ?? "")
                        .Set("software_family_list_id", model.SoftwarePatchesSelected ?? "")
                        .Set("updated_date", DateTime.UtcNow);

                    await CISOptionsCollections.UpdateOneAsync(_cisfilter, _update); // ✅ Fixed here
                    return new MongoError() { Status = true, Message = "Updated CIS Options Successfully..." };
                }
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }

        public async Task<List<CISDTO>> GetCISOptions()
        {
            try
            {
                var cisDocs = await CISOptionsCollections.Find(_ => true).ToListAsync();

                return cisDocs.Select(doc => new CISDTO
                {
                    OrgId = (doc.GetValue("OrgId", BsonNull.Value).AsInt32).ToString(),
                    UserId = doc.GetValue("user_id", BsonNull.Value).AsString,
                    OsPatchesSelected = doc.GetValue("os_product_family_list_id", BsonNull.Value).AsString,
                    //SoftwarePatchesSelected = doc.GetValue("software_family_list_id", BsonNull.Value).AsString
                }).ToList();
            }
            catch (Exception ex)
            {
                // TODO: Log ex here
                return new List<CISDTO>();
            }
        }

        public async Task<CISDTO> GetCISOptions(string orgId)
        {
            CISDTO _cis = null;
            try
            {
                var _cisfilter = new BsonDocument { { "OrgId", Convert.ToInt32(orgId) } };
                var _cisObject = await CISOptionsCollections.Find(_cisfilter).ToListAsync();
                if (_cisObject != null)
                {
                    _cis = new CISDTO();
                    foreach (var cis in _cisObject)
                    {
                        _cis.OrgId = orgId;
                        _cis.UserId = cis["user_id"].AsString;
                        _cis.OsPatchesSelected = cis["os_product_family_list_id"].AsString;
                        _cis.SoftwarePatchesSelected = cis["software_family_list_id"].AsString;
                    }

                    return _cis;
                }

            }
            catch (Exception ex)
            {
                return _cis;
            }

            return _cis;
        }
        public async Task<List<CentralOSPatches>> GetOSCentralPatches(string orgId)
        {
            List<CentralOSPatches> patchesList = new List<CentralOSPatches>();

            try
            {
                var patchDocuments = await CentralOSPatchesCollections
                    .Find(Builders<BsonDocument>.Filter.Empty)   // ✅ No filters
                    .ToListAsync();

                foreach (var doc in patchDocuments)
                {
                    var patch = new CentralOSPatches
                    {
                        Id = doc.GetValue("_id", null)?.ToString(),
                        OrgId = doc.GetValue("OrgId", null)?.ToString(),
                        UpdateId = doc.GetValue("update_id", null)?.ToString(),
                        UpdateOS = doc.GetValue("update_os", null)?.ToString(),
                        OSVersion = doc.GetValue("os_version", null)?.ToString(),
                        BitRate = doc.GetValue("bit_rate", null)?.ToString(),
                        Title = doc.GetValue("title", null)?.ToString(),
                        Product = doc.GetValue("product", null)?.ToString(),
                        Classification = doc.GetValue("classification", null)?.ToString(),
                        KBNumber = doc.GetValue("kb_number", null)?.ToString(),
                        KBNumberDescription = doc.GetValue("kb_description", null)?.ToString(),
                        ProductFamily = doc.GetValue("product_family", null)?.ToString(),
                        Platform = doc.GetValue("platform", null)?.ToString(),
                        Version = doc.GetValue("version", null)?.ToString(),
                        Size = doc.GetValue("size", null)?.ToString(),
                        BuildNumber = doc.GetValue("build_number", null)?.ToString(),
                        Articles = doc.GetValue("article", null)?.ToString(),
                        PatchPath = doc.GetValue("patch_path", null)?.ToString(),
                        IsDeleted = doc.GetValue("is_deleted", false).ToBoolean()
                    };
                    patchesList.Add(patch);
                }

                return patchesList;
            }
            catch
            {
                return patchesList;
            }
        }
        public async Task<List<CentralOSPatches>> GetOSCentralPatchesbyOS(string orgId, string platform)
        {
            List<CentralOSPatches> patchesList = new List<CentralOSPatches>();
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("OrgId", orgId) & Builders<BsonDocument>.Filter.Eq("update_os", platform);
                var osresult = await CentralOSPatchesCollections.Find(filter).ToListAsync();
                foreach (var doc in osresult)
                {
                    var patch = new CentralOSPatches
                    {
                        Id = doc.GetValue("_id", null)?.ToString(),
                        OrgId = doc.GetValue("OrgId", null)?.ToString(),
                        UpdateId = doc.GetValue("update_id", null)?.ToString(),
                        UpdateOS = doc.GetValue("update_os", null)?.ToString(),
                        OSVersion = doc.GetValue("os_version", null)?.ToString(),
                        BitRate = doc.GetValue("bit_rate", null)?.ToString(),
                        Title = doc.GetValue("title", null)?.ToString(),
                        Product = doc.GetValue("product", null)?.ToString(),
                        Classification = doc.GetValue("classification", null)?.ToString(),
                        KBNumber = doc.GetValue("kb_number", null)?.ToString(),
                        KBNumberDescription = doc.GetValue("kb_description", null)?.ToString(),
                        ProductFamily = doc.GetValue("product_family", null)?.ToString(),
                        Platform = doc.GetValue("platform", null)?.ToString(),
                        Version = doc.GetValue("version", null)?.ToString(),
                        Size = doc.GetValue("size", null)?.ToString(),
                        BuildNumber = doc.GetValue("build_number", null)?.ToString(),
                        Articles = doc.GetValue("article", null)?.ToString(),
                        FileName = doc.GetValue("file_name", null)?.ToString(),
                        PatchPath = doc.GetValue("patch_path", null)?.ToString(),
                        IsDeleted = doc.GetValue("is_deleted", false).ToBoolean()
                    };
                    patchesList.Add(patch);
                }

                return patchesList;
            }
            catch
            {
                return patchesList;
            }

        }
        public async Task<List<CentralSoftwarePatches>> GetSoftwareCentralPatches(string orgId)
        {
            List<CentralSoftwarePatches> patchesList = new List<CentralSoftwarePatches>();

            try
            {
                var patchDocuments = await CentralSoftwarePatchesCollections
                    .Find(Builders<BsonDocument>.Filter.Empty)   // ✅ No filters
                    .ToListAsync();

                foreach (var doc in patchDocuments)
                {
                    var patch = new CentralSoftwarePatches
                    {
                        Id = doc.GetValue("_id", null)?.ToString(),
                        OrgId = doc.GetValue("org_id", null)?.ToString(),
                        UpdateId = doc.GetValue("update_id", null)?.ToString(),
                        SoftwareName = doc.GetValue("software_name", null)?.ToString(),
                        Vendor = doc.GetValue("vendor", null)?.ToString(),
                        PatchNumber = doc.GetValue("patch_number", null)?.ToString(),
                        PatchDescription = doc.GetValue("patch_description", null)?.ToString(),
                        Classification = doc.GetValue("classification", null)?.ToString(),
                        Severity = doc.GetValue("severity", null)?.ToString(),
                        Version = doc.GetValue("version", null)?.ToString(),
                        ProductFamily = doc.GetValue("product_family", null)?.ToString(),
                        Platform = doc.GetValue("platform", null)?.ToString(),
                        BitRate = doc.GetValue("bit_rate", null)?.ToString(),
                        Size = doc.GetValue("size", null)?.ToString(),
                        BuildNumber = doc.GetValue("build_number", null)?.ToString(),
                        Articles = doc.GetValue("article", null)?.ToString(),
                        PatchPath = doc.GetValue("patch_path", null)?.ToString(),
                        IsDeleted = doc.GetValue("is_deleted", false).ToBoolean()
                    };
                    patchesList.Add(patch);
                }

                return patchesList;
            }
            catch
            {
                return patchesList;
            }
        }
        public async Task<HashSet<string>> GetQueuedKbNumbers(string assetId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("system_id", assetId);

            var queueDocs = await UpdatePatchQueueCollections
                .Find(filter)
                .Project(Builders<BsonDocument>.Projection.Include("update_kb_number"))
                .ToListAsync();

            return queueDocs
                .Where(x => x.Contains("update_kb_number") &&
                            !x["update_kb_number"].IsBsonNull)
                .Select(x => x["update_kb_number"].ToString().Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        public async Task<PatchResponseDto> GetOSPatchesAndSoftwares(string orgId)
        {
            PatchResponseDto _result = new PatchResponseDto();
            _result.OSPatches = await GetOSCentralPatches(orgId);
            _result.SoftwarePatches = await GetSoftwareCentralPatches(orgId);
            return _result;
        }

        #endregion

        #region WhiteListSoftware | Desktop Policies

        public async Task<List<DesktopPoliciesDTO>> GetDesktopPolicies(string orgId)
        {
            var policies = new List<DesktopPoliciesDTO>();

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("org_id", Convert.ToInt32(orgId));

                var policyDocs = await WhiteListSoftwareCollections
                    .Find(filter)
                    .ToListAsync();

                if (policyDocs == null || !policyDocs.Any())
                    return policies;

                foreach (var policy in policyDocs)
                {
                    policies.Add(new DesktopPoliciesDTO
                    {
                        SoftwareName = policy.GetValue("software_name", "").AsString,
                        Version = policy.GetValue("version", "").AsString,
                        Size = policy.GetValue("size", "").AsString,
                        Platform = policy.GetValue("platform", "").AsString,
                        BitRate = policy.GetValue("bit_rate", "").AsString,
                        OrgId = policy.GetValue("org_id").AsInt32.ToString()
                    });
                }

                return policies;
            }
            catch (Exception ex)
            {
                // Log ex here if you have a logger
                return policies;
            }
        }

        public async Task<MongoError> AddDesktopPolicies(DesktopPoliciesDTO model)
        {
            try
            {
                // Check if already exists
                var _desktopPoliciesFilter = new BsonDocument { { "org_id", Convert.ToInt32(model.OrgId) }, { "software_name", model.SoftwareName } };
                var _desktopPoliciesObject = await WhiteListSoftwareCollections.Find(_desktopPoliciesFilter).ToListAsync();

                if (_desktopPoliciesObject == null || _desktopPoliciesObject.Count == 0)
                {
                    // Add New Record
                    var _options = new BsonDocument
                    {
                        { "org_id", Convert.ToInt32(model.OrgId) },
                        { "software_name", model.SoftwareName },
                        { "version", model.Version ?? "" },
                        { "size", model.Size ?? "" },
                        { "bit_rate", model.BitRate ?? "" },
                        { "platform",  model.Platform ?? "" },
                        { "created_at", DateTime.UtcNow }
                    };
                    await WhiteListSoftwareCollections.InsertOneAsync(_options);
                    return new MongoError() { Status = true, Message = "Desktop Policy Added Successfully..." };
                }
                //else
                //{
                //    // ✅ FIX: Changed from DevicesCollections to CISOptionsCollections
                //    var _update = Builders<BsonDocument>.Update
                //        .Set("os_product_family_list_id", model.OsPatchesSelected ?? "")
                //        .Set("software_family_list_id", model.SoftwarePatchesSelected ?? "")
                //        .Set("updated_date", DateTime.UtcNow);

                //    await CISOptionsCollections.UpdateOneAsync(_desktopPoliciesFilter, _update); // ✅ Fixed here
                //    return new MongoError() { Status = true, Message = "Updated CIS Options Successfully..." };
                //}

                return new MongoError() { Status = false, Message = "Already Exist..." };

            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }

        #endregion

        #region EndPoint Grouping

        //Get Devices details which are not mapped with any Endpoint groups
        public async Task<List<DeviceNameForEndpointGroup>> GetDevicesNotInAnyGroup(string orgId)
        {
            // Step 1: Collect all device IDs already assigned in any group for this org
            var groupFilter = Builders<BsonDocument>.Filter.Eq("org_id", orgId);
            var groupProjection = Builders<BsonDocument>.Projection
                .Include("devicelists.device_asset_unique_id")
                .Exclude("_id");

            var groupDocs = await EndpointGroupCollections
                .Find(groupFilter)
                .Project(groupProjection)
                .ToListAsync();

            var assignedDeviceIds = new HashSet<string>();
            foreach (var groupDoc in groupDocs)
            {
                if (groupDoc.Contains("devicelists") && groupDoc["devicelists"].IsBsonArray)
                {
                    foreach (var entry in groupDoc["devicelists"].AsBsonArray)
                    {
                        var id = entry.AsBsonDocument.GetValue("device_asset_unique_id", "").AsString;
                        if (!string.IsNullOrEmpty(id))
                            assignedDeviceIds.Add(id);
                    }
                }
            }

            // Step 2: Fetch all devices for this org
            var deviceFilter = Builders<BsonDocument>.Filter.Eq("org_id", Convert.ToInt32(orgId));
            var deviceProjection = Builders<BsonDocument>.Projection
                .Include("asset_unique_id")
                .Include("system_name")
                .Include("login_user")
                .Include("public_ip")
                .Exclude("_id");

            var deviceDocs = await DevicesCollections
                .Find(deviceFilter)
                .Project(deviceProjection)
                .ToListAsync();

            // Step 3: Filter out assigned ones and map
            var result = new List<DeviceNameForEndpointGroup>();
            foreach (var device in deviceDocs)
            {
                var assetId = device.GetValue("asset_unique_id", "").AsString;
                if (assignedDeviceIds.Contains(assetId))
                    continue;

                result.Add(new DeviceNameForEndpointGroup
                {
                    ID=device.GetValue("asset_unique_id","").AsString,
                    SystemName = device.GetValue("system_name", "").AsString,
                    LoginUser = device.GetValue("login_user", "").AsString,
                    PublicIP = device.GetValue("public_ip", "").AsString,
                    GroupName = null
                });
            }
            return result;
        }
        //Get All group name including total no of devices mapped with each groups
        public async Task<List<EndpointGroupVM>> GetGroupsByOrgId(string orgId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("org_id", orgId),
                Builders<BsonDocument>.Filter.Eq("is_active", true)
            );

            var projection = Builders<BsonDocument>.Projection
                .Include("_id")
                .Include("name")
                .Include("devicelists")
                .Include("group_patch_schedule_history");

            var groupDocs = await EndpointGroupCollections
                .Find(filter)
                .Project(projection)
                .ToListAsync();

            var result = new List<EndpointGroupVM>();
            foreach (var doc in groupDocs)
            {
                string _groupname = doc.GetValue("name", "").AsString;
                string groupPatchScheduledTimeId = string.Empty;
                bool isPatchEnabled = false;
                DateTime? scheduleTime = null;
                if (doc.Contains("group_patch_schedule_history") && doc["group_patch_schedule_history"].IsBsonArray)
                {
                    var historyArray = doc["group_patch_schedule_history"].AsBsonArray;

                    var latestRecord = historyArray
                        .Where(x => x.AsBsonDocument.Contains("created_at"))
                        .Select(x => x.AsBsonDocument)
                        .OrderByDescending(x =>
                            x["created_at"].IsBsonNull ? DateTime.MinValue : x["created_at"].ToUniversalTime()
                        )
                        .FirstOrDefault();

                    if (latestRecord != null)
                    {
                        groupPatchScheduledTimeId = latestRecord.GetValue("_id", "").ToString();
                        isPatchEnabled = latestRecord.GetValue("is_group_patch_flag", false).ToBoolean();

                        if (latestRecord.Contains("group_patching_scheduled_time") &&
                            !latestRecord["group_patching_scheduled_time"].IsBsonNull)
                        {
                            scheduleTime = latestRecord["group_patching_scheduled_time"].ToUniversalTime();
                        }
                    }
                }

                result.Add(new EndpointGroupVM
                {
                    Id = doc["_id"].AsObjectId.ToString(),
                    GroupName = !string.IsNullOrEmpty(_groupname) ? char.ToUpper(_groupname[0]) + _groupname.Substring(1) : string.Empty,
                    TotalDeviceCount = doc.Contains("devicelists") && doc["devicelists"].IsBsonArray
                                                                   ? doc["devicelists"].AsBsonArray.Count
                                                                   : 0,
                    GroupPatchScheduledTimeId= groupPatchScheduledTimeId,
                    IsGroupPatchEnabled = isPatchEnabled,
                    ScheduledTime = scheduleTime
                });
            }
            return result;
        }
        //Get all devices which are mapped with particular endpoint group
        public async Task<List<DeviceNameForEndpointGroup>> GetDevicesByGroupId(string groupId,string orgId)
        {
            // Step 1: Fetch the group document by _id
            // Step 1: Fetch the group document by _id AND org_id
            var groupFilter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(groupId)),
                Builders<BsonDocument>.Filter.Eq("org_id", orgId),
                Builders<BsonDocument>.Filter.Eq("is_active", true)
            );
            var groupProjection = Builders<BsonDocument>.Projection
                .Include("name")
                .Include("devicelists.device_asset_unique_id")
                .Exclude("_id");

            var groupDoc = await EndpointGroupCollections
                .Find(groupFilter)
                .Project(groupProjection)
                .FirstOrDefaultAsync();

            if (groupDoc == null)
                return new List<DeviceNameForEndpointGroup>();

            string _groupname = groupDoc.GetValue("name", "").AsString;
            string? groupName = !string.IsNullOrEmpty(_groupname) ? char.ToUpper(_groupname[0]) + _groupname.Substring(1) : string.Empty;

            // Step 2: Extract assigned device IDs from the group
            var assignedIds = new List<string>();
            if (groupDoc.Contains("devicelists") && groupDoc["devicelists"].IsBsonArray)
            {
                foreach (var entry in groupDoc["devicelists"].AsBsonArray)
                {
                    var id = entry.AsBsonDocument.GetValue("device_asset_unique_id", "").AsString;
                    if (!string.IsNullOrEmpty(id))
                        assignedIds.Add(id);
                }
            }

            if (!assignedIds.Any())
                return new List<DeviceNameForEndpointGroup>();

            // Step 3: Fetch matching devices from DevicesCollections
            var deviceFilter = Builders<BsonDocument>.Filter.In("asset_unique_id", assignedIds);
            var deviceProjection = Builders<BsonDocument>.Projection
                .Include("asset_unique_id")
                .Include("system_name")
                .Include("login_user")
                .Include("public_ip")
                .Exclude("_id");

            var deviceDocs = await DevicesCollections
                .Find(deviceFilter)
                .Project(deviceProjection)
                .ToListAsync();

            var result = new List<DeviceNameForEndpointGroup>();
            foreach (var device in deviceDocs)
            {
                result.Add(new DeviceNameForEndpointGroup
                {
                    ID = device.GetValue("asset_unique_id", "").AsString,
                    SystemName = device.GetValue("system_name", "").AsString,
                    LoginUser = device.GetValue("login_user", "").AsString,
                    PublicIP = device.GetValue("public_ip", "").AsString,
                    GroupName = groupName
                });
            }
            return result;
        }
        //Creating New EndPoint Grouping
        public async Task<MongoError> CreateEndpointGroup(EndpointGroup request)
        {
            try
            {
                var deviceList = new BsonArray();
                foreach (var deviceId in request.DeviceIds)
                {
                    deviceList.Add(new BsonDocument
                    {
                        { "device_asset_unique_id", deviceId },
                        { "created_at", DateTime.UtcNow }
                    });
                }

                // Case-insensitive match on group name + org_id
                var existingFilter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Regex("name", new BsonRegularExpression($"^{Regex.Escape(request.GroupName)}$", "i")),
                    Builders<BsonDocument>.Filter.Eq("org_id", request.OrgId),
                    Builders<BsonDocument>.Filter.Eq("is_active", true)
                );

                var existingGroup = await EndpointGroupCollections
                    .Find(existingFilter)
                    .FirstOrDefaultAsync();

                if (existingGroup != null)
                {
                    // Group exists — push new devices into existing devicelists
                    // Also avoid duplicate device IDs using $not $in on existing ones
                    var existingDeviceIds = new HashSet<string>();
                    if (existingGroup.Contains("devicelists") && existingGroup["devicelists"].IsBsonArray)
                    {
                        foreach (var entry in existingGroup["devicelists"].AsBsonArray)
                        {
                            var id = entry.AsBsonDocument.GetValue("device_asset_unique_id", "").AsString;
                            if (!string.IsNullOrEmpty(id))
                                existingDeviceIds.Add(id);
                        }
                    }

                    // Filter out devices already present in the group
                    var newDevicesToAdd = new BsonArray();
                    foreach (var deviceId in request.DeviceIds)
                    {
                        if (!existingDeviceIds.Contains(deviceId))
                        {
                            newDevicesToAdd.Add(new BsonDocument
                            {
                                { "device_asset_unique_id", deviceId },
                                { "created_at", DateTime.UtcNow }
                            });
                        }
                    }

                    if (newDevicesToAdd.Count == 0)
                        return new MongoError() { Status = true, Message = "all devices already exist in endagent group" }; //return true; // Nothing new to add, all devices already exist in group

                    var updateDefinition = Builders<BsonDocument>.Update
                        .PushEach("devicelists", newDevicesToAdd)
                        .Set("modified_date", DateTime.UtcNow);

                    await EndpointGroupCollections.UpdateOneAsync(existingFilter, updateDefinition);
                }
                else
                {
                    var groupPatchScheduledTime = new BsonArray();
                    groupPatchScheduledTime.Add(new BsonDocument
                    {
                        {"_id",ObjectId.GenerateNewId()},
                        {"is_group_patch_flag",false },
                        {"group_patching","disabled" },
                        {"is_scheduled_time_flag",false },
                        {"group_patching_scheduled_time",BsonNull.Value },
                        {"created_at",DateTime.UtcNow },
                        {"updated_at",BsonNull.Value}
                    });
                    // Group does not exist — insert as new document
                    var document = new BsonDocument
                    {
                        { "name", request.GroupName },
                        { "org_id", request.OrgId },
                        { "devicelists", deviceList },
                        { "group_patch_schedule_history",groupPatchScheduledTime },
                        { "is_active", true },
                        { "created_at", DateTime.UtcNow },
                        { "modified_date", BsonNull.Value }
                    };

                    await EndpointGroupCollections.InsertOneAsync(document);
                }

                return new MongoError() { Status = true, Message = "EndAgent Group Created Successfully..." };
            }
            catch (Exception ex)
            {
                // log ex if you have a logger
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }
        public async Task<MongoError> UpdateEndPointGroupScheduledTime(GroupPatchScheduleTimeDTO request)
        {
            MongoError response = new MongoError();

            try
            {
                var parentId = ObjectId.Parse(request.GroupId);

                if (request.IsEnabled)
                {
                    var childId = ObjectId.Parse(request.GroupPatchScheduledTimeId);

                    var filter = Builders<BsonDocument>.Filter.And(
                        Builders<BsonDocument>.Filter.Eq("_id", parentId),
                        Builders<BsonDocument>.Filter.Eq("org_id", request.OrgId),
                        Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                            "group_patch_schedule_history",
                            new BsonDocument("_id", childId)
                        )
                    );

                    var document = await EndpointGroupCollections.Find(filter).FirstOrDefaultAsync();

                    if (document == null)
                    {
                        response.Status = false;
                        response.Message = "Group not found";
                        return response;
                    }

                    var historyArray = document["group_patch_schedule_history"].AsBsonArray;

                    var historyItem = historyArray
                        .FirstOrDefault(x => x["_id"].AsObjectId == childId)
                        ?.AsBsonDocument;

                    if (historyItem == null)
                    {
                        response.Status = false;
                        response.Message = "Schedule history not found";
                        return response;
                    }

                    var createdAt = historyItem.GetValue("created_at", BsonNull.Value);
                    var updatedAt = historyItem.GetValue("updated_at", BsonNull.Value);

                    BsonValue scheduledTime = request.ScheduledTime.HasValue
                        ? new BsonDateTime(request.ScheduledTime.Value)
                        : BsonNull.Value;

                    var updateBuilder = Builders<BsonDocument>.Update
                        .Set("group_patch_schedule_history.$.is_group_patch_flag", true)
                        .Set("group_patch_schedule_history.$.group_patching", "enabled")
                        .Set("group_patch_schedule_history.$.is_scheduled_time_flag",
                                request.ScheduledTime != null)
                        .Set("group_patch_schedule_history.$.group_patching_scheduled_time", scheduledTime);

                    if (createdAt.IsBsonNull && updatedAt.IsBsonNull)
                    {
                        updateBuilder = updateBuilder.Set(
                            "group_patch_schedule_history.$.created_at",
                            DateTime.UtcNow);
                    }
                    else
                    {
                        updateBuilder = updateBuilder.Set(
                            "group_patch_schedule_history.$.updated_at",
                            DateTime.UtcNow);
                    }

                    var result = await EndpointGroupCollections.UpdateOneAsync(filter, updateBuilder);

                    response.Status = result.ModifiedCount > 0;
                    response.Message = result.ModifiedCount > 0
                        ? "Group patch schedule updated successfully"
                        : "No record updated";
                }
                else
                {
                    var filter = Builders<BsonDocument>.Filter.And(
                        Builders<BsonDocument>.Filter.Eq("_id", parentId),
                        Builders<BsonDocument>.Filter.Eq("org_id", request.OrgId)
                    );

                    var newSchedule = new BsonDocument
                    {
                        { "_id", ObjectId.GenerateNewId() },
                        { "is_group_patch_flag", false },
                        { "group_patching", "disabled" },
                        { "is_scheduled_time_flag", false },
                        { "group_patching_scheduled_time", BsonNull.Value },
                        { "created_at", DateTime.UtcNow },
                        { "updated_at", BsonNull.Value }
                    };

                    var update = Builders<BsonDocument>.Update
                        .Push("group_patch_schedule_history", newSchedule);

                    var result = await EndpointGroupCollections.UpdateOneAsync(filter, update);

                    response.Status = result.ModifiedCount > 0;
                    response.Message = result.ModifiedCount > 0
                        ? "New group patch schedule inserted"
                        : "Group not found";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<MongoError> UpdatePatchQueueforEndpointGrouping(string orgId, string groupId, string gpscheduledTimeId)
        {
            try
            {
                DateTime? _scheduledTime = null;

                var parentId = ObjectId.Parse(groupId);
                var childId = ObjectId.Parse(gpscheduledTimeId);
                var epgfilter = Builders<BsonDocument>.Filter.And(
                            Builders<BsonDocument>.Filter.Eq("_id", parentId),
                            Builders<BsonDocument>.Filter.Eq("org_id", orgId),
                            Builders<BsonDocument>.Filter.ElemMatch<BsonValue>("group_patch_schedule_history", new BsonDocument("_id", childId)));
                var epgresult = await EndpointGroupCollections.Find(epgfilter).FirstOrDefaultAsync();
                //Fetch latest enabled end point grouping patch fetch scheduled time.
                var historyArray = epgresult["group_patch_schedule_history"].AsBsonArray;
                var historyItem = historyArray.FirstOrDefault(x => x["_id"].AsObjectId == childId)?.AsBsonDocument;
                //Fetch list of Asset Unique Id
                var epgassetIdArray = epgresult["devicelists"].AsBsonArray;
                var epg_assetId_list = epgassetIdArray.Select(x => x["device_asset_unique_id"].AsString).ToList();
                _scheduledTime = historyItem.AsBsonDocument.GetValue("group_patching_scheduled_time", "").AsBsonDateTime.ToLocalTime();
                foreach(var assetId in epg_assetId_list)
                {
                    string os = string.Empty, platform = string.Empty;
                    var _device = await GetEASpecificationByAssetId(assetId);
                    if (_device != null && _device.OperatingSystem.Contains("Windows 10"))
                        platform = "Windows 10";
                    else
                        platform = "Windows 11";

                    var _ospatchesList = await GetOSCentralPatchesbyOS(orgId, platform);
                    if (_ospatchesList != null && _ospatchesList.Count > 0)
                    {
                        bool isInserted = false;
                        foreach(var ospatch in _ospatchesList)
                        {
                            if (ospatch.OSVersion == _device.OSVersion && ospatch.BitRate == _device.SubnetMask)
                            {

                                TEST_WebApiOsDetails.Models.UpdatePatchQueue value = new TEST_WebApiOsDetails.Models.UpdatePatchQueue()
                                {
                                    PatchId = ospatch.Id,
                                    OrgId = orgId,
                                    SystemID = assetId,
                                    PatchFileName = ospatch.FileName,
                                    PatchFilePath = "D:\\EPT\\OSPatches\\" + ospatch.FileName,
                                    UpdateKBNumber = ospatch.KBNumber,
                                    Status = "0",
                                    Reason = "",
                                    ScheduledTime = _scheduledTime,
                                    CreatedAt = DateTime.UtcNow
                                };

                                var _result = await UpdatePatchQueue(value);
                                isInserted = _result.Status;
                            }
                        }
                    }
                }


                return new MongoError() { Status = true, Message = "Success" };
            }
            catch(Exception ex)
            {
                return new MongoError() { Status = false, Message = $"error occured : {ex}" };
            }

        }

        public async Task<MongoError> UpdateEndpointGroup(UpdateEndpointGroup request)
        {
            try
            {
                // Match by _id + org_id + is_active
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(request.GroupId)),
                    Builders<BsonDocument>.Filter.Eq("org_id", request.OrgId),
                    Builders<BsonDocument>.Filter.Eq("is_active", true)
                );

                var existingGroup = await EndpointGroupCollections
                    .Find(filter)
                    .FirstOrDefaultAsync();

                if (existingGroup == null)
                    return new MongoError() { Status = false, Message = "Endpoint group not found or does not belong to this organization." };

                // Collect already existing device IDs to avoid duplicates
                var existingDeviceIds = new HashSet<string>();
                if (existingGroup.Contains("devicelists") && existingGroup["devicelists"].IsBsonArray)
                {
                    foreach (var entry in existingGroup["devicelists"].AsBsonArray)
                    {
                        var id = entry.AsBsonDocument.GetValue("device_asset_unique_id", "").AsString;
                        if (!string.IsNullOrEmpty(id))
                            existingDeviceIds.Add(id);
                    }
                }

                // Only add devices not already in the group
                var newDevicesToAdd = new BsonArray();
                foreach (var deviceId in request.DeviceIds)
                {
                    if (!existingDeviceIds.Contains(deviceId))
                    {
                        newDevicesToAdd.Add(new BsonDocument
                {
                    { "device_asset_unique_id", deviceId },
                    { "created_at", DateTime.UtcNow }
                });
                    }
                }

                if (newDevicesToAdd.Count == 0)
                    return new MongoError() { Status = true, Message = "All devices already exist in this endpoint group." };

                var updateDefinition = Builders<BsonDocument>.Update
                    .PushEach("devicelists", newDevicesToAdd)
                    .Set("modified_date", DateTime.UtcNow);

                var updateResult = await EndpointGroupCollections.UpdateOneAsync(filter, updateDefinition);

                if (updateResult.ModifiedCount == 0)
                    return new MongoError() { Status = false, Message = "Failed to update endpoint group." };

                return new MongoError() { Status = true, Message = "Endpoint group updated successfully." };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }
        public async Task<MongoError> DeleteDeviceFromEndpointGroup(DeleteDeviceEndPointGroupDTO request)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(request.GroupId)),
                    Builders<BsonDocument>.Filter.Eq("org_id", request.OrgId)
                );

                var existingGroup = await EndpointGroupCollections
                    .Find(filter)
                    .FirstOrDefaultAsync();

                if (existingGroup == null)
                    return new MongoError() { Status = false, Message = "Endpoint group not found or does not belong to this organization." };

                // Pull the specific device from devicelists by device_asset_unique_id
                var updateDefinition = Builders<BsonDocument>.Update
                    .Pull("devicelists", new BsonDocument("device_asset_unique_id", request.DeviceId))
                    .Set("modified_date", DateTime.UtcNow);

                var updateResult = await EndpointGroupCollections.UpdateOneAsync(filter, updateDefinition);

                if (updateResult.ModifiedCount == 0)
                    return new MongoError() { Status = false, Message = "Device not found in this endpoint group." };

                return new MongoError() { Status = true, Message = "Device removed from endpoint group successfully." };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }
        public async Task<MongoError> DeleteEndpointGroup(string groupId, string orgId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(groupId)),
                    Builders<BsonDocument>.Filter.Eq("org_id", orgId)
                );

                var existingGroup = await EndpointGroupCollections
                    .Find(filter)
                    .FirstOrDefaultAsync();

                if (existingGroup == null)
                    return new MongoError() { Status = false, Message = "Endpoint group not found or does not belong to this organization." };

                var deleteResult = await EndpointGroupCollections.DeleteOneAsync(filter);

                if (deleteResult.DeletedCount == 0)
                    return new MongoError() { Status = false, Message = "Failed to delete endpoint group." };

                return new MongoError() { Status = true, Message = "Endpoint group deleted successfully." };
            }
            catch (Exception ex)
            {
                return new MongoError() { Status = false, Message = ex.Message };
            }
        }
        #endregion
    }

    public class MongoError
    {
        public bool Status { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }
}
