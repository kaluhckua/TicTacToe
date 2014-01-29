using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TicTacToe.Models
{
    public class ServerErrorException:Exception
    {

        public string ErrorCode { get; set; }
          public ServerErrorException() : base()
        {
        }

        public ServerErrorException(string msg) : base(msg)
    {
        }

        public ServerErrorException(string msg, string errCode): base(msg)
        {
            this.ErrorCode = errCode;
        }

       
    }
}