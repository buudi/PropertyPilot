using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PropertyPilot.Services.FinanceServices;

namespace PropertyPilot.Services.HostedServices;

public class InvoiceRenewHostedService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceScopeFactory _scopeFactory;

    public InvoiceRenewHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Start immediately and execute every 1 minute
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var financesService = scope.ServiceProvider.GetRequiredService<FinancesService>();
            financesService.RenewInvoiceScheduledJob();
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
