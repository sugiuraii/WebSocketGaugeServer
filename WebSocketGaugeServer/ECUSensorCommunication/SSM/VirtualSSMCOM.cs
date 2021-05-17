using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM
{
    public class VirtualSSMCOM : ISSMCOM
    {
        private readonly ILogger logger;
        private readonly SSMContentTable content_table;
        //SSMCOM data received event
        public event EventHandler<SSMCOMDataReceivedEventArgs> SSMDataReceived;
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

        public VirtualSSMCOM(ILoggerFactory logger, int WaitTime)
        {
            this.logger = logger.CreateLogger<VirtualSSMCOM>();
            this.content_table = new SSMContentTable();
            this.WaitTime = WaitTime;
        }

        public void SetRawValue(SSMParameterCode code, UInt32 rawValue)
        {
            content_table[code].RawValue = rawValue;
        }

        public void SetSwitch(SSMSwitchCode code, bool value)
        {
            var masterNumericCode = content_table[code].MasterNumericContent;
            int bit_index = content_table[code].BitIndex;
            var rawNumericVal = masterNumericCode.RawValue;
            var newRawNumericVal = (rawNumericVal & ~(1U << bit_index)) | ((value?1U:0U) << bit_index);
            masterNumericCode.RawValue = newRawNumericVal;
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
            bool slow_read = false;

            while (!ct.IsCancellationRequested)
            {
                var query_SSM_code_list = new List<SSMParameterCode>();
                foreach (SSMParameterCode code in Enum.GetValues(typeof(SSMParameterCode)))
                {
                    if(readCount > SlowReadInterval)
                    {
                        slow_read = true;
                        readCount = 0;
                    }
                    else
                    {
                        slow_read = false;
                        readCount++;
                    }

                    if (slow_read)
                    {
                        if (content_table[code].SlowReadEnable)
                            query_SSM_code_list.Add(code);
                    }
                    else
                    {
                        if (content_table[code].FastReadEnable)
                            query_SSM_code_list.Add(code);
                    }
                }
                    
                //Invoke SSMDatareceived event
                SSMCOMDataReceivedEventArgs ssm_received_eventargs = new SSMCOMDataReceivedEventArgs();
                ssm_received_eventargs.Slow_read_flag = slow_read;
                ssm_received_eventargs.Received_Parameter_Code = new List<SSMParameterCode>(query_SSM_code_list);
                SSMDataReceived(this,ssm_received_eventargs);
                await Task.Delay(WaitTime);
            }

            IsCommunitateThreadAlive = false;
        }
        
        public double get_value(SSMParameterCode code)
        {
            return content_table[code].Value;
        }

        public UInt32 get_raw_value(SSMParameterCode code)
        {
            return content_table[code].RawValue;
        }

        public bool get_switch(SSMSwitchCode code)
        {
            return content_table[code].Value;
        }

        public string get_unit(SSMParameterCode code)
        {
            return content_table[code].Unit;
        }

        public bool get_slowread_flag(SSMParameterCode code)
        {
            return content_table[code].SlowReadEnable;
        }

        public bool get_fastread_flag(SSMParameterCode code)
        {
            return content_table[code].FastReadEnable;
        }

        public void set_slowread_flag(SSMParameterCode code, bool flag)
        {
            set_slowread_flag(code, flag, false);
        }
        public void set_slowread_flag(SSMParameterCode code, bool flag, bool quiet)
        {
            if (!quiet)
                logger.LogDebug("Slowread flag of " + code.ToString() + "is enabled.");
            content_table[code].SlowReadEnable = flag;
        }

        public void set_fastread_flag(SSMParameterCode code, bool flag)
        {
            set_fastread_flag(code, flag, false);
        }
        public void set_fastread_flag(SSMParameterCode code, bool flag, bool quiet)
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