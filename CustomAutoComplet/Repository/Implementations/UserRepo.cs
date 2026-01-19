using CustomAutoComplet.Data;
using CustomAutoComplet.Repository.Contracts;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;
using static MudBlazor.CategoryTypes;
using static MudBlazor.Icons;

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

    //SELECT name AS DatabaseName, is_broker_enabled FROM sys.databases; => verifie si Broker activé
    //ALTER DATABASE DatabaseName SET ENABLE_BROKER; =>  activer Broker
    public async IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync(
    string keyword,
    [EnumeratorCancellation] CancellationToken ct)
    {
        await using var conn =
            _connectionFactory.CreateConnection() as SqlConnection;

        await conn?.OpenAsync(ct);

        await using var reader = await conn.ExecuteReaderAsync(
            new CommandDefinition(
                """
            Select
                   Id,
                   FirstName,
                   LastName,
                   Email,
                   Guid,
                CASE
                    WHEN FirstName COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 'FirstName'
                    WHEN LastName  COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 'LastName'
                    WHEN Email     COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 'Email'
                    WHEN Guid  LIKE @kw + '%' THEN 'Guid'
                END AS MatchType,
                CASE
                    WHEN FirstName COLLATE Latin1_General_CI_AI = @kw THEN 100
                    WHEN LastName  COLLATE Latin1_General_CI_AI = @kw THEN 95
                    WHEN FirstName COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 90
                    WHEN LastName  COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 85
                    WHEN Email     COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 80
                    WHEN Guid  LIKE @kw + '%' THEN 70
                    ELSE 0
                END AS Score
            FROM dbo.[User]
            WHERE
                  FirstName COLLATE Latin1_General_CI_AI LIKE @kw + '%'
               OR LastName  COLLATE Latin1_General_CI_AI LIKE @kw + '%'
               OR Email     COLLATE Latin1_General_CI_AI LIKE @kw + '%'
               OR Guid  LIKE @kw + '%'
            ORDER BY
                CASE
                    WHEN FirstName LIKE @kw + '%' THEN 1
                    WHEN LastName  LIKE @kw + '%' THEN 2
                    WHEN Email     LIKE @kw + '%' THEN 3
                    WHEN Guid  LIKE @kw + '%' THEN 4
                END;
            """,
                new { kw = keyword },
                cancellationToken: ct));   //ORDER BY Score DESC, FirstName

        var parser = reader.GetRowParser<UserResultWithScore>();

        while (await reader.ReadAsync(ct))
        {
            yield return parser(reader);
        }
    }


    public async Task<List<User>> GetUsersAsync()
    {
        const string query = "SELECT TOP(50) * FROM  dbo.[User];";

        await using var conn =
           _connectionFactory.CreateConnection() as SqlConnection;

        await conn.OpenAsync();
        {
            var customers = await conn.QueryAsync<User>(query);
            return customers.AsList();
        }
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

    public async IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync10(string keyword, CancellationToken ct)
    {
        await using var conn =
            _connectionFactory.CreateConnection() as SqlConnection;

        await conn.OpenAsync(ct);

        await using var reader = await conn.ExecuteReaderAsync(
            new CommandDefinition(
                """
            Select
                  TOP(10)
                   Id,
                   FirstName,
                   LastName,
                   Email,
                   Guid,
                CASE
                    WHEN FirstName COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 'FirstName'
                    WHEN LastName  COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 'LastName'
                    WHEN Email     COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 'Email'
                    WHEN Guid  LIKE @kw + '%' THEN 'Guid'
                END AS MatchType,
                CASE
                    WHEN FirstName COLLATE Latin1_General_CI_AI = @kw THEN 100
                    WHEN LastName  COLLATE Latin1_General_CI_AI = @kw THEN 95
                    WHEN FirstName COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 90
                    WHEN LastName  COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 85
                    WHEN Email     COLLATE Latin1_General_CI_AI LIKE @kw + '%' THEN 80
                    WHEN Guid  LIKE @kw + '%' THEN 70
                    ELSE 0
                END AS Score
            FROM dbo.[User]
            WHERE
                  FirstName COLLATE Latin1_General_CI_AI LIKE @kw + '%'
               OR LastName  COLLATE Latin1_General_CI_AI LIKE @kw + '%'
               OR Email     COLLATE Latin1_General_CI_AI LIKE @kw + '%'
               OR Guid  LIKE @kw + '%'
            ORDER BY
                CASE
                    WHEN FirstName LIKE @kw + '%' THEN 1
                    WHEN LastName  LIKE @kw + '%' THEN 2
                    WHEN Email     LIKE @kw + '%' THEN 3
                    WHEN Guid  LIKE @kw + '%' THEN 4
                END;
            """,
                new { kw = keyword },
                cancellationToken: ct));   //ORDER BY Score DESC, FirstName

        var parser = reader.GetRowParser<UserResultWithScore>();

        while (await reader.ReadAsync(ct))
        {
            yield return parser(reader);
        }
    }
}
