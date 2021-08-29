using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JackSparrus.State
{
    public class GoHintState : AState
    {
        public override void RunState(MainWindow window)
        {
            base.RunState(window);

            TreasureRow row = window.TreasureHub.GetCurrentTreasureRow();

            //window.WebManager.GetHintDistance()

            Random rand = new Random();

            //int i = 0;
            //bool isThereHint = false;
            //do
            //{
            //    this.ClickOnNextArea(row.Direction);
            //    Thread.Sleep(5000 + rand.Next(0, 500));



            //    i++;
            //}
            //while (isThereHint == false && i < 10);
        }
    }
}
