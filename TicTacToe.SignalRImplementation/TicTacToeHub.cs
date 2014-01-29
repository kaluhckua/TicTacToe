using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using TicTacToe.Models;
using TicTacToe.Service;
using TicTacToe.Service.Abstract;
using System.Threading.Tasks;

namespace TicTacToe.SignalRImplementation
{
    public class TicTacToeHub : Hub
    {
        readonly IUserService UserServise;
        readonly IGameService GameServise;
        readonly IGuessService GuessService;

        private static int onlinePlayers = 0;
     
        public TicTacToeHub(IUserService userService, IGameService gameService, IGuessService guessService)
        {
            this.UserServise = userService;
            this.GameServise = gameService;
            this.GuessService = guessService;
        }

        public UserLoggedModel Login(UserLoginModel user)
        {
           
            UserLoggedModel userLogged = new UserLoggedModel();
            try
            {
                string nickName;
                userLogged.SessionKey = UserServise.LoginUser(new UserLoginModel()
                {
                    AuthCode = user.AuthCode,
                    Username = user.Username,
                    ConnectionId = Context.ConnectionId,
                }, out nickName);
                userLogged.Nickname = nickName;
                return userLogged;
            }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
                return null;
            }
           
        }

        public UserLoggedModel RegisterAndReturnLoggedUser(UserRegisterModel user)
        {
            user.ConnectionId = Context.ConnectionId;
            try
            {
                UserServise.CreateUser(user);
                var userLogged = Login(user);
                return userLogged;
            }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
                return null;
            }
           
        }

        public void Logout(string sessionKey)
        {
            try
            {
                UserServise.LogoutUser(sessionKey);
            }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void SaveConnectionIdBySessionKey(string sessionKey)
        {
            try
            {
                UserServise.SaveConnectionId(sessionKey, Context.ConnectionId);
            }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void ReturnOpenGamesToClient(string sessionKey)
        {
           
            try
            {
                IEnumerable<GameModel> openGames;
                openGames = GameServise.GetOpenGames(sessionKey);
                Clients.Caller.updateOpenGamesList(openGames);
            }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }

          
        }

        public void ReturnJoinedGamesToClient(string sessionKey)
        {
            try
            {
                IEnumerable<GameModel> joinedGames;

                joinedGames = GameServise.GetJoinedGames(sessionKey).ToList();

                Clients.Caller.updateJoinedGamesList(joinedGames);
            }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void ReturnActiveGamesToClient(string sessionKey)
        {
            try
            {
            IEnumerable<GameModel> activeGames;

            activeGames = GameServise.GetActiveGames(sessionKey).ToList();

            Clients.Caller.updateActiveGamesList(activeGames);
            }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void ReturnCreatedGamesToClient(string sessionKey)
        {
            try
            {
            IEnumerable<GameModel> createdGames;

            createdGames = GameServise.GetCreatedGames(sessionKey).ToList();

            Clients.Caller.updateCreatedGamesList(createdGames);
             }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void CreateGameAndNotifyOthersClients(CreateGameModel createGameModel)
        {
             try
            {
            GameServise.CreateGame(createGameModel);

            Clients.Others.createdNewGame();
            }
             catch (ServerErrorException ex)
             {
                 Clients.Caller.serverErrorException(ex.Message);
             }
        }

        public void JoinGameAndNotifyMyOpponent(JoinGameModel joinGameModel)
        {
            try{
            User opponent;

            GameServise.JoinGame(joinGameModel);
            opponent = GameServise.GetAnotherOpponentInGame(joinGameModel.GameId, joinGameModel.SessionKey);

            Clients.All.nothifyAllClientForOpenGames();
            Clients.Client(opponent.ConnectionId).joinedGame();
              }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void StartGameAndNotifyMyOpponent(int gameId, string sessionKey)
        {
            try { 
            Game game;
            User user;
            User opponent;

            user = UserServise.GetUserBySessionKey(sessionKey);
            opponent = GameServise.GetAnotherOpponentInGame(gameId, sessionKey);
            game = GameServise.StartGame(sessionKey, gameId);

            Clients.Client(opponent.ConnectionId).startedGame(
                new GameState()
                {
                    Title = game.Title,
                    GameId = gameId,
                    Opponent = user.Nickname,
                    Symbol = "X"
                });
            Clients.Caller.updateGameUI(
                new GameState()
                {
                    Title = game.Title,
                    GameId = gameId,
                    Opponent = opponent.Nickname,
                    Symbol = "O"
                });
            UpdateUserInTurn(game, user, opponent);
            }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void ReloadGameAndNotifyMyOpponent(int gameId, string sessionKey)
        {
            try{
            User opponent;

            GameServise.RestartGameState(sessionKey, gameId, GameStatusType.Full);
            opponent = GameServise.GetAnotherOpponentInGame(gameId, sessionKey);
            Clients.Caller.reloadedGame();
            Clients.Client(opponent.ConnectionId).reloadedGame();
              }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void DeleteGameAndNotifyMyOpponent(int gameId, string sessionKey)
        {
            try{
            Game game;
            User opponent;
            game = GameServise.GetGameById(gameId);
            opponent = GameServise.GetAnotherOpponentInGame(gameId, sessionKey);
            GameServise.DeleteGame(sessionKey, gameId);
            Clients.All.nothifyAllClientForOpenGames();
            if (opponent == null)
            {
                return;
            }
            GameState gameState = new GameState()
            {
                GameId = gameId,
                Title = game.Title,
                Opponent = opponent.Nickname,
            };
            if (game.GameStatus == GameStatusType.InProgress)
            {
                gameState.Winner = opponent.Nickname;
            }
            Clients.Client(opponent.ConnectionId).deletedGame(gameState);
              }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void UpdateGame(int gameId, string sessionKey)
        {
            try{
            Game game;
            User user;
            GameState gameState;

            game = GameServise.GetGameById(gameId);
            user = UserServise.GetUserBySessionKey(sessionKey);
            gameState = GameServise.GetGameState(gameId, sessionKey);

            Clients.Caller.updateGameUI(gameState);
            if (gameState.Winner == null)
            {
                if (game.UserInTurn == user.Id)
                {
                    Clients.Caller.waitingForMarkerPlacement(gameId);
                }
                else
                {
                    Clients.Caller.waitingForOpponent(gameId, "play");
                }
            }
              }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void LeaveGameAndRestartState(int gameId, string sessionKey)
        {
            try{
            Game game;
            User user;
            User opponent;
            string gameStatus;

            user = UserServise.GetUserBySessionKey(sessionKey);
            game = GameServise.GetGameById(gameId);
            gameStatus = game.GameStatus;
            opponent = GameServise.GetAnotherOpponentInGame(gameId, sessionKey);
            GameServise.RestartGameState(sessionKey, gameId, GameStatusType.Open);
            GameServise.LeaveGame(gameId, sessionKey);

            GameState gameState = new GameState()
            {
                GameId = gameId,
                Title = game.Title,
                Opponent = user.Nickname,
            };
            if (gameStatus == GameStatusType.InProgress)
            {
                gameState.Winner = opponent.Nickname;
            }
            Clients.Client(opponent.ConnectionId).leftGame(gameState);
            Clients.All.nothifyAllClientForOpenGames();
              }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void MakeGuess(GuessModel guessModel)
        {
            User user;
            User opponent;
            try
            {
                user = UserServise.GetUserBySessionKey(guessModel.SessionKey);
                opponent = GameServise.GetAnotherOpponentInGame(guessModel.GameId, guessModel.SessionKey);

                Game game;
                GameState gameState;

                game = GuessService.MakeGuess(guessModel, out gameState);

                string symbol;
                if (game.RedUser.Id == user.Id)
                {
                    symbol = "O";
                }
                else
                {
                    symbol = "X";
                }
                Clients.Caller.addMarkerPlacement(guessModel.GameId, symbol, guessModel.Position);
                Clients.Client(opponent.ConnectionId).addMarkerPlacement(guessModel.GameId, symbol, guessModel.Position);
                if (gameState.gameOver)
                {
                    Clients.Caller.gameOver(gameState);
                    Clients.Client(opponent.ConnectionId).gameOver(gameState);
                    if (game.RedUser.Id == user.Id)
                    {
                        Clients.Client(user.ConnectionId).restartOption(guessModel.GameId);
                        Clients.Client(opponent.ConnectionId).waitingForOpponent(guessModel.GameId, "restart");
                    }
                    else
                    {
                        Clients.Client(opponent.ConnectionId).restartOption(guessModel.GameId);
                        Clients.Client(user.ConnectionId).waitingForOpponent(guessModel.GameId, "restart");
                    }
                }
                else
                {
                    UpdateUserInTurn(game, user, opponent);
                }
            }
            catch (ServerErrorException ex)
            {
                Clients.Caller.serverErrorException(ex.Message);
            }
        }

        public void UpdateUserInTurn(Game game, User user, User opponent)
        {
            if (game.UserInTurn == user.Id)
            {
                Clients.Client(user.ConnectionId).waitingForMarkerPlacement(game.Id);
                Clients.Client(opponent.ConnectionId).waitingForOpponent(game.Id, "play");
            }
            else
            {
                Clients.Client(opponent.ConnectionId).waitingForMarkerPlacement(game.Id);
                Clients.Client(user.ConnectionId).waitingForOpponent(game.Id, "play");
            }
        }

        public bool CheckGamePassword(int gameId)
        {
            return GameServise.ChechGamePassword(gameId);
        }
    }
}