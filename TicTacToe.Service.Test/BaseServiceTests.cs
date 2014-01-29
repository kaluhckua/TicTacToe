using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TicTacToe.Models;
using TicTacToe.Repository;
using TicTacToe.Service.Abstract;
using TicTacToe.Service.Concrete;
using System.Collections.Generic;

namespace TicTacToe.Service.Test
{
    [TestClass]
    public class BaseServiceTests
    {
        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateSessionKey_WhenSessionKeyLenIsLessThan50_ShouldThrowException()
        {
            BaseService.ValidateSessionKey("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJte");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateSessionKey_WhenSessionKeyLenIsMoreThan50_ShouldThrowException()
        {
            BaseService.ValidateSessionKey("123456789012345678901234567890123456789012345678901");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateSessionKey_WhenSessionKeyIsNull_ShouldThrowException()
        {
            BaseService.ValidateSessionKey(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateSessionKey_WhenSessionKeyContainsNotAllowedChar_ShouldThrowException()
        {
            BaseService.ValidateSessionKey("!12345678901234567890123456890123456789012345678901");
        }

        [TestMethod]
        public void GenerateSessionKey_WhenIdIs5_ShouldReturnStringStartsWith5AndLengthIs50()
        {
            string sessionKey = UserService.GenerateSessionKey(5);
            Assert.IsTrue(sessionKey.StartsWith("5"));
            Assert.IsTrue(sessionKey.Length == 50);
        }


        [TestMethod]
        public void GetAnotherOpponentInGame_WhenUserIsRedUser_ShouldReturnBlueUser()
        {
            Mock<IUowData> mock = new Mock<IUowData>();
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                  new Game()
                {
                    GameStatus = "InProgress",
                    RedUserId = 1,
                    BlueUserId = 2,
                    Title = "Title2",
                    RedUser = new User() { Id = 1, Nickname = "RedUser" },
                    BlueUser = new User() { Id = 2, Nickname = "BlueUser" },
                });

            BaseService baseService = new BaseService(mock.Object);
            User otherUser = baseService.GetAnotherOpponentInGame(1, "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");

            Assert.AreEqual(2, otherUser.Id);
        }

        [TestMethod]
        public void GetAnotherOpponentInGame_WhenUserIsBlueUser_ShouldReturnRedUser()
        {
            Mock<IUowData> mock = new Mock<IUowData>();
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=2
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                  new Game()
                  {
                      GameStatus = "InProgress",
                      RedUserId = 1,
                      BlueUserId = 2,
                      Title = "Title2",
                      RedUser = new User() { Id = 1, Nickname = "RedUser" },
                      BlueUser = new User() { Id = 2, Nickname = "BlueUser" },
                  });

            BaseService baseService = new BaseService(mock.Object);
            User otherUser = baseService.GetAnotherOpponentInGame(1, "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");

            Assert.AreEqual(1, otherUser.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateUserInGame_WhenGameIsNULL_ShouldThrowException()
        {
            BaseService.ValidateUserInGame(null, new User());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateUserInGame_WhenUserIsNULL_ShouldThrowException()
        {
            BaseService.ValidateUserInGame(new Game(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateUserInGame_WhenRedUserAndBlueUserInGameIsNULL_ShouldThrowException()
        {
            BaseService.ValidateUserInGame(default(Game), default(User));
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateUserInGame_WhenRedUserOrBlueUserNotMatchWithUser_ShouldThrowException()
        {


            Game game = new Game()
            {

                RedUserId = 1,
                BlueUserId = 2
            };
            BaseService.ValidateUserInGame(game, new User());
        }

        [TestMethod]
        public void ValidateUserInGame_WhenRedUserIsUser_ShouldThrowException()
        {
            User redUser = new User()
            {
                Id = 1,
            };

            Game game = new Game();
            game.RedUserId = 1;
            BaseService.ValidateUserInGame(game, redUser);
        }

        [TestMethod]
        public void ValidateUserInGame_WhenBlueUserIsUser_ShouldThrowException()
        {
            User redUser = new User()
            {
                Id = 1,
            };

            Game game = new Game();
            game.BlueUserId = 1;
            BaseService.ValidateUserInGame(game, redUser);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetGameById_WhenIdIsNegative_ShouldThrowException()
        {
            BaseService gameService = new BaseService(null);
            gameService.GetGameById(-5);
        }

        [TestMethod]
        public void GetGameById_WhenIdIs2_ShouldReturnGameWithId2()
        {
            Mock<IUowData> mock = new Mock<IUowData>();
            mock.Setup(m => m.Games.GetById(2)).Returns(new Game
            {
                Id = 2
            });
            BaseService gameService = new BaseService(mock.Object);
            Game game = gameService.GetGameById(2);
            Assert.AreEqual(game.Id, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void GetGameById_WhenNoGameWithId2_ShouldThrowException()
        {
            Mock<IUowData> mock = new Mock<IUowData>();
            mock.Setup(m => m.Games.GetById(It.IsAny<int>())).Returns(default(Game));
            BaseService gameService = new BaseService(mock.Object);
            Game game = gameService.GetGameById(2);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void GetUserBySessionKey_WhenValidSessionKeyButNoUsersWithHim_ShouldThrowException()
        {
            Mock<IUowData> mock = new Mock<IUowData>();
            mock.Setup(m => m.Users.GetAll()).Returns(new User[]
            {
                new User { SessionKey = "other" },
                new User { SessionKey = "other" },
                new User { SessionKey = "other" },
                new User { SessionKey = "other" },
            }.AsQueryable());

            BaseService userService = new BaseService(mock.Object);
            userService.GetUserBySessionKey("100448Sitruv4IfYOGRSp6PqmxNFYr11vvZKQpCcLzmWThbTI3");
        }

        [TestMethod]
        public void GetUserBySessionKey_WhenValidSessionKey_ShouldReturnUser()
        {
            Mock<IUowData> mock = new Mock<IUowData>();
            mock.Setup(m => m.Users.GetAll()).Returns(new User[]
            {
                new User { SessionKey = "other" },
                new User { SessionKey = "other" },
                new User { SessionKey = "100448Sitruv4IfYOGRSp6PqmxNFYr11vvZKQpCcLzmWThbTI3" },
                new User { SessionKey = "other" },
            }.AsQueryable());
            BaseService userService = new BaseService(mock.Object);
            User user = userService.GetUserBySessionKey("100448Sitruv4IfYOGRSp6PqmxNFYr11vvZKQpCcLzmWThbTI3");
            Assert.AreEqual(user.SessionKey, "100448Sitruv4IfYOGRSp6PqmxNFYr11vvZKQpCcLzmWThbTI3");
        }




    }
}