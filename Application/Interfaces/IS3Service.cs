using System.IO;
using System.Threading.Tasks;

namespace EmpadronamientoBackend.Application.Interfaces
{
    /// <summary>
    /// Define operaciones básicas para almacenamiento en Amazon S3.
    /// </summary>
    public interface IS3Service
    {
        /// <summary>
        /// Sube un archivo al bucket.
        /// </summary>
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Obtiene la URL pública del archivo usando su nombre.
        /// </summary>
        string GetFileUrl(string fileName);

        /// <summary>
        /// Genera una URL temporal para acceder a un archivo privado.
        /// </summary>
        string GetPreSignedUrl(string fileName, int expirationMinutes = 60);

        /// <summary>
        /// Elimina un archivo del bucket S3 usando su nombre.
        /// </summary>
        Task DeleteImageAsync(string fileName);
    }
}