using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WormRace
{
    public class Bet
    {
        public Player? Player { get; set; }
        public Worm? Worm { get; set; }
        public int Money { get; set; }

        public Bet(Player player, Worm? worm, int money) 
        {
            this.Player = player;
            this.Worm = worm;
            this.Money = money;
        }
    }
}
