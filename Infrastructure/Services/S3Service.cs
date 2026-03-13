using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using EmpadronamientoBackend.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EmpadronamientoBackend.Infrastructure.Services
{
    /// <summary>
    /// Servicio que maneja almacenamiento de archivos en Amazon S3.
    /// </summary>
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        /// <summary>
        /// Constructor que obtiene el cliente S3 y configuración del bucket.
        /// </summary>
        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:S3:Buckets:Empadronamiento"];
        }

        /// <summary>
        /// Sube un archivo al bucket configurado.
        /// </summary>
        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType)
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = contentType
            };

            var transferUtility = new TransferUtility(_s3Client);

            await transferUtility.UploadAsync(uploadRequest);

            return GetFileUrl(fileName);
        }

        /// <summary>
        /// Obtiene la URL pública del archivo.
        /// </summary>
        public string GetFileUrl(string fileName)
        {
            return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
        }

        /// <summary>
        /// Genera una URL temporal para acceder a un archivo privado.
        /// </summary>
        public string GetPreSignedUrl(string fileName, int expirationMinutes = 60)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Verb = HttpVerb.GET
            };

            return _s3Client.GetPreSignedURL(request);
        }

        /// <summary>
        /// Elimina un archivo del bucket S3 usando su nombre.
        /// </summary>
        /// <param name="fileName">Nombre completo del archivo en S3 (incluyendo carpetas si aplica).</param>
        public async Task DeleteImageAsync(string fileName)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
            }
            catch (AmazonS3Exception ex)
            {
                // Registrar el error, pero no interrumpir la aplicación
                // Puedes lanzar una excepción personalizada si quieres
                throw new Exception($"Error al eliminar el archivo '{fileName}' en S3: {ex.Message}", ex);
            }
        }
    }
}