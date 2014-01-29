using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Ninject;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TicTacToe.DataLayer;
using TicTacToe.DataLayer.Migrations;
using TicTacToe.SignalRImplementation.App_Start;
using TicTacToe.SignalRImplementation.Infrastructure;

namespace TicTacToe.SignalRImplementation
{
    // Note: For instructions on enabling IIS7 classic mode, 
    // visit http://go.microsoft.com/fwlink/?LinkId=301868
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DataContext, Configuration>());
            GlobalHost.DependencyResolver = new NinjectSignalRDependencyResolver(NinjectIoC.Initialize());

            RouteTable.Routes.MapHubs();
        }
    }
}
