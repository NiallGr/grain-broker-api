using AutoMapper;
using GrainBroker.Core.DTOs;
using GrainBroker.Data.Entities;

namespace GrainBroker.Core.Mappings;
public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<GrainOrder, OrderDto>();
    }
}
