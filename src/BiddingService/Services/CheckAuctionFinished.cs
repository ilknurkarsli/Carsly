
using MassTransit;
using MongoDB.Entities;
using BiddingService.Models;
using Contracts;

namespace BiddingService
{
    public class CheckAuctionFinished : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting check for finished auctions");
            stoppingToken.Register(() => _logger.LogInformation("Auction check is stopping"));
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAuctions(stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task CheckAuctions(CancellationToken stoppingToken)
        {
            var finishedAuctions = await DB.Default.Find<Auction>()
                .Match(x=>x.AuctionEnd <= DateTime.UtcNow)
                .Match(x=> !x.Finished)
                .ExecuteAsync(stoppingToken);
            
            if (finishedAuctions.Count== 0) return;

            _logger.LogInformation("==> Found {Count} finished auctions", finishedAuctions.Count);

            using var scope = _services.CreateAsyncScope();
            var endPoint =scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            foreach(var auction in finishedAuctions)
            {
                auction.Finished = true;
                await DB.Default.SaveAsync(auction, cancellation: stoppingToken);

                var winningBid = await DB.Default.Find<Bid>()
                    .Match(a=>a.AuctionId == auction.ID)
                    .Match(a=>a.BidStatus == BidStatus.Accepted)
                    .Sort(s=>s.Descending(a=>a.Amount))
                    .ExecuteFirstAsync(stoppingToken);

                await endPoint.Publish(new AuctionFinished
                {
                  ItemSold = winningBid != null,
                  AuctionId = auction.ID,
                  Winner = winningBid?.Bidder,
                  Amount = winningBid?.Amount,
                  Seller = auction.Seller
                }, stoppingToken);
            }

        }
    }
}