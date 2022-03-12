using System;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DuelBot
{
    public interface IDiscordMessageListener : IDisposable
    {
        void Tick(double now);
        void RecvMessage(SocketMessage socketMessage);
    }

    public class DuelBot : IDiscordMessageListener
    {
        private DiscordClient _client;

        public DuelBot(IServiceProvider serviceProvider)
        {
            _client = serviceProvider.GetRequiredService<DiscordClient>();
            _client.AddListenr(this);
        }

        public void Tick(double now)
        {

        }

        public void RecvMessage(SocketMessage socketMessage)
        {
            Console.WriteLine(socketMessage);
        }

        public void Dispose()
        {
            _client = null;
        }
    }
}