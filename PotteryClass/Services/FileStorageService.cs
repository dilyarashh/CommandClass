using Minio;
using Minio.DataModel.Args;

namespace PotteryClass.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;
    private readonly string _objectPrefix;
    private readonly SemaphoreSlim _bucketInitLock = new(1, 1);
    private bool _bucketInitialized;

    public FileStorageService(
        string endpoint,
        string accessKey,
        string secretKey,
        string bucket,
        bool useSsl,
        string? publicBaseUrl,
        string? objectPrefix)
    {
        var clientBuilder = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey);

        if (useSsl)
            clientBuilder = clientBuilder.WithSSL();

        _client = clientBuilder.Build();

        _bucket = bucket;
        _objectPrefix = NormalizePrefix(objectPrefix);
        _publicBaseUrl = !string.IsNullOrWhiteSpace(publicBaseUrl)
            ? publicBaseUrl.TrimEnd('/')
            : BuildDefaultPublicBaseUrl(endpoint, bucket, useSsl);
    }

    public async Task<string> UploadFileAsync(byte[] content, string fileName, string mimeType)
    {
        await EnsureBucketExistsAsync();

        using var ms = new MemoryStream(content);
        var objectKey = BuildObjectKey(fileName);

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithStreamData(ms)
            .WithObjectSize(ms.Length)
            .WithContentType(mimeType));

        return $"{_publicBaseUrl}/{EncodeObjectKeyForUrl(objectKey)}";
    }

    public async Task DeleteFileAsync(string url)
    {
        await EnsureBucketExistsAsync();

        var fileName = ResolveObjectKeyFromUrl(url);
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(fileName));
    }

    private async Task EnsureBucketExistsAsync()
    {
        if (_bucketInitialized)
            return;

        await _bucketInitLock.WaitAsync();
        try
        {
            if (_bucketInitialized)
                return;

            bool exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
            if (!exists)
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));

            _bucketInitialized = true;
        }
        finally
        {
            _bucketInitLock.Release();
        }
    }

    private static string NormalizePrefix(string? objectPrefix)
    {
        if (string.IsNullOrWhiteSpace(objectPrefix))
            return string.Empty;

        return objectPrefix.Trim().Trim('/');
    }

    private static string BuildDefaultPublicBaseUrl(string endpoint, string bucket, bool useSsl)
    {
        var scheme = useSsl ? "https" : "http";
        return $"{scheme}://{endpoint.TrimEnd('/')}/{bucket}";
    }

    private string BuildObjectKey(string originalFileName)
    {
        var safeName = Path.GetFileName(originalFileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
        var datePath = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var objectKey = $"{datePath}/{uniqueName}";

        if (string.IsNullOrWhiteSpace(_objectPrefix))
            return objectKey;

        return $"{_objectPrefix}/{objectKey}";
    }

    private static string EncodeObjectKeyForUrl(string objectKey)
    {
        return string.Join("/", objectKey
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));
    }

    private string ResolveObjectKeyFromUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url.Trim().Trim('/');

        var path = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var bucketPrefix = $"{_bucket}/";
        if (path.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
            return path[bucketPrefix.Length..];

        return path;
    }
}
