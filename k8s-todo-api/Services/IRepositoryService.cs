using System.Collections.Generic;
using System.Threading.Tasks;

namespace k8stodoapi.Services
{
    public interface IRepositoryService<T> where T : new()
    {
        Task<T> GetByIdAsync(int id);

        Task<List<T>> GetAllAsync();

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(int id);
    }
}
