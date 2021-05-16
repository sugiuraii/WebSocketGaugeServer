using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM
{
    public interface ISSMCOM
    {
        event EventHandler<SSMCOMDataReceivedEventArgs> SSMDataReceived;
        double get_value(SSMParameterCode code);
        UInt32 get_raw_value(SSMParameterCode code);
        bool get_switch(SSMSwitchCode code);
        string get_unit(SSMParameterCode code);
        bool get_slowread_flag(SSMParameterCode code);
        bool get_fastread_flag(SSMParameterCode code);
        void set_slowread_flag(SSMParameterCode code, bool flag);
        void set_slowread_flag(SSMParameterCode code, bool flag, bool quiet);
        void set_fastread_flag(SSMParameterCode code, bool flag);
        void set_fastread_flag(SSMParameterCode code, bool flag, bool quiet);
        void set_all_disable();
        void set_all_disable(bool quiet);
        void BackgroundCommunicateStart();
        void BackgroundCommunicateStop();
    }
}