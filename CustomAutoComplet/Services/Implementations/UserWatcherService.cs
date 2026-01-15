// CustomAutoComplet.Services.Implementations/UserWatcherService.cs
using CustomAutoComplet.Data;
using CustomAutoComplet.Hubs;
using CustomAutoComplet.Repository.Contracts;
using CustomAutoComplet.Services.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace CustomAutoComplet.Services.Implementations;

public class UserWatcherService : IHostedService, IDisposable
{
    private readonly IHubContext<UserHub> _hubContext;
    private readonly ILogger<UserWatcherService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private SqlTableDependency<User> _dependency;
    private readonly string _connectionString;
    private bool _disposed;

    public UserWatcherService(
        IHubContext<UserHub> hubContext,
        IConfiguration configuration,
        ILogger<UserWatcherService> logger,
        IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _dependency = new SqlTableDependency<User>(
                _connectionString,
                "User" );

            _dependency.OnChanged += OnTableChanged;
            _dependency.OnError += OnTableError;

            _dependency.Start();

            _logger.LogInformation("UserWatcherService started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start UserWatcherService");
            throw;
        }

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _dependency?.Stop();
        _logger.LogInformation("UserWatcherService stopped");
        await Task.CompletedTask;
    }

    private void OnTableChanged(object sender, RecordChangedEventArgs<User> e)
    {
        if (e.ChangeType != ChangeType.None)
        {
            _ = HandleTableChangeAsync(); // Fire-and-forget
        }
    }

    // Dans UserWatcherService.cs, modifier la méthode HandleTableChangeAsync
    private async Task HandleTableChangeAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            // Invalider le cache
            userService.InvalidateCache();

            var users = await repo.GetUsersAsync();
            await _hubContext.Clients.All.SendAsync("RefreshUsers", users);

            _logger.LogDebug("Cache invalidated and users refreshed via SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle table change");
        }
    }

    private void OnTableError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
    {
        _logger.LogError(e.Error, "SqlTableDependency Error: {Message}", e.Message);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dependency?.Dispose();
            }
            _disposed = true;
        }
    }
}