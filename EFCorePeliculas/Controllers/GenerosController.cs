using EFCorePeliculas.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCorePeliculas.Controllers
{

    [ApiController]
    [Route("api/generos")]
    public class GenerosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GenerosController(ApplicationDbContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Genero>> Get()
        {
            _context.Logs.Add(new Log 
            {
                Id = Guid.NewGuid(),
                Mensaje = "Ejecutando guid"
            });
            await _context.SaveChangesAsync();

            return await _context.Generos.OrderBy(g => g.Nombre).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Genero>> Get(int id)
        {
            var genero = await _context.Generos.FirstOrDefaultAsync(g => g.Id == id);

            if (genero is null)
            {
                return NotFound();
            }

            return genero;
        }

        [HttpPost]
        public async Task<ActionResult>Post(Genero genero)
        {

            var existeGeneroConNombre = await _context.Generos.AnyAsync(g => g.Nombre == genero.Nombre);

            if (existeGeneroConNombre)
            {
                return BadRequest("Ya existe un genero con ese nombre: " + genero.Nombre);
            }

            _context.Add(genero);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("varios")]
        public async Task<ActionResult> Post(Genero[] generos)
        {
            _context.AddRange(generos);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("agregar2")]
        public async Task<ActionResult> Agregar2(int id)
        {
            var genero = await _context.Generos.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);

            if (genero is null)
            {
                return NotFound();
            }

            genero.Nombre += "2";
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var genero = await _context.Generos.FirstOrDefaultAsync(g => g.Id==id);

            if (genero is null)
            {
                return NotFound();
                
            }

            _context.Remove(genero);
            await _context.SaveChangesAsync();
            return Ok();

        }

        [HttpDelete("borradosuave/{id:int}")]
        public async Task<ActionResult> DeleteSuave(int id)
        {
            var genero = await _context.Generos.AsTracking().FirstOrDefaultAsync(g => g.Id == id);

            if (genero is null)
            {
                return NotFound();

            }

            genero.EstaBorrado = true;
            await _context.SaveChangesAsync();
            return Ok();

        }

        [HttpPost("restaurar/{id:int}")]
        public async Task<ActionResult> Restaurar(int id)
        {
            var genero = await _context.Generos.AsTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genero is null)
            {
                return NotFound();

            }

            genero.EstaBorrado = false;
            await _context.SaveChangesAsync();
            return Ok();

        }

        //[HttpGet("primer")]
        //public async Task<ActionResult<Genero>> Primer()
        //{
        //    //return await _context.Generos.FirstAsync(g => g.Nombre.StartsWith("Z"));

        //    var genero =  await _context.Generos.FirstOrDefaultAsync(g => g.Nombre.StartsWith("C"));

        //    if (genero is null)
        //    {
        //        return NotFound();
        //    }

        //    return genero;

        //}

        //[HttpGet("filtrar")]

        //public async Task<IEnumerable<Genero>> Filtrar(string nombre)
        //{
        //    return await _context.Generos
        //        .Where(g => g.Nombre.Contains(nombre))
        //        //.OrderByDescending(g => g.Nombre) //Tambien puede ser la funcion para ordenar .OrderBy

        //        .ToListAsync();
        //}
        //// g.Nombre.StartsWith("c") || g.Nombre.StartsWith("a")

        //[HttpGet("paginacion")]
        //public async Task<ActionResult<IEnumerable<Genero>>> GetPaginacion(int pagina = 1) 
        //{
        //    var cantidadRegistrosPagina = 2;
        //    var generos = await _context.Generos
        //        .Skip((pagina-1)*cantidadRegistrosPagina)
        //        .Take(cantidadRegistrosPagina)
        //        .ToListAsync();
        //    return generos;
        //}
    }
}
