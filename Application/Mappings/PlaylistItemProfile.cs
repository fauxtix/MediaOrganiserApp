using System;
using System.Collections.Generic;
using System.Text;

namespace MediaOrganizerApp.Application.Mappings
{
    using AutoMapper;
    using MediaOrganizerApp.Application.DTOs;
    using MediaOrganizerApp.Application.DTOs.PlayLists;
    using MediaOrganizerApp.Domain.Entities;

    public class PlaylistItemProfile : Profile
    {
        public PlaylistItemProfile()
        {
            CreateMap<PlaylistItem, PlaylistItemDto>();

            CreateMap<PlaylistItemCreateDto, PlaylistItem>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.SortOrder, o => o.Ignore()); // podes tratar no serviço

            CreateMap<PlaylistItemUpdateDto, PlaylistItem>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.PlaylistId, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());
        }
    }
}
