using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.PlayLists;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaOrganizerApp.Application.Mappings
{
    public class PlaylistProfile : Profile
    {
        public PlaylistProfile()
        {
            CreateMap<Playlist, PlaylistDto>();
            CreateMap<Playlist, PlaylistDetailsDto>();

            CreateMap<PlaylistCreateDto, Playlist>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());

            CreateMap<PlaylistUpdateDto, Playlist>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());
        }
    }
}
