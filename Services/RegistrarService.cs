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

        public async Task<(bool Exitoso, string Mensaje)> Registrar(RegistrarViewModel model)
        {
            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            // Verificar usuario duplicado
            var existeUsuario = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM usuarios WHERE nombre = @nombre",
                new { model.nombre }
            );
            if (existeUsuario > 0)
                return (false, "El nombre de usuario ya está en uso.");

            // Verificar celular duplicado
            var existeCelular = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM usuarios WHERE celular = @celular",
                new { model.celular }
            );
            if (existeCelular > 0)
                return(false, "El número de celular ya está registrado.");

            // Verificar correo duplicado
            var existeCorreo = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM usuarios WHERE correo = @correo",
                new { model.correo }
            );
            if (existeCorreo > 0)
                return (false, "El correo ya está registrado.");

            // Hashear contraseña y guardar
            var hash = BCrypt.Net.BCrypt.HashPassword(model.contraseña);
            await conn.ExecuteAsync(
                @"INSERT INTO usuarios (nombre, celular, correo, contraseña, intentosFallidos, bloqueado, fechaRegistro) 
                VALUES (@nombre, @celular, @correo, @contraseña, 0, 0, NOW())",
                new { model.nombre, model.celular, model.correo, contraseña = hash }
            );

            return (true, "Usuario registrado exitosamente.");
        }
    }
}