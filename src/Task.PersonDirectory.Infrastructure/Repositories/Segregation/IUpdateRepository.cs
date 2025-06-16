namespace Task.PersonDirectory.Infrastructure.Repositories.Segregation;

public interface IUpdateRepository<T> where T : class
{
    void Update(T entity);
}