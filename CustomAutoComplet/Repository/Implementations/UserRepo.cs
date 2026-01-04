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
        await using var conn =
            _connectionFactory.CreateConnection() as SqlConnection;

        await conn.OpenAsync(ct);

        await using var reader = await conn.ExecuteReaderAsync(
            new CommandDefinition(
                """
            SELECT 
                   Id,
                   FirstName,
                   LastName,
                   Email,
                   Guid
            FROM dbo.[User]
            WHERE
                  FirstName COLLATE Latin1_General_CI_AI LIKE @kw + '%'
               OR LastName  COLLATE Latin1_General_CI_AI LIKE @kw + '%'
               OR Email     COLLATE Latin1_General_CI_AI LIKE @kw + '%'
               OR CONVERT(varchar(36), Guid) LIKE @kw + '%'
            ORDER BY
                CASE
                    WHEN FirstName LIKE @kw + '%' THEN 1
                    WHEN LastName  LIKE @kw + '%' THEN 2
                    WHEN Email     LIKE @kw + '%' THEN 3
                    WHEN CONVERT(varchar(36), Guid) LIKE @kw + '%' THEN 4
                    ELSE 5
                END,
                FirstName;
            """,
                new { kw = keyword },
                cancellationToken: ct));

        var parser = reader.GetRowParser<User>();

        while (await reader.ReadAsync(ct))
        {
            yield return parser(reader);
        }
    }

}
