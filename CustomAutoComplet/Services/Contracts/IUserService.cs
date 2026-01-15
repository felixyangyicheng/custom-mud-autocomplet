using CustomAutoComplet.Data;
using System.Collections.Generic;

namespace CustomAutoComplet.Services.Contracts;

public interface IUserService
{
    IAsyncEnumerable<User> StreamUsersAsync(string keyword, CancellationToken ct);
    IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync(string keyword, CancellationToken ct);
    IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync10(string keyword, CancellationToken ct);
    void InvalidateCache();
}
