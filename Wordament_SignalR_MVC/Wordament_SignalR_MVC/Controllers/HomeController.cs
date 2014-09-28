using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wordament_SignalR_MVC.Models;

namespace Wordament_SignalR_MVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult FirstLoad()
        {
            return Json(MvcApplication.CurrentServedObject, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetGrid()
        {
            var g = (GridSolution)HttpContext.Application["CurrentGrid"];
            var curGrid = g.Grid;
            Session["CurGrid"] = g;
            return Json(new {GameStatus = MvcApplication.UniversalGameStatus.Status, Grid = curGrid, Life = curGrid.LifeLeft}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSolution(string id)
        {
            var retVal = Json(new { isResultsFine = false }, JsonRequestBehavior.AllowGet);
            var g = (GridSolution)Session["CurGrid"];
            // if a user issues a request before time, the solution should not be returned to the user
            //if (g.Grid.LifeLeft < 5)
            {
                var g1 = (GridSolution)HttpContext.Application["CurrentGrid"];
                var curGrid = g1.Grid;
                Session["CurGrid"] = g1;
                if (g.Grid.GUID == g1.Grid.GUID)
                {}
                retVal = Json(new {isResultsFine = true,GameStatus = MvcApplication.UniversalGameStatus.Status, solution = g.Solution, Grid = g1.Grid, Life = g1.Grid.LifeLeft }, JsonRequestBehavior.AllowGet);
            }
            return retVal;
        }
    }



    //public class Solver
    //{
    //    public Grid grid;
    //    public Solver(Grid gridToSolve)
    //    {
    //        grid = gridToSolve;
    //    }

    //    private List<Tile> alphaList = new List<Tile>();
    //    private List<string> foundWords = new List<string>();
    //    private bool IsSolutionFound = false;
    //    private void Solve()
    //    {
    //        alphaList = grid.Tiles;
    //        List<string> foundWords = new List<string>();
    //        var dict = AppVars.Dictionary;
    //        for (int i = 0; i < 16; i++)
    //        {
    //            // start with every word
    //            var curTile = alphaList[i];
    //            var subDict = dict.Where(d => d.StartsWith(curTile.Letter)).ToList();
    //            if (subDict.Count == 0)
    //            {
    //                // no words found, break the loop and continue with next letter in the alphabet
    //                continue;
    //            }
    //            else // iterate recursively to look for words
    //            {
    //                foreach (var x in connectedGrids[curTile.Order].Select(k => alphaList[k.Index()]))
    //                    IsValidWord(curTile.Letter, x, subDict);
    //            }
    //        }
    //        IsSolutionFound = true;
    //    }
    //    private bool IsValidWord(string searchString, Tile t, List<string> dict)
    //    {
    //        searchString += t.Letter;
    //        var subDict = dict.Where(l => l.StartsWith(searchString)).ToList();
    //        if (searchString.Length >= 3 && subDict.Contains(searchString))
    //        {
    //            foundWords.Add(searchString);
    //        }
    //        if (subDict.Count > 0)
    //        {
    //            // check with connected letters
    //            var conLetters = connectedGrids[t.Order].Select(k => alphaList[k.Index()]).ToList();
    //            foreach (var x in conLetters)
    //            {
    //                bool isValidWord = IsValidWord(searchString, x, subDict);
    //            }

    //        }
    //        return false;
    //    }

    //    public List<string> Results()
    //    {
    //        if (!IsSolutionFound)
    //            Solve();
    //        return foundWords;
    //    }

    //    static readonly Dictionary<string, List<string>> connectedGrids = new Dictionary<string, List<string>> { 
    //        {"00",new List<string>{ "01","10","11"}},
    //        {"01",new List<string>{ "00","02","10","11","12"}},
    //        {"02",new List<string>{ "01","03","11","12","13"}},
    //        {"03",new List<string>{ "02","12","13"}},
    //        {"10",new List<string>{ "00","01","11","21","20"}},
    //        {"11",new List<string>{ "00","01","02","10","12","20","21","22"}},
    //        {"12",new List<string>{ "01","02","03","11","13","21","22","23"}},
    //        {"13",new List<string>{ "02","03","12","22","23"}},

    //        {"20",new List<string>{ "10","11","21","30","31"}},
    //        {"21",new List<string>{ "10","11","12","20","22","30","31","32"}},
    //        {"22",new List<string>{ "11","12","13","21","23","31","32","33"}},
    //        {"23",new List<string>{ "12","13","22","32","33"}},

    //        {"30",new List<string>{ "20","21","31"}},
    //        {"31",new List<string>{ "02","21","22","30","32"}},
    //        {"32",new List<string>{ "21","22","23","31","33"}},
    //        {"33",new List<string>{ "22","23","32"}}
    //    };

    //    public static Dictionary<string, List<string>> Solutions = new Dictionary<string, List<string>>();
    //    public void SolveThis()
    //    {
    //        if (!Solutions.ContainsKey(grid.GUID.ToString()))
    //        {
    //            Solve();
    //            Solutions.Add(grid.GUID.ToString(), foundWords);
    //            // save the solution to the file for record
    //            string serializeddString = JsonConvert.SerializeObject(new SerializedSolution() { Grid = grid, Solution = foundWords });
    //            File.WriteAllText(String.Format("{0}/{1}_{2}", ConfigurationManager.AppSettings["SavedDbDir"], "Grid", DateTime.Now.ToString().Replace('/', '-').Replace(':', '-').Replace(' ', '-')), serializeddString);
    //        }
    //    }
    //}

    //public class GridHandler
    //{
    //    private Grid currentGrid;
    //    public Grid GetCurrentGrid
    //    {
    //        get
    //        {
    //            currentGrid = AppVars.CurrentGrid;
    //            return currentGrid;
    //        }
    //    }
    //}
}