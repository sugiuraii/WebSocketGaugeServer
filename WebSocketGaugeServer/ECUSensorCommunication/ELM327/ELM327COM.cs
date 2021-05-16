using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public class ELM327COM : ELM327COMBase
    {
        public const byte MODECODE = 0x01;
        //Wait time after calling ATZ command (in milliseconds)
        private const int WAIT_AFTER_ATZ = 4000;

        //Recommended baudrate for USB ELM327 adaptor
        private const int RECOMMENDED_BAUD_RATE = 115200;

        private const int INITIALIZE_FAILED_MAX = 30;
        private const int PID_COMMUNICATE_RETRY_MAX = 5;

        private readonly string ELM327SetProtocolMode;
        private readonly int ELM327AdaptiveTimingMode;
        private readonly int ELM327Timeout;

        private readonly ILogger logger;

        //Constructor
        public ELM327COM(ILoggerFactory logger, string comPortName, string elm327ProtocolStr, int elm327AdaptiveTimingMode, int elm327Timeout) : base(logger)
        {
            this.logger = logger.CreateLogger<ELM327COM>();
            //Setup serial port

            PortName = comPortName;
            DefaultBaudRate = 115200;

            ResetBaudRate = 4800;
            ReadTimeout = 500;

            ELM327SetProtocolMode = elm327ProtocolStr;
            ELM327AdaptiveTimingMode = elm327AdaptiveTimingMode;
            ELM327Timeout = elm327Timeout;
        }

        public ELM327COM(ILoggerFactory logger, string comPortName) : this(logger, comPortName, String.Empty, 1, 32)
        {
        }

        //Changing DefaultBaudRate is allowed in ELM327COM
        public void overrideDefaultBaudRate(int baudRate)
        {
            DefaultBaudRate = baudRate;
        }

        protected override void communicate_initialize()
        {
            base.communicate_initialize();
            if (DefaultBaudRate != RECOMMENDED_BAUD_RATE)
                logger.LogWarning("Baurdate is different from recommended ELM327-USB bardrate of " + RECOMMENDED_BAUD_RATE.ToString() + "bps");

            initializeELM327ATCommand();
        }

        private void initializeELM327ATCommand()
        {
            // Ignore Timeout on ReadExisting
            try
            {
                ReadExisting();
            }
            catch (TimeoutException ex)
            {
                logger.LogDebug("TimeoutException in initializeELM327ATCommand() (Ignored) . Message : " + ex.Message);
            }
            DiscardInBuffer();
            bool initializeFinished = false;
            int initializeFailedCount = 0;

            do
            {
                try
                {
                    // Input initial AT commands
                    Write("ATZ\r");
                    Thread.Sleep(WAIT_AFTER_ATZ);
                    logger.LogDebug("Call ATZ to initialize. Return Msg is " + ReadTo(">"));

                    // Disable space.
                    Write("ATS0\r");
                    logger.LogDebug("Call ATS0 to disable space. Return Msg is " + ReadTo(">"));
                    // Disable echoback.
                    Write("ATE0\r");
                    logger.LogDebug("Call ATE0 to disable echoback. Return Msg is " + ReadTo(">"));
                    // Disable Linefeed on delimiter
                    Write("ATL0\r");
                    logger.LogDebug("Call ATL0 to disable linefeed. Return Msg is " + ReadTo(">"));

                    ELM327SetProtocol();
                    ELM327TimingControlSet();

                    initializeFinished = true;
                }
                catch (TimeoutException ex)
                {
                    logger.LogError("Timeout is occured during ELM327 initialization AT command settings. Wait 2sec and retry.. : " + ex.Message);
                    initializeFailedCount++;
                    if (initializeFailedCount > INITIALIZE_FAILED_MAX)
                    {
                        throw new InvalidOperationException("ELM327 initialization AT command setting is failed over " + INITIALIZE_FAILED_MAX + "counts.");
                    }
                    initializeFinished = false;
                    Thread.Sleep(2000);
                }
                finally
                {
                    DiscardInBuffer();
                }
            } while (!initializeFinished);
        }

        private void ELM327SetProtocol()
        {
            if (string.IsNullOrEmpty(ELM327SetProtocolMode))
            {
                logger.LogDebug("ELM327SetProtocolMode string is blank. ELM327 protocol set (AT SP) will be skipped.");
                return;
            }
            if (ELM327SetProtocolMode.Length != 1)
                logger.LogWarning("ELM327SetProtocolMode is not a signle character. AT SP command may fail.");
            if (!Regex.IsMatch(ELM327SetProtocolMode, "[0-9]|[A-C]"))
                logger.LogWarning("ELM327SetProtocolMode is not 0-9 or A-C. AT SP command may fail.");

            string setprotocolStr = "AT SP " + ELM327SetProtocolMode;
            Write(setprotocolStr + "\r");
            logger.LogDebug("Call " + setprotocolStr + " to set ELM327 protocol. Return Msg is " + ReadTo(">"));
        }

        private void ELM327TimingControlSet()
        {
            // Adaptive timing control set
            if (this.ELM327AdaptiveTimingMode < 0 || this.ELM327AdaptiveTimingMode > 2)
                logger.LogWarning("ELM327 Adaptive timing mode is not 0-2. AT AT command may fail.");

            Write("ATAT" + ELM327AdaptiveTimingMode.ToString() + "\r");
            logger.LogDebug("Call AT AT" + ELM327AdaptiveTimingMode.ToString() + " to set adaptive timing control mode. Return Msg is " + ReadTo(">"));

            // Timeout set
            int timeoutToSet = this.ELM327Timeout;
            if (timeoutToSet < 0)
            {
                logger.LogWarning("ELM327 Timeout is not positive. Set 0 instead.");
                timeoutToSet = 0;
            }
            if (timeoutToSet > 255)
            {
                logger.LogWarning("ELM327 Timeout needs to be less than 256. Set 255 instead.");
                timeoutToSet = 255;
            }

            Write("ATST" + timeoutToSet.ToString("X2") + "\r");
            logger.LogDebug("Call AT ST" + timeoutToSet.ToString("X2") + " to set timeout. Return Msg is " + ReadTo(">"));
        }

        protected override void communicate_main(bool slow_read_flag)
        {
            try
            {
                //Create PID list to query
                List<OBDIIParameterCode> query_OBDII_code_list = new List<OBDIIParameterCode>();
                foreach (OBDIIParameterCode code in Enum.GetValues(typeof(OBDIIParameterCode)))
                {
                    if (slow_read_flag)
                    {
                        if (content_table[code].SlowReadEnable)
                        {
                            query_OBDII_code_list.Add(code);
                        }
                    }
                    else
                    {
                        if (content_table[code].FastReadEnable)
                        {
                            query_OBDII_code_list.Add(code);
                        }
                    }
                }

                //Exit loop if the PIDs to query are not exists.
                if (query_OBDII_code_list.Count <= 0)
                {
                    //If no PIDs are in query list, return with waiting 500ms(wait is ignored if slow_read_flag = true).
                    if (!slow_read_flag)
                        Thread.Sleep(500);
                    return;
                }

                foreach (OBDIIParameterCode code in query_OBDII_code_list)
                {
                    communicateOnePID(code);
                }

                //Invoke SSMDatareceived event
                ELM327DataReceivedEventArgs elm327_received_eventargs = new ELM327DataReceivedEventArgs();
                elm327_received_eventargs.Slow_read_flag = slow_read_flag;
                elm327_received_eventargs.Received_Parameter_Code = new List<OBDIIParameterCode>(query_OBDII_code_list);
                OnELM327DataReceived(this, elm327_received_eventargs);
            }
            catch (TimeoutException ex)
            {
                logger.LogWarning("SSMCOM timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
        }

        private void communicateOnePID(OBDIIParameterCode code)
        {
            this.communicateOnePID(code, 0);
        }

        //Communication on 1PID
        private void communicateOnePID(OBDIIParameterCode code, int errorRetryCount)
        {
            //Clean up serial port buffer
            DiscardInBuffer();

            String outMsg;
            String outPID = content_table[code].PID.ToString("X2");
            int returnByteLength = content_table[code].ReturnByteLength;
            outMsg = MODECODE.ToString("X2") + outPID;
            // Wait receiving only single message (for speed up)
            outMsg.Concat("1");
            Write(outMsg + "\r");
            //logger.LogDebug("ELM327OUT:" + outMsg);
            String inMsg = "";

            try
            {
                //inMsg = ReadTo("\r");

                // Read to next prompt char of '>'
                inMsg = ReadTo(">");
                // Discard after the char of \r
                // (discard all after \r)
                // (This routine is implemented to make countermeasure in the case of multiple message returned.
                inMsg = discardStringAfterChar(inMsg, '\r');

                //logger.LogDebug("ELM327IN:" + inMsg);

                // Get ECU data.
                inMsg = inMsg.Replace(">", "").Replace("\n", "").Replace("\r", "");

                // Check ECU data format.
                if (inMsg.Equals(""))
                    throw new FormatException("Return message at communicateOnePID() is empty.");
                else if (inMsg.Contains("NO DATA"))
                {
                    logger.LogWarning("ELM327 returns NO DATA." + " OutPID :" + outPID + " Code : " + code.ToString());
                    return;
                }

                String inPID = inMsg.Substring(2, 2);
                if (!inPID.Equals(outPID))
                {
                    throw new FormatException("PID return from ELM327 does not match with commanded PID." + "outPID : " + outPID + " inPID :" + inPID);
                }

                //logger.LogDebug("Filtered ELM327IN:" + inMsg);
                var returnValue = Convert.ToUInt32(inMsg.Remove(0, 4), 16);

                content_table[code].RawValue = returnValue;
            }
            catch (TimeoutException ex)
            {
                logger.LogError("ELM327COM timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
            catch (FormatException ex)
            {
                logger.LogWarning(ex.GetType().ToString() + " " + ex.Message + " Received string Is : " + inMsg);
                if (errorRetryCount < PID_COMMUNICATE_RETRY_MAX)
                {
                    logger.LogWarning("Retry communication" + " OutPID :" + outPID + " Code : " + code.ToString());
                    communicateOnePID(code, errorRetryCount + 1);
                }
                else
                {
                    logger.LogError("PID communication retry count exceeds maximum (" + PID_COMMUNICATE_RETRY_MAX.ToString() + ")");
                    communicateRealtimeIsError = true;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                logger.LogWarning(ex.GetType().ToString() + " " + ex.Message + " Received string Is : " + inMsg);
                if (errorRetryCount < PID_COMMUNICATE_RETRY_MAX)
                {
                    logger.LogWarning("Retry communication" + " OutPID :" + outPID + " Code : " + code.ToString());
                    communicateOnePID(code, errorRetryCount + 1);
                }
                else
                {
                    logger.LogError("PID communication retry count exceeds maximum (" + PID_COMMUNICATE_RETRY_MAX.ToString() + ")");
                    communicateRealtimeIsError = true;
                }
            }
        }

        private string discardStringAfterChar(string instr, char delimiter)
        {
            int index = instr.IndexOf(delimiter);
            string instrTemp;

            //Remove 1st char if the 1st char is delimiter
            if (index == 0)
                instrTemp = instr.Remove(0, 1);
            else
                instrTemp = instr;

            index = instrTemp.IndexOf(delimiter);

            if (index < 0)
                return instrTemp;
            else
                return instrTemp.Substring(0, index);
        }

    }
}
