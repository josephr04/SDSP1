using MySql.Data.MySqlClient;

namespace Proyecto1_Seguridad.Database
{
    public class Conexion
    {
        private string connectionString =
            "server=localhost;database=seguridad_software;user=root;password=;";

        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(connectionString);
        }
    }
}