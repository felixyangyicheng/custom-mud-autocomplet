using CustomAutoComplet.Data;
using System.Runtime.CompilerServices;

namespace CustomAutoComplet.Repository.Contracts;

public interface IUserRepo
{
    IAsyncEnumerable<User> StreamUsersAsync( string keyword, CancellationToken ct);
}
