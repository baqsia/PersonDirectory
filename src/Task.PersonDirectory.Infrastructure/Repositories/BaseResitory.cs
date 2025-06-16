using Microsoft.EntityFrameworkCore;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Infrastructure.Repositories;

public class BaseRepository<TEntity> where TEntity : class
{
    protected IQueryable<TEntity> ApplySpecification(DbSet<TEntity> set, ISpecification<TEntity> spec)
    {
        var query = set.AsQueryable();

        if (spec.Criteria is not null)
            query = query.Where(spec.Criteria);

        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (spec.OrderBy is not null)
            query = spec.OrderBy(query);

        return query;
    }
}