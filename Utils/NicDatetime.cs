using System;

namespace CustomerService.API.Utils
{
    public class NicDatetime: INicDatetime
    {
        public async Task<DateTime> GetNicDatetime()
        {
            // Obtener la zona horaria de Nicaragua
            TimeZoneInfo nicaraguaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Managua");

            // Obtener la fecha y hora actual en la zona horaria de Nicaragua
            DateTime fechaNicaragua = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nicaraguaTimeZone);


            // Eliminar milisegundos
            fechaNicaragua = new DateTime(
            fechaNicaragua.Year,
            fechaNicaragua.Month,
            fechaNicaragua.Day,
            fechaNicaragua.Hour,
            fechaNicaragua.Minute,
            fechaNicaragua.Second,
            0
          );
            return fechaNicaragua;
        }
    }
}
