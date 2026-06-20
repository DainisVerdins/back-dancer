using Application.Entities;

namespace Application.Interfaces.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads file into dedicated storage
    /// </summary>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <returns>string contains cloud information to access resource</returns>
    Task<string> UploadFileAsync(FileRequest request, CancellationToken token = default);
    Task DeleteFileAsync(string key, CancellationToken token = default);
}
