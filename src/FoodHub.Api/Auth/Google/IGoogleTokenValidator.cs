namespace FoodHub.Api.Auth.Google;

public interface IGoogleTokenValidator
{
    Task<GoogleTokenInfo?> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed class GoogleTokenInfo
{
    public string Email { get; }
    public string Name { get; }
    public string GoogleSubjectId { get; }

    public GoogleTokenInfo(string email, string name, string googleSubjectId)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        GoogleSubjectId = googleSubjectId ?? throw new ArgumentNullException(nameof(googleSubjectId));
    }
}