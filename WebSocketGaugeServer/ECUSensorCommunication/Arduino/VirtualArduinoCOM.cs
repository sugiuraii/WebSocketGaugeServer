using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino
{
    public class VirtualArduinoCOM : ArduinoCOMBase
    {
        private readonly ILogger logger;
        private readonly int WaitTime;
        private CancellationTokenSource cancellationTokenSource;
        
        public VirtualArduinoCOM(ILoggerFactory logger, int WaitTime) : base(logger)
        {
            this.logger = logger.CreateLogger<VirtualArduinoCOM>();
            this.WaitTime = WaitTime;
        }
        
        public void SetRawValue(ArduinoParameterCode code, UInt32 rawValue)
        {
            content_table[code].RawValue = rawValue;
        }

        protected override void communicate_main(bool slowread_flag) //slowread flag is ignored on arduinoCOM
        {
            OnArduinoPacketReceived(this, EventArgs.Empty);
            Thread.Sleep(WaitTime);
        }
    }
}