﻿using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using DameChanceSV2.Models;

namespace DameChanceSV2.DAL
{
    public class MatchesDAL
    {
        // ==========================================
        // DEPENDENCIA DE ACCESO A BASE DE DATOS
        // ==========================================
        private readonly Database _database;
        public MatchesDAL(Database database) => _database = database;

        // ==========================================
        // INSERTAR UN LIKE (interés de un usuario hacia otro)
        // INSERTA un registro unilateral en la tabla Matches
        // ==========================================
        public void InsertLike(int usuarioId, int targetId)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                INSERT INTO Matches (Usuario1Id, Usuario2Id)
                VALUES (@u1, @u2)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u1", usuarioId);
            cmd.Parameters.AddWithValue("@u2", targetId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // VERIFICAR SI EL LIKE ES RECÍPROCO
        // Es decir, si el usuario objetivo también dio like
        // ==========================================
        public bool IsReciprocal(int usuarioId, int targetId)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT COUNT(*) 
                FROM Matches 
                WHERE Usuario1Id = @u2 AND Usuario2Id = @u1";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u1", usuarioId);
            cmd.Parameters.AddWithValue("@u2", targetId);
            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        // ==========================================
        // VERIFICAR SI YA EXISTE UN LIKE DE USUARIO A OTRO
        // ==========================================
        public bool ExistsLike(int usuarioId, int targetId)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT COUNT(*) 
                FROM Matches 
                WHERE Usuario1Id = @u1 AND Usuario2Id = @u2";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u1", usuarioId);
            cmd.Parameters.AddWithValue("@u2", targetId);
            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        // ==========================================
        // OBTENER USUARIOS QUE TIENEN LIKE MUTUO CON EL USUARIO ACTUAL
        // EXCLUYE USUARIOS BLOQUEADOS Y QUE LO BLOQUEARON
        // ==========================================
        public List<Usuario> GetMatchesForUser(int usuarioId)
        {
            var list = new List<Usuario>();
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT u.Id, u.Nombre, u.Correo, u.Contrasena, u.Estado, u.RolId, u.FechaRegistro
                FROM Usuarios u
                INNER JOIN Matches m1 
                    ON m1.Usuario2Id = u.Id AND m1.Usuario1Id = @uid
                INNER JOIN Matches m2 
                    ON m2.Usuario1Id = u.Id AND m2.Usuario2Id = @uid
                WHERE u.Id NOT IN (
                    SELECT UsuarioBloqueadoId FROM Bloqueos WHERE UsuarioId = @uid
                )
                AND u.Id NOT IN (
                    SELECT UsuarioId FROM Bloqueos WHERE UsuarioBloqueadoId = @uid
                )";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", usuarioId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Usuario
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Correo = reader.GetString(2),
                    Contrasena = reader.GetString(3),
                    Estado = reader.GetBoolean(4),
                    RolId = reader.GetInt32(5),
                    FechaRegistro = reader.GetDateTime(6)
                });
            }
            return list;
        }

        // ==========================================
        // OBTENER UN OBJETO MATCH COMPLETO POR SU ID
        // ==========================================
        public Match? GetMatchById(int matchId)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT Id, Usuario1Id, Usuario2Id, FechaMatch
                FROM Matches
                WHERE Id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", matchId);
            conn.Open();
            using var rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                return new Match
                {
                    Id = rd.GetInt32(0),
                    Usuario1Id = rd.GetInt32(1),
                    Usuario2Id = rd.GetInt32(2),
                    FechaMatch = rd.GetDateTime(3)
                };
            }
            return null;
        }

        // ==========================================
        // OBTENER EL ID DE UN LIKE (no recíproco necesariamente)
        // Se usa por ejemplo para navegación de mensajes
        // ==========================================
        public int GetMatchId(int usuario1Id, int usuario2Id)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT TOP 1 Id
                FROM Matches
                WHERE Usuario1Id = @u1 AND Usuario2Id = @u2";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u1", usuario1Id);
            cmd.Parameters.AddWithValue("@u2", usuario2Id);
            conn.Open();
            var result = cmd.ExecuteScalar();
            return result != null ? (int)result : 0;
        }

        // ==========================================
        // OBTENER EL ID DEL MATCH CONVERSACIONAL ENTRE DOS USUARIOS
        // No importa quién fue el primero en dar like
        // Se ordena por fecha para tomar el más antiguo
        // ==========================================
        public int GetConversationMatchId(int userA, int userB)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT TOP 1 Id
                FROM Matches
                WHERE (Usuario1Id = @u1 AND Usuario2Id = @u2)
                   OR (Usuario1Id = @u2 AND Usuario2Id = @u1)
                ORDER BY FechaMatch";  // Prioriza el primer match cronológicamente
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u1", userA);
            cmd.Parameters.AddWithValue("@u2", userB);
            conn.Open();
            var result = cmd.ExecuteScalar();
            return result != null ? (int)result : 0;
        }
    }
}
