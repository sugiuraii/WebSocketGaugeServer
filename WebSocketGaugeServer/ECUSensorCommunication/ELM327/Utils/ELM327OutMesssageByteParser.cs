using System;
using System.Collections.Generic;
using System.Globalization;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Utils
{
    public class ELM327OutMessageByteParser
    {
        public byte[] parse(string elm327outStr)
        {
            string filteredElm327outStr = elm327outStr.Replace("\n", "").Replace(" ", "");
            if (filteredElm327outStr.Contains(":")) // Multi frame message
            {
                string convertedMultilineStr = convertMultiLineMessage(filteredElm327outStr);
                return parseSingleLineToBytes(convertedMultilineStr);
            }
            else // Single frame message
                return parseSingleLineToBytes(filteredElm327outStr);
        }
        private byte[] parseSingleLineToBytes(string lineStr)
        {
            string lineStrFiltered = lineStr.Replace("\r", "");
            var readBytes = new List<byte>();
            for (int ofst = 0; ofst < lineStrFiltered.Length; ofst += 2)
            {
                readBytes.Add(Convert.ToByte(lineStrFiltered.Substring(ofst, 2), 16));
            }
            return readBytes.ToArray();
        }

        private string convertMultiLineMessage(string multiLineMsg)
        {
            var separatedLineMsg = multiLineMsg.Split("\r");
            int byteLength = int.Parse(separatedLineMsg[0], NumberStyles.HexNumber);
            string convertedStr = "";
            for (int line = 1; line < separatedLineMsg.Length; line++)
            {
                string lineStr = separatedLineMsg[line];
                if (lineStr.Length == 0)
                    continue;
                string contentStr = lineStr.Split(":")[1];
                convertedStr += contentStr;
            }
            // Trim str by number of return bytes
            return convertedStr.Substring(0, byteLength * 2);
        }
    }
}