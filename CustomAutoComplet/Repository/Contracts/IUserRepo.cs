using CustomAutoComplet.Data;
using System.Runtime.CompilerServices;

namespace CustomAutoComplet.Repository.Contracts;

public interface IUserRepo
{
    IAsyncEnumerable<User> StreamUsersAsync( string keyword, CancellationToken ct);
    IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync( string keyword, CancellationToken ct);
    Task<List<User>> GetUsersAsync();
}
