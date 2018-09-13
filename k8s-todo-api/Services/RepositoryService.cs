using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using k8stodoapi.Models;

namespace k8stodoapi.Services
{
    public class RepositoryService : IRepositoryService<TodoItem>
    {

        List<TodoItem> myTodoList = new List<TodoItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:k8stodoapi.Services.RepositoryService"/> class.
        /// </summary>
        public RepositoryService()
        {
            myTodoList.Add(
                new TodoItem
                {
                    Id = 1,
                    Name = "First ToDo",
                    DueDate = DateTime.Now
                });
            myTodoList.Add(new TodoItem
            {
                Id = 2,
                Name = "Another ToDo",
                DueDate = DateTime.Now
            });
            myTodoList.Add(new TodoItem
            {
                Id = 3,
                Name = "Yet Another ToDo",
                DueDate = DateTime.Now
            });
            myTodoList.Add(new TodoItem
            {
                Id = 4,
                Name = "Yet Another ToDo",
                DueDate = DateTime.Now
            });
        }

        /// <summary>
        /// Adds the todo item.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="entity">Entity.</param>
        public Task AddAsync(TodoItem entity)
        {
            return Task.Run(() =>
            {
                if (entity != null)
                {
                    myTodoList.Add(entity);
                }
            });
        }

        /// <summary>
        /// Deletes the todo item
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="id">Identifier.</param>
        public Task DeleteAsync(int id)
        {
            return Task.Run(() =>
            {
                myTodoList.RemoveAll(todo => todo.Id == id);
            });
        }

        /// <summary>
        /// Gets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        public Task<List<TodoItem>> GetAllAsync()
        {
            return Task.FromResult<List<TodoItem>>(myTodoList);
        }

        /// <summary>
        /// Gets by identifier async.
        /// </summary>
        /// <returns>The by identifier async.</returns>
        /// <param name="id">Identifier.</param>
        public Task<TodoItem> GetByIdAsync(int id)
        {
            var result = myTodoList.Find(todo => todo.Id == id);
            return Task.FromResult<TodoItem>(result);
        }

        /// <summary>
        /// Updates the todo item.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="entity">Entity.</param>
        public Task UpdateAsync(TodoItem entity)
        {
            return Task.Run(() =>
            {
                var result = myTodoList.Find(todo => todo.Id == entity.Id);
                if (result != null)
                {
                    result.Name = entity.Name;
                    result.IsCompleted = entity.IsCompleted;
                    result.CompletedDate = entity.CompletedDate;
                };
            });
        }
    }
}
