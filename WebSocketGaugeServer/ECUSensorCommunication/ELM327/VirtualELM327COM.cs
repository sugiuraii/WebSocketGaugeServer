using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public class VirtualELM327COM : IELM327COM
    {
        private readonly ILogger logger;
        private readonly OBDIIContentTable content_table;
        public event EventHandler<ELM327DataReceivedEventArgs> ELM327DataReceived;
        public bool IsCommunitateThreadAlive { get; private set; } = false;
        private readonly int WaitTime;
        private CancellationTokenSource BackGroundCommunicateCancellationTokenSource = new CancellationTokenSource();

        public int SlowReadInterval
        {
            get
            {
                return slowReadInterval;
            }
            set
            {
                slowReadInterval = value;
                logger.LogDebug("Set slowread interval to " + value.ToString());
            }
        }
        private int slowReadInterval;

        public VirtualELM327COM(ILoggerFactory logger, int WaitTime)
        {
            this.logger = logger.CreateLogger<VirtualELM327COM>();
            this.content_table = new OBDIIContentTable();
            this.WaitTime = WaitTime;
        }

        public void SetRawValue(OBDIIParameterCode code, UInt32 rawValue)
        {
            content_table[code].RawValue = rawValue;
        }
        public void BackgroundCommunicateStart()
        {
            if (!IsCommunitateThreadAlive)
                Task.Run(() => communicate_main(BackGroundCommunicateCancellationTokenSource.Token));
            else
                throw new InvalidOperationException("BackgroundCommunicateStart() is called. Howevre, background communicate task is already running.");
        }
        public void BackgroundCommunicateStop()
        {
            if (IsCommunitateThreadAlive)
                BackGroundCommunicateCancellationTokenSource.Cancel();
            else
                throw new InvalidOperationException("BackgroundCommunicateStop(). However, background communicate task is already stopped.");
        }

        private async Task communicate_main(CancellationToken ct)
        {
            IsCommunitateThreadAlive = true;
            int readCount = 0;
            bool slow_read_flag = false;

            while (!ct.IsCancellationRequested)
            {
                if(readCount > SlowReadInterval)
                {
                    slow_read_flag = true;
                    readCount = 0;
                }
                else
                {
                    slow_read_flag = false;
                    readCount++;
                }

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
                ELM327DataReceived(this, elm327_received_eventargs);
                await Task.Delay(WaitTime);
            }
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
    }
}
