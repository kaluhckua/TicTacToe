using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Models
{
    public class UserLoginModel
    {
        public string Username { get; set; }
        public string AuthCode { get; set; }
        public string ConnectionId { get; set; }
    }

    public class UserRegisterModel : UserLoginModel
    {
        public string Nickname { get; set; }
    }
    public class UserLoggedModel
    {

        public string SessionKey { get; set; }


        public string Nickname { get; set; }
    }
}
