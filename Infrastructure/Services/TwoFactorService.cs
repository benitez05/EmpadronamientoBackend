using EmpadronamientoBackend.Application.Interfaces;
using OtpNet;

namespace EmpadronamientoBackend.Infrastructure.Services;

public class TwoFactorService : ITwoFactorService
{
    public string GenerateSecretKey()
    {
        // Genera una llave de 20 bytes
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string email, string secretKey, string issuer)
    {
        // Aseguramos que los caracteres especiales sean válidos para una URL
        var escapedIssuer = Uri.EscapeDataString(issuer);
        var escapedEmail = Uri.EscapeDataString(email);
        
        return $"otpauth://totp/{escapedIssuer}:{escapedEmail}?secret={secretKey}&issuer={escapedIssuer}";
    }

    public bool ValidateCode(string secretKey, string code)
    {
        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(code))
            return false;

        var secretBytes = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(secretBytes);

        // VerificationWindow(2, 2) da una tolerancia de +- 1 minuto por si el reloj del celular está desfasado
        return totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(2, 2));
    }
}