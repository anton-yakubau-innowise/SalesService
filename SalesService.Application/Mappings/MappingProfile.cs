using AutoMapper;
using SalesService.Application.Dtos;
using SalesService.Domain.Entities;

namespace SalesService.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrderDto>();
            CreateMap<Order, OrderWithCancellationReasonDto>();
        }
    }
}