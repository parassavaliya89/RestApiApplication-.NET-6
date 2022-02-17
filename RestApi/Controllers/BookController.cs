using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestApi.Data;
using RestApi.Models;

namespace RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly DataContext _context;

        public BookController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Book>>> GetAllBook()
        {
            return Ok(await _context.bookes.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await _context.bookes.FindAsync(id);
            if (book == null)
                return BadRequest("Book not found.");
            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<List<Book>>> AddBook(Book hero)
        {
            _context.bookes.Add(hero);
            await _context.SaveChangesAsync();

            return Ok(await _context.bookes.ToListAsync());
        }

        [HttpPut]
        public async Task<ActionResult<List<Book>>> UpdateBook(Book request)
        {
            var dbBook = await _context.bookes.FindAsync(request.Id);
            if (dbBook == null)
                return BadRequest("Book not found.");

            dbBook.Name = request.Name;
            dbBook.Title = request.Title;
            dbBook.Price = request.Price;
            

            await _context.SaveChangesAsync();

            return Ok(await _context.bookes.ToListAsync());
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBook(int id,
            [FromBody] JsonPatchDocument<Book> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest("Null values");

            var book = _context.bookes.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            patchDoc.ApplyTo(book, ModelState);

            TryValidateModel(book);

            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            _context.SaveChanges();
            return Ok(book);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<List<Book>>> DeleteBook(int id)
        {
            var dbBook = await _context.bookes.FindAsync(id);
            if (dbBook == null)
                return BadRequest("Book not found.");

            _context.bookes.Remove(dbBook);
            await _context.SaveChangesAsync();

            return Ok(await _context.bookes.ToListAsync());
        }

    }
}
