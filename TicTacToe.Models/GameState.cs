using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Models
{
    public class GameState
    {
        public int GameId { get; set; }
        public string State { get; set; }
        public string Winner { get; set; }
        public bool gameOver { get; set; }
        public string Title { get;set; }
        public string  Opponent { get; set; }
        public string Symbol { get; set; }
        public int[] X { get; set; }
        public int[] O { get; set; }
    }
}
