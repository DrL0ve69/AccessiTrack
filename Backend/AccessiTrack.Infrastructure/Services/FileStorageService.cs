using AccessiTrack.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AccessiTrack.Infrastructure.Services;

/// <summary>
/// Implementation of file storage service for handling file operations (avatars, etc.)
/// Currently stores files locally in wwwroot/uploads/avatars/
/// Can be extended to use cloud storage (Azure Blob, S3, etc.)
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private const string UploadFolder = "uploads/avatars";
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadAvatarAsync(IFormFile file, Guid userId, CancellationToken cancellationToken)
    {
        // Validate file
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("File is empty");

        if (file.Length > MaxFileSize)
            throw new InvalidOperationException($"File size exceeds maximum limit of {MaxFileSize / (1024 * 1024)} MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, UploadFolder);
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            _logger.LogInformation("Avatar uploaded successfully for user {UserId}", userId);

            // Return relative URL path
            return $"/uploads/avatars/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar for user {UserId}", userId);
            throw new InvalidOperationException("Failed to upload file", ex);
        }
    }

    public async Task DeleteAvatarAsync(string avatarPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(avatarPath))
            return;

        try
        {
            // Extract filename from URL path
            var fileName = Path.GetFileName(avatarPath);
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var filePath = Path.Combine(_environment.WebRootPath, UploadFolder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Avatar deleted: {FileName}", fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting avatar: {AvatarPath}", avatarPath);
            // Don't throw - deletion failure shouldn't block profile deletion
        }
    }
}
