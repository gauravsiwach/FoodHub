# Prompt 5: Authentication System (Google OAuth + JWT)
## Overview
Implement two-token authentication: Google OAuth for identity verification  FoodHub JWT for API access.
## Components Required
### 1. Google Token Validator
- Create Auth/Google/IGoogleTokenValidator.cs interface
- Implement GoogleTokenValidator using Google.Apis.Auth
- Validate Google ID token with Google servers
- Return GoogleTokenInfo (Email, Name, GoogleSubjectId)
### 2. JWT Token Generator
- Create Auth/JWT/IJwtTokenGenerator.cs interface
- Implement JwtTokenGenerator using System.IdentityModel.Tokens.Jwt
- Claims: UserId (sub), Email, Name, JTI, IssuedAt
- HMAC SHA256 signing with secret key
- 60-minute expiry (configurable)
### 3. Google Auth Controller
- Route: POST /auth/google
- Input: GoogleIdToken
- Process: Validate  Find/Create User  Generate JWT
- Return: { userId, email, name, token, message }
### 4. JWT Authentication Middleware
- Configure JwtBearer authentication
- Validate: Issuer, Audience, Lifetime, Signature
- Auto-populate HttpContext.User with claims
### 5. GraphQL Authorization
- Add [Authorize] attribute to all Query/Mutation classes
- Enforced by UseAuthorization() middleware
## Configuration (appsettings.json)
`json
\"GoogleAuth\": {
  \"ClientId\": \"your-google-client-id.apps.googleusercontent.com\",
  \"Aud\": \"your-google-aud\"
},
\"Jwt\": {
  \"Secret\": \"Your_Super_Secret_32_Character_Key_Here\",
  \"Issuer\": \"FoodHub\",
  \"Audience\": \"FoodHub\",
  \"ExpiryMinutes\": 60
}
`
## Program.cs Registration
1. services.Configure<GoogleAuthOptions>()
2. services.AddScoped<IGoogleTokenValidator>()
3. services.AddScoped<IJwtTokenGenerator>()
4. services.AddAuthentication(JwtBearerDefaults)
5. services.AddAuthorization()
6. services.AddControllers()
7. app.UseAuthentication()
8. app.UseAuthorization()
9. app.MapControllers()
## Success Criteria
- Google token validation works
- JWT generation successful
- All GraphQL endpoints require authentication
- Unauthorized requests return 401
