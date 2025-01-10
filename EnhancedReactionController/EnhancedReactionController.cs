using System;
using System.Timers;

namespace SimpleReactionMachine
{
    public class EnhancedReactionController : IController
    {
        private const int min_wt = 100;
        private const int max_wt = 250;
        private const int maxgame_time = 200;
        private const int gt = 300;
        private const double tps = 100.0;
        private const int MaxGamesToPlay = 3; 

        private State _state;
        private IGui _gui { get; set; }
        private IRandom _rng { get; set; }
        private int Ticks { get; set; }
        private int GamesPlayed { get; set; } 
        private double totalTime { get; set; } 
        private double averageTime { get; set; } 

        public void Connect(IGui gui, IRandom rng)
        {
            _gui = gui;
            _rng = rng;
            Init();
        }

        public void CoinInserted()
        {
            _state.CoinInserted();
        }

        public void GoStopPressed()
        {
            _state.GoStopPressed();
        }

        public void Init()
        {
            _state = new OnState(this);
            GamesPlayed = 0;
            totalTime = 0;
            averageTime = 0;
        }

        private void SetState(State state)
        {
            _state = state;
        }

        public void Tick()
        {
            _state.Tick();
        }

        private abstract class State
        {
            protected EnhancedReactionController _controller;

            public State(EnhancedReactionController controller)
            {
                _controller = controller;
            }

            public abstract void CoinInserted();
            public abstract void GoStopPressed();
            public abstract void Tick();
        }

        private class OnState : State
        {
            public OnState(EnhancedReactionController controller) : base(controller)
            {
                _controller._gui.SetDisplay("INSERT COIN");
            }

            public override void CoinInserted()
            {
                _controller.SetState(new ReadyState(_controller));
            }

            public override void GoStopPressed()
            {
            }

            public override void Tick()
            {
            }
        }

        private class ReadyState : State
        {
            private int _readyTicks;

            public ReadyState(EnhancedReactionController controller) : base(controller)
            {
                _controller._gui.SetDisplay("PRESS GO!");
                _controller.Ticks = 0;
                _readyTicks = 0;
            }

            public override void CoinInserted()
            {
            }

            public override void GoStopPressed()
            {
                _controller.SetState(new WaitState(_controller));
                _controller._gui.SetDisplay("Wait...");
            }

            public override void Tick()
            {
                _readyTicks++;
                if (_readyTicks == 1000) // 10 seconds
                {
                    _controller.SetState(new GameOverState(_controller));
                }
            }
        }

        private class WaitState : State
        {
            private int _waitTime;

            public WaitState(EnhancedReactionController controller) : base(controller)
            {
                _controller._gui.SetDisplay("WILL START ANY TIME...");
                _controller.Ticks = 0;
                _waitTime = _controller._rng.GetRandom(min_wt, max_wt);
            }

            public override void CoinInserted()
            {
                
                _controller.Init();
            }

            public override void GoStopPressed()
            {
                _controller.SetState(new GameOverState(_controller));
            }

            public override void Tick()
            {
                _controller.Ticks++;
                if (_controller.Ticks == _waitTime)
                {
                    _controller.SetState(new RunningState(_controller));
                }
            }
        }


        private class RunningState : State
        {
            public RunningState(EnhancedReactionController controller) : base(controller)
            {
                _controller._gui.SetDisplay("0.00");
                _controller.Ticks = 0;
            }

            public override void CoinInserted()
            {
                _controller.Init();
            }

            public override void GoStopPressed()
            {
                double currentTime = _controller.Ticks / tps;
                _controller.totalTime += currentTime;
                _controller.GamesPlayed++;
                if (_controller.GamesPlayed == MaxGamesToPlay)
                {
                    _controller.averageTime = _controller.totalTime / MaxGamesToPlay;
                    _controller.SetState(new GameOverState(_controller));
                }
                else
                {
                    _controller.SetState(new WaitState(_controller));
                    _controller._gui.SetDisplay("Wait...");
                }
            }

            public override void Tick()
            {
                _controller.Ticks++;
                double currentTime = _controller.Ticks / tps;
                _controller._gui.SetDisplay(currentTime.ToString("0.00"));
                if (_controller.Ticks == maxgame_time)
                {
                    GoStopPressed(); 
                }
            }
        }

        private class GameOverState : State
        {
            private int _gameOverTicks;

            public GameOverState(EnhancedReactionController controller) : base(controller)
            {
                if (_controller.GamesPlayed == MaxGamesToPlay)
                {
                    _controller._gui.SetDisplay($"Average = {_controller.averageTime.ToString("0.00")}");
                }
                else
                {
                    _controller._gui.SetDisplay("GAME OVER");
                }
                _gameOverTicks = 0;
            }

            public override void CoinInserted()
            {
               
                _controller.Init();
            }

            public override void GoStopPressed()
            {
                
                Environment.Exit(0);
            }

            public override void Tick()
            {
                _gameOverTicks++;
                if (_gameOverTicks == gt)
                {
                    
                    _controller.Init();
                }
            }
        }
    }
}
