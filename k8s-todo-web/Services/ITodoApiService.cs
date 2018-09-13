using System.Threading.Tasks;
using System.Collections.Generic;
using k8stodo.Models;

namespace k8stodo.Services
{
    public interface ITodoApiService
    {
        Task<List<TodoItem>> GetTodoItemsAsync();
        Task<TodoItem> GetTodoItemAsync(int id);
        Task<TodoItem> CreateTodoItemAsync(TodoItem item);
        Task<bool> UpdateTodoItemAsync(TodoItem item);
        Task<bool> DeleteTodoItemAsync(int id);
    }
}
