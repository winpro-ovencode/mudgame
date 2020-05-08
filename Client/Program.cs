using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Client
    {
        private IPEndPoint _endpoint;
        private Socket _socket;
        private byte[] inputBuffer = new byte[1024];
        private byte[] outputBuffer = new byte[1024]; 

        public bool Connect(string address, int port)
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(address), port);
            _socket = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
             
            try
            {
                _socket.Connect(_endpoint);
                TryRecv();
                
            } catch(Exception e)
            {
                Console.WriteLine("접속 실패 : {0}", e.Message);
                return false;
            }
            return true;
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            if(!_socket.Connected)
                return;

            ProcessRecv(e);
            TryRecv();
        }

        private void TryRecv()
        {
            while (true)
            {
                SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                arg.Completed += OnReceive;
                arg.SetBuffer(inputBuffer, 0, inputBuffer.Length);
                if (!_socket.ReceiveAsync(arg))
                {
                    ProcessRecv(arg);
                    continue;
                }
                break;
            }
        }

        private void ProcessRecv(SocketAsyncEventArgs e)
        {
            string text = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
            Console.Write(text);
        }

        public void Write(string msg)
        {
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(msg);
            _socket.Send(buffer);
        }

        internal void Disconnect()
        {
            Console.WriteLine("서버와의 접속이 끊겼습니다.");
            if (_socket.Connected)
                _socket.Close();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                Console.Write("접속하실 IP를 입력하세요: ");
                string input = Console.ReadLine();
                if (input == "quit")
                    return;

                // 127.0.0.1:4444

                var inputs = input.Split(":");
                //inputs[0] IP
                //inputs[1] port;

                Client client = new Client();
                try
                {
                    if (!client.Connect(inputs[0], int.Parse(inputs[1])))
                        continue;

                    while (true)
                    {
                        input = Console.ReadLine();
                        if (input == "quit")
                        {
                            client.Disconnect();
                            return;
                        }
                        else if (input == "close")
                        {
                            client.Disconnect();
                            break;
                        }

                        client.Write(input);
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine("예외가 발생하였습니다: {0}", ex.Message);
                }
            }
        }
    }
}