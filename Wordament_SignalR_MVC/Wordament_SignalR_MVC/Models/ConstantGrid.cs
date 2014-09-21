using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wordament_SignalR_MVC.Models
{

    public static class CurrentGrid
    {
        private static readonly Grid g;
        static CurrentGrid()
        {
            g = new Grid();
        }
        public static Grid Get { get { return g; } }
    }

    public class Tile
    {
        public string Letter { get; set; }
        public int Weight { get; set; }
        public string Order { get; set; }
    }
    public class Grid
    {
        public Guid GUID { get; private set; }
        public DateTime StartTime { get; set; }
        public List<Tile> Tiles { get; private set; }

        #region Alpha Declarations
        private List<string> alphabets = new List<string>() { "A", "A", "B", "C", "D", "E", "E", "F", "I", "G", "H", "I", "J", "K", "L", "M", "M", "N", "O", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        private Dictionary<string, int> alphaToWeight = new Dictionary<string, int>() { 
            {"A", 2},{"B", 6},{"C", 2},{"D", 4},{"E", 2},{"F", 4},{"G", 4},{"H", 6},{"I", 2},{"J", 12},{"K", 9},{"L", 2},{"M", 3},{"N", 3},{"O", 2},{"P", 3},{"Q", 12},{"R", 2},{"S", 2},{"T", 2},{"U", 2},{"V", 2},{"W", 2},{"X", 12},{"Y", 2},{"Z", 15}
        };

        private static Dictionary<string, double> letterFrequency = new Dictionary<string, double> { 
            {"E",12.02},
            {"T",9.10 },
            {"A",8.12 },
            {"O",7.68 },
            {"I",7.31 },
            {"N",6.95 },
            {"S",6.28 },
            {"R",6.02 },
            {"H",5.92 },
            {"D",4.32 },
            {"L",3.98 },
            {"U",2.88 },
            {"C",2.71 },
            {"M",2.61 },
            {"F",2.30 },
            {"Y",2.11 },
            {"W",2.09 },
            {"G",2.03 },
            {"P",1.82 },
            {"B",1.49 },
            {"V",1.11 },
            {"K",0.69 },
            {"X",0.17 },
            {"Q",0.11 },
            {"J",0.10 },
            {"Z",0.0  }
        };
        #endregion
        public Grid()
        {
            this.GUID = Guid.NewGuid();
            this.StartTime = DateTime.Now;
            this.Tiles = new List<Tile>(16);
            GenAlphabetTiles(); // generate alphabet list based on cumulative letter frequency
        }

        private void GenAlphabetTiles()
        {
            List<string> alphaList = new List<string>();
            #region ElementsWithProb
            List<KeyValuePair<string, double>> elements = new List<KeyValuePair<string, double>>() { 
                new KeyValuePair<string,double>("E",12.02),
                new KeyValuePair<string,double>("T",9.10 ),
                new KeyValuePair<string,double>("A",8.12 ),
                new KeyValuePair<string,double>("O",7.68 ),
                new KeyValuePair<string,double>("I",7.31 ),
                new KeyValuePair<string,double>("N",6.95 ),
                new KeyValuePair<string,double>("S",6.28 ),
                new KeyValuePair<string,double>("R",6.02 ),
                new KeyValuePair<string,double>("H",5.92 ),
                new KeyValuePair<string,double>("D",4.32 ),
                new KeyValuePair<string,double>("L",3.98 ),
                new KeyValuePair<string,double>("U",2.88 ),
                new KeyValuePair<string,double>("C",2.71 ),
                new KeyValuePair<string,double>("M",2.61 ),
                new KeyValuePair<string,double>("F",2.30 ),
                new KeyValuePair<string,double>("Y",2.11 ),
                new KeyValuePair<string,double>("W",2.09 ),
                new KeyValuePair<string,double>("G",2.03 ),
                new KeyValuePair<string,double>("P",1.82 ),
                new KeyValuePair<string,double>("B",1.49 ),
                new KeyValuePair<string,double>("V",1.11 ),
                new KeyValuePair<string,double>("K",0.69 ),
                new KeyValuePair<string,double>("X",0.17 ),
                new KeyValuePair<string,double>("Q",0.11 ),
                new KeyValuePair<string,double>("J",0.10 ),
                new KeyValuePair<string,double>("Z",0.07 )
            };
            #endregion
            Random r = new Random();
            string selectedElement = "";
            List<KeyValuePair<string, double>> sl = elements.OrderBy(e => e.Value).ToList();
            for (int x = 0; x < 16; x++)
            {
                double diceRoll = r.NextDouble() * 100;
                double cumulative = 0.0;
                for (int i = 0; i < sl.Count; i++)
                {
                    cumulative += sl[i].Value;
                    if (diceRoll < cumulative)
                    {
                        selectedElement = sl[i].Key;
                        alphaList.Add(selectedElement);
                        // Add the generated alphabets to tiles
                        this.Tiles.Add(new Tile { Letter = alphaList[x], Weight = alphaToWeight[alphabets[x]], Order = x.Order() });
                        break;
                    }
                }
            }
        }
    }

    public static class Extensions
    {
        public static int Index(this string order)
        {
            switch (order)
            {
                case "00":
                    return 0;
                case "01":
                    return 1;
                case "02":
                    return 2;
                case "03":
                    return 3;
                case "10":
                    return 4;
                case "11":
                    return 5;
                case "12":
                    return 6;
                case "13":
                    return 7;
                case "20":
                    return 8;
                case "21":
                    return 9;
                case "22":
                    return 10;
                case "23":
                    return 11;
                case "30":
                    return 12;
                case "31":
                    return 13;
                case "32":
                    return 14;
                case "33":
                    return 15;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// GetOrderByIndex
        /// </summary>
        /// <param name="index">grid index</param>
        /// <returns></returns>
        public static string Order(this int index)
        {
            switch (index)
            {
                case 0:
                    return "00";
                case 1:
                    return "01";
                case 2:
                    return "02";
                case 3:
                    return "03";
                case 4:
                    return "10";
                case 5:
                    return "11";
                case 6:
                    return "12";
                case 7:
                    return "13";
                case 8:
                    return "20";
                case 9:
                    return "21";
                case 10:
                    return "22";
                case 11:
                    return "23";
                case 12:
                    return "30";
                case 13:
                    return "31";
                case 14:
                    return "32";
                case 15:
                    return "33";
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    public class SerializedSolution
    {
        public Grid Grid { get; set; }
        public List<string> Solution { get; set; }
    }
}