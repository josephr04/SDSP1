using Dapper;
using BCryptNet = BCrypt.Net.BCrypt;
using SDSP1.Models;
using SDSP1.Database;

namespace SDSP1.Services
{
    public class LoginService
    {
        private readonly Conexion _db;
        private readonly LogService _log;
        private const int MaxIntentos = 3;
        private const int MinutosBloqueo = 5;

        public LoginService(Conexion db, LogService log)
        {
            _db = db;
            _log = log;
        }

        public async Task<(bool Exitoso, string Mensaje, int IdUsuario, string Nombre, string Correo, bool TwoFactorEnabled, string TwoFactorSecret)> Login(LoginViewModel model, string ip)
        {
            try
            {
                using var conn = _db.ObtenerConexion();

                var usuario = await conn.QueryFirstOrDefaultAsync<UsuarioDTO>(
                    @"SELECT 
                        id_usuario,
                        nombre,
                        correo,
                        contraseña,
                        intentosFallidos,
                        bloqueado,
                        fechaRegistro,
                        fechaBloqueo,
                        two_factor_secret,
                        two_factor_enabled,
                        two_factor_verified_at
                      FROM usuarios 
                      WHERE correo = @correo",
                    new { correo = model.correo },
                    commandTimeout: 30
                );

                if (usuario == null)
                {
                    await _log.Registrar(model.correo, "LOGIN_FALLIDO", "Correo no encontrado", ip);
                    return (false, "Usuario o contraseña incorrectos.", 0, "", "", false, "");
                }

                bool recienDesbloqueado = false;

                if (usuario.bloqueado == 1)
                {
                    DateTime desbloqueo = usuario.fechaBloqueo?.AddMinutes(MinutosBloqueo) ?? DateTime.Now;

                    if (DateTime.Now < desbloqueo)
                    {
                        var minutosRestantes = (int)(desbloqueo - DateTime.Now).TotalMinutes + 1;
                        await _log.Registrar(model.correo, "CUENTA_BLOQUEADA", "Intento de acceso con cuenta bloqueada", ip);
                        return (false, $"Cuenta bloqueada. Intenta de nuevo en {minutosRestantes} minuto(s).", 0, "", "", false, "");
                    }

                    await conn.ExecuteAsync(
                        "UPDATE usuarios SET bloqueado = 0, intentosFallidos = 0, fechaBloqueo = NULL WHERE correo = @correo",
                        new { correo = model.correo },
                        commandTimeout: 30
                    );
                    recienDesbloqueado = true;
                }

                bool contraseñaValida = BCryptNet.Verify(model.contraseña, usuario.contraseña);

                if (!contraseñaValida)
                {
                    int intentos = recienDesbloqueado ? 1 : usuario.intentosFallidos + 1;

                    if (intentos >= MaxIntentos)
                    {
                        await conn.ExecuteAsync(
                            "UPDATE usuarios SET intentosFallidos = @intentos, bloqueado = 1, fechaBloqueo = NOW() WHERE correo = @correo",
                            new { intentos, correo = model.correo },
                            commandTimeout: 30
                        );
                        await _log.Registrar(model.correo, "CUENTA_BLOQUEADA", "Cuenta bloqueada por exceso de intentos", ip);
                        return (false, $"Demasiados intentos fallidos. Cuenta bloqueada por {MinutosBloqueo} minutos.", 0, "", "", false, "");
                    }

                    await conn.ExecuteAsync(
                        "UPDATE usuarios SET intentosFallidos = @intentos WHERE correo = @correo",
                        new { intentos, correo = model.correo },
                        commandTimeout: 30
                    );

                    int intentosRestantes = MaxIntentos - intentos;
                    await _log.Registrar(model.correo, "LOGIN_FALLIDO", $"Contraseña incorrecta, intentos restantes: {intentosRestantes}", ip);
                    return (false, "Usuario o contraseña incorrectos.", 0, "", "", false, "");
                }

                await conn.ExecuteAsync(
                    "UPDATE usuarios SET intentosFallidos = 0, bloqueado = 0, fechaBloqueo = NULL WHERE correo = @correo",
                    new { correo = model.correo },
                    commandTimeout: 30
                );

                await _log.Registrar(model.correo, "LOGIN_EXITOSO", "Acceso exitoso", ip);

                return (
                    true,
                    "Login exitoso.",
                    usuario.id_usuario,
                    usuario.nombre,
                    usuario.correo,
                    usuario.two_factor_enabled,
                    usuario.two_factor_secret ?? ""
                );
            }
            catch (TimeoutException)
            {
                return (false, "Timeout: La conexión tardó demasiado. Intenta nuevamente.", 0, "", "", false, "");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0, "", "", false, "");
            }
        }
    }

    public class UsuarioDTO
    {
        public int id_usuario { get; set; }
        public string nombre { get; set; }
        public string correo { get; set; }
        public string contraseña { get; set; }
        public int intentosFallidos { get; set; }
        public int bloqueado { get; set; }
        public DateTime fechaRegistro { get; set; }
        public DateTime? fechaBloqueo { get; set; }
        public string two_factor_secret { get; set; }
        public bool two_factor_enabled { get; set; }
        public DateTime? two_factor_verified_at { get; set; }
    }
}