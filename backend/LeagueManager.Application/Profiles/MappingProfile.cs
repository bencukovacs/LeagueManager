using AutoMapper;
using LeagueManager.Application.Dtos;
using LeagueManager.Domain.Models;

namespace LeagueManager.Application.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Player, PlayerResponseDto>()
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : string.Empty));
        CreateMap<PlayerDto, Player>();

        CreateMap<Location, LocationResponseDto>();
        CreateMap<LocationDto, Location>();

        CreateMap<Team, TeamResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<CreateTeamDto, Team>();

        CreateMap<Fixture, FixtureResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Result, ResultResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<MOMVote, MomVoteResponseDto>()
            .ForMember(dest => dest.VotingTeamName, opt => opt.MapFrom(src => src.VotingTeam != null ? src.VotingTeam.Name : string.Empty))
            .ForMember(dest => dest.VotedForOwnPlayerName, opt => opt.MapFrom(src => src.VotedForOwnPlayer != null ? src.VotedForOwnPlayer.Name : string.Empty))
            .ForMember(dest => dest.VotedForOpponentPlayerName, opt => opt.MapFrom(src => src.VotedForOpponentPlayer != null ? src.VotedForOpponentPlayer.Name : string.Empty));
    }
}
