using DefiSSMCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefiSSMCOM.WebSocket
{
    public abstract class COMPortBasedWebSocketServerCommon : WebSocketServerCommon
    {
        private COMCommon _com1;

        protected COMCommon com1
        {
            get { return _com1; }
            set 
            {
                _com1 = value;
                backgroundCommunicate = value;
            }
        }

        /// <summary>
        /// COM port name to communicate sensors.
        /// </summary>
        public string COMPortName
        {
            get
            {
                return _com1.PortName;
            }
            set
            {
                _com1.PortName = value;
            }
        }
    }
}
