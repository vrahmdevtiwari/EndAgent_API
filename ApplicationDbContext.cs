using Microsoft.EntityFrameworkCore;
using System.Numerics;
using TEST_WebApiOsDetails.Models;
using TEST_WebApiOsDetails.Models.Notifications;

namespace TEST_WebApiOsDetails.data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<EASpecification> EASpecifications { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<InstalledApp> Apps { get; set; }
        public DbSet<Update> Updates { get; set; }
        public DbSet<Port> Ports { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<BLsoftware> BlackListedSoftwares { get; set; }
        public DbSet<AppLog> AppLogs { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<Processor> Processors { get; set; }
        public DbSet<RAMDetail> RAMDetails { get; set; }
        public DbSet<ScheduledTask> ScheduledTasks { get; set; }
        public DbSet<StorageVolume> StorageVolumes { get; set; }
        public DbSet<NetworkAdapter> NetworkAdapters { get; set; }
        public DbSet<RAIDController> RAIDControllers { get; set; }
        public DbSet<GraphicCard> GraphicCards { get; set; }
        public DbSet<PhysicalDrive> PhysicalDrives { get; set; }
        public DbSet<OtherSpecification> OtherSpecifications { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<ActivePort> ActivePorts { get; set; }
        public DbSet<ActiveNetworkDetail> ActiveNetworkDetails { get; set; }
        public DbSet<ResourceUtil> ResourceUtils { get; set; }
        public DbSet<DiskDetail> DiskDetails { get; set; }
        public DbSet<PartitionDetail> PartitionDetails { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<PatchQueue> PatchQueue { get; set; }
        public DbSet<FileUpload> FileUploads { get; set; }
        public DbSet<UpdatePatchQueue> UpdatePatchQueue { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PartitionDetail>().HasOne(p => p.DiskDetail).WithMany(t => t.PartitionDetails).HasForeignKey(p => p.DiskDetailId);
            modelBuilder.Entity<Notification>()
                .Property(n => n.Body)
                .HasColumnType("nvarchar(max)"); // Adjust data type as needed for your database
            modelBuilder.Entity<Device>().HasIndex(u => new { u.ObjectID, u.BIOS_SN }).IsUnique();

            base.OnModelCreating(modelBuilder);


        }

        private string CS;
        public string GetConnection()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: false);

            var configuration = builder.Build();

            CS = configuration.GetConnectionString("DefaultConnection").ToString();
            return CS;
        }
    }


    
}
