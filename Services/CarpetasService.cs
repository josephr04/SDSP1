using Dapper;
using SDSP1.Database;
using SDSP1.Models;

namespace SDSP1.Services
{
    public class CarpetasService
    {
        private readonly Conexion _db;

        public CarpetasService(Conexion db)
        {
            _db = db;
        }

        public async Task<List<Carpetas>> ObtenerCarpetas(int idUsuario)
        {
            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            var resultado = await conn.QueryAsync<Carpetas>(
                "SELECT nombre, f_modificacion AS f_creacion FROM carpetas WHERE id_usuario = @idUsuario",
                new { idUsuario }
            );

            return resultado.ToList();
        }

        public async Task CrearCarpeta(int idUsuario, string nombre)
        {
            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                "INSERT INTO carpetas (id_usuario, nombre, f_modificacion) VALUES (@idUsuario, @nombre, NOW())",
                new { idUsuario, nombre }
            );
        }
    }
}