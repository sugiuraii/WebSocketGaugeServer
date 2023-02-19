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
            if(elm327outStr.Contains(":")) // Multi frame message
            {
                string convertedMultilineStr = convertMultiLineMessage(elm327outStr);
                return parseSingleLine(convertedMultilineStr);
            }
            else // Single frame message
                return parseSingleLine(elm327outStr);
        }

        private ELM327OutMessageParseResult parseSingleLine(string elm327outStr)
        {
            byte modeCode = Byte.Parse(elm327outStr.Substring(0,2));
            var valueStrMap = new Dictionary<OBDIIParameterCode, string>();

            int ofst = 2;
            while(ofst < elm327outStr.Length)
            {
                byte pid = Byte.Parse(elm327outStr.Substring(ofst, 2));
                ofst += 2;
                var code = pidToParamCodeReverseMap[pid];
                int byteLength = content_table[code].ReturnByteLength;
                string valueStr = elm327outStr.Substring(ofst, byteLength*2);
                ofst += byteLength*2;
                valueStrMap.Add(code, valueStr);
            }

            return new ELM327OutMessageParseResult(modeCode, valueStrMap);
        }

        private string convertMultiLineMessage(string multiLineMsg)
        {
            var separatedLineMsg = multiLineMsg.Split("\r");
            int byteLength = int.Parse(separatedLineMsg[0], NumberStyles.HexNumber);
            string convertedStr = "";
            for(int line = 1; line < separatedLineMsg.Length; line++)
            {
                string lineStr = separatedLineMsg[line];
                string contentStr = lineStr.Split(":")[1];
                convertedStr += contentStr;
            }
            // Trim str by number of return bytes
            return convertedStr.Substring(0, byteLength*2);
        }
    }
}