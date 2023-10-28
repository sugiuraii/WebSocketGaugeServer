using System;
using System.Collections.Generic;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Utils
{
    public class AvailablePIDMessageDecoder
    {
        public List<byte> parse(byte pid_offset, byte[] encoded_message)
        {
            var pidList = new List<byte>();
            if (encoded_message.Length != 4)
                throw new ArgumentException("Length of encoded_message is not 4. encoded_message = " + BitConverter.ToString(encoded_message));
            for (int byteIndex = 0; byteIndex < 4; byteIndex++)
            {
                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {
                    byte targetByte = encoded_message[byteIndex];
                    byte targetPID = (byte)(pid_offset + byteIndex * 8 + bitIndex + 1);
                    byte mask = (byte)(0b10000000 >> bitIndex); // bitIndex is orderd from highest
                    if (((targetByte & mask) & 0xFF) > 0)
                        pidList.Add(targetPID);
                }
            }
            return pidList;
        }
    }
}
