namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Config
{
    public record ELM327PIDWhiteListConfig (
        ELM327PIDWhiteListMode Mode,
        byte[] CustomList
    );
}