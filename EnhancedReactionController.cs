using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleReactionMachine;

namespace SimpleReactionMachine
{
    public class EnhancedReactionController : IController
    {
        private IGui gui { get; set; }
        private IRandom random { get; set; }

        private State state;
        private int Ticks { get; set; }
        private int TimeRepeat {  get; set; }
        private double TotalReactionTime { get; set; }

        public void Connect(IGui gui, IRandom rng)
        {
            this.gui = gui;
            this.random = rng;
            Init();
        }

        //Called to initialise the controller
        public void Init()
        {
            state = new OnState(this);
        }

        //Called whenever a coin is inserted into the machine
        public void CoinInserted()
        {
            state.CoinInserted();
        }

        //Called whenever the go/stop button is pressed
        public void GoStopPressed()
        {
            state.GoStopPressed();
        }

        //Called to deliver a TICK to the controller
        public void Tick()
        {
            state.Tick();
        }
        private abstract class State
        {
            protected EnhancedReactionController controller;
            public State(EnhancedReactionController controller)
            {
                this.controller = controller;
            }
            public abstract void CoinInserted();
            public abstract void GoStopPressed();
            public abstract void Tick();
        }
        private class OnState : State
        {
            public OnState(EnhancedReactionController controller) : base(controller)
            {
                controller.TimeRepeat = 0;
                controller.TotalReactionTime = 0;
                controller.gui.SetDisplay("Insert coin");
                
            }
            public override void CoinInserted()
            {
                controller.state = new ReadyState(controller);
            }
            public override void GoStopPressed() { }
            public override void Tick() { }
        }
        private class ReadyState : State
        {
            public ReadyState(EnhancedReactionController controller) : base(controller)
            {
                controller.gui.SetDisplay("Press GO!");
                controller.Ticks = 0;

            }
            public override void CoinInserted()
            {
            }
            public override void GoStopPressed()
            {
                controller.state = new WaitState(controller);
            }
            public override void Tick()
            {
                controller.Ticks++;
                if (controller.Ticks == 1000)
                {
                    controller.state = new OnState(controller);
                }
            }
        }
        private class WaitState : State
        {
            private int WaitTime;
            public WaitState(EnhancedReactionController controller) : base(controller)
            {
                controller.gui.SetDisplay("Wait...");
                controller.Ticks = 0;
                WaitTime = controller.random.GetRandom(100, 250);
            }
            public override void CoinInserted() { }
            public override void GoStopPressed()
            {
                controller.state = new OnState(controller);
            }
            public override void Tick()
            {
                controller.Ticks++;
                if (controller.Ticks == WaitTime)
                {
                    controller.state = new RunningState(controller);
                    controller.TimeRepeat++;
                }
            }
        }
        private class RunningState : State
        {
            public RunningState(EnhancedReactionController controller) : base(controller)
            {
                controller.gui.SetDisplay("0.00");
                controller.Ticks = 0;
            }
            public override void CoinInserted() { }
            public override void GoStopPressed()
            {
                controller.TotalReactionTime += controller.Ticks;

                controller.state = new GameOverState(controller);
            }
            public override void Tick()
            {
                controller.Ticks++;
                controller.gui.SetDisplay((controller.Ticks / 100.0).ToString("0.00"));
                if (controller.Ticks == 200)
                {
                    controller.TotalReactionTime += controller.Ticks;
                    controller.state = new GameOverState(controller);                    
                }
            }
        }
        private class GameOverState : State
        {
            public GameOverState(EnhancedReactionController controller) : base(controller)
            {
                controller.Ticks = 0;
            }
            public override void CoinInserted() { }
            public override void GoStopPressed()
            {
                CheckGames();
            }
            public override void Tick()
            {                
                controller.Ticks++;
                if (controller.Ticks == 300)
                {
                    CheckGames();
                }
            }
            private void CheckGames()
            {
                if (controller.TimeRepeat == 3)
                {
                    controller.state = new ResultState(controller);
                    return;
                }
                controller.state = new WaitState(controller);
            }
        }

        private class ResultState : State
        {
            public ResultState(EnhancedReactionController controller) : base(controller)
            {
                controller.gui.SetDisplay("Average = "
                   + ((controller.TotalReactionTime / controller.TimeRepeat) / 100.0).ToString("0.00"));
                controller.Ticks = 0;
            }
            public override void CoinInserted() { }
            public override void GoStopPressed()
            {
                controller.state = new OnState(controller);
            }
            public override void Tick()
            {
                controller.Ticks++;
                if (controller.Ticks == 500)
                {
                    controller.state = new OnState(controller);

                }
            }
        }
    }
}
