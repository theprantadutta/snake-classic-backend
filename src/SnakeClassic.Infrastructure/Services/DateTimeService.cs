using SnakeClassic.Application.Common.Interfaces;

namespace SnakeClassic.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
