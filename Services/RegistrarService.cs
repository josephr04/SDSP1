using Dapper;
using BCrypt.Net;
using SDSP1.Models;
using SDSP1.Database;

namespace SDSP1.Services
{
    public class RegistrarService
    {
        private readonly Conexion _db;

        public RegistrarService(Conexion db)
        {
            _db = db;
        }

        public async Task<(bool Exitoso, string Mensaje, int IdUsuario)> Registrar(RegistrarViewModel model)
        {
            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            // Verificar usuario duplicado
            var existeUsuario = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM usuarios WHERE nombre = @nombre",
                new { model.nombre }
            );
            if (existeUsuario > 0)
                return (false, "El nombre de usuario ya está en uso.", 0);

            // Verificar correo duplicado
            var existeCorreo = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM usuarios WHERE correo = @correo",
                new { model.correo }
            );
            if (existeCorreo > 0)
                return (false, "El correo ya está registrado.", 0);

            // Hashear contraseña y guardar
            var hash = BCrypt.Net.BCrypt.HashPassword(model.contraseña);
            var idUsuario = await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO usuarios (nombre, correo, contraseña, intentosFallidos, bloqueado, fechaRegistro) 
                VALUES (@nombre, @correo, @contraseña, 0, 0, NOW());
                SELECT LAST_INSERT_ID();",
                new { model.nombre, model.correo, contraseña = hash }
            );

            return (true, "Usuario registrado exitosamente.", idUsuario);
        }
    }
}