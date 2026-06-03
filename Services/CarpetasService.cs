using Dapper;
using SDSP1.Database;
using SDSP1.Models;
using System.Text.RegularExpressions;

namespace SDSP1.Services
{
    public class CarpetasService
    {
        private readonly Conexion _db;
        private const int NombreMaxLength = 255;

        public CarpetasService(Conexion db)
        {
            _db = db;
        }

        // ✅ Sin filtro por usuario — devuelve todas las carpetas
        public async Task<List<Carpetas>> ObtenerCarpetas()
        {
            using var conn = _db.ObtenerConexion();

            var resultado = await conn.QueryAsync<Carpetas>(
                "SELECT id_carpeta, nombre, tipo, f_modificacion FROM carpetas ORDER BY f_modificacion DESC"
            );

            return resultado.ToList();
        }

        public async Task CrearCarpeta(int idUsuario, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la carpeta no puede estar vacío.");

            nombre = nombre.Trim();

            if (nombre.Length > NombreMaxLength)
                throw new ArgumentException($"El nombre no puede exceder {NombreMaxLength} caracteres.");

            var caracteresProhibidos = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            if (nombre.Any(c => caracteresProhibidos.Contains(c)))
                throw new ArgumentException("El nombre contiene caracteres no permitidos.");

            nombre = SanitizarNombre(nombre);

            if (string.IsNullOrEmpty(nombre))
                throw new ArgumentException("El nombre es inválido después de sanitización.");

            using var conn = _db.ObtenerConexion();

            // ✅ Incluye tipo con valor 'general' por defecto
            await conn.ExecuteAsync(
                @"INSERT INTO carpetas (id_usuario, nombre, tipo, f_modificacion) 
                  VALUES (@idUsuario, @nombre, 'general', NOW())",
                new { idUsuario, nombre }
            );
        }

        private string SanitizarNombre(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "";

            nombre = Regex.Replace(nombre, @"[\x00-\x1F\x7F]", "");
            return nombre;
        }

        public async Task EliminarCarpeta(int idCarpeta, int idUsuario)
        {
            using var conn = _db.ObtenerConexion();

            var carpeta = await conn.QueryFirstOrDefaultAsync<Carpetas>(
                "SELECT id_carpeta FROM carpetas WHERE id_carpeta = @idCarpeta AND id_usuario = @idUsuario",
                new { idCarpeta, idUsuario }
            );

            if (carpeta == null)
                throw new ArgumentException("Carpeta no encontrada o no tienes permiso para eliminarla.");

            await conn.ExecuteAsync(
                "DELETE FROM carpetas WHERE id_carpeta = @idCarpeta AND id_usuario = @idUsuario",
                new { idCarpeta, idUsuario }
            );
        }
    }
}