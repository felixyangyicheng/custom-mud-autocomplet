using CustomAutoComplet.Repository.Contracts;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CustomAutoComplet.Repository.Implementations;

public class SqlConnectionFactory: ISqlConnectionFactory
{
    private readonly string _connectionString;


    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("La chaîne de connexion ne peut pas être vide.", nameof(connectionString));

        _connectionString = connectionString;
  
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
