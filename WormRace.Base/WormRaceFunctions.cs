using System.Data.SqlClient;

namespace WormRace.Base;

public class WormRaceFunctions
{
    public static string[] CreatePlayers()
    {
        string[] playerNames;
        do
        {
            Console.Write("Wie viele Spieler sind Sie? ");

            string? input = Console.ReadLine();
            if (!Int32.TryParse(input, out int playerAmmount))
                throw new ArgumentException("Bitte geben Sie eine Zahl ein!");

            if (playerAmmount < 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Sie müssen mindestens zu zweit sein!");
                Console.ForegroundColor = ConsoleColor.White;
                continue;
            }

            playerNames = new string[playerAmmount];
            for (int i = 0; i < playerAmmount; i++)
            {
                do
                {
                    Console.Write($"Spieler {i + 1}, wie lauet Ihr Name? ");
                    string? name = Console.ReadLine();

                    if (name != null && name.Trim() != String.Empty && !playerNames.Contains(name.Trim()))
                        playerNames[i] = name;
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ungültiger Name!");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    break;
                } while (true);
            }

            Console.Clear();
            break;
        } while (true);

        return playerNames;
    }

    public static string[] CreateWorms()
    {
        string[] wormNames;
        do
        {
            Console.Write("Wie viele Würmer brauchen Sie zum Spielen? ");

            string? input = Console.ReadLine();

            if (!Int32.TryParse(input, out int wormAmmount))
                throw new ArgumentException();

            if (wormAmmount < 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ihr braucht mindestens 2 Würmer!");
                Console.ForegroundColor = ConsoleColor.White;
                continue;
            }
            else if (wormAmmount > 10)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Leider können wir Ihnen nicht so viele Würmer bereitstellen!");
                Console.ForegroundColor = ConsoleColor.White;
                continue;
            }

            wormNames = new string[wormAmmount];
            for (int i = 0; i < wormAmmount; i++)
            {
                do
                {
                    Console.Write($"Wie lautet der Name von Wurm {i + 1}? ");
                    string? name = Console.ReadLine();

                    if (name != null && name.Trim() != String.Empty && !wormNames.Contains(name.Trim()))
                        wormNames[i] = name;
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ungültiger Name!");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    break;
                } while (true);
            }

            Console.Clear();
            break;
        } while (true);

        return wormNames;
    }

    public static string GetConnectionString()
    {
        var str = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;Connect Timeout=2";
        SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
        sb.ConnectionString = str;
        sb.ApplicationName = "AdoNet";
        sb.MinPoolSize = 10;
        return sb.ToString();
    }
}