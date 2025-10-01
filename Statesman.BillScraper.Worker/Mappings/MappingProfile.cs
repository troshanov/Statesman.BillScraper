using AutoMapper;
using Statesman.BillScraper.DTOs;
using Statesman.BillScraper.Data.Models;

namespace Statesman.BillScraper.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<BillDto, BillEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Sign, opt => opt.MapFrom(src => src.PdfId))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateTime.Parse(src.Date)))
            .ForMember(dest => dest.IsParsed, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.ParsedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RawText, opt => opt.Ignore())
            .ForMember(dest => dest.NodeId, opt => opt.Ignore())
            .ForMember(dest => dest.Sponsors, opt => opt.Ignore());

        CreateMap<LegislatorDto, LegislatorEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.NodeId, opt => opt.Ignore())
            .ForMember(dest => dest.SponsoredBills, opt => opt.Ignore());
    }
}
