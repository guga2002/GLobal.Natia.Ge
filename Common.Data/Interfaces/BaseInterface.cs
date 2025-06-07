namespace Common.Data.Interfaces;

public interface BaseInterface<T> where T : class
{
    Task Add(T item);
    Task Remove(int id);
    Task<T> GetById(int id);
    Task<IEnumerable<T>> GetAll();
    Task Update(T item);
}
