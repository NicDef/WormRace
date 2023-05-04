using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WormRace
{
    public class Odd
    {
        public Worm? Worm { get; set; }
        public int WinningFactor { get; set; }

        public Odd(Worm worm, int winningFactor)
        {
            this.Worm = worm;
            this.WinningFactor = winningFactor;
        }

        public void SetWinningFactor()
        {
            Random random = new();
            this.WinningFactor = random.Next(1, 5);
        }
    }
}
