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

        public async Task<(bool Exitoso, string Mensaje)> Login(LoginViewModel model, string ip)
        {
            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM usuarios WHERE correo = @correo",
                new { model.correo }
            );

            if (usuario == null)
            {
                await _log.Registrar(model.correo, "LOGIN_FALLIDO", "Correo no encontrado", ip);
                return (false, "Usuario o contraseña incorrectos.");
            }

            if (Convert.ToBoolean(usuario.bloqueado))
            {
                DateTime fechaBloqueo = Convert.ToDateTime(usuario.fechaBloqueo);
                DateTime desbloqueo = fechaBloqueo.AddMinutes(MinutosBloqueo);

                if (DateTime.Now < desbloqueo)
                {
                    var minutosRestantes = (int)(desbloqueo - DateTime.Now).TotalMinutes + 1;
                    await _log.Registrar(model.correo, "CUENTA_BLOQUEADA", "Intento de acceso con cuenta bloqueada", ip);
                    return (false, $"Cuenta bloqueada. Intenta de nuevo en {minutosRestantes} minuto(s).");
                }

                await conn.ExecuteAsync(
                    "UPDATE usuarios SET bloqueado = 0, intentosFallidos = 0, fechaBloqueo = NULL WHERE correo = @correo",
                    new { model.correo }
                );
            }

            bool contraseñaValida = BCryptNet.Verify(model.contraseña, (string)usuario.contraseña);

            if (!contraseñaValida)
            {
                int intentos = Convert.ToInt32(usuario.intentosFallidos) + 1;

                if (intentos >= MaxIntentos)
                {
                    await conn.ExecuteAsync(
                        "UPDATE usuarios SET intentosFallidos = @intentos, bloqueado = 1, fechaBloqueo = NOW() WHERE correo = @correo",
                        new { intentos, model.correo }
                    );
                    await _log.Registrar(model.correo, "CUENTA_BLOQUEADA", "Cuenta bloqueada por exceso de intentos", ip);
                    return (false, $"Demasiados intentos fallidos. Cuenta bloqueada por {MinutosBloqueo} minutos.");
                }

                await conn.ExecuteAsync(
                    "UPDATE usuarios SET intentosFallidos = @intentos WHERE correo = @correo",
                    new { intentos, model.correo }
                );

                int intentosRestantes = MaxIntentos - intentos;
                await _log.Registrar(model.correo, "LOGIN_FALLIDO", $"Contraseña incorrecta, intentos restantes: {intentosRestantes}", ip);
                return (false, $"Usuario o contraseña incorrectos.");
            }

            await conn.ExecuteAsync(
                "UPDATE usuarios SET intentosFallidos = 0, bloqueado = 0, fechaBloqueo = NULL WHERE correo = @correo",
                new { model.correo }
            );

            await _log.Registrar(model.correo, "LOGIN_EXITOSO", "Acceso exitoso", ip);
            return (true, "Login exitoso.");
        }
    }
}