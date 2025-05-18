using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using DameChanceSV2.Models;

namespace DameChanceSV2.DAL
{
    public class BloqueoDAL
    {
        private readonly Database _database;
        public BloqueoDAL(Database database) => _database = database;

        public void InsertBloqueo(int usuarioId, int bloqueadoId)
        {
            using var conn = _database.GetConnection();
            const string sql = "INSERT INTO Bloqueos (UsuarioId, UsuarioBloqueadoId) VALUES (@u1, @u2)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u1", usuarioId);
            cmd.Parameters.AddWithValue("@u2", bloqueadoId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public bool ExisteBloqueo(int usuarioId, int bloqueadoId)
        {
            using var conn = _database.GetConnection();
            const string sql = "SELECT COUNT(*) FROM Bloqueos WHERE UsuarioId = @u1 AND UsuarioBloqueadoId = @u2";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u1", usuarioId);
            cmd.Parameters.AddWithValue("@u2", bloqueadoId);
            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        public List<int> GetUsuariosBloqueados(int usuarioId)
        {
            var list = new List<int>();
            using var conn = _database.GetConnection();
            const string sql = "SELECT UsuarioBloqueadoId FROM Bloqueos WHERE UsuarioId = @uid";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", usuarioId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(reader.GetInt32(0));
            return list;
        }
    }
}
