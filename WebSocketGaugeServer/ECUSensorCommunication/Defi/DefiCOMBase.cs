using System;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi
{
    public abstract class DefiCOMBase : COMCommon, IDefiCOM
    {
        protected DefiContentTable content_table;

		// Defilink received Event
		public event EventHandler DefiPacketReceived;

        private readonly ILogger logger;
        public DefiCOMBase(ILoggerFactory logger) : base(logger)
        {
            this.logger = logger.CreateLogger<DefiCOMBase>();
            content_table = new DefiContentTable();
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

        protected void OnDefiPacketReceived(object o, EventArgs e)
        {
            DefiPacketReceived(o, e);
        }
    }
}