namespace CustomerService.API.Utils
{
   /// <summary>
   /// Obtener la fecha actual de Nicaragua
   /// </summary>
    public interface INicDatetime
    {
        Task<DateTime> GetNicDatetime();
    }
}
