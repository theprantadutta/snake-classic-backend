using MediatR;
using SnakeClassic.Application.Features.DailyChallenges.DTOs;

namespace SnakeClassic.Application.Features.DailyChallenges.Queries.GetTodaysChallenges;

public record GetTodaysChallengesQuery() : IRequest<DailyChallengesResponse>;
