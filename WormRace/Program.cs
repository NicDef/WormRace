using System;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Data.SqlClient;
using WormRace.Base;
using static WormRace.Game;

namespace WormRace
{
    public class Program
    {
        static void Main(string[] args)
        {
            var cnnStr = WormRaceFunctions.GetConnectionString();
            var cnn = new SqlConnection(cnnStr);

            int gameId = -1;

            // Setup logger
            ILoggerFactory loggerFactory = LoggerFactory.Create(lb =>
            {
                lb.AddDebug();
                lb.AddNLog();
                lb.SetMinimumLevel(LogLevel.Debug);
            });
            ILogger logger = loggerFactory.CreateLogger("main");
            logger.LogInformation("Programm gestartet");

            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = true;
            Console.WriteLine("╔════════════════════════════╗");
            Console.WriteLine("║ Willkommen zum Wurm-Rennen ║");
            Console.WriteLine("╚════════════════════════════╝");

            string[] playerNames = WormRaceFunctions.CreatePlayers();
            string[] wormNames = WormRaceFunctions.CreateWorms();

            // Insert into database
            using (cnn)
            {
                try
                {
                    cnn.Open();
                    var sqlCmd = cnn.CreateCommand();
                    sqlCmd.CommandText = $"INSERT INTO Worm_Race (Players, Worms) VALUES ({playerNames.Length}, {wormNames.Length}); SELECT TOP 1 Game_Id FROM Worm_Race ORDER BY ID DESC";
                    sqlCmd.CommandTimeout = 20;
                    using (var cmd = sqlCmd.ExecuteReader())
                    {
                        while (cmd.Read())
                        {
                            var res = cmd.GetSqlInt32(0);
                            if (!res.IsNull) gameId = (int)res;
                        }
                    }

                    logger.LogInformation($"Erfolgreiches Einfügen von Daten in die Datenbank");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Datenbankfehler: {ex.Message}");
                }
            }

            // Create game
            Game game = new(CreateBet, SetLines, loggerFactory.CreateLogger<Game>(), gameId, 10000);
            game.TextOutput += TextOutput;
            try
            {
                game.Configure(playerNames, wormNames);
                game.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void TextOutput(object? sender, TextOutputEventArgs e)
        {
            Console.WriteLine(e.Text);
        }

        private static Bet CreateBet(Player player, Worm[] worms)
        {
            if (player.Money <= 0)
                throw new ArgumentException($"Spieler {player.Name} ist bankrott");

            Worm playersWorm = _ChooseWorm(player, worms);
            int playersBet = _SetBet(player, worms);

            return new Bet(player, playersWorm, playersBet);
        }

        private static Worm _ChooseWorm(Player player, Worm[] worms)
        {
            do
            {
                Console.Write($"{player.Name}, auf welchen Wurm möchten Sie wetten? ");
                string? input = Console.ReadLine();

                string[] names = new string[worms.Length];

                for (int j = 0; j < worms.Length; j++)
                    names[j] = worms[j].Name.ToLower();

                if (input != null && names.Contains(input.ToLower()))
                    for (int k = 0; k < worms.Length; k++)
                        if (worms[k].Name == input)
                            return worms[k];

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Geben Sie einen gültigen Namen ein!");
                Console.ForegroundColor = ConsoleColor.White;
            } while (true);
        }
        
        private static int _SetBet(Player player, Worm[] worms)
        {
            do
            {
                Console.Write($"{player.Name}, wie viel von Ihren {player.Money}$ möchten Sie wetten? ");
                string? input = Console.ReadLine();

                Int32.TryParse(input, out int playersBet);

                if (playersBet > player.Money || playersBet <= 0)
                {
                    if (playersBet >= player.Money)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Zu großer Einsatz!");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }
                    else if (playersBet <= 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Zu kleiner Einsatz!");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }
                }

                player.RemoveMoney(playersBet);

                return playersBet;
            } while (true);
        }

        private static int[] SetLines(int wormsAmmount)
        {
            int[] lines = new int[wormsAmmount];
            int j = 5;
            for (int i = 0; i < wormsAmmount; i++)
            {
                j += 3;
                lines[i] = j;
            }

            return lines;
        }
    }
}