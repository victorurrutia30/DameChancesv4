using DameChanceSV2.Models;
using Microsoft.Data.SqlClient;

namespace DameChanceSV2.DAL
{
    public class MensajeDAL
    {
        // ==========================================
        // DEPENDENCIAS INYECTADAS
        // Database: acceso a conexión SQL Server
        // BloqueoDAL: para verificar restricciones de mensajería
        // ==========================================
        private readonly Database _database;
        private readonly BloqueoDAL _bloqueoDAL;

        public MensajeDAL(Database database, BloqueoDAL bloqueoDAL)
        {
            _database = database;
            _bloqueoDAL = bloqueoDAL;
        }

        // ==========================================
        // OBTENER TODOS LOS MENSAJES DE UN MATCH
        // Devuelve la conversación ordenada por fecha
        // ==========================================
        public List<Mensaje> GetMensajesByMatch(int matchId)
        {
            var lista = new List<Mensaje>();
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT Id, MatchId, EmisorId, ReceptorId, Contenido, FechaEnvio, Leido
                FROM Mensajes
                WHERE MatchId = @m
                ORDER BY FechaEnvio";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@m", matchId);
            conn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Mensaje
                {
                    Id = rd.GetInt32(0),
                    MatchId = rd.GetInt32(1),
                    EmisorId = rd.GetInt32(2),
                    ReceptorId = rd.GetInt32(3),
                    Contenido = rd.GetString(4),
                    FechaEnvio = rd.GetDateTime(5),
                    Leido = rd.GetBoolean(6)
                });
            }
            return lista;
        }

        // ==========================================
        // INSERTAR NUEVO MENSAJE
        // Verifica primero si hay bloqueo mutuo antes de guardar
        // ==========================================
        public void InsertMensaje(Mensaje msg)
        {
            if (_bloqueoDAL.ExisteBloqueo(msg.EmisorId, msg.ReceptorId) ||
                _bloqueoDAL.ExisteBloqueo(msg.ReceptorId, msg.EmisorId))
            {
                throw new InvalidOperationException("No puedes enviar mensajes a un usuario bloqueado.");
            }

            using var conn = _database.GetConnection();
            const string sql = @"
                INSERT INTO Mensajes (MatchId, EmisorId, ReceptorId, Contenido)
                VALUES (@mid, @e, @r, @c)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mid", msg.MatchId);
            cmd.Parameters.AddWithValue("@e", msg.EmisorId);
            cmd.Parameters.AddWithValue("@r", msg.ReceptorId);
            cmd.Parameters.AddWithValue("@c", msg.Contenido);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // MARCAR MENSAJE COMO LEÍDO
        // Se actualiza el campo Leido a 1 (true)
        // ==========================================
        public void MarcarComoLeido(int mensajeId)
        {
            using var conn = _database.GetConnection();
            const string sql = "UPDATE Mensajes SET Leido = 1 WHERE Id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", mensajeId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // CONTAR MENSAJES NO LEÍDOS DE UN MATCH
        // Solo se cuentan si el usuario es el receptor y aún no los ha leído
        // ==========================================
        public int ContarNoLeidos(int usuarioId, int matchId)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Mensajes
                WHERE MatchId = @m
                  AND ReceptorId = @u
                  AND Leido = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@m", matchId);
            cmd.Parameters.AddWithValue("@u", usuarioId);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }
}
