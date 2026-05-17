using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SDSP1.Database;
using SDSP1.Models;

namespace SDSP1.Controllers
{
    public class CarpetasController : Controller
    {
        public IActionResult Index()
        {
            List<Carpetas> lista =
                new List<Carpetas>();

            Conexion conexion =
                new Conexion();

            using (MySqlConnection conn =
                   conexion.ObtenerConexion())
            {
                try
                {
                    conn.Open();

                    /* BUSCAR ID DEL USUARIO */

                    string sqlUsuario = @"
                SELECT id_usuario
                FROM usuarios
                WHERE nombre = @nombreUsuario";

                    MySqlCommand cmdUsuario =
                        new MySqlCommand(
                            sqlUsuario,
                            conn
                        );

                    cmdUsuario.Parameters.AddWithValue(
                        "@nombreUsuario",
                        "prueba"
                    );

                    object resultado =
                        cmdUsuario.ExecuteScalar();

                    if (resultado != null)
                    {
                        int idUsuario =
                            Convert.ToInt32(resultado);

                        /* BUSCAR CARPETAS DEL USUARIO */

                        string sql = @"
                    SELECT nombre,
                           f_creacion
                    FROM carpetas
                    WHERE id_usuario = @id_usuario";

                        MySqlCommand cmd =
                            new MySqlCommand(
                                sql,
                                conn
                            );

                        cmd.Parameters.AddWithValue(
                            "@id_usuario",
                            idUsuario
                        );

                        MySqlDataReader reader =
                            cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            lista.Add(new Carpetas
                            {
                                nombre =
                                    reader["nombre"]?.ToString(),

                                f_creacion =
                                    reader["f_creacion"]?.ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Content(ex.Message);
                }
            }

            return View("Carpetas", lista);
        }
        [HttpPost]
        public IActionResult CrearCarpeta(string nombre)
        {
            Conexion conexion = new Conexion();

            using (MySqlConnection conn =
                   conexion.ObtenerConexion())
            {
                conn.Open();

                /* BUSCAR ID DEL USUARIO */

                string sqlUsuario = @"
            SELECT id_usuario
            FROM usuarios
            WHERE nombre = @nombreUsuario";

                MySqlCommand cmdUsuario =
                    new MySqlCommand(sqlUsuario, conn);

                cmdUsuario.Parameters.AddWithValue(
                    "@nombreUsuario",
                    "prueba"
                );

                object resultado =
                    cmdUsuario.ExecuteScalar();

                if (resultado != null)
                {
                    int idUsuario =
                        Convert.ToInt32(resultado);

                    /* INSERTAR CARPETA */

                    string sql = @"
                INSERT INTO carpetas
                (
                    id_usuario,
                    nombre,
                    f_creacion
                )
                VALUES
                (
                    @id_usuario,
                    @nombre,
                    NOW()
                )";

                    MySqlCommand cmd =
                        new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue(
                        "@id_usuario",
                        idUsuario
                    );

                    cmd.Parameters.AddWithValue(
                        "@nombre",
                        nombre
                    );

                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
    }
}
