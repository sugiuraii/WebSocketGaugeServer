using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DefiSSMCOM.WebSocket.JSON
{
    public class DefiWSSendJSONFormat : JSONFormats
    {
        public const string ModeCode = "DEFI_WS_SEND";
        public DefiWSSendJSONFormat()
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
                    if (!(Enum.IsDefined(typeof(DefiParameterCode), code)))
                        throw new JSONFormatsException("Defi_Parameter_Code property of " + ModeCode + " packet is not valid.");
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

    public class DefiWSIntervalJSONFormat : JSONFormats
    {
        public const string ModeCode = "DEFI_WS_INTERVAL";
        public int interval;
        public DefiWSIntervalJSONFormat()
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
