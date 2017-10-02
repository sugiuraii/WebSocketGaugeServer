using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DefiSSMCOM.OBDII
{
    public class ELM327COM : COMCommon
    {
        /// <summary>
        /// Switch to query available PID.
        /// Currently this flag is set disabled since PID query function have problem in multiple ECU communication.
        /// </summary>
        private const bool QUERY_AND_CHECK_AVAILABLE_PID = false;

        /// <summary>
        /// Content table to store read data.
        /// </summary>
        private OBDIIContentTable content_table;
        
        /// <summary>
        /// Flag list whether PID is available on ECU. (will be filled at the initial stage of communication)
        /// </summary>
        private Dictionary<byte, bool> PIDAvailableFlag = new Dictionary<byte, bool>();
        
        /// <summary>
        /// OBD communication mode (0x01 = read current frame)
        /// </summary>
        public const byte MODECODE = 0x01;
        
        /// <summary>
        /// Wait time after calling ATZ command (in milliseconds)
        /// </summary>
        private const int WAIT_AFTER_ATZ = 4000;

        /// <summary>
        /// Recommended baudrate for USB ELM327 adaptor
        /// </summary>
        private const int RECOMMENDED_BAUD_RATE = 115200;

        /// <summary>
        /// Maximum count to retry initialization.
        /// </summary>
        private const int INITIALIZE_FAILED_MAX = 30;

        /// <summary>
        /// Eventhandler called when ELM327 data is received.
        /// </summary>
        public event EventHandler<ELM327DataReceivedEventArgs> ELM327DataReceived;

        /// <summary>
        /// Constructor
        /// </summary>
        public ELM327COM()
        {
            //Setup serial port
            DefaultBaudRate = 115200;

            ResetBaudRate = 4800;
            ReadTimeout = 500;

            content_table = new OBDIIContentTable();
        }

        /// <summary>
        /// Override ELM327 default baudrate
        /// Changing DefaultBaudRate is allowed in ELM327COM.
        /// </summary>
        /// <param name="baudRate">Baudrate to change to.</param>
        public void overrideDefaultBaudRate(int baudRate)
        {
            DefaultBaudRate = baudRate;
        }

        public double get_value(OBDIIParameterCode code)
        {
            return content_table[code].Value;
        }

        public Int32 get_raw_value(OBDIIParameterCode code)
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
                logger.Debug("Slowread flag of " + code.ToString() + "is enabled.");

            if (!checkPIDisAvailable(content_table[code].PID) && flag)
                logger.Warn("SlowRead-requested parameter code of " + code.ToString() + "is not available on ECU. Read request is discarded.");
            else
                content_table[code].SlowReadEnable = flag;
        }

        public void set_fastread_flag(OBDIIParameterCode code, bool flag)
        {
            set_fastread_flag(code, flag, false);
        }
        public void set_fastread_flag(OBDIIParameterCode code, bool flag, bool quiet)
        {
            if (!quiet)
                logger.Debug("Fastread flag of " + code.ToString() + "is enabled.");

            if (!checkPIDisAvailable(content_table[code].PID) && flag)
                logger.Warn("FastRead-requested parameter code of " + code.ToString() + "is not available on ECU. Read request is discarded.");
            else
                content_table[code].FastReadEnable = flag;
        }

        public void set_all_disable()
        {
            set_all_disable(false);
        }

        public void set_all_disable(bool quiet)
        {
            if (!quiet)
                logger.Debug("All flag reset.");
            content_table.setAllDisable();
        }

        protected override void communicate_initialize()
        {
            base.communicate_initialize();
            if (DefaultBaudRate != RECOMMENDED_BAUD_RATE)
                logger.Warn("Baurdate is different from recommended ELM327-USB bardrate of " + RECOMMENDED_BAUD_RATE.ToString() + "bps");

            initializeELM327ATCommand();
        }

        private void initializeELM327ATCommand()
        {
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
                    logger.Debug("Call ATZ to initialize. Return Msg is " + ReadTo(">"));

                    // Disable space.
                    Write("ATS0\r");
                    logger.Debug("Call ATS0 to disable space. Return Msg is " + ReadTo(">"));
                    // Disable echoback.
                    Write("ATE0\r");
                    logger.Debug("Call ATE0 to disable echoback. Return Msg is " + ReadTo(">"));
                    // Disable Linefeed on delimiter
                    Write("ATL0\r");
                    logger.Debug("Call ATL0 to disable linefeed. Return Msg is " + ReadTo(">"));

                    //Query available PIDs to ECU
                    if (QUERY_AND_CHECK_AVAILABLE_PID)
                        queryAvailablePIDs();
                    else
                        logger.Warn("Available PID query is disabled. Any PIDs will be queried to ECU.");

                    initializeFinished = true;
                }
                catch (TimeoutException ex)
                {
                    logger.Error("Timeout is occured during ELM327 initialization AT command settings. Wait 2sec and retry.. : " + ex.Message);
                    initializeFailedCount++;
                    if(initializeFailedCount > INITIALIZE_FAILED_MAX)
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

                foreach( OBDIIParameterCode code in query_OBDII_code_list)
                    content_table[code].RawValue = communicateOnePID(code);

                //Invoke SSMDatareceived event
                ELM327DataReceivedEventArgs elm327_received_eventargs = new ELM327DataReceivedEventArgs();
                elm327_received_eventargs.Slow_read_flag = slow_read_flag;
                elm327_received_eventargs.Received_Parameter_Code = new List<OBDIIParameterCode>(query_OBDII_code_list);
                ELM327DataReceived(this, elm327_received_eventargs);
            }
            catch (TimeoutException ex)
            {
                logger.Warn("SSMCOM timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
        }

        /// <summary>
        /// Send PID and get value from ELM327.
        /// </summary>
        /// <param name="code">OBDII parameter code</param>
        /// <returns>Returned data.</returns>
        private Int32 communicateOnePID(OBDIIParameterCode code)
        {
            String inMsg = "";
            try
            {
                byte PID = content_table[code].PID;
                int returnByteLength = content_table[code].ReturnByteLength;
                inMsg = communicateOnePID(PID, returnByteLength);
                return Convert.ToInt32(inMsg, 16);
            }
            catch (FormatException ex)
            {
                logger.Warn("String conversion to Int32 is failed");
                logger.Warn(ex.GetType().ToString());
                logger.Warn(ex.Message);
                logger.Warn("Send code : " + code.ToString());
                logger.Warn("Received message : " + inMsg);
                //communicateRealtimeIsError = true;
                return 0;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                logger.Warn("String conversion to Int32 is failed");
                logger.Warn(ex.GetType().ToString());
                logger.Warn(ex.Message);
                logger.Warn("Send code : " + code.ToString());
                logger.Warn("Received message : " + inMsg);
                //communicateRealtimeIsError = true;
                return 0;
            }
        }

        /// <summary>
        /// Send PID and get value from ELM327.
        /// </summary>
        /// <param name="PID">PID byte</param>
        /// <param name="returnByteLength">Length of return byte from ECU.</param>
        /// <returns>Returned data.</returns>
        private String communicateOnePID(byte PID, int returnByteLength)
        {
            //Clean up serial port buffer
            DiscardInBuffer();

            String outMsg;
            outMsg = MODECODE.ToString("X2") + PID.ToString("X2") + returnByteLength.ToString("X1");
            Write(outMsg + "\r");
            //logger.Debug("ELM327OUT:" + outMsg);
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

                //logger.Debug("ELM327IN:" + inMsg);
                inMsg = inMsg.Replace(">","").Replace("\n","").Replace("\r","");
                if (inMsg.Equals(""))
                {
                    logger.Warn("ELM327 respond blank message.");
                    return "0";
                }

                return inMsg.Remove(0, 4);
            }
            catch(TimeoutException ex)
            {
                logger.Warn("ELM327COM timeout. ");
                logger.Warn(ex.GetType().ToString());
                logger.Warn(ex.Message);
                logger.Warn("Send message : " + outMsg);
                communicateRealtimeIsError = true;
                return "0";
            }
        }

        /// <summary>
        /// Discard string after given character.
        /// </summary>
        /// <param name="instr">Input string.</param>
        /// <param name="delimiter">Character to discard after.</param>
        /// <returns>Result string.</returns>
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
                return instrTemp.Substring(0,index);
        }

        /// <summary>
        /// Query availavble PIDs to ECU and store to PIDAvailableFlags;
        /// </summary>
        private void queryAvailablePIDs()
        {
            logger.Debug("Query available PIDs to ECU...");

            for (byte PIDoffset = 0x00; PIDoffset <= 0xc0; PIDoffset += 0x20)
            {
                String PIDAvaliablequeryResult = communicateOnePID(PIDoffset, 4);
                if(!setAvailablePIDFlags(PIDoffset, PIDAvaliablequeryResult))
                    break;
            }
            
            // Generate available PID code list string and output to log.
            String availablePIDListStr = "";
            foreach (KeyValuePair<byte, bool> PID in PIDAvailableFlag)
            {
                if (PID.Value)
                    availablePIDListStr += (PID.Key.ToString("X2") + " ");
            }

            logger.Debug("Available PID : " + availablePIDListStr);

        }

        /// <summary>
        /// Set available PID flags.
        /// </summary>
        /// <param name="offsetPID"></param>
        /// <param name="hexAvailablePIDFlags"></param>
        /// <returns>Next PID group is available or not</returns>
        private bool setAvailablePIDFlags(byte offsetPID, string hexPIDFlagString)
        {
            if(hexPIDFlagString.Length != 8)
                throw new ArgumentException("Available PID flag string is not 8 (=4bytes). OffsetPID is " + offsetPID.ToString(), "hexPIDFlagString");

            byte[] PIDFlagToSet = new byte[4];
            PIDFlagToSet[0] = Convert.ToByte(hexPIDFlagString.Substring(0, 2), 16);
            PIDFlagToSet[1] = Convert.ToByte(hexPIDFlagString.Substring(2, 2), 16);
            PIDFlagToSet[2] = Convert.ToByte(hexPIDFlagString.Substring(4, 2), 16);
            PIDFlagToSet[3] = Convert.ToByte(hexPIDFlagString.Substring(6, 2), 16);

            for(int i = 0; i < 4; i++)
            {
                for(int j = 7; j >= 0; j--)
                {
                    int bitoffset = 8 - j;
                    byte targetPID = (byte)(offsetPID + i * 8 + bitoffset);
                    bool flag = ((PIDFlagToSet[i] & (0x01) << j) > 0) ? true:false;
                    PIDAvailableFlag[targetPID] = flag;
                }
            }
            
            // Check next PID gruop is availale or not
            bool nextPIDGroupAvailable = (PIDFlagToSet[3] & (0x01)) > 0 ? true : false;
            return nextPIDGroupAvailable;
        }

        /// <summary>
        /// Check PID is available or not.
        /// </summary>
        /// <param name="PID">PID to check.</param>
        /// <returns>Result</returns>
        private bool checkPIDisAvailable(byte PID)
        {
            // Pass-through checking if QUERY_AND_CHECK_AVAILABLE_PID is false.
            if (!QUERY_AND_CHECK_AVAILABLE_PID)
                return true;

            bool result;
            try
            {
                result = PIDAvailableFlag[PID];
            }
            catch(KeyNotFoundException ex)
            {
                result = false;
            }

            return result;
        }
    }


    public class ELM327DataReceivedEventArgs : EventArgs
    {
        public bool Slow_read_flag { get; set; }
        public List<OBDIIParameterCode> Received_Parameter_Code { get; set; }
    }
}
