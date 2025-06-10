namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327
{
    public record ELM327COMOption(
        string COMPortName,
        int Waitmsec,
        string Elm327ProtocolStr,
        int Elm327AdaptiveTimingMode,
        int Elm327Timeout,
        string Elm327HeaderBytes,
        string Elm327ReceiveAddress,
        int Elm327BatchQueryCount,
        bool SeparateBatchQueryToAvoidMultiFrameResponse,
        bool QueryOnlyAvilablePID
    );
}