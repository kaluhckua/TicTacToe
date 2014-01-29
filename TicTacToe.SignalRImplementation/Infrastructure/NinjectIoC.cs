using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TicTacToe.Service.Abstract;
using TicTacToe.Service.Concrete;
using TicTacToe.Repository;
using TicTacToe.DataLayer;
using TicTacToe.Models;
using TicTacToe.Service;
using Ninject.Web.Common; 

namespace TicTacToe.SignalRImplementation.Infrastructure
{
    public static class NinjectIoC
    {
        public static IKernel Initialize()
        {
          
            IKernel kernel = new StandardKernel();
          
            kernel.Bind(typeof(IRepository<>)).To(typeof(DBRepository<>)).InRequestScope().WithConstructorArgument("context",new DataContext());
            kernel.Bind<IUserService>().To<UserService>();
            kernel.Bind<IGameService>().To<GameService>();
            kernel.Bind<IGuessService>().To<GuessService>();
            kernel.Bind<IUowData>().To<DBUow>();
           
           
            return kernel;
        }
    }
}