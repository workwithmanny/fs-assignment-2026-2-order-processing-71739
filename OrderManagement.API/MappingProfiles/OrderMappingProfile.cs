using AutoMapper;
using OrderManagement.API.Models;
using Shared.Contracts.DTOs;

namespace OrderManagement.API.MappingProfiles;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<OrderItem, OrderItemResponseDto>();

        CreateMap<PaymentRecord, PaymentResponseDto>();

        CreateMap<ShipmentRecord, ShipmentResponseDto>();

        CreateMap<OrderItemDto, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.Order, opt => opt.Ignore());
    }
}
