using System;
using Microsoft.AspNet.SignalR;
using MemoriesCollection.Function.Common;

namespace MemoriesCollection.Hubs
{
    public class ProgressHub : Hub
    {
        public int count = 0;

        public static void SendMessage(double count, double diff = 0)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            count = count > 100 ? 100 : count;
            hubContext.Clients.All.sendMessage(string.Format("{0}", count.ToString("0.00")));
        }

    }
}