using Dapper;
using MySqlConnector;
using SkyNetApi.Entidades;

namespace SkyNetApi.Repositorios
{
    public class RepositorioVisitas : IRepositorioVisitas
    {
        private readonly string? connectionString;

        public RepositorioVisitas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> Crear(Visita visita)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var idVisita = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO db_skynet.visita 
                      (idCliente, idSupervisor, idTecnico, idEstadoVisita, idTipoVisita, 
                       fechaHoraProgramada, descripcion)
                      VALUES 
                      (@IdCliente, @IdSupervisor, @IdTecnico, @IdEstadoVisita, @IdTipoVisita,
                       @FechaHoraProgramada, @Descripcion);
                      SELECT LAST_INSERT_ID();",
                    new
                    {
                        visita.IdCliente,
                        visita.IdSupervisor,
                        visita.IdTecnico,
                        visita.IdEstadoVisita,
                        visita.IdTipoVisita,
                        visita.FechaHoraProgramada,
                        visita.Descripcion
                    });

                visita.IdVisita = idVisita;
                return idVisita;
            }
        }

        public async Task<Visita?> ObtenerPorId(int idVisita)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var visita = await connection.QueryFirstOrDefaultAsync<Visita>(
                    @"SELECT idVisita AS IdVisita, idCliente AS IdCliente, idSupervisor AS IdSupervisor,
                             idTecnico AS IdTecnico, idEstadoVisita AS IdEstadoVisita, idTipoVisita AS IdTipoVisita,
                             fechaHoraProgramada AS FechaHoraProgramada, descripcion AS Descripcion
                      FROM db_skynet.visita 
                      WHERE idVisita = @idVisita",
                    new { idVisita });

                return visita;
            }
        }

        public async Task<List<Visita>> ObtenerTodas()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var visitas = await connection.QueryAsync<Visita>(
                    @"SELECT idVisita AS IdVisita, idCliente AS IdCliente, idSupervisor AS IdSupervisor,
                             idTecnico AS IdTecnico, idEstadoVisita AS IdEstadoVisita, idTipoVisita AS IdTipoVisita,
                             fechaHoraProgramada AS FechaHoraProgramada, descripcion AS Descripcion
                      FROM db_skynet.visita 
                      ORDER BY fechaHoraProgramada DESC");

                return visitas.ToList();
            }
        }

        public async Task<List<Visita>> ObtenerPorCliente(int idCliente)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var visitas = await connection.QueryAsync<Visita>(
                    @"SELECT idVisita AS IdVisita, idCliente AS IdCliente, idSupervisor AS IdSupervisor,
                             idTecnico AS IdTecnico, idEstadoVisita AS IdEstadoVisita, idTipoVisita AS IdTipoVisita,
                             fechaHoraProgramada AS FechaHoraProgramada, descripcion AS Descripcion
                      FROM db_skynet.visita 
                      WHERE idCliente = @idCliente
                      ORDER BY fechaHoraProgramada DESC",
                    new { idCliente });

                return visitas.ToList();
            }
        }

        public async Task<List<Visita>> ObtenerPorTecnico(string idTecnico)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var visitas = await connection.QueryAsync<Visita>(
                    @"SELECT idVisita AS IdVisita, idCliente AS IdCliente, idSupervisor AS IdSupervisor,
                             idTecnico AS IdTecnico, idEstadoVisita AS IdEstadoVisita, idTipoVisita AS IdTipoVisita,
                             fechaHoraProgramada AS FechaHoraProgramada, descripcion AS Descripcion
                      FROM db_skynet.visita 
                      WHERE idTecnico = @idTecnico
                      ORDER BY fechaHoraProgramada DESC",
                    new { idTecnico });

                return visitas.ToList();
            }
        }

        public async Task<List<Visita>> ObtenerPorSupervisor(string idSupervisor)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var visitas = await connection.QueryAsync<Visita>(
                    @"SELECT idVisita AS IdVisita, idCliente AS IdCliente, idSupervisor AS IdSupervisor,
                             idTecnico AS IdTecnico, idEstadoVisita AS IdEstadoVisita, idTipoVisita AS IdTipoVisita,
                             fechaHoraProgramada AS FechaHoraProgramada, descripcion AS Descripcion
                      FROM db_skynet.visita 
                      WHERE idSupervisor = @idSupervisor
                      ORDER BY fechaHoraProgramada DESC",
                    new { idSupervisor });

                return visitas.ToList();
            }
        }

        public async Task<bool> Existe(int idVisita)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var existe = await connection.QuerySingleAsync<bool>(
                    @"SELECT COUNT(*) > 0 
                      FROM db_skynet.visita 
                      WHERE idVisita = @idVisita",
                    new { idVisita });

                return existe;
            }
        }

        public async Task Actualizar(Visita visita)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE db_skynet.visita
                      SET idCliente = @IdCliente,
                          idSupervisor = @IdSupervisor,
                          idTecnico = @IdTecnico,
                          idEstadoVisita = @IdEstadoVisita,
                          idTipoVisita = @IdTipoVisita,
                          fechaHoraProgramada = @FechaHoraProgramada,
                          descripcion = @Descripcion
                      WHERE idVisita = @IdVisita",
                    new
                    {
                        visita.IdVisita,
                        visita.IdCliente,
                        visita.IdSupervisor,
                        visita.IdTecnico,
                        visita.IdEstadoVisita,
                        visita.IdTipoVisita,
                        visita.FechaHoraProgramada,
                        visita.Descripcion
                    });
            }
        }

        public async Task ActualizarEstado(int idVisita, int idEstadoVisita)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE db_skynet.visita 
                      SET idEstadoVisita = @idEstadoVisita
                      WHERE idVisita = @idVisita",
                    new { idVisita, idEstadoVisita });
            }
        }

        public async Task<int> CrearRegistroVisita(RegistroVisita registroVisita)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var idRegistro = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO db_skynet.registroVisita 
                      (idVisita, fechaHoraInicioReal, fechaHoraFinReal, observaciones)
                      VALUES 
                      (@IdVisita, @FechaHoraInicioReal, @FechaHoraFinReal, @Observaciones);
                      SELECT LAST_INSERT_ID();",
                    new
                    {
                        registroVisita.IdVisita,
                        registroVisita.FechaHoraInicioReal,
                        registroVisita.FechaHoraFinReal,
                        registroVisita.Observaciones
                    });

                return idRegistro;
            }
        }

        public async Task<RegistroVisita?> ObtenerRegistroVisita(int idVisita)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var registro = await connection.QueryFirstOrDefaultAsync<RegistroVisita>(
                    @"SELECT idRegistroVisita AS IdRegistroVisita, idVisita AS IdVisita,
                             fechaHoraInicioReal AS FechaHoraInicioReal, fechaHoraFinReal AS FechaHoraFinReal,
                             observaciones AS Observaciones
                      FROM db_skynet.registroVisita 
                      WHERE idVisita = @idVisita",
                    new { idVisita });

                return registro;
            }
        }

        public async Task Eliminar(int idVisita)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.ExecuteAsync(
                    @"DELETE FROM db_skynet.visita 
                      WHERE idVisita = @idVisita",
                    new { idVisita });
            }
        }
    }
}

