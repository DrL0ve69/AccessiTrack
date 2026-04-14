using Microsoft.AspNetCore.Http;

namespace AccessiTrack.Application.Common.Interfaces;

/// <summary>
/// Service for handling file storage operations (avatars, etc.)
/// Currently stores files locally in wwwroot/uploads/avatars/
/// Can be extended to use cloud storage (Azure Blob, S3, etc.)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload and store an avatar file
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="userId">User ID for file organization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>URL/path to the stored file</returns>
    Task<string> UploadAvatarAsync(IFormFile file, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Delete an avatar file
    /// </summary>
    /// <param name="avatarPath">Path or URL of the file to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAvatarAsync(string avatarPath, CancellationToken cancellationToken);
}
