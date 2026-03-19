using System.IO;
using System.Threading.Tasks;
using EmpadronamientoBackend.Application.DTOs.Responses;

namespace EmpadronamientoBackend.Application.Interfaces;

/// <summary>
/// Contrato para operaciones de reconocimiento facial.
/// </summary>
public interface IRekognitionService
{
    /// <summary>
    /// Valida que la imagen cumpla con reglas de rostro:
    /// - Exactamente un rostro
    /// - Ojos abiertos
    /// - Rostro no obstruido
    /// </summary>
    /// <param name="imageStream">Stream de la imagen a validar</param>
    /// <returns>
    /// Resultado de la validación con estado y mensaje de error en caso de fallo
    /// </returns>
    Task<FaceValidationResult> ValidateFaceAsync(Stream imageStream);

    Task<IndexFaceResult> IndexFaceAndGetDetailsAsync(
        string bucketName,
        string fileName,
        string collectionId,
        string externalImageId);

    /// <summary>
    /// Indexa un rostro en una colección.
    /// </summary>
    Task<string?> IndexFaceAsync(Stream imageStream, string collectionId, string externalImageId);

    /// <summary>
    /// Busca coincidencias en una colección usando una imagen.
    /// </summary>
    Task<string?> SearchFaceAsync(Stream imageStream, string collectionId);

    /// <summary>
    /// Analiza atributos faciales (edad, emociones, etc).
    /// </summary>
    Task<object?> AnalyzeFaceAsync(Stream imageStream);

    /// <summary>
    /// Compara dos imágenes para determinar similitud facial.
    /// </summary>
    Task<float?> CompareFacesAsync(Stream sourceImage, Stream targetImage);

    /// <summary>
    /// Elimina un rostro de una colección.
    /// </summary>
    Task DeleteFaceAsync(string collectionId, string faceId);
}