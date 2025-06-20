using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IOpeningHourRepository: IGenericRepository<OpeningHour>
    {
        ///<summary>Verificar si la fecha actual, dia y mes es feriaro a nivel nacional o no.</summary>
        ///<param name="ct"></param>
        ///<returns>True si la fecha actual esta dentro de unos de los feriados espesificados por el admin, caso contrario false.</returns>
        Task<bool> IsHolidayAsync(CancellationToken ct = default);
        /// <summary>
        /// Verificar si la hora actual esta dentro de un horario de atención o no.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>True si la hora actual esta dentro del rango de una hora de atención al cliente, caso contrario false.</returns>
        Task<bool> IsOutOfOpeningHour(CancellationToken ct = default);
    }
}
