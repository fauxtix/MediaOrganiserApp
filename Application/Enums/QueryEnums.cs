using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.Enums
{
    public class QueryEnums
    {
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public enum QueryOptions
        {
            Todos,
            Autor,
            Editora,
            Genero,
            Genero_Media,
            Formato,
            Formato_Media,
            Arquivo,
            Lido,
            SemPaginas,
            SemCapa,
            Pago,
            Duplicados,
            Utilizador,
            Titulo
        }
    }
}
