using BenitezLabs.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;

public static class CatalogosSeed
{
    public static void Seed(ModelBuilder modelBuilder, DateTime fechaSeed, string sys, string ip, string dev)
    {
        // =========================
        // CATÁLOGOS
        // =========================
        modelBuilder.Entity<Catalogo>().HasData(
            new Catalogo { Id = 1, Clave = "TIPO_RED_SOCIAL", Nombre = "Tipo de Red Social", Descripcion = "Redes sociales disponibles", OrganizacionId = 1, FechaCreacion = fechaSeed, CreadoPor = sys },
            new Catalogo { Id = 2, Clave = "ESTADO_CIVIL", Nombre = "Estado Civil", Descripcion = "Estado civil de la persona", OrganizacionId = 1, FechaCreacion = fechaSeed, CreadoPor = sys },
            new Catalogo { Id = 3, Clave = "ESCOLARIDAD", Nombre = "Escolaridad", Descripcion = "Nivel educativo", OrganizacionId = 1, FechaCreacion = fechaSeed, CreadoPor = sys },
            new Catalogo { Id = 5, Clave = "PARENTESCO", Nombre = "Parentesco", Descripcion = "Relación familiar", OrganizacionId = 1, FechaCreacion = fechaSeed, CreadoPor = sys },
            new Catalogo { Id = 6, Clave = "TIPO_CARRO_RADIO_PATRULLA", Nombre = "Tipo Carro Radio Patrulla", Descripcion = "Tipos de unidades", OrganizacionId = 1, FechaCreacion = fechaSeed, CreadoPor = sys },
            new Catalogo { Id = 7, Clave = "OFICIO_PROFESION", Nombre = "Oficio / Profesión", Descripcion = "Ocupación de la persona", OrganizacionId = 1, FechaCreacion = fechaSeed, CreadoPor = sys }
        );

        // =========================
        // ITEMS
        // =========================
        modelBuilder.Entity<CatalogoItem>().HasData(

            // =====================
            // RED SOCIAL
            // =====================
            new CatalogoItem { Id = 1, Nombre = "Facebook", Codigo = "FACEBOOK", Orden = 1, CatalogoId = 1, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 2, Nombre = "Instagram", Codigo = "INSTAGRAM", Orden = 2, CatalogoId = 1, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 3, Nombre = "X (Twitter)", Codigo = "X", Orden = 3, CatalogoId = 1, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 4, Nombre = "TikTok", Codigo = "TIKTOK", Orden = 4, CatalogoId = 1, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 5, Nombre = "WhatsApp", Codigo = "WHATSAPP", Orden = 5, CatalogoId = 1, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 6, Nombre = "Telegram", Codigo = "TELEGRAM", Orden = 6, CatalogoId = 1, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

            // =====================
            // ESTADO CIVIL
            // =====================
            new CatalogoItem { Id = 10, Nombre = "Soltero", Codigo = "SOLTERO", Orden = 1, CatalogoId = 2, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 11, Nombre = "Casado", Codigo = "CASADO", Orden = 2, CatalogoId = 2, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 12, Nombre = "Divorciado", Codigo = "DIVORCIADO", Orden = 3, CatalogoId = 2, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 13, Nombre = "Unión Libre", Codigo = "UNION_LIBRE", Orden = 4, CatalogoId = 2, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 14, Nombre = "Viudo", Codigo = "VIUDO", Orden = 5, CatalogoId = 2, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

            // =====================
            // ESCOLARIDAD
            // =====================
            new CatalogoItem { Id = 20, Nombre = "Sin estudios", Codigo = "SIN_ESTUDIOS", Orden = 1, CatalogoId = 3, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 21, Nombre = "Primaria", Codigo = "PRIMARIA", Orden = 2, CatalogoId = 3, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 22, Nombre = "Secundaria", Codigo = "SECUNDARIA", Orden = 3, CatalogoId = 3, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 23, Nombre = "Preparatoria", Codigo = "PREPARATORIA", Orden = 4, CatalogoId = 3, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 24, Nombre = "Técnico", Codigo = "TECNICO", Orden = 5, CatalogoId = 3, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 25, Nombre = "Licenciatura", Codigo = "LICENCIATURA", Orden = 6, CatalogoId = 3, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 26, Nombre = "Maestría", Codigo = "MAESTRIA", Orden = 7, CatalogoId = 3, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 27, Nombre = "Doctorado", Codigo = "DOCTORADO", Orden = 8, CatalogoId = 3, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

            // =====================
            // PARENTESCO
            // =====================
            new CatalogoItem { Id = 40, Nombre = "Padre", Codigo = "PADRE", Orden = 1, CatalogoId = 5, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 41, Nombre = "Madre", Codigo = "MADRE", Orden = 2, CatalogoId = 5, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 42, Nombre = "Hermano(a)", Codigo = "HERMANO", Orden = 3, CatalogoId = 5, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 43, Nombre = "Pareja", Codigo = "PAREJA", Orden = 4, CatalogoId = 5, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 44, Nombre = "Hijo(a)", Codigo = "HIJO", Orden = 5, CatalogoId = 5, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 45, Nombre = "Otro", Codigo = "OTRO", Orden = 6, CatalogoId = 5, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

            // =====================
            // PATRULLA
            // =====================
            new CatalogoItem { Id = 50, Nombre = "Sedán", Codigo = "SEDAN", Orden = 1, CatalogoId = 6, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 51, Nombre = "PickUp", Codigo = "PICKUP", Orden = 2, CatalogoId = 6, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 52, Nombre = "Motocicleta", Codigo = "MOTO", Orden = 3, CatalogoId = 6, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 53, Nombre = "SUV", Codigo = "SUV", Orden = 4, CatalogoId = 6, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

            // =====================
            // OFICIO / PROFESIÓN
            // =====================
            new CatalogoItem { Id = 60, Nombre = "Empleado", Codigo = "EMPLEADO", Orden = 1, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 61, Nombre = "Obrero", Codigo = "OBRERO", Orden = 2, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 62, Nombre = "Comerciante", Codigo = "COMERCIANTE", Orden = 3, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 63, Nombre = "Estudiante", Codigo = "ESTUDIANTE", Orden = 4, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 64, Nombre = "Desempleado", Codigo = "DESEMPLEADO", Orden = 5, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 65, Nombre = "Chofer", Codigo = "CHOFER", Orden = 6, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 66, Nombre = "Albañil", Codigo = "ALBANIL", Orden = 7, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 67, Nombre = "Técnico", Codigo = "TECNICO", Orden = 8, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 68, Nombre = "Profesionista", Codigo = "PROFESIONISTA", Orden = 9, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
            new CatalogoItem { Id = 69, Nombre = "Otro", Codigo = "OTRO", Orden = 10, CatalogoId = 7, OrganizacionId = 1, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev }
        );
    }
}