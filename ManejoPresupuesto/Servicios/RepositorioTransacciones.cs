using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{

    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }

    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);

            //QuerySingle -> ejecuta la consulta esperando que devuelva una sola fila de tipo <T> en este caso es int
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar",
                new {transaccion.UsuarioId,transaccion.FechaTransaccion,transaccion.Monto,
                transaccion.CategoriaId,transaccion.CuentaId,transaccion.Nota},
                commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(
            ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"SELECT t.Id,t.Monto,t.FechaTransaccion ,c.Nombre AS Categoria,
                    cu.Nombre AS Cuenta, c.TipoOperacionId 
                    FROM Transacciones AS t
                    INNER JOIN Categorias AS c
                    ON c.Id = t.CategoriaId
                    INNER JOIN Cuentas AS cu ON cu.Id = t.CuentaId 
                    WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId
                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin",modelo);
        }

        //
        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(
           ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"SELECT t.Id,t.Monto,t.FechaTransaccion ,c.Nombre AS Categoria,
                    cu.Nombre AS Cuenta, c.TipoOperacionId , t.Nota
                    FROM Transacciones AS t
                    INNER JOIN Categorias AS c
                    ON c.Id = t.CategoriaId
                    INNER JOIN Cuentas AS cu ON cu.Id = t.CuentaId 
                    WHERE t.UsuarioId = @UsuarioId
                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                    ORDER BY t.FechaTransaccion DESC ", modelo);
        }


        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior,
            int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                },commandType: System.Data.CommandType.StoredProcedure ); 
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"SELECT t.*,cat.TipoOperacionId  FROM Transacciones AS t
INNER JOIN Categorias AS cat
ON cat.Id= t.CategoriaId 
WHERE t.Id=@Id AND t.UsuarioId =@UsuarioId", 
                new{ id,usuarioId });
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
            SELECT DATEDIFF(d,@fechaInicio,t.FechaTransaccion) / 7 + 1 AS Semana,
                SUM(t.Monto) AS Monto, cat.TipoOperacionId 
                FROM Transacciones AS t
                INNER JOIN Categorias AS cat
                ON cat.Id = t.CategoriaId
                WHERE t.UsuarioId = @usuarioId AND
                t.FechaTransaccion BETWEEN @fechaInicio AND @fechaFin 
                GROUP BY DATEDIFF(d,@fechaInicio,t.FechaTransaccion) / 7 , cat.TipoOperacionId 
            ",modelo);
        
        }


        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"
            SELECT MONTH(FechaTransaccion) AS Mes,
            SUM(Monto) AS Monto, cat.TipoOperacionId 
            FROM Transacciones AS t
            INNER JOIN Categorias AS cat
            ON cat.Id= t.CategoriaId 
            WHERE t.UsuarioId = @usuarioId  AND YEAR(FechaTransaccion) = @Año
            GROUP BY Month(FechaTransaccion),cat.TipoOperacionId ",new {usuarioId,año}); 
        }



        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Transacciones_Borrar",
                new {id}, commandType: System.Data.CommandType.StoredProcedure);


        }

    }
}
