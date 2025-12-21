namespace SnakeClassic.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string? email = null);
    JwtValidationResult ValidateToken(string token);
}

public class JwtValidationResult
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? ErrorMessage { get; set; }
}
