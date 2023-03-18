using System;
using System.IO.Ports;
using System.Net;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SerialPortWrapper
{
    public class SerialPortWrapperFactory
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;
        public SerialPortWrapperFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<SerialPortWrapperFactory>();
        }

        public ISerialPortWrapper Create(string portname, int baudRate, Parity parity)
        {
            if(portname.StartsWith("tcp:"))
            {
                this.logger.LogInformation("TCP SerialPortWrapper is called. Baudrate and parity setting will be ignnored. Please set these settings in remote serial gateway.");
                var addressStr = portname.Replace("tcp:","").Split(":");
                if(addressStr.Length != 2)
                    throw new ArgumentException("TCP address string does not conatins address and port.");
                
                string hostName = addressStr[0];
                int portNo;
                if(!int.TryParse(addressStr[1], out portNo))
                    throw new ArgumentException("TCP port number is not the number");
                
                IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
                return new TCPClientCommunicator(new IPEndPoint(ipAddresses[0], portNo), loggerFactory);
            }
            else 
            {
                return new SerialPortCommunicator(new SerialPort(portname, baudRate, parity), loggerFactory);
            }
        }
    }
}