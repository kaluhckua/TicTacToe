using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacToe.Models
{
    public class JoinGameModel
    {
        public int GameId { get; set; }
        public string  Password { get; set; }
        public string SessionKey { get; set; }
    }
}
