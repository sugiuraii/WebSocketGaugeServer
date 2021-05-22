using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public interface IELM327COM : IBackgroundCommunicate
    {
        event EventHandler<ELM327DataReceivedEventArgs> ELM327DataReceived;
        double get_value(OBDIIParameterCode code);
        UInt32 get_raw_value(OBDIIParameterCode code);
        string get_unit(OBDIIParameterCode code);
        bool get_slowread_flag(OBDIIParameterCode code);
        bool get_fastread_flag(OBDIIParameterCode code);
        void set_slowread_flag(OBDIIParameterCode code, bool flag);
        void set_slowread_flag(OBDIIParameterCode code, bool flag, bool quiet);
        void set_fastread_flag(OBDIIParameterCode code, bool flag);
        void set_fastread_flag(OBDIIParameterCode code, bool flag, bool quiet);
        void set_all_disable();
        void set_all_disable(bool quiet);
        int SlowReadInterval {get; set;}
    }
}