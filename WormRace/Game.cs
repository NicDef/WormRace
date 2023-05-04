using Microsoft.Extensions.Logging;
using NLog;
using System.Data.SqlClient;
using WormRace.Base;

namespace WormRace
{
    public delegate Bet CreateBet(Player player, Worm[] worms);

    public delegate int[] SetLines(int wormsAmmount);

    public class Game
    {
        public Player[]? Players { get; set; }
        public Worm[]? Worms { get; set; }

        public event EventHandler<TextOutputEventArgs>? TextOutput;

        private CreateBet _createBetDelegate;

        private SetLines _setLinesDelegate;

        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        private int _targetAmmount;

        private int _gameId;

        public class TextOutputEventArgs : EventArgs
        {
            public string? Text { get; set; }
        }

        public Game(CreateBet chooseWormDelegate, SetLines setLinesDelegate, ILogger<Game> logger, int gameId, int targetAmmount)
        {
            _createBetDelegate = chooseWormDelegate;
            _setLinesDelegate = setLinesDelegate;
            _logger = logger;
            _targetAmmount = targetAmmount;
            _gameId = gameId;
        }

        public void Start()
        {
            if (Players == null || Worms == null)
                throw new NullReferenceException("Spieler oder Würmer sind null");
            _logger.LogInformation("Game started");

            List<Player> winningPlayers = new List<Player>();
            bool gameOver = false;
            do
            {
                // Remove players that are bankrupt
                for (int i = 0; i < this.Players.Length; i++)
                    if (this.Players[i].Money <= 0)
                        this.Players = this.Players.Where(val => val != this.Players[i]).ToArray();

                winningPlayers = PlayerReachedTarget(this.Players, _targetAmmount);

                // If no more players exist or one player has reached a certain amount of money
                if (this.Players != null && this.Players.Length > 1 && winningPlayers.Count == 0)
                {
                    Round round = ConfigureRound(_setLinesDelegate);
                    int winner = round.Start(Worms, _setLinesDelegate);
                    _logger.LogDebug($"Gewinner: Wurmindex {winner}");
                    round.DistributeMoney(winner, Players, Worms);
                } else
                    gameOver = true;
            } while (!gameOver);

            this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = "Das Spiel ist vorbei!" });

            var cnnStr = WormRaceFunctions.GetConnectionString();
            var cnn = new SqlConnection(cnnStr);

            // Output winners
            if (this.Players == null)
            {
                this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = "Keiner hat gewonnen. Alle sind Bankrott!" });
            }
            else if (this.Players.Length == 1)
            {
                this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = $"Herzlichen Glückwunsch {this.Players[0].Name} Sie haben gewonnen, da Sie die einzige Person sind die nicht bankrott ist!" });
                
                if (this.Players[0].Money >= _targetAmmount)
                    this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = $"Und außerdem haben Sie das Ziel von {_targetAmmount}$ erreicht!" });

                try
                {
                    cnn.Open();
                    var sqlCmd = cnn.CreateCommand();
                    sqlCmd.CommandText = $"UPDATE Worm_Race SET Winner = {this.Players[0].Name};";
                    using (var cmd = sqlCmd.ExecuteReader())

                        _logger.LogInformation($"Erfolgreiches Einfügen von Daten in die Datenbank");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Datenbankfehler: {ex.Message}");
                }
            }
            else if (this.Players.Length > 1)
            {
                foreach (Player player in winningPlayers)
                {
                    this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = $"{player.Name} hat gewonnen!" });

                    try
                    {
                        cnn.Open();
                        var sqlCmd = cnn.CreateCommand();
                        sqlCmd.CommandText = $"UPDATE Worm_Race SET Winner = {this.Players[0].Name} WHERE Game_Id = {_gameId};";
                        using (var cmd = sqlCmd.ExecuteReader())

                            _logger.LogInformation($"Erfolgreiches Einfügen von Daten in die Datenbank");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Datenbankfehler: {ex.Message}");
                    }
                }
            }       
        }

        public List<Player> PlayerReachedTarget(Player[] players, int targetAmmount)
        {
            List<Player> winningPlayers = new(); 
            for (var i = 0; i < players.Length; i++)
                if (players[i].Money >= targetAmmount)
                    winningPlayers.Add(players[i]);

            return winningPlayers;
        }

        public void Configure(string[] playerNames, string[] wormNames)
        {
            if (playerNames == null || wormNames == null)
                throw new ArgumentException("Spieler oder Würmer sind null");

            Random random = new();
            ConsoleColor[] colors = new ConsoleColor[] { ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Cyan, ConsoleColor.Magenta }; // TODO

            Players = new Player[playerNames.Length];
            Worms = new Worm[wormNames.Length];

            for (int i = 0; i < playerNames.Length; i++)
                Players[i] = new Player(playerNames[i], 1000);

            for (int i = 0; i < wormNames.Length; i++)
                Worms[i] = new Worm(wormNames[i], colors[random.Next(0, colors.Length - 1)]);
        }

        public Round ConfigureRound(SetLines SetLinesDelegate)
        {
            if (this.Worms == null || this.Players == null)
                throw new NullReferenceException();

            // Display name
            this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = "Hier sind die Verfielfachungsfaktoren der Würmer!\n" });

            for (int i = 0; i < this.Worms.Length; i++)
            {
                Console.ForegroundColor = this.Worms[i].Color;
                Console.Write($"{this.Worms[i].Name}\t");
            }

            Console.ForegroundColor = ConsoleColor.White;
            this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = "\n" });

            // Create Odds
            Odd[] odds = new Odd[this.Worms.Length];
            Random random = new();
            for (int i = 0; i < this.Worms.Length; i++)
            {
                odds[i] = new Odd(this.Worms[i], random.Next(1, 5));
                odds[i].SetWinningFactor();
                // Display odds
                Console.Write($"x{odds[i].WinningFactor}\t");
            }

            this.TextOutput?.Invoke(this, new TextOutputEventArgs { Text = "\n" });

            // Let players bet
            Bet[] bets = new Bet[this.Players.Length];
            for (int i = 0; i < this.Players.Length; i++)
            {
                Bet playersBet = _createBetDelegate(this.Players[i], this.Worms);             
                bets[i] = playersBet;
            }

            // Return new round
            return new Round(odds, bets, _logger);
        }
    }
}
