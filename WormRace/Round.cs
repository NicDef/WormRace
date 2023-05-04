using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WormRace
{
    public class Round
    {
        public Odd[]? Odds { get; set; }
        public Bet[]? Bets { get; set; }
        private readonly ILogger _logger;

        public event EventHandler<TextOutputEventArgs>? TextOutput;

        public Round(Odd[] odds, Bet[] bets, ILogger logger)
        {
            this.Odds = odds;
            this.Bets = bets;
            _logger = logger;
        }

        public class TextOutputEventArgs : EventArgs
        {
            public string? Text { get; set; }
        }

        public int Start(Worm[] worms, SetLines setLinesDelegate, int distance = 50)
        {
            // TODO: Delegate UI Setup
            Random random = new();
            int[] position = new int[worms.Length];
            int delay = 10;

            int wormIndex;

            int[] lines = setLinesDelegate(worms.Length);

            // Setup UI
            string WORM_STRING_1 = @"__/\__0-";
            string WORM_STRING_2 = @"______0-";

            Console.Clear();
            Console.CursorVisible = false;
            Console.Write(new String(' ', Console.BufferWidth));
            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = false;

            this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = "╔══════════════════════════╗" });
            this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = "║ Das Wurm-Rennen startet! ║" });
            this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = "╚══════════════════════════╝" });

            _logger.LogDebug("Runde gestartet");

            for (int yPos = lines[0]; yPos <= lines[^1]; yPos++)
            {
                if (!lines.Contains(yPos))
                {
                    Console.SetCursorPosition(distance, yPos);
                    Console.WriteLine("|");
                }
                else
                {
                    Console.SetCursorPosition(0, yPos);
                    wormIndex = Array.IndexOf(lines, yPos);
                    Console.ForegroundColor = worms[wormIndex].Color;
                    Console.Write(WORM_STRING_1);

                    Console.SetCursorPosition(distance, yPos);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("|");
                }
            }

            // Let the worms run
            do
            {
                wormIndex = random.Next(0, worms.Length);
                int line = lines[wormIndex];
                ConsoleColor color = worms[wormIndex].Color;

                position[wormIndex]++;

                if (position[wormIndex] >= distance - WORM_STRING_1.Length)
                    break;

                Console.SetCursorPosition(0, line);
                Console.Write(new String(' ', Console.BufferWidth));

                Console.SetCursorPosition(position[wormIndex], line);
                Console.ForegroundColor = color;

                if (position[wormIndex] % 2 == 0)
                    Console.WriteLine(WORM_STRING_1);
                else
                    Console.WriteLine(WORM_STRING_2);

                Console.SetCursorPosition(position[wormIndex] + (distance - position[wormIndex]), line);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("|");

                //Console.Beep(64, 10);

                Thread.Sleep(delay);
            } while (true);

            // Play victory sound
            int[] victorySound = { 440, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400 };
            foreach (int frequency in victorySound)
            {
                Console.Beep(frequency, 100);
                Thread.Sleep(50);
            }

            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = true;

            int winningWormIndex = wormIndex;

            return winningWormIndex;
        }

        public void DistributeMoney(int wormIndex, Player[] players, Worm[] worms)
        {
            List<List<Player[]>> correctBet = new List<List<Player[]>>();
            for (int i = 0; i < players.Length; i++)
            {
                if (this.Bets == null || this.Odds == null)
                    throw new NullReferenceException();

                string playerBet = this.Bets[i].Worm.Name.ToString();
                int winningFactor = Odds[wormIndex].WinningFactor;

                if (worms[wormIndex].Name == playerBet)
                    this.Bets[i].Player.AddMoney(winningFactor * this.Bets[i].Player.LastBet);

                Console.WriteLine("Hier sind die Zwischenstände:");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Spieler {players[i].Name} hat {players[i].Money}$."); 
                _logger.LogDebug($"Spieler {players[i].Name} hat {players[i].Money}$.");
            }

            Thread.Sleep(3000);
            Console.Clear();
        }      
    }
}
