namespace Application.Interfaces.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads file into dedicated service 
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <returns>string containing url with image what was uploaded</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
