using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public abstract class ELM327COMBase : COMCommon, IELM327COM
    {
        protected readonly OBDIIContentTable content_table;
        public event EventHandler<ELM327DataReceivedEventArgs> ELM327DataReceived;

        private readonly ILogger logger;

        //Constructor
        public ELM327COMBase(ILoggerFactory logger) : base(logger)
        {
            this.logger = logger.CreateLogger<ELM327COMBase>();
            content_table = new OBDIIContentTable();
        }
        public double get_value(OBDIIParameterCode code)
        {
            return content_table[code].Value;
        }

        public UInt32 get_raw_value(OBDIIParameterCode code)
        {
            return content_table[code].RawValue;
        }

        public string get_unit(OBDIIParameterCode code)
        {
            return content_table[code].Unit;
        }

        public bool get_slowread_flag(OBDIIParameterCode code)
        {
            return content_table[code].SlowReadEnable;
        }

        public bool get_fastread_flag(OBDIIParameterCode code)
        {
            return content_table[code].FastReadEnable;
        }

        public void set_slowread_flag(OBDIIParameterCode code, bool flag)
        {
            set_slowread_flag(code, flag, false);
        }
        public void set_slowread_flag(OBDIIParameterCode code, bool flag, bool quiet)
        {
            if (!quiet)
                logger.LogDebug("Slowread flag of " + code.ToString() + "is enabled.");
            content_table[code].SlowReadEnable = flag;
        }

        public void set_fastread_flag(OBDIIParameterCode code, bool flag)
        {
            set_fastread_flag(code, flag, false);
        }
        public void set_fastread_flag(OBDIIParameterCode code, bool flag, bool quiet)
        {
            if (!quiet)
                logger.LogDebug("Fastread flag of " + code.ToString() + "is enabled.");
            content_table[code].FastReadEnable = flag;
        }

        public void set_all_disable()
        {
            set_all_disable(false);
        }

        public void set_all_disable(bool quiet)
        {
            if (!quiet)
                logger.LogDebug("All flag reset.");
            content_table.setAllDisable();
        }

        protected void OnELM327DataReceived(object o, ELM327DataReceivedEventArgs a)
        {
            ELM327DataReceived(o, a);           
        }
    }
}
