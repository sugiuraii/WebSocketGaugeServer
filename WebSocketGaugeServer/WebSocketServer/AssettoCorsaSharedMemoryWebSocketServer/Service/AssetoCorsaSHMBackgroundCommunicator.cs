using AssettoCorsaSharedMemory;
using DefiSSMCOM;
    
namespace SZ2.WebSocketGaugeServer.WebSocketServer.AssettoCorsaSharedMemoryWebSocketServer.Service
{    
    public class AssetoCorsaSHMBackgroundCommunicator : IBackgroundCommunicate
    {
        public readonly AssettoCorsa AssettoCorsaSharedMemory;

        public AssetoCorsaSHMBackgroundCommunicator(double physicsInterval, double graphicsInterval, double staticInfoInterval)
        {
            AssettoCorsaSharedMemory = new AssettoCorsa();
            AssettoCorsaSharedMemory.PhysicsInterval = physicsInterval;
            AssettoCorsaSharedMemory.GraphicsInterval = graphicsInterval;
            AssettoCorsaSharedMemory.StaticInfoInterval = staticInfoInterval;
        }

        public void BackgroundCommunicateStart()
        {
            AssettoCorsaSharedMemory.Start();
        }
        
        public void BackGroundCommunicateStop()
        {
            AssettoCorsaSharedMemory.Stop();
        }

        public bool IsCommunitateThreadAlive 
        {
            get
            {
                return AssettoCorsaSharedMemory.IsRunning;
            }
        }
    }
}