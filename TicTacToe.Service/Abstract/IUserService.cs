using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Service.Abstract
{
    public interface IUserService
    {

        void CreateUser(UserRegisterModel registerUser);
        void SaveConnectionId(string sessionKey, string connectionId);
        string LoginUser(UserLoginModel userLogin, out string nickname);
        int LoginUser(string sessionKey);
        void LogoutUser(string sessionKey);       
        IEnumerable<User> GetAllUsers();

        User GetUserBySessionKey(string sessionKey);
       


    }
}
