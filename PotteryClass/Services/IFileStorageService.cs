namespace PotteryClass.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(byte[] content, string fileName, string mimeType);
    Task DeleteFileAsync(string url);
}