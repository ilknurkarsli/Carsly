using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers
{
    public class MapppingProfiles : Profile
    {
        public MapppingProfiles()
        {
            CreateMap<Auction, AuctionDto>().IncludeMembers(s => s.Item);
            CreateMap<Item, AuctionDto>().IncludeMembers();
            CreateMap<CreateAutionDto, Auction>().ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<CreateAutionDto, Item>();

        }
    }
}