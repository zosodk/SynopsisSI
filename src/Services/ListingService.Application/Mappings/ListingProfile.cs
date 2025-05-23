// using AutoMapper;
// using SynopsisSI.Services.ListingService.Domain.Entities;
// using SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.GetListingById;
//
// namespace SynopsisSI.Services.ListingService.Application.Mappings;
//
// public class ListingProfile : Profile
// {
//     public ListingProfile()
//     {
//         CreateMap<ListingItem, ListingItemDto>()
//             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
//         CreateMap<Domain.ValueObjects.GeoLocation, GeoLocationDto>();
//     }
// }