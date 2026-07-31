using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using Grpc.Core;

namespace AuctionService.Services
{
    public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
    {
        private readonly AuctionDbContext _dbcontext;

        public GrpcAuctionService(AuctionDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public override async Task<GrpcAuctionResponse> GetAuction (GetAuctionRequest request, ServerCallContext context)
        {
            Console.WriteLine("===> Recieved GRPC request for auction");

            var auction = await _dbcontext.Auctions.FindAsync(Guid.Parse(request.Id)) ?? throw new RpcException(new Status(StatusCode.NotFound, $"Auction with id {request.Id} not found"));

            var response = new GrpcAuctionResponse
            {
                Auction = new GrpcAuctionModel
                {
                    AuctionEnd =auction.AuctionEnd.ToString(),
                    Id = auction.Id.ToString(),
                    ReservePrice = auction.ReservePrice,
                    Seller = auction.Seller,
                }
            };
            return response;
        }
    }
}