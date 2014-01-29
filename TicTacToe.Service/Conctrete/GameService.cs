using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using TicTacToe.DataLayer;
using TicTacToe.Models;
using TicTacToe.Repository;
using TicTacToe.Service.Abstract;

namespace TicTacToe.Service.Concrete
{
    public class GameService : BaseService, IGameService
    {

        public GameService(IUowData data)
            : base(data)
        {

        }

        public void CreateGame(CreateGameModel gameModel)
        {
            //TODO: Validate Title
            if(gameModel==null)
            {
                throw new ArgumentNullException("CreateGameModel is null");
            }
            ValidateGamePassword(gameModel.Password);
            User redUser = GetUserBySessionKey(gameModel.SessionKey);
            Game game = new Game()
            {
                Title = gameModel.Title,
                Password = gameModel.Password,
                RedUser = redUser,
                GameStatus = GameStatusType.Open,
                MovesLeft = 9
            };
            this.Data.Games.Add(game);
            this.Data.SaveChanges();
        }

        public void JoinGame(JoinGameModel joinGameModel)
        {
            if (joinGameModel==null)
            {
                throw new ArgumentNullException("JoinGameModel is null");
            }
            ValidateGamePassword(joinGameModel.Password);
            Game game = GetGameById(joinGameModel.GameId);
            if (game.Password != null && game.Password != joinGameModel.Password)
            {
                throw new ServerErrorException("Incorrect game password", "INV_GAME_AUTH");
            }
            if (game.GameStatus != GameStatusType.Open)
            {
                throw new ServerErrorException("Тhe game is not open", "ERR_GAME_STAT_NOT_OPEN");
            }
            
            User blueUser = GetUserBySessionKey(joinGameModel.SessionKey);
            if(game.RedUserId==blueUser.Id)
            {
                throw new ServerErrorException("You are the creator of the game", "INV_GAME_USR");
            }
            game.BlueUser = blueUser;
            game.GameStatus = GameStatusType.Full;
            this.Data.Games.Update(game);
            this.Data.SaveChanges();
        }

        public void RestartGameState(string sessionKey, int gameId, string statusType)
        {
            
            User user = GetUserBySessionKey(sessionKey);
            Game game = GetGameById(gameId);
            ValidateUserInGame(game, user);
            var guess = this.Data.Guesses.GetAll().Where(g => g.GameId == gameId);
            this.Data.Guesses.Remove(guess);
            game.MovesLeft = 9;
            game.Winner = null;            
            game.GameStatus = statusType;
            this.Data.Games.Update(game);
            this.Data.SaveChanges();
        }

        public Game StartGame(string sessionKey, int gameId)
        {

            User redUser = GetUserBySessionKey(sessionKey);
            Game game = GetGameById(gameId);
            if (game.RedUserId != redUser.Id)
            {
                throw new ServerErrorException("You cannot start the game", "INV_OP_GAME_OWNER");
            }
            if (game.GameStatus != GameStatusType.Full)
            {
                throw new ServerErrorException("Game cannot be started", "INV_OP_GAME_STAT");
            }
            game.GameStatus = GameStatusType.InProgress;
            game.UserInTurn = rand.Next(2) == 0 ? game.RedUserId : game.BlueUserId;
            this.Data.Games.Update(game);
            this.Data.SaveChanges();
            return game;

        }

        public void LeaveGame(int gameId, string sessionKey)
        {
            User user = GetUserBySessionKey(sessionKey);
            Game game = GetGameById(gameId);
            ValidateUserInGame(game, user);
            if (game.BlueUserId != user.Id)
            {
                return;
            }
            game.BlueUser = null;
            game.GameStatus = GameStatusType.Open;
            this.Data.Games.Update(game);
            this.Data.SaveChanges();

        }

        public void DeleteGame(string sessionKey, int gameId)
        {
            User redUser = GetUserBySessionKey(sessionKey);
            Game game = GetGameById(gameId);
            if (game.RedUserId != redUser.Id)
            {
                throw new ServerErrorException("You cannot delete this game", "INV_OP_GAME_OWNER");
            }
            this.Data.Games.Delete(game);
            this.Data.SaveChanges();

        }
               

        public IEnumerable<GameModel> GetOpenGames(string sessionKey)
        {
            IEnumerable<GameModel> openGameModels;
            User user = GetUserBySessionKey(sessionKey);
            var openGames = this.Data.Games.GetAll()
                .Where(g => g.GameStatus == GameStatusType.Open && g.RedUserId != user.Id);          
                openGameModels = ParseGamesToModel(openGames);            
            return openGameModels;
        }

        public IEnumerable<GameModel> GetActiveGames(string sessionKey)
        {

            IEnumerable<GameModel> activeGameModels;
            User user = GetUserBySessionKey(sessionKey);

            var createdAndJoinedGames = this.Data.Games.GetAll().Where(g => g.RedUserId == user.Id || g.BlueUserId == user.Id)
                .Where(s => s.GameStatus == GameStatusType.InProgress || s.GameStatus == GameStatusType.Full);
                activeGameModels = ParseGamesToModel(createdAndJoinedGames);
            return activeGameModels;
        }

        public IEnumerable<GameModel> GetCreatedGames(string sessionKey)
        {
            IEnumerable<GameModel> createdGameModels;
            User user = GetUserBySessionKey(sessionKey);

            var createdGames = this.Data.Games.GetAll().Where(g => g.RedUserId == user.Id);          
                createdGameModels = ParseGamesToModel(createdGames);        
            return createdGameModels;
        }

        public IEnumerable<GameModel> GetJoinedGames(string sessionKey)
        {
            IEnumerable<GameModel> joinedGameModels;
            User user = GetUserBySessionKey(sessionKey);

            var joinedGames = this.Data.Games.GetAll().Where(g => g.BlueUserId == user.Id);
                joinedGameModels = ParseGamesToModel(joinedGames);         
            return joinedGameModels;
        }

        public IEnumerable<GameModel> GetGamesInProgress(string sessionKey)
        {
            IEnumerable<GameModel> inProgressGameModels;
            User user = GetUserBySessionKey(sessionKey);
            var inProgressGames = this.Data.Games.GetAll().Where(g => g.RedUserId == user.Id || g.BlueUserId == user.Id)
                 .Where(s => s.GameStatus == GameStatusType.InProgress);

            if (inProgressGames != null)
            {
                inProgressGameModels = ParseGamesToModel(inProgressGames);
            }
            else
            {
                inProgressGameModels = new List<GameModel>();
            }
            return inProgressGameModels;
        }

        public GameState GetGameState(int gameId, string sessionKey)
        {
            //TODO: Optimization method
            ValidateSessionKey(sessionKey);
            User user = GetUserBySessionKey(sessionKey);
            Game game = GetGameById(gameId);
            ValidateUserInGame(game, user);
            GameState gameState = new GameState();
            if (game.RedUser == user)
            {
                gameState.Symbol = "O";
            }
            else
            {
                gameState.Symbol = "Y";
            }
            gameState.GameId = gameId;
            gameState.Title = game.Title;
            gameState.State = game.GameStatus;
            if (game.Winner != null)
            {
                gameState.Winner = game.Winner.Nickname;
            }
            gameState.Opponent = GetAnotherOpponentInGame(gameId, sessionKey).Nickname;
            gameState.O = game.Guesses.Where(g => g.User == game.RedUser).Select(p => p.Position).ToArray();
            gameState.X = game.Guesses.Where(g => g.User == game.BlueUser).Select(p => p.Position).ToArray();
            return gameState;

        }
        public User GetCreator(int gameId)
        {
            return this.Data.Games.GetById(gameId).RedUser;
        }
        public bool ChechGamePassword(int gameId)
        {
            Game game = this.Data.Games.GetById(gameId);
            if (game == null)
            {
                throw new ServerErrorException("Invalid game", "ERR_INV_GAME");
            }
            if (game.Password != null)
            {
                return true;
            }
            return false;
        }

        private static IEnumerable<GameModel> ParseGamesToModel(IQueryable<Game> openGames)
        {
            if(openGames==null)
            {
                throw new ArgumentNullException("OpenGamesList is null");
            }
            IEnumerable<GameModel> models = openGames.Select(game => new GameModel()
                {
                    Id = (int)game.Id,
                    Title = game.Title,
                    CreatorNickname = game.RedUser.Nickname,
                    Status = game.GameStatus

                });
            return models;
        }

        public static void ValidateGamePassword(string password)
        {
            if (password != null)
            {
                if (password.Length != Sha1CodeLength)
                {
                    throw new ServerErrorException("Invalid authentication code length", "INV_GAME_AUTH_LEN");
                }
            }
        }

        //TODO: Create method "ValidateGameTitle"
    }
}
