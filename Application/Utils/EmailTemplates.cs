namespace EmpadronamientoBackend.Application.Utils;

public static class EmailTemplates
{
    // Template 1: Confirmación de Correo
    public static string ConfirmacionCorreo(string nombre, string codigo)
    {
        return $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eaeaea; border-radius: 10px; background-color: #ffffff;'>
            <div style='text-align: center; padding-bottom: 20px; border-bottom: 1px solid #eaeaea;'>
                <h2 style='color: #2c3e50; margin: 0;'>Sistema de Empadronamiento</h2>
            </div>
            <div style='padding: 20px 0;'>
                <h3 style='color: #333;'>¡Hola {nombre}!</h3>
                <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                    Gracias por registrarte. Para completar la validación de tu cuenta, por favor ingresa el siguiente código de seguridad en la aplicación:
                </p>
                <div style='text-align: center; margin: 35px 0;'>
                    <span style='font-size: 36px; font-weight: bold; color: #4A90E2; letter-spacing: 8px; padding: 15px 30px; background-color: #f8f9fa; border-radius: 8px; border: 1px dashed #4A90E2;'>
                        {codigo}
                    </span>
                </div>
                <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                    Este código expirará en 15 minutos.
                </p>
            </div>
            <div style='text-align: center; padding-top: 20px; border-top: 1px solid #eaeaea;'>
                <p style='color: #999; font-size: 12px;'>
                    Si no solicitaste este código, puedes ignorar este correo con seguridad.<br>
                    © {DateTime.Now.Year} SPII. Todos los derechos reservados.
                </p>
            </div>
        </div>";
    }

    // Template 2: Recuperación de Contraseña (dejamos la estructura lista)
    public static string RecuperarContrasena(string nombre, string linkRecuperacion)
    {
        return $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eaeaea; border-radius: 10px; background-color: #ffffff;'>
            <div style='text-align: center; padding-bottom: 20px; border-bottom: 1px solid #eaeaea;'>
                <h2 style='color: #2c3e50; margin: 0;'>Recuperación de Acceso</h2>
            </div>
            <div style='padding: 20px 0;'>
                <h3 style='color: #333;'>¡Hola {nombre}!</h3>
                <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                    Hemos recibido una solicitud para restablecer la contraseña de tu cuenta. Haz clic en el siguiente botón para crear una nueva contraseña:
                </p>
                <div style='text-align: center; margin: 35px 0;'>
                    <a href='{linkRecuperacion}' style='background-color: #e74c3c; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;'>
                        Restablecer Contraseña
                    </a>
                </div>
                <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                    Si el botón no funciona, copia y pega este enlace en tu navegador:<br>
                    <a href='{linkRecuperacion}' style='color: #3498db; font-size: 14px;'>{linkRecuperacion}</a>
                </p>
            </div>
        </div>";
    }
}