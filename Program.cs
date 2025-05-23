﻿using DameChanceSV2.DAL;
using DameChanceSV2.Services;

namespace DameChanceSV2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton<Database>();
            builder.Services.AddTransient<UsuarioDAL>();
            builder.Services.AddTransient<PerfilDeUsuarioDAL>();

            builder.Services.AddTransient<MatchesDAL>();
            builder.Services.AddTransient<MensajeDAL>();
            builder.Services.AddTransient<ReporteDAL>();
            builder.Services.AddTransient<BloqueoDAL>();
            builder.Services.AddTransient<RolDAL>();

            // Registro del servicio de correo.
            builder.Services.AddTransient<IEmailService, EmailService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. YouK may want to chaSnge this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
