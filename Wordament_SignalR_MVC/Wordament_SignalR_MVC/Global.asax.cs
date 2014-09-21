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
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //AppVars.SetTimeUpToFalse(null); // manually call the first time, after that it will be handler by the timer event
            System.Timers.Timer gridTimer = new System.Timers.Timer(240000); // set interval to 4*60 = 240 seconds
            gridTimer.Elapsed += (sender, args) => { GenerateNewGrid(null); };
            gridTimer.Start();
            Application["GridSolutions"] = new Dictionary<string, List<string>>();
            Application["ConnectedGrids"] = new Dictionary<string, List<string>> { 
                                                    {"00",new List<string>{ "01","10","11"}},
                                                    {"01",new List<string>{ "00","02","10","11","12"}},
                                                    {"02",new List<string>{ "01","03","11","12","13"}},
                                                    {"03",new List<string>{ "02","12","13"}},
                                                    {"10",new List<string>{ "00","01","11","21","20"}},
                                                    {"11",new List<string>{ "00","01","02","10","12","20","21","22"}},
                                                    {"12",new List<string>{ "01","02","03","11","13","21","22","23"}},
                                                    {"13",new List<string>{ "02","03","12","22","23"}},

                                                    {"20",new List<string>{ "10","11","21","30","31"}},
                                                    {"21",new List<string>{ "10","11","12","20","22","30","31","32"}},
                                                    {"22",new List<string>{ "11","12","13","21","23","31","32","33"}},
                                                    {"23",new List<string>{ "12","13","22","32","33"}},

                                                    {"30",new List<string>{ "20","21","31"}},
                                                    {"31",new List<string>{ "02","21","22","30","32"}},
                                                    {"32",new List<string>{ "21","22","23","31","33"}},
                                                    {"33",new List<string>{ "22","23","32"}}
                                            };
            GenerateNewGrid(null);

        }

        static bool isDictLoaded = false;

        private void GenerateNewGrid(object state)
        {
            LoadDictionary(HttpRuntime.AppDomainAppPath + "/" + System.Configuration.ConfigurationManager.AppSettings["DictionaryPath"] + "/Dictionary.txt");
            //if (Application["CurrentGrid"] == null)
            Application["CurrentGrid"] = new Grid();
            Task.Factory.StartNew(new Action(SolveGrid));
        }
        
        private void LoadDictionary(string dictPath)
        {
            if (!isDictLoaded)
                using (StreamReader sr = new StreamReader(dictPath))
                {
                    List<string> allWords = sr.ReadToEnd().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).OrderBy(k => k[0]).ToList();
                    Application["Dictionary"] = allWords;
                    isDictLoaded = true;
                }
        }

        object lockObj = new object();
        private void SolveGrid()
        {
            lock (lockObj)
            {
                var Solutions = (Dictionary<string, List<string>>)Application["GridSolutions"];
                if (!Solutions.ContainsKey(((Grid)Application["CurrentGrid"]).GUID.ToString()))
                {
                    Solve((Grid)Application["CurrentGrid"]);
                    Solutions.Add(((Grid)Application["CurrentGrid"]).GUID.ToString(), foundWords);
                    // save the solution to the file for record
                    string serializeddString = JsonConvert.SerializeObject(new SerializedSolution() { Grid = (Grid)Application["CurrentGrid"], Solution = foundWords });
                    File.WriteAllText(String.Format("{0}/{1}/{2}_{3}", HttpRuntime.AppDomainAppPath, ConfigurationManager.AppSettings["SavedDbDir"], "Grid", DateTime.Now.ToString().Replace('/', '-').Replace(':', '-').Replace(' ', '-')), serializeddString);
                }
            }
        }

        private List<Tile> alphaList = new List<Tile>();
        private List<string> foundWords = new List<string>();
        private void Solve(Grid grid)
        {
            this.foundWords.Clear();
            alphaList = grid.Tiles;
            List<string> foundWords = new List<string>();
            var dict = (List<string>)Application["Dictionary"];
            for (int i = 0; i < 16; i++)
            {
                // start with every word
                var curTile = alphaList[i];
                var subDict = dict.Where(d => d.StartsWith(curTile.Letter)).ToList();
                if (subDict.Count == 0)
                {
                    // no words found, break the loop and continue with next letter in the alphabet
                    continue;
                }
                else // iterate recursively to look for words
                {
                    foreach (var x in ConnectedGrids[curTile.Order].Select(k => alphaList[k.Index()]))
                        IsValidWord(curTile.Letter, x, subDict);
                }
            }
        }
        private bool IsValidWord(string searchString, Tile t, List<string> dict)
        {
            searchString += t.Letter;
            var subDict = dict.Where(l => l.StartsWith(searchString)).ToList();
            if (searchString.Length >= 3 && subDict.Contains(searchString))
            {
                foundWords.Add(searchString);
            }
            if (subDict.Count > 0)
            {
                // check with connected letters
                var conLetters = ConnectedGrids[t.Order].Select(k => alphaList[k.Index()]).ToList();
                foreach (var x in conLetters)
                {
                    bool isValidWord = IsValidWord(searchString, x, subDict);
                }

            }
            return false;
        }

        private Dictionary<string, List<string>> ConnectedGrids
        {
            get
            {
                return (Dictionary<string, List<string>>)Application["ConnectedGrids"];
            }
        }
    }


    //public static class AppVars
    //{
    //    private static Grid curGrid = null;
    //    public static List<string> Dictionary;
    //    public static Grid CurrentGrid
    //    {
    //        get
    //        {
    //            return RefreshGrid();
    //        }
    //    }
    //    private static bool isDictLoaded = false;
    //    private static string dictPath = "";
    //    static AppVars()
    //    {
    //        dictPath = System.Configuration.ConfigurationManager.AppSettings["DictionaryPath"];
    //        LoadDictionary();
    //    }

    //    private static void LoadDictionary()
    //    {
    //        if (!isDictLoaded)
    //            using (StreamReader sr = new StreamReader(dictPath))
    //            {
    //                List<string> allWords = sr.ReadToEnd().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).OrderBy(k => k[0]).ToList();
    //                AppVars.Dictionary = allWords;
    //                isDictLoaded = true;
    //            }
    //    }

    //    private static Grid RefreshGrid()
    //    {
    //        if (!isDictLoaded)
    //            LoadDictionary();
    //        if (curGrid == null)
    //        {
    //            curGrid = new Grid();
    //            if (!gridSolvingState.Contains(curGrid.GUID.ToString()))
    //            {
    //                gridSolvingState.Add(curGrid.GUID.ToString());
    //                Task.Factory.StartNew(new Action(new Solver(curGrid).SolveThis));
    //            }
    //        }
    //        return curGrid;
    //    }
    //    static List<string> gridSolvingState = new List<string>();

    //    static object obj = new object();
    //    public static void SetTimeUpToFalse(object state)
    //    {
    //        lock (obj)
    //        {
    //            curGrid = null;
    //            RefreshGrid();
    //        }
    //    }
    //}
}
