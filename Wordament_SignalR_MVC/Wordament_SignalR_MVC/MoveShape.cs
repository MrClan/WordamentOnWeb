using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Wordament_SignalR_MVC.Models;

namespace Wordament_SignalR_MVC
{
    public class Broadcaster
    {
        private readonly static Lazy<Broadcaster> _instance =
            new Lazy<Broadcaster>(() => new Broadcaster());
        // We're going to broadcast to all clients a maximum of 25 times per second
        private readonly TimeSpan BroadcastInterval =
            TimeSpan.FromMilliseconds(40);
        private readonly TimeSpan GameBroadcastInterval =
            TimeSpan.FromMilliseconds(10000);
        private readonly IHubContext _hubContext;
        private Timer _scoreBroadcastLoop;
        private Timer _gameBroadcastLoop;
        private ShapeModel _model;
        private ScoreModel _scoreString;
        private bool _modelUpdated;

        public Broadcaster()
        {
            // Save our hub context so we can easily use it 
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MoveShapeHub>();
            _model = new ShapeModel();
            _scoreString = new ScoreModel() { Score = "DYNAMIC SCORE UPDATE", Name = "" };
            _modelUpdated = false;
            // Start the broadcast loop
            _scoreBroadcastLoop = new Timer(
                BroadcastShape,
                null,
                BroadcastInterval,
                BroadcastInterval);
            //_scoreBroadcastLoop = new Timer(
            //    BroadcastGame,
            //    null,
            //    GameBroadcastInterval,
            //    GameBroadcastInterval);
        }
        public void BroadcastShape(object state)
        {
            //if (MvcApplication.UniversalGameStatus.Status == GameStatusEnum.GAMEON)
            {
                // This is how we can access the Clients property 
                // in a static hub method or outside of the hub entirely
                //_hubContext.Clients.AllExcept(_model.LastUpdatedBy).updateShape(_model);
                _hubContext.Clients.AllExcept(_model.LastUpdatedBy).updateScore(_scoreString);
                _modelUpdated = false;
            }
        }


        public void BroadcastGame(object state)
        {
            if (DisableGamePlay) // game is in paused state, only return results of the previous grid
            {
                _hubContext.Clients.All.DisableGamePlay(new { Status = MvcApplication.UniversalGameStatus, Solution = MvcApplication.CurrentGridSolution });
            }
            else // game is on, distribute current grid to all clients
            {
                var curGrid = MvcApplication.CurrentGrid;
                _hubContext.Clients.All.StartNewGame(new { Status = MvcApplication.UniversalGameStatus.Status, Grid = curGrid, Life = curGrid.LifeLeft });
            }
        }
        public static Broadcaster Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        internal void UpdateScore(ScoreModel scoreString)
        {
            _scoreString = scoreString;
            _modelUpdated = true;
        }

        public static bool DisableGamePlay = false;

    }

    public class MoveShapeHub : Hub
    {
        // Is set via the constructor on each creation
        private Broadcaster _broadcaster;
        public MoveShapeHub()
            : this(Broadcaster.Instance)
        {
        }
        public MoveShapeHub(Broadcaster broadcaster)
        {
            _broadcaster = broadcaster;
        }

        public void UpdateScore(ScoreModel scoreModel)
        {
            scoreModel.LastUpdatedBy = Context.ConnectionId;
            _broadcaster.UpdateScore(scoreModel);
        }
    }
    public class ShapeModel
    {
        // We declare Left and Top as lowercase with 
        // JsonProperty to sync the client and server models
        [JsonProperty("left")]
        public double Left { get; set; }
        [JsonProperty("top")]
        public double Top { get; set; }
        // We don't want the client to get the "LastUpdatedBy" property
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }
    }

    public class ScoreModel
    {
        public string Name { get; set; }
        public string Score { get; set; }
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }
    }
}
