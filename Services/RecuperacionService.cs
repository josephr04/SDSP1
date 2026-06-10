using Dapper;
using BCrypt.Net;
using SDSP1.Database;
using Npgsql;

namespace SDSP1.Services
{
    public class RecuperacionService
    {
        private readonly Conexion _db;
        private const int CommandTimeout = 60;

        public RecuperacionService(Conexion db)
        {
            _db = db;
        }

        /// <summary>
        /// Genera un código de 6 dígitos, lo guarda en la BD con expiración de 15 minutos
        /// Retorna (éxito, mensaje, codigo)
        /// </summary>
        public async Task<(bool Exitoso, string Mensaje, string Codigo)> GenerarCodigoRecuperacion(string correo)
        {
            try
            {
                using var conn = _db.ObtenerConexion();
                await conn.OpenAsync();

                // Verificar que el usuario existe
                var usuario = await conn.QueryFirstOrDefaultAsync(
                    "SELECT id_usuario FROM usuarios WHERE correo = @correo",
                    new { correo },
                    commandTimeout: CommandTimeout
                );

                if (usuario == null)
                    return (false, "El correo no está registrado.", "");

                // Generar código de 6 dígitos
                var random = new Random();
                var codigo = random.Next(100000, 999999).ToString();

                // Calcular tiempo de expiración (15 minutos)
                var expiracion = DateTime.UtcNow.AddMinutes(15);

                // Actualizar usuario con el código
                await conn.ExecuteAsync(
                    "UPDATE usuarios SET codigo_recuperacion = @codigo, expira_codigo = @expira WHERE correo = @correo",
                    new { codigo, expira = expiracion, correo },
                    commandTimeout: CommandTimeout
                );

                return (true, "Código generado. Revisa tu correo.", codigo);
            }
            catch (NpgsqlException ex)
            {
                return (false, "Error en la base de datos. Intenta más tarde.", "");
            }
            catch (TimeoutException ex)
            {
                return (false, "La solicitud tardó demasiado. Intenta más tarde.", "");
            }
            catch (Exception ex)
            {
                return (false, "Error inesperado. Intenta más tarde.", "");
            }
        }

        /// <summary>
        /// Valida que el código sea válido y no haya expirado
        /// </summary>
        public async Task<(bool Valido, int IdUsuario)> ValidarCodigoRecuperacion(string codigo)
        {
            try
            {
                using var conn = _db.ObtenerConexion();
                await conn.OpenAsync();

                var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT id_usuario, codigo_recuperacion, expira_codigo 
                      FROM usuarios 
                      WHERE codigo_recuperacion = @codigo",
                    new { codigo },
                    commandTimeout: CommandTimeout
                );

                if (usuario == null)
                    return (false, 0);

                // Verificar que no haya expirado
                if (DateTime.UtcNow > usuario.expira_codigo)
                    return (false, 0);

                return (true, usuario.id_usuario);
            }
            catch (NpgsqlException ex)
            {
                return (false, 0);
            }
            catch (TimeoutException ex)
            {
                return (false, 0);
            }
            catch (Exception ex)
            {
                return (false, 0);
            }
        }

        /// <summary>
        /// Cambiar contraseña usando el código de recuperación
        /// </summary>
        public async Task<(bool Exitoso, string Mensaje)> CambiarContraseña(string codigo, string nuevaContraseña)
        {
            try
            {
                // Validar el código
                var (codigoValido, idUsuario) = await ValidarCodigoRecuperacion(codigo);
                if (!codigoValido)
                    return (false, "El código es inválido o ha expirado.");

                // Hashear la nueva contraseña
                var hash = BCrypt.Net.BCrypt.HashPassword(nuevaContraseña);

                using var conn = _db.ObtenerConexion();
                await conn.OpenAsync();

                // Actualizar contraseña y limpiar código
                await conn.ExecuteAsync(
                    @"UPDATE usuarios 
                      SET contraseña = @contraseña, 
                          codigo_recuperacion = NULL, 
                          expira_codigo = NULL 
                      WHERE id_usuario = @id",
                    new { contraseña = hash, id = idUsuario },
                    commandTimeout: CommandTimeout
                );

                return (true, "Contraseña restablecida correctamente.");
            }
            catch (NpgsqlException ex)
            {
                return (false, "Error en la base de datos. Intenta más tarde.");
            }
            catch (TimeoutException ex)
            {
                return (false, "La solicitud tardó demasiado. Intenta más tarde.");
            }
            catch (Exception ex)
            {
                return (false, "Error inesperado. Intenta más tarde.");
            }
        }

        /// <summary>
        /// Obtener el correo de un usuario por su código de recuperación (para mostrar en la vista)
        /// </summary>
        public async Task<string> ObtenerCorreoPorCodigo(string codigo)
        {
            try
            {
                using var conn = _db.ObtenerConexion();
                await conn.OpenAsync();

                var correo = await conn.ExecuteScalarAsync<string>(
                    "SELECT correo FROM usuarios WHERE codigo_recuperacion = @codigo",
                    new { codigo },
                    commandTimeout: CommandTimeout
                );

                return correo ?? "";
            }
            catch (NpgsqlException ex)
            {
                return "";
            }
            catch (TimeoutException ex)
            {
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
