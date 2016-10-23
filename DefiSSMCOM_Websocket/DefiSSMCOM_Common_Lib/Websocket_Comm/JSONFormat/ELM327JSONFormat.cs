using System;
using DefiSSMCOM.OBDII;

namespace DefiSSMCOM.WebSocket.JSON
{
    public class ELM327COMReadJSONFormat : JSONFormats
    {
        public const string ModeCode = "ELM327_COM_READ";
        public const string FastReadModeCode = "FAST";
        public const string SlowReadModeCOde = "SLOW";
        public ELM327COMReadJSONFormat()
        {
            mode = ModeCode;
        }
        public string code;
        public string read_mode;
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
                    if (!(Enum.IsDefined(typeof(OBDIIParameterCode), code)))
                        throw new JSONFormatsException("OBDII_Parameter_Code property of " + ModeCode + " packet is not valid.");
                    if (read_mode != FastReadModeCode && read_mode != SlowReadModeCOde)
                        throw new JSONFormatsException("read_mode of " + ModeCode + " packet is not valid (Should be SLOW or FAST).");
                    if (flag != true && flag != false)
                        throw new JSONFormatsException("flag of " + ModeCode + " packet is not valid.");
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new JSONFormatsException("Null is found in " + ModeCode + " packet.", ex);
            }
        }
    }

    public class ELM327SLOWREADIntervalJSONFormat : JSONFormats
    {
        public const string ModeCode = "ELM327_SLOWREAD_INTERVAL";
        public int interval;
        public ELM327SLOWREADIntervalJSONFormat()
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
