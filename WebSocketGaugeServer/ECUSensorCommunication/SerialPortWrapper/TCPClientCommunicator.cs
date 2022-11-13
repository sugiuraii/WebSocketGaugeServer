using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SerialPortWrapper
{
    public class TCPClientCommunicator : ISerialPortWrapper
    {
        private readonly TcpClient tcpClient;
        private readonly IPEndPoint remoteEP;
        private readonly NetworkStream stream;
        private readonly StreamWriter writer;
        private readonly StreamReader reader;

        private readonly string NewLine = "\n";

        private int readTimeOut = 500;

        public TCPClientCommunicator(IPEndPoint remoteEP)
        {
            this.tcpClient = new TcpClient();
            this.remoteEP = remoteEP;
            this.stream = this.tcpClient.GetStream();

            this.writer = new StreamWriter(this.stream);
            this.reader = new StreamReader(this.stream);
        }

        public bool IsOpen => tcpClient.Connected;

        public int BytesToRead => tcpClient.Available;

        public int ReadTimeOut { get => this.readTimeOut; set => this.readTimeOut = value; }
        public int BaudRate { get => 0; set {} }

        public void Close()
        {
            tcpClient.Close();
        }

        public void DiscardInBuffer()
        {
            reader.DiscardBufferedData();
        }

        public void Dispose()
        {
            writer.Dispose();
            reader.Dispose();
            stream.Dispose();
            tcpClient.Dispose();
        }

        public void Open()
        {
            tcpClient.Connect(remoteEP);
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            stream.Read(buffer, offset, count);
        }

        public int ReadByte()
        {
            return stream.ReadByte();
        }

        public string ReadExisting()
        {
            return reader.ReadToEnd();
        }

        public string ReadLine()
        {
            return ReadTo(this.NewLine);
        }

        public string ReadTo(string str)
        {
            var inCharList = new List<char>(BytesToRead);
            int timeUsed = 0;
            while(true)
            {
                int currTime = Environment.TickCount;
                int indat = reader.Read();
                if(indat != -1)
                {
                    inCharList.Add((char)indat);                    
                    string lineStr = new string(inCharList.ToArray());
                    if(str.Equals(lineStr.Substring(lineStr.Length - str.Length)))
                        return lineStr;
                }
                timeUsed += Environment.TickCount - currTime;
                if(timeUsed > readTimeOut)
                    throw new TimeoutException();
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            tcpClient.GetStream().Write(buffer, offset, count);
        }

        public void Write(string buffer)
        {
            writer.Write(buffer);
        }
    }
}