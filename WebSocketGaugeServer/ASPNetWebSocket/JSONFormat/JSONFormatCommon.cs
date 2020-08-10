using System;

namespace DefiSSMCOM.WebSocket.JSON
{
    public class WSSendJSONFormat<parameterCodeType> : JSONFormat 
        where parameterCodeType:struct
    {
        public string code;
        public bool flag;
        private readonly string ModeCode;

        public WSSendJSONFormat(string modecode)
        {
            ModeCode = modecode;
            mode = ModeCode;
        }

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
                    if (!(Enum.IsDefined(typeof(parameterCodeType), code)))
                        throw new JSONFormatsException(typeof(parameterCodeType).ToString() +  " property of " + ModeCode + " packet is not valid.");
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

    public class WSIntervalJSONFormat : JSONFormat
    {
        public int interval;
        private readonly string ModeCode;

        public WSIntervalJSONFormat(string modecode)
        {
            ModeCode = modecode;
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

    public class SlowFastCOMReadJSONFormat<parameterCodeType> : JSONFormat
        where parameterCodeType : struct
    {
        private readonly string ModeCode;
        public const string FastReadModeCode = "FAST";
        public const string SlowReadModeCode = "SLOW";
        public SlowFastCOMReadJSONFormat(string modecode)
        {
            ModeCode = modecode;
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
                    if (!(Enum.IsDefined(typeof(parameterCodeType), code)))
                        throw new JSONFormatsException(typeof(parameterCodeType).ToString() + " property of " + ModeCode + " packet is not valid.");
                    if (read_mode != FastReadModeCode && read_mode != SlowReadModeCode)
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
}
