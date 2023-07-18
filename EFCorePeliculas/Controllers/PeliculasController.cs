using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFCorePeliculas.DTOs;
using EFCorePeliculas.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCorePeliculas.Controllers
{
    [ApiController]
    [Route("api/peliculas")]
    public class PeliculasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public PeliculasController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PeliculaDTO>> Get(int id)
        {
            var pelicula = await _context.Peliculas
                .Include(p => p.Generos.OrderByDescending(g => g.Nombre))
                .Include(p => p.SalasDeCines)
                    .ThenInclude(s => s.Cine)
                .Include(p => p.PeliculasActores.Where(pa => pa.Actor.FechaNacimiento.Value.Year >= 1980))
                    .ThenInclude(pa => pa.Actor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (pelicula is null)
            {

                return NotFound();

            }

            //Esta manera de usar el MAP es difirente cuando no se usa el ProjectTo
            //A diferencia de como lo usamos en AutoresController

            var peliculaDTO = _mapper.Map<PeliculaDTO>(pelicula);

            peliculaDTO.Cines = peliculaDTO.Cines.DistinctBy(x => x.Id).ToList();


            return peliculaDTO;
        }
        [HttpGet("filtrar")]
        public async Task<ActionResult<List<PeliculaDTO>>> Filtrar([FromQuery] PeliculasFiltroDTO peliculasFiltroDTO)
        {
            var peliculasQueryable = _context.Peliculas.AsQueryable();
            if (!string.IsNullOrEmpty(peliculasFiltroDTO.Titulo))
            {
                peliculasQueryable = peliculasQueryable.Where(p => p.Titulo.Contains(peliculasFiltroDTO.Titulo));
            }

            if (peliculasFiltroDTO.EnCartelera)
            {
                peliculasQueryable = peliculasQueryable.Where(p => p.EnCartelera);
            }

            if (peliculasFiltroDTO.ProximosEstrenos)
            {
                var hoy = DateTime.Today;
                peliculasQueryable = peliculasQueryable.Where(p => p.FechaEstreno > hoy);

            }

            if (peliculasFiltroDTO.GeneroId != 0)
            {
                peliculasQueryable = peliculasQueryable.Where(p =>
                    p.Generos.Select(g => g.Id)
                             .Contains(peliculasFiltroDTO.GeneroId));

            }

            var peliculas = await peliculasQueryable.Include(p => p.Generos).ToListAsync();
            return _mapper.Map<List<PeliculaDTO>>(peliculas);
        }

        [HttpPost]
         public async Task<ActionResult> Post(PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = _mapper.Map<Pelicula>(peliculaCreacionDTO);
            pelicula.Generos.ForEach(g => _context.Entry(g).State = EntityState.Unchanged);
            pelicula.SalasDeCines.ForEach(s => _context.Entry(s).State = EntityState.Unchanged);

            if (pelicula.PeliculasActores is not null)
            {
                for( int i = 0; i < pelicula.PeliculasActores.Count; i++ )
                {
                    pelicula.PeliculasActores[i].Orden = i + 1;
                }
                
            }

            _context.Add(pelicula);
            await _context.SaveChangesAsync();
            return Ok();
        }
        //65464
        [HttpGet("conprojectto/{id:int}")]
        public async Task<ActionResult<PeliculaDTO>> GetProjectTo(int id)
        {
            var pelicula = await _context.Peliculas
                .ProjectTo<PeliculaDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (pelicula is null)
            {

                return NotFound();

            }

            //Esta manera de usar el MAP es difirente cuando no se usa el ProjectTo
            //A diferencia de como lo usamos en AutoresController


            pelicula.Cines = pelicula.Cines.DistinctBy(x => x.Id).ToList();


            return pelicula;
        }

        [HttpGet("cargadoselectivo/{id:int}")]
        public async Task<ActionResult> GetSelectivo(int id)
        {
            var pelicula = await _context.Peliculas.Select(p =>
            new
            {
                Id = p.Id,
                Titulo = p.Titulo,
                Generos = p.Generos.OrderByDescending(g => g.Nombre).Select(g => g.Nombre).ToList(),
                CantidadActores = p.PeliculasActores.Count(),
                CantidadCines = p.SalasDeCines.Select(s => s.CineId).Distinct().Count(),

            }).FirstOrDefaultAsync(p => p.Id == id);

            if (pelicula is null)
            {
                return NotFound();
            }

            return Ok(pelicula);
        }



        [HttpGet("cargadoexplicito/{id:int}")]
        public async Task<ActionResult<PeliculaDTO>> GetExplicito(int id)
        {
            var pelicula = await _context.Peliculas.AsTracking().FirstOrDefaultAsync(p => p.Id == id);
            //..

            // await _context.Entry(pelicula).Collection(p => p.Generos).LoadAsync();

            var cantidadGeneros = await _context.Entry(pelicula).Collection(p => p.Generos).Query().CountAsync();

            if (pelicula is null)
            {
                return NotFound();

            }

            var peliculaDTO = _mapper.Map<PeliculaDTO>(pelicula);
            return peliculaDTO;
        }


        [HttpGet("lazyloading/{id:int}")]
        public async Task<ActionResult<PeliculaDTO>> GetLazyLoading(int id)
        {
            var pelicula = await _context.Peliculas.AsTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (pelicula == null)
            {
                return NotFound();

            }

            var peliculaDTO = _mapper.Map<PeliculaDTO>(pelicula);
            peliculaDTO.Cines = peliculaDTO.Cines.DistinctBy(x => x.Id).ToList();
            return peliculaDTO;
        }

        [HttpGet("agrupadasporestreno")]
        public async Task<ActionResult<PeliculaDTO>> GetAgrupadasPorCartelera()
        {
            var peliculasAgrupadas = await _context.Peliculas.GroupBy(p => p.EnCartelera)
                                            .Select(g => new
                                            {
                                                EnCartelera = g.Key,
                                                Conteo = g.Count(),
                                                Peliculas = g.ToList(),
                                            }).ToListAsync();

            return Ok(peliculasAgrupadas);
        }

        [HttpGet("agrupadaPorCantidadGeneros")]
        public async Task<ActionResult> GetAgrupadasPorCantidadDeGeneros()
        {
            var peliculasAgrupadas = await _context.Peliculas.GroupBy(p => p.Generos.Count())
                                            .Select(g => new
                                            {
                                                Conteo = g.Key,
                                                Titulos = g.Select(x => x.Titulo),
                                                Generos = g.Select(p => p.Generos).SelectMany(gen => gen).Select(gen => gen.Nombre).Distinct()
                                            }).ToListAsync();
            return Ok(peliculasAgrupadas);
        }


    }
}