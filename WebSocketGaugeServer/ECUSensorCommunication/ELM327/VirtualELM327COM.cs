using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public class VirtualELM327COM : ELM327COMBase
    {
        private readonly ILogger logger;
        private readonly int WaitTime;
        //Constructor
        public VirtualELM327COM(ILoggerFactory logger, int WaitTime) : base(logger)
        {
            this.logger = logger.CreateLogger<VirtualELM327COM>();
            this.WaitTime = WaitTime;
        }

        protected override void communicate_initialize()
        {
            base.communicate_initialize();
        }

        protected override void communicate_main(bool slow_read_flag)
        {
            //Create PID list to query
            List<OBDIIParameterCode> query_OBDII_code_list = new List<OBDIIParameterCode>();
            foreach (OBDIIParameterCode code in Enum.GetValues(typeof(OBDIIParameterCode)))
            {
                if (slow_read_flag)
                {
                    if (content_table[code].SlowReadEnable)
                    {
                        query_OBDII_code_list.Add(code);
                    }
                }
                else
                {
                    if (content_table[code].FastReadEnable)
                    {
                        query_OBDII_code_list.Add(code);
                    }
                }
            }
            
            //Invoke SSMDatareceived event
            ELM327DataReceivedEventArgs elm327_received_eventargs = new ELM327DataReceivedEventArgs();
            elm327_received_eventargs.Slow_read_flag = slow_read_flag;
            elm327_received_eventargs.Received_Parameter_Code = new List<OBDIIParameterCode>(query_OBDII_code_list);
            OnELM327DataReceived(this, elm327_received_eventargs);
            Thread.Sleep(WaitTime);
        }
    }
}
