using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnakeClassic.Application.Common.Interfaces;

namespace SnakeClassic.Infrastructure.Services;

public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly ILogger<FirebaseAuthService> _logger;

    public FirebaseAuthService(IConfiguration configuration, ILogger<FirebaseAuthService> logger)
    {
        _logger = logger;

        if (FirebaseApp.DefaultInstance == null)
        {
            var credentialsPath = configuration["Firebase:CredentialsPath"]
                ?? Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")
                ?? "firebase-admin-sdk.json";

            try
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credentialsPath)
                });
                _logger.LogInformation("Firebase Admin SDK initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Admin SDK from path: {Path}", credentialsPath);
                throw;
            }
        }
    }

    public async Task<FirebaseUserInfo> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

            // Extract sign-in provider
            var signInProvider = "unknown";
            if (decodedToken.Claims.TryGetValue("firebase", out var firebaseClaim))
            {
                if (firebaseClaim is Dictionary<string, object> firebaseDict)
                {
                    if (firebaseDict.TryGetValue("sign_in_provider", out var provider))
                    {
                        signInProvider = provider?.ToString() ?? "unknown";
                    }
                }
            }

            var authProvider = signInProvider switch
            {
                "google.com" => "google",
                "anonymous" => "anonymous",
                _ => "firebase"
            };

            return new FirebaseUserInfo
            {
                FirebaseUid = decodedToken.Uid,
                Email = decodedToken.Claims.TryGetValue("email", out var email) ? email?.ToString() : null,
                DisplayName = decodedToken.Claims.TryGetValue("name", out var name) ? name?.ToString() : null,
                PhotoUrl = decodedToken.Claims.TryGetValue("picture", out var picture) ? picture?.ToString() : null,
                AuthProvider = authProvider,
                IsAnonymous = signInProvider == "anonymous"
            };
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogWarning(ex, "Failed to verify Firebase token");
            throw new UnauthorizedAccessException("Invalid Firebase token", ex);
        }
    }
}
