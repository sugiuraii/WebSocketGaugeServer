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
    public enum ActionOnNODATAReceived
    {
        Ignore,
        AddPIDToBlackList,
        ThrowException
    }

    public class ELM327COM : COMCommon, IELM327COM
    {
        public const byte MODECODE = 0x01;
        //Wait time after calling ATZ command (in milliseconds)
        private const int WAIT_AFTER_ATZ = 4000;

        //Recommended baudrate for USB ELM327 adaptor
        private const int RECOMMENDED_BAUD_RATE = 115200;

        private const int INITIALIZE_FAILED_MAX = 30;
        private const int PID_COMMUNICATE_RETRY_MAX = 5;

        private readonly ELM327COMOption Option;

        private readonly ActionOnNODATAReceived ActionOnNODATAReceived;
        private readonly OBDIIContentTable content_table;

        private readonly ELM327OutMessageParser elm327MsgParser;
        public event EventHandler<ELM327DataReceivedEventArgs> ELM327DataReceived;
        private readonly ILogger logger;

        private ELM327PIDFilter ELM327PIDFilter = null; // Assigned when connected
        private static readonly string[] NewLineSeparators = ["\r\n", "\r", "\n"];

        //Constructor
        public ELM327COM(ELM327COMOption option, ILoggerFactory logger, ActionOnNODATAReceived actionOnNODATAReceived) : base(new COMCommonOption(option.COMPortName, Parity.None), logger)
        {
            this.Option = option;
            this.logger = logger.CreateLogger<ELM327COM>();
            this.content_table = new OBDIIContentTable();
            this.elm327MsgParser = new ELM327OutMessageParser(this.content_table);
            this.ActionOnNODATAReceived = actionOnNODATAReceived;

            //Setup serial port
            DefaultBaudRate = 115200;

            ResetBaudRate = 4800;
            ReadTimeout = 10000;

            if (option.ELM327BatchQueryCount > 6 || option.ELM327BatchQueryCount < 1)
                throw new ArgumentException("ELM327 batch query count needs to be 1 to 6.");
        }

        /*
                public ELM327COM(ILoggerFactory logger, string comPortName) : this(logger, comPortName, 0, String.Empty, 1, 32, "", "", 1, true, true, ActionOnNODATAReceived.Ignore)
                {
                }
        */
        //Changing DefaultBaudRate is allowed in ELM327COM
        public void OverrideDefaultBaudRate(int baudRate)
        {
            DefaultBaudRate = baudRate;
        }

        protected override void communicate_initialize()
        {
            base.communicate_initialize();

            if (DefaultBaudRate != RECOMMENDED_BAUD_RATE)
                logger.LogWarning("Baudrate is different from recommended ELM327-USB baudrate of {RecommendedBaudRate}bps", RECOMMENDED_BAUD_RATE);

            InitializeELM327ATCommand();
            logger.LogInformation("ELM327 initialization is finished.");

            // Get available PIDs
            if (this.Option.QueryOnlyAvailablePID)
            {
                // Query available PID (PID:00, 20, 40, ...)
                logger.LogInformation("Query available PIDs.");
                var availablePIDs = GetAvailablePIDs();
                logger.LogInformation("Available PID count: {Count}", availablePIDs.Count);
                logger.LogInformation("Available PID List: {PidList}", BitConverter.ToString([.. availablePIDs]));

                // Show available code name from available PID list
                var pidToParameterCodeReverseMap = new PIDToOBDIIParameterCodeReverseMapBuilder().create();
                var availableParameterCodes = availablePIDs
                    .Where(cd => pidToParameterCodeReverseMap.ContainsKey(cd))
                    .Select(cd => pidToParameterCodeReverseMap[cd].ToString());

                logger.LogInformation("Available code: {Codes}", String.Join(",\n", availableParameterCodes));

                // Activate ELM327PIDFilter
                this.ELM327PIDFilter = new ELM327PIDFilter(availablePIDs, true, []);
            }
            else
            {
                // Fill all PID available
                var allAvailablePID = Enumerable.Range(0, 0x100).Select(x => (byte)x).ToList();
                this.ELM327PIDFilter = new ELM327PIDFilter(allAvailablePID, false, []);
            }
        }

        private void InitializeELM327ATCommand()
        {
            // Ignore Timeout on ReadExisting
            try
            {
                ReadExisting();
            }
            catch (TimeoutException ex)
            {
                logger.LogDebug("TimeoutException in initializeELM327ATCommand() (Ignored). Message: {Message}", ex.Message);
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
                    logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));

                    // Disable echoback
                    Write("ATE0\r");
                    logger.LogDebug("Call ATE0 to disable echoback.");
                    logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));

                    // Disable Linefeed on delimiter
                    Write("ATL0\r");
                    logger.LogDebug("Call ATL0 to disable linefeed.");
                    logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));

                    // Set protocol
                    ELM327SetProtocol(this.Option.ELM327ProtocolStr);

                    // Test communication
                    ELM327TestCommunicationToSearchProtocol();

                    // Disable space
                    Write("ATS0\r");
                    logger.LogDebug("Call ATS0 to disable space.");
                    logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));

                    // Setup ELM327 timing and timeout
                    ELM327TimingControlSet(this.Option.ELM327AdaptiveTimingMode, this.Option.ELM327TimeOut);

                    // Setup ELM327 header setting
                    ELM327SetHeader(this.Option.ELM327ReceiveAddress, this.Option.ELM327HeaderBytes);

                    // Check multiple ECU connection
                    ELM327MultipleECUNodeCheck();

                    initializeFinished = true;
                }
                catch (TimeoutException ex)
                {
                    logger.LogError(ex, "Timeout occurred during ELM327 initialization AT command settings. Wait 2sec and retry.");
                    initializeFailedCount++;
                    if (initializeFailedCount > INITIALIZE_FAILED_MAX)
                    {
                        throw new InvalidOperationException($"ELM327 initialization AT command setting failed over {INITIALIZE_FAILED_MAX} counts.");
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
        private void ELM327SetProtocol(string protocolStr)
        {
            if (string.IsNullOrEmpty(protocolStr))
            {
                logger.LogDebug("ELM327SetProtocolMode string is blank. ELM327 protocol set (AT SP) will be skipped.");
                return;
            }
            if (protocolStr.Length != 1)
                logger.LogWarning("ELM327SetProtocolMode is not a single character. AT SP command may fail.");
            if (!Regex.IsMatch(protocolStr, "[0-9]|[A-C]"))
                logger.LogWarning("ELM327SetProtocolMode is not 0-9 or A-C. AT SP command may fail.");

            string setprotocolStr = "AT SP " + protocolStr;
            Write(setprotocolStr + "\r");
            logger.LogDebug("Call {SetProtocolStr} to set ELM327 protocol.", setprotocolStr);
            logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));
        }

        private void ELM327TestCommunicationToSearchProtocol()
        {
            // Enable header out
            Write("ATH1\r");
            logger.LogDebug("Call ATH1 to enable header out.");
            logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));

            // Test communication by 0100 (Query available PID)
            Write("0100\r");
            logger.LogDebug("Call 0100 to test communication.");
            var return_0100 = ReadTo(">")
                .Split(NewLineSeparators, StringSplitOptions.None)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            logger.LogDebug("Return Msg: {msg}", Environment.NewLine + string.Join(Environment.NewLine, return_0100));

            // Disable header out
            Write("ATH0\r");
            logger.LogDebug("Call ATH0 to disable header out.");
            logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));
        }

        private void ELM327TimingControlSet(int adaptiveTimingModeSetting, int timeout)
        {
            // Adaptive timing control set
            if (adaptiveTimingModeSetting < 0 || adaptiveTimingModeSetting > 2)
                logger.LogWarning("ELM327 Adaptive timing mode is not 0-2. AT AT command may fail.");

            Write("ATAT" + adaptiveTimingModeSetting.ToString() + "\r");
            logger.LogDebug("Call AT AT{AdaptiveTimingMode} to set adaptive timing control mode.", adaptiveTimingModeSetting);
            logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));

            // Timeout set
            int timeoutToSet = timeout;
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
            logger.LogDebug("Call AT ST {TimeoutHex} to set timeout.", timeoutToSet.ToString("X2"));
            logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));
        }

        private void ELM327SetHeader(string receiveAddress, string headerBytes)
        {
            // Receive address set (ATCRA)
            if (string.IsNullOrEmpty(receiveAddress))
                logger.LogInformation("ELM327 receive address byte is not set (or blank). AT CRA command will be skipped.");
            else
            {
                Write("ATCRA" + receiveAddress + "\r");
                logger.LogDebug("Call AT CRA {ReceiveAddress} to set receive address.", receiveAddress);
                logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));
            }

            // Header byte set.
            if (string.IsNullOrEmpty(headerBytes))
                logger.LogInformation("ELM327 header byte is not set (or blank). AT SH command will be skipped.");
            else
            {
                Write("ATSH" + headerBytes + "\r");
                logger.LogDebug("Call AT SH {HeaderBytes} to set header ID.", headerBytes);
                logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));
            }
        }
        private void ELM327MultipleECUNodeCheck()
        {
            // Enable header out
            Write("ATH1\r");
            logger.LogDebug("Call ATH1 to enable header out.");
            logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));

            // Test communication by 0100 (Query available PID)
            Write("0100\r");
            logger.LogDebug("Call 0100 to search ECUs.");
            var return_0100 = ReadTo(">")
                .Split(NewLineSeparators, StringSplitOptions.None)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            logger.LogDebug("Return Msg:{msg}", Environment.NewLine + string.Join(Environment.NewLine, return_0100));

            // Check reply from multiple ECU
            var return_0100_PIDs = return_0100.Where(s => !Regex.IsMatch(s, "[^0-9A-F ]+")); // Exclude interactive messages like "SEARCHING..."
            if (return_0100_PIDs.Count() > 1)
            {
                logger.LogWarning("Multiple reply is detected on 0100 PID query. Multiple ECU nodes may be connected. Return Msg:");
                logger.LogWarning("{msg}", string.Join(Environment.NewLine, return_0100_PIDs));
                logger.LogWarning("\"elm327QueryOnlyAvailablePID\" feature may cause errors.");
                logger.LogWarning("Consider limiting the communicating ECU nodes by \"elm327HeaderBytes\" or \"elm327ReceiveAddress\" settings.");
            }

            // Disable header out
            Write("ATH0\r");
            logger.LogDebug("Call ATH0 to disable header out.");
            logger.LogDebug("Return Msg is {msg}", ReplaceCRLFWithSpace(ReadTo(">")));
        }

        protected override void communicate_main(bool slow_read_flag)
        {
            try
            {
                // Create PID list to query
                List<OBDIIParameterCode> query_OBDII_code_list = [];
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

                // Apply filter to query_OBDII_code_list
                query_OBDII_code_list = this.ELM327PIDFilter.applyToList(query_OBDII_code_list, content_table);

                // Exit loop if the PIDs to query do not exist.
                if (query_OBDII_code_list.Count <= 0)
                {
                    // If no PIDs are in query list, wait 500ms (ignored if slow_read_flag is true) then return.
                    if (!slow_read_flag)
                        Thread.Sleep(500);
                    return;
                }

                var batchedQueryCodeList = GroupBatchQueryCode(query_OBDII_code_list, this.Option.ELM327BatchQueryCount, this.Option.SeparateBatchQueryToAvoidMultiFrameResponse);
                batchedQueryCodeList.ForEach(mcode => CommunicateMultiPID(mcode, 0));

                // Invoke ELM327DataReceived event
                ELM327DataReceivedEventArgs elm327_received_eventargs = new()
                {
                    Slow_read_flag = slow_read_flag,
                    Received_Parameter_Code = new List<OBDIIParameterCode>(query_OBDII_code_list)
                };
                ELM327DataReceived(this, elm327_received_eventargs);

                // Wait before issuing next query
                if (this.Option.Waitmsec > 0)
                    Thread.Sleep(this.Option.Waitmsec);
            }
            catch (TimeoutException ex)
            {
                logger.LogWarning("ELM327 timeout occurred. Exception: {ExceptionType} Message: {Message}", ex.GetType(), ex.Message);
                communicateRealtimeIsError = true;
            }
        }

        private List<List<OBDIIParameterCode>> GroupBatchQueryCode(List<OBDIIParameterCode> queryCodeList, int batchPIDCount, bool separateBatchQueryToAvoidMultiFrameResponse)
        {
            var groupedCodeList = new List<List<OBDIIParameterCode>>();
            int codeCount = 0;
            const int responseContentByteSize = 6;
            int returnByteSum = 0;
            for (int i = 0; i < queryCodeList.Count; i++)
            {
                var code = queryCodeList[i];
                var valueByteLength = content_table[code].ReturnByteLength;
                if (i == 0)
                    groupedCodeList.Add([]);

                else if ((codeCount + 1 > batchPIDCount) ||
                    (separateBatchQueryToAvoidMultiFrameResponse && (returnByteSum + valueByteLength + 1 > responseContentByteSize)))
                {
                    groupedCodeList.Add([]);
                    codeCount = 0;
                    returnByteSum = 0;
                }
                groupedCodeList.Last().Add(code);
                codeCount++;
                returnByteSum += valueByteLength + 1;
            }

            return groupedCodeList;
        }

        private string QueryMsg(string outMsg)
        {
            DiscardInBuffer();
            // logger.LogDebug("ELM327OUT: {OutMsg}", outMsg);

            // Issue query
            Write(outMsg + "\r");
            // Read to next prompt char of '>'
            string inMsg = ReadTo(">");
            return inMsg;
        }
        private string QueryPIDs(byte[] pids, int returnByteLength)
        {
            string outMsg = MODECODE.ToString("X2") + pids.Select(pid => pid.ToString("X2")).Aggregate((prev, next) => prev + next);
            // Calculate number of ISO-TP return frames 
            int returnMessageBlocks;
            if (returnByteLength <= 7)
                returnMessageBlocks = 1;
            else if (returnByteLength <= 13)
                returnMessageBlocks = 2;
            else
                returnMessageBlocks = 3 + (returnByteLength - 14) / 7;

            // logger.LogDebug("Return message blocks: {Blocks}", returnMessageBlocks);

            // Append number of message frames at the end of query string.
            outMsg += returnMessageBlocks.ToString();
            string inMsg = QueryMsg(outMsg);
            return inMsg;
        }

        private void CommunicateMultiPID(List<OBDIIParameterCode> codes, int errorRetryCount)
        {
            if (codes.Count > 6)
                throw new ArgumentException("Code list size of multiple PID communication must be less than or equal 6.");

            var pids = codes.Select(code => content_table[code].PID).ToArray();
            int returnByteLength = 1 + codes.Select(code => content_table[code].ReturnByteLength + 1).Sum(); // Return byte length = 1(mode code) + sum (1(=PID byte) + Return byte length)
            string inMsg = QueryPIDs(pids, returnByteLength);
            // logger.LogDebug("ELM327IN: {InMsg}", inMsg);

            try
            {
                if (string.IsNullOrEmpty(inMsg))
                    throw new FormatException("Return message at communicateOnePID() is empty.");
                else if (inMsg.Contains("NO DATA"))
                {
                    var error_code_names = codes.Select(code => code.ToString());
                    switch(ActionOnNODATAReceived)
                    {
                        case ActionOnNODATAReceived.AddPIDToBlackList:
                            logger.LogWarning("ELM327 returns NO DATA on communicating PID of {PIDbytes}. Corresponding code names are {ErrorPIDNames}. These PIDs are added to blacklist.",
                                                BitConverter.ToString(pids), string.Join(",", error_code_names));
                            Array.ForEach(pids, pid => this.ELM327PIDFilter.addToBlackList(pid));
                            return;
                        case ActionOnNODATAReceived.ThrowException:
                            throw new FormatException("ELM327 returns NO DATA.");
                        case ActionOnNODATAReceived.Ignore:
                            logger.LogDebug("ELM327 returns NO DATA on communicating PID of {PIDbytes}. Corresponding code names are {ErrorPIDNames}. These PIDs will be ignored.",
                                                BitConverter.ToString(pids), string.Join(",", error_code_names));
                            return;
                    }
                }
                var parseResult = elm327MsgParser.parse(inMsg);
                var parsedValueList = parseResult.ValueStrMap
                    .Select(kvp => new KeyValuePair<OBDIIParameterCode, uint>(kvp.Key, Convert.ToUInt32(kvp.Value, 16)))
                    .ToList();
                parsedValueList.ForEach(kvp => content_table[kvp.Key].RawValue = kvp.Value);
            }
            catch (TimeoutException ex)
            {
                logger.LogError("ELM327COM timeout. ExceptionType: {ExceptionType}, Message: {Message}", ex.GetType(), ex.Message);
                communicateRealtimeIsError = true;
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentOutOfRangeException || ex is KeyNotFoundException || ex is ArgumentException)
            {
                logger.LogWarning("{ExceptionType} {Message} Received string is: {ReceivedString}", ex.GetType(), ex.Message, inMsg);
                logger.LogWarning("Requested PID is: {PIDs}", BitConverter.ToString(pids));
                logger.LogWarning("{StackTrace}", ex.StackTrace);
                if (errorRetryCount < PID_COMMUNICATE_RETRY_MAX)
                {
                    logger.LogWarning("Retry communication cycle: {RetryCount}", errorRetryCount + 1);
                    CommunicateMultiPID(codes, errorRetryCount + 1);
                }
                else
                {
                    logger.LogError("PID communication retry count exceeds maximum ({MaxRetries})", PID_COMMUNICATE_RETRY_MAX);
                    communicateRealtimeIsError = true;
                }
            }
        }

        // Communication on 1 PID
        private void CommunicateOnePID(OBDIIParameterCode code, int errorRetryCount)
        {
            CommunicateMultiPID([code], errorRetryCount);
        }

        private string ReplaceCRLFWithSpace(string instr)
        {
            return instr.Replace("\r", " ").Replace("\n", " ");
        }

        private string DiscardStringAfterChar(string instr, char delimiter)
        {
            int index = instr.IndexOf(delimiter);
            string instrTemp;

            // Remove first char if the first char is delimiter
            if (index == 0)
                instrTemp = instr.Remove(0, 1);
            else
                instrTemp = instr;

            index = instrTemp.IndexOf(delimiter);

            if (index < 0)
                return instrTemp;
            else
                return instrTemp[..index];
        }

        private List<byte> GetAvailablePIDs()
        {
            var byteParser = new ELM327OutMessageByteParser();
            var availablePIDDecoder = new AvailablePIDMessageDecoder();
            var availablePIDList = new List<byte>();
            for (uint pidOffset = 0x00; pidOffset <= 0xFF; pidOffset += 0x20)
            {
                int returnByteLength = 6; // Modecode 1 byte + pid 1 byte + data 4 bytes
                var inMsg = QueryPIDs([(byte)pidOffset], returnByteLength);
                var inBytes = byteParser.parse(inMsg).Skip(2).ToArray();
                var availablePIDs_temp = availablePIDDecoder.parse((byte)pidOffset, inBytes);
                availablePIDList.AddRange(availablePIDs_temp);
                if (!availablePIDList.Contains((byte)(pidOffset + 0x20)))
                    break;
            }

            return availablePIDList;
        }
        public double GetValue(OBDIIParameterCode code)
        {
            return content_table[code].Value;
        }

        public UInt32 GetRawValue(OBDIIParameterCode code)
        {
            return content_table[code].RawValue;
        }

        public string GetUnit(OBDIIParameterCode code)
        {
            return content_table[code].Unit;
        }

        public bool GetSlowreadFlag(OBDIIParameterCode code)
        {
            return content_table[code].SlowReadEnable;
        }

        public bool GetFastreadFlag(OBDIIParameterCode code)
        {
            return content_table[code].FastReadEnable;
        }

        public void SetSlowreadFlag(OBDIIParameterCode code, bool flag)
        {
            SetSlowreadFlag(code, flag, false);
        }

        public void SetSlowreadFlag(OBDIIParameterCode code, bool flag, bool quiet)
        {
            if (!quiet)
                logger.LogDebug("Slowread flag of {Code} is enabled.", code);
            content_table[code].SlowReadEnable = flag;
        }

        public void SetFastreadFlag(OBDIIParameterCode code, bool flag)
        {
            SetFastreadFlag(code, flag, false);
        }

        public void SetFastreadFlag(OBDIIParameterCode code, bool flag, bool quiet)
        {
            if (!quiet)
                logger.LogDebug("Fastread flag of {Code} is enabled.", code);
            content_table[code].FastReadEnable = flag;
        }

        public void SetAllDisable()
        {
            SetAllDisable(false);
        }

        public void SetAllDisable(bool quiet)
        {
            if (!quiet)
                logger.LogDebug("All flags reset.");
            content_table.setAllDisable();
        }
    }
}