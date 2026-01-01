using System;
using System.Linq;
using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi
{
    public class DefiCOM : COMCommon, IDefiCOM
    {
        // DefiLink packet byte size
        const int DEFI_PACKET_SIZE = 35;
        private readonly ILogger logger;
        private readonly DefiContentTable content_table;

        // DefiLink received event
        public event EventHandler DefiPacketReceived;

        public DefiCOM(ILoggerFactory logger, string comPortName) : base(new COMCommonOption(comPortName, Parity.Even), logger)
        {
            this.logger = logger.CreateLogger<DefiCOM>();
            this.content_table = new DefiContentTable();

            DefaultBaudRate = 19200;
            // Baudrate on resetting serial port (ref: communicate_reset())
            // When using FT232RL, baud rate must be 3000000/n
            // where n is integer or integer + {0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875}
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
                // Read until finding delimiter character between 0x01 and 0x0f inclusive
                do
                {
                    firstInbuf[0] = (byte)ReadByte();
                }
                while (firstInbuf[0] < 0x01 || firstInbuf[0] > 0x0f);

                remainingInbuf = ReadMultiBytes(DEFI_PACKET_SIZE - 1);
            }
            catch (TimeoutException ex)
            {
                // On read timeout, set error flag and schedule reset for next cycle
                logger.LogWarning("Defi packet timeout. Exception: {ExceptionType}, Message: {Message}", ex.GetType().ToString(), ex.Message);
                communicateRealtimeIsError = true;
                return;
            }

            // Concatenate first byte and remaining bytes, then convert to char array
            var inbuf = firstInbuf.Concat(remainingInbuf).Select(b => (char)b).ToArray();

            // Discard remaining buffer data if needed
            // DiscardInBuffer();

            // Parse ReceiverID and store raw values
            for (int j = 0; j < DEFI_PACKET_SIZE; j += 5)
            {
                try
                {
                    if (inbuf[j] == (char)content_table[DefiParameterCode.Manifold_Absolute_Pressure].Receiver_id)
                    {
                        string boost_str = new string(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Manifold_Absolute_Pressure].RawValue = UInt32.Parse(boost_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Engine_Speed].Receiver_id)
                    {
                        string tacho_str = new string(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Engine_Speed].RawValue = UInt32.Parse(tacho_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Oil_Pressure].Receiver_id)
                    {
                        string oilpres_str = new string(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Oil_Pressure].RawValue = UInt32.Parse(oilpres_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Fuel_Rail_Pressure].Receiver_id)
                    {
                        string fuelpres_str = new string(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Fuel_Rail_Pressure].RawValue = UInt32.Parse(fuelpres_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Exhaust_Gas_Temperature].Receiver_id)
                    {
                        string exttemp_str = new string(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Exhaust_Gas_Temperature].RawValue = UInt32.Parse(exttemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Oil_Temperature].Receiver_id)
                    {
                        string oiltemp_str = new string(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Oil_Temperature].RawValue = UInt32.Parse(oiltemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Coolant_Temperature].Receiver_id)
                    {
                        string watertemp_str = new string(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Coolant_Temperature].RawValue = UInt32.Parse(watertemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                }
                catch (FormatException ex)
                {
                    // If Defi packet is corrupted, set error flag and schedule reset
                    logger.LogWarning(ex, "Invalid Defi packet.");
                    communicateRealtimeIsError = true;
                    return;
                }
            }

            // Invoke DefiPacketReceived event safely
            DefiPacketReceived.Invoke(this, EventArgs.Empty);
        }
    }
}
