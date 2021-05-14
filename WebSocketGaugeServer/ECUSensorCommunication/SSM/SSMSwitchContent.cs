using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM
{
    public class SSMSwitchContent
    {
        private SSMNumericContent _master_content;
        private int _bit_index;

        public SSMSwitchContent(SSMNumericContent master_content, int bit_index)
        {
            _master_content = master_content;
            _bit_index = bit_index;
        }

        public int BitIndex {get => _bit_index; }
        public SSMNumericContent MasterNumericContent { get => _master_content; }
        public bool Value
        {
            get
            {
                return (_master_content.RawValue & 0x01<<_bit_index)>>_bit_index == 1; 
            }
        }
    }
}
