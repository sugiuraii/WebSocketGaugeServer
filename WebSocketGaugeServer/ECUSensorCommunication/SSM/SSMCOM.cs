using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM
{
    public class SSMCOM : COMCommon
    {
        private SSMContentTable content_table;

		//SSMCOM data received event
		public event EventHandler<SSMCOMDataReceivedEventArgs> SSMDataReceived;
        private readonly ILogger logger;
        //コンストラクタ
        public SSMCOM(ILogger<SSMCOM> logger) : base(logger)
        {
            this.logger = logger;

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

        protected override void communicate_main(bool slow_read)
        {
            try
            {
                int i;

                byte[] outbuf;

                //クエリするSSM_codeリストの作成
				List<SSMParameterCode> query_SSM_code_list = new List<SSMParameterCode>();

                //送信バッファの作成
                outbuf = create_outbuf(slow_read, query_SSM_code_list);
                    
                //クエリするSSM_codeがない場合は抜ける
                if (query_SSM_code_list.Count <= 0)
                {
                    //SSM_codeがない場合、すぐに抜けると直後にcommunicate_mainが呼び出されCPUを占有するので、500ms待つ
                    //SlowReadの場合、この処理はしない
                    if(!slow_read)
                        Thread.Sleep(500);
                    return;
                }

                //エコーバックサイズの設定
                int echoback_length = outbuf.Length;

                //入力データ長の設定(ヘッダ、サイズ、コマンド、チェックサムで7バイト、1アドレスあたり3バイト、から計算)
                int inbuf_length = echoback_length + (outbuf.Length - 7) / 3 + 6;
                byte[] inbuf = new byte[inbuf_length];

                //シリアルポート入力バッファ掃除
                DiscardInBuffer();

                //クエリ送信
                Write(outbuf, 0, outbuf.Length);

                //受信
                for (i = 0; i < inbuf_length; i++)
                {
                    inbuf[i] = (byte)ReadByte();
                }

                //読み出しオフセットの設定(エコーバックとヘッダを飛ばす）
                int read_offset = echoback_length + 5;

                read_inbuf(inbuf, read_offset, query_SSM_code_list);

				//Invoke SSMDatareceived event
				SSMCOMDataReceivedEventArgs ssm_received_eventargs = new SSMCOMDataReceivedEventArgs();
				ssm_received_eventargs.Slow_read_flag = slow_read;
				ssm_received_eventargs.Received_Parameter_Code = new List<SSMParameterCode>(query_SSM_code_list);
				SSMDataReceived(this,ssm_received_eventargs);
            }
            catch (TimeoutException ex)
            {
                logger.LogWarning("SSMCOM timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
        }

		private byte[] create_outbuf(bool slow_read, List<SSMParameterCode> query_code_list)
        {
            int i;
            int outbuf_sum = 0;
            byte[]outbuf = new byte[] { };


            // outbuf packet = header_bytes + datasize_byte + command_byte(A8=read single addresses) + padding byte(0x00) + address_bytes + checksum_byte
            byte[] header_bytes = new byte[] { 0x80, 0x10, 0xF0 };
            byte[] datasize_byte;
            byte[] command_byte = new byte[] { 0xA8 };
            byte[] padding_byte = new byte[] { 0x00 };
            byte[] checksum_byte;
            byte[] address_bytes = new byte[]{};


            foreach (SSMParameterCode code in Enum.GetValues(typeof(SSMParameterCode)))
            {
                if (slow_read)
                {
                    if (content_table[code].SlowReadEnable)
                    {
                        address_bytes = address_bytes.Concat(content_table[code].ReadAddress).ToArray();
                        query_code_list.Add(code);
                    }
                }
                else
                {
                    if (content_table[code].FastReadEnable)
                    {
                        address_bytes = address_bytes.Concat(content_table[code].ReadAddress).ToArray();
                        query_code_list.Add(code);
                    }
                }
            }

            //datasize計算 (チェックサムは含まれない?)
            int datasize =  command_byte.Length + padding_byte.Length + address_bytes.Length;
            datasize_byte = new byte[] { (byte)datasize };

            //連結
            outbuf = outbuf.Concat(header_bytes).ToArray();
            outbuf = outbuf.Concat(datasize_byte).ToArray();
            outbuf = outbuf.Concat(command_byte).ToArray();
            outbuf = outbuf.Concat(padding_byte).ToArray();
            outbuf = outbuf.Concat(address_bytes).ToArray();

            //チェックサム計算
            for (i = 0; i < outbuf.Length; i++)
            {
                outbuf_sum = outbuf_sum + (int)outbuf[i];
            }

            checksum_byte = new byte[] { (byte)(outbuf_sum & 0xFF) };

            outbuf = outbuf.Concat(checksum_byte).ToArray();

            return outbuf;
        }

		private void read_inbuf(byte[] inbuf, int read_offset, List<SSMParameterCode> query_code_list)
        {
            int get_offset = read_offset;
            foreach (SSMParameterCode code in query_code_list)
            {
                //アドレス3バイトあたりデータ1バイト
                int read_byte_length = content_table[code].AddressLength / 3;

                UInt32 temp_buf = inbuf[get_offset];
                for (int i = 1; i < read_byte_length; i++)
                {
                    temp_buf = (temp_buf << 8) + inbuf[get_offset + i];
                }

                content_table[code].RawValue = temp_buf;

                get_offset += read_byte_length;
            }
        }
    }

    public class SSMCOMDataReceivedEventArgs : EventArgs
    {
	    public bool Slow_read_flag { get; set; }
	    public List<SSMParameterCode> Received_Parameter_Code { get; set; }
	}
}