using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Serilog;

namespace FoodHub.Api.Auth.Google;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthOptions _options;

    public GoogleTokenValidator(IOptions<GoogleAuthOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<GoogleTokenInfo?> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            Log.Warning("Google token validation failed: empty or null token provided");
            return null;
        }

        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            Log.Error("Google token validation failed: ClientId not configured");
            return null;
        }

        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _options.ClientId, _options.Aud },
                IssuedAtClockTolerance = TimeSpan.FromMinutes(5),
                ExpirationTimeClockTolerance = TimeSpan.FromMinutes(5)
            };

            Log.Information("Validating Google ID token for audience {ClientId}", _options.ClientId);

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            // Verify issuer explicitly
            if (payload.Issuer != "https://accounts.google.com" && payload.Issuer != "accounts.google.com")
            {
                Log.Error("Google token validation failed: invalid issuer {Issuer}", payload.Issuer);
                return null;
            }

            Log.Information("Successfully validated Google token for email {Email} from issuer {Issuer}", payload.Email, payload.Issuer);

            return new GoogleTokenInfo(
                email: payload.Email,
                name: payload.Name,
                googleSubjectId: payload.Subject);
        }
        catch (InvalidJwtException ex)
        {
            Log.Error(ex, "Google token validation failed: Invalid JWT - {Message}", ex.Message);
            return null;
        }
        catch (ArgumentException ex)
        {
            Log.Error(ex, "Google token validation failed: Invalid argument - {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Google token validation failed: Unexpected error - {Message}", ex.Message);
            return null;
        }
    }
}

public sealed class GoogleAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string Aud { get; set; } = string.Empty;
}