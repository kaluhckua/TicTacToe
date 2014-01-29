using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.DataLayer;
using TicTacToe.Models;
using TicTacToe.Repository;

namespace TicTacToe.Service.Abstract
{
    public  class BaseService
    {
        private const string SessionKeyChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const int SessionKeyLen = 50;

        protected IUowData Data { get; set; }
    
        public BaseService(IUowData data)
        {
            this.Data = data;           
        }

        protected  const int Sha1CodeLength = 40;
       
        public static Random rand = new Random();
        public  User GetUserBySessionKey(string sessionKey)
        {
            ValidateSessionKey(sessionKey);
            User user = Data.Users.GetAll().Where(u => u.SessionKey == sessionKey).FirstOrDefault();
            if (user == null)
            {
                throw new ServerErrorException("Invalid user authentication", "INV_USR_AUTH");
            }
            return user;
        }
        public Game GetGameById(int gameId)
        {
            if(gameId<0)
            {
                throw new ArgumentException("Game id must be a positive number");
            }
            Game game = Data.Games.GetById(gameId);
            if (game == null)
            {
                throw new ServerErrorException("Invalid game", "ERR_INV_GAME");
            }
            return game;
        }
        public User GetAnotherOpponentInGame(int gameId, string sessionKey)
        {
            User user = GetUserBySessionKey(sessionKey);
            Game game = Data.Games.GetById(gameId);
            ValidateUserInGame(game, user);
            if (game.RedUserId == user.Id)
            {
                return game.BlueUser;
            }
            return game.RedUser;

        }
        public static void ValidateUserInGame(Game game,User user)
        {
            if(game==null||user==null)
            {
                throw new ArgumentNullException("Game or user is null");
            }
            if (game.RedUserId != user.Id && game.BlueUserId != user.Id)
            {
                throw new ServerErrorException("User not in game", "INV_GAME_USR");
            }
        }
        public static void ValidateSessionKey(string sessionKey)
        {
            if(sessionKey==null)
            {
                throw new ArgumentNullException("SessionKey is null");
            }
           
            if (sessionKey.Length != SessionKeyLen || sessionKey.Any(ch => !SessionKeyChars.Contains(ch)))
            {
                throw new ServerErrorException("Invalid sessionKey", "ERR_INV_AUTH");
            }
        }
        public static string GenerateSessionKey(int userId)
        {

            StringBuilder keyChars = new StringBuilder(50);
            keyChars.Append(userId.ToString());
            Random rand = new Random();
            while (keyChars.Length < SessionKeyLen)
            {
                int randomCharNum;
                randomCharNum = rand.Next(SessionKeyChars.Length);
                char randomKeyChar = SessionKeyChars[randomCharNum];
                keyChars.Append(randomKeyChar);
            }
            string sessionKey = keyChars.ToString();
            return sessionKey;
        }
    }
}
