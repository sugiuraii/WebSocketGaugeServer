using System;
using System.Collections.Generic;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM
{
    public class SSMCOMDataReceivedEventArgs : EventArgs
    {
	    public bool Slow_read_flag { get; set; }
	    public List<SSMParameterCode> Received_Parameter_Code { get; set; }
	}
}