using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TicTacToe.Repository;
using TicTacToe.Models;
using System.Collections.Generic;
using TicTacToe.Service.Concrete;
using TicTacToe.Service.Abstract;
using System.Reflection;


namespace TicTacToe.Service.Test
{
    [TestClass]
    public class GuessServiceTests
    {
        private static Mock<IUowData> mock;
        private static GuessService guessService;
        private static GameState gameState;

        [TestInitialize]
        public void TestInit()
        {
            mock = new Mock<IUowData>();
            guessService = new GuessService(mock.Object);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MakeGuess_WhenGuessModelIsNull_ShouldThrowException()
        {
            guessService.MakeGuess(null, out gameState);
        }


        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void MakeGuess_WhenGameStatusIsNotInProgress_ShouldThrowException()
        {
            Game game = new Game()
            {
                GameStatus = "Full",
            };
             mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
             GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 1);
             mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(game);
             guessService.MakeGuess(guessModel, out gameState);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void MakeGuess_WhenUserNotInTurn_ShouldThrowException()
        {
            Game game = new Game()
            {
                GameStatus = "InProgress",
                UserInTurn=2,
                RedUserId=1
            };
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 1);
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(game);
            guessService.MakeGuess(guessModel, out gameState);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void MakeGuess_WhenPositionIsNegative_ShouldThrowException()
        {
            Game game = new Game()
            {
                GameStatus = "InProgress",
                UserInTurn=1,
                RedUserId=1
            };
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, -1);
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(game);
            guessService.MakeGuess(guessModel, out gameState);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void MakeGuess_WhenPositionIsGreaterThan9_ShouldThrowException()
        {
            Game game = new Game()
            {
                GameStatus = "InProgress",
                UserInTurn = 1,
                RedUserId = 1
            };
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 10);
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(game);
            guessService.MakeGuess(guessModel, out gameState);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void MakeGuess_WhenPositionIsOccupied_ShouldThrowException()
        {
            List<Guess> guesses = new List<Guess>();
            guesses.Add(new Guess() { Position = 1 });
            Game game = new Game()
            {
                Guesses=guesses,
                GameStatus = "InProgress",
                UserInTurn = 1,
                RedUserId = 1
            };
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 1);
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(game);
            guessService.MakeGuess(guessModel, out gameState);
        }

        [TestMethod]
        public void MakeGuess_WhenPlacedMaerkerAndGameIsNotOver_ShouldUpdateGame()
        {

            User redUser = new User()
            {
                SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 1
            };
               User blueUser = new User()
            {
                SessionKey = "20043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 2
            };
            Game updatedGame = new Game();
            List<Guess> guesses = new List<Guess>();
            guesses.Add(new Guess() { Position = 1, User = blueUser });
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
              new Game()
              {
                  MovesLeft = 8,
                  Guesses = guesses,
                  GameStatus = "InProgress",
                  UserInTurn = 1,
                  RedUser=redUser,
                  RedUserId = 1,
                  BlueUser=blueUser,
                  BlueUserId=2
              });
            mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());

            mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
            GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 2);
            guessService.MakeGuess(guessModel, out gameState);

            Assert.AreEqual(2, updatedGame.UserInTurn);
            Assert.IsFalse(gameState.gameOver);
            Assert.IsNull(gameState.Winner);
            Assert.AreEqual(updatedGame.GameStatus, "InProgress");
        }

        [TestMethod]      
        public void MakeGuess_WhenLastMoveAndNoWinner_ShouldReturnStateGameOver()
        {
           
            User user=new User()           
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            };
            Game updatedGame = new Game();
            List<Guess> guesses = new List<Guess>();
            guesses.Add(new Guess() { Position = 1, User=user });
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
              new Game()
            {
                MovesLeft=1,
                Guesses = guesses,
                GameStatus = "InProgress",
                UserInTurn = 1,
                RedUserId = 1
            });
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]{ user }.AsQueryable());    
                 
            mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
            GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 2);
            guessService.MakeGuess(guessModel, out gameState);

            Assert.IsTrue(gameState.gameOver);
            Assert.IsNull(gameState.Winner);
            Assert.AreEqual(updatedGame.GameStatus, "Finished");
        }

        [TestMethod]       
        public void MakeGuess_WhenLastMoveAndWinnerIsRedUser_ShouldReturnStateGameOver()
        {
            User redUser = new User()
            {
                SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 1
            };
            Game updatedGame = new Game();
            List<Guess> guesses = new List<Guess>();
            guesses.Add(new Guess() { Position = 1,User= redUser });
            guesses.Add(new Guess() { Position = 2,User= redUser });
           
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
              new Game()
              {
                  MovesLeft = 2,
                  Guesses = guesses,
                  GameStatus = "InProgress",
                  UserInTurn = 1,
                  RedUserId = 1
              });
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]{redUser}.AsQueryable());
            mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
            GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 3);
            guessService.MakeGuess(guessModel, out gameState);
            Assert.AreEqual(1, updatedGame.Winner.Id);
            Assert.IsTrue(gameState.gameOver);
        }

        [TestMethod]
        public void MakeGuess_WhenPlaceFirstRow_ShouldWinning()
        {
            User redUser = new User()
            {
                SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 1
            };
            Game updatedGame = new Game();
            List<Guess> guesses = new List<Guess>();
            guesses.Add(new Guess() { Position = 1, User = redUser });
            guesses.Add(new Guess() { Position = 2, User = redUser });

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
              new Game()
              {
                  MovesLeft = 9,
                  Guesses = guesses,
                  GameStatus = "InProgress",
                  UserInTurn = 1,
                  RedUserId = 1
              });
            mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
            mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
            GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 3);
            guessService.MakeGuess(guessModel, out gameState);
            Assert.AreEqual(1, updatedGame.Winner.Id);
            Assert.IsTrue(gameState.gameOver);
        }

         [TestMethod]
        public void MakeGuess_WhenPlaceSecondRow_ShouldWinning()
        {
            User redUser = new User()
            {
                SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 1
            };
            Game updatedGame = new Game();
            List<Guess> guesses = new List<Guess>();
            guesses.Add(new Guess() { Position = 4, User = redUser });
            guesses.Add(new Guess() { Position = 5, User = redUser });

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
              new Game()
              {
                  MovesLeft = 9,
                  Guesses = guesses,
                  GameStatus = "InProgress",
                  UserInTurn = 1,
                  RedUserId = 1
              });
            mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
            mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
            GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 6);
            guessService.MakeGuess(guessModel, out gameState);
            Assert.AreEqual(1, updatedGame.Winner.Id);
            Assert.IsTrue(gameState.gameOver);
        }
               

         [TestMethod]
         public void MakeGuess_WhenPlaceThirdRow_ShouldWinning()
         {
             User redUser = new User()
             {
                 SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                 Id = 1
             };
             Game updatedGame = new Game();
             List<Guess> guesses = new List<Guess>();
             guesses.Add(new Guess() { Position = 7, User = redUser });
             guesses.Add(new Guess() { Position = 8, User = redUser });

             mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
               new Game()
               {
                   MovesLeft = 9,
                   Guesses = guesses,
                   GameStatus = "InProgress",
                   UserInTurn = 1,
                   RedUserId = 1
               });
             mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
             mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
             GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 9);
             guessService.MakeGuess(guessModel, out gameState);
             Assert.AreEqual(1, updatedGame.Winner.Id);
             Assert.IsTrue(gameState.gameOver);
         }

         [TestMethod]
         public void MakeGuess_WhenPlaceFirstCol_ShouldWinning()
         {
             User redUser = new User()
             {
                 SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                 Id = 1
             };
             Game updatedGame = new Game();
             List<Guess> guesses = new List<Guess>();
             guesses.Add(new Guess() { Position = 1, User = redUser });
             guesses.Add(new Guess() { Position = 4, User = redUser });

             mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
               new Game()
               {
                   MovesLeft = 9,
                   Guesses = guesses,
                   GameStatus = "InProgress",
                   UserInTurn = 1,
                   RedUserId = 1
               });
             mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
             mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
             GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 7);
             guessService.MakeGuess(guessModel, out gameState);
             Assert.AreEqual(1, updatedGame.Winner.Id);
             Assert.IsTrue(gameState.gameOver);
         }

         [TestMethod]
         public void MakeGuess_WhenPlaceSecondCol_ShouldWinning()
         {
             User redUser = new User()
             {
                 SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                 Id = 1
             };
             Game updatedGame = new Game();
             List<Guess> guesses = new List<Guess>();
             guesses.Add(new Guess() { Position = 2, User = redUser });
             guesses.Add(new Guess() { Position = 5, User = redUser });

             mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
               new Game()
               {
                   MovesLeft = 9,
                   Guesses = guesses,
                   GameStatus = "InProgress",
                   UserInTurn = 1,
                   RedUserId = 1
               });
             mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
             mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
             GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 8);
             guessService.MakeGuess(guessModel, out gameState);
             Assert.AreEqual(1, updatedGame.Winner.Id);
             Assert.IsTrue(gameState.gameOver);
         }

         [TestMethod]
         public void MakeGuess_WhenPlaceThirdCol_ShouldWinning()
         {
             User redUser = new User()
             {
                 SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                 Id = 1
             };
             Game updatedGame = new Game();
             List<Guess> guesses = new List<Guess>();
             guesses.Add(new Guess() { Position = 3, User = redUser });
             guesses.Add(new Guess() { Position = 6, User = redUser });

             mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
               new Game()
               {
                   MovesLeft = 9,
                   Guesses = guesses,
                   GameStatus = "InProgress",
                   UserInTurn = 1,
                   RedUserId = 1
               });
             mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
             mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
             GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 9);
             guessService.MakeGuess(guessModel, out gameState);
             Assert.AreEqual(1, updatedGame.Winner.Id);
             Assert.IsTrue(gameState.gameOver);
         }

         [TestMethod]
         public void MakeGuess_WhenPlaceFiratDiagonal_ShouldWinning()
         {
             User redUser = new User()
             {
                 SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                 Id = 1
             };
             Game updatedGame = new Game();
             List<Guess> guesses = new List<Guess>();
             guesses.Add(new Guess() { Position = 1, User = redUser });
             guesses.Add(new Guess() { Position = 5, User = redUser });

             mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
               new Game()
               {
                   MovesLeft = 9,
                   Guesses = guesses,
                   GameStatus = "InProgress",
                   UserInTurn = 1,
                   RedUserId = 1
               });
             mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
             mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
             GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 9);
             guessService.MakeGuess(guessModel, out gameState);
             Assert.AreEqual(1, updatedGame.Winner.Id);
             Assert.IsTrue(gameState.gameOver);
         }

         [TestMethod]
         public void MakeGuess_WhenPlaceSecondDiagonal_ShouldWinning()
         {
             User redUser = new User()
             {
                 SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                 Id = 1
             };
             Game updatedGame = new Game();
             List<Guess> guesses = new List<Guess>();
             guesses.Add(new Guess() { Position = 3, User = redUser });
             guesses.Add(new Guess() { Position = 5, User = redUser });

             mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
               new Game()
               {
                   MovesLeft = 9,
                   Guesses = guesses,
                   GameStatus = "InProgress",
                   UserInTurn = 1,
                   RedUserId = 1
               });
             mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
             mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
             GuessModel guessModel = new GuessModel("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, 7);
             guessService.MakeGuess(guessModel, out gameState);
             Assert.AreEqual(1, updatedGame.Winner.Id);
             Assert.IsTrue(gameState.gameOver);
         }
    }
}
