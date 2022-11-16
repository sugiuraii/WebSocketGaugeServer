using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino
{
    public class VirtualArduinoCOM : IArduinoCOM
    {
        private readonly VirtualArduinoContentTable content_table;
        public event EventHandler ArduinoPacketReceived;
        private readonly ILogger logger;
        private readonly int WaitTime;

        private CancellationTokenSource BackGroundCommunicateCancellationTokenSource = new CancellationTokenSource();
        public bool IsCommunitateThreadAlive { get; private set;} = false;
        public VirtualArduinoCOM(ILoggerFactory logger, int WaitTime)
        {
            this.logger = logger.CreateLogger<VirtualArduinoCOM>();
            this.content_table = new VirtualArduinoContentTable();
            this.WaitTime = WaitTime;
        }
        
        public void SetValue(ArduinoParameterCode code, double value)
        {
            content_table[code].Value = value;
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
                ArduinoPacketReceived(this, EventArgs.Empty);
                await Task.Delay(WaitTime);
            }
            IsCommunitateThreadAlive = false;
        }

        public double get_value(ArduinoParameterCode code)
        {
            return content_table[code].Value;
        }

        public string get_unit(ArduinoParameterCode code)
        {
            return content_table[code].Unit;
        }
    }
}