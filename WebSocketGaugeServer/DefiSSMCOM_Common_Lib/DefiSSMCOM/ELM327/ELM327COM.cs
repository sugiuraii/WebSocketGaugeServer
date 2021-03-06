﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DefiSSMCOM.OBDII
{
    public class ELM327COM : COMCommon
    {
        private OBDIIContentTable content_table;
        public const byte MODECODE = 0x01;
        //Wait time after calling ATZ command (in milliseconds)
        private const int WAIT_AFTER_ATZ = 4000;

        //Recommended baudrate for USB ELM327 adaptor
        private const int RECOMMENDED_BAUD_RATE = 115200;

        private const int INITIALIZE_FAILED_MAX = 30;
        private const int PID_COMMUNICATE_RETRY_MAX = 5;

        //ELM327COM data received event
        public event EventHandler<ELM327DataReceivedEventArgs> ELM327DataReceived;

        //Constructor
        public ELM327COM()
        {
            //Setup serial port
            DefaultBaudRate = 115200;

            ResetBaudRate = 4800;
            ReadTimeout = 500;

            content_table = new OBDIIContentTable();
        }

        //Changing DefaultBaudRate is allowed in ELM327COM
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
            ReadExisting();
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
                {
                    communicateOnePID(code);
                }

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
            outMsg = MODECODE.ToString("X2") + outPID + returnByteLength.ToString("X1");
            Write(outMsg + "\r");
            //logger.Debug("ELM327OUT:" + outMsg);
            String inMsg = "";

            try
            {
                Int32 returnValue;
                //inMsg = ReadTo("\r");
                
                // Read to next prompt char of '>'
                inMsg = ReadTo(">");
                // Discard after the char of \r
                // (discard all after \r)
                // (This routine is implemented to make countermeasure in the case of multiple message returned.
                inMsg = discardStringAfterChar(inMsg, '\r');

                //logger.Debug("ELM327IN:" + inMsg);
                
                // Get ECU data.
                inMsg = inMsg.Replace(">","").Replace("\n","").Replace("\r","");
                
                // Check ECU data format.
                if (inMsg.Equals(""))
                    throw new FormatException("Return message at communicateOnePID() is empty.");
                else if (inMsg.Contains("NO DATA"))
                {
                    logger.Warn("ELM327 returns NO DATA." + " OutPID :" + outPID + " Code : " + code.ToString());
                    return;
                }

                String inPID = inMsg.Substring(2, 2);
                if (!inPID.Equals(outPID))
                {
                    throw new FormatException("PID return from ELM327 does not match with commanded PID." + "outPID : " + outPID + " inPID :" + inPID);
                }

                //logger.Debug("Filtered ELM327IN:" + inMsg);
                returnValue = Convert.ToInt32(inMsg.Remove(0, 4), 16);

                content_table[code].RawValue = returnValue;
            }
            catch(TimeoutException ex)
            {
                logger.Error("ELM327COM timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
            catch(FormatException ex)
            {
                logger.Warn(ex.GetType().ToString() + " " + ex.Message + " Received string Is : " + inMsg);
                if (errorRetryCount < PID_COMMUNICATE_RETRY_MAX)
                {
                    logger.Warn("Retry communication" + " OutPID :" + outPID + " Code : " + code.ToString());
                    communicateOnePID(code, errorRetryCount + 1);
                }
                else
                {
                    logger.Error("PID communication retry count exceeds maximum (" + PID_COMMUNICATE_RETRY_MAX.ToString() + ")");
                    communicateRealtimeIsError = true;
                }
            }
            catch(ArgumentOutOfRangeException ex)
            {
                logger.Warn(ex.GetType().ToString() + " " + ex.Message + " Received string Is : " + inMsg);
                if (errorRetryCount < PID_COMMUNICATE_RETRY_MAX)
                {
                    logger.Warn("Retry communication"  + " OutPID :" + outPID + " Code : " + code.ToString());
                    communicateOnePID(code, errorRetryCount + 1);
                }
                else
                {
                    logger.Error("PID communication retry count exceeds maximum (" + PID_COMMUNICATE_RETRY_MAX.ToString() + ")");
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
                return instrTemp.Substring(0,index);
        }

    }


    public class ELM327DataReceivedEventArgs : EventArgs
    {
        public bool Slow_read_flag { get; set; }
        public List<OBDIIParameterCode> Received_Parameter_Code { get; set; }
    }
}
