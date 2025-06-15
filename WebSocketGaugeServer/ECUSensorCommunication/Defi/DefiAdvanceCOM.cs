// This code is experimental, should not work well

using System;
using System.Linq;
using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi
{
    public class DefiAdvanceCOM : COMCommon, IDefiCOM
    {
        // DefiAdvance packet byte size
        const int DEFI_PACKET_SIZE = 35; // Header 1 + content 4 bytes 
        private readonly ILogger logger;
        private readonly DefiAdvanceContentTable content_table;

        // Defilink received event
        public event EventHandler DefiPacketReceived;

        public DefiAdvanceCOM(ILoggerFactory logger, string comPortName)
            : base(new COMCommonOption(comPortName, Parity.None), logger)
        {
            this.logger = logger.CreateLogger<DefiAdvanceCOM>();
            this.content_table = new DefiAdvanceContentTable();

            DefaultBaudRate = 19200;

            // Baudrate on resetting serial port (ref: communicate_reset())
            // When using FT232RL, only baudrates of the form 3000000/n are allowed
            // (n is integer or integer + 0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875)
            ResetBaudRate = 9600;

            ReadTimeout = 500;
        }

        public double get_value(DefiParameterCode code)
        {
            return content_table[code].Value;
        }

        public UInt32 get_raw_value(DefiParameterCode code)
        {
            return content_table[code].RawValue;
        }

        public string get_unit(DefiParameterCode code)
        {
            return content_table[code].Unit;
        }

        protected override void communicate_main(bool slowread_flag)
        {
            byte[] firstInbuf = new byte[1];
            byte[] remainingInbuf;

            try
            {
                // Read until finding delimiter character
                do
                {
                    firstInbuf[0] = (byte)ReadByte();
                }
                while (firstInbuf[0] < 'A' || firstInbuf[0] > 'G');

                remainingInbuf = ReadMultiBytes(DEFI_PACKET_SIZE - 1);
            }
            catch (TimeoutException ex)
            {
                // On read timeout, set error flag and perform reset in next cycle
                logger.LogWarning("Defi packet timeout. ExceptionType={ExceptionType}, Message={Message}", ex.GetType().ToString(), ex.Message);
                communicateRealtimeIsError = true;
                return;
            }

            // Concat firstInbuf and remainingInbuf, and convert to char[]
            var inbuf = firstInbuf.Concat(remainingInbuf).Select(b => (char)b).ToArray();

            // Discard remaining buffer data
            // DiscardInBuffer();

            // Interpret ReceiverID and store in private variables
            for (int j = 0; j < DEFI_PACKET_SIZE; j += 5)
            {
                try
                {
                    if (inbuf[j] == (char)content_table[DefiParameterCode.Manifold_Absolute_Pressure].Receiver_id)
                    {
                        string boost_str = new string(inbuf, j + 1, 4);
                        content_table[DefiParameterCode.Manifold_Absolute_Pressure].RawValue = UInt32.Parse(boost_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Engine_Speed].Receiver_id)
                    {
                        string tacho_str = new string(inbuf, j + 1, 4);
                        content_table[DefiParameterCode.Engine_Speed].RawValue = UInt32.Parse(tacho_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Oil_Pressure].Receiver_id)
                    {
                        string oilpres_str = new string(inbuf, j + 1, 4);
                        content_table[DefiParameterCode.Oil_Pressure].RawValue = UInt32.Parse(oilpres_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Fuel_Rail_Pressure].Receiver_id)
                    {
                        string fuelpres_str = new string(inbuf, j + 1, 4);
                        content_table[DefiParameterCode.Fuel_Rail_Pressure].RawValue = UInt32.Parse(fuelpres_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Exhaust_Gas_Temperature].Receiver_id)
                    {
                        string exttemp_str = new string(inbuf, j + 1, 4);
                        content_table[DefiParameterCode.Exhaust_Gas_Temperature].RawValue = UInt32.Parse(exttemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Oil_Temperature].Receiver_id)
                    {
                        string oiltemp_str = new string(inbuf, j + 1, 4);
                        content_table[DefiParameterCode.Oil_Temperature].RawValue = UInt32.Parse(oiltemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Coolant_Temperature].Receiver_id)
                    {
                        string watertemp_str = new string(inbuf, j + 1, 4);
                        content_table[DefiParameterCode.Coolant_Temperature].RawValue = UInt32.Parse(watertemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                }
                catch (FormatException ex)
                {
                    // If Defi packet is broken, set error flag and perform reset in next cycle
                    logger.LogWarning("Invalid Defi packet. ExceptionType={ExceptionType}, Message={Message}", ex.GetType().ToString(), ex.Message);
                    communicateRealtimeIsError = true;
                    return;
                }
            }

            // Invoke PacketReceived event
            DefiPacketReceived?.Invoke(this, EventArgs.Empty);
        }
    }
}
