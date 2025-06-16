namespace Task.PersonDirectory.Infrastructure.Repositories.Segregation;

public interface IDeleteRepository<in T> where T : class
{
    public void Remove(T entity);
}