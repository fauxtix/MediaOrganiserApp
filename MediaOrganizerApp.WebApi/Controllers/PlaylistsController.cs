using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistsController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;
        private readonly IPlaylistItemService _playlistItemService;
        private readonly IMapper _mapper;

        public PlaylistsController(
            IPlaylistService playlistService,
            IPlaylistItemService playlistItemService,
            IMapper mapper)
        {
            _playlistService = playlistService;
            _playlistItemService = playlistItemService;
            _mapper = mapper;
        }

        // --------------------------------------------------------
        // PLAYLISTS
        // --------------------------------------------------------

        // GET: api/playlists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlaylistDto>>> GetAll(CancellationToken cancellationToken)
        {
            var entities = await _playlistService.GetAllAsync(cancellationToken);
            var dtos = _mapper.Map<IEnumerable<PlaylistDto>>(entities);
            return Ok(dtos);
        }

        // GET: api/playlists/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PlaylistDetailsDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var playlist = await _playlistService.GetByIdAsync(id, cancellationToken);
            if (playlist == null)
                return NotFound();

            var items = await _playlistItemService.GetByPlaylistIdAsync(id, cancellationToken);

            var detailsDto = _mapper.Map<PlaylistDetailsDto>(playlist);
            detailsDto.Items = _mapper.Map<List<PlaylistItemDto>>(items);

            return Ok(detailsDto);
        }

        // POST: api/playlists
        [HttpPost]
        public async Task<ActionResult<PlaylistDto>> Create(
            [FromBody] PlaylistCreateDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<Playlist>(dto);
            var created = await _playlistService.CreateAsync(entity, cancellationToken);
            var resultDto = _mapper.Map<PlaylistDto>(created);

            // devolve 201 Created com Location header
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        // PUT: api/playlists/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] PlaylistUpdateDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _playlistService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound();

            // Mapear DTO para a entidade existente
            _mapper.Map(dto, existing);

            await _playlistService.UpdateAsync(existing, cancellationToken);

            return NoContent();
        }

        // DELETE: api/playlists/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var existing = await _playlistService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound();

            await _playlistService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }

        // --------------------------------------------------------
        // PLAYLIST ITEMS (rotas aninhadas)
        // --------------------------------------------------------

        // GET: api/playlists/{playlistId}/items
        [HttpGet("{playlistId:int}/items")]
        public async Task<ActionResult<IEnumerable<PlaylistItemDto>>> GetItems(
            int playlistId,
            CancellationToken cancellationToken)
        {
            // opcional: validar se a playlist existe
            var playlist = await _playlistService.GetByIdAsync(playlistId, cancellationToken);
            if (playlist == null)
                return NotFound($"Playlist with id {playlistId} not found.");

            var items = await _playlistItemService.GetByPlaylistIdAsync(playlistId, cancellationToken);
            var dtos = _mapper.Map<IEnumerable<PlaylistItemDto>>(items);

            return Ok(dtos);
        }

        // GET: api/playlists/{playlistId}/items/{itemId}
        [HttpGet("{playlistId:int}/items/{itemId:int}")]
        public async Task<ActionResult<PlaylistItemDto>> GetItemById(
            int playlistId,
            int itemId,
            CancellationToken cancellationToken)
        {
            var playlist = await _playlistService.GetByIdAsync(playlistId, cancellationToken);
            if (playlist == null)
                return NotFound($"Playlist with id {playlistId} not found.");

            var item = await _playlistItemService.GetByIdAsync(itemId, cancellationToken);
            if (item == null || item.PlaylistId != playlistId)
                return NotFound();

            var dto = _mapper.Map<PlaylistItemDto>(item);
            return Ok(dto);
        }

        // POST: api/playlists/{playlistId}/items
        [HttpPost("{playlistId:int}/items")]
        public async Task<ActionResult<PlaylistItemDto>> AddItem(
            int playlistId,
            [FromBody] PlaylistItemCreateDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var playlist = await _playlistService.GetByIdAsync(playlistId, cancellationToken);
            if (playlist == null)
                return NotFound($"Playlist with id {playlistId} not found.");

            // Garante que o PlaylistId do DTO coincide com o da rota
            dto.PlaylistId = playlistId;

            var entity = _mapper.Map<PlaylistItem>(dto);

            // Se SortOrder for null no DTO (e 0 no entity), a SP calcula o próximo
            var created = await _playlistItemService.AddAsync(entity, cancellationToken);

            var resultDto = _mapper.Map<PlaylistItemDto>(created);

            return CreatedAtAction(
                nameof(GetItemById),
                new { playlistId = playlistId, itemId = resultDto.Id },
                resultDto);
        }

        // PUT: api/playlists/{playlistId}/items/{itemId}
        [HttpPut("{playlistId:int}/items/{itemId:int}")]
        public async Task<IActionResult> UpdateItem(
            int playlistId,
            int itemId,
            [FromBody] PlaylistItemUpdateDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var playlist = await _playlistService.GetByIdAsync(playlistId, cancellationToken);
            if (playlist == null)
                return NotFound($"Playlist with id {playlistId} not found.");

            var existing = await _playlistItemService.GetByIdAsync(itemId, cancellationToken);
            if (existing == null || existing.PlaylistId != playlistId)
                return NotFound();

            // Mapear DTO para a entidade existente
            _mapper.Map(dto, existing);

            await _playlistItemService.UpdateAsync(existing, cancellationToken);

            return NoContent();
        }

        // DELETE: api/playlists/{playlistId}/items/{itemId}
        [HttpDelete("{playlistId:int}/items/{itemId:int}")]
        public async Task<IActionResult> DeleteItem(
            int playlistId,
            int itemId,
            CancellationToken cancellationToken)
        {
            var playlist = await _playlistService.GetByIdAsync(playlistId, cancellationToken);
            if (playlist == null)
                return NotFound($"Playlist with id {playlistId} not found.");

            var existing = await _playlistItemService.GetByIdAsync(itemId, cancellationToken);
            if (existing == null || existing.PlaylistId != playlistId)
                return NotFound();

            await _playlistItemService.DeleteAsync(itemId, cancellationToken);

            return NoContent();
        }
    }
}