using Dapper;
using MySqlConnector;

namespace SkyNetApi.Repositorios;

public class RepositorioGeneros : IRepositorioGenero
{
    private readonly string? connectionString;

    public RepositorioGeneros(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public Task<int> CrearGenero()
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            var query = connection.QuerySingle<int>("SELECT 1");
            return Task.FromResult(query);
        }
    }
}