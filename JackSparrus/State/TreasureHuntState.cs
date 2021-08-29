using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackSparrus.State
{
    public class TreasureHuntState: AState
    {
        protected Dictionary<string, IState> idToNextStates;

        public String StartState
        {
            get;
            set;
        }

        public TreasureHuntState()
        {
            this.idToNextStates = new Dictionary<string, IState>();

            GetHintState getHintState = new GetHintState();
            this.idToNextStates.Add("getHint", getHintState);

            GoPhorreurState goPhorreurState = new GoPhorreurState();
            this.idToNextStates.Add("goPhorreur", goPhorreurState);

            GoHintState goHintState = new GoHintState();
            this.idToNextStates.Add("goHint", goHintState);
        }

        public override void RunState(MainWindow window)
        {
            base.RunState(window);
            IState currentState = this.idToNextStates[this.StartState];
            do
            {
                currentState.RunState(window);

                if(this.idToNextStates.TryGetValue(currentState.NextStateId, out IState nextState))
                {
                    currentState = nextState;
                }
                else
                {
                    currentState = null;
                }
            }
            while (currentState != null);

            window.ResetGoButton();
        }
    }
}
