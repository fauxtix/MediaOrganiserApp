using AutoMapper;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Mappings
{
    public class ApplicationProfiles : Profile
    {
        public ApplicationProfiles()
        {
            CreateMap<Conversation, ConversationDto>();
            CreateMap<ConversationCreateDto, Conversation>();
            CreateMap<ConversationUpdateDto, Conversation>();
        }
    }
}