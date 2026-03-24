namespace EmpadronamientoBackend.Application.Interfaces;

public interface ITwoFactorService
{
    // Genera la llave secreta en Base32 (Ej: JBSWY3DPEHPK3PXP)
    string GenerateSecretKey();
    
    // Genera la URL para el código QR / Deep Link
    string GenerateQrCodeUri(string email, string secretKey, string issuer);
    
    // Valida si el código de 6 dígitos que metió el usuario es correcto
    bool ValidateCode(string secretKey, string code);
}