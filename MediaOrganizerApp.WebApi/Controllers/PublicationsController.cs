using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class PublicationsController : ControllerBase
    {
        private readonly IPublicacoesRepository _repoPubs;
        private readonly IAutoresRepository _repoAuthors;
        protected IMapper _mapper;
        private readonly ILogger<PublicationsController> _logger;

        public PublicationsController(IPublicacoesRepository repoPubs,
                                      ILogger<PublicationsController> logger,
                                      IMapper mapper,
                                      IAutoresRepository repoAuthors)
        {
            _repoPubs = repoPubs;
            _logger = logger;
            _mapper = mapper;
            _repoAuthors = repoAuthors;
        }

        [HttpPost]
        //[ServiceFilter(typeof(ValidateModelAttribute))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePublication([FromBody] PubsWithDescriptions publicacaoDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                if (publicacaoDTO is null)
                {
                    var errorResponse = new ApiResponse<object>(false, "invalid request body.", null);
                    return BadRequest(errorResponse);
                }

                bool TitleExist = await _repoPubs.TituloExiste_BD(!string.IsNullOrEmpty(publicacaoDTO.TituloOriginal) ? publicacaoDTO.TituloOriginal : publicacaoDTO.Titulo);
                if (TitleExist)
                {
                    var errorResponse = new ApiResponse<object>(false, "Publication already exists.", null);

                    _logger.LogWarning($"Título '{publicacaoDTO.TituloOriginal}' já existe na base de dados");
                    return BadRequest(errorResponse);
                }

                var publicacao = _mapper.Map<Publicacao>(publicacaoDTO);
                publicacao.CreatedBy = publicacaoDTO.CreatedBy;
                long createdPublicationId = await _repoPubs.Insert_Async(publicacao);

                var createdPublication = await GetByIdAsync_Extended((int)createdPublicationId);

                var actionReturned = CreatedAtAction(nameof(GetByIdAsync_Extended), new { id = createdPublicationId }, createdPublication);

                return actionReturned;
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<object>(false, $"{location}: Error creating publication", null);

                _logger.LogError(ex, $"Erro ao criar Publicação {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }


        [HttpPost("storepdfnotdeleted/{pdfName}")]
        //[ServiceFilter(typeof(ValidateModelAttribute))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StorePdfNotDeleted([FromBody] PdfsAndPngNotDeleted pdfName)
        {
            var location = GetControllerActionNames();
            if (pdfName is null)
            {
                var errorResponse = new ApiResponse<object>(false, $"{location}: O ficheiro passado como parâmetro  é incorreto.", null);

                _logger.LogWarning($"{location}: O ficheiro passado como parâmetro  é incorreto.");
                return BadRequest(errorResponse);
            }

            try
            {
                await _repoPubs.StorePdfNotDeleted(pdfName);
                return Ok();
            }
            catch (Exception exc)
            {
                var errorResponse = new ApiResponse<object>(false, $"{location}: Error inserting file", null);

                _logger.LogError($"{location} - Error inserting file ({exc.Message})");

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }

        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePublication(int id, [FromBody] PubsWithDescriptions publicacaoDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                if (publicacaoDTO == null)
                {
                    var errorResponse = new ApiResponse<object>(false, $"{location}: A publicação passada como paràmetro, é incorreta.", null);

                    _logger.LogWarning($"A publicação passada como paràmetro, é incorreta.");
                    return BadRequest(errorResponse);
                }

                if (id != publicacaoDTO.Id)
                {
                    var errorResponse = new ApiResponse<object>(false, $"{location}: O ID da publicação ({id}) passado como paràmetro, está incorreto", null);
                    _logger.LogWarning($"O ID da publicação ({id}) passado como paràmetro, está incorreto");
                    return BadRequest(errorResponse);
                }

                var pubToUpdate = await _repoPubs.Query_ById_Async(id);
                if (pubToUpdate == null)
                {
                    var errorResponse = new ApiResponse<object>(false, $"{location}: A publicação passada não existe na base de dados", null);

                    _logger.LogWarning($"A publicação passada não existe na base de dados.");
                    return NotFound(errorResponse);
                }
                _mapper.Map(publicacaoDTO, pubToUpdate);
                await _repoPubs.Update_Async(pubToUpdate);
                return NoContent();
            }
            catch (Exception exc)
            {
                var errorResponse = new ApiResponse<object>(false, $"{location}: Error updating publication", null);

                _logger.LogError($"{location} - Error updating publication ({exc.Message})");

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> DeletePublication(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                if (id < 1)
                {
                    var errorResponse = new ApiResponse<object>(false, $"{location}: Delete failed with bad data - id: {id}", null);

                    _logger.LogWarning($"{location}: Delete failed with bad data - id: {id}");
                    return BadRequest(errorResponse);
                }

                var pubToDelete = await _repoPubs.Query_ById_Extended(id);
                if (pubToDelete == null)
                {
                    var errorResponse = new ApiResponse<object>(false, $"{location}: Publicação com o Id {id} não encontrado", null);

                    _logger.LogError($"{location}: Delete failed with bad data - id: {id}");
                    return BadRequest(errorResponse);
                }

                // when we delete a publication, we must also delete the associated author from the Author table, as long as they don't exist in other publications
                var pubAuthorId = pubToDelete.IdAutor;
                var authorCanBeDeleted = (await _repoPubs.CanAuthorBeDeleted(pubAuthorId));

                var pubAuthor = await _repoAuthors.GetAuthorById_Async(pubAuthorId);
                if (pubAuthor is null)
                {
                    var errorResponse = new ApiResponse<object>(false, $"{location}:Auhor with Id {pubAuthorId} not found to delete", null);

                    _logger.LogWarning($"Auhor with Id {pubAuthorId} not found to delete");
                    return NotFound(errorResponse);
                }

                await _repoPubs.DeleteById_Async(id);
                var authorName = pubAuthor.Descricao;
                _logger.LogWarning($"Publication {pubToDelete.Titulo} deleted");
                if (authorCanBeDeleted) // the author has only this publication in the database, so delete the author record too
                {
                    await _repoAuthors.DeleteById_Async(pubAuthorId);
                    _logger.LogWarning($"Author of the Publication ('{authorName}') deleted");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<object>(false, $"{location}:Publication was not deleted. Please check log", null);

                _logger.LogError($"Publicação com o Id {id} não foi apagado ({ex.Message})");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPut("AtualizaTituloOriginal/{id:int}/{tituloOriginal}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtualizaTituloOriginal(int id, string tituloOriginal)
        {
            var location = GetControllerActionNames();

            try
            {
                await _repoPubs.AtualizaTituloOriginal(id, tituloOriginal);
                return NoContent();

            }
            catch (Exception exc)
            {
                var errorResponse = new ApiResponse<object>(false, $"{location}:Erro no método 'AtualizaTituloOriginal'. Please check log", null);

                _logger.LogError($"AtualizaTituloOriginal() - Erro no método 'AtualizaTituloOriginal' ({exc.Message})");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPut("AtualizaCapaPublicacao/{id:int}/{ImageCoverPath}")]
        public async Task<IActionResult> Atualiza_CapaPublicacao(int id, string imageCoverPath)
        {
            var location = GetControllerActionNames();
            try
            {

                var result = await _repoPubs.Atualiza_CapaPublicacao(id, imageCoverPath);
                return NoContent();
            }
            catch (Exception exc)
            {
                _logger.LogError($"Atualiza_CapaPublicacao() - Erro na atualização da publicação ({exc.Message})");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        [Route("atualiza_publicacao/{id:int}")]
        public async Task<IActionResult> Atualiza_Publicacao(int id, [FromBody] PubsWithDescriptions pub)
        {
            var location = GetControllerActionNames();

            try
            {
                // 09/08/21 - incluído param 'idcategoria', para prevenir erro de transferência de fivheiros, em que é necessário repôr categoria anterior.
                var result = await _repoPubs.Atualiza_Publicacao(id, pub.DigitalPath, pub.ImagemCapa, pub.IdCategoria);
                return NoContent();

            }
            catch (Exception exc)
            {

                _logger.LogError($"Atualiza_Publicacao() - Erro na atualização da publicação ({exc.Message})");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet("{userId}/{userRole}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync(string userId, string userRole)
        {
            var location = GetControllerActionNames();

            try
            {
                var output = await _repoPubs.GetAllPubs_Extended(userId, userRole);
                if (output.Any())
                {
                    var response = new ApiResponse<object>(false, $"{location}:. Data retrieved", output);
                    return Ok(output);
                }
                else
                {
                    return Ok(Enumerable.Empty<PubsWithDescriptions>());
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<object>(false, $"{location}:. Please check log", new { ex.Message });

                _logger.LogError(ex, $"Erro no Api (GetAllAsync ({ex.Message}))");
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync_Extended(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                var output = await _repoPubs.Query_ById_Extended(id);
                if (output is not null)
                {
                    return Ok(output);
                }
                else
                {
                    _logger.LogWarning($"Erro no Api (GetByIdAsync_Extended ({id}))");

                    return NotFound($"GetByIdAsync_Extended ({id})");
                }
            }
            catch (Exception ex)
            {
                //response.ErrorMessage = $"{location}:  erro ao pesquisar publicação ({id}";

                _logger.LogError(ex, $"Erro ao pesquisar publicação (GetByIdAsync_Extended) ({id}) ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("filter/{filter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByFilterAsync(string filter)
        {
            var location = GetControllerActionNames();

            try
            {
                var output = (await _repoPubs.Query_Async_Extended(filter)).ToList();
                if (output.Any())
                    return Ok(output);
                else
                {
                    _logger.LogWarning($"Query_Async_Extended(filter))- {filter}. Não encontrados resultados ");

                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                //response.ErrorMessage = $"{location}:  Erro ao pesquisar publicação por Filtro (Query_Async_Extended({filter}))";

                _logger.LogError(ex, $"Erro ao pesquisar publicação por Filtro (Query_Async_Extended(filter)) - {filter} ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("categoria/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCategoria(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                var output = await _repoPubs.Listar_IDCategoria(id);
                if (output.Any())
                    return Ok(output);
                else
                {
                    _logger.LogWarning($"Erro ao pesquisar publicação pela Categoria {id}");

                    return NotFound($"Pesquisar publicação pela Categoria ({GetByCategoria}) {id}. não encontrado");

                }
            }
            catch (Exception ex)
            {
                //response.ErrorMessage = $"{location}:  Erro ao pesquisar publicação pela Categoria {id}";

                _logger.LogError(ex, $"Erro ao pesquisar publicação pela Categoria {id} ({ex.Message})");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



        [HttpGet("titulo/{titulo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByTitulo(string titulo)
        {
            var location = GetControllerActionNames();

            try
            {
                var output = await _repoPubs.Listar_Titulo(titulo);
                if (output.Any())
                    return Ok(output);
                else
                {
                    _logger.LogWarning($"Pesquisar (GetByTitulo) por {titulo} não encontrado");

                    return NotFound($"Pesquisar (GetByTitulo) por {titulo} não encontrado");
                }
            }
            catch (Exception exc)
            {
                //response.ErrorMessage = $"{location}:  Erro no Api 'GetByTitulo' ({titulo})";

                _logger.LogError(exc, $"Erro no Api (GetByTitulo) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("getimagelocations/{idcategoria:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImageLocationsAsync(int idCategoria)
        {
            var location = GetControllerActionNames();

            try
            {
                var output = await _repoPubs.GetImageLocations(idCategoria);
                if (output.Any())
                    return Ok(output);
                else
                {
                    _logger.LogWarning($"Erro no Api (GetImageLocationsAsync) {idCategoria}");
                    return NotFound("GetImageLocationsAsync não devolveu resultados");
                }
            }
            catch (Exception exc)
            {
                // response.ErrorMessage = $"{location}:  Erro no Api 'GetImageLocationsAsync' (categoria = {idCategoria})";

                _logger.LogError(exc, $"Erro no Api (GetImageLocationsAsync) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet("PubsNotFoundOnDisk/{webRoot}")]
        public async Task<IActionResult> PubsNotFoundOnDisk(string webRoot)
        {
            try
            {
                var decodedWebRoot = Uri.UnescapeDataString(webRoot);
                var output = await _repoPubs.PubsNotFoundOnDisk(decodedWebRoot);
                if (output.Any())
                {
                    return Ok(output);
                }
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (PubsNotFoundInDisk) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar publicações não existentes em disco ({exc.Message})");
            }
        }


        [HttpGet("AutoresComLivros/{id:int}")]
        public async Task<IActionResult> AutorComLivros(int idAutor)
        {
            try
            {
                bool hasBooks = await _repoPubs.AutorComLivros(idAutor);
                return Ok(hasBooks);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (AutoresComLivros) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar autores com livros ({exc.Message})");
            }
        }

        [HttpGet("EditorasSemLivros")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditorasSemLivros()
        {
            try
            {
                var output = await _repoPubs.EditorasSemLivros();
                if (output.Any())
                {
                    return Ok(output);
                }
                else
                {
                    _logger.LogWarning("(EditorasSemLivros) não devolveu resultados");
                    return NotFound("(EditorasSemLivros) não devolveu resultados");
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (EditorasSemLivros) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (publicações) Editoras sem livros ({exc.Message})");
            }
        }

        [HttpGet("GetFirstIdLivro")]
        public async Task<IActionResult> GetFirstIdLivro()
        {
            try
            {
                int result = await _repoPubs.GetFirstIdLivro();
                return Ok(result);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (GetFirstIdLivro) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar GetFirstIdLivro ({exc.Message})");
            }
        }

        [HttpGet("Query_ById_Extended/{id:int}")]
        public async Task<IActionResult> Query_ById_Extended(int Id)
        {
            try
            {
                var output = await _repoPubs.Query_ById_Extended(Id);
                if (output is not null)
                    return Ok(output);
                else
                {
                    _logger.LogWarning($"(Query_ById_Extended) {Id} não devolveu resultados");

                    return NotFound($"(Query_ById_Extended) {Id} não devolveu resultados");

                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (Query_ById_Extended) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar publicação  por Id (Query_ById_Extended) ({exc.Message})");
            }
        }

        [HttpGet("Query_Async_Extended/{where}")]
        public async Task<IActionResult> Query_Async_Extended(string where)
        {
            try
            {
                var output = (await _repoPubs.Query_Async_Extended(where)).ToList();
                if (output.Any())
                    return Ok(output);
                else
                {
                    _logger.LogWarning($"(Query_Async_Extended(filter)) - {where} não devolveu resultados");

                    return NotFound($"(Query_Async_Extended(filter)) - {where} não devolveu resultados");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao pesquisar publicação por Filtro (Query_Async_Extended(filter)) - {where} ");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar por Filtro ({where})");
            }
        }

        [HttpGet("GetID_ByCapa/{capa}")]
        public async Task<IActionResult> GetID_ByCapa(string capa)
        {
            try
            {
                var output = (await _repoPubs.GetID_ByCapa(capa)).ToList();
                if (output.Any())
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao pesquisar por Filtro (GetID_ByCapa(capa)) - {capa}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar por Id (GetID_ByCapa) ({capa})");
            }
        }

        [HttpGet("GetID_ByPath/{titulo}")]
        public async Task<IActionResult> GetID_ByPath(string titulo)
        {
            try
            {
                var output = await _repoPubs.GetID_ByPath(titulo);
                if (output > 0)
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao pesquisar publicação (GetID_ByPath) ({titulo}) ");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar publicação (GetID_ByPath) ({titulo})");
            }
        }

        [HttpGet("GetLastIdLivro")]
        public async Task<IActionResult> GetLastIdLivro()
        {
            try
            {
                int result = await _repoPubs.GetLastId_Async();
                return Ok(result);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (GetLastId) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (GetLastId) ({exc.Message})");
            }
        }

        [HttpGet("GetLastTitle")]
        public async Task<IActionResult> GetLastTitle()
        {
            try
            {
                string result = await _repoPubs.GetLastTitle();
                return Ok(result);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (GetLastTitle) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (GetLastTitle) ({exc.Message})");
            }
        }

        [HttpGet("GetPubUserName/{id:int}")]
        public IActionResult GetPubUserName(int userID)
        {
            try
            {
                string result = _repoPubs.GetPubUserName(userID);
                return Ok(result);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (GetPubUserName) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (GetPubUserName) ({exc.Message})");
            }
        }

        [HttpGet("GetPubsByTitle/{title}")]
        public async Task<IActionResult> GetPubsByTitle(string title)
        {
            try
            {
                var output = await _repoPubs.GetPubsByTitle(title);
                if (output.Any())
                    return Ok(output);
                else
                {
                    _logger.LogWarning($"(GetPubsByTitle) {title} não devolveu resultados");

                    return NotFound($"(GetPubsByTitle) {title} não devolveu resultados");
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (GetPubsByTitle) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (GetPubsByTitle) ({exc.Message})");
            }
        }

        [HttpGet("IDPublicacaoExiste_BD/{id:int}")]
        public async Task<IActionResult> IDLivroExiste_BD(int id)
        {
            try
            {
                bool idExists = await _repoPubs.IDLivroExiste_BD(id);
                return Ok(idExists);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (IDLivroExiste_BD) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (IDLivroExiste_BD) ({exc.Message})");
            }
        }

        [HttpGet("Listar/{grupo}")]
        public async Task<IActionResult> Listar(Agrupamento grupo)
        {
            try
            {
                var output = await _repoPubs.Listar(grupo);
                if (output.Any())
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (Listar) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (Listar) ({exc.Message})");
            }
        }

        [HttpGet]
        [Route("Listar_Duplicados")]
        public async Task<IActionResult> Listar_Duplicados()
        {
            try
            {
                var output = await _repoPubs.Listar_Duplicados();
                if (output is null)
                {
                    return BadRequest();
                }

                return Ok(output);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (Listar_Duplicados) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (Listar_Duplicados) ({exc.Message})");
            }
        }

        [HttpGet("listar_id_extended/{id:int}")]
        public async Task<IActionResult> Listar_ID_Extended(int id)
        {
            try
            {
                var output = await _repoPubs.Listar_ID_Extended(id);
                if (output.Id > 0)
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (Listar_ID_Extended) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (Listar_ID_Extended) ({exc.Message})");
            }
        }

        [HttpGet("Listar_Lidos/{lido}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Listar_Lidos(bool lido)
        {
            try
            {
                var output = await _repoPubs.Listar_Lidos(lido);
                if (output.Any())
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (Listar_ID_Extended) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar (Listar_ID_Extended) ({exc.Message})");
            }
        }

        [HttpGet("listar_pagos/{pago}")]
        public async Task<IActionResult> Listar_Pagos(bool pago)
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoPubs.Listar_Pagos(pago);
                if (output.Any())
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao pesquisar por Filtro (Listar_Pagos(Pago)) - {pago} ");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet]
        [Route("Listar_SemCapa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> Listar_SemCapa()
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoPubs.Listar_SemCapa();
                return Ok(output);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao pesquisar (Listar_SemCapa) ");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("Listar_SemPaginas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Listar_SemPaginas()
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoPubs.Listar_SemPaginas();
                if (output is null)
                {
                    return BadRequest();
                }

                return Ok(output);
                //else
                //    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao pesquisar por Filtro (Listar_SemPaginas)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("tituloexiste_bd/{sTituloOriginal}")]
        public async Task<bool> TituloExiste_BD(string sTituloOriginal)
        {
            bool TitleExist = await _repoPubs.TituloExiste_BD(sTituloOriginal);
            string sErrorMsg = $"Título '{sTituloOriginal}' já existe na base de dados";
            if (TitleExist)
            {
                _logger.LogWarning(sErrorMsg);
                return true;
            }

            return false;
        }

        [HttpGet("getpubformat/{pubId:int}")]
        public async Task<int> GetPubFormat(int pubId)
        {
            int pubFormat = await _repoPubs.GetPubFormat(pubId);

            return pubFormat;
        }


        [HttpPut("updatepubsfromcontextmenu/{updatequery}")]
        public async Task<bool> UpdatePubsFromContextMenu(string updateQuery)
        {
            try
            {
                await _repoPubs.UpdatePubsFromContextMenu(updateQuery);
                return true;

            }
            catch
            {
                return false;
            }
        }


        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, $"Algo de errado ocorreu ({message}). Contacte Administrador, p.f.");
        }
    }
}
