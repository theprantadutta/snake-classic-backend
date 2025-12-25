using MediatR;
using SnakeClassic.Application.Features.DailyChallenges.DTOs;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.DailyChallenges.Commands.UpdateProgress;

public record UpdateChallengeProgressCommand(
    ChallengeType Type,
    int Value,
    string? GameMode = null
) : IRequest<UpdateChallengeProgressResponse>;
