using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public AuctionFinishedConsumer(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            //When we receive auction finished events, we send a message to all connected clients via SignalR
            Console.WriteLine("--> auction finished event received ");
            await _hubContext.Clients.All.SendAsync("AuctionFinished", context.Message);
        }
    }
}