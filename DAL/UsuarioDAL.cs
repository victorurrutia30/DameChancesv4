﻿using System.Data.SqlClient;
using DameChanceSV2.Models;
using Microsoft.Data.SqlClient;

namespace DameChanceSV2.DAL
{
    public class UsuarioDAL
    {
        // ==========================================
        // CONEXIÓN A BASE DE DATOS INYECTADA
        // ==========================================
        private readonly Database _database;

        public UsuarioDAL(Database database)
        {
            _database = database;
        }

        // ==========================================
        // INSERTAR NUEVO USUARIO
        // Devuelve el ID del nuevo usuario insertado
        // ==========================================
        public int InsertUsuario(Usuario usuario)
        {
            int newId = 0;
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = @"
                    INSERT INTO Usuarios (Nombre, Correo, Contrasena, Estado, RolId)
                    OUTPUT INSERTED.Id
                    VALUES (@Nombre, @Correo, @Contrasena, @Estado, @RolId)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                    cmd.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);
                    cmd.Parameters.AddWithValue("@Estado", usuario.Estado);
                    cmd.Parameters.AddWithValue("@RolId", usuario.RolId);

                    conn.Open();
                    newId = (int)cmd.ExecuteScalar();
                }
            }
            return newId;
        }

        // ==========================================
        // OBTENER USUARIO POR CORREO
        // Usado para login y validaciones de registro
        // ==========================================
        public Usuario GetUsuarioByCorreo(string correo)
        {
            Usuario usuario = null;
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = @"SELECT Id, Nombre, Correo, Contrasena, Estado, RolId, FechaRegistro
                                 FROM Usuarios
                                 WHERE Correo = @Correo";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Correo", correo);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
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
                    }
                }
            }
            return usuario;
        }

        // ==========================================
        // OBTENER TODOS LOS USUARIOS (ADMIN DASHBOARD)
        // ==========================================
        public List<Usuario> GetAllUsuarios()
        {
            List<Usuario> usuarios = new List<Usuario>();
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = @"SELECT Id, Nombre, Correo, Contrasena, Estado, RolId, FechaRegistro
                                 FROM Usuarios";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usuarios.Add(new Usuario
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
                    }
                }
            }
            return usuarios;
        }

        // ==========================================
        // OBTENER USUARIO POR ID
        // ==========================================
        public Usuario GetUsuarioById(int id)
        {
            Usuario usuario = null;
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = @"SELECT Id, Nombre, Correo, Contrasena, Estado, RolId, FechaRegistro
                                 FROM Usuarios
                                 WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
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
                    }
                }
            }
            return usuario;
        }

        // ==========================================
        // ACTUALIZAR ESTADO DE VERIFICACIÓN
        // Usado tras confirmar correo
        // ==========================================
        public void UpdateEstado(int usuarioId, bool estado)
        {
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = "UPDATE Usuarios SET Estado = @Estado WHERE Id = @UsuarioId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Estado", estado);
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ==========================================
        // ACTUALIZAR CONTRASEnA (RESET)
        // ==========================================
        public void UpdateContrasena(int userId, string nuevaContrasenaHash)
        {
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = "UPDATE Usuarios SET Contrasena = @Contrasena WHERE Id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Contrasena", nuevaContrasenaHash);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ==========================================
        // ACTUALIZAR DATOS GENERALES DEL USUARIO (ADMIN)
        // Incluye: nombre, correo, rol y estado
        // ==========================================
        public void UpdateUsuario(Usuario user)
        {
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = @"UPDATE Usuarios
                                 SET Nombre = @Nombre,
                                     Correo = @Correo,
                                     RolId = @RolId,
                                     Estado = @Estado
                                 WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", user.Nombre);
                    cmd.Parameters.AddWithValue("@Correo", user.Correo);
                    cmd.Parameters.AddWithValue("@RolId", user.RolId);
                    cmd.Parameters.AddWithValue("@Estado", user.Estado);
                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ==========================================
        // ELIMINAR USUARIO POR SU ID
        // ==========================================
        public void DeleteUsuario(int userId)
        {
            using var conn = _database.GetConnection();
            const string sql = "DELETE FROM Usuarios WHERE Id = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", userId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // CONTADORES GENERALES PARA DASHBOARD ADMIN
        // Total, verificados, no verificados y admins
        // ==========================================
        public (int total, int verificados, int noVerificados, int admins) GetUserCounts()
        {
            int total = 0;
            int verificados = 0;
            int noVerificados = 0;
            int admins = 0;

            using (SqlConnection conn = _database.GetConnection())
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios", conn))
                    total = (int)cmd.ExecuteScalar();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE Estado = 1", conn))
                    verificados = (int)cmd.ExecuteScalar();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE Estado = 0", conn))
                    noVerificados = (int)cmd.ExecuteScalar();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE RolId = 1", conn))
                    admins = (int)cmd.ExecuteScalar();
            }

            return (total, verificados, noVerificados, admins);
        }

        // ==========================================
        // USUARIOS NO VERIFICADOS DESDE HACE MÁS DE 3 DÍAS
        // ==========================================
        public int GetUnverifiedCountOlderThan3Days()
        {
            int count = 0;
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM Usuarios
                    WHERE Estado = 0
                      AND DATEDIFF(DAY, FechaRegistro, GETDATE()) > 3";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    count = (int)cmd.ExecuteScalar();
                }
            }
            return count;
        }

        // ==========================================
        // USUARIOS REGISTRADOS EN EL DÍA ACTUAL
        // Se usa para métrica en Dashboard Admin
        // ==========================================
        public int GetUsuariosRegistradosHoy()
        {
            using var conn = _database.GetConnection();
            const string sql = "SELECT COUNT(*) FROM Usuarios WHERE CAST(FechaRegistro AS DATE) = CAST(GETDATE() AS DATE)";
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }
}
