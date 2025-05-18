using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DameChanceSV2.DAL
{
    // ==========================================
    // CLASE DE ACCESO A BASE DE DATOS
    // Encapsulamos la lógica para obtener una conexión SQL
    // usando la cadena de conexión del archivo appsettings.json
    // ==========================================
    public class Database
    {
        // Campo privado que almacena la cadena de conexión
        private readonly string _connectionString;

        // ==========================================
        // CONSTRUCTOR
        // Recibe una instancia de IConfiguration (inyectada)
        // y extrae la cadena de conexión llamada "DefaultConnection"
        // ==========================================
        public Database(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ==========================================
        // MÉTODO PARA OBTENER UNA NUEVA CONEXIÓN SQL
        // Devuelve un objeto SqlConnection con la cadena ya configurada
        // ==========================================
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
