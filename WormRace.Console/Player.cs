using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WormRace
{
    public class Player
    {
        public int Money {  get; private set; }
        public string? Name { get; private set; }
        public int LastBet { get; private set; }

        public Player(string name, int money)
        {
            this.Money = money;
            this.Name = name;
        }

        public void AddMoney(int money)
        {
            this.Money += money;
        }

        public void RemoveMoney(int money)
        {
            this.Money -= money;
            this.LastBet = money;
        }
    }
}
