using Amazon.S3;
using Amazon.S3.Model;
using Application.Entities;
using Application.Interfaces.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Settings _settings;

    public S3FileStorageService(IAmazonS3 s3Client, IOptions<S3Settings> settings)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
    }

    public async Task DeleteFileAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key
        }, cancellationToken);
    }

    public async Task<string> UploadFileAsync(FileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = request.Key,
            InputStream = request.FileStream,
            ContentType = request.ContentType,
            CannedACL = S3CannedACL.PublicRead
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        // return url of the image
        return $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{request.Key}";
    }
}
