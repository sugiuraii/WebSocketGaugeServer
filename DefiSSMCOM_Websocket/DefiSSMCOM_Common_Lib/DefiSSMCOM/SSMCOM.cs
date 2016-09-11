using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;

namespace DefiSSMCOM
{
    public class SSMCOM : COMCommon
    {
        private SSM_Content_Table content_table;

		//SSMCOM data received event
		public event EventHandler<SSMCOMDataReceivedEventArgs> SSMDataReceived;

        //コンストラクタ
        public SSMCOM()
        {
            //シリアルポート設定
            DefaultBaudRate = 4800;
            ResetBaudRate = 4800;
            ReadTimeout = 500;

            content_table = new SSM_Content_Table();
        }

        public double get_value(SSM_Parameter_Code code)
        {
            return content_table[code].Value;
        }

        public Int32 get_raw_value(SSM_Parameter_Code code)
        {
            return content_table[code].Raw_Value;
        }

        public bool get_switch(SSM_Switch_Code code)
        {
            return content_table[code].Value;
        }

        public string get_unit(SSM_Parameter_Code code)
        {
            return content_table[code].Unit;
        }

		public bool get_slowread_flag(SSM_Parameter_Code code)
		{
			return content_table[code].Slow_Read_Enable;
		}

		public bool get_fastread_flag(SSM_Parameter_Code code)
		{
			return content_table[code].Fast_Read_Enable;
		}

		public void set_slowread_flag(SSM_Parameter_Code code, bool flag)
        {
            set_slowread_flag(code, flag, false);
        }
        public void set_slowread_flag(SSM_Parameter_Code code, bool flag, bool quiet)
        {
            if(!quiet)
                logger.Debug("Slowread flag of " + code.ToString() + "is enabled.");
            content_table[code].Slow_Read_Enable = flag;
        }

		public void set_fastread_flag(SSM_Parameter_Code code, bool flag)
        {
            set_fastread_flag(code, flag, false);
        }
        public void set_fastread_flag(SSM_Parameter_Code code, bool flag, bool quiet)
        {
            if(!quiet)
                logger.Debug("Fastread flag of " + code.ToString() + "is enabled.");
            content_table[code].Fast_Read_Enable = flag;
        }

        public void set_all_disable()
        {
            set_all_disable(false);
        }

        public void set_all_disable(bool quiet)
        {
            if(!quiet)
                logger.Debug("All flag reset.");
            content_table.set_all_disable();
        }

        protected override void communicate_main(bool slow_read)
        {
            try
            {
                int i;

                byte[] outbuf;

                //クエリするSSM_codeリストの作成
				List<SSM_Parameter_Code> query_SSM_code_list = new List<SSM_Parameter_Code>();

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
				ssm_received_eventargs.Received_Parameter_Code = new List<SSM_Parameter_Code>(query_SSM_code_list);
				SSMDataReceived(this,ssm_received_eventargs);
            }
            catch (TimeoutException ex)
            {
                logger.Warn("SSMCOM timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
        }

		private byte[] create_outbuf(bool slow_read, List<SSM_Parameter_Code> query_code_list)
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


            foreach (SSM_Parameter_Code code in Enum.GetValues(typeof(SSM_Parameter_Code)))
            {
                if (slow_read)
                {
                    if (content_table[code].Slow_Read_Enable)
                    {
                        address_bytes = address_bytes.Concat(content_table[code].Read_Address).ToArray();
                        query_code_list.Add(code);
                    }
                }
                else
                {
                    if (content_table[code].Fast_Read_Enable)
                    {
                        address_bytes = address_bytes.Concat(content_table[code].Read_Address).ToArray();
                        query_code_list.Add(code);
                    }
                }
            }

            //datasize計算 (チェックサムは含まれない?)
            Int32 datasize =  command_byte.Length + padding_byte.Length + address_bytes.Length;
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

		private void read_inbuf(byte[] inbuf, int read_offset, List<SSM_Parameter_Code> query_code_list)
        {
            int get_offset = read_offset;
            foreach (SSM_Parameter_Code code in query_code_list)
            {
                //アドレス3バイトあたりデータ1バイト
                int read_byte_length = content_table[code].Address_Length / 3;

                int i;
                int temp_buf = inbuf[get_offset];
                for (i = 1; i < read_byte_length; i++)
                {
                    temp_buf = (temp_buf << 8) + inbuf[get_offset + i];
                }

                content_table[code].Raw_Value = temp_buf;

                get_offset += read_byte_length;
            }
        }
    }

    public class SSMCOMDataReceivedEventArgs : EventArgs
    {
	    public bool Slow_read_flag { get; set; }
	    public List<SSM_Parameter_Code> Received_Parameter_Code { get; set; }
	}
}