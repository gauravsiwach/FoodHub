using FoodHub.Api.Auth.Google;
using FoodHub.Api.Auth.JWT;
using FoodHub.User.Application.Commands.CreateUser;
using FoodHub.User.Application.Dtos;
using FoodHub.User.Application.Interfaces;
using FoodHub.User.Application.Queries.GetUserByEmail;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace FoodHub.Api.Auth;

[ApiController]
[Route("auth")]
public sealed class GoogleAuthController : ControllerBase
{
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly Serilog.ILogger _logger;

    public GoogleAuthController(
        IGoogleTokenValidator googleTokenValidator,
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        Serilog.ILogger logger)
    {
        _googleTokenValidator = googleTokenValidator ?? throw new ArgumentNullException(nameof(googleTokenValidator));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("google")]
    public async Task<IActionResult> AuthenticateWithGoogle([FromBody] GoogleAuthRequest request, CancellationToken cancellationToken)
    {
        _logger.ForContext<GoogleAuthController>().Information("Begin: Google authentication request");

        try
        {
            // Validate Google ID token
            var googleTokenInfo = await _googleTokenValidator.ValidateTokenAsync(request.GoogleIdToken, cancellationToken);
            if (googleTokenInfo == null)
            {
                _logger.ForContext<GoogleAuthController>().Warning("Failed: Invalid Google ID token");
                return Unauthorized(new { error = "Invalid Google token" });
            }

            // Find or create user
            var getUserQuery = new GetUserByEmailQuery(_userRepository, _logger.ForContext<GetUserByEmailQuery>());
            var existingUser = await getUserQuery.ExecuteAsync(googleTokenInfo.Email, cancellationToken);

            Guid userId;
            if (existingUser != null)
            {
                userId = existingUser.Id;
                _logger.ForContext<GoogleAuthController>().Information("Found existing user {UserId} for email {Email}", userId, googleTokenInfo.Email);
            }
            else
            {
                // Create new user
                var createUserCommand = new CreateUserCommand(_userRepository, _logger.ForContext<CreateUserCommand>());
                var createUserDto = new CreateUserDto(
                    Name: googleTokenInfo.Name,
                    Email: googleTokenInfo.Email,
                    Phone: null); // Google doesn't provide phone by default

                userId = await createUserCommand.ExecuteAsync(createUserDto, cancellationToken);
                _logger.ForContext<GoogleAuthController>().Information("Created new user {UserId} for email {Email}", userId, googleTokenInfo.Email);
            }

            // Generate FoodHub JWT
            var jwtToken = _jwtTokenGenerator.GenerateToken(userId, googleTokenInfo.Email, googleTokenInfo.Name);

            // Return user information with JWT
            var response = new
            {
                userId = userId,
                email = googleTokenInfo.Email,
                name = googleTokenInfo.Name,
                token = jwtToken,
                message = "Authentication successful"
            };

            _logger.ForContext<GoogleAuthController>().Information("Success: Google authentication completed for user {UserId}", userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.ForContext<GoogleAuthController>().Error(ex, "Error: Google authentication failed");
            return StatusCode(500, new { error = "Authentication failed" });
        }
    }
}

public sealed class GoogleAuthRequest
{
    public string GoogleIdToken { get; set; } = string.Empty;
}