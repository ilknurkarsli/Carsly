using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AutionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionsController(AutionDbContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            var auctions = await _context.Auctions.Include(a => a.Item).OrderBy(x => x.Item.Make).ToListAsync();
            return _mapper.Map<List<AuctionDto>>(auctions);

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null) return NotFound();
            return _mapper.Map<AuctionDto>(auction);
        }
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAutionDto createAutionDto)
        {
            var auction = _mapper.Map<Auction>(createAutionDto);

            //Todo: add current user as a seller 
            auction.Seller = "testuser";

            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Failed to save changes to auction database");
            return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));

        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null) return NotFound();

            //Todo: check seller == username
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();
            return BadRequest("Failed saving changes to auction database");

        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();
            
            //todo: check seller == username
            _context.Auctions.Remove(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();
            return BadRequest("Failed to delete the auction from database");

        }
    }
}