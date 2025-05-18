using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace DameChanceSV2.Utilities
{
    // ============================================================================
    // PasswordHelper (UTILIDADES DE SEGURIDAD)
    // Utiliza PBKDF2 (Password-Based Key Derivation Function 2) para hashear
    // contrasenias de forma segura y verificar si coinciden contra su hash guardado.
    // ============================================================================

    public class PasswordHelper
    {
        // ==========================================================
        // MÉTODO: HashPassword
        // ENTRADA: contrasenias en texto plano
        // SALIDA: string en formato "salt:hash", ambos en Base64
        // FUNCIONALIDAD:
        // 1. Genera una sal aleatoria segura de 16 bytes.
        // 2. Usa PBKDF2 con HMACSHA256 y 10,000 iteraciones para derivar un hash.
        // 3. Devuelve ambos (sal y hash) codificados en Base64 y unidos por ":".
        // ==========================================================
        public static string HashPassword(string password)
        {
            // Generar una sal aleatoria de 128 bits (16 bytes)
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt); // Llenar la sal con bytes aleatorios
            }

            // Aplicar PBKDF2 para obtener un hash seguro
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256, // Algoritmo de derivación
                iterationCount: 10000,            // Número de iteraciones
                numBytesRequested: 32);           // Longitud del hash resultante

            // Retornar la sal y el hash en formato Base64 separados por ":"
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        // ==========================================================
        // MÉTODO: VerifyPassword
        // ENTRADA: contrasenias ingresada, y el hash guardado en formato "salt:hash"
        // SALIDA: true si coinciden, false si no
        // FUNCIONALIDAD:
        // 1. Separa la sal y el hash almacenado.
        // 2. Aplica PBKDF2 nuevamente a la contrasenias ingresada con la sal.
        // 3. Compara los hashes usando FixedTimeEquals para prevenir ataques de tiempo.
        // ==========================================================
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            // Dividir el string en sal y hash
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            // Decodificar sal y hash desde Base64
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedPasswordHash = Convert.FromBase64String(parts[1]);

            // Recalcular hash de la contrasenias ingresada con la misma sal
            byte[] enteredPasswordHash = KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32);

            // Comparación segura (evita ataques de timing)
            return CryptographicOperations.FixedTimeEquals(enteredPasswordHash, storedPasswordHash);
        }
    }
}
