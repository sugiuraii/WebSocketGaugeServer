using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefiSSMCOM_Common_Lib.DefiSSMCOM
{
    public interface IBackgroundCommunicate
    {
        void BackgroundCommunicateStart();
        void BackGroundCommunicateStop();
        bool IsCommunitateThreadAlive { get; }
    }
}
