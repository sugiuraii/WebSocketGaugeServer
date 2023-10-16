using System;
using System.Collections.Generic;
using System.Linq;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Utils
{
    public class ELM327PIDFilter
    {
        private readonly List<byte> AvailablePIDs;
        private readonly bool FilterByAvailablePIDs;
        private readonly List<byte> PIDBlackList;
        public ELM327PIDFilter(List<byte> AvailablePIDs, bool FilterByAvailablePIDs, List<byte> PIDBlackList)
        {
            this.AvailablePIDs = AvailablePIDs;
            this.FilterByAvailablePIDs = FilterByAvailablePIDs;
            this.PIDBlackList = PIDBlackList;
        }
        public bool test(byte pid)
        {
            bool result = true;
            if(this.FilterByAvailablePIDs)
                if(!AvailablePIDs.Contains(pid))
                    result = false;
            if(this.PIDBlackList.Contains(pid))
                    result = false;
            return result;
        }
        public List<OBDIIParameterCode> applyToList(List<OBDIIParameterCode> incodes, OBDIIContentTable table) {
            return incodes.Where(code => test(table[code].PID)).ToList();
        }
    }
}