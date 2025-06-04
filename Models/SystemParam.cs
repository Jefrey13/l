using CustomerService.API.Utils.Enums;

//Creare una base d edatos, para pasar los json a base de dtaos dentro de este tabla para poder hacer dinamicos los parametros y que el usuario pueda acceder y modificarlos y que no esten fijos en ese archivo .json. Asi dame los inset crado que sea por el id 2, y no agregeues datos en los update, la fecha coloca la hoa actual en nicaragu no utf, y el type trabaja on un enum tengo un enum
//Creare una base d edatos, para pasar los json a base de dtaos dentro de este tabla para poder hacer dinamicos los parametros y que el usuario pueda acceder y modificarlos y que no esten fijos en ese archivo .json. Asi dame los inset crado que sea por el id 2, y no agregeues datos en los update, la fecha coloca la hoa actual en nicaragu no utf, y el type trabaja on un enum tengo un enum
namespace CustomerService.API.Models
{
    public class SystemParam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string? Description { get; set; }
        public SystemParamType Type { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public int? CreateBy { get; set; }
        public int? UpdateBy { get; set; }
        public bool IsActive { get; set; } = true;

        public byte[] RowVersion { get; set; } = null!;
    }
}