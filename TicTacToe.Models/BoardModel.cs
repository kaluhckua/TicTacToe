using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Models
{
    public class BoardModel
    {
        public string  Symbol { get; set; }
        public int[] OPosition { get; set; }
        public int[] XPosition { get; set; }
    }
}
