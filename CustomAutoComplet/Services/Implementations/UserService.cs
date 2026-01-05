using CustomAutoComplet.Data;
using CustomAutoComplet.Hubs;
using CustomAutoComplet.Repository.Contracts;
using CustomAutoComplet.Services.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace CustomAutoComplet.Services.Implementations;

public class UserService:IUserService
{
    private readonly IUserRepo _repo;
    private readonly ILogger<UserService> _logger;
    private readonly IHubContext<UserHub> _hubContext;
    private readonly SqlTableDependency<User> _dependency;
    private readonly string _connectionString;
    public UserService(IUserRepo repo, IHubContext<UserHub> hubContext, IConfiguration configuration, ILogger<UserService> logger)
    {
            _repo   = repo;
        _logger = logger;
        _hubContext= hubContext;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _dependency = new SqlTableDependency<User>(_connectionString,"User");
        _dependency.OnChanged += OnTableChanged;
        _dependency.OnError += OnTableError;
        _dependency.Start();
    }
    public IAsyncEnumerable<User> StreamUsersAsync( string keyword, CancellationToken ct)
    {
        return _repo.StreamUsersAsync(keyword, ct);
    }
    public IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync(string keyword, CancellationToken ct)
    {
        return _repo.StreamUsersWithScoreAsync(keyword, ct);
    }

    private void OnTableChanged(object sender, RecordChangedEventArgs<User> e)
    {
        if (e.ChangeType != ChangeType.None)
        {
            _ = RefreshClientsAsync(); // Fire-and-forget
        }
    }
    private void OnTableError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
    {
        _logger.LogError(e.Error, "Erreur SqlTableDependency : {Message}", e.Message);
    }
    private async Task RefreshClientsAsync()
    {
        try
        {
            var users = await _repo.GetUsersAsync();
            await _hubContext.Clients.All.SendAsync("RefreshUsers", users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec de la mise à jour en temps réel");
        }
    }
}
