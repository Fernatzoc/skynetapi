using Dapper;
using MySqlConnector;
using SkyNetApi.Entidades;
using System.Data;

namespace SkyNetApi.Repositorios
{
    public class RepositorioClientes : IRepositorioClientes
    {
        private readonly string? connectionString;
        public RepositorioClientes(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Cliente>> ObtenerClientes()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var clientes = await connection.QueryAsync<Cliente>("db_skynet.proObtenerClientes", 
                    commandType: CommandType.StoredProcedure);
                return clientes.ToList();
            }
        }

        public async Task<Cliente?> ObtenerCliente(int idCliente)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var cliente = await connection.QueryFirstOrDefaultAsync<Cliente>(@"SELECT * FROM db_skynet.cliente WHERE idCliente = @idCliente", new { idCliente });
            
                return cliente;
            }
        }

        public async Task<int> crearCliente(Cliente cliente)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var idCliente = await connection.QuerySingleAsync<int>(@"proAccionClientes", new
                {
                    _idCliente = (int?)null, // <- nombre con guión bajo
                    _primerNombre = cliente.PrimerNombre,
                    _segundoNombre = cliente.SegundoNombre,
                    _tercerNombre = cliente.TercerNombre,
                    _primerApellido = cliente.PrimerApellido,
                    _segundoApellido = cliente.SegundoApellido,
                    _telefono = cliente.Telefono,
                    _latitud = cliente.Latitud,
                    _longitud = cliente.Longitud,
                    _direccion = cliente.Direccion
                },
                    commandType: CommandType.StoredProcedure);

                cliente.IdCliente = idCliente;
                return idCliente;
            }

        }

        public async Task<bool> Existe(int id)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                var existe = await conexion.QuerySingleAsync<bool>(@"SELECT COUNT(*) > 0 AS existe FROM db_skynet.cliente WHERE idCliente = @id;", new {id});

                return existe;
            }
        }

        public async Task Actualizar(Cliente cliente)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(@"UPDATE db_skynet.cliente
SET primerNombre = @primerNombre,
    segundoNombre = @segundoNombre,
    tercerNombre = @tercerNombre
WHERE idCliente = @idCliente;", cliente);
            }
        }

        public async Task Eliminar(int idCliente)
        {
            using (var conexion = new MySqlConnection(connectionString))
            {
                await conexion.ExecuteAsync(@"DELETE FROM db_skynet.cliente WHERE idCliente = @idCliente;", new { idCliente });
            }
        }
    }
}
