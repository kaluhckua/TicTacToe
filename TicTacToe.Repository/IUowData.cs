using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Repository
{
    public interface IUowData
    {
        IRepository<Game> Games { get; }
        IRepository<Guess> Guesses { get; }
        IRepository<User> Users { get;  }        

        int SaveChanges();
    }
}
