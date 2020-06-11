using AutoMapper;
using ParkyAPI.Models;
using ParkyAPI.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyAPI.ParkyMapper
{
    public class ParkyMapper: Profile
    {
        public ParkyMapper()
        {
            CreateMap<NationalPark, NationalParkDto>().ReverseMap();
        }
    }
}
