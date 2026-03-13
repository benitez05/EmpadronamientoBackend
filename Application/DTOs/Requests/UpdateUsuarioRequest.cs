using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace EmpadronamientoBackend.Application.DTOs.Requests
{
    /// <summary>
    /// Request para actualizar un usuario con imagen como archivo.
    /// </summary>
    public class UpdateUsuarioRequest
    {
        [DefaultValue("Roberto")]
        public string Nombre { get; set; } = null!;

        [DefaultValue("Benitez")]
        public string Apellidos { get; set; } = null!;

        [DefaultValue("8112345678")]
        public string? Celular { get; set; }

        /// <summary>
        /// Imagen enviada como archivo.
        /// </summary>
        public IFormFile? Imagen { get; set; }
    }
}