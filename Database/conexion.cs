using Npgsql;

namespace SDSP1.Database
{
    public class Conexion
    {
        // Cambiar a Session Pooler (puerto 5432) o Direct Connection
        // Session Pooler es compatible con Npgsql sin fricciones
        private static readonly string connectionString =
            "Host=aws-1-us-east-2.pooler.supabase.com;" +
            "Port=5432;" +                        // Session Pooler, compatible con Npgsql
            "Database=postgres;" +
            "Username=postgres.qlccxqyxjjrkilsisiao;" +
            "Password=seguridad_software;" +
            "SSL Mode=Require;" +
            "Trust Server Certificate=true;" +
            "Pooling=true;" +
            "Minimum Pool Size=1;" +              // Mantiene una conexión viva (evita cold starts)
            "Maximum Pool Size=10;" +
            "Connection Idle Lifetime=300;" +     // Reutiliza conexiones por 5 min
            "Timeout=15;" +                       // Más agresivo, falla rápido si algo está mal
            "CommandTimeout=30;" +
            "No Reset On Close=true;";            // Evita round-trip de reset con PgBouncer

        // Pool estático compartido por toda la app
        private static readonly NpgsqlDataSource _dataSource =
            NpgsqlDataSource.Create(connectionString);

        public NpgsqlConnection ObtenerConexion()
        {
            return _dataSource.CreateConnection();
        }
    }
}