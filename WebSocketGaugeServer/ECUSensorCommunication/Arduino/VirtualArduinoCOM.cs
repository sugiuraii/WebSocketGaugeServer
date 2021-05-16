using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino
{
    public class VirtualArduinoCOM : IArduinoCOM
    {
        private readonly ArduinoContentTable content_table;
        public event EventHandler ArduinoPacketReceived;
        private readonly ILogger logger;
        private readonly int WaitTime;

        private CancellationTokenSource BackGroundCommunicateCancellationTokenSource = new CancellationTokenSource();
        public bool IsCommunitateThreadAlive { get; private set;} = false;
        public VirtualArduinoCOM(ILoggerFactory logger, int WaitTime)
        {
            this.logger = logger.CreateLogger<VirtualArduinoCOM>();
            this.content_table = new ArduinoContentTable();
            this.WaitTime = WaitTime;
        }
        
        public void SetRawValue(ArduinoParameterCode code, UInt32 rawValue)
        {
            content_table[code].RawValue = rawValue;
        }
        public void BackgroundCommunicateStart()
        {
            Task.Run(() => communicate_main(BackGroundCommunicateCancellationTokenSource.Token));
            IsCommunitateThreadAlive = true;
        }

        public void BackgroundCommunicateStop()
        {
            BackGroundCommunicateCancellationTokenSource.Cancel();
            IsCommunitateThreadAlive = false;
        }

        private async Task communicate_main(CancellationToken ct) //slowread flag is ignored on arduinoCOM
        {
            while(!ct.IsCancellationRequested)
            {
                ArduinoPacketReceived(this, EventArgs.Empty);
                await Task.Delay(WaitTime);
            }
        }

        public double get_value(ArduinoParameterCode code)
        {
            return content_table[code].Value;
        }

        public UInt32 get_raw_value(ArduinoParameterCode code)
        {
            return content_table[code].RawValue;
        }

        public string get_unit(ArduinoParameterCode code)
        {
            return content_table[code].Unit;
        }
    }
}