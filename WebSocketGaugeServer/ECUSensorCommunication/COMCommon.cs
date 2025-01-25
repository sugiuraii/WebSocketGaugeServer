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
        protected bool communicateRealtimeIsError; // True on error. (Try communicate_reset() to re-initialize)

        private int communicateResetCount; //Count of communicate_reset() call (Exit program on exceeding COMMUNICATE_RESET_MAX)
        private const int COMMUNICATE_RESET_MAX = 20;

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
            communicateRealtimeTask = Task.Run(() => communicate_realtime(ctokenSource.Token));
            logger.LogInformation("Communication Started.");
        }

        public void BackgroundCommunicateStop()
        {
            // Ends communication thread(By set the flag false)
            ctokenSource.Cancel();
            communicateRealtimeTask.Wait();
            // Wait until the communication thread ends.
            logger.LogInformation("Communication Stopped.");
        }

        // Thrad of reading（Call this by creating the thread on communicate_realtime_start()）
        private void communicate_realtime(CancellationToken ct)
        {
            while(!ct.IsCancellationRequested)
            {
                try
                {
                    // Port open
                    logger.LogInformation("COMport open.");
                    logger.LogInformation("Call initialization routine");
                    serialPort.Open();
                    communicate_initialize();

                    int i = 0;
                    while (!ct.IsCancellationRequested)
                    {
                        if (i > SlowReadInterval)
                        {
                            //Communicatie by Slow read mode, by the interval of slowread_interval

                            communicate_main(true);
                            i = 0;
                        }
                        else
                        {
                            communicate_main(false);
                            i++;
                        }

                        if (communicateRealtimeIsError) // Try communication reset  on error(Timeout, parity, framing)
                        {
                            communticate_reset();
                            communicateResetCount++;

                            if (communicateResetCount > COMMUNICATE_RESET_MAX)
                            {
                                throw new InvalidOperationException("Number of communicate_reset() call exceeds COMMUNICATE_RESET_MAX : " + COMMUNICATE_RESET_MAX.ToString() + ". Terminate communicate_realtime().");
                            }

                            communicateRealtimeIsError = false;
                        }
                        else
                        {
                            //Reset counter when the error is not detected.
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
                catch (Exception ex)
                {
                    logger.LogError(ex.GetType().ToString() + " " + ex.Message);
                    logger.LogError(ex.StackTrace);
                }
                finally
                {
                    // Close port
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                        logger.LogInformation("COMPort is closed.");
                    }
                }
                if(!ct.IsCancellationRequested)
                {   
                    logger.LogWarning("Communcation has been abnormally stoped due to error. Retry connect after 5sec.");
                    Thread.Sleep(5000);
                }
            }
        }

        //  Process per communication frame (need to be implemented by sub classes)
        protected abstract void communicate_main(bool slowread_flag);

        //　Initialization (need to be implemented by sub classes)
        protected virtual void communicate_initialize(){}

        //　Reset
        private void communticate_reset()
        {
            logger.LogInformation("COM communication reset.");
            serialPort.Close();
            // Workaround to refresh communication
            // Try dummy communication with changing baudrate (from default), to re-aligh frame
            SetBaduRateToResetBaudRate();

            // Dummy communication 1000ms -> Discard buffer
            serialPort.Open();
            Thread.Sleep(1000);
            serialPort.DiscardInBuffer();
            serialPort.Close();

            // Revert port (by chainging baudrate to default.)
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
