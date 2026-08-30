using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;


namespace ChatApp.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subFolder)
    {

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", subFolder);


        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var outputStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputStream);
        }


        return $"/uploads/{subFolder}/{uniqueFileName}";
    }

    public void DeleteFile(string fileUrl)
    {
        var relativePath = fileUrl.TrimStart('/');
        var fullPath = Path.Combine(_environment.WebRootPath, "..", relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}