namespace AccessiTrack.Application.Common.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(string userId, string email, IList<string> roles);
}
