using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using log4net;

namespace DefiSSMCOM
{
    namespace Defi
    {
        public class DefiCOM
        {
            private SerialPort serialPort1;
            private Defi_Content_Table _content_table;

			private string _portname;

            private Thread communicate_realtime_thread1;
            private bool _communicate_realtime_start; // 読み込みスレッド継続フラグ(communicate_realtime_stop()でfalseになり、読み込みスレッドは終了)
            private bool _communicate_realtime_error; // エラー発生時にtrue trueになったらcommunicate_reset()を呼び出して初期化を試みる。

			// Defilink received Event
			public event EventHandler DefiLinkPacketReceived;
            //Log4net logger
            private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            //DefiLinkパケットサイズ
            const int DEFI_PACKET_SIZE = 35;
            //DefiLinkボーレート設定
            const int DEFI_BAUD_RATE = 19200;
            //リセット時のボーレート設定(communticate_reset()参照)
            //FT232RLの場合、許容されるボーレートは3000000/n (nは整数または小数点以下が0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875)
            const int DEFI_RESET_BAUD_RATE = 9600;


            //コンストラクタ
            public DefiCOM()
            {
                serialPort1 = new System.IO.Ports.SerialPort();
				PortName = "COM1";

                _content_table = new Defi_Content_Table();

                //シリアルポート設定
                serialPort1.BaudRate = DEFI_BAUD_RATE;
                serialPort1.Parity = Parity.Even;
                serialPort1.ReadTimeout = 500;
                _communicate_realtime_start = false;
                _communicate_realtime_error = false;

                //通信エラー発生時のイベント処理登録
                serialPort1.ErrorReceived += new SerialErrorReceivedEventHandler(SerialPortErrorReceived);


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
						error_message("Port name set error : " + ex1.GetType().ToString() + " " + ex1.Message);
                    }
                }
            }

            public void communicate_realtime_start()
            {
                communicate_realtime_thread1 = new Thread(new ThreadStart(communicate_realtime));
                _communicate_realtime_start = true;
                _communicate_realtime_error = false;
                communicate_realtime_thread1.Start();
                info_message("DefiCom communication Started.");
            }

            public void communicate_realtime_stop()
            {
                //通信スレッドを終了させる(フラグをfalseに)
                _communicate_realtime_start = false;

                //通信スレッド終了まで待つ
                communicate_realtime_thread1.Join();
                info_message("DefiCom communication Stopped.");
            }

            //読み込みスレッド実装（communicate_realtime_start()からスレッドを作って呼び出すこと）
            private void communicate_realtime()
            {
                //ポートオープン
                try
                {
                    serialPort1.Open();
                    //スレッドフラグがfalseにされるまで続ける
                    while (_communicate_realtime_start)
                    {
                        communicate_main();
                        if (_communicate_realtime_error) // シリアルポートエラー（タイムアウト、パリティ、フレーミング)を受信したら、初期化を試みる。
                        {
                            communticate_reset();
                            _communicate_realtime_error = false;
                        }

                    }
                }
                catch (System.IO.IOException ex)
                {
					error_message(ex.GetType().ToString() + " " +  ex.Message);
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
                    _communicate_realtime_start = false;

                    if(serialPort1.IsOpen)
                        serialPort1.Close();
                }
            }

            //　通信リセット
            private void communticate_reset()
            {
                info_message("Deficom communication reset.");
                serialPort1.Close();

                //フレームをずらすために、一旦別ボーレートで通信させる
                serialPort1.BaudRate = DEFI_RESET_BAUD_RATE;
                
                //1000ms ダミー通信させた後、バッファ破棄
                serialPort1.Open();
                Thread.Sleep(1000);
                serialPort1.DiscardInBuffer();
                serialPort1.Close();

                //ボーレート戻し、ポート復帰させる
                serialPort1.BaudRate = DEFI_BAUD_RATE;
                serialPort1.Open();
            }

            //通信部ルーチン
            private void communicate_main()
            {
                int i, c;
                char[] inbuf = new char[DEFI_PACKET_SIZE];
                //読み込みルーチン

                try
                {
                    //デリミタ(ReceiverID)を見つけるまで読み進める
                    do
                    {
                        c = serialPort1.ReadByte();
                    }
                    while (c < 0x01 || c > 0x0f);

                    //Defiパケットサイズ分だけ読み出す。
                    inbuf[0] = (char)c;

                    for (i = 1; i < DEFI_PACKET_SIZE; i++)
                    {
                        c = serialPort1.ReadByte();
                        inbuf[i] = (char)c;
                    }
                }
                catch (TimeoutException ex)
                {
                    //読み出しタイムアウト時はエラーフラグを立て、次のサイクルでリセット処理を入れる
                    warning_message("Defi packet timeout. " + ex.GetType().ToString() + " " + ex.Message);
                    _communicate_realtime_error = true;
                    return;
                }

                //バッファの残り分は破棄
                serialPort1.DiscardInBuffer();
                    
                //ReceiverIDを判読し、private変数に格納
                int j;
                for (j = 0; j < DEFI_PACKET_SIZE; j += 5)
                {
                    try
                    {
                        if (inbuf[j] == (char)_content_table[Defi_Parameter_Code.Boost].Receiver_id)
                        {
                            String boost_str = new String(inbuf, j + 2, 3);
                            _content_table[Defi_Parameter_Code.Boost].Raw_Value = Int32.Parse(boost_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                        }
                        else if (inbuf[j] == (char)_content_table[Defi_Parameter_Code.Tacho].Receiver_id)
                        {
                            String tacho_str = new String(inbuf, j + 2, 3);
                            _content_table[Defi_Parameter_Code.Tacho].Raw_Value = Int32.Parse(tacho_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                        }
                        else if (inbuf[j] == (char)_content_table[Defi_Parameter_Code.Oil_Pres].Receiver_id)
                        {
                            String oilpres_str = new String(inbuf, j + 2, 3);
                            _content_table[Defi_Parameter_Code.Oil_Pres].Raw_Value = Int32.Parse(oilpres_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                        }
                        else if (inbuf[j] == (char)_content_table[Defi_Parameter_Code.Fuel_Pres].Receiver_id)
                        {
                            String fuelpres_str = new String(inbuf, j + 2, 3);
                            _content_table[Defi_Parameter_Code.Fuel_Pres].Raw_Value = Int32.Parse(fuelpres_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                        }
                        else if (inbuf[j] == (char)_content_table[Defi_Parameter_Code.Ext_Temp].Receiver_id)
                        {
                            String exttemp_str = new String(inbuf, j + 2, 3);
                            _content_table[Defi_Parameter_Code.Ext_Temp].Raw_Value = Int32.Parse(exttemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                        }
                        else if (inbuf[j] == (char)_content_table[Defi_Parameter_Code.Oil_Temp].Receiver_id)
                        {
                            String oiltemp_str = new String(inbuf, j + 2, 3);
                            _content_table[Defi_Parameter_Code.Oil_Temp].Raw_Value = Int32.Parse(oiltemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                        }
                        else if (inbuf[j] == (char)_content_table[Defi_Parameter_Code.Water_Temp].Receiver_id)
                        {
                            String watertemp_str = new String(inbuf, j + 2, 3);
                            _content_table[Defi_Parameter_Code.Water_Temp].Raw_Value = Int32.Parse(watertemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                        }
                    }
                    catch (FormatException ex)
                    {
                        //DefiPacketが崩れていた場合エラーフラグを立て、次のサイクルでリセット処理を入れる。
                        warning_message("Invalid Defi packet. " + ex.GetType().ToString() + " " + ex.Message);
                        _communicate_realtime_error = true;
                        return;
                    }
                }

				// Invoke PacketReceived Event
				DefiLinkPacketReceived(this, EventArgs.Empty);                    
            }

            //エラー発生時のイベント処理
            private void SerialPortErrorReceived(object sender, SerialErrorReceivedEventArgs e)
            {
				SerialPort port = (SerialPort)sender;
                _communicate_realtime_error = true;
                error_message("SerialPortError Event is invoked.");
            }

            public double get_value(Defi_Parameter_Code code)
            {
                return _content_table[code].Value;
            }

            public Int32 get_raw_value(Defi_Parameter_Code code)
            {
                return _content_table[code].Raw_Value;
            }

            public string get_unit(Defi_Parameter_Code code)
            {
                return _content_table[code].Unit;
            }

            private void error_message(string message)
            {
                string send_message = "DefiCOM Error : " + message;
                logger.Error(message);
            }

            private void info_message(string message)
            {
                string send_message = "DefiCOM Info : " + message;
                logger.Info(message);
            }

            private void warning_message(string message)
            {
                string send_message = "DefiCOM Warning : " + message;
                logger.Warn(message);
            }
            static private void debug_message(string message)
            {
                string send_message = "SSMCOM Debug : " + message;
                logger.Debug(message);
            }
        }
    }
}