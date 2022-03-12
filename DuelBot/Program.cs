using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DuelBot
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var task = MainAsync();
            var awaiter = task.GetAwaiter();
            awaiter.GetResult();
        }

        private static async Task MainAsync()
        {
            Console.WriteLine("\t   +++++++++++++++++++++++++++   ");
            Console.WriteLine("\t :+++++++++++++++++++++++++++++: ");
            Console.WriteLine("\t-+++++++++++++++++++++++++++++++-");
            Console.WriteLine("\t:+++++++++++++++++++++++++++++++:");
            Console.WriteLine("\t:+++++++++++++++++++++++++++++++:");
            Console.WriteLine("\t:++++++++/:---:--------:++++++++:");
            Console.WriteLine("\t:+++++++`               `/++++++:");
            Console.WriteLine("\t:++++++.                 `++++++:");
            Console.WriteLine("\t:+++++:     .-`   `--     -+++++:");
            Console.WriteLine("\t:+++++`    :+++   +++:     +++++:");
            Console.WriteLine("\t:+++++     `--`   `--`     /++++:");
            Console.WriteLine("\t:+++++`   `..``    `...`  `/++++:");
            Console.WriteLine("\t:++++++/-.../++++++++.`.-/++++++:");
            Console.WriteLine("\t:+++++++++++++++++++++++++++++++:");
            Console.WriteLine("\t:+++++++++++++++++++++++++++++++:");
            Console.WriteLine("\t-+++++++++++++++++++++++:+++++++:");
            Console.WriteLine("\t -/+++++++++++++++++++++: -/++++:");
            Console.WriteLine("\t                            ./++:");
            Console.WriteLine("\t                              .::");

            using var serviceProvider = ConfigureService();
            {
                var discordClient = serviceProvider.GetRequiredService<DiscordClient>();
                var duelBot = serviceProvider.GetRequiredService<DuelBot>();

                await discordClient.Start();

                await discordClient.Tick();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private static ServiceProvider ConfigureService()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordClient())
                .AddSingleton<DuelBot>()
                .BuildServiceProvider();
        }
    }
}
