using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi
{
    public interface IDefiCOM
    {
        event EventHandler DefiPacketReceived;
        double get_value(DefiParameterCode code);
        UInt32 get_raw_value(DefiParameterCode code);
        string get_unit(DefiParameterCode code);
    }
}