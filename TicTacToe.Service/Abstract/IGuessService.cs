using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Service.Abstract
{
    public interface IGuessService
    {
        Game MakeGuess(GuessModel guessModel, out GameState gameState);
    }
}
