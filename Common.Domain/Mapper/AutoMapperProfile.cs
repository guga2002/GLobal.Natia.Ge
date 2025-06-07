using AutoMapper;
using Common.Data.Entities;
using Common.Domain.Models;

namespace Common.Domain.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<ChanellModel, Chanell>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Sources, opt => opt.MapFrom(src => src.Sources)).ReverseMap();

            CreateMap<SourceModel, Source>()
              .ForMember(dest => dest.ChanellFormat, opt => opt.MapFrom(src => src.ChanellFormat))
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
              .ForMember(dest => dest.ChanellId, opt => opt.MapFrom(src => src.ChanellId)).ReverseMap();

            CreateMap<DesclamblerModel, Desclambler>()
              .ForMember(dest => dest.EmrNumber, opt => opt.MapFrom(src => src.EmrNumber))
              .ForMember(dest => dest.Card, opt => opt.MapFrom(src => src.Card))
              .ForMember(dest => dest.Port, opt => opt.MapFrom(src => src.Port))
              .ForMember(dest => dest.Source_ID, opt => opt.MapFrom(src => src.Source_ID))
              .ForMember(dest => dest.DescCard, opt => opt.MapFrom(src => src.DescCard)).ReverseMap();

            CreateMap<DesclamlerCardModel, DesclamlerCard>()
               .ForMember(dest => dest.CardManufacturer, opt => opt.MapFrom(src => src.CardManufacturer))
               .ForMember(dest => dest.CardCode, opt => opt.MapFrom(src => src.CardCode))
               .ForMember(dest => dest.DesclamblerId, opt => opt.MapFrom(src => src.DesclamblerId))
               .ForMember(dest => dest.desclambler, opt => opt.MapFrom(src => src.desclambler)).ReverseMap();

            CreateMap<Emr60InfoModel, Emr60Info>()
              .ForMember(dest => dest.Port, opt => opt.MapFrom(src => src.Port))
              .ForMember(dest => dest.SourceEmr, opt => opt.MapFrom(src => src.SourceEmr)).ReverseMap();

            CreateMap<TranscoderModel, Transcoder>()
               .ForMember(dest => dest.EmrNumber, opt => opt.MapFrom(src => src.EmrNumber))
               .ForMember(dest => dest.Card, opt => opt.MapFrom(src => src.Card))
               .ForMember(dest => dest.Port, opt => opt.MapFrom(src => src.Port))
               .ForMember(dest => dest.Source_ID, opt => opt.MapFrom(src => src.Source_ID)).ReverseMap();
        }
    }
}
