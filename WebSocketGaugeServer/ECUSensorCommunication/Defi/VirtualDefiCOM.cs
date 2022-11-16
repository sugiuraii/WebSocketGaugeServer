using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi
{
    public class VirtualDefiCOM : IDefiCOM
    {
        private readonly ILogger logger;
        private readonly int WaitTime;
        private readonly DefiContentTable content_table;

		// Defilink received Event
		public event EventHandler DefiPacketReceived;
        private CancellationTokenSource BackGroundCommunicateCancellationTokenSource = new CancellationTokenSource();
        public bool IsCommunitateThreadAlive { get; private set;} = false;

        public VirtualDefiCOM(ILoggerFactory logger, int WaitTime)
        {
            this.logger = logger.CreateLogger<VirtualDefiCOM>();
            this.content_table = new DefiContentTable();
            this.WaitTime = WaitTime;
        }
        public double get_value(DefiParameterCode code)
        {
            return content_table[code].Value;
        }

        public UInt32 get_raw_value(DefiParameterCode code)
        {
            return content_table[code].RawValue;
        }

        public string get_unit(DefiParameterCode code)
        {
            return content_table[code].Unit;
        }

        public void SetRawValue(DefiParameterCode code, UInt32 rawValue)
        {
            content_table[code].RawValue = rawValue;
        }
        public void BackgroundCommunicateStart()
        {
            if(!IsCommunitateThreadAlive)
                Task.Run(() => communicate_main(BackGroundCommunicateCancellationTokenSource.Token));
            else
                throw new InvalidOperationException("BackgroundCommunicateStart() is called. Howevre, background communicate task is already running.");
        }

        public void BackgroundCommunicateStop()
        {
            if(IsCommunitateThreadAlive)
                BackGroundCommunicateCancellationTokenSource.Cancel();
            else
                throw new InvalidOperationException("BackgroundCommunicateStop(). However, background communicate task is already stopped.");
        }

        private async Task communicate_main(CancellationToken ct)
        {
            IsCommunitateThreadAlive = true;
            while(!ct.IsCancellationRequested)
            {
                DefiPacketReceived(this, EventArgs.Empty);
                await Task.Delay(WaitTime);
            }
            IsCommunitateThreadAlive = false;
        }
    }
}