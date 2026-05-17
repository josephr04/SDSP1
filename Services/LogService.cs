using Dapper;
using SDSP1.Database;

namespace SDSP1.Services
{
    public class LogService
    {
        private readonly Conexion _db;

        public LogService(Conexion db)
        {
            _db = db;
        }

        public async Task Registrar(string correo, string evento, string descripcion, string ip)
        {
            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                @"INSERT INTO logs (correo, evento, descripcion, ip, fecha) 
                  VALUES (@correo, @evento, @descripcion, @ip, NOW())",
                new { correo, evento, descripcion, ip }
            );
        }
    }
}