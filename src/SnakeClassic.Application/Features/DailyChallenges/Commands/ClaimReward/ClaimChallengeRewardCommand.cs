using MediatR;
using SnakeClassic.Application.Features.DailyChallenges.DTOs;

namespace SnakeClassic.Application.Features.DailyChallenges.Commands.ClaimReward;

public record ClaimChallengeRewardCommand(
    Guid ChallengeId
) : IRequest<ClaimRewardResponse>;
