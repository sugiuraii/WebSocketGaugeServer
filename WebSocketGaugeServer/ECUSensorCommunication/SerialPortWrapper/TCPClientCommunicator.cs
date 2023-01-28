using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace SZ2.WebSocketGaugeServer.ECUSensorCommunication.SerialPortWrapper
{
    public class TCPClientCommunicator : ISerialPortWrapper
    {
        private readonly TcpClient tcpClient;
        private readonly IPEndPoint remoteEP;
        private NetworkStream stream = null;
        private StreamWriter writer = null;
        private StreamReader reader = null;

        private readonly string NewLine = "\n";

        private int readTimeOut = 500;
        private readonly ILogger logger;
        public TCPClientCommunicator(string hostname, int port, ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger<TCPClientCommunicator>();
            this.tcpClient = new TcpClient(hostname, port);
        }

        public bool IsOpen => tcpClient.Connected;

        public int BytesToRead => tcpClient.Available;

        public int ReadTimeout { get => this.readTimeOut; set => this.readTimeOut = value; }
        public int BaudRate { get => 0; set { } }

        public void Close()
        {
            this.writer.Dispose();
            this.reader.Dispose();
            this.stream.Dispose();
            tcpClient.Close();
            this.writer = null;
            this.reader = null;
            this.stream = null;
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
            this.stream = this.tcpClient.GetStream();
            this.stream.ReadTimeout = readTimeOut;

            this.writer = new StreamWriter(this.stream);
            this.reader = new StreamReader(this.stream);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public int ReadByte()
        {
            return stream.ReadByte();
        }

        public string ReadExisting()
        {
            var inCharList = new List<char>(BytesToRead);
            while (stream.DataAvailable)
            {
                int indat = reader.Read();
                if (indat != -1)
                    inCharList.Add((char)indat);
                else
                {
                    logger.LogWarning("End of stream is detected in network stream.");
                    break;
                }
            }
            return new String(inCharList.ToArray());
        }

        public string ReadLine()
        {
            return ReadTo(this.NewLine);
        }

        public string ReadTo(string str)
        {
            var inCharList = new List<char>(BytesToRead);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                int indat = reader.Read();
                if (indat != -1)
                {
                    inCharList.Add((char)indat);
                    string lineStr = new string(inCharList.ToArray());
                    if (str.Equals(lineStr.Substring(lineStr.Length - str.Length)))
                        return lineStr;
                }
                else
                {
                    logger.LogWarning("End of stream is detected in network stream.");
                    return new string(inCharList.ToArray());
                }

                if (stopwatch.ElapsedMilliseconds > readTimeOut)
                    throw new TimeoutException();
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
            // No need to use stream.Flush(), since buffer is not implemented in NetworkStream.
            // Actually, NetworkStream.Flush() do nothing.
            // stream.Flush(); 
        }

        public void Write(string buffer)
        {
            writer.Write(buffer);
            writer.Flush(); // Fource flush write buffer of StreamWriter
        }
    }
}