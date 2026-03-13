using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using EmpadronamientoBackend.Application.Interfaces;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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