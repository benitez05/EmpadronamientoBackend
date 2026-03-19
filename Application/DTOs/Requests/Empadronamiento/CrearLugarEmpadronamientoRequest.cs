using Microsoft.AspNetCore.Http;

namespace EmpadronamientoBackend.Application.DTOs.Requests;
public class CrearLugarEmpadronamientoRequest
{
    public string Calle { get; set; } = string.Empty;

    public string NumeroExterior { get; set; } = string.Empty;

    public string NumeroInterior { get; set; } = string.Empty;

    public int CP { get; set; }

    public string? Colonia { get; set; }

    public string? Municipio { get; set; }

    public string? Estado { get; set; }

    public string? Referencia { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public int ImagenID { get; set; }
}