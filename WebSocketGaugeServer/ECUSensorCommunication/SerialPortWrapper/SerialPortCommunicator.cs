using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SerialPortWrapper
{
    public class SerialPortCommunicator : ISerialPortWrapper
    {
        private readonly SerialPort serialPort;
        private readonly ILogger logger;
        public SerialPortCommunicator(SerialPort serialPort, ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger<SerialPortCommunicator>();
            this.serialPort = serialPort;
            // Register event of SerialPort error.
            serialPort.ErrorReceived += (sender, e) => 
            {
                this.logger.LogError("SerialPortError Event is invoked.");
                this.logger.LogError("Error type is  :" + e.EventType.ToString());
            };
        }

        public int BaudRate { get => serialPort.BaudRate; set => serialPort.BaudRate = value; }

        public bool IsOpen => serialPort.IsOpen;

        public int BytesToRead => serialPort.BytesToRead;

        public int ReadTimeout { get => serialPort.ReadTimeout; set => serialPort.ReadTimeout = value; }

        public void Close()
        {
            serialPort.Close();
        }

        public void DiscardInBuffer()
        {
            serialPort.DiscardInBuffer();
        }

        public void Dispose()
        {
            serialPort.Dispose();
        }

        public void Open()
        {
            serialPort.Open();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return serialPort.Read(buffer, offset, count);
        }

        public int ReadByte()
        {
            return serialPort.ReadByte();
        }

        public string ReadExisting()
        {
            return serialPort.ReadExisting();
        }

        public string ReadLine()
        {
            return serialPort.ReadLine();
        }

        public string ReadTo(string str)
        {
            return serialPort.ReadTo(str);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            serialPort.Write(buffer, offset, count);
        }

        public void Write(string buffer)
        {
            serialPort.Write(buffer);
        }
    }
}