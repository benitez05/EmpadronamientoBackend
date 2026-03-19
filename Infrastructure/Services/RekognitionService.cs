using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using EmpadronamientoBackend.Application.Interfaces;
using EmpadronamientoBackend.Application.DTOs.Responses;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace EmpadronamientoBackend.Infrastructure.Services
{
    /// <summary>
    /// Servicio que interactúa con Amazon Rekognition.
    /// </summary>
    public class RekognitionService : IRekognitionService
    {
        private readonly IAmazonRekognition _rekognitionClient;

        public RekognitionService(IAmazonRekognition rekognitionClient)
        {
            _rekognitionClient = rekognitionClient;
        }

        // Agrega este método a IRekognitionService y RekognitionService
        public async Task<string?> IndexFaceFromS3Async(string bucketName, string fileName, string collectionId, string externalImageId)
        {
            var request = new IndexFacesRequest
            {
                CollectionId = collectionId,
                ExternalImageId = externalImageId,
                Image = new Amazon.Rekognition.Model.Image
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = bucketName,
                        Name = fileName
                    }
                },
                MaxFaces = 1,
                QualityFilter = QualityFilter.AUTO
            };

            var response = await _rekognitionClient.IndexFacesAsync(request);
            return response.FaceRecords.FirstOrDefault()?.Face?.FaceId;
        }

        /// <summary>
        /// Indexa un rostro en una colección.
        /// Devuelve el FaceId generado por Rekognition.
        /// </summary>
        public async Task<string?> IndexFaceAsync(Stream imageStream, string collectionId, string externalImageId)
        {
            var request = new IndexFacesRequest
            {
                CollectionId = collectionId,
                ExternalImageId = externalImageId,
                Image = new Image
                {
                    Bytes = new MemoryStream(ReadStream(imageStream))
                },
                MaxFaces = 1,
                QualityFilter = QualityFilter.AUTO
            };

            var response = await _rekognitionClient.IndexFacesAsync(request);

            return response.FaceRecords.FirstOrDefault()?.Face?.FaceId;
        }

        public async Task<IndexFaceResult> IndexFaceAndGetDetailsAsync(
    string bucketName,
    string fileName,
    string collectionId,
    string externalImageId)
        {
            var resultado = new IndexFaceResult();

            var request = new IndexFacesRequest
            {
                CollectionId = collectionId,
                ExternalImageId = externalImageId,
                Image = new Amazon.Rekognition.Model.Image
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = bucketName,
                        Name = fileName
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
        /// Devuelve el ExternalImageId asociado.
        /// </summary>
        public async Task<string?> SearchFaceAsync(Stream imageStream, string collectionId)
        {
            var request = new SearchFacesByImageRequest
            {
                CollectionId = collectionId,
                Image = new Image
                {
                    Bytes = new MemoryStream(ReadStream(imageStream))
                },
                MaxFaces = 1,
                FaceMatchThreshold = 90
            };

            var response = await _rekognitionClient.SearchFacesByImageAsync(request);

            return response.FaceMatches.FirstOrDefault()?.Face?.ExternalImageId;
        }

        public async Task<FaceValidationResult> ValidateFaceAsync(Stream imageStream)
        {
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);
            ms.Position = 0;

            var request = new DetectFacesRequest
            {
                Image = new Amazon.Rekognition.Model.Image
                {
                    Bytes = ms
                },
                Attributes = new List<string> { "ALL" } // 🔥 NECESARIO
            };

            var response = await _rekognitionClient.DetectFacesAsync(request);

            // ❌ No hay caras
            if (response.FaceDetails == null || !response.FaceDetails.Any())
            {
                return new FaceValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "No se detectó ningún rostro en la imagen."
                };
            }

            // ❌ Más de una cara
            if (response.FaceDetails.Count > 1)
            {
                return new FaceValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Se detectaron múltiples rostros. Solo se permite uno."
                };
            }

            var face = response.FaceDetails.First();

            if (face.EyesOpen == null || face.EyesOpen.Value == false || face.EyesOpen.Confidence < 80)
            {
                return new FaceValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Los ojos deben estar abiertos."
                };
            }

            if (face.FaceOccluded?.Value == true && face.FaceOccluded.Confidence > 80)
            {
                return new FaceValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "El rostro está obstruido (cubierto parcialmente)."
                };
            }

            // ✅ Todo OK
            return new FaceValidationResult
            {
                IsValid = true
            };
        }

        /// <summary>
        /// Analiza atributos faciales como edad, género y emociones.
        /// </summary>
        public async Task<object?> AnalyzeFaceAsync(Stream imageStream)
        {
            var request = new DetectFacesRequest
            {
                Image = new Image
                {
                    Bytes = new MemoryStream(ReadStream(imageStream))
                },
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
                SourceImage = new Image
                {
                    Bytes = new MemoryStream(ReadStream(sourceImage))
                },
                TargetImage = new Image
                {
                    Bytes = new MemoryStream(ReadStream(targetImage))
                },
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

        /// <summary>
        /// Convierte Stream a byte[] para el SDK.
        /// </summary>
        private byte[] ReadStream(Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }


    }
}