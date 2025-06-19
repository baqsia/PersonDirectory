namespace Task.PersonDirectory.Infrastructure.Specifications;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

public class IncludeBuilder<TEntity> where TEntity : class
{
    private Func<IQueryable<TEntity>, IQueryable<TEntity>>? _applyIncludes;

    public IncludeBuilder<TEntity> Include<TProperty>(
        Expression<Func<TEntity, TProperty>> include)
    {
        _applyIncludes = q => q.Include(include);
        return this;
    }

    public IncludeBuilder<TEntity> ThenInclude<TPreviousProperty, TProperty>(
        Expression<Func<TPreviousProperty, TProperty>> thenInclude)
    {
        if (_applyIncludes is null)
            throw new InvalidOperationException("ThenInclude must follow an Include.");

        var previousApply = _applyIncludes;
        _applyIncludes = q =>
        {
            var includable = (IIncludableQueryable<TEntity, TPreviousProperty>)previousApply(q);
            return includable.ThenInclude(thenInclude);
        };

        return this;
    } 
    
    public IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query)
    {
        return _applyIncludes is not null ? _applyIncludes(query) : query;
    }
}