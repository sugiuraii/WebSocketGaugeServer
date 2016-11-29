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

        //ELM327COM data received event
        public event EventHandler<ELM327DataReceivedEventArgs> ELM327DataReceived;

        //コンストラクタ
        public ELM327COM()
        {
            //シリアルポート設定
            DefaultBaudRate = 9600;
            ResetBaudRate = 4800;
            ReadTimeout = 500;

            content_table = new OBDIIContentTable();
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

            DiscardInBuffer();

            // Input initial AT commands
            Write("ATZ\r");
            logger.Debug("Call ATZ to initialize. Return Msg is " + ReadTo(">"));
            // Disable space.
            Write("ATS0\r");
            logger.Debug("Call ATS0 to disable space. Return Msg is " + ReadTo(">"));
            // Disable echoback.
            Write("ATE0\r");
            logger.Debug("Call ATE0 to disable echoback. Return Msg is " + ReadTo(">"));

            DiscardInBuffer();
        }

        protected override void communicate_main(bool slow_read_flag)
        {
            try
            {
                int i;

                //クエリするSSM_codeリストの作成
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

                //クエリするSSM_codeがない場合は抜ける
                if (query_OBDII_code_list.Count <= 0)
                {
                    //SSM_codeがない場合、すぐに抜けると直後にcommunicate_mainが呼び出されCPUを占有するので、500ms待つ
                    //SlowReadの場合、この処理はしない
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

        //1PID分の通信
        private void communicateOnePID(OBDIIParameterCode code)
        {
            //シリアルポート入力バッファ掃除
            DiscardInBuffer();

            String outMsg;
            outMsg = MODECODE.ToString("X2") + content_table[code].PID.ToString("X2") + content_table[code].ReturnByteLength.ToString("X2");
            Write(outMsg + "\r");
            logger.Debug("ELM327OUT:" + outMsg);
            ReadLine();

            try
            {
                String inMsg;
                Int32 returnValue;

                inMsg = ReadTo(">").Replace("\r", "").Replace("\n", "").Replace(">","");
                logger.Debug("ELM327IN:" + inMsg);
                returnValue = Convert.ToInt32(inMsg.Remove(0, 4), 16);

                content_table[code].RawValue = returnValue;
            }
            catch(TimeoutException ex)
            {
                logger.Warn("ELM327COM timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
            catch(FormatException ex)
            {
                logger.Error("String conversion to Int32 was failed " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
            }
        }
    }

    public class ELM327DataReceivedEventArgs : EventArgs
    {
        public bool Slow_read_flag { get; set; }
        public List<OBDIIParameterCode> Received_Parameter_Code { get; set; }
    }
}