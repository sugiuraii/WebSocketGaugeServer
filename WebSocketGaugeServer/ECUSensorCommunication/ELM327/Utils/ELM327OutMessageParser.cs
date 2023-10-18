using System;
using System.Collections.Generic;

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
            var byteParser = new ELM327OutMessageByteParser();
            var parsedReturnBytes = byteParser.parse(elm327outStr);
            return convertBytesToResultSet(parsedReturnBytes);
        }

        private ELM327OutMessageParseResult convertBytesToResultSet(byte[] readBytes)
        {
            if (readBytes.Length < 0)
                throw new ArgumentException("Length of readBytes is 0. Cannot process to ELM327OutMessageParseResult");
            string lineStr = BitConverter.ToString(readBytes);
            byte modeCode = Convert.ToByte(lineStr.Substring(0, 2), 16);
            var valueStrMap = new Dictionary<OBDIIParameterCode, string>();

            int ofst = 2;
            while (ofst < lineStr.Length)
            {
                byte pid = Convert.ToByte(lineStr.Substring(ofst, 2), 16);
                ofst += 2;
                var code = pidToParamCodeReverseMap[pid];
                int byteLength = content_table[code].ReturnByteLength;
                string valueStr = lineStr.Substring(ofst, byteLength * 2);
                ofst += byteLength * 2;
                valueStrMap.Add(code, valueStr);
            }
            return new ELM327OutMessageParseResult(modeCode, valueStrMap);
        }
    }
}