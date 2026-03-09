using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.Domain.Entities.Author_Api_Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	//[ApiKey]
	public class AuthorsController : ControllerBase
	{
		private readonly ILogger<AuthorsController> _logger;
		private readonly IAutoresRepository _repoAutores;
		private readonly IPublicacoesRepository _repoPubs;

		private readonly IMapper _mapper;

		public AuthorsController(ILogger<AuthorsController> logger,
			IAutoresRepository repoAutores,
			IPublicacoesRepository repoPubs,
			IMapper mapper)
		{
			_logger = logger;
			_repoAutores = repoAutores;
			_repoPubs = repoPubs;
			_mapper = mapper;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Author))]
		[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Author))]
		public async Task<IActionResult> GetAuthors()
		{
			var location = GetControllerActionNames();
			try
			{
				var authors = await _repoAutores.Query_Async();
				if (authors.Any())
				{
					return Ok(authors);
				}
				else
					return Ok(Enumerable.Empty<AutorDTO>());
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Erro no Api (GetAuthors): {e.Message}");
				return InternalError($"{location}: {e.Message} - {e.InnerException}");
			}
		}

		[HttpGet("{id:int}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAuthorById(int id)
		{
			var location = GetControllerActionNames();
			try
			{
				var output = await _repoAutores.GetAuthorById_Async(id);
				if (output != null)
				{
					return Ok(output);
				}
				else
				{
					_logger.LogWarning($"{location}: Failed to retrieve record with id: {id}");
					return Ok(Enumerable.Empty<AutorDTO>());
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Erro no Api (GetAuthorById)");
				return InternalError($"{location}: {e.Message} - {e.InnerException}");
			}
		}

		[HttpGet("entradaexiste_bd/{campo}/{str}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> EntradaExiste_BD(string campo, string str)
		{
			var location = GetControllerActionNames();
			try
			{
				var entryExists = await _repoAutores.EntradaExiste_BD_Async(campo, str);
				return Ok(entryExists);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (EntradaExiste_BD)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}


		[HttpGet("getfirstid")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetFirstId()
		{
			var location = GetControllerActionNames();
			try
			{
				int output = await _repoAutores.GetFirstId_Async();
				return Ok(output);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (GetFirstId)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("getlastid")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetLastId()
		{
			var location = GetControllerActionNames();
			try
			{
				int output = await _repoAutores.GetLastId_Async();
				return Ok(output);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (getlastid)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("query/{where}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Query(string where = null)
		{
			var location = GetControllerActionNames();
			try
			{
				var output = await _repoAutores.Query_Async(where);
				_mapper.Map<IEnumerable<AutorDTO>>(output);
				return Ok(output);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (Query_Async)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("GetIdByDescription/{description}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetIdByDescription(string description)
		{
			var location = GetControllerActionNames();
			try
			{
				int output = await _repoAutores.GetIdByDescription_Async(description);
				if (output > 0)
					return Ok(output);
				else
					return NotFound();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (GetIdByDescription_Async)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("GetDescriptionById/{id:int}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetDescriptionById(int id)
		{
			var location = GetControllerActionNames();
			try
			{
				string output = await _repoAutores.GetDescriptionById_Async(id);
				if (!string.IsNullOrEmpty(output))
					return Ok(output);
				else
					return NotFound();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (GetIdByDescription_Async)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("RecInUse/{id:int}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> RecInUse(int id)
		{
			var location = GetControllerActionNames();
			try
			{
				bool inUse = await _repoAutores.RecInUse_Async(id);
				if (inUse)
					return Ok(true);
				else
					return Ok(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (RecInUse(int))");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("RecInUse/{tipo}/{descricao}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> RecInUse(int tipo, string descricao)
		{
			var location = GetControllerActionNames();
			try
			{
				bool inUse = await _repoAutores.RecInUse_ByDescription_Async(tipo, descricao);
				if (inUse)
					return Ok(true);
				else
					return Ok(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (RecInUse(string))");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("TableHasData")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> TableHasData()
		{
			var location = GetControllerActionNames();
			try
			{
				bool hasData = await _repoAutores.TableHasData_Async();
				if (hasData)
					return Ok(true);
				else
					return Ok(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (TableHasData)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("Listar")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Listar()
		{
			var location = GetControllerActionNames();
			try
			{
				var autores = await _repoAutores.GetAutores_Extended_Async();
				if (autores.Any())
				{
					_mapper.Map<List<AutorDTO>>(autores);
					return Ok(autores);
				}

				else
					return Ok(Enumerable.Empty<AutorDTO>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (Listar)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}


		[HttpGet("ListByDescription/{description}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> ListByDescription(string description)
		{
			var location = GetControllerActionNames();

			try
			{
				var autores = await _repoAutores.ListByDescription_Async(description);
				if (autores.Any())
				{
					_mapper.Map<IEnumerable<AutorDTO>>(autores);
					return Ok(autores);
				}

				else
					return Ok(Enumerable.Empty<AutorDTO>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (ListByDescription)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("ListById/{id:int}")]
		public async Task<IActionResult> ListById(int id)
		{
			var location = GetControllerActionNames();
			try
			{
				var autores = await _repoAutores.Listar_ID_Async(id);

				if (autores.Any())
				{
					_mapper.Map<IEnumerable<AutorDTO>>(autores);
					return Ok(true);
				}
				else
					return Ok(Enumerable.Empty<AutorDTO>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (ListById)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("AutoresSemLivros")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> AutoresSemLivros()
		{
			var location = GetControllerActionNames();
			try
			{
				var autores = await _repoPubs.AutoresSemLivros();
				if (autores.Any())
				{
					return Ok(autores);
				}
				else
					return Ok(Enumerable.Empty<AutorDTO>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (AutoresSemLivros)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("AutorComLivros/{id:int}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> AutorComLivros(int id)
		{
			var location = GetControllerActionNames();
			try
			{
				var output = await _repoAutores.GetAuthorById_Async(id);
				if (output != null)
				{
					var result = await _repoPubs.AutorComLivros(id);
					return Ok(result);
				}
				else
				{
					_logger.LogWarning($"{location}: Failed to retrieve record with id: {id}");
					return NotFound();
				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (AutoresSemLivros)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("PublicacoesCatalogadas/{id:int}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]

		public IActionResult PublicacoesCatalogadas(int id)
		{
			var location = GetControllerActionNames();
			try
			{
				var output = _repoAutores.PublicacoesCatalogadas(id);
				if (output.Any())
					return Ok(output);
				else
					return Ok(Enumerable.Empty<PublicacoesVM>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (Autores/PublicacoesCatalogadas)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("AutorExiste/{nomeAutor}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> AutorExiste(string nomeAutor)
		{
			var location = GetControllerActionNames();
			try
			{
				var authorExist = await _repoAutores.AutorExiste(nomeAutor);
				return Ok(authorExist);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (AutorExiste)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpGet("GetAuthorInfo/{nomeautor}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAuthorInfo(string nomeAutor)
		{
			var location = GetControllerActionNames();
			try
			{
				HttpClient _httpClient = new();

				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri($"https://openlibrary.org/search/authors.json?q={nomeAutor}"),
				};

				AuthorModel AuthorInfo = new();
				try
				{
					using (var response = await _httpClient.SendAsync(request))
					{
						response.EnsureSuccessStatusCode();
						AuthorInfo = await response.Content.ReadAsAsync<AuthorModel>();
						var authorKey = AuthorInfo.docs[0].key;
						AuthorInfoDetail authorDetail = await GetAuthorDetail(authorKey);

						AuthorWorksModel authorWorks = await GetAuthorWorks(authorKey);
						if (authorWorks.entries is not null)
						{
							return Ok(authorWorks);
						}
						else
						{
							return NotFound();
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
				}

				return null;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (GetAuthorInfo)");
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		private async Task<AuthorInfoDetail> GetAuthorDetail(string authorKey)
		{
			try
			{
				HttpClient _httpClient = new();
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri($"https://openlibrary.org/authors/{authorKey}.json"),
				};

				AuthorInfoDetail AuthorDetail = new();
				try
				{
					using (var response = await _httpClient.SendAsync(request))
					{
						response.EnsureSuccessStatusCode();
						AuthorDetail = await response.Content.ReadAsAsync<AuthorInfoDetail>();

					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
				}

				return AuthorDetail;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (AutorExiste)");
			}

			return null;

		}

		private async Task<AuthorWorksModel> GetAuthorWorks(string authorKey)
		{
			try
			{
				HttpClient _httpClient = new();
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri($"https://openlibrary.org/authors/{authorKey}/works.json"),
				};

				AuthorWorksModel authorWorks = new();
				try
				{
					using (var response = await _httpClient.SendAsync(request))
					{
						response.EnsureSuccessStatusCode();
						authorWorks = await response.Content.ReadAsAsync<AuthorWorksModel>();

					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
				}

				return authorWorks;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro no Api (AutorExiste)");
			}

			return null;

		}


		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateAuthor([FromBody] AutorDTO autorDTO)
		{
			var location = GetControllerActionNames();
			try
			{
				if (autorDTO is null)
				{
					return BadRequest();
				}

				var autor = _mapper.Map<Autor>(autorDTO);

				bool TitleExist = await _repoAutores.AutorExiste(autor.Descricao);
				if (TitleExist)
				{
					ModelState.AddModelError("Autor", "Nome do Autor já existe na base de dados");
					return BadRequest(ModelState);
				}
				await _repoAutores.Insert_Async(autor);
				int createdAuthorId = _repoAutores.GetLastId();
				var createdAuthor = await GetAuthorById(createdAuthorId);
				return CreatedAtAction(nameof(GetAuthorById), new { id = createdAuthorId }, createdAuthor);
			}
			catch (Exception ex)
			{
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpPut("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> UpdateAuthor(int id, [FromBody] AutorDTO autorDTO)
		{
			var location = GetControllerActionNames();
			try
			{
				if (autorDTO == null)
				{
					string msg = "O Autor passado como paràmetro, é incorreto.";
					_logger.LogWarning(msg);
					return BadRequest(msg);
				}
				if (id != autorDTO.Id)
				{
					return BadRequest($"O id ({id}) passado como paràmetro é incorreto");
				}

				var authorToUpdate = await _repoAutores.Query_ById_Async(id);
				if (authorToUpdate == null)
				{
					return NotFound($"O Autor com o Id {id} não foi encontrado");
				}

				_mapper.Map(autorDTO, authorToUpdate);

				await _repoAutores.Update_Async(authorToUpdate);
				return NoContent();
			}
			catch (Exception ex)
			{
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
			}
		}

		[HttpDelete("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteAuthor(int id)
		{
			var location = GetControllerActionNames();
			try
			{
				if (id < 1)
				{
					_logger.LogWarning($"{location}: Delete failed with bad data - id: {id}");
					return BadRequest();
				}
				var authorToDelete = await _repoAutores.GetAuthorById_Async(id);
				if (authorToDelete == null)
				{
					return NotFound($"Autor com o Id {id} não encontrado");
				}

				await _repoAutores.DeleteById_Async(id);
				return NoContent();
			}
			catch (Exception ex)
			{
				return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
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
			return StatusCode(500, "Algo de errado ocorreu. Contacte o Administrador");
		}
	}

}
