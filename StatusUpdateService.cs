using TEST_WebApiOsDetails.data;

namespace TEST_WebApiOsDetails
{
    public class StatusUpdateService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        public StatusUpdateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(UpdateStatus, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
            return Task.CompletedTask;
        }

        private void UpdateStatus(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var systemNames = dbContext.EASpecifications.Select(e => e.SystemName).ToList();

                foreach (var systemName in systemNames)
                {
                    var existingModel = dbContext.EASpecifications.FirstOrDefault(e => e.SystemName == systemName);
                    var existingStatus = dbContext.Statuses.FirstOrDefault(e => e.SystemName == systemName);

                    if (existingModel != null)
                    {
                        DateTime currentTime = DateTime.Now;
                        TimeSpan timeDifference = currentTime - existingModel.CreatedAt;

                        if (timeDifference.TotalMinutes > 2)
                        {
                            existingModel.SystemStatus = "System is inactive";
                        }
                        else
                        {
                            existingModel.SystemStatus = "System is active";
                        }

                        if (existingStatus != null)
                        {

                            if (timeDifference.TotalMinutes > 2)
                            {
                                existingStatus.SystemStatus = "offline";
                            }
                            else
                            {
                                existingStatus.SystemStatus = "online";
                            }
                        }
                    }
                }

                dbContext.SaveChanges();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

}
