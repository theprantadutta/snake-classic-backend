using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Auth.DTOs;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Auth.Commands.AuthenticateWithFirebase;

public class AuthenticateWithFirebaseCommandHandler
    : IRequestHandler<AuthenticateWithFirebaseCommand, Result<AuthResponse>>
{
    private readonly IAppDbContext _context;
    private readonly IFirebaseAuthService _firebaseAuth;
    private readonly IJwtService _jwtService;
    private readonly IDateTimeService _dateTime;

    public AuthenticateWithFirebaseCommandHandler(
        IAppDbContext context,
        IFirebaseAuthService firebaseAuth,
        IJwtService jwtService,
        IDateTimeService dateTime)
    {
        _context = context;
        _firebaseAuth = firebaseAuth;
        _jwtService = jwtService;
        _dateTime = dateTime;
    }

    public async Task<Result<AuthResponse>> Handle(
        AuthenticateWithFirebaseCommand request,
        CancellationToken cancellationToken)
    {
        // Verify Firebase token
        FirebaseUserInfo firebaseUser;
        try
        {
            firebaseUser = await _firebaseAuth.VerifyIdTokenAsync(request.IdToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<AuthResponse>.Unauthorized(ex.Message);
        }

        // Find or create user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUser.FirebaseUid, cancellationToken);

        bool isNewUser = user == null;

        if (isNewUser)
        {
            user = new User
            {
                FirebaseUid = firebaseUser.FirebaseUid,
                Email = firebaseUser.Email,
                DisplayName = firebaseUser.DisplayName,
                PhotoUrl = firebaseUser.PhotoUrl,
                AuthProvider = Enum.TryParse<AuthProvider>(firebaseUser.AuthProvider, true, out var provider)
                    ? provider : AuthProvider.Google,
                IsAnonymous = firebaseUser.IsAnonymous,
                Status = UserStatus.Online,
                LastActiveAt = _dateTime.UtcNow
            };

            _context.Users.Add(user);

            // Create default preferences
            var preferences = new UserPreferences { User = user };
            _context.UserPreferences.Add(preferences);

            // Create premium content record
            var premiumContent = new UserPremiumContent { User = user };
            _context.UserPremiumContents.Add(premiumContent);

            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Update last active
            user!.LastActiveAt = _dateTime.UtcNow;
            user.Status = UserStatus.Online;

            // Update profile info if changed
            if (!string.IsNullOrEmpty(firebaseUser.Email) && user.Email != firebaseUser.Email)
                user.Email = firebaseUser.Email;
            if (!string.IsNullOrEmpty(firebaseUser.DisplayName) && user.DisplayName != firebaseUser.DisplayName)
                user.DisplayName = firebaseUser.DisplayName;
            if (!string.IsNullOrEmpty(firebaseUser.PhotoUrl) && user.PhotoUrl != firebaseUser.PhotoUrl)
                user.PhotoUrl = firebaseUser.PhotoUrl;

            await _context.SaveChangesAsync(cancellationToken);
        }

        // Generate JWT
        var accessToken = _jwtService.GenerateToken(user.Id, user.Email);

        return Result<AuthResponse>.Success(new AuthResponse(
            UserId: user.Id,
            AccessToken: accessToken,
            Email: user.Email,
            Username: user.Username,
            DisplayName: user.DisplayName,
            PhotoUrl: user.PhotoUrl,
            IsNewUser: isNewUser
        ));
    }
}
