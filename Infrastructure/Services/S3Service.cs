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
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _prefixFolder;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:S3:Buckets:Empadronamiento"];
            
            // Leemos el prefijo (dev/prod). Por defecto dev.
            var configPrefix = configuration["AWS:S3:Buckets:PrefixFolder"];
            _prefixFolder = string.IsNullOrWhiteSpace(configPrefix) ? "dev" : configPrefix.Trim('/');
        }

        /// <summary>
        /// Aplica el prefijo de entorno (dev/prod) preservando la subcarpeta enviada.
        /// Ejemplo: "Fotos/1.jpg" -> "dev/Fotos/1.jpg"
        /// </summary>
        private string AplicarPrefijo(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;

            // Si ya tiene el prefijo, no lo duplicamos
            if (path.StartsWith($"{_prefixFolder}/"))
                return path;

            return $"{_prefixFolder}/{path.TrimStart('/')}";
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string path, string contentType)
        {
            var keyCompleta = AplicarPrefijo(path);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = keyCompleta,
                BucketName = _bucketName,
                ContentType = contentType
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            // Retornamos la KEY completa con el prefijo para que se guarde en la BD
            return keyCompleta; 
        }

        public string GetFileUrl(string path)
        {
            var keyCompleta = AplicarPrefijo(path);
            return $"https://{_bucketName}.s3.amazonaws.com/{keyCompleta}";
        }

        public string GetPreSignedUrl(string path, int expirationMinutes = 60)
        {
            var keyCompleta = AplicarPrefijo(path);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = keyCompleta,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Verb = HttpVerb.GET
            };

            return _s3Client.GetPreSignedURL(request);
        }

        public async Task DeleteImageAsync(string path)
        {
            var keyCompleta = AplicarPrefijo(path);

            try
            {
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyCompleta
                });
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"Error al eliminar '{keyCompleta}' en S3: {ex.Message}", ex);
            }
        }
    }
}