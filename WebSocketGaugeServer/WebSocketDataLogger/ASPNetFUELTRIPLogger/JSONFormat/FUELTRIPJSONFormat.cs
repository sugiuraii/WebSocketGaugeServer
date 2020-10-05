using DefiSSMCOM.WebSocket.JSON;

namespace FUELTRIP_Logger
{
    public class FUELTRIPJSONFormat : JSONFormat
    {
        public double moment_gasmilage;
        public double total_gas;
        public double total_trip;
        public double total_gasmilage;
        public const string ModeCode = "MOMENT_FUELTRIP";

        public FUELTRIPJSONFormat()
        {
            mode = ModeCode;
        }

        public override void Validate()
        {
            if (mode != ModeCode)
            {
                throw new JSONFormatsException("mode property of " + this.GetType().ToString() + " packet is not valid.");
            }
        }
    }

    public class SectResetJSONFormat : JSONFormat
    {
        public const string ModeCode = "SECTRESET";
        public override void Validate()
        {
            if (mode != ModeCode)
            {
                throw new JSONFormatsException("mode property of " + this.GetType().ToString() + " packet is not valid.");
            }
        }
    }

    public class SectFUELTRIPJSONFormat : JSONFormat
    {
        public long sect_span;
        public double[] sect_trip;
        public double[] sect_gas;
        public double[] sect_gasmilage;
        public const string ModeCode = "SECT_FUELTRIP";

        public SectFUELTRIPJSONFormat()
        {
            mode = ModeCode;
        }

        public override void Validate()
        {
            if (mode != ModeCode)
            {
                throw new JSONFormatsException("mode property of " + this.GetType().ToString() + " packet is not valid.");
            }
        }
    }

    public class SectSpanJSONFormat : JSONFormat
    {
        public int sect_span;
        public const string ModeCode = "SECT_SPAN";

        public SectSpanJSONFormat()
        {
            mode = ModeCode;
        }

        public override void Validate()
        {
            if (mode != ModeCode)
            {
                throw new JSONFormatsException("mode property of " + this.GetType().ToString() + " packet is not valid.");
            }
        }
    }

    public class SectStoreMaxJSONFormat : JSONFormat
    {
        public int storemax;
        public const string ModeCode = "SECT_STOREMAX";

        public SectStoreMaxJSONFormat()
        {
            mode = ModeCode;
        }

        public override void Validate()
        {
            if (mode != ModeCode)
            {
                throw new JSONFormatsException("mode property of " + this.GetType().ToString() + " packet is not valid.");
            }
        }
    }
}
