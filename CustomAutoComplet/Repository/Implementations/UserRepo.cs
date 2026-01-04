using CustomAutoComplet.Data;
using CustomAutoComplet.Repository.Contracts;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace CustomAutoComplet.Repository.Implementations;

public class UserRepo : IUserRepo
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<UserRepo> _logger;

    public UserRepo(ISqlConnectionFactory connectionFactory, ILogger<UserRepo> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async IAsyncEnumerable<User> StreamUsersAsync(
        string keyword,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await using var conn = _connectionFactory.CreateConnection() as SqlConnection;


        await using var reader = await conn.ExecuteReaderAsync(
            new CommandDefinition(
                @"SELECT Id, Name
              FROM Users
              WHERE Name LIKE @kw
              ORDER BY Name",
                new { kw = $"%{keyword}%" },
                cancellationToken: ct));

        var parser = reader.GetRowParser<User>();

        while (await reader.ReadAsync(ct))
        {
            yield return parser(reader);
        }
    }
}
