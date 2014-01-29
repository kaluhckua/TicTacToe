using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Models;
using TicTacToe.Service.Abstract;
using TicTacToe.DataLayer;
using TicTacToe.Repository;

namespace TicTacToe.Service.Concrete
{
    public class UserService : BaseService, IUserService
    {
        private const string ValidUsernameChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM_1234567890";
        private const string ValidNicknameChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM_1234567890 -";
        private const int MinUsernameNicknameChars = 4;
        private const int MaxUsernameNicknameChars = 30;

        public UserService(IUowData data)
            : base(data)
        {
        }

        public void CreateUser(UserRegisterModel registerUser)
        {
            if (registerUser == null)
            {
                throw new ArgumentNullException("UserRegisteModel is null");
            }
            ValidateUsername(registerUser.Username);
            ValidateNickname(registerUser.Nickname);
            ValidateAuthCode(registerUser.AuthCode);

            var usernameToLower = registerUser.Username.ToLower();
            var nickmameToLower = registerUser.Nickname.ToLower();

            var user = this.Data.Users.GetAll().FirstOrDefault(u => u.Username.ToLower() == usernameToLower || u.Nickname.ToLower() == nickmameToLower);
            if (user != null)
            {
                if (user.Username.ToLower() == usernameToLower)
                {
                    throw new ServerErrorException("Username already exists", "ERR_DUP_USR");
                }
                if (user.Nickname.ToLower() == nickmameToLower)
                {
                    throw new ServerErrorException("Nickname already exists", "ERR_DUP_NICK");
                }
            }
            User newUser = new User()
            {
                Username = registerUser.Username,
                Nickname = registerUser.Nickname,
                AuthCode = registerUser.AuthCode,
                ConnectionId = registerUser.ConnectionId,
            };
            this.Data.Users.Add(newUser);
            this.Data.SaveChanges();
        }

        public void SaveConnectionId(string sessionKey, string connectionId)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException("ConnectionId is null");
            }
            User user = GetUserBySessionKey(sessionKey);
            user.ConnectionId = connectionId;
            Data.Users.Update(user);
            this.Data.SaveChanges();
        }

        public string LoginUser(UserLoginModel userLogin, out string nickname)
        {
            if (userLogin == null)
            {
                throw new ArgumentNullException("UserLoginModel is null");
            }
            ValidateUsername(userLogin.Username);
            ValidateAuthCode(userLogin.AuthCode);
            var userNameToLower = userLogin.Username.ToLower();
            var user = this.Data.Users.GetAll().Where(u => u.Username.ToLower() == userNameToLower && u.AuthCode == userLogin.AuthCode).FirstOrDefault();
            if (user == null)
            {
                throw new ServerErrorException("Invalid user authentication", "INV_USR_AUTH");
            }
            nickname = user.Nickname;
            var sessionKay = GenerateSessionKey((int)user.Id);
            user.SessionKey = sessionKay;
            user.ConnectionId = userLogin.ConnectionId;
            Data.Users.Update(user);
            this.Data.SaveChanges();
            return sessionKay;
        }

        public int LoginUser(string sessionKey)
        {
            var user = GetUserBySessionKey(sessionKey);
            return (int)user.Id;
        }

        public void LogoutUser(string sessionKey)
        {
            var user = GetUserBySessionKey(sessionKey);
            user.SessionKey = null;
            this.Data.Users.Update(user);
            this.Data.SaveChanges();
        }

        public IEnumerable<User> GetAllUsers()
        {
            return this.Data.Users.GetAll();
        }

        public static void ValidateUsername(string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException("Username is null");
            }
            if (username.Length < MinUsernameNicknameChars || username.Length > MaxUsernameNicknameChars)
            {
                throw new ServerErrorException("Username should be between 4 and 30 symbols long", "INV_USRNAME_LEN");
            }
            else if (username.Any(ch => !ValidUsernameChars.Contains(ch)))
            {
                throw new ServerErrorException("Username contains invalid characters", "INV_USRNAME_CHARS");
            }
        }

        public static void ValidateNickname(string nickname)
        {
            if (nickname == null)
            {
                throw new ArgumentNullException("Nickname is null");
            }
            if (nickname.Length < MinUsernameNicknameChars || nickname.Length > MaxUsernameNicknameChars)
            {
                throw new ServerErrorException("Nickname should be between 4 and 30 symbols long", "INV_NICK_LEN");
            }
            else if (nickname.Any(ch => !ValidNicknameChars.Contains(ch)))
            {
                throw new ServerErrorException("Nickname contains invalid characters", "INV_NICK_CHARS");
            }
        }

        public static void ValidateAuthCode(string authCode)
        {
            if (authCode == null)
            {
                throw new ArgumentNullException("AuthCode is null");
            }
            if (authCode.Length != Sha1CodeLength)
            {
                throw new ServerErrorException("Invalid authentication code length", "INV_USR_AUTH_LEN");
            }
        }
    }
}