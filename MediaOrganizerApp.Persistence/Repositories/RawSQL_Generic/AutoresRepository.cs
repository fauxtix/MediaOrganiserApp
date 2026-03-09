using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public class AutoresRepository : BaseRepository<Autor>, IAutoresRepository
    {
        private readonly IPublicacoesRepository repoPubs;
        readonly StringBuilder sbData = new();
        private readonly IDbConnection sqlConnection;

        public AutoresRepository(IPublicacoesRepository pubsRepository,
           IDbConnectionFactory dbFactory)
        {
            repoPubs = pubsRepository ?? throw new ArgumentNullException(nameof(pubsRepository));
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }


        public async Task<bool> AutorComLivros(int IdAutor)
        {
            return await repoPubs.AutorComLivros(IdAutor);
        }

        public async Task<IEnumerable<AutorDTO>> AutoresSemLivros()
        {
            return await repoPubs.AutoresSemLivros();
        }

        public async Task<bool> AutorExiste(string NomeAutor)
        {
            return await AutorExiste(NomeAutor);
        }

        public async Task<IEnumerable<AutorDTO>> GetAutores_Extended_Async()
        {
            sbData.Clear();
            sbData.Append("SELECT aut.Id, aut.Descricao, Sobre, DataNascimento, ImagemAutor, ImagemAutor, ");
            sbData.Append("pub.Titulo, pub.DigitalPath, edi.Descricao NomeEditora ");
            sbData.Append("FROM[Publicacoes].[dbo].[Autor] aut ");
            sbData.Append("INNER JOIN Publicacao pub ");
            sbData.Append("ON aut.id = pub.IdAutor ");
            sbData.Append("INNER JOIN Editora edi ");
            sbData.Append("ON edi.Id = pub.IdEditora");
            using (IDbConnection connection = sqlConnection)
            {
                return await connection.QueryAsync<AutorDTO>(sbData.ToString());
            }
        }

        public IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int IdAutor = 0)
        {
            sbData.Clear();

            sbData.Append("select e.Descricao, a.Descricao, l.Titulo, l.DigitalPath, e.Descricao AS NomeEditora ");
            sbData.Append("from publicacao l ");
            sbData.Append("inner join Editora e on l.IdEditora = e.Id ");
            sbData.Append("inner join Autor a on l.IdAutor = a.Id ");

            using (IDbConnection connection = sqlConnection)
            {
                if (IdAutor > 0)
                {
                    sbData.Append("where l.IdAutor = @IdAutor");
                    return connection.Query<PublicacoesVM>(sbData.ToString(), new { IdAutor });
                }
                else
                {
                    return connection.Query<PublicacoesVM>(sbData.ToString());
                }
            }
        }

        public async Task<IEnumerable<PublicacoesVM>> PublicacoesCatalogadas_Async(int IdAutor)
        {
            sbData.Clear();

            sbData.Append("select e.Descricao, a.Descricao, l.Titulo, l.DigitalPath, e.Descricao AS NomeEditora ");
            sbData.Append("from publicacao l ");
            sbData.Append("inner join Editora e on l.IdEditora = e.Id ");
            sbData.Append("inner join Autor a on l.IdAutor = a.Id ");

            using (IDbConnection connection = sqlConnection)
            {
                if (IdAutor > 0)
                {
                    sbData.Append("where l.IdAutor = @IdAutor");
                    return await connection.QueryAsync<PublicacoesVM>(sbData.ToString(), new { IdAutor });
                }
                else
                {
                    return await connection.QueryAsync<PublicacoesVM>(sbData.ToString());
                }
            }
        }

        public async Task<AutorDTO> GetAuthorById_Async(int Id)
        {
            sbData.Clear();
            sbData.Append("SELECT Id, Descricao, Sobre, DataNascimento, ImagemAutor ");
            sbData.Append("FROM[Publicacoes].[dbo].[Autor] ");
            sbData.Append("WHERE ");
            sbData.Append("Id = @Id ");
            using (IDbConnection connection = sqlConnection)
            {
                return await connection.QuerySingleAsync<AutorDTO>(sbData.ToString(), new { Id });
            }
        }
    }
}
