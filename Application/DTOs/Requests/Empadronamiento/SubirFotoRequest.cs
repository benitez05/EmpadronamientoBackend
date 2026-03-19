using Microsoft.AspNetCore.Http;

public class SubirFotoRequest
{
    public IFormFile Archivo { get; set; } = null!;
    public required string Tipo { get; set; } 
}