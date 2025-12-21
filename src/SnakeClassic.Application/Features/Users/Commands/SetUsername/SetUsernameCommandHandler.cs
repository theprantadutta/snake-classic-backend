using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;

namespace SnakeClassic.Application.Features.Users.Commands.SetUsername;

public class SetUsernameCommandHandler : IRequestHandler<SetUsernameCommand, Result<SetUsernameResponse>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetUsernameCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SetUsernameResponse>> Handle(SetUsernameCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<SetUsernameResponse>.Unauthorized();
        }

        var username = request.Username.Trim().ToLowerInvariant();

        // Check if username is already taken
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username != null &&
                u.Username.ToLower() == username &&
                u.Id != _currentUser.UserId.Value, cancellationToken);

        if (existingUser != null)
        {
            return Result<SetUsernameResponse>.Conflict("Username is already taken");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);

        if (user == null)
        {
            return Result<SetUsernameResponse>.NotFound("User not found");
        }

        user.Username = username;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<SetUsernameResponse>.Success(new SetUsernameResponse(true, username));
    }
}
