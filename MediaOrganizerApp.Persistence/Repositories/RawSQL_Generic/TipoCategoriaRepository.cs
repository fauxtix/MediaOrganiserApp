using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public class TipoCategoriaRepository : BaseRepository<TipoCategoria>, ITipoCategoriaRepository
    {

        public int GetIdTipoCategoria(string descricao)
        {
            return base.GetIdByDescription(descricao);
        }
    }
}
