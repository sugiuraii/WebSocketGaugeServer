using System;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SerialPortWrapper
{
    public interface ISerialPortWrapper : IDisposable
    {
        int BaudRate {get; set;}
        void Open();
        void Close();
        bool IsOpen {get;}
        void DiscardInBuffer();
        int BytesToRead{get;}
        void Read(byte[] buffer, int offset, int count);
        int ReadByte();
        string ReadTo(string str);
        string ReadLine();
        string ReadExisting();
        void Write(byte[] buffer, int offset, int count);
        void Write(string buffer);
        int ReadTimeOut {get; set;}
    }
}