namespace WormRace
{
    public class Worm
    {
        public string Name { get; set; }
        public ConsoleColor Color { get; set; }

        public Worm(string name, ConsoleColor color) 
        {
            this.Name = name;
            this.Color = color;
        }
    }
}