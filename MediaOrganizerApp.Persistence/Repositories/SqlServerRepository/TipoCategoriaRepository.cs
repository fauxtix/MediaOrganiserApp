using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class TipoCategoriaRepository : SQLServerBaseRepository<TipoCategoria>, ITipoCategoriaRepository
    {

        public int GetIdTipoCategoria(string descricao)
        {
            return base.GetIdByDescription(descricao);
        }
    }
}
