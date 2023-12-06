using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using RquestBridge.Config;

namespace RquestBridge.Services;

public class MinioService
{
  private readonly ILogger<MinioService> _logger;
  private readonly IMinioClient _minioClient;
  private readonly MinioOptions _options;

  public MinioService(ILogger<MinioService> logger, IOptions<MinioOptions> options)
  {
    _logger = logger;
    _options = options.Value;
    _minioClient = new MinioClient()
      .WithEndpoint(_options.Host)
      .WithCredentials(_options.AccessKey, _options.SecretKey)
      .WithSSL(_options.Secure)
      .Build();
  }

  /// <summary>
  /// Check if a given S3 bucket exists.
  /// </summary>
  /// <returns><c>true</c> if the bucket exists, else <c>false</c>.</returns>
  public async Task<bool> StoreExists()
  {
    var args = new BucketExistsArgs().WithBucket(_options.Bucket);
    return await _minioClient.BucketExistsAsync(args);
  }

  /// <summary>
  /// Upload a file to an S3 bucket.
  /// </summary>
  /// <param name="filePath">The path of the file to be uploaded.</param>
  /// <exception cref="BucketNotFoundException">Thrown when the given bucket doesn't exists.</exception>
  /// <exception cref="MinioException">Thrown when any other error related to MinIO occurs.</exception>
  /// <exception cref="FileNotFoundException">Thrown when the file to be uploaded does not exist.</exception>
  public async Task WriteToStore(string filePath)
  {
    if (!await StoreExists())
      throw new BucketNotFoundException(_options.Bucket, $"No such bucket: {_options.Bucket}");

    if (!File.Exists(filePath)) throw new FileNotFoundException();

    var objectName = Path.GetFileName(filePath);
    var putObjectArgs = new PutObjectArgs()
      .WithBucket(_options.Bucket)
      .WithFileName(filePath)
      .WithObject(objectName);

    _logger.LogInformation($"Uploading {objectName} to {_options.Bucket}...");
    await _minioClient.PutObjectAsync(putObjectArgs);
    _logger.LogInformation($"Successfully uploaded {objectName} to {_options.Bucket}.");
  }
}
