using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioUsuarios
    {
        Task Actualizar(Usuario usuario);
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }

    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            //QuerySingle -> ejecuta la consulta esperando que devuelva una sola fila de tipo <T> en este caso es int
            //SCOPE_IDENTITY ->función de SQL Server que devuelve el último valor Identity generado
            //usuario -> objeto que se pasa para reemplazar los parametros, este objeto es del modelo

            using var connection = new SqlConnection(connectionString);
            var usuarioId = await connection.QuerySingleAsync<int>(@"
                        INSERT INTO Usuarios (Email,EmailNormalizado,PasswordHash)
                        VALUES (@Email,@EmailNormalizado,@PasswordHash);
                        SELECT SCOPE_IDENTITY();",usuario);


            await connection.ExecuteAsync("CrearDatosUsuarioNuevo", new { usuarioId },
                commandType: System.Data.CommandType.StoredProcedure);
            
            return usuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            //QuerySingleOrDefaultAsync
            //Devuelve un solo registro si existe, sino devuelve por defecto null
            //Lanza excepción si hay más de una fila

            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(@"SELECT * FROM Usuarios WHERE EmailNormalizado=@emailNormalizado", new { emailNormalizado });

        }

        public async Task Actualizar(Usuario usuario)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Usuarios SET PasswordHash = @PasswordHash WHERE Id = @Id ", usuario);
        }

    }
}
