using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.IO.Ports;

using SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Utils;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public class ELM327COM : COMCommon, IELM327COM
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
        private readonly string ELM327HeaderBytes;

        private readonly int ELM327BatchQueryCount;
        private readonly bool SeparateBatchQueryToAvoidMultiFrameResponse;
        private readonly OBDIIContentTable content_table;

        private readonly ELM327OutMessageParser elm327MsgParser;
        public event EventHandler<ELM327DataReceivedEventArgs> ELM327DataReceived;
        private readonly ILogger logger;

        //Constructor
        public ELM327COM(ILoggerFactory logger, string comPortName, string elm327ProtocolStr, int elm327AdaptiveTimingMode, int elm327Timeout, string elm327HeaderBytes, int elm327BatchQueryCount, bool separateBatchQueryToAvoidMultiFrameResponse) : base(comPortName, Parity.None, logger)
        {
            this.logger = logger.CreateLogger<ELM327COM>();
            this.content_table = new OBDIIContentTable();

            //Setup serial port
            DefaultBaudRate = 115200;

            ResetBaudRate = 4800;
            ReadTimeout = 10000;

            ELM327SetProtocolMode = elm327ProtocolStr;
            ELM327AdaptiveTimingMode = elm327AdaptiveTimingMode;
            ELM327Timeout = elm327Timeout;
            ELM327HeaderBytes = elm327HeaderBytes;
            if(elm327BatchQueryCount > 6 || elm327BatchQueryCount < 1)
                throw new ArgumentException("ELM327 batch query count needs to be 1 to 6.");
            this.ELM327BatchQueryCount = elm327BatchQueryCount;
            this.SeparateBatchQueryToAvoidMultiFrameResponse = separateBatchQueryToAvoidMultiFrameResponse;
            this.elm327MsgParser = new ELM327OutMessageParser(this.content_table);
        }

        public ELM327COM(ILoggerFactory logger, string comPortName) : this(logger, comPortName, String.Empty, 1, 32, "", 1, true)
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
            logger.LogInformation("ELM327 initialization is finished.");
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
                    logger.LogDebug("Call ATZ to initialize.");
                    logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));
                    // Disable echoback.
                    Write("ATE0\r");
                    logger.LogDebug("Call ATE0 to disable echoback.");
                    logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));
                    // Disable Linefeed on delimiter
                    Write("ATL0\r");
                    logger.LogDebug("Call ATL0 to disable linefeed.");
                    logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));

                    // Set protocol
                    ELM327SetProtocol();
                    // Test communication
                    ELM327TestCommunicationToSearchProtocol();

                    // Disable space.
                    Write("ATS0\r");
                    logger.LogDebug("Call ATS0 to disable space.");
                    logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));
                    // Setup ELM327 timing and timeout
                    ELM327TimingControlSet();
                    // Setup ELM327 header setting
                    ELM327SetHeader();

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
            logger.LogDebug("Call " + setprotocolStr + " to set ELM327 protocol.");
            logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));
        }

        private void ELM327TestCommunicationToSearchProtocol() 
        {
            // Enable header ouut
            Write("ATH1\r");
            logger.LogDebug("Call ATH1 to enable header out.");
            logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));

            Write("0100\r");
            logger.LogDebug("Call 0100 to test communication.");
            logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));

            // Disable header ouut
            Write("ATH0\r");
            logger.LogDebug("Call ATH0 to disable header out.");
            logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));
        } 

        private void ELM327TimingControlSet()
        {
            // Adaptive timing control set
            if (this.ELM327AdaptiveTimingMode < 0 || this.ELM327AdaptiveTimingMode > 2)
                logger.LogWarning("ELM327 Adaptive timing mode is not 0-2. AT AT command may fail.");

            Write("ATAT" + ELM327AdaptiveTimingMode.ToString() + "\r");
            logger.LogDebug("Call AT AT" + ELM327AdaptiveTimingMode.ToString() + " to set adaptive timing control mode.");
            logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));

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
            logger.LogDebug("Call AT ST" + timeoutToSet.ToString("X2") + " to set timeout.");
            logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));
        }

        private void ELM327SetHeader()
        {
            // Header byte set.
            if (this.ELM327HeaderBytes.Length <= 0)
            {
                logger.LogInformation("ELM327 header byte is not set (or blank). AT SH command will be skipped.");
                return;
            }

            Write("ATSH" + this.ELM327HeaderBytes + "\r");
            logger.LogDebug("Call AT SH" + this.ELM327HeaderBytes + " to set header ID.");
            logger.LogDebug("Return Msg is " + replaceCRLFWithSpace(ReadTo(">")));
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

                var batchedQueryCodeList = groupBatchQueryCode(query_OBDII_code_list, this.ELM327BatchQueryCount, this.SeparateBatchQueryToAvoidMultiFrameResponse);
                batchedQueryCodeList.ForEach(mcode => communicateMultiPID(mcode, 0));

                //Invoke SSMDatareceived event
                ELM327DataReceivedEventArgs elm327_received_eventargs = new ELM327DataReceivedEventArgs();
                elm327_received_eventargs.Slow_read_flag = slow_read_flag;
                elm327_received_eventargs.Received_Parameter_Code = new List<OBDIIParameterCode>(query_OBDII_code_list);
                ELM327DataReceived(this, elm327_received_eventargs);
            }
            catch (TimeoutException ex)
            {
                logger.LogWarning("SSMCOM timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
        }

        private List<List<OBDIIParameterCode>> groupBatchQueryCode(List<OBDIIParameterCode> queryCodeList, int batchPIDCount, bool separateBatchQueryToAvoidMultiFrameResponse)
        {
            var groupedCodeList = new List<List<OBDIIParameterCode>>();
            int codeCount = 0;
            const int responseContentByteSize = 6;
            int returnByteSum = 0;
            for(int i = 0; i < queryCodeList.Count; i++)
            {
                var code = queryCodeList[i];
                var valueByteLength = content_table[code].ReturnByteLength;
                if( i == 0 )
                    groupedCodeList.Add(new List<OBDIIParameterCode>());
                
                else if((codeCount + 1 > batchPIDCount) ||
                    ( separateBatchQueryToAvoidMultiFrameResponse && (returnByteSum + valueByteLength + 1 >  responseContentByteSize )))
                {
                    groupedCodeList.Add(new List<OBDIIParameterCode>());
                    codeCount = 0;
                    returnByteSum = 0;
                }
                groupedCodeList.Last().Add(code);
                codeCount++;
                returnByteSum += valueByteLength + 1;

            }

            return groupedCodeList;
        }

        private String queryMsg(String outMsg)
        {
            DiscardInBuffer();
            // logger.LogDebug("ELM327OUT:" + outMsg);

            // Issue query
            Write(outMsg + "\r");
            // Read to next prompt char of '>'
            String inMsg = ReadTo(">");
            return inMsg;
        }

        private String queryPIDs(List<byte> pids, int returnByteLength)
        {
            String outMsg = MODECODE.ToString("X2") + pids.Select(pid => pid.ToString("X2")).Aggregate((prev, next) => prev + next);
            // Calculate number of ISO-TP return frame 
            int returnMessageBlocks;
            if(returnByteLength <= 7)
                returnMessageBlocks = 1;
            else if (returnByteLength <= 13)
                returnMessageBlocks = 2;
            else
                returnMessageBlocks = 3 + (returnByteLength - 14)/7;
            // logger.LogDebug("Return message blocks: " + returnMessageBlocks.ToString());
            
            // Append number of message frames at the end of query string.
            outMsg = outMsg + returnMessageBlocks.ToString();
            String inMsg = queryMsg(outMsg);
            return inMsg;
        }

        private void communicateMultiPID(List<OBDIIParameterCode> codes, int errorRetryCount)
        {
            if(codes.Count > 6)
                throw new ArgumentException("Code list size of multiple PID communication must be less than or equal 6.");
            
            //Clean up serial port buffer
            DiscardInBuffer();
            String outMsg = MODECODE.ToString("X2") + codes.Select(code => content_table[code].PID.ToString("X2")).Aggregate((prev, next) => prev + next);
            int returnByteLength = 1 + codes.Select(code => content_table[code].ReturnByteLength + 1).Sum(); // Return byte lenght = 1(mode code) + sum (1(=PID byte) + Return byte length)
            // Calculate number of ISO-TP return frame 
            int returnMessageBlocks;
            if(returnByteLength <= 7)
                returnMessageBlocks = 1;
            else if (returnByteLength <= 13)
                returnMessageBlocks = 2;
            else
                returnMessageBlocks = 3 + (returnByteLength - 14)/7;
            // logger.LogDebug("Return message blocks: " + returnMessageBlocks.ToString());
            
            // Append number of message frames at the end of query string.
            outMsg = outMsg + returnMessageBlocks.ToString();
            // Issue query
            Write(outMsg + "\r");
            // logger.LogDebug("ELM327OUT:" + outMsg);

            // Read to next prompt char of '>'
            String inMsg = ReadTo(">");
            // logger.LogDebug("ELM327IN:" + inMsg);
            try
            {
                if (inMsg.Equals(""))
                    throw new FormatException("Return message at communicateOnePID() is empty.");
                else if (inMsg.Contains("NO DATA"))
                    throw new FormatException("ELM327 returns NO DATA.");

                var parseResult = elm327MsgParser.parse(inMsg);
                var parsedValueList = parseResult.ValueStrMap.Select(kvp => new KeyValuePair<OBDIIParameterCode, uint>(kvp.Key, Convert.ToUInt32(kvp.Value, 16)))
                                                       .ToList();
                parsedValueList.ForEach(kvp => content_table[kvp.Key].RawValue = kvp.Value);
            }
            catch (TimeoutException ex)
            {
                logger.LogError("ELM327COM timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentOutOfRangeException || ex is KeyNotFoundException | ex is ArgumentException)
            {
                logger.LogWarning(ex.GetType().ToString() + " " + ex.Message + " Query string is:" + outMsg +" Received string is : " + inMsg);
                logger.LogWarning(ex.StackTrace);
                if (errorRetryCount < PID_COMMUNICATE_RETRY_MAX)
                {
                    logger.LogWarning("Retry communication cycle :" + (errorRetryCount + 1).ToString());
                    communicateMultiPID(codes, errorRetryCount + 1);
                }
                else
                {
                    logger.LogError("PID communication retry count exceeds maximum (" + PID_COMMUNICATE_RETRY_MAX.ToString() + ")");
                    communicateRealtimeIsError = true;
                }
            }
        }

        //Communication on 1PID
        private void communicateOnePID(OBDIIParameterCode code, int errorRetryCount)
        {
            communicateMultiPID(new List<OBDIIParameterCode>{code}, errorRetryCount);
        }

        private string replaceCRLFWithSpace(string instr)
        {
            return instr.Replace("\r", " ").Replace("\n", " ");
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

        private List<byte> getAvailavlePIDs()
        {

        }

        public double get_value(OBDIIParameterCode code)
        {
            return content_table[code].Value;
        }

        public UInt32 get_raw_value(OBDIIParameterCode code)
        {
            return content_table[code].RawValue;
        }

        public string get_unit(OBDIIParameterCode code)
        {
            return content_table[code].Unit;
        }

        public bool get_slowread_flag(OBDIIParameterCode code)
        {
            return content_table[code].SlowReadEnable;
        }

        public bool get_fastread_flag(OBDIIParameterCode code)
        {
            return content_table[code].FastReadEnable;
        }

        public void set_slowread_flag(OBDIIParameterCode code, bool flag)
        {
            set_slowread_flag(code, flag, false);
        }
        public void set_slowread_flag(OBDIIParameterCode code, bool flag, bool quiet)
        {
            if (!quiet)
                logger.LogDebug("Slowread flag of " + code.ToString() + "is enabled.");
            content_table[code].SlowReadEnable = flag;
        }

        public void set_fastread_flag(OBDIIParameterCode code, bool flag)
        {
            set_fastread_flag(code, flag, false);
        }
        public void set_fastread_flag(OBDIIParameterCode code, bool flag, bool quiet)
        {
            if (!quiet)
                logger.LogDebug("Fastread flag of " + code.ToString() + "is enabled.");
            content_table[code].FastReadEnable = flag;
        }

        public void set_all_disable()
        {
            set_all_disable(false);
        }

        public void set_all_disable(bool quiet)
        {
            if (!quiet)
                logger.LogDebug("All flag reset.");
            content_table.setAllDisable();
        }

    }

}
