using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace touti_game_logic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting matchmaking...");
            var matchMaking = new MatchMaking();
            matchMaking.StartClient();

            Console.ReadLine(); // Keep the console open
        }
    }
}
