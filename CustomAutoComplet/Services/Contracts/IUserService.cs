using CustomAutoComplet.Data;

namespace CustomAutoComplet.Services.Contracts;

public interface IUserService
{
    IAsyncEnumerable<User> StreamUsersAsync(string keyword, CancellationToken ct);
    IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync(string keyword, CancellationToken ct);
}
