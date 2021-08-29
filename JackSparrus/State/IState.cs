using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackSparrus.State
{
    public interface IState
    {
        string NextStateId
        {
            get;
        }

        void RunState(MainWindow window);
    }
}
