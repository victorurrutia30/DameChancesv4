using System.Collections.Generic;
using System.Data.SqlClient;
using DameChanceSV2.Models;
using Microsoft.Data.SqlClient;

namespace DameChanceSV2.DAL
{
    public class RolDAL
    {
        // ==========================================
        // CONEXIÓN A BASE DE DATOS INYECTADA
        // ==========================================
        private readonly Database _database;

        public RolDAL(Database database)
        {
            _database = database;
        }

        // ==========================================
        // OBTENER TODOS LOS ROLES DE LA BASE DE DATOS
        // Se usa para poblar listas desplegables (select)
        // ==========================================
        public List<Rol> GetRoles()
        {
            List<Rol> roles = new List<Rol>();
            using (SqlConnection conn = _database.GetConnection())
            {
                string query = "SELECT Id, Nombre FROM Roles";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(new Rol
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return roles;
        }

        // ==========================================
        // OBTENER UN ROL POR SU ID
        // Devuelve el objeto Rol si existe, sino null
        // ==========================================
        public Rol? GetRolById(int id)
        {
            using var conn = _database.GetConnection();
            const string sql = "SELECT Id, Nombre FROM Roles WHERE Id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Rol
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1)
                };
            }
            return null;
        }

        // ==========================================
        // INSERTAR NUEVO ROL
        // Se utiliza desde el panel de administración
        // ==========================================
        public void InsertRol(Rol rol)
        {
            using var conn = _database.GetConnection();
            const string sql = "INSERT INTO Roles (Nombre) VALUES (@nombre)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nombre", rol.Nombre);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // ACTUALIZAR NOMBRE DE UN ROL EXISTENTE
        // Se utiliza para edición desde el admin panel
        // ==========================================
        public void UpdateRol(Rol rol)
        {
            using var conn = _database.GetConnection();
            const string sql = "UPDATE Roles SET Nombre = @nombre WHERE Id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nombre", rol.Nombre);
            cmd.Parameters.AddWithValue("@id", rol.Id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // ELIMINAR UN ROL POR SU ID
        // Solo debe ejecutarse si el rol no está en uso
        // ==========================================
        public void DeleteRol(int id)
        {
            using var conn = _database.GetConnection();
            const string sql = "DELETE FROM Roles WHERE Id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // VERIFICAR SI UN ROL ESTÁ EN USO
        // Retorna true si hay usuarios asignados a ese rol
        // Evita que se elimine un rol en uso
        // ==========================================
        public bool EstaRolEnUso(int rolId)
        {
            using var conn = _database.GetConnection();
            const string sql = "SELECT COUNT(*) FROM Usuarios WHERE RolId = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", rolId);
            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }
    }
}
