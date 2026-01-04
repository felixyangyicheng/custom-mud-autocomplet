using CustomAutoComplet.Data;
using CustomAutoComplet.Repository.Contracts;
using CustomAutoComplet.Services.Contracts;

namespace CustomAutoComplet.Services.Implementations;

public class UserService:IUserService
{
    private readonly IUserRepo _repo;
    private readonly ILogger<UserService> _logger;
    public UserService(IUserRepo repo, ILogger<UserService> logger)
    {
            _repo   = repo;
        _logger = logger;
    }
    public IAsyncEnumerable<User> StreamUsersAsync( string keyword, CancellationToken ct)
    {
        return _repo.StreamUsersAsync(keyword, ct);
    }
    public IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync(string keyword, CancellationToken ct)
    {
        return _repo.StreamUsersWithScoreAsync(keyword, ct);
    }
}
