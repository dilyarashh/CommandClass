using Minio;
using Minio.DataModel.Args;

namespace PotteryClass.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly SemaphoreSlim _bucketInitLock = new(1, 1);
    private bool _bucketInitialized;

    public FileStorageService(string endpoint, string accessKey, string secretKey, string bucket)
    {
        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();
        
        _bucket = bucket;
    }

    public async Task<string> UploadFileAsync(byte[] content, string fileName, string mimeType)
    {
        await EnsureBucketExistsAsync();

        using var ms = new MemoryStream(content);

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(fileName)
            .WithStreamData(ms)
            .WithObjectSize(ms.Length)
            .WithContentType(mimeType));

        return$"http://localhost:9000/pottery-files/{fileName}";
    }

    public async Task DeleteFileAsync(string url)
    {
        await EnsureBucketExistsAsync();

        var fileName = url.Split('/').Last();
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
}
