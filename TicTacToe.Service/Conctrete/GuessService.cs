using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.DataLayer;
using TicTacToe.Models;
using TicTacToe.Repository;
using TicTacToe.Service.Abstract;

namespace TicTacToe.Service.Concrete
{
    public class GuessService : BaseService, IGuessService
    {

        public GuessService(IUowData data)
            : base(data)
        {

        }

        public Game MakeGuess(GuessModel guessModel, out GameState gameState)
        {
            if(guessModel==null)
            {
                throw new ArgumentNullException("GuessModel is null");
            }
            gameState = new GameState();
            ValidateSessionKey(guessModel.SessionKey);
            User user = GetUserBySessionKey(guessModel.SessionKey);
            Game game = GetGameById(guessModel.GameId);
            if (game.GameStatus != GameStatusType.InProgress)
            {
                throw new ServerErrorException("Game is not in progress", "INV_OP_GAME_STAT");
            }
            ValidateUserInGame(game, user);
            //check userInTurn
            if (game.UserInTurn != user.Id)
            {
                throw new ServerErrorException("Not your turn", "INV_OP_TURN");
            }
            CheckPosition(guessModel.Position);            
            PlaceMarker(game, user, guessModel.Position);
          gameState.gameOver= CheckGameOver(game);
          
                bool isWinner = false;
                int[] arrayOfMarkedPositions = game.Guesses.Where(g => g.User.Id == user.Id).Select(p => p.Position).ToArray();
                isWinner = CheckWinner(arrayOfMarkedPositions);
                if (isWinner)
                {
                    gameState.gameOver = true;
                    game.Winner = user;
                    gameState.Winner = user.Nickname;
                }
                if (gameState.gameOver)
                {
                   
                    game.GameStatus = GameStatusType.Finished;
                    gameState.GameId = (int)game.Id;
                    gameState.Title = game.Title;
                }              
                else
                {
                    var otherUser = (game.RedUser == user) ? game.BlueUser : game.RedUser;
                    game.UserInTurn = otherUser.Id;
                }
            

            this.Data.Games.Update(game);
            this.Data.SaveChanges();
            return game;

        }

        private bool CheckGameOver(Game game)
        {
            if (game.MovesLeft <= 0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="position">The position where to place the marker, should be between 0 and 9</param>
        /// <returns>True if the marker position was not already taken</returns>
        private  void PlaceMarker(Game game, User user, int position)
        {
          
            int count = game.Guesses.Where(g => g.Position == position).Count();
            if (count != 0)
            {
               throw new ServerErrorException("The position is occupied", "INV_POS");
            }
            Guess newGuess = new Guess()
            {
                Position = position,
                User = user,
                Game = game,
            };
            game.Guesses.Add(newGuess);
            game.MovesLeft--;                       
        }

        private bool CheckWinner(int[] arrayOfMarkedPositions)
        {


            bool[] arrayOfPossitions = new bool[9];
            for (int i = 0; i < arrayOfMarkedPositions.Length; i++)
            {
                arrayOfPossitions[arrayOfMarkedPositions[i] - 1] = true;
            }
            for (int i = 0; i < 3; i++)
            {
                if (
                    ((arrayOfPossitions[i * 3] != false && arrayOfPossitions[(i * 3)] == arrayOfPossitions[(i * 3) + 1] && arrayOfPossitions[(i * 3)] == arrayOfPossitions[(i * 3) + 2]) ||
                     (arrayOfPossitions[i] != false && arrayOfPossitions[i] == arrayOfPossitions[i + 3] && arrayOfPossitions[i] == arrayOfPossitions[i + 6])))
                {

                    return true;
                }
            }

            if ((arrayOfPossitions[0] != false && arrayOfPossitions[0] == arrayOfPossitions[4] && arrayOfPossitions[0] == arrayOfPossitions[8]) || (arrayOfPossitions[2] != false && arrayOfPossitions[2] == arrayOfPossitions[4] && arrayOfPossitions[2] == arrayOfPossitions[6]))
            {

                return true;
            }
            return false;
        }
        private void CheckPosition(int position)
        {
            if (position < 1 || position > 9)
            {
                throw new ServerErrorException("The position where to place the marker, should be between 0 and 9", "INV_POS");
            }
        }

    }
}
