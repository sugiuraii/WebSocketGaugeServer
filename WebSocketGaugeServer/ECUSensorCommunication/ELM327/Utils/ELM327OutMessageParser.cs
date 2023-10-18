using System;
using System.Collections.Generic;
using System.Globalization;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Utils
{
    public class ELM327OutMessageParser
    {
        private readonly OBDIIContentTable content_table;
        private Dictionary<byte, OBDIIParameterCode> pidToParamCodeReverseMap;
        public ELM327OutMessageParser(OBDIIContentTable content_table)
        {
            this.content_table = content_table;
            // Create PID byte => OBDIIParameterCode reversemap
            var revMapBuilder = new PIDToOBDIIParameterCodeReverseMapBuilder();
            this.pidToParamCodeReverseMap = revMapBuilder.create();
        } 

        public ELM327OutMessageParseResult parse(string elm327outStr) 
        {
            var parsedReturnBytes = parseToBytes(elm327outStr);
            return convertBytesToResultSet(parsedReturnBytes);
        }

        private ELM327OutMessageParseResult convertBytesToResultSet(byte[] readBytes)
        {
            if(readBytes.Length < 0)
                throw new ArgumentException("Length of readBytes is 0. Cannot process to ELM327OutMessageParseResult");
            string lineStr = BitConverter.ToString(readBytes);
            byte modeCode = Convert.ToByte(lineStr.Substring(0,2), 16);
            var valueStrMap = new Dictionary<OBDIIParameterCode, string>();

            int ofst = 2;
            while(ofst < lineStr.Length)
            {
                byte pid = Convert.ToByte(lineStr.Substring(ofst, 2), 16);
                ofst += 2;
                var code = pidToParamCodeReverseMap[pid];
                int byteLength = content_table[code].ReturnByteLength;
                string valueStr = lineStr.Substring(ofst, byteLength*2);
                ofst += byteLength*2;
                valueStrMap.Add(code, valueStr);
            }
            return new ELM327OutMessageParseResult(modeCode, valueStrMap);
        }

        public byte[] parseToBytes(string elm327outStr) 
        {
            string filteredElm327outStr = elm327outStr.Replace("\n","").Replace(" ","");
            if(filteredElm327outStr.Contains(":")) // Multi frame message
            {
                string convertedMultilineStr = convertMultiLineMessage(filteredElm327outStr);
                return parseSingleLineToBytes(convertedMultilineStr);
            }
            else // Single frame message
                return parseSingleLineToBytes(filteredElm327outStr);
        }

        private byte[] parseSingleLineToBytes(string lineStr){
            string lineStrFiltered = lineStr.Replace("\r","");
            var readBytes = new List<byte>();
            for(int ofst = 0; ofst < lineStrFiltered.Length; ofst+=2)
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
            for(int line = 1; line < separatedLineMsg.Length; line++)
            {
                string lineStr = separatedLineMsg[line];
                if(lineStr.Length == 0)
                    continue;
                string contentStr = lineStr.Split(":")[1];
                convertedStr += contentStr;
            }
            // Trim str by number of return bytes
            return convertedStr.Substring(0, byteLength*2);
        }
    }
}