using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication
{
    public interface IBackgroundCommunicate
    {
        void BackgroundCommunicateStart();
        void BackGroundCommunicateStop();
        bool IsCommunitateThreadAlive { get; }
    }
}
