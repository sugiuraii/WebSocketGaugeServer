using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi
{
    public class VirtualDefiCOM : DefiCOMBase
    {
        private readonly ILogger logger;
        private readonly int WaitTime;
        
        public VirtualDefiCOM(ILoggerFactory logger, int WaitTime) : base(logger)
        {
            this.logger = logger.CreateLogger<VirtualDefiCOM>();
            this.WaitTime = WaitTime;
        }
        
        public void SetRawValue(DefiParameterCode code, UInt32 rawValue)
        {
            content_table[code].RawValue = rawValue;
        }

        protected override void communicate_main(bool slowread_flag) //slowread flag is ignored on arduinoCOM
        {
            OnDefiPacketReceived(this, EventArgs.Empty);
            Thread.Sleep(WaitTime);
        }
    }
}