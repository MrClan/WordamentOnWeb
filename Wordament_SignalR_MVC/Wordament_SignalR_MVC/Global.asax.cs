using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Wordament_SignalR_MVC.Controllers;
using Wordament_SignalR_MVC.Models;

namespace Wordament_SignalR_MVC
{
    public class GameStatus
    {
        public GameStatus(GameStatusEnum status, int age)
        {
            CreationTime = DateTime.Now;
        }
        public DateTime CreationTime { get; private set; }
        public GameStatusEnum Status { get; set; }
        public int TimeLeftInSeconds { get { return (int)(DateTime.Now - CreationTime).TotalSeconds; } }
    }

    public enum GameStatusEnum
    {
        GAMEON,
        SCOREDISPLAY
    }
    public class MvcApplication : System.Web.HttpApplication
    {
        static System.Timers.Timer gameStatusTimer;
        protected void Application_Start()
        {
            lst.Add(new GridSolution()); // Add 1 grid intially to start the app quickly
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //AppVars.SetTimeUpToFalse(null); // manually call the first time, after that it will be handler by the timer event
            System.Timers.Timer gridTimer = new System.Timers.Timer(235000); // set interval to 4*60 = 240 seconds
            gridTimer.Elapsed += (sender, args) => { GenerateNewGrid(null); };
            gridTimer.Start();
            // GameStatus timer
            gameStatusTimer = new System.Timers.Timer(30000) { AutoReset = true };
            gameStatusTimer.Elapsed += (sender, args) => { UpdateClientStatus(null); };

            GenerateNewGrid(null);

        }
        public static Grid CurrentGrid = null;
        public static List<string> CurrentGridSolution = null;
        public static GameStatus UniversalGameStatus = null;
        static int servedGridIndex = 0;
        public static DateTime LatestServedTime = DateTime.Now;
        private void GenerateNewGrid(object state)
        {
            var curGrid = lst[servedGridIndex++];
            LatestServedTime = DateTime.Now.AddSeconds(30);
            UniversalGameStatus = new GameStatus(GameStatusEnum.SCOREDISPLAY, 30);
            gameStatusTimer.Start();
            Application["CurrentGrid"] = CurrentGrid = curGrid.Grid;
            Application["CurrentGridSolution"] = CurrentGridSolution = servedGridIndex > 1 ? lst[servedGridIndex - 2].Solution : null;// distribute results of the previous grid
            Broadcaster.DisableGamePlay = true;
            CurrentServedObject = new ServedObject { Status = MvcApplication.UniversalGameStatus, Solution = MvcApplication.CurrentGridSolution };
            GlobalHost.ConnectionManager.GetHubContext<MoveShapeHub>().Clients.All.DisableGamePlay(CurrentServedObject);
            curGrid.Grid.ServedOn = LatestServedTime;
            if ((lst.Count() - servedGridIndex) < 10)
                Task.Factory.StartNew(new Action(AddNewGrids));
        }

        public static ServedObject CurrentServedObject = null;

        private void UpdateClientStatus(object state)
        {
            UniversalGameStatus = new GameStatus(GameStatusEnum.GAMEON, 30);
            Broadcaster.DisableGamePlay = false;
            Application["CurrentGridSolution"] = CurrentGridSolution = null;
            CurrentServedObject = new ServedObject { Status = MvcApplication.UniversalGameStatus, Grid = CurrentGrid, TotalWords = lst[servedGridIndex-1].Solution.Count() };
            GlobalHost.ConnectionManager.GetHubContext<MoveShapeHub>().Clients.All.StartNewGame(CurrentServedObject);
            gameStatusTimer.Stop();
        }

        object lockObj = new object();
        static List<GridSolution> lst = new List<GridSolution>();
        private void AddNewGrids()
        {
            for (int i = 0; i < 10; i++)
            {
                // currently, it should take 12*10 = 120 seconds to fill up the list with 10 items
                lst.Add(new GridSolution());
            }
        }
    }

    public class ServedObject
    {
        public GameStatus Status { get; set; }
        public Grid Grid { get; set; }
        public List<string> Solution { get; set; }
        public int TotalWords { get; set; }
    }
}
