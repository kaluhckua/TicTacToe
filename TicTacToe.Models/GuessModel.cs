using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacToe.Models
{
    public class GuessModel
    {
        public string SessionKey { get; set; }
        public int GameId { get; set; }

        public int Position { get; set; }
        public GuessModel(string  sessionKey,int gameId,int position)
        {
            this.SessionKey = sessionKey;
            this.Position = position;
            this.GameId = gameId;
        }
       

    }
}
