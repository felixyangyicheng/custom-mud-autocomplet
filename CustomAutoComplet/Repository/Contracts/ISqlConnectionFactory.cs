using System.Data;

namespace CustomAutoComplet.Repository.Contracts;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
