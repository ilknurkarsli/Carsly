using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            this._publishEndpoint = publishEndpoint;
            this._context = context;
            this._mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
            if(!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }
            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null) return NotFound();
            return _mapper.Map<AuctionDto>(auction);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAutionDto createAutionDto)
        {
            var auction = _mapper.Map<Auction>(createAutionDto);

            auction.Seller = User.Identity.Name;

            _context.Auctions.Add(auction);

            //MassTransit kullanarak “AuctionCreated” adlı bir event gönderiyoruz
            //Yeni bir auction oluşturuldu!” bilgisini sistemin diğer mikroservislerine yani SearchService e gönderiyoruz.
            var newAuction = _mapper.Map<AuctionDto>(auction);
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Failed to save changes to auction database");
            return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, newAuction);

        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            //MassTransit kullanarak “AuctionUpdated” adlı bir event gönderiyoruz
            //Bir auction güncellendi!” bilgisini sistemin diğer mikroservislerine yani SearchService e gönderiyoruz.
            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();
            return BadRequest("Failed saving changes to auction database");

        }
        
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();

            if(auction.Seller != User.Identity.Name) return Forbid();
            _context.Auctions.Remove(auction);

            //MassTransit kullanarak “AuctionDeleted” adlı bir event gönderiyoruz
            //Bir auction silindi!” bilgisini sistemin diğer mikroservislerine yani SearchService
            await _publishEndpoint.Publish<AuctionDeleted>( new { Id = auction.Id.ToString() });
            
            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();
            return BadRequest("Failed to delete the auction from database");

        }
    }
}