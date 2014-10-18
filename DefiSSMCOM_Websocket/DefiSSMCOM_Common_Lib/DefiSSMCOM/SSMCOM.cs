using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace DefiSSMCOM
{
    namespace SSM
    {
        public class SSMCOM
        {
            private System.IO.Ports.SerialPort serialPort1;
			private string _portname;
            private Int32 _slowread_interval;
            private Thread communicate_realtime_thread1;
            private bool _communicate_realtime_state1;

            private SSM_Content_Table _content_table;

			//SSMCOM data received event
			public event EventHandler<SSMCOMDataReceivedEventArgs> SSMDataReceived;

            //Log4net logger
            static private readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            //コンストラクタ
            public SSMCOM()
            {
                serialPort1 = new System.IO.Ports.SerialPort();
				PortName = "COM1";
                Slow_Read_Interval = 10;

                //シリアルポート設定
                serialPort1.BaudRate = 4800;
                serialPort1.ReadTimeout = 500;
                _communicate_realtime_state1 = false;

                _content_table = new SSM_Content_Table();
            }

			public string PortName
            {
                get
                {
                    return _portname;
                }
                set
                {
                    try
                    {
                        _portname = value;
                        serialPort1.PortName = _portname;
                    }
                    catch (System.InvalidOperationException ex1)
                    {
                        error_message("PortName error : " + ex1.GetType().ToString() + " " + ex1.Message);
                    }
                }
            }

            public double get_value(SSM_Parameter_Code code)
            {
                return _content_table[code].Value;
            }

            public Int32 get_raw_value(SSM_Parameter_Code code)
            {
                return _content_table[code].Raw_Value;
            }

            public bool get_switch(SSM_Switch_Code code)
            {
                return _content_table[code].Value;
            }

            public string get_unit(SSM_Parameter_Code code)
            {
                return _content_table[code].Unit;
            }

			public bool get_slowread_flag(SSM_Parameter_Code code)
			{
				return _content_table[code].Slow_Read_Enable;
			}

			public bool get_fastread_flag(SSM_Parameter_Code code)
			{
				return _content_table[code].Fast_Read_Enable;
			}

			public void set_slowread_flag(SSM_Parameter_Code code, bool flag)
            {
                info_message("Slowread flag of " + code.ToString() + "is enabled.");
				_content_table[code].Slow_Read_Enable = flag;
            }
			public void set_fastread_flag(SSM_Parameter_Code code, bool flag)
            {
                info_message("Fastread flag of " + code.ToString() + "is enabled.");
				_content_table[code].Fast_Read_Enable = flag;
            }

            public void set_all_disable()
            {
                info_message("All flag reset.");
                _content_table.set_all_disable();
            }

            public int Slow_Read_Interval
            {
                get
                {
                    return _slowread_interval;
                }
                set
                {
                    info_message("Set slowread interval to " + value.ToString());
                    _slowread_interval = value;
                }
            }

            public void communicate_start()
            {
                communicate_realtime_thread1 = new Thread(new ThreadStart(communicate_realtime));
                _communicate_realtime_state1 = true;
                communicate_realtime_thread1.Start();
                info_message("SSMCom communication Started.");
            }

            public void communicate_stop()
            {
                //通信スレッドを終了させる
                _communicate_realtime_state1 = false;

                //通信スレッド終了まで待つ
                communicate_realtime_thread1.Join();

                info_message("SSMCom communication Stopped.");
            }

            //無限ループ使用版の読み込み無限ループスレッド実装（communicate_realtime_start()からスレッドを作って呼び出すこと）
            private void communicate_realtime()
            {
                //ポートオープン
                try
                {
                    serialPort1.Open();
                    info_message("SSMCOM COMPort open.");

                    int i = 0;
                    //スレッドがアボートされるまで続ける（無限ループ）
                    while (_communicate_realtime_state1)
                    {
                        if (i > _slowread_interval)
                        {
                            communicate_main(true);
                            i = 0;
                        }
                        else
                        {
                            communicate_main(false);
                            i++;
                        }
                    }
                }
                catch (System.IO.IOException ex)
                {
                    error_message(ex.GetType().ToString() + " " + ex.Message);
                }
                catch (System.InvalidOperationException ex)
                {
                    error_message(ex.GetType().ToString() + " " + ex.Message);
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    error_message(ex.GetType().ToString() + " " + ex.Message);
                }
                finally
                {
                    //ポートクローズ
                    serialPort1.Close();
                    info_message("SSMCOM COMPort closed.");
                    _communicate_realtime_state1 = false;
                }
            }

            private void communicate_main(bool slow_read)
            {
                try
                {
                    int i;

                    byte[] outbuf;

                    //クエリするSSM_codeリストの作成
					List<SSM_Parameter_Code> query_SSM_code_list = new List<SSM_Parameter_Code>();

                    //送信バッファの作成
                    outbuf = create_outbuf(slow_read, query_SSM_code_list);

                    //エコーバックサイズの設定
                    int echoback_length = outbuf.Length;

                    //入力データ長の設定(ヘッダ、サイズ、コマンド、チェックサムで7バイト、1アドレスあたり3バイト、から計算)
                    int inbuf_length = echoback_length + (outbuf.Length - 7) / 3 + 6;
                    byte[] inbuf = new byte[inbuf_length];

                    //シリアルポート入力バッファ掃除
                    serialPort1.DiscardInBuffer();

                    //クエリ送信
                    serialPort1.Write(outbuf, 0, outbuf.Length);

                    //受信
                    for (i = 0; i < inbuf_length; i++)
                    {
                        inbuf[i] = (byte)serialPort1.ReadByte();
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
                    warning_message("SSMCOM timeout. " + ex.GetType().ToString() + " " + ex.Message);
					Thread.Sleep (500);
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
                        if (_content_table[code].Slow_Read_Enable)
                        {
                            address_bytes = address_bytes.Concat(_content_table[code].Read_Address).ToArray();
                            query_code_list.Add(code);
                        }
                    }
                    else
                    {
                        if (_content_table[code].Fast_Read_Enable)
                        {
                            address_bytes = address_bytes.Concat(_content_table[code].Read_Address).ToArray();
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
                    int read_byte_length = _content_table[code].Address_Length / 3;

                    int i;
                    int temp_buf = inbuf[get_offset];
                    for (i = 1; i < read_byte_length; i++)
                    {
                        temp_buf = (temp_buf << 8) + inbuf[get_offset + i];
                    }

                    _content_table[code].Raw_Value = temp_buf;

                    get_offset += read_byte_length;
                }
            }

            static private void error_message(string message)
            {
                string send_message = "SSMCOM Error : " + message;
                logger.Error(message);
            }

            static private void info_message(string message)
            {
                string send_message = "SSMCOM Info : " + message;
                logger.Info(message);
            }

            static private void warning_message(string message)
            {
                string send_message = "SSMCOM Warning : " + message;
                logger.Warn(message);
            }

        }

    	public class SSMCOMDataReceivedEventArgs : EventArgs
    	{
	    	public bool Slow_read_flag { get; set; }
	    	public List<SSM_Parameter_Code> Received_Parameter_Code { get; set; }
	    }

    }
}