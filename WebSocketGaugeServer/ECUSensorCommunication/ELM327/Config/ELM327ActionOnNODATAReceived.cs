namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Config
{
    public enum ELM327ActionOnNODATAReceived
    {
        Ignore,
        Log,
        AddPIDToBlackList,
        ThrowException
    }
}