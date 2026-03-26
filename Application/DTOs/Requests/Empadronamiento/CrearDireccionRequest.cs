public class CrearDireccionRequest
{
    // 🔥 Se agrega el Id opcional para actualización
    public int? Id { get; set; }

    public string Calle { get; set; } = string.Empty;
    public string NumeroExterior { get; set; } = string.Empty;
    public string NumeroInterior { get; set; } = string.Empty;
    public int CP { get; set; }
    public required string Colonia { get; set; }
    public required string Municipio { get; set; }
    public required string Estado { get; set; }
    public required string Pais { get; set; }
    public string? Referencia { get; set; }
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public bool EsPrincipal { get; set; } = true;
}