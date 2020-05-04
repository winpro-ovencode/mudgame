using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MudGameTuto
{
    public class Session
    {
        private Socket _socket;
        private IPEndPoint _endPoint;
        private byte[] inputBuffer = new byte[8192];
        private byte[] outputBuffer = new byte[8192];

        public object tag;

        public EventHandler<SocketAsyncEventArgs> OnMessage;
        public EventHandler<SocketAsyncEventArgs> OnClose;

        public void Open(Socket socket)
        {
            _socket = socket;
        }

        public void ReadStart()
        {
            while(true)
            {
                SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                arg.Completed += OnReceive;
                arg.SetBuffer(inputBuffer, 0, inputBuffer.Length);
                if(!_socket.ReceiveAsync(arg))
                {
                    ProcessRecv(arg);
                    continue;
                }

                break;
            }
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            //TIME_WAIT 발생시키지 않고 세션이 끊어졌다.
            //gracefully close
            if(e.BytesTransferred == 0)
            {
                OnClose(this, e);
                return;
            }
            
            ProcessRecv(e);
            ReadStart();
        }

        public void ProcessRecv(SocketAsyncEventArgs e)
        {
            Console.WriteLine("{0} bytes 전송됨", e.BytesTransferred);
            OnMessage(this, e);
        }

        public void Send(string message)
        {
            var data = UTF8Encoding.UTF8.GetBytes(message);
            try
            {
                _socket.Send(data, data.Length, SocketFlags.None);
            } catch(Exception)
            {

            }
        }
    }

    public class Server
    {
        private Socket _socket;
        private IPEndPoint _endPoint;
        private List<Session> _sessions;

        public Action<Session> SessionClose;
        public Action<Session> SessionOpen;
        public Action<Session, string> SessionPacket;

        public Server()
        {
            _sessions = new List<Session>();
        }

        public void Listen(int backlog)
        {
            _endPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 4444);
            _socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(_endPoint);
            _socket.Listen(backlog);
            StartAccept();
            Console.WriteLine("접속 대기중...");
        }

        private void StartAccept()
        {
            while(true)
            {
                SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                arg.Completed += OnAccept;
                if(!_socket.AcceptAsync(arg))
                {
                    ProcessAccept(arg);
                    continue;
                }

                break;
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Session session = new Session();
            session.Open(e.AcceptSocket);
            session.OnMessage += ClientOnMessage;
            session.OnClose += ClientOnClose;
            session.ReadStart();
            SessionOpen(session);
            _sessions.Add(session);
        }

        private void OnAccept(object sender, SocketAsyncEventArgs e)
        { // WorkerThread
            var client = e.AcceptSocket;
            Console.WriteLine("접속됨 {0}", client.RemoteEndPoint.ToString());
            ProcessAccept(e);
            StartAccept();
            Console.WriteLine("접속 완료");
        }

        private void ClientOnClose(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("SessionClose");
            _sessions.Remove((Session)sender);
            SessionClose((Session)sender);
        }

        private void ClientOnMessage(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                Console.WriteLine("{0}", _sessions.Count);
                var msg = UTF8Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
                Console.WriteLine(msg);

                if(SessionPacket != null)
                    SessionPacket((Session)sender, msg);

            } catch(Exception)
            {

            }
        }

        internal void Broadcast(string arg)
        {
            foreach(var s in _sessions)
            {
                s.Send(arg);
            }
        }
    }

    class Adapter
    {
        Server owner;
        public Adapter(Server server)
        {
            owner = server;
            server.SessionOpen = SessionOpen;
            server.SessionClose = SessionClose;
            server.SessionPacket = SessionPacket;
        }

        private void SessionPacket(Session arg1, string arg2)
        {
            IPrompt prompt = (IPrompt) arg1.tag;
            prompt.Dispatch(arg1, arg2);
        }

        private void SessionClose(Session obj)
        {
        }

        protected void SessionOpen(Session s)
        {
            Console.WriteLine("SessionOpen");
            var prompt = new AuthPrompt(owner);
            s.tag = prompt;
            s.Send("계정을 입력하세요. : ");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Server server = new Server();
            Adapter adapter = new Adapter(server);
            server.Listen(5);
            Console.Read();
        }
    }
}