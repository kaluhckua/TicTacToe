using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacToe.Models
{
    public class Guess
    {
        public long Id { get; set; }
      
        public Nullable<long> GameId { get; set; }
        public int Position { get; set; }

        public virtual Game Game { get; set; }
        public virtual User User { get; set; }

    }
}
