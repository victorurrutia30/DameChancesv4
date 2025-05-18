using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using DameChanceSV2.Models;

namespace DameChanceSV2.DAL
{
    public class BloqueoDAL
    {
        // ==========================================
        // CONSTRUCTOR CON INYECCIÓN DE CONEXIÓN
        // ==========================================
        private readonly Database _database;
        public BloqueoDAL(Database database) => _database = database;

        // ==========================================
        // INSERTAR BLOQUEO EN LA TABLA Bloqueos
        // ==========================================
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

        // ==========================================
        // VERIFICAR SI YA EXISTE UN BLOQUEO ENTRE DOS USUARIOS
        // ==========================================
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

        // ==========================================
        // OBTENER LISTA DE USUARIOS BLOQUEADOS POR UN USUARIO
        // ==========================================
        public List<int> GetUsuariosBloqueados(int usuarioId)
        {
            var list = new List<int>();
            using var conn = _database.GetConnection();
            const string sql = "SELECT UsuarioBloqueadoId FROM Bloqueos WHERE UsuarioId = @uid";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", usuarioId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(reader.GetInt32(0)); // Agrega el ID bloqueado a la lista
            return list;
        }

        // ==========================================
        // OBTENER INFORMACIÓN COMPLETA DE UN USUARIO BLOQUEADO
        // ==========================================
        public Usuario GetUsuarioBloqueadoConInfo(int id)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT Id, Nombre, Correo, Contrasena, Estado, RolId, FechaRegistro
                FROM Usuarios
                WHERE Id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                // Mapeo de datos a objeto Usuario
                return new Usuario
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Correo = reader.GetString(2),
                    Contrasena = reader.GetString(3),
                    Estado = reader.GetBoolean(4),
                    RolId = reader.GetInt32(5),
                    FechaRegistro = reader.GetDateTime(6)
                };
            }
            return null; // Si no encuentra el usuario
        }

        // ==========================================
        // ELIMINAR UN BLOQUEO ENTRE DOS USUARIOS
        // ==========================================
        public void EliminarBloqueo(int usuarioId, int bloqueadoId)
        {
            using var conn = _database.GetConnection();
            const string sql = "DELETE FROM Bloqueos WHERE UsuarioId = @u1 AND UsuarioBloqueadoId = @u2";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u1", usuarioId);
            cmd.Parameters.AddWithValue("@u2", bloqueadoId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
