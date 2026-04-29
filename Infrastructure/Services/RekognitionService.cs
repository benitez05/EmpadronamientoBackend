using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using EmpadronamientoBackend.Application.Interfaces;
using EmpadronamientoBackend.Application.DTOs.Responses;
using Microsoft.Extensions.Configuration; // 🔥 Necesario para leer el prefijo
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace EmpadronamientoBackend.Infrastructure.Services
{
    /// <summary>
    /// Servicio que interactúa con Amazon Rekognition.
    /// Maneja automáticamente el prefijo de entorno (dev/prod) para rutas de S3.
    /// </summary>
    public class RekognitionService : IRekognitionService
    {
        private readonly IAmazonRekognition _rekognitionClient;
        private readonly string _prefixFolder;

        public RekognitionService(IAmazonRekognition rekognitionClient, IConfiguration configuration)
        {
            _rekognitionClient = rekognitionClient;
            
            // Leemos el prefijo (dev/prod). Por defecto dev si no existe.
            var configPrefix = configuration["AWS:S3:Buckets:PrefixFolder"];
            _prefixFolder = string.IsNullOrWhiteSpace(configPrefix) ? "dev" : configPrefix.Trim('/');
        }

        /// <summary>
        /// Asegura que la ruta del archivo incluya el prefijo del entorno actual.
        /// </summary>
        private string AplicarPrefijoS3(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;
            if (path.StartsWith($"{_prefixFolder}/")) return path;
            return $"{_prefixFolder}/{path.TrimStart('/')}";
        }

        /// <summary>
        /// Indexa un rostro referenciando un archivo ya existente en S3.
        /// </summary>
        public async Task<string?> IndexFaceFromS3Async(string bucketName, string fileName, string collectionId, string externalImageId)
        {
            var rutaCompleta = AplicarPrefijoS3(fileName);

            var request = new IndexFacesRequest
            {
                CollectionId = collectionId,
                ExternalImageId = externalImageId,
                Image = new Amazon.Rekognition.Model.Image
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = bucketName,
                        Name = rutaCompleta
                    }
                },
                MaxFaces = 1,
                QualityFilter = QualityFilter.AUTO
            };

            var response = await _rekognitionClient.IndexFacesAsync(request);
            return response.FaceRecords.FirstOrDefault()?.Face?.FaceId;
        }

        /// <summary>
        /// Indexa un rostro enviando los bytes directamente (No requiere prefijo S3).
        /// </summary>
        public async Task<string?> IndexFaceAsync(Stream imageStream, string collectionId, string externalImageId)
        {
            var request = new IndexFacesRequest
            {
                CollectionId = collectionId,
                ExternalImageId = externalImageId,
                Image = new Amazon.Rekognition.Model.Image
                {
                    Bytes = new MemoryStream(ReadStream(imageStream))
                },
                MaxFaces = 1,
                QualityFilter = QualityFilter.AUTO
            };

            var response = await _rekognitionClient.IndexFacesAsync(request);
            return response.FaceRecords.FirstOrDefault()?.Face?.FaceId;
        }

        /// <summary>
        /// Indexa un rostro desde S3 y devuelve detalles como BoundingBox y Confidence.
        /// </summary>
        public async Task<IndexFaceResult> IndexFaceAndGetDetailsAsync(
            string bucketName,
            string fileName,
            string collectionId,
            string externalImageId)
        {
            var resultado = new IndexFaceResult();
            var rutaCompleta = AplicarPrefijoS3(fileName);

            var request = new IndexFacesRequest
            {
                CollectionId = collectionId,
                ExternalImageId = externalImageId,
                Image = new Amazon.Rekognition.Model.Image
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = bucketName,
                        Name = rutaCompleta
                    }
                },
                MaxFaces = 1,
                QualityFilter = QualityFilter.AUTO
            };

            var response = await _rekognitionClient.IndexFacesAsync(request);
            var faceRecord = response.FaceRecords.FirstOrDefault();

            if (faceRecord?.Face != null)
            {
                resultado.FaceId = faceRecord.Face.FaceId;
                resultado.Confidence = faceRecord.Face.Confidence;

                if (faceRecord.Face.BoundingBox != null)
                {
                    var box = faceRecord.Face.BoundingBox;
                    resultado.BoundingBoxJson = JsonSerializer.Serialize(new
                    {
                        Width = box.Width,
                        Height = box.Height,
                        Left = box.Left,
                        Top = box.Top
                    });
                }
            }

            return resultado;
        }

        /// <summary>
        /// Busca coincidencias en la colección.
        /// Devuelve una lista de tuplas con el ExternalImageId y el % de Similitud.
        /// </summary>
        public async Task<List<(string ExternalImageId, float Similarity)>> SearchFaceAsync(Stream imageStream, string collectionId, int maxFaces = 3)
        {
            var request = new SearchFacesByImageRequest
            {
                CollectionId = collectionId,
                Image = new Amazon.Rekognition.Model.Image
                {
                    Bytes = new MemoryStream(ReadStream(imageStream))
                },
                MaxFaces = maxFaces,
                FaceMatchThreshold = 85f
            };

            try
            {
                var response = await _rekognitionClient.SearchFacesByImageAsync(request);

                return response.FaceMatches
                    .Where(m => !string.IsNullOrEmpty(m.Face?.ExternalImageId))
                    .Select(m => (m.Face.ExternalImageId, m.Similarity ?? 0f))
                    .ToList();
            }
            catch (AmazonRekognitionException ex)
            {
                if (ex.ErrorCode == "InvalidParameterException") return new List<(string, float)>();
                throw;
            }
        }

        /// <summary>
        /// Valida que la imagen tenga un rostro apto para biometría.
        /// </summary>
        public async Task<FaceValidationResult> ValidateFaceAsync(Stream imageStream)
        {
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);
            ms.Position = 0;

            var request = new DetectFacesRequest
            {
                Image = new Amazon.Rekognition.Model.Image { Bytes = ms },
                Attributes = new List<string> { "ALL" }
            };

            var response = await _rekognitionClient.DetectFacesAsync(request);

            if (response.FaceDetails == null || !response.FaceDetails.Any())
            {
                return new FaceValidationResult { IsValid = false, ErrorMessage = "No se detectó ningún rostro." };
            }

            if (response.FaceDetails.Count > 1)
            {
                return new FaceValidationResult { IsValid = false, ErrorMessage = "Se detectaron múltiples rostros." };
            }

            var face = response.FaceDetails.First();

            if (face.EyesOpen?.Value == false || (face.EyesOpen?.Confidence < 70))
            {
                return new FaceValidationResult { IsValid = false, ErrorMessage = "Los ojos deben estar abiertos." };
            }

            if (face.FaceOccluded?.Value == true && face.FaceOccluded.Confidence > 80)
            {
                return new FaceValidationResult { IsValid = false, ErrorMessage = "El rostro está obstruido." };
            }

            return new FaceValidationResult { IsValid = true };
        }

        /// <summary>
        /// Analiza atributos faciales.
        /// </summary>
        public async Task<object?> AnalyzeFaceAsync(Stream imageStream)
        {
            var request = new DetectFacesRequest
            {
                Image = new Amazon.Rekognition.Model.Image { Bytes = new MemoryStream(ReadStream(imageStream)) },
                Attributes = new List<string> { "ALL" }
            };

            var response = await _rekognitionClient.DetectFacesAsync(request);
            return response.FaceDetails.FirstOrDefault();
        }

        /// <summary>
        /// Compara dos imágenes y devuelve el porcentaje de similitud.
        /// </summary>
        public async Task<float?> CompareFacesAsync(Stream sourceImage, Stream targetImage)
        {
            var request = new CompareFacesRequest
            {
                SourceImage = new Amazon.Rekognition.Model.Image { Bytes = new MemoryStream(ReadStream(sourceImage)) },
                TargetImage = new Amazon.Rekognition.Model.Image { Bytes = new MemoryStream(ReadStream(targetImage)) },
                SimilarityThreshold = 90
            };

            var response = await _rekognitionClient.CompareFacesAsync(request);
            return response.FaceMatches.FirstOrDefault()?.Similarity;
        }

        /// <summary>
        /// Elimina un rostro de la colección.
        /// </summary>
        public async Task DeleteFaceAsync(string collectionId, string faceId)
        {
            var request = new DeleteFacesRequest
            {
                CollectionId = collectionId,
                FaceIds = new List<string> { faceId }
            };

            await _rekognitionClient.DeleteFacesAsync(request);
        }

        private byte[] ReadStream(Stream stream)
        {
            if (stream is MemoryStream ms2) return ms2.ToArray();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}