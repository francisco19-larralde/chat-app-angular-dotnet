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
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
            throw new InvalidOperationException("El nombre del archivo no es válido.");

        var safeSubFolder = Path.GetFileName(subFolder);
        if (!string.Equals(safeSubFolder, subFolder, StringComparison.Ordinal))
            throw new InvalidOperationException("La carpeta de destino no es válida.");

        var uniqueFileName = $"{Guid.NewGuid():N}_{safeFileName}";

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", safeSubFolder);


        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var outputStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputStream);
        }


        return $"/uploads/{safeSubFolder}/{uniqueFileName}";
    }

    public void DeleteFile(string fileUrl)
    {
        var relativePath = fileUrl.TrimStart('/');
        var fullPath = Path.Combine(_environment.WebRootPath, "..", relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
