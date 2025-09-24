using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTiposCuentas //Creamos interfaz, nos permite obligar a las clases que implementen esta interfaz a tener los mismos metodos.
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId,int id = 0);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
    }

    public class RepositorioTiposCuentas : IRepositorioTiposCuentas //esta clase implementa la interfaz, Ojo no es herencia, estamos implementando una interfaz
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration) ////al recibir un parametro de tipo IConfiguration tenemos acceso a (appsettings.json + appsettings.Development.json) , de esta forma tenemos acceso a la cadena de conexion
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Task representa un metodo que no devuelve un valor (asi como void) pero que se ejecuta de forma asincróna
        // Task<> representa un metodo que si devuelve un valor pero que se ejecuta de forma asincróna
        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);

            //QuerySingle -> ejecuta la consulta esperando que devuelva una sola fila de tipo <T> en este caso es int
            //SCOPE_IDENTITY ->función de SQL Server que devuelve el último valor Identity generado
            //tipoCuenta -> objeto que se pasa para reemplazar los parametros, este objeto es del modelo
            var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar", 
                                new
                                {
                                    usuarioId = tipoCuenta.UsuarioId,
                                    nombre = tipoCuenta.Nombre
                                }, commandType: System.Data.CommandType.StoredProcedure); //Ejecutamos Procedimiento almacenado  TiposCuentas_Insertar

            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId,int id=0)
        {
            using var connection = new SqlConnection(connectionString);

            //QueryFirstOrDefaultAsync -> Obtiene lo primero que encuentre o un valor por defecto
            // valor por defecto como es <int> seria 0 en caso no encuentre nada
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM TiposCuentas 
                        WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId AND Id<>@id;", new { nombre, usuarioId,id });

            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId) //IEnumerable representa una colección que se puede recorrer
        {
            //QueryAsync Ejecuta consulta y devuelve Task<IEnumerable<T>> , es decir devuelve una coleccion
           
            using var connection= new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id,Nombre,Orden FROM TiposCuentas 
                        WHERE UsuarioId=@UsuarioId ORDER BY Orden",new {usuarioId});
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);

            //ExecuteAsync -> Ejecuta comando SQL por ejemplo (INSERT,UPDATE,DELETE)
            await connection.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre=@Nombre WHERE Id=@Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            //QueryFirstOrDefaultAsync -> Obtiene lo primero que encuentre o un valor por defecto
            // valor por defecto como es <TipoCuenta> seria NULL en caso no encuentre nada
            // No Lanza excepción si hay más de una fila, simplemete ignora las demas

            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"SELECT Id,Nombre,Orden FROM TiposCuentas WHERE Id=@Id AND UsuarioId=@UsuarioId",new {id,usuarioId});
        
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE FROM TiposCuentas WHERE Id=@Id", new {id});
        }


        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id=@Id;";
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(query, tipoCuentasOrdenados); //Actualizamos por cada valor de tipoCuentasOrdenados, al ser de tipo IEnumerable puede tener varios valores (coleccion)
        }

    }
}
