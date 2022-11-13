using System.IO.Ports;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SerialPortWrapper
{
    public class SerialPortCommunicator : ISerialPortWrapper
    {
        private readonly SerialPort serialPort;

        public SerialPortCommunicator(SerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        public int BaudRate { get => serialPort.BaudRate; set => serialPort.BaudRate = value; }

        public bool IsOpen => serialPort.IsOpen;

        public int BytesToRead => serialPort.BytesToRead;

        public int ReadTimeOut { get => serialPort.ReadTimeout; set => serialPort.ReadTimeout = value; }

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

        public void Read(byte[] buffer, int offset, int count)
        {
            serialPort.Read(buffer, offset, count);
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