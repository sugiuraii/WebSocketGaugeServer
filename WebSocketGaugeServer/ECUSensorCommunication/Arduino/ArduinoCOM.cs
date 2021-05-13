using System;
using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino
{
    public class ArduinoCOM : COMCommon, IArduinoCOM
    {
        private ArduinoContentTable content_table;

        // Number of row sent from arduino by 1cycle (Tacho + Speed + ADC6ch = 8ch)
        private const int NUM_ROWS_PER_CYCLE = 8;
        // Arduino received Event
        public event EventHandler ArduinoPacketReceived;
        private readonly ILogger logger;
        //Constructor
        public ArduinoCOM(ILoggerFactory logger) : base(logger)
        {
            this.logger = logger.CreateLogger<ArduinoCOM>();
            content_table = new ArduinoContentTable();

            //Default baudrate (can be overrided by setting xml file)
            DefaultBaudRate = 38400;
            //Baudrate on emergency reset(refer communticate_reset()参)
            //On using FT232RL, baudrate should be 3000000/n (n is integer or x.125, x.25, x.375, x.5, x.625, x.75, x.875)
            ResetBaudRate = 9600;

            Parity = Parity.None;
            ReadTimeout = 5000;
        }

        //Override ArudinoCOM DefaultBaudRate
        public void overrideDefaultBaudRate(int baudRate)
        {
            DefaultBaudRate = baudRate;
        }

        //Main method for communication
        protected override void communicate_main(bool slowread_flag) //slowread flag is ignored on arduinoCOM
        {
            int i;
            string readbuf;
            char headerCode;
            
            //Read
            for (i = 0; i < NUM_ROWS_PER_CYCLE; i++)
            {
                try
                {
                    readbuf = ReadLine();
                    if (readbuf.Length > 0)
                        headerCode = readbuf[0];
                    else
                    {
                        //Header code is failed to read (0 length)
                        logger.LogWarning("Arduino header read failed (0length data packet)");
                        return;
                    }
                }
                catch (TimeoutException ex)
                {
                    //On timeout, set communicateRealtimeIsError = true to try reset on next cycle.
                    logger.LogWarning("Arduino packet timeout. " + ex.GetType().ToString() + " " + ex.Message);
                    communicateRealtimeIsError = true;
                    return;
                }

                //Read header code and store values into variables.
                try
                {
                    bool paramCodeHit = false;
                    foreach (ArduinoParameterCode paramCode in Enum.GetValues(typeof(ArduinoParameterCode)))
                    {
                        if (headerCode == content_table[paramCode].Header_char)
                        {
                            content_table[paramCode].RawValue = UInt32.Parse(readbuf.Remove(0, 1));
                            paramCodeHit = true;
                            break;
                        }
                    }

                    //Invoke warning if unknown header code is received
                    if(!paramCodeHit)
                        logger.LogWarning("Header code matching is failed. Header code is : " + headerCode);
                }
                catch (FormatException ex)
                {
                    // If the message from arduino is corrupt, set communicatRealtime error flag to try reset on next cycle.
                    logger.LogWarning("Invalid Arduino packet format. " + ex.GetType().ToString() + " " + ex.Message);
                    communicateRealtimeIsError = true;
                    return;
                }
            }
            // Invoke PacketReceived Event
            ArduinoPacketReceived(this, EventArgs.Empty);
        }

        public double get_value(ArduinoParameterCode code)
        {
            return content_table[code].Value;
        }

        public UInt32 get_raw_value(ArduinoParameterCode code)
        {
            return content_table[code].RawValue;
        }

        public string get_unit(ArduinoParameterCode code)
        {
            return content_table[code].Unit;
        }
    }
}
