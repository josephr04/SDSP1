using Npgsql;

namespace SDSP1.Database
{
    public class Conexion
    {
        private string connectionString =
            "Host=aws-1-us-east-2.pooler.supabase.com;" +
            "Port=6543;" +
            "Database=postgres;" +
            "Username=postgres.qlccxqyxjjrkilsisiao;" +
            "Password=seguridad_software;" +
            "SSL Mode=Require;" +
            "Trust Server Certificate=true;" +
            "Pooling=true;";

        public NpgsqlConnection ObtenerConexion()
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}