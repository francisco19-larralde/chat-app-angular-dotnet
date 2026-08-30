namespace ChatApp.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subFolder);
    void DeleteFile(string fileUrl);
}