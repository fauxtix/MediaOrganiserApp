using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Persistence.Repositories.MySqlRepository
{
    public class TipoCategoriaRepository : SQLServerBaseRepository<TipoCategoria>, ITipoCategoriaRepository
    {

        public int GetIdTipoCategoria(string descricao)
        {
            return base.GetIdByDescription(descricao);
        }
    }
}
