using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFCorePeliculas.DTOs;
using EFCorePeliculas.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCorePeliculas.Controllers
{
    [ApiController]
    [Route("api/actores")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public AutoresController(ApplicationDbContext context , IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ActorDTO>> Get()
        {
            return await _context.Actores
                .ProjectTo<ActorDTO>(_mapper.ConfigurationProvider).ToListAsync();
                

                //Eta linea se esta sustituyendo ya que estamos usando auto mapper
                //.Select( a => new ActorDTO {Id = a.Id,Nombre = a.Nombre}).ToListAsync();
         
        }
        [HttpPost]

        public async Task<ActionResult> Post (ActorCreacionDTO actorCreacionDTO)
        {
            var actor = _mapper.Map<Actor>(actorCreacionDTO);
            _context.Add(actor);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(ActorCreacionDTO actorCreacionDTO,int id)
        {
            var actorDB = await _context.Actores.AsTracking().FirstOrDefaultAsync(a => a.Id == id);

            if (actorDB is null)
            {
                return NotFound();
                
            }

            actorDB = _mapper.Map(actorCreacionDTO, actorDB);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPut("desconectado/{id:int}")]
        public async Task<ActionResult> PutDesconectado(ActorCreacionDTO actorCreacionDTO, int id)
        {
            var existeActor = await _context.Actores.AnyAsync( a => a.Id == id);
            if (!existeActor)
            {
                return NotFound();
                

            }

            var actor = _mapper.Map<Actor>(actorCreacionDTO);
            actor.Id = id;
            await _context.SaveChangesAsync();
            return Ok();
        }


    }
}
