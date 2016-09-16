using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DefiSSMCOM.WebSocket.JSON
{
    public class SSMCOMReadJSONFormat : JSONFormats
    {
        public const string ModeCode = "SSM_COM_READ";
        public const string FastReadModeCode = "FAST";
        public const string SlowReadModeCOde = "SLOW";
        public SSMCOMReadJSONFormat()
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
                    if (!(Enum.IsDefined(typeof(SSMParameterCode), code)))
                        throw new JSONFormatsException("SSM_Parameter_Code property of " + ModeCode + " packet is not valid.");
                    if (read_mode != FastReadModeCode && read_mode != SlowReadModeCOde)
                        throw new JSONFormatsException("read_mode of " + ModeCode +" packet is not valid (Should be SLOW or FAST).");
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

    public class SSMSLOWREADIntervalJSONFormat : JSONFormats
    {
        public const string ModeCode = "SSM_SLOWREAD_INTERVAL"; 
        public int interval;
        public SSMSLOWREADIntervalJSONFormat()
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
                    throw new JSONFormatsException("interval property of "+ ModeCode + " packet is less than 0.");
            }
        }
    }
}
