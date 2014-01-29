using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace TicTacToe.Models
{
    public class User
    {
        public User()
        {
         
            this.Guesses = new HashSet<Guess>();           
       
        }

        public long Id { get; set; }

       
        public string Username { get; set; }   

        
        public string Nickname { get; set; }

        
        public string AuthCode { get; set; }

        
        public string SessionKey { get; set; }

      
        public string  ConnectionId { get; set; }

        public long? Score { get; set; }

      
        public virtual ICollection<Guess> Guesses { get; set; }
       
     
    }
}
