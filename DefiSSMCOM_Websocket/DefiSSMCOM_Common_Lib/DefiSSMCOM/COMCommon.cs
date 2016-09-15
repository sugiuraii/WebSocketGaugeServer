using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using log4net;

namespace DefiSSMCOM
{
    public abstract class COMCommon
    {
        private SerialPort serialPort1;

        private int slowReadInterval;
        private Thread communicate_realtime_thread1;
        private bool communicateRealtimeIsRunning; // 読み込みスレッド継続フラグ(communicate_realtime_stop()でfalseになり、読み込みスレッドは終了)
        protected bool communicateRealtimeIsError; // エラー発生時にtrue trueになったらcommunicate_reset()を呼び出して初期化を試みる。

        private int communicateResetCount; //何回communicate_reset()が連続でコールされたか？ (COMMUNICATE_RESET_MAXを超えたらプログラムを落とす)
        private const int COMMUNICATE_RESET_MAX = 20; //communicate_reset()コールを連続で許可する回数。

        //Log4net logger
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public COMCommon()
        {
            serialPort1 = new SerialPort();
            DefaultBaudRate = 19200;
            ResetBaudRate = 9600;
            SlowReadInterval = 10;

            communicateRealtimeIsRunning = false;
            communicateRealtimeIsError = false;
            communicateResetCount = 0;

            //通信エラー発生時のイベント処理登録
            serialPort1.ErrorReceived += new SerialErrorReceivedEventHandler(SerialPortErrorReceived);

        }

        public void CommunicateRealtimeStart()
        {
            // Set serialport1.BaudRate to default baud rate
            serialPort1.BaudRate = DefaultBaudRate;

            communicate_realtime_thread1 = new Thread(new ThreadStart(communicate_realtime));
            communicateRealtimeIsRunning = true;
            communicateRealtimeIsError = false;
            communicate_realtime_thread1.Start();
            logger.Info("Communication Started.");
        }

        public void CommunicateRealtimeStop()
        {
            //通信スレッドを終了させる(フラグをfalseに)
            communicateRealtimeIsRunning = false;

            //通信スレッド終了まで待つ
            communicate_realtime_thread1.Join();
            logger.Info("Communication Stopped.");
        }

        //読み込みスレッド実装（communicate_realtime_start()からスレッドを作って呼び出すこと）
        private void communicate_realtime()
        {
            try
            {
                //ポートオープン
                serialPort1.Open();
                logger.Info("COMport open.");

                int i = 0;
                while (communicateRealtimeIsRunning)
                {
                    if (i > SlowReadInterval)
                    {
                        //slowread_intervalごとにSlowreadモードで通信。
                        //slowreadモードを実装しないケースもあり(引数によらず同じ処理をする実装もあり)

                        communicate_main(true);
                        i = 0;
                    }
                    else
                    {
                        communicate_main(false);
                        i++;
                    }

                    if (communicateRealtimeIsError) // シリアルポートエラー（タイムアウト、パリティ、フレーミング)を受信したら、初期化を試みる。
                    {
                        communticate_reset();
                        communicateResetCount++;

                        if (communicateResetCount > COMMUNICATE_RESET_MAX)
                        {
                            throw new System.InvalidOperationException("Number of communicate_reset() call exceeds COMMUNICATE_RESET_MAX : " + COMMUNICATE_RESET_MAX.ToString() + ". Terminate communicate_realtime().");
                        }

                        communicateRealtimeIsError = false;
                    }
                    else
                    {
                        //communicate_mainでエラーなければエラーカウンタリセット。
                        communicateResetCount = 0;
                    }

                }
            }
            catch (System.IO.IOException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message);
            }
            catch (System.InvalidOperationException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message);
            }
            catch (System.UnauthorizedAccessException ex)
            {
                logger.Error(ex.GetType().ToString() + " " + ex.Message);
            }
            finally
            {
                communicateRealtimeIsRunning = false;

                //ポートクローズ
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    logger.Info("COMPort is closed.");
                }
            }
        }

        //  通信フレーム当たりの処理
        //　継承先のクラスにて実装すること
        protected abstract void communicate_main(bool slowread_flag);

        //　通信リセット
        private void communticate_reset()
        {
            logger.Info("COM communication reset.");
            serialPort1.Close();

            //フレームをずらすために、一旦別ボーレートで通信させる
            serialPort1.BaudRate = ResetBaudRate;

            //1000ms ダミー通信させた後、バッファ破棄
            serialPort1.Open();
            Thread.Sleep(1000);
            serialPort1.DiscardInBuffer();
            serialPort1.Close();

            //ボーレート戻し、ポート復帰させる
            serialPort1.BaudRate = DefaultBaudRate;
            serialPort1.Open();
        }

        //シリアルポートエラー発生時のイベント処理
        private void SerialPortErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            SerialPort port = (SerialPort)sender;
            communicateRealtimeIsError = true;
            logger.Error("SerialPortError Event is invoked.");
        }

        public int ReadByte()
        {
            return serialPort1.ReadByte();
        }

        public string ReadLine()
        {
            return serialPort1.ReadLine();
        }

        public void DiscardInBuffer()
        {
            serialPort1.DiscardInBuffer();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            serialPort1.Write(buffer, offset, count);
        }

        public bool IsCommunitateThreadAlive
        {
            get
            {
                return communicate_realtime_thread1.IsAlive;
            }
        }

        public int SlowReadInterval
        {
            get
            {
                return slowReadInterval;
            }
            set
            {
                slowReadInterval = value;
                logger.Debug("Set slowread interval to " + value.ToString());
            }
        }

        protected int DefaultBaudRate {get; set; }
        protected int ResetBaudRate { get; set; }

        public string PortName
        {
            get
            {
                return serialPort1.PortName;
            }
            set
            {
                try
                {
                    serialPort1.PortName = value;
                }
                catch (System.InvalidOperationException ex1)
                {
                    logger.Error("Port name set error : " + ex1.GetType().ToString() + " " + ex1.Message);
                }
            }
        }

        public Parity Parity
        {
            get
            {
                return serialPort1.Parity;
            }
            set
            {
                serialPort1.Parity = value;
            }
        }

        public int ReadTimeout
        {
            get
            {
                return serialPort1.ReadTimeout;
            }
            set
            {
                serialPort1.ReadTimeout = value;
            }
        }
        public int DataBits
        {
            get
            {
                return serialPort1.DataBits;
            }
            set
            {
                serialPort1.DataBits = value;
            }
        }

        public StopBits StopBits
        {
            get
            {
                return serialPort1.StopBits;
            }
            set
            {
                serialPort1.StopBits = value;
            }
        }

    }
}
