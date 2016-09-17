using System;

namespace DefiSSMCOM.WebSocket.JSON
{
    public class ArduinoWSSendJSONFormat : JSONFormats
    {
        public const string ModeCode = "ARDUINO_WS_SEND";
        public ArduinoWSSendJSONFormat()
        {
            mode = ModeCode;
        }
        public string code;
        public bool flag;

        public override void Validate()
        {
            try
            {
                if (mode != ModeCode)
                {
                    throw new JSONFormatsException("mode property of " + this.GetType().ToString() + " packet is not valid.");
                }
                else
                {
                    if (!(Enum.IsDefined(typeof(ArduinoParameterCode), code)))
                        throw new JSONFormatsException("Arduino_Parameter_Code property of " + ModeCode + " packet is not valid.");
                    if (flag != true && flag != false)
                        throw new JSONFormatsException("flag of "+ ModeCode +" packet is not valid.");
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new JSONFormatsException("Null is found in " + ModeCode + " packet.", ex);
            }
        }
    }

    public class ArduinoWSIntervalJSONFormat : JSONFormats
    {
        public const string ModeCode = "ARDUINO_WS_INTERVAL";
        public int interval;
        public ArduinoWSIntervalJSONFormat()
        {
            mode = ModeCode;
        }

        public override void Validate()
        {
            if (mode != ModeCode)
            {
                throw new JSONFormatsException("mode property is not valid.");
            }
            else
            {
                if (interval < 0)
                    throw new JSONFormatsException("interval property of " + ModeCode + " packet is less than 0.");
            }
        }
    }
}
