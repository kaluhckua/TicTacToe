using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Models
{
    public class Game
    {
        public Game()
        {
            this.Guesses = new HashSet<Guess>();          

        }
         
        public long Id { get; set; }
        public string Title { get; set; }
        public string Password { get; set; }

        public int MovesLeft { get; set; }

        public Nullable<long> UserInTurn { get; set; }
        public long RedUserId { get; set; }
        public Nullable<long> BlueUserId { get; set; }

         public Nullable<long> WinnerId { get; set; }

        public virtual User Winner { get; set; }
        public virtual User BlueUser { get; set; }
        public virtual User RedUser { get; set; }


        public string GameStatus { get; set; }
        public virtual ICollection<Guess> Guesses { get; set; }
      




    }
}
