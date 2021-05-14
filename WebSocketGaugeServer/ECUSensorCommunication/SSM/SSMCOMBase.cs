using System;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM
{
    public abstract class SSMCOMBase : COMCommon, ISSMCOM
    {
        protected readonly SSMContentTable content_table;

		//SSMCOM data received event
		public event EventHandler<SSMCOMDataReceivedEventArgs> SSMDataReceived;
        private readonly ILogger logger;
        //コンストラクタ
        public SSMCOMBase(ILoggerFactory logger) : base(logger)
        {
            this.logger = logger.CreateLogger<SSMCOMBase>();

            //シリアルポート設定
            DefaultBaudRate = 4800;
            ResetBaudRate = 4800;
            ReadTimeout = 500;

            content_table = new SSMContentTable();
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
            if(!quiet)
                logger.LogDebug("Slowread flag of " + code.ToString() + "is enabled.");
            content_table[code].SlowReadEnable = flag;
        }

		public void set_fastread_flag(SSMParameterCode code, bool flag)
        {
            set_fastread_flag(code, flag, false);
        }
        public void set_fastread_flag(SSMParameterCode code, bool flag, bool quiet)
        {
            if(!quiet)
                logger.LogDebug("Fastread flag of " + code.ToString() + "is enabled.");
            content_table[code].FastReadEnable = flag;
        }

        public void set_all_disable()
        {
            set_all_disable(false);
        }

        public void set_all_disable(bool quiet)
        {
            if(!quiet)
                logger.LogDebug("All flag reset.");
            content_table.setAllDisable();
        }

        protected void OnSSMDataReceived(object s, SSMCOMDataReceivedEventArgs a)
        {
            SSMDataReceived(s, a);
        }
    }
}