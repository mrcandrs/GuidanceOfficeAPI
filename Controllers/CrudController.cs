using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuidanceOfficeAPI.Data;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class CrudController<T> : ControllerBase where T : class
    {
        private readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public CrudController(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<T>>> GetAll() => await _dbSet.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<T>> Get(int id)
        {
            var item = await _dbSet.FindAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<T>> Create(T item)
        {
            _dbSet.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, T item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _dbSet.FindAsync(id);
            if (item == null) return NotFound();
            _dbSet.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
