﻿using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class SystemParamRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public SystemParamType Type { get; set; }
    }
}
