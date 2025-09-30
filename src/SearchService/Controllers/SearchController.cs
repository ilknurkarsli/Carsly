using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
        {
            // first we create the query, which is going to be a page search.
            var query = DB.PagedSearch<Item, Item>();

            //check to see if we've got a search term. If we do, we match it against the full text index, and sort by text score.
            query.Sort(x => x.Ascending(a => a.Make));
            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            //Kullanıcının istediği sıralamayı uygula, yoksa varsayılan olarak AuctionEnd tarihine göre sırala.
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd)),
            };
            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6)&& x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow),
            };
            if(!string.IsNullOrEmpty(searchParams.Seller))
            {
                query = query.Match(x => x.Seller == searchParams.Seller);
            }
            if(!string.IsNullOrEmpty(searchParams.Winner))
            {
                query = query.Match(x => x.Winner == searchParams.Winner);
            }
            query.PageNumber (searchParams.PageNumber);
            query.PageSize (searchParams.PageSize);
            var result = await query.ExecuteAsync();
            return Ok(new { results = result.Results, pageCount =result.PageCount, totalCount = result.TotalCount});
        } 
    }
    
}