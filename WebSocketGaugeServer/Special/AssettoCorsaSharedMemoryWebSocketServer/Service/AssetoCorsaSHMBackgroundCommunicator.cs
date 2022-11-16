using AssettoCorsaSharedMemory;

namespace SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.Service
{
    public class AssetoCorsaSHMBackgroundCommunicator
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