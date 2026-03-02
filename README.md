<<<<<<< HEAD
Markdown
# 🚀 Roberto Benitez Enterprise Backend API

Template base de alto rendimiento con **.NET 9**, **Clean Architecture** y **Seguridad JWT**. Diseñado para ser la base sólida de cualquier proyecto en **BenitezLabs**.



## 🛠️ Tecnologías y Paquetes

* **Framework:** .NET 9.0
* **Base de Datos:** MariaDB / MySQL (vía Pomelo EF Core)
* **Seguridad:** JWT Bearer + ASP.NET Identity (Hashing de contraseñas)
* **Documentación:** OpenAPI + Scalar UI
* **Validaciones:** FluentValidation

---

## 📦 Instalación de Dependencias (Fase 3 & 4)

Si estás construyendo el proyecto desde la raíz, ejecuta estos comandos para tener la base técnica completa:

```bash
# Capa API (Presentación y Seguridad)
dotnet add API package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add API package Microsoft.EntityFrameworkCore.Design
dotnet add API package FluentValidation.AspNetCore
dotnet add API package Microsoft.AspNetCore.OpenApi 
dotnet add API package Scalar.AspNetCore

# Capa Application (Lógica y DTOs)
dotnet add Application package Microsoft.EntityFrameworkCore
dotnet add Application package FluentValidation

# Capa Infrastructure (Persistencia e Identidad)
dotnet add Infrastructure package Microsoft.EntityFrameworkCore
dotnet add Infrastructure package Microsoft.Extensions.Identity.Core
dotnet add Infrastructure package System.IdentityModel.Tokens.Jwt
dotnet add Infrastructure package Pomelo.EntityFrameworkCore.MySql
dotnet add Infrastructure package Microsoft.EntityFrameworkCore.Design
🗄️ Manejo de Base de Datos (Entity Framework)
Debido a la estructura de Clean Architecture, los comandos de EF deben especificar siempre el proyecto donde reside el DbContext (Infrastructure) y el proyecto donde reside la configuración (API).

1. Crear una nueva migración
Cada vez que cambies una Entidad en el Domain, ejecuta:

Bash
dotnet ef migrations add NombreDeLaMigracion --project Infrastructure --startup-project API
2. Actualizar la Base de Datos (MariaDB)
Para impactar los cambios en tu servidor local:

Bash
dotnet ef database update --project Infrastructure --startup-project API
Nota para MariaDB: Asegúrate de usar CURRENT_TIMESTAMP(6) en lugar de GETUTCDATE() en tus configuraciones de Fluent API para evitar errores de sintaxis.

🔐 Seguridad y Autenticación
El template ya viene con un Auth/Login y Auth/Register funcional.

Configuración: Define tu Jwt:Key en el archivo appsettings.Development.json.

Uso: Los endpoints protegidos requieren el header:

Authorization: Bearer <TU_TOKEN_JWT>

Seed: El sistema crea automáticamente un usuario administrador al aplicar la primera migración:

Usuario: admin@benitezlabs.com

Password: Admin123!

⚙️ Uso del Template
Instalar localmente:
Bash
dotnet new install .
Crear nuevo proyecto:
Bash
dotnet new template-api -n NombreDeTuProyecto

▶️ Ejecución en Desarrollo
Navega a la carpeta de la API y arranca con monitoreo de cambios:

Bash
cd API
dotnet watch
Acceso a Scalar UI: http://localhost:<puerto>/scalar/v1
=======
# EmpadronamientoBackend
backend de sistema de empadronamiento
>>>>>>> 037df300faf7ab4489db7201d216856266d425be