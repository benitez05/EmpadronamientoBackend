namespace EmpadronamientoBackend.Application.DTOs.Responses.Busqueda;

public class PersonaResumenResponse
{
    public int PersonaId { get; set; }
    
    // Campos de nombre separados como en el JSON
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    
    public string? FechaNacimiento { get; set; }
    public int? Edad { get; set; }
    public decimal? Estatura { get; set; }
    public string? Sexo { get; set; }
    public string? Originario { get; set; }
    public string? Telefono { get; set; }
    public string? Apodo { get; set; }
    public string? Nacionalidad { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Escolaridad { get; set; }
    public string? OficioProfesion { get; set; }
    public string? ObservacionesGenerales { get; set; }

    // Objeto rostro como lo pide el JSON
    public RostroDto? Rostro { get; set; }
    // Mantenemos la URL por si el front la necesita directo
    public string? FotoPrincipalUrl { get; set; } 

    // Listas con todo lo referente a la persona
    public List<EmpadronamientoHistorialDto> Empadronamientos { get; set; } = new();
    public List<DireccionDto> Direcciones { get; set; } = new();
    public List<FamiliarDto> Familiares { get; set; } = new();
    public List<RedSocialDto> RedesSociales { get; set; } = new();
    public List<FotoDto> Fotos { get; set; } = new();
}

public class RostroDto
{
    public int FotoId { get; set; }
}

public class EmpadronamientoHistorialDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty; 
    public string Hora { get; set; } = string.Empty;  
    public string Crpn { get; set; } = string.Empty; // Cambiado a Crpn
    public string NarrativaHechos { get; set; } = string.Empty; // Cambiado de Observaciones a NarrativaHechos
    
    // El lugar ahora es un objeto estructurado, no un string plano
    public LugarDto? Lugar { get; set; }
}

public class LugarDto
{
    public string Calle { get; set; } = string.Empty;
    public string NumeroExterior { get; set; } = string.Empty;
    public string NumeroInterior { get; set; } = string.Empty;
    public int? Cp { get; set; }
    public string Colonia { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public int? ImagenID { get; set; }
}

public class DireccionDto
{
    public string Calle { get; set; } = string.Empty;
    public string NumeroExterior { get; set; } = string.Empty;
    public string NumeroInterior { get; set; } = string.Empty;
    public int? Cp { get; set; } // Agregado
    public string Colonia { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public decimal Latitud { get; set; } 
    public decimal Longitud { get; set; } 
    public bool EsPrincipal { get; set; }
}

public class FamiliarDto
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Parentesco { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}

public class RedSocialDto
{
    public string TipoRedSocial { get; set; } = string.Empty; // Ajustado al JSON
    public string Usuario { get; set; } = string.Empty;
    public string? UrlPerfil { get; set; }
}

public class FotoDto
{
    public int FotoId { get; set; } // Agregado
    public string TipoFoto { get; set; } = string.Empty; // Ajustado al JSON
    public string? Descripcion { get; set; }
    public string Url { get; set; } = string.Empty; // Mantenemos la URL
}