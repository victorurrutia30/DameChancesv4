﻿using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using DameChanceSV2.Models;

namespace DameChanceSV2.DAL
{
    public class ReporteDAL
    {
        // ==========================================
        // CONEXIÓN A LA BASE DE DATOS (INYECTADA)
        // ==========================================
        private readonly Database _database;
        public ReporteDAL(Database database) => _database = database;

        // ==========================================
        // INSERTAR NUEVO REPORTE EN LA BASE DE DATOS
        // ==========================================
        public void InsertReporte(Reporte reporte)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                INSERT INTO Reportes (IdReportante, IdReportado, Motivo)
                VALUES (@reportante, @reportado, @motivo)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@reportante", reporte.IdReportante);
            cmd.Parameters.AddWithValue("@reportado", reporte.IdReportado);
            cmd.Parameters.AddWithValue("@motivo", reporte.Motivo);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // OBTENER TODOS LOS REPORTES (ADMINISTRADOR)
        // ORDENADOS POR FECHA DESCENDENTE
        // ==========================================
        public List<Reporte> GetAllReportes()
        {
            var list = new List<Reporte>();
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT Id, IdReportante, IdReportado, Motivo, FechaReporte, Resuelto, MensajeResolucion
                FROM Reportes
                ORDER BY FechaReporte DESC";
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Reporte
                {
                    Id = reader.GetInt32(0),
                    IdReportante = reader.GetInt32(1),
                    IdReportado = reader.GetInt32(2),
                    Motivo = reader.GetString(3),
                    FechaReporte = reader.GetDateTime(4),
                    Resuelto = reader.GetBoolean(5),
                    MensajeResolucion = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
            return list;
        }

        // ==========================================
        // MARCAR REPORTE COMO RESUELTO (SIN MENSAJE)
        // ==========================================
        public void UpdateResuelto(int id, bool resuelto)
        {
            using var conn = _database.GetConnection();
            const string sql = "UPDATE Reportes SET Resuelto = @resuelto WHERE Id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@resuelto", resuelto);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // MARCAR REPORTE COMO RESUELTO CON MENSAJE
        // Guarda mensaje adicional para justificación o detalle
        // ==========================================
        public void MarcarComoResueltoConMensaje(int id, string mensaje)
        {
            using var conn = _database.GetConnection();
            const string sql = @"
                UPDATE Reportes
                SET Resuelto = 1,
                    MensajeResolucion = @mensaje
                WHERE Id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mensaje", mensaje);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ==========================================
        // OBTENER TOTAL DE REPORTES NO RESUELTOS
        // Se usa en panel administrativo
        // ==========================================
        public int GetTotalReportes()
        {
            using var conn = _database.GetConnection();
            const string sql = "SELECT COUNT(*) FROM Reportes WHERE Resuelto = 0";
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        // ==========================================
        // OBTENER REPORTES HECHOS POR UN USUARIO
        // Usado para vista MisReportes
        // ==========================================
        public List<Reporte> GetReportesPorUsuario(int usuarioId)
        {
            var list = new List<Reporte>();
            using var conn = _database.GetConnection();
            const string sql = @"
                SELECT Id, IdReportante, IdReportado, Motivo, FechaReporte, Resuelto, MensajeResolucion
                FROM Reportes
                WHERE IdReportante = @uid
                ORDER BY FechaReporte DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", usuarioId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Reporte
                {
                    Id = reader.GetInt32(0),
                    IdReportante = reader.GetInt32(1),
                    IdReportado = reader.GetInt32(2),
                    Motivo = reader.GetString(3),
                    FechaReporte = reader.GetDateTime(4),
                    Resuelto = reader.GetBoolean(5),
                    MensajeResolucion = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
            return list;
        }
    }
}
