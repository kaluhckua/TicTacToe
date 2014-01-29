using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using TicTacToe.Models;
using System.Transactions;
using TicTacToe.DataLayer;

namespace TicTacToe.Repository.Test
{
    [TestClass]
    public class DBUserRepositoryTests
    {
        public DbContext dbContext { get; set; }
       
        public IUowData uowData { get; set; }

        private static TransactionScope tranScope;

        public DBUserRepositoryTests()
        {
            this.dbContext = new DataContext();
            this.uowData = new DBUow(this.dbContext);
        }
        [TestInitialize]
        public void TestInit()
        {
            tranScope = new TransactionScope();
        }

        [TestCleanup]
        public void TestTearDown()
        {
            tranScope.Dispose();
        }

        [TestMethod]
        public void AddUser_WhenFieldsIsValid_ShouldBeInDb()
        {
            var username = "kalin";
            User user = new User()
            {
                Username = "kalin",
                Nickname = "kaluhckua",
                AuthCode = "80a63648010756ed51eecccf94c01bc0015048c5",
                SessionKey = "100431CZhiZTwwJAh8VTo559HfIyYf8lXyq74Bi2UkBP64ZoLL",
                ConnectionId = "5262b767-b2cc-46a9-9142-10fef01ded1e",
            };
            uowData.Users.Add(user);
            uowData.SaveChanges();           
            var actual = dbContext.Set<User>().Find(user.Id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.Username, username);

        }

        public void UpdateUser_WhenFieldsIsValid_ShouldBeInDb()
        {
        }
    }
}
