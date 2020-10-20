using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Service.FUELTripCalculator
{
    public enum FuelCalculationMethod
    {
        RPM_INJECTION_PW,
        MASS_AIR_FLOW,
        MASS_AIR_FLOW_AF,
        FUEL_RATE
    }
}
