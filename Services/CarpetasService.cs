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
            // Validación en el servicio (defensa en profundidad)
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la carpeta no puede estar vacío.");

            nombre = nombre.Trim();

            if (nombre.Length > NombreMaxLength)
                throw new ArgumentException($"El nombre de la carpeta no puede exceder {NombreMaxLength} caracteres.");

            // Validar caracteres prohibidos
            var caracteresProhibidos = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            if (nombre.Any(c => caracteresProhibidos.Contains(c)))
                throw new ArgumentException("El nombre contiene caracteres no permitidos.");

            // Sanitizar
            nombre = SanitizarNombre(nombre);

            if (string.IsNullOrEmpty(nombre))
                throw new ArgumentException("El nombre de la carpeta es inválido después de sanitización.");

            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                "INSERT INTO carpetas (id_usuario, nombre, f_modificacion) VALUES (@idUsuario, @nombre, NOW())",
                new { idUsuario, nombre }
            );
        }

        /// <summary>
        /// Sanitiza el nombre para evitar ataques XSS
        /// </summary>
        private string SanitizarNombre(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "";

            // Remover caracteres de control y caracteres especiales peligrosos
            nombre = Regex.Replace(nombre, @"[\x00-\x1F\x7F]", "");

            return nombre;
        }
    }
}
