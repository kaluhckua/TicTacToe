using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TicTacToe.Repository;
using TicTacToe.Models;
using System.Collections.Generic;
using TicTacToe.Service.Concrete;
using TicTacToe.Service.Abstract;

namespace TicTacToe.Service.Test
{
    [TestClass]
    public class UserServiceTests
    {
        private static Mock<IUowData> mock;

        [TestInitialize]
        public void TestInit()
        {
            mock = new Mock<IUowData>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateUser_WhenUserRegisterModelIsNull_ShouldShouldThrowException()
        {
            UserService userService = new UserService(mock.Object);
            string nickname;
            userService.CreateUser(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void CreateUser_WhenUsernameAlreadyExists_ShouldThrowException()
        {
            User[] users = new User[]
            {
                new User { Username = "username", Nickname = "nickaname" },
                new User { Username = "otherUsername", Nickname = "otherNickaname" },
            };
            UserRegisterModel userModel = new UserRegisterModel()
            {
                Username = "username",
                Nickname = "nickname2",
                AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5",
                ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
            };
            mock.Setup(m => m.Users.GetAll()).Returns(users.AsQueryable());
            UserService userService = new UserService(mock.Object);
            userService.CreateUser(userModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void CreateUser_WhenNicknameAlreadyExists_ShouldThrowException()
        {
            User[] users = new User[]
            {
                new User { Username = "username", Nickname = "nickname" },
                new User { Username = "otherUsername", Nickname = "otherNickName" },
            };
            UserRegisterModel userModel = new UserRegisterModel()
            {
                Username = "username2",
                Nickname = "nickname",
                AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5",
                ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
            };
            mock.Setup(m => m.Users.GetAll()).Returns(users.AsQueryable());
            UserService userService = new UserService(mock.Object);
            userService.CreateUser(userModel);
        }

        [TestMethod]
        public void CreateUser_WhenModelIsValid_ShouldAddToRepository()
        {
            List<User> users = new List<User>();
            users.Add(new User { Username = "username", Nickname = "nickname" });
            users.Add(new User { Username = "otherUsername", Nickname = "otherNickName" });

            UserRegisterModel userModel = new UserRegisterModel()
            {
                Username = "kalin",
                Nickname = "kaluhckua",
                AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5",
                ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
            };
            mock.Setup(m => m.Users.GetAll()).Returns(users.AsQueryable());
            mock.Setup(m => m.Users.Add(It.IsAny<User>())).Callback((User user) => users.Add(user));
            UserService userService = new UserService(mock.Object);
            userService.CreateUser(userModel);
            Assert.AreEqual(userModel.AuthCode, users[2].AuthCode);
            Assert.AreEqual(userModel.Nickname, users[2].Nickname);
            Assert.AreEqual(userModel.Username, users[2].Username);
            Assert.AreEqual(userModel.ConnectionId, users[2].ConnectionId);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void SaveConnectionId_WhenSessionKeyIsInvalid_ShouldThrowException()
        {
            UserService userService = new UserService(mock.Object);
            userService.SaveConnectionId("InvalidSessionKey", "1234");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveConnectionId_WhenConnectionIdIsNull_ShouldThrowException()
        {
            UserService userService = new UserService(mock.Object);
            userService.SaveConnectionId("100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL", null);
        }

        [TestMethod]
        public void SaveConnectionId_WhenDataIsValid_ShouldUpdataConnectionId()
        {
            Mock<IUowData> mock = new Mock<IUowData>();
            User updatedUser = new User();
            UserService userService = new UserService(mock.Object);
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User { SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL" },
            }.AsQueryable());
            mock.Setup(u => u.Users.Update(It.IsAny<User>())).Callback((User user) => updatedUser = user);
            userService.SaveConnectionId("100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL", "1234");
            Assert.AreEqual("1234",updatedUser.ConnectionId );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoginUser_WhenUserLoginModelIsNull_ShouldShouldThrowException()
        {
            UserService userService = new UserService(mock.Object);
            string nickname;
            userService.LoginUser(null, out nickname);
        }

        [TestMethod]
        public void LoginUser_WhenUserLoginModelIsValid_ShouldReturnSessionKeyAndNickname()
        {
            User updatedUser = new User();
            UserLoginModel loginModel = new UserLoginModel()
            {
                AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5",
                Username = "username",
                ConnectionId = "newConnectionId"
            };
            UserService userService = new UserService(mock.Object);
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Username = "username",
                    Nickname = "nickname",
                    ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
                    SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                    AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5"
                },
            }.AsQueryable());
            mock.Setup(u => u.Users.Update(It.IsAny<User>())).Callback((User user) => updatedUser = user);

            string nickname;
            string sessionKey = userService.LoginUser(loginModel, out nickname);
            Assert.AreEqual("nickname",nickname);
            Assert.IsNotNull(sessionKey);
            Assert.AreEqual(loginModel.ConnectionId, updatedUser.ConnectionId);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void LoginUser_WhenAuthCodeIsNotMatch_ShouldThrowException()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Username = "username",
                    Nickname = "nickname",
                    ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
                    SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                    AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5"
                },
            }.AsQueryable());
            UserLoginModel loginModel = new UserLoginModel()
            {
                AuthCode = "11111648010756ed51eecccf94c01bc0015048c5",
                Username = "username",
                ConnectionId = "newConnectionId"
            };

            UserService userService = new UserService(mock.Object);
            string nickname;
            userService.LoginUser(loginModel, out nickname);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void LoginUserByModel_WhenUsernameIsNotMatch_ShouldThrowException()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Username = "username",
                    Nickname = "nickname",
                    ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
                    SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                    AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5"
                },
            }.AsQueryable());
            UserLoginModel loginModel = new UserLoginModel()
            {
                AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5",
                Username = "otherUsername",
                ConnectionId = "newConnectionId"
            };

            UserService userService = new UserService(mock.Object);
            string nickname;
            userService.LoginUser(loginModel, out nickname);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoginUserBySeesionKey_WhenSessionKeyIsNull_ShouldThrowException()
        {
            UserService userService = new UserService(mock.Object);
            userService.LoginUser(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void LoginUserBySeesionKey_WhenSessionKeyIsInvalid_ShouldThrowException()
        {
            UserService userService = new UserService(mock.Object);
            userService.LoginUser("InvalidSessionKey");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void LoginUserBySeesionKey_WhenSessionKeyIsValindButNoUserWithHim_ShouldThrowException()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Username = "username",
                    Nickname = "nickname",
                    ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
                    SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                    AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5"
                },
            }.AsQueryable());
            UserService userService = new UserService(mock.Object);
            userService.LoginUser("888431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL");
        }

        [TestMethod]
        public void LoginUserBySeesionKey_WhenCompletedLogin_ShouldReturnId()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id = 1,
                    Username = "username",
                    Nickname = "nickname",
                    ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
                    SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                    AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5"
                },
            }.AsQueryable());
            UserService userService = new UserService(mock.Object);
            int id = userService.LoginUser("100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL");
            Assert.AreEqual( 1,id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogoutUser_WhenSessionKeyIsNull_ShouldThrowException()
        {
            UserService userService = new UserService(mock.Object);
            userService.LogoutUser(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void LogoutUser_WhenSessionKeyIsInvalid_ShouldThrowException()
        {
            UserService userService = new UserService(mock.Object);
            userService.LogoutUser("InvalidSesionKey");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void LogoutUser_WhenSessionKeyIsValindButNoUserWithHim_ShouldThrowException()
        {
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Username = "username",
                    Nickname = "nickname",
                    ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
                    SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                    AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5"
                },
            }.AsQueryable());
            UserService userService = new UserService(mock.Object);
            userService.LogoutUser("888431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL");
        }

        [TestMethod]
        public void LogoutUser_WhenCompletedLogin_ShouldSetSessinKeyToNull()
        {
            User updatedUser = new User();
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id=1,
                    Username = "username",
                    Nickname = "nickname",
                    ConnectionId = "75ccd4c3-fd0f-4a1d-80bb-885fb1bb5296",
                    SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                    AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5"
                },
            }.AsQueryable());
            mock.Setup(g => g.Users.Update(It.IsAny<User>())).Callback((User user) => updatedUser = user);
            UserService userService = new UserService(mock.Object);
            userService.LogoutUser("100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL");
            Assert.IsNull(updatedUser.SessionKey);

        }

        [TestMethod]
        public void GetAllUsers_WhenHasTwoUsersInRepository_ShouldReturnTwoUsers()
        {
           
            mock.Setup(g => g.Users.GetAll()).Returns(new User[]
            {
                new User
                {
                    Id=1,                    
                },
                  new User
                {
                    Id=2,                    
                },
            }.AsQueryable());          
            UserService userService = new UserService(mock.Object);
            IEnumerable<User> users = userService.GetAllUsers();
            Assert.AreEqual(2, users.Count());

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateUsername_WhenUsernameIsNull_ShouldThrowException()
        {
            UserService.ValidateUsername(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateUsername_WhenUsernameIsTooLong_ShouldThrowException()
        {
            UserService.ValidateUsername("1234567890123456789012345678901");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateUsername_WhenUsernameIsTooShort_ShouldThrowException()
        {
            UserService.ValidateUsername("123");
        }

        [TestMethod]
        public void ValidateUsername_WhenUsernameIsValin_ShouldNotThrowException()
        {
            UserService.ValidateUsername("ValindUsername");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateUsername_WhenUsernameContainsInvalidChar_ShouldThrowException()
        {

            UserService.ValidateUsername("!InvalidChar");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateUsername_WhenNicknameIsNull_ShouldThrowException()
        {
            UserService.ValidateNickname(null);

        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateUsername_WhenNicknameIsTooLong_ShouldThrowException()
        {
            UserService.ValidateNickname("1234567890123456789012345678901");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateUsername_WhenNicknameIsTooShort_ShouldThrowException()
        {
            UserService.ValidateNickname("123");
        }

        [TestMethod]
        public void ValidateUsername_WhenNicknameIsValin_ShouldNotThrowException()
        {
            UserService.ValidateNickname("ValindUsername");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateUsername_WhenNicknameContainsInvalidChar_ShouldThrowException()
        {

            UserService.ValidateNickname("!InvalidChar");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateUsername_WhenAuthCodeIsNull_ShouldThrowException()
        {
            UserService.ValidateAuthCode(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateUsername_WhenAuthCodeIsTooLong_ShouldThrowException()
        {

            UserService.ValidateAuthCode("123456789012345678901234567890123456789012345678901");
        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public void ValidateUsername_WhenAuthCodeContainsInvalidChar_ShouldThrowException()
        {

            UserService.ValidateAuthCode("!InvalidChar");
        }
    }
}