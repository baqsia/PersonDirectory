using System.Linq.Expressions;

namespace Task.PersonDirectory.Infrastructure.Specifications;

public interface ISpecification<T> where T : class
{
    public Expression<Func<T, bool>>? Criteria { get; }
    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    public List<string> IncludeStrings { get; }
}

public abstract class Specification<T> : ISpecification<T> where T : class
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; protected set; } = [];
    public List<string> IncludeStrings { get; } = [];
    
    protected void SetCriteria(Expression<Func<T, bool>> criteria)
        => Criteria = criteria;

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
        => Includes.Add(includeExpression);
    
    protected void AddInclude(string includeString)
        => IncludeStrings.Add(includeString);
    
    protected void SetOrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        => OrderBy = orderBy;
}