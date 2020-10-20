using SZ2.WebSocketGaugeServer.ECUSensorCommunication.AssettoCorsaSHM;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.AssettoCorsaSharedMemoryWebSocketServer.JSONFormat
{
    public class AssettoCorsaPhysicsSendJSONFormat : WSSendJSONFormat<AssettoCorsaSHMPhysicsParameterCode>
    {
        public const string ModeCode = "ACSHM_PHYS_WS_SEND";
        public AssettoCorsaPhysicsSendJSONFormat()
            : base(ModeCode)
        {
        }
    }

    public class AssettoCorsaGraphicsSendJSONFormat : WSSendJSONFormat<AssettoCorsaSHMGraphicsParameterCode>
    {
        public const string ModeCode = "ACSHM_GRPH_WS_SEND";
        public AssettoCorsaGraphicsSendJSONFormat()
            : base(ModeCode)
        {
        }
    }

    public class AssettoCorsaStaticInfoSendJSONFormat : WSSendJSONFormat<AssettoCorsaSHMStaticInfoParameterCode>
    {
        public const string ModeCode = "ACSHM_STATIC_WS_SEND";
        public AssettoCorsaStaticInfoSendJSONFormat()
            : base(ModeCode)
        {
        }
    }

    public class AssettoCorsaPhysicsWSIntervalJSONFormat : WSIntervalJSONFormat
    {
        public const string ModeCode = "ACSHM_PHYS_WS_INTERVAL";
        public AssettoCorsaPhysicsWSIntervalJSONFormat()
            : base(ModeCode)
        {
        }
    }

    public class AssettoCorsaGraphicsWSIntervalJSONFormat : WSIntervalJSONFormat
    {
        public const string ModeCode = "ACSHM_GRPH_WS_INTERVAL";
        public AssettoCorsaGraphicsWSIntervalJSONFormat()
            : base(ModeCode)
        {
        }
    }

    public class AssettoCorsaStaticInfoWSIntervalJSONFormat : WSIntervalJSONFormat
    {
        public const string ModeCode = "ACSHM_STATIC_WS_INTERVAL";
        public AssettoCorsaStaticInfoWSIntervalJSONFormat()
            : base(ModeCode)
        {
        }
    }
}
