using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8stodoapi.Models;
using k8stodoapi.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace k8stodoapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class TodoController : Controller
    {
        IRepositoryService<TodoItem> todoRepositoryService;
        public TodoController(IRepositoryService<TodoItem> repositoryService)
        {
            todoRepositoryService = repositoryService;
        }

        // GET: api/todo/getall
        [HttpGet]
        [ProducesResponseType(typeof(TodoItem), 200)]
        public async Task<IActionResult> GetAll()
        {
            var myTodoItems = await todoRepositoryService.GetAllAsync();
            return new ObjectResult(myTodoItems);
        }

        [HttpGet("{id}", Name = "GetTodo")]
        [ProducesResponseType(typeof(TodoItem), 200)]
        [ProducesResponseType(typeof(TodoItem), 404)]
        public async Task<IActionResult> Get(int id)
        {
            var myTodoItem = await todoRepositoryService.GetByIdAsync(id);
            if (myTodoItem == null)
            {
                return NotFound();
            }
            else
            {
                return new ObjectResult(myTodoItem);
            }

        }

        // POST api/Add
        [HttpPost]
        [ProducesResponseType(typeof(TodoItem), 201)]
        [ProducesResponseType(typeof(TodoItem), 400)]
        public async Task<IActionResult> Add([FromBody]TodoItem newItem)
        {
            if (newItem != null)
            {
                await todoRepositoryService.AddAsync(newItem);
                return CreatedAtRoute("GetTodo", new { id = newItem.Id }, newItem);
            }
            else
            {
                return BadRequest();
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TodoItem), 204)]
        [ProducesResponseType(typeof(TodoItem), 404)]
        public async Task<IActionResult> Update(int id, [FromBody]TodoItem todoItem)
        {
            if (todoItem == null || todoItem.Id != id)
            {
                return BadRequest();
            }
            await todoRepositoryService.UpdateAsync(todoItem);
            return new NoContentResult();
        }

        // DELETE api/Delete
        [HttpDelete]
        [ProducesResponseType(typeof(TodoItem), 204)]
        public async Task<IActionResult> Delete(int id)
        {
            await todoRepositoryService.DeleteAsync(id);
            return new NoContentResult();
        }
    }
}
