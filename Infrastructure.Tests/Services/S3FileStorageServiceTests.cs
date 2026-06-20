using Amazon.S3;
using Amazon.S3.Model;
using Application.Entities;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Moq;

namespace Infrastructure.Tests.Services;

public class S3FileStorageServiceTests
{
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly S3Settings _settings;
    private readonly S3FileStorageService _service;

    public S3FileStorageServiceTests()
    {
        _s3ClientMock = new Mock<IAmazonS3>();
        _settings = new S3Settings
        {
            BucketName = "test-bucket",
            Region = "us-east-1"
        };

        var options = Options.Create(_settings);
        _service = new S3FileStorageService(_s3ClientMock.Object, options);
    }

    [Fact]
    public async Task UploadFileAsync_ShouldReturnCorrectUrl_WhenUploadIsSuccessful()
    {
        // Arrange
        var request = new FileRequest
        {
            Key = "test-key.jpg",
            FileStream = new MemoryStream(),
            ContentType = "image/jpeg"
        };

        // Act
        var result = await _service.UploadFileAsync(request, CancellationToken.None);

        // Assert
        var expectedUrl = $"https://test-bucket.s3.us-east-1.amazonaws.com/test-key.jpg";
        Assert.Equal(expectedUrl, result);

        _s3ClientMock.Verify(x => x.PutObjectAsync(
            It.Is<PutObjectRequest>(r => r.BucketName == _settings.BucketName && r.Key == request.Key),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldCallDeleteObjectAsync()
    {
        // Arrange
        var key = "test-key.jpg";

        // Act
        await _service.DeleteFileAsync(key, CancellationToken.None);

        // Assert
        _s3ClientMock.Verify(x => x.DeleteObjectAsync(
            It.Is<DeleteObjectRequest>(r => r.BucketName == _settings.BucketName && r.Key == key),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldThrowArgumentNullException_WhenKeyIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.DeleteFileAsync(string.Empty, CancellationToken.None));
    }
}
