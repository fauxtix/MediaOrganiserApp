using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Validators;
using MediaOrganizerApp.Domain.Entities.Author_Api_Models;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web.Services.Description;

namespace Web.Blazor.Services.ApiWrappers.Authors
{
    public class AutorService : IAutorService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AutorService> _logger;
        private readonly IConfiguration _env;
        private readonly string _Uri;

        private readonly IAuthorImageService _authorImageService;

        public AutorService(HttpClient httpClient,
            ILogger<AutorService> logger,
            IConfiguration Env,
            IAuthorImageService authorImageService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = Env;
            _authorImageService = authorImageService;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/authors";
        }

        public async Task<IEnumerable<AutorDTO>> GetAllAsync()
        {
            try
            {
                var authors = (await _httpClient.GetFromJsonAsync<IEnumerable<AutorDTO>>(_Uri)).ToList();
                return authors;

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (Autores)");
                return Enumerable.Empty<AutorDTO>();
            }
        }

        public async Task<AutorDTO> GetAuthorsById(int id)
        {
            var uri = $"{_Uri}/{id}";

            try
            {
                var author = await _httpClient.GetFromJsonAsync<AutorDTO>(uri);
                return author;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (Autores)");
                return new AutorDTO();
            }
        }

        public async Task<bool> CreateAuthor(AutorDTO authorDTO)
        {

            try
            {
                using (HttpResponseMessage result = await _httpClient.PostAsJsonAsync(_Uri, authorDTO))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao criar Autor {exc.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAuthor(int id, AutorDTO authorDTO)
        {
            var uri = $"{_Uri}/{id}";

            try
            {
                using (HttpResponseMessage result = await _httpClient.PutAsJsonAsync(uri, authorDTO))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar Autor ({id})");
                return false;
            }
        }
        public async Task<bool> DeleteAuthor(int id)
        {
            var uri = $"{_Uri}/{id}";

            try
            {
                using (HttpResponseMessage result = await _httpClient.DeleteAsync(uri))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao apagar Autor ({id})");
                return false;
            }
        }

        public async Task<int> GetFirstId()
        {
            var uri = $"{_Uri}/getfirstid";

            try
            {
                var authorId = await _httpClient.GetFromJsonAsync<int>(uri);
                return authorId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (GetFirstId)");
                return 0;
            }
        }

        public async Task<int> GetLastId()
        {
            var uri = $"{_Uri}/getlastid";

            try
            {
                var authorId = await _httpClient.GetFromJsonAsync<int>(uri);
                return authorId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (GetLastId)");
                return 0;
            }
        }

        public async Task<int> GetIdByDescription(string description)
        {
            var uri = $"{_Uri}/GetIdByDescription/{description}";

            try
            {
                var authorId = await _httpClient.GetFromJsonAsync<int>(uri);
                return authorId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (GetIdByDescription)");
                return 0;
            }
        }

        public async Task<string> GetDescriptionById(int id)
        {
            var uri = $"{_Uri}/GetDescriptionById/{id}";

            try
            {
                var description = await _httpClient.GetFromJsonAsync<string>(uri);
                return description;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (GetDescriptionById)");
                return "";
            }
        }

        public async Task<bool> RecInUse(int id)
        {
            var uri = $"{_Uri}/RecInUse/{id}";

            try
            {
                bool inUse = await _httpClient.GetFromJsonAsync<bool>(uri);
                return inUse;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (RecInUse)");
                return true;
            }
        }

        public async Task<bool> RecInUse(string descricao)
        {
            var uri = $"{_Uri}/RecInUse/{descricao}";

            try
            {
                bool inUse = await _httpClient.GetFromJsonAsync<bool>(uri);
                return inUse;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores  (RecInUse)");
                return true;
            }
        }

        public async Task<bool> TableHasData()
        {
            var uri = $"{_Uri}/TableHasData";

            try
            {
                bool inUse = await _httpClient.GetFromJsonAsync<bool>(uri);
                return inUse;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (TableHasData)");
                return true;
            }
        }

        public async Task<List<AutorDTO>> Listar()
        {
            var uri = $"{_Uri}/Listar";

            try
            {
                var authors = await _httpClient.GetFromJsonAsync<List<AutorDTO>>(uri);
                return authors;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (GetLastId)");
                return new List<AutorDTO>();
            }
        }

        public async Task<bool> AutorExiste(string nomeAutor)
        {

            if (nomeAutor.ToLower() == "n/d") return true;

            var uri = $"{_Uri}/AutorExiste/{nomeAutor}";

            try
            {
                var authorExist = await _httpClient.GetFromJsonAsync<bool>(uri);
                return authorExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (AutorExiste)");
                return true;
            }
        }

        public async Task<AuthorModel> GetAuthorInfo_Api(string nomeAutor)
        {
            var uri = $"{_Uri}/GetAuthorInfo/{nomeAutor}";

            try
            {
                var authorInfo = await _httpClient.GetFromJsonAsync<AuthorModel>(uri);
                return authorInfo;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (GetAuthorInfo_Api)");
                return new AuthorModel();
            }
        }

        public async Task<IEnumerable<AutorDTO>> AutoresSemLivros()
        {
            var uri = $"{_Uri}/AutoresSemLivros";

            try
            {
                var autoresSemLivros = await _httpClient.GetFromJsonAsync<IEnumerable<AutorDTO>>(uri);
                return autoresSemLivros;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (GetLastId)");
                return Enumerable.Empty<AutorDTO>();
            }
        }

        public async Task<bool> AutorComLivros(int id)
        {
            var uri = $"{_Uri}/AutorComLivros/{id}";

            try
            {
                var autorComLivros = await _httpClient.GetFromJsonAsync<bool>(uri);
                return autorComLivros;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (AutorComLivros)");
                return true;
            }
        }

        public IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int CodAutor)
        {
            var uri = $"{_Uri}/PublicacoesCatalogadas/{CodAutor}";

            try
            {
                var pubs = _httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri).Result;
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Autores (PublicacoesCatalogadas)");
                return Enumerable.Empty<PublicacoesVM>();
            }
        }

        public List<string> RegistoComErros(AutorDTO autor)
        {
            List<string> validationMessages = new();
            AutorValidator validator = new();
            ValidationResult results = validator.Validate(autor);

            if (!results.IsValid)
            {

                foreach (var failure in results.Errors)
                {
                    validationMessages.Add(failure.ErrorMessage);
                }
                return validationMessages;
            }

            return default;
        }

        public async Task<(string bio, string photo, string birthDate)> GetAuthorDetails(string authorName)
        {
            string openLibraryBaseUrl = "https://openlibrary.org/";
            string searchUrl = $"{openLibraryBaseUrl}/search/authors.json?q={Uri.EscapeDataString(authorName)}";
            string authorBio;
            string authorBirthDate = "";
            string authorPhotoUrl = "images/NoPhoto.png";

            if (string.IsNullOrWhiteSpace(authorName))
            {
                return ("", "", "");
            }

            try
            {
                var searchResult = await GetJsonAsync(searchUrl);

                if (searchResult.GetProperty("numFound").GetInt32() == 0)
                {
                    return ("Sem Biografia disponível", "", "");
                }

                string authorKey = searchResult.GetProperty("docs")[0].GetProperty("key").GetString();

                string authorDetailsUrl = $"{openLibraryBaseUrl}/authors/{authorKey}.json";
                var authorDetails = await GetJsonAsync(authorDetailsUrl);

                if (authorDetails.TryGetProperty("birth_date", out JsonElement birthDateElement))
                {
                    if (birthDateElement.ValueKind == JsonValueKind.String)
                    {
                        authorBirthDate = birthDateElement.GetString();
                        if (authorBirthDate.Length == 4) // year only
                        {
                            authorBirthDate = "01/01/" + authorBirthDate;
                        }
                    }
                    else
                    {
                        authorBirthDate = DateTime.MinValue.ToString("dd/MM/yyyy");
                    }
                }

                if (authorDetails.TryGetProperty("bio", out JsonElement bioElement))
                {
                    if (bioElement.ValueKind == JsonValueKind.String)
                    {
                        // "bio" é uma string simples
                        authorBio = bioElement.GetString();
                    }
                    else if (bioElement.ValueKind == JsonValueKind.Object)
                    {
                        // "bio" é um objeto, acessar o campo "value"
                        if (bioElement.TryGetProperty("value", out JsonElement valueElement))
                        {
                            authorBio = valueElement.GetString();
                        }
                        else
                        {
                            authorBio = "Biografia disponível, mas não no formato esperado.";
                        }
                    }
                    else
                    {
                        authorBio = "Formato de biografia desconhecido.";
                    }
                }
                else
                {
                    authorBio = "Sem biografia disponível.";
                }

                if (authorDetails.TryGetProperty("photos", out JsonElement photosElement) &&
                    photosElement.ValueKind == JsonValueKind.Array &&
                    photosElement.GetArrayLength() > 0)
                {
                    int photoId = photosElement[0].GetInt32(); // Obtém o primeiro ID da lista
                    authorPhotoUrl = $"https://covers.openlibrary.org/a/id/{photoId}-M.jpg";
                }
                else
                {
                    var image = await _authorImageService.GetAuthorImageAsync(authorName);
                    if (image != null)
                    {
                        authorPhotoUrl = image;
                    }
                    else
                    {
                        authorPhotoUrl = "images/NoPhoto.png";
                    }
                }

                string translatedBio = await TranslateWithMyMemoryAsync(authorBio, "en", "pt");
                translatedBio = RemoveRepeatedBio(translatedBio);
                return (translatedBio, authorPhotoUrl, authorBirthDate);

            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao obter detalhes do autor ({authorName}) da API Open Library. Exception: {exc.Message}");
                return ("", "", ""); // the given key (bio) does not exist
            }
        }

        private async Task<JsonElement> GetJsonAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro na solicitação: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            return jsonDocument.RootElement;
        }

        public string RemoveRepeatedBio(string bio)
        {
            // Expressão regular para detectar e remover frases repetidas (caso a frase se repita imediatamente)
            //var cleanedBio = Regex.Replace(bio, @"\b([^\s]+(?:\s+[^\s]+){1,})\s+\1\b", "$1");

            // Alternativamente, podemos remover qualquer tipo de repetição (não necessariamente uma frase inteira)
            var cleanedBio = Regex.Replace(bio, @"(.+?)\s*\1", "$1"); // Detecção de repetição de palavras ou frases

            return cleanedBio;
        }


        public async Task<string> TranslateWithMyMemoryAsync(string text, string sourceLang, string targetLang)
        {
            if (text.Length > 500) text = text.Substring(0, 493) + " (...)"; // api quota (500 characters)

            string apiUrl = "https://api.mymemory.translated.net/get";
            string requestUrl = $"{apiUrl}?q={Uri.EscapeDataString(text)}&langpair={sourceLang}|{targetLang}";

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao acessar a API: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(responseContent);

            if (jsonResponse.RootElement.TryGetProperty("responseData", out var responseData) &&
                responseData.TryGetProperty("translatedText", out var translatedText))
            {
                return translatedText.GetString();
            }
            else
            {
                throw new Exception("Erro ao processar a resposta da API.");
            }
        }



        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private async Task<(HttpStatusCode, string)> AuthenticateApiAndGetData(string uri)
        {
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.RequestUri = new Uri(uri);
                request.Method = HttpMethod.Get;
                request.Headers.Add("ApiKey", _env["ApiKey"]);
                using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var statusCode = response.StatusCode;
                    return (statusCode, responseString);
                }
            }
        }
    }
}
