namespace EmpadronamientoBackend.Application.DTOs.Responses.Busqueda;

public class PersonaResumenResponse
{
    public int PersonaId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Apodo { get; set; } = string.Empty;
    public string? Sexo { get; set; }
    public int? Edad { get; set; }
    public decimal? Estatura { get; set; }
    public string? FechaNacimiento { get; set; }
    public string? Nacionalidad { get; set; }
    public string? Originario { get; set; }
    public string? Telefono { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Escolaridad { get; set; }
    public string? OficioProfesion { get; set; }
    public string? ObservacionesGenerales { get; set; }
    public string? FotoPrincipalUrl { get; set; }

    public List<EmpadronamientoHistorialDto> Empadronamientos { get; set; } = new();
    public List<DireccionDto> Direcciones { get; set; } = new();
    public List<FamiliarDto> Familiares { get; set; } = new();
    public List<RedSocialDto> RedesSociales { get; set; } = new();
    public List<FotoDto> Fotos { get; set; } = new();
}

public class EmpadronamientoHistorialDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty; 
    public string Hora { get; set; } = string.Empty;  
    public string CRP { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    
    public string UbicacionEvento { get; set; } = string.Empty; // Calle + Números
    public decimal? LatitudEvento { get; set; }
    public decimal? LongitudEvento { get; set; }
}

public class DireccionDto
{
    public string Calle { get; set; } = string.Empty;
    public string NumeroExterior { get; set; } = string.Empty;
    public string NumeroInterior { get; set; } = string.Empty;
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
    public string Tipo { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string? UrlPerfil { get; set; }
}

public class FotoDto
{
    public string Tipo { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}