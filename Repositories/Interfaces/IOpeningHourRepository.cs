using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IOpeningHourRepository: IGenericRepository<OpeningHour>
    {
        /// <summary>
        /// Cambia el estado de activo a inactivo y viseversa dependiendo de estado actual !
        /// </summary>
        /// <param name="id">Id de la entidad a actualizar</param>
        /// <returns>Entitdad actualizada</returns>
       //Task<OpeningHour> ToggleStatusAsync(int id);
    }
}
