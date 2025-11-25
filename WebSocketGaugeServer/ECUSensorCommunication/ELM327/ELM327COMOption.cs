namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public record ELM327COMOption(
        string COMPortName,
        int Waitmsec,
        string ELM327ProtocolStr,
        int ELM327AdaptiveTimingMode,
        int ELM327TimeOut,
        string ELM327HeaderBytes,
        string ELM327ReceiveAddress,
        int ELM327BatchQueryCount,
        bool SeparateBatchQueryToAvoidMultiFrameResponse,
        bool QueryOnlyAvailablePID
    );
}