using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM
{
    public class VirtualSSMCOM : SSMCOMBase
    {
        private readonly ILogger logger;
        //コンストラクタ
        public VirtualSSMCOM(ILoggerFactory logger) : base(logger)
        {
            this.logger = logger.CreateLogger<VirtualSSMCOM>();
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

        protected override void communicate_main(bool slow_read)
        {
            var query_SSM_code_list = new List<SSMParameterCode>();
            foreach (SSMParameterCode code in Enum.GetValues(typeof(SSMParameterCode)))
            {
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
            OnSSMDataReceived(this,ssm_received_eventargs);
        }
   }
}