using System;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using DameChanceSV2.Models;

namespace DameChanceSV2.DAL
{
    public class PerfilDeUsuarioDAL
    {
        private readonly Database _database;

        public PerfilDeUsuarioDAL(Database database)
        {
            _database = database;
        }

        // Inserta un nuevo perfil y retorna el Id insertado.
        public int InsertPerfil(PerfilDeUsuario perfil)
        {
            int newId = 0;
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = @"INSERT INTO PerfilesDeUsuario (UsuarioId, Edad, Genero, Intereses, Ubicacion, ImagenPerfil)
                                 OUTPUT INSERTED.Id
                                 VALUES (@UsuarioId, @Edad, @Genero, @Intereses, @Ubicacion, @ImagenPerfil)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UsuarioId", perfil.UsuarioId);
                    cmd.Parameters.AddWithValue("@Edad", perfil.Edad);
                    cmd.Parameters.AddWithValue("@Genero", perfil.Genero);
                    cmd.Parameters.AddWithValue("@Intereses", perfil.Intereses ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Ubicacion", perfil.Ubicacion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ImagenPerfil", perfil.ImagenPerfil ?? (object)DBNull.Value);

                    conn.Open();
                    newId = (int)cmd.ExecuteScalar();
                }
            }
            return newId;
        }

        // Retorna el perfil de un usuario, o null si no existe
        public PerfilDeUsuario GetPerfilByUsuarioId(int userId)
        {
            PerfilDeUsuario perfil = null;
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = @"SELECT Id, UsuarioId, Edad, Genero, Intereses, Ubicacion, ImagenPerfil
                         FROM PerfilesDeUsuario
                         WHERE UsuarioId = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            perfil = new PerfilDeUsuario
                            {
                                Id = reader.GetInt32(0),
                                UsuarioId = reader.GetInt32(1),
                                Edad = reader.GetInt32(2),
                                Genero = reader.GetString(3),
                                Intereses = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Ubicacion = reader.IsDBNull(5) ? null : reader.GetString(5),
                                ImagenPerfil = reader.IsDBNull(6) ? null : reader.GetString(6)
                            };
                        }
                    }
                }
            }
            return perfil;
        }

        // Actualiza un perfil existente
        public void UpdatePerfil(PerfilDeUsuario perfil)
        {
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = @"UPDATE PerfilesDeUsuario
                         SET Edad = @Edad,
                             Genero = @Genero,
                             Intereses = @Intereses,
                             Ubicacion = @Ubicacion,
                             ImagenPerfil = @ImagenPerfil
                         WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Edad", perfil.Edad);
                    cmd.Parameters.AddWithValue("@Genero", perfil.Genero);
                    cmd.Parameters.AddWithValue("@Intereses", (object)perfil.Intereses ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Ubicacion", (object)perfil.Ubicacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ImagenPerfil", (object)perfil.ImagenPerfil ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", perfil.Id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        //Extender el DAL para obtener todos los perfiles (menos el mio)

        public List<DashboardProfileViewModel> GetAllOtherProfiles(int currentUserId)
        {
            var list = new List<DashboardProfileViewModel>();
            using var conn = _database.GetConnection();
            const string sql = @"
        SELECT p.UsuarioId, u.Nombre, p.Edad, p.Genero, p.Intereses, p.Ubicacion, p.ImagenPerfil
        FROM PerfilesDeUsuario p
        INNER JOIN Usuarios u ON u.Id = p.UsuarioId
        WHERE p.UsuarioId <> @me AND u.Estado = 1
        AND p.UsuarioId NOT IN (
            SELECT UsuarioBloqueadoId FROM Bloqueos WHERE UsuarioId = @me
        )
        AND p.UsuarioId NOT IN (
            SELECT UsuarioId FROM Bloqueos WHERE UsuarioBloqueadoId = @me
        )"; // sólo verificados
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@me", currentUserId);
            conn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new DashboardProfileViewModel
                {
                    UsuarioId = rd.GetInt32(0),
                    Nombre = rd.GetString(1),
                    Edad = rd.GetInt32(2),
                    Genero = rd.GetString(3),
                    Intereses = rd.IsDBNull(4) ? "" : rd.GetString(4),
                    Ubicacion = rd.IsDBNull(5) ? "" : rd.GetString(5),
                    ImagenPerfil = rd.IsDBNull(6) ? null : rd.GetString(6)
                });
            }
            return list;
        }


    }
}
