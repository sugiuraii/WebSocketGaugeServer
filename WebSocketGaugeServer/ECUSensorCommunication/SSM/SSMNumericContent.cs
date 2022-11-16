using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM
{   
    public class SSMNumericContent : NumericContent
    {
        private byte[] _read_address;

        public SSMNumericContent(byte[] address, Func<UInt32, double> conversion_function, String unit)
        {
            _read_address = address;
            _conversion_function = conversion_function;
            _unit = unit;

            SlowReadEnable = false;
            FastReadEnable = false;
        }

        public int AddressLength
        {
            get
            {
                return _read_address.Length;
            }
        }

        public byte[] ReadAddress
        {
            get
            {
                return _read_address;
            }
        }

        public bool SlowReadEnable { get; set; }
        public bool FastReadEnable { get; set; }
    }
}
