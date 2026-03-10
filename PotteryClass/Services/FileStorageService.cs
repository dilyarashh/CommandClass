using Minio;
using Minio.DataModel.Args;

namespace PotteryClass.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;

    public FileStorageService(string endpoint, string accessKey, string secretKey, string bucket)
    {
        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();
        
        _bucket = bucket;

        Task.Run(async () =>
        {
            bool exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
            if (!exists)
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));
        }).GetAwaiter().GetResult();
    }

    public async Task<string> UploadFileAsync(byte[] content, string fileName, string mimeType)
    {
        using var ms = new MemoryStream(content);

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(fileName)
            .WithStreamData(ms)
            .WithObjectSize(ms.Length)
            .WithContentType(mimeType));

        return $"http://{_client}/{_bucket}/{fileName}";
    }

    public async Task DeleteFileAsync(string url)
    {
        var fileName = url.Split('/').Last();
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(fileName));
    }
}