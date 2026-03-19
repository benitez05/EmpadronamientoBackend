public class CrearFotoRequest
{
    public int FotoId { get; set; } // Ajustado a PascalCase
    public required string TipoFoto { get; set; } 
    public string? Descripcion { get; set; } 
}