using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacToe.Models;
using TicTacToe.Service.Abstract;

namespace TicTacToe.Service
{
    public interface IGameService
    {
        Game GetGameById(int gameId);
        void CreateGame(CreateGameModel gameModel);
        void JoinGame(JoinGameModel gameModel);

        Game StartGame(string sessionKey, int gameId);
        void RestartGameState(string sessionKey, int gameId, string statusType);

        void DeleteGame(string sessionKey, int gameId);

       
        GameState GetGameState(int gameId, string sessionKey);
        IEnumerable<GameModel> GetOpenGames(string sessionKey);

        IEnumerable<GameModel> GetActiveGames(string sessionKey);

        IEnumerable<GameModel> GetCreatedGames(string sessionKey);
        IEnumerable<GameModel> GetJoinedGames(string sessionKey);

        IEnumerable<GameModel> GetGamesInProgress(string sessionKey);
        User GetAnotherOpponentInGame(int gameId, string sessionKey);

        bool ChechGamePassword(int gameId);
        void LeaveGame(int gameId, string sessionKey);
      
    }
}
