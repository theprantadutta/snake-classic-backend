namespace SnakeClassic.Application.Common.Interfaces;

public interface IFirebaseAuthService
{
    Task<FirebaseUserInfo> VerifyIdTokenAsync(string idToken);
}

public class FirebaseUserInfo
{
    public string FirebaseUid { get; set; } = null!;
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? PhotoUrl { get; set; }
    public string AuthProvider { get; set; } = "google";
    public bool IsAnonymous { get; set; }
}
