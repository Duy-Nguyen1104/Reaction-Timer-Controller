using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleReactionMachine;


namespace SimpleReactionMachine
{
    public class SimpleReactionController : IController
    {
        private IGui gui { get; set; }
        private IRandom random { get; set; }

        private State state;
        private int Ticks;

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
            protected SimpleReactionController controller;
            public State(SimpleReactionController controller)
            {
                this.controller = controller;
            }
            public abstract void CoinInserted();
            public abstract void GoStopPressed();
            public abstract void Tick();
        }
        private class OnState : State
        {
            public OnState(SimpleReactionController controller) : base(controller)
            {
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
            public ReadyState(SimpleReactionController controller) : base(controller)
            {
                controller.gui.SetDisplay("Press GO!");
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
            public WaitState(SimpleReactionController controller) : base(controller)
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
                }
            }
        }
        private class RunningState : State
        {
            public RunningState(SimpleReactionController controller) : base(controller)
            {
                controller.gui.SetDisplay("0.00");
                controller.Ticks = 0;
            }
            public override void CoinInserted() { }
            public override void GoStopPressed()
            {                
                controller.state = new GameOverState(controller);
            }
            public override void Tick()
            {
                controller.Ticks++;
                controller.gui.SetDisplay((controller.Ticks / 100.0).ToString("0.00"));        
                if (controller.Ticks == 200)
                {
                    controller.state = new GameOverState(controller);
                }
            }
        }
        private class GameOverState : State
        {
            public GameOverState(SimpleReactionController controller) : base(controller)
            {
                controller.Ticks = 0;
            }
            public override void CoinInserted() { }
            public override void GoStopPressed()
            {
                controller.state = new OnState(controller);
            }
            public override void Tick()
            {
                //for (int i = 0; i < 3; i++)
                //{
                //    controller.Ticks++;
                //    if (controller.Ticks == 300)
                //    {
                //        controller.state = new WaitState(controller);
                //    }
                //}
                //controller.state = new OnState(controller);

                //controller.Ticks++;
                //if (controller.Ticks == 300)
                //{
                //    controller.state = new OnState(controller);
                //}
            }
        }

    }

}



