public class Plan
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public decimal Precio { get; set; }
    public int MaxUsuarios { get; set; } // El único límite que cuenta
    
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}