using System;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino
{
    public abstract class ArduinoCOMBase : COMCommon, IArduinoCOM
    {
        protected ArduinoContentTable content_table;
        public event EventHandler ArduinoPacketReceived;
        private readonly ILogger logger;
        //Constructor
        public ArduinoCOMBase(ILoggerFactory logger) : base(logger)
        {
            this.logger = logger.CreateLogger<ArduinoCOMBase>();
            content_table = new ArduinoContentTable();
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

        protected void OnArduinoPacketReceived(object o, EventArgs e)
        {
            ArduinoPacketReceived(o, e);
        }
    }
}
