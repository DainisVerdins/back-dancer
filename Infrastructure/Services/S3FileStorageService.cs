using Amazon.S3;
using Amazon.S3.Model;
using Application.Interfaces.Services;

namespace Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName = "your-bucket-name";

    public S3FileStorageService(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {

        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = fileUrl
        }, cancellationToken);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var key = $"{Guid.NewGuid()}-{fileName}"; // unique file name

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead // makes file public by url
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        // return url of the image
        return $"https://{_bucketName}.s3.amazonaws.com/{key}";
    }
}
