using System;

namespace FoodHub.Api.Auth.JWT;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, string name);
}