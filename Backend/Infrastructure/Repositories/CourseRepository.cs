
using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class CourseRepository : Repository, ICourseRepository
{
    public CourseRepository(IDatabaseContext dbContext, IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, domainEventDispatcher)
    {
    }
}
