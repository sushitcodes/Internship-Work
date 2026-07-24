using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]  // URL: /api/todos
public class TodosController : ControllerBase
{
    private readonly TodoDbContext _context;

    public TodosController(TodoDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Todo>>> GetTodos()
    {
        var todos = await _context.Todos.ToListAsync();
        return Ok(todos);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Todo>> GetTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
        {
            return NotFound($"Todo with ID {id} not found");
        }

        return Ok(todo);
    }

    [HttpPost]
    public async Task<ActionResult<Todo>> PostTodo(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTodo(int id, Todo todo)
    {
        if (id != todo.Id)
        {
            return BadRequest("ID mismatch");
        }

        _context.Entry(todo).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TodoExists(id))
            {
                return NotFound($"Todo with ID {id} not found");
            }
            throw;
        }

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
        {
            return NotFound($"Todo with ID {id} not found");
        }

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> CompleteTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
        {
            return NotFound($"Todo with ID {id} not found");
        }

        todo.IsCompleted = true;
        await _context.SaveChangesAsync();

        return Ok(todo);
    }
    private bool TodoExists(int id)
    {
        return _context.Todos.Any(e => e.Id == id);
    }
}


