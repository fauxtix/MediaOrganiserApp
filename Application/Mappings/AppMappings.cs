using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
namespace MediaOrganizerApp.Application.Mappings
{
    public class AppMappings : Profile
    {
        public AppMappings()
        {
            CreateMap<Autor, AutorDTO>().ReverseMap();
            CreateMap<Categoria, CategoryVM>().ReverseMap();
            CreateMap<Editora, PublisherVM>().ReverseMap();
            CreateMap<Formato, FormatoPublicacaoVM>().ReverseMap();
            CreateMap<Formato_Media, FormatoMediaVM>().ReverseMap();
            CreateMap<Publicacao, PubsWithDescriptions>().ReverseMap();
            CreateMap<Editora, PublisherVM>().ReverseMap();
            CreateMap<LocalArquivo, LocalArquivoVM>().ReverseMap();
            CreateMap<Media, MediaVM>().ReverseMap();

            CreateMap<SummaryData, SummaryDataDTO>().ReverseMap();
            CreateMap<AppLogDto, AppLog>().ReverseMap();

            CreateMap<ApplicationSettings, ApplicationSettingsDto>().ReverseMap();


            //int count = 0;

            //var configuration = new MapperConfiguration(cfg =>
            //   cfg.CreateMap<Autor, SummaryData>()
            //     .ForMember(dest => dest.Contagem, opt => opt.MapFrom<CustomResolver>()));

            //CreateMap<Autor, SummaryData>()
            //  .ForMember(dest => dest.Contagem, opt => opt.MapFrom<CustomResolver>());
        }

    }

    public interface IValueResolver<in TSource, in TDestination, TDestMember>
    {
        TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context);
    }
    public class CustomResolver : IValueResolver<Autor, SummaryData, int>
    {
        public int Resolve(Autor source, SummaryData destination, int member, ResolutionContext context)
        {
            return 5;
        }
    }
}
