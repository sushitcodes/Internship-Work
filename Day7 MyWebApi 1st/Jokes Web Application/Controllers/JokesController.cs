
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class JokesController : Controller
{
    private readonly NewDbContext _context;

    public JokesController(NewDbContext context)
    {
        _context = context;
    }

    // GET: JOKES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Joke.ToListAsync());
    }

    // GET: JOKES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var joke = await _context.Joke
            .FirstOrDefaultAsync(m => m.Id == id);
        if (joke == null)
        {
            return NotFound();
        }

        return View(joke);
    }

    // GET: JOKES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: JOKES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,JokeQuestion,JokeAnswer")] Joke joke)
    {
        if (ModelState.IsValid)
        {
            _context.Add(joke);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(joke);
    }

    // GET: JOKES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var joke = await _context.Joke.FindAsync(id);
        if (joke == null)
        {
            return NotFound();
        }
        return View(joke);
    }

    // POST: JOKES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,JokeQuestion,JokeAnswer")] Joke joke)
    {
        if (id != joke.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(joke);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JokeExists(joke.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(joke);
    }

    // GET: JOKES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var joke = await _context.Joke
            .FirstOrDefaultAsync(m => m.Id == id);
        if (joke == null)
        {
            return NotFound();
        }

        return View(joke);
    }

    // POST: JOKES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var joke = await _context.Joke.FindAsync(id);
        if (joke != null)
        {
            _context.Joke.Remove(joke);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool JokeExists(int? id)
    {
        return _context.Joke.Any(e => e.Id == id);
    }
}
