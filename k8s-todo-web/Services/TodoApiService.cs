using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using k8stodo.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace k8stodo.Services
{
    public class TodoApiService : ITodoApiService
    {

        private const string DefaultBaseAddress = "todoapi";
        private const string GetAllTodoItemsUrl = "/api/todo";
        private const string GetTodoItemByIdUrl = "/api/todo/{0}";
        private const string CreateTodoItemUrl = "/api/todo";
        private const string UpdateTodoItemUrl = "/api/todo/{0}";
        private const string DeleteTodoItemUrl = "/api/todo/{0}";

        private readonly IOptions<TodoApiServiceOptions> todoServiceOptions;

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:k8stodo.Services.TodoApiService"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public TodoApiService(IOptions<TodoApiServiceOptions> options)
        {
            todoServiceOptions = options;
            var endpoint = string.IsNullOrWhiteSpace(todoServiceOptions?.Value?.EndpointUri) ?
                          DefaultBaseAddress : todoServiceOptions.Value.EndpointUri;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{endpoint}")
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Creates the todo item asynchronously.
        /// </summary>
        /// <returns>The todo item</returns>
        /// <param name="item">Item.</param>
        public async Task<TodoItem> CreateTodoItemAsync(TodoItem item)
        {
            if (item == null)
            {
                var message = "The item cannot be null.";
                throw new ArgumentNullException(nameof(item), message);
            }
            var json = JsonConvert.SerializeObject(item);
            var postContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(CreateTodoItemUrl, postContent);
            response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
            var todoItem = JsonConvert.DeserializeObject<TodoItem>(json);
            return todoItem;
        }

        /// <summary>
        /// Deletes the todo item async.
        /// </summary>
        /// <returns>The todo item async.</returns>
        /// <param name="id">Identifier.</param>
        public async Task<bool> DeleteTodoItemAsync(int id)
        {
            var response = await _httpClient.DeleteAsync(string.Format(DeleteTodoItemUrl, id));
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the todo item async.
        /// </summary>
        /// <returns>The todo item async.</returns>
        /// <param name="id">Identifier.</param>
        public async Task<TodoItem> GetTodoItemAsync(int id)
        {
            var response = await _httpClient.GetAsync(string.Format(GetTodoItemByIdUrl, id));
            var resultJson = await response.Content.ReadAsStringAsync();
            var todoItem = JsonConvert.DeserializeObject<TodoItem>(resultJson);
            return todoItem;
        }

        /// <summary>
        /// Gets the todo items async.
        /// </summary>
        /// <returns>The todo items async.</returns>
        public async Task<List<TodoItem>> GetTodoItemsAsync()
        {
            var response = await _httpClient.GetAsync(GetAllTodoItemsUrl);
            var result = await response.Content.ReadAsStringAsync();
            var todoItems = JsonConvert.DeserializeObject<List<TodoItem>>(result);
            return todoItems;
        }

        /// <summary>
        /// Updates the todo item async.
        /// </summary>
        /// <returns>The todo item async.</returns>
        /// <param name="item">Item.</param>
        public async Task<bool> UpdateTodoItemAsync(TodoItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            }
            var todoJson = JsonConvert.SerializeObject(item);
            var putContent = new StringContent(todoJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(string.Format(UpdateTodoItemUrl, item.Id), putContent);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
