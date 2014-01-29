using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TicTacToe.Repository;
using TicTacToe.Models;
using System.Collections.Generic;
using TicTacToe.Service.Concrete;
using System.Reflection;

namespace TicTacToe.Service.Test
{
    [TestClass]
    public class GameServiceTest
    {
        private static Mock<IUowData> mock;

        [TestInitialize]
        public void TestInit()
        {
            mock = new Mock<IUowData>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateGame_WhenCreateGameModelIsNull_ShouldThrowException()
        {
            GameService gameservice = new GameService(mock.Object);
            gameservice.CreateGame(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void CreateGame_WhenPasswordIsTooLong_ShouldThrowException()
        {
            GameService gameservice = new GameService(mock.Object);

            CreateGameModel createGameModel = new CreateGameModel()
            {
                SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                Password = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLLesrbsrtnsrtnsrtnsrt",
                Title = "Title",
            };
            gameservice.CreateGame(createGameModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void CreateGame_WhenSessionKeyIsInvalid_ShouldThrowException()
        {
            GameService gameservice = new GameService(mock.Object);
            CreateGameModel createGameModel = new CreateGameModel()
            {
                SessionKey = "InvalidSessionKey",
                Password = null,
                Title = "Title",
            };
            gameservice.CreateGame(createGameModel);
        }

        [TestMethod]
        public void CreateGame_WhenCreateGameModelIsValid_ShouldAddGameToRepository()
        {
            Game newGame = new Game();
            CreateGameModel createGameModel = new CreateGameModel()
            {
                SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                Password = null,
                Title = "Title",
            };

            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                    Nickname = "Kalinskia",
                    Username = "Kalin",
                },
            }.AsQueryable());
            mock.Setup(u => u.Games.Add(It.IsAny<Game>())).Callback((Game game) => newGame = game);
            GameService gameservice = new GameService(mock.Object);
            gameservice.CreateGame(createGameModel);
            Assert.AreEqual("Title", newGame.Title);
            Assert.IsNull(newGame.Password);
            Assert.AreEqual("Title", newGame.Title);
            Assert.AreEqual("100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL", newGame.RedUser.SessionKey);
            Assert.AreEqual("Kalinskia", newGame.RedUser.Nickname);
            Assert.AreEqual("Kalin", newGame.RedUser.Username);
            Assert.AreEqual(9, newGame.MovesLeft);
            Assert.AreEqual("Open", newGame.GameStatus);
            Assert.IsNull(newGame.UserInTurn);
            Assert.IsNull(newGame.Winner);
            Assert.IsNull(newGame.BlueUser);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinGame_WhenJoinGameModelIsNull_ShouldThrowException()
        {
            GameService gameService = new GameService(mock.Object);
            gameService.JoinGame(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void JoinGame_WhenPasswordIsNotSame_ShouldThrowException()
        {
            JoinGameModel joinGameModel = new JoinGameModel()
            {
                GameId = 1,
                Password = "2234567890123456789012345678901234567890",
                SessionKey = "100448Sitruv4IfYOGRSp6PqmxNFYr11vvZKQpCcLzmWThbTI3"
            };
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = "1234567890123456789012345678901234567890",
                    GameStatus = "Open",
                });
            GameService gameService = new GameService(mock.Object);
            gameService.JoinGame(joinGameModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void JoinGame_WhenGameIsNotOpen_ShouldThrowException()
        {
            JoinGameModel joinGameModel = new JoinGameModel()
            {
                GameId = 1,
            };
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    GameStatus = "InProgress",
                });
            GameService gameService = new GameService(mock.Object);
            gameService.JoinGame(joinGameModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void JoinGame_WhenUserIsCreator_ShouldThrowException()
        {
            JoinGameModel joinGameModel = new JoinGameModel()
            {
                GameId = 1,
                Password = null,
                SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
            };
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = null,
                    GameStatus = "Open",
                    RedUserId = 1
                });
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 1,
                    Nickname = "creatorNickname",
                    Username = "creatorUsername",
                    SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
            }.AsQueryable());
            GameService gameService = new GameService(mock.Object);
            gameService.JoinGame(joinGameModel);
        }

        [TestMethod]
        public void JoinGame_WhenDataIsValid_ShouldUpdateGameState()
        {
            Game updatedGame = new Game();
            JoinGameModel joinGameModel = new JoinGameModel()
            {
                GameId = 1,
                Password = null,
                SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
            };
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = null,
                    GameStatus = "Open",
                    RedUserId = 1
                });
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 2,
                    Nickname = "creatorNickname",
                    Username = "creatorUsername",
                    SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
            }.AsQueryable());
            mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
            GameService gameService = new GameService(mock.Object);
            gameService.JoinGame(joinGameModel);
            Assert.AreEqual("Full", updatedGame.GameStatus);
            Assert.AreEqual(2, updatedGame.BlueUser.Id);
        }

        [TestMethod]
        public void RestartGameState_WhenUserInGame_ShouldRestartAndUpdateGameState()
        {
            Game updatedGame = new Game();
            Guess[] guess = new Guess[]
            {
                new Guess() { Id = 1 },
                new Guess() { Id = 2 },
                new Guess() { Id = 3 },
            };
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 2,
                    Nickname = "creatorNickname",
                    Username = "creatorUsername",
                    SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
            }.AsQueryable());

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = null,
                    GameStatus = "Open",
                    RedUserId = 1,
                    Winner = new User(),
                    BlueUserId = 2,
                    Guesses = guess
                });

            GameService gameSerivce = new GameService(mock.Object);
            mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);
            mock.Setup(u => u.Guesses.Remove(It.IsAny<Guess[]>()));
            gameSerivce.RestartGameState("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1, "Open");

            Assert.AreEqual("Open", updatedGame.GameStatus);
            Assert.AreEqual(9, updatedGame.MovesLeft);
            Assert.IsNull(updatedGame.Winner);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void StartGame_WhenUserIsNotCreator_ShouldThrowException()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 2,
                    Nickname = "creatorNickname",
                    Username = "creatorUsername",
                    SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
            }.AsQueryable());

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = null,
                    GameStatus = "Open",
                    RedUserId = 1,
                    Winner = new User(),
                    BlueUserId = 2,
                });
            GameService gameSerivce = new GameService(mock.Object);
            gameSerivce.StartGame("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void StartGame_WhenGameIsNotFull_ShouldThrowException()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 1,
                    Nickname = "creatorNickname",
                    Username = "creatorUsername",
                    SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
            }.AsQueryable());

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = null,
                    GameStatus = "Open",
                    RedUserId = 1,
                    Winner = new User(),
                    BlueUserId = 2,
                });
            GameService gameSerivce = new GameService(mock.Object);
            gameSerivce.StartGame("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1);
        }

        [TestMethod]
        public void StartGame_WhenStateIsFullAndUserIsCreator_ShouldUpdateGameState()
        {
            Game updatedGame = new Game();
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 1,
                    Nickname = "creatorNickname",
                    Username = "creatorUsername",
                    SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
            }.AsQueryable());

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = null,
                    GameStatus = "Full",
                    RedUserId = 1,
                    Winner = new User(),
                    BlueUserId = 2,
                });

            mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);

            GameService gameSerivce = new GameService(mock.Object);
            gameSerivce.StartGame("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef", 1);

            Assert.AreEqual("InProgress", updatedGame.GameStatus);
            Assert.IsTrue(updatedGame.UserInTurn == 1 || updatedGame.UserInTurn == 2);
        }

        [TestMethod]
        public void LeaveGame_WhenUserIsCreator_ShouldNotUpdateGameState()
        {
            Game updatedGame = new Game();
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 1,
                    Nickname = "creatorNickname",
                    Username = "creatorUsername",
                    SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
            }.AsQueryable());

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = null,
                    GameStatus = "Full",
                    RedUserId = 1,
                    Winner = new User(),
                    BlueUserId = 2,
                });

            GameService gameSerivce = new GameService(mock.Object);
            gameSerivce.LeaveGame(1, "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");

            mock.Verify(u => u.Games.Update(It.IsAny<Game>()), Times.Never());


        }

        [TestMethod]
        public void LeaveGame_WhenUserIsГuest_ShouldUpdateGameState()
        {
            Game updatedGame = new Game();
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 2,
                    Nickname = "creatorNickname",
                    Username = "creatorUsername",
                    SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
            }.AsQueryable());

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = null,
                    GameStatus = "Full",
                    RedUserId = 1,
                    Winner = new User(),
                    BlueUserId = 2,
                    BlueUser = new User(),
                });
            mock.Setup(u => u.Games.Update(It.IsAny<Game>())).Callback((Game game) => updatedGame = game);

            GameService gameSerivce = new GameService(mock.Object);
            gameSerivce.LeaveGame(1, "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");

            Assert.AreEqual("Open", updatedGame.GameStatus);
            Assert.IsNull(updatedGame.BlueUser);

        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void  DeleteGame_WhenUserIsNotCreator_ShouldThrowExceptiom()
        {            
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 1,
                    Nickname = "creatorNickname",
                    Username = "creatorUsername",
                    SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
                 new User
                {
                    Id = 2,
                    Nickname = "blueUserNickname",
                    Username = "blueUserUsername",
                    SessionKey = "20043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                },
            }.AsQueryable());

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                new Game()
                {
                    Password = null,
                    GameStatus = "Full",
                    RedUserId = 1,                  
                    BlueUserId = 2,
                 
                });          

            GameService gameSerivce = new GameService(mock.Object);
            gameSerivce.DeleteGame("20043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",2);
        }

        [TestMethod]
        public void GetOpenGames_WhenNoOpenGameInRepository_ShouldReturnEmptyListOfGamesModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[]
            {
                new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=2,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                }
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> openGames = gameService.GetOpenGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(0, openGames.Count());
        }

        [TestMethod]
        public void GetOpenGames_WhenASingleOpenGameInRepository_ShouldReturnSingleGameModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[]
            { 
                new Game()
                {
                    GameStatus="Open",
                    RedUserId=2,
                    Title="Title",
                    RedUser=new User(){Nickname="Kalin"}
                },
                 new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=2,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                }
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> openGames = gameService.GetOpenGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(openGames.Count(), 1);
            Assert.AreEqual("Kalin", openGames.FirstOrDefault().CreatorNickname);
            Assert.AreEqual("Title", openGames.FirstOrDefault().Title);
            Assert.AreEqual("Open", openGames.FirstOrDefault().Status);

        }

        [TestMethod]
        public void GetOpenGames_WhenASingleOpenGameInRepositoryButUserIsCreator_ShouldReturnEmptyListOfGameModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[]
            { 
                new Game()
                {
                    GameStatus="Open",
                    RedUserId=1,
                    Title="Title",
                    RedUser=new User(){Nickname="Kalin"}
                },
               
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> openGames = gameService.GetOpenGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(0, openGames.Count());

        }

        [TestMethod]
        public void GetActiveGames_WhenNoActiveGameInRepository_ShouldReturnEmptyListOfGamesModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] 
            { 
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=2,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                }
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> active = gameService.GetActiveGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(0, active.Count());
        }

        [TestMethod]
        public void GetActiveGames_WhenASingleActiveGameInRepository_ShouldReturnSingleGameModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] 
            { 
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=2,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
                 new Game()
                {
                    GameStatus="Full",
                    RedUserId=2,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                }
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> active = gameService.GetActiveGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(0, active.Count());
        }

        [TestMethod]
        public void GetActiveGames_WhenASingleActiveGameInRepositoryButUserNotInTheGame_ShouldReturnEmptyListOfGamesModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] 
            { 
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=2,
                    BlueUserId=3,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
              
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> active = gameService.GetActiveGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(0, active.Count());
        }

        [TestMethod]
        public void GetCreatedGames_WhenNoCreatedGameInRepository_ShouldReturnEmptyListOfGamesModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] { }.AsQueryable());
            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> createdGames = gameService.GetCreatedGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(0, createdGames.Count());
        }

        [TestMethod]
        public void GetCreatedGames_WhenASingleCreatedGameInRepositoryOnWhichCreatorIsUser_ShouldReturnSingleGameModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] 
            { 
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=1,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
                 new Game()
                {
                    GameStatus="Full",
                    RedUserId=2,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                }
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> createdGames = gameService.GetCreatedGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(1, createdGames.Count());
        }

        [TestMethod]
        public void GetCreatedGames_WhenASingleCreatedGameInRepositoryButUserNotIsCreator_ShouldReturnEmptyListOfGamesModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] 
            { 
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=2,
                    BlueUserId=3,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
              
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> createdGames = gameService.GetCreatedGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(0, createdGames.Count());
        }

        [TestMethod]
        public void GetJoinedGames_WhenNoGamesInRepositoryInWhichUserIsJoined_ShouldReturnEmptyListOfGamesModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] 
            { 
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=2,
                    BlueUserId=3,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
                 new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=2,
                    BlueUserId=4,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
              
            }.AsQueryable());
            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> joinedGames = gameService.GetJoinedGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(0, joinedGames.Count());
        }

        [TestMethod]
        public void GetJoinedGames_WhenASingleGameInRepositoryInWhichUserIsJoined_ShouldReturnSingleGameModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=2
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] 
            { 
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=1,
                    BlueUserId=2,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
                 new Game()
                {
                    GameStatus="Full",
                    RedUserId=2,
                    BlueUserId=4,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                }
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> joinedGames = gameService.GetJoinedGames("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(1, joinedGames.Count());
        }

        [TestMethod]
        public void GetGamesInProgress_WhenASingleGameInProgressButUserIsNotInTheGame_ShouldReturnEmptyListOfGamesModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=3
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] 
            { 
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=1,
                    BlueUserId=2,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
                 new Game()
                {
                    GameStatus="Full",
                    RedUserId=2,
                    BlueUserId=3,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                }
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> inProgressGames = gameService.GetGamesInProgress("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(0, inProgressGames.Count());
        }

        [TestMethod]
        public void GetGamesInProgress_WhenTwoOfTheGamesAreInProgressAndTheUserIsInThem_ShouldReturnListOfTwoGamesModel()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {new User()
            {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=3
            }}.AsQueryable());
            mock.Setup(g => g.Games.GetAll()).Returns(new Game[] 
            { 
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=3,
                    BlueUserId=1,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
                
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=1,
                    BlueUserId=3,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                },
                 new Game()
                {
                    GameStatus="Full",
                    RedUserId=2,
                    BlueUserId=4,
                    Title="Title2",
                    RedUser=new User(){Nickname="Kalin"}
                }
            }.AsQueryable());

            GameService gameService = new GameService(mock.Object);
            IEnumerable<GameModel> inProgressGames = gameService.GetGamesInProgress("10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual(2, inProgressGames.Count());
        }

        [TestMethod]
        public void GetGameState_WhenUserInGameIsRedUserAndNoWinner_ShouldReturnGameState()
         {
              User redUser=new User()
              {
                SessionKey="10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id=1,
                Nickname="RedUser",
              };
              User blueUser = new User()
              {
                  SessionKey = "20043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                  Id = 2,
                  Nickname="BlueUser",
              };
             mock.Setup(g => g.Users.GetAll()).Returns(new User[]{redUser}.AsQueryable());            
             List<Guess> guesses = new List<Guess>();
              guesses.Add(new Guess()
            {
                Id=1,
                Position=1,
                User=redUser
            });
              guesses.Add(new Guess()
              {
                  Id = 2,
                  Position = 2,
                  User = redUser
              });
              guesses.Add(new Guess()
              {
                  Id = 3,
                  Position = 3,
                  User = blueUser
              });
             
             mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
           
                  new Game()
                {
                    GameStatus="InProgress",
                    RedUserId=1,
                    BlueUserId=2,                    
                    RedUser=redUser,
                    BlueUser=blueUser,
                    Guesses=guesses,
                    Title="Title",
                    Winner=null,
                });

             GameService gameService = new GameService(mock.Object);
             GameState gameState = gameService.GetGameState(1, "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
             Assert.AreEqual("O",gameState.Symbol);
             Assert.AreEqual("BlueUser", gameState.Opponent);
             Assert.AreEqual("InProgress", gameState.State);
             Assert.AreEqual("Title", gameState.Title);
             Assert.IsNull(gameState.Winner);
             Assert.AreEqual(2, gameState.O.Count());
             Assert.AreEqual(1, gameState.X.Count());
         }

        [TestMethod]
        public void GetGameState_WhenUserInGameIsBlueUserAndNoWinner_ShouldReturnGameState()
        {
            User redUser = new User()
            {
                SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 1,
                Nickname = "RedUser",
            };
            User blueUser = new User()
            {
                SessionKey = "20043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 2,
                Nickname = "BlueUser",
            };
            mock.Setup(g => g.Users.GetAll()).Returns(new User[] { blueUser }.AsQueryable());
            List<Guess> guesses = new List<Guess>();
            guesses.Add(new Guess()
            {
                Id = 1,
                Position = 1,
                User = redUser
            });
            guesses.Add(new Guess()
            {
                Id = 2,
                Position = 2,
                User = redUser
            });
            guesses.Add(new Guess()
            {
                Id = 3,
                Position = 3,
                User = blueUser
            });

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(

                 new Game()
                 {
                     GameStatus = "InProgress",
                     RedUserId = 1,
                     BlueUserId = 2,
                     RedUser = redUser,
                     BlueUser = blueUser,
                     Guesses = guesses,
                     Title = "Title",
                     Winner = null,
                 });

            GameService gameService = new GameService(mock.Object);
            GameState gameState = gameService.GetGameState(1, "20043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual("Y", gameState.Symbol);
            Assert.AreEqual("RedUser", gameState.Opponent);
            Assert.AreEqual("InProgress", gameState.State);
            Assert.AreEqual("Title", gameState.Title);
            Assert.IsNull(gameState.Winner);
            Assert.AreEqual(2, gameState.O.Count());
            Assert.AreEqual(1, gameState.X.Count());
        }

        [TestMethod]
        public void GetGameState_WhenWinerIsRedUser_ShouldReturnGameState()
        {
            User redUser = new User()
            {
                SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 1,
                Nickname = "RedUser",
            };
            User blueUser = new User()
            {
                SessionKey = "20043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 2,
                Nickname = "BlueUser",
            };
            mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
           
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(

                 new Game()
                 {
                     GameStatus = "InProgress",
                     RedUserId = 1,
                     BlueUserId = 2,
                     RedUser = redUser,
                     BlueUser = blueUser,                   
                     Title = "Title",
                     Winner = redUser,
                 });

            GameService gameService = new GameService(mock.Object);
            GameState gameState = gameService.GetGameState(1, "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");          
            Assert.AreEqual("RedUser",gameState.Winner);
        }

        [TestMethod]
        public void GetGameState_WhenWinerIsBlueUser_ShouldReturnGameState()
        {
            User redUser = new User()
            {
                SessionKey = "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 1,
                Nickname = "RedUser",
            };
            User blueUser = new User()
            {
                SessionKey = "20043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef",
                Id = 2,
                Nickname = "BlueUser",
            };
            mock.Setup(g => g.Users.GetAll()).Returns(new User[] { redUser }.AsQueryable());
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(

                 new Game()
                 {
                     GameStatus = "InProgress",
                     RedUserId = 1,
                     BlueUserId = 2,
                     RedUser = redUser,
                     BlueUser = blueUser,
                     Title = "Title",
                     Winner = blueUser,
                 });

            GameService gameService = new GameService(mock.Object);
            GameState gameState = gameService.GetGameState(1, "10043IOvy7N9Bn9BDAk2mtT7ZcYKtZbBpdp00ZoIpJikyIJtef");
            Assert.AreEqual("BlueUser", gameState.Winner);
        }

        [TestMethod]
        public void GetCreator_WhenGameIsValid_ShouldReturnCreatorUser()
        {
            User redUser = new User()
            {               
                Nickname = "RedUser",
            };
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                 new Game()
                 {
                     RedUser = redUser,
                 });
            GameService gameService = new GameService(mock.Object);
            User creator = gameService.GetCreator(1);
            Assert.AreEqual("RedUser", creator.Nickname);
        }

        [TestMethod]
        public void ChechGamePassword_WhenTheGameHasAPassword_ShouldReturnTrue()
        {
          
            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                 new Game()
                 {
                    Password="password",
                 });
            GameService gameService = new GameService(mock.Object);
           bool isPassword = gameService.ChechGamePassword(1);
           Assert.IsTrue(isPassword);
        }

        [TestMethod]
        public void ChechGamePassword_WhenTheGameHasNotAPassword_ShouldReturnTrue()
        {

            mock.Setup(g => g.Games.GetById(It.IsAny<int>())).Returns(
                 new Game()
                 {
                     Password= null,
                 });
            GameService gameService = new GameService(mock.Object);
            bool isPassword = gameService.ChechGamePassword(1);
            Assert.IsFalse(isPassword);
        }


        [TestMethod]
        [ExpectedException(typeof(System.Reflection.TargetInvocationException))]
        public void ParseGamesToModel_WhenOpenGamesListIsNull_ShouldReturnEmptyList()
        {

            var method = typeof(GameService).GetMethod("ParseGamesToModel", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(obj: null, parameters: new object[] { null });
        }

        [TestMethod]

        public void ParseGamesToModel_WhenOpenGamesListContaintOneGames_ShouldReturnParseGameModelsWithOneGames()
        {
            List<Game> openGames = new List<Game>();
            openGames.Add(new Game
            {
                Id = 1,
                Title = "Title1",
                RedUser = new User() { Nickname = "Kalin" },
            });

            var method = typeof(GameService).GetMethod("ParseGamesToModel", BindingFlags.Static | BindingFlags.NonPublic);
            IEnumerable<GameModel> gamesModel = (IEnumerable<GameModel>)method
                .Invoke(obj: null, parameters: new object[] { openGames.AsQueryable() });
            Assert.AreEqual(gamesModel.Count(), 1);
            Assert.AreEqual(gamesModel.FirstOrDefault().Id, 1);
            Assert.AreEqual("Title1", gamesModel.FirstOrDefault().Title);
            Assert.AreEqual("Kalin", gamesModel.FirstOrDefault().CreatorNickname);
        }

        [TestMethod]
        public void ValidateGamePassword_WhenPasswordIsOk_ShouldNotThrowException()
        {
            GameService.ValidateGamePassword("1234567890123456789012345678901234567890");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateGamePassword_WhenPasswordIsTooLong_ShouldNotThrowException()
        {
            GameService.ValidateGamePassword("12345678901234567890123456789012345678901");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateGamePassword_WhenPasswordIsTooShort_ShouldNotThrowException()
        {
            GameService.ValidateGamePassword("123456789012345678901234567890123456789");
        }
    }
}