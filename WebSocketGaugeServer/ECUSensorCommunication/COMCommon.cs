using System;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.SerialPortWrapper;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication
{
    public abstract class COMCommon : IBackgroundCommunicate
    {
        private ISerialPortWrapper serialPort;

        private int slowReadInterval;
        private Task communicateRealtimeTask;
        private CancellationTokenSource ctokenSource = new CancellationTokenSource();
        protected bool communicateRealtimeIsError; // エラー発生時にtrue trueになったらcommunicate_reset()を呼び出して初期化を試みる。

        private int communicateResetCount; //何回communicate_reset()が連続でコールされたか？ (COMMUNICATE_RESET_MAXを超えたらプログラムを落とす)
        private const int COMMUNICATE_RESET_MAX = 20; //communicate_reset()コールを連続で許可する回数。

        private readonly ILogger logger;

        public COMCommon(string portname, Parity parity, ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger<COMCommon>();
            DefaultBaudRate = 19200;
            ResetBaudRate = 9600;
            SlowReadInterval = 10;

            var serialWrapperFactory = new SerialPortWrapperFactory(logger);
            this.serialPort = serialWrapperFactory.Create(portname, DefaultBaudRate, parity);

            communicateRealtimeIsError = false;
            communicateResetCount = 0;
        }

        public void BackgroundCommunicateStart()
        {
            // Set serialport1.BaudRate to default baud rate
            serialPort.BaudRate = DefaultBaudRate;
            logger.LogInformation("Set baudrate to " + serialPort.BaudRate.ToString() + " bps.");
            communicateRealtimeIsError = false;
            communicateRealtimeTask = Task.Run(async() => await communicate_realtime(ctokenSource.Token));
            logger.LogInformation("Communication Started.");
        }

        public void BackgroundCommunicateStop()
        {
            //通信スレッドを終了させる(フラグをfalseに)
            ctokenSource.Cancel();
            communicateRealtimeTask.Wait();
            //通信スレッド終了まで待つ
            logger.LogInformation("Communication Stopped.");
        }

        //読み込みスレッド実装（communicate_realtime_start()からスレッドを作って呼び出すこと）
        private async Task communicate_realtime(CancellationToken ct)
        {
            try
            {
                //ポートオープン
                logger.LogInformation("COMport open.");
                logger.LogInformation("Call initialization routine");
                serialPort.Open();
                communicate_initialize();

                int i = 0;
                while (!ct.IsCancellationRequested)
                {
                    if (i > SlowReadInterval)
                    {
                        //slowread_intervalごとにSlowreadモードで通信。
                        //slowreadモードを実装しないケースもあり(引数によらず同じ処理をする実装もあり)

                        await Task.Run(() => communicate_main(true));
                        i = 0;
                    }
                    else
                    {
                        await Task.Run(() => communicate_main(false));
                        i++;
                    }

                    if (communicateRealtimeIsError) // シリアルポートエラー（タイムアウト、パリティ、フレーミング)を受信したら、初期化を試みる。
                    {
                        await Task.Run(() => communticate_reset());
                        communicateResetCount++;

                        if (communicateResetCount > COMMUNICATE_RESET_MAX)
                        {
                            throw new InvalidOperationException("Number of communicate_reset() call exceeds COMMUNICATE_RESET_MAX : " + COMMUNICATE_RESET_MAX.ToString() + ". Terminate communicate_realtime().");
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
            catch (IOException ex)
            {
                logger.LogError(ex.GetType().ToString() + " " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex.GetType().ToString() + " " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError(ex.GetType().ToString() + " " + ex.Message);
            }
            finally
            {
                //ポートクローズ
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    logger.LogInformation("COMPort is closed.");
                }
            }
        }

        //  通信フレーム当たりの処理
        //　継承先のクラスにて実装すること
        protected abstract void communicate_main(bool slowread_flag);

        //　通信初期の初期化処理
        //　初期化処理がある場合は継承先のクラスにてoverrideすること
        protected virtual void communicate_initialize(){}

        //　通信リセット
        private void communticate_reset()
        {
            logger.LogInformation("COM communication reset.");
            serialPort.Close();

            //フレームをずらすために、一旦別ボーレートで通信させる
            SetBaduRateToResetBaudRate();

            //1000ms ダミー通信させた後、バッファ破棄
            serialPort.Open();
            Thread.Sleep(1000);
            serialPort.DiscardInBuffer();
            serialPort.Close();

            //ボーレート戻し、ポート復帰させる
            SetBaudRateToDefaultBaudRate();
            logger.LogInformation("COMport open.");
            serialPort.Open();
            logger.LogInformation("Call initialization routine");
            communicate_initialize();
        }

        public async Task<byte[]> ReadMultiBytesAsync(int count, CancellationToken ct)
        {
            var buf = new byte[count];
            int currPos = 0;
            await Task.Run(() =>
                { 
                    while(currPos < count || ct.IsCancellationRequested)
                    {
                        int numToRead = count - currPos;
                        if(serialPort.BytesToRead < numToRead)
                            continue;
                        int numReadBytes = serialPort.Read(buf, currPos, numToRead);
                        currPos += numReadBytes;
                    }
                    ct.ThrowIfCancellationRequested();                
                });
            return buf;
        }

        public byte[] ReadMultiBytes(int count)
        {
            var buf = new byte[count];
            int currPos = 0;
            while(currPos < count)
            {
                int numToRead = count - currPos;
                if(serialPort.BytesToRead < numToRead)
                    continue;
                int numReadBytes = serialPort.Read(buf, currPos, numToRead);
                currPos += numReadBytes;
            }
            return buf;
        }

        public int ReadByte()
        {
            return serialPort.ReadByte();
        }

        public string ReadTo(string str)
        {
            return serialPort.ReadTo(str);
        }

        public string ReadLine()
        {
            return serialPort.ReadLine();
        }

        public string ReadExisting()
        {
            return serialPort.ReadExisting();
        }

        public void DiscardInBuffer()
        {
            serialPort.DiscardInBuffer();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            serialPort.Write(buffer, offset, count);
        }

        public void Write(string buffer)
        {
            serialPort.Write(buffer);
        }

        public void SetBaudRateToTemporaryBaudRate(int baudrate)
        {
            serialPort.BaudRate = baudrate;
            logger.LogInformation("Set baud rate to temporary badurate of : " + baudrate.ToString());
        }

        public void SetBaduRateToResetBaudRate()
        {
            serialPort.BaudRate = ResetBaudRate;
            logger.LogInformation("Set baud rate to reset badurate of : " + ResetBaudRate.ToString());
        }

        public void SetBaudRateToDefaultBaudRate()
        {
            serialPort.BaudRate = DefaultBaudRate;
            logger.LogInformation("Recover to default baudrate of : " + DefaultBaudRate.ToString());
        }

        public bool IsCommunitateThreadAlive
        {
            get
            {
                if(communicateRealtimeTask == null)
                    throw new InvalidOperationException("Communication thread is null. Maybe not created. Run BackgroundCommunicateStart() before query IsCommunicationThradAlive.");
                else
                    return communicateRealtimeTask.Status == TaskStatus.Running;
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
                logger.LogDebug("Set slowread interval to " + value.ToString());
            }
        }

        protected int DefaultBaudRate {get; set; }
        protected int ResetBaudRate { get; set; }

        public int ReadTimeout
        {
            get
            {
                return serialPort.ReadTimeout;
            }
            set
            {
                serialPort.ReadTimeout = value;
            }
        }
    }
}
