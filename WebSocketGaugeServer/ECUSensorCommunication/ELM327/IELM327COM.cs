using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public interface IELM327COM : IBackgroundCommunicate
    {
        event EventHandler<ELM327DataReceivedEventArgs> ELM327DataReceived;
        double GetValue(OBDIIParameterCode code);
        uint GetRawValue(OBDIIParameterCode code);
        string GetUnit(OBDIIParameterCode code);
        bool GetSlowreadFlag(OBDIIParameterCode code);
        bool GetFastreadFlag(OBDIIParameterCode code);
        void SetSlowreadFlag(OBDIIParameterCode code, bool flag);
        void SetSlowreadFlag(OBDIIParameterCode code, bool flag, bool quiet);
        void SetFastreadFlag(OBDIIParameterCode code, bool flag);
        void SetFastreadFlag(OBDIIParameterCode code, bool flag, bool quiet);
        void SetAllDisable();
        void SetAllDisable(bool quiet);
        int SlowReadInterval {get; set;}
    }
}