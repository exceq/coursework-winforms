using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace client_connect
{
    public class Client
    {
        public int PortConnect { get; set; }
        public string AddressConnect { get; set; }
        public TcpClient TcpClient { get; }
        public bool Connected => TcpClient.Connected;

        private int port;
        private TcpListener listener;
        private NetworkStream stream;

        // Блок конструкторов;
        public Client(int portConnect, string addressConnect)
        {
            this.PortConnect = portConnect;
            this.AddressConnect = addressConnect;
            this.TcpClient = new TcpClient();

            Connect();
        }

        public Client(int port, int portConnect, string addressConnect)
        {
            this.port = port;
            this.PortConnect = portConnect;
            this.AddressConnect = addressConnect;
            this.TcpClient = new TcpClient();

            Listen();
            Connect();
        }
        

        // Блок отправки;
        public void Send(string message)
        {
            try
            {
                string textException = "Error! Сообщение не может быть отправлено.";
                byte[] bytes = Encoding.UTF8.GetBytes(message);

                if (!Connected)
                {
                    throw new Exception($"{textException} Соединение не установлено.");
                }

                stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }

        // Блок повторного соединения;
        public void Reconnect()
        {
            Connect();
        }

        public void Reconnect(int portConnect)
        {
            int portConnectLast = this.PortConnect;
            this.PortConnect = portConnect;

            Reconnect();

            if (!Connected)
            {
                this.PortConnect = portConnectLast;
            }
        }

        public void Reconnect(string addressConnect)
        {
            string addressConnectLast = addressConnect;
            this.AddressConnect = addressConnect;

            Reconnect();

            if (!Connected)
            {
                this.AddressConnect = addressConnectLast;
            }
        }

        public void Reconnect(string addressConnect, int portConnect)
        {
            int portConnectLast = this.PortConnect;
            string addressConnectLast = this.AddressConnect;

            this.PortConnect = portConnect;
            this.AddressConnect = addressConnect;

            Connect();

            if (!Connected)
            {
                this.PortConnect = portConnectLast;
                this.AddressConnect = addressConnectLast;
            }
        }


        // Блок установки соединения;
        private void Connect()
        {
            try
            {
                this.TcpClient.Connect(new IPEndPoint(IPAddress.Parse(this.AddressConnect), this.PortConnect));
                this.stream = TcpClient.GetStream();
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                Console.WriteLine("Выберите дальнейшие действия:");
                Console.WriteLine(" 0. Оставить без подключения;");
                Console.WriteLine(" 1. Переподключиться;");
                Console.WriteLine(" 2. Переподключиться с новыми параметрами;");
                Console.WriteLine(" -. Приостановить работу программы;");

                switch (Console.ReadKey().KeyChar)
                {
                    case '0':
                        break;
                    case '1':
                        Console.Clear();
                        Connect();
                        break;
                    case '2':
                        Console.Clear();
                        Console.WriteLine(" Укажите новый IP:");
                        string addressConnects = Console.ReadLine();
                        Console.WriteLine(" Укажите новый Порт:");
                        bool success = false;
                        do
                        {
                            success = int.TryParse(Console.ReadLine(), out int port);
                            PortConnect = port;
                        } while (success);

                        Reconnect();
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine($"Порт: {this.PortConnect};");
                        Console.WriteLine($"IP  : {this.AddressConnect};");
                        Environment.Exit(0);
                        break;
                }
            }
        }

        // Блок прослушивания соединений;
        private void Listen()
        {
            this.listener = new TcpListener(IPAddress.Parse("127.0.0.1"), this.port);
            this.listener.Start();
        }

        // Принятие клиента;
        public void Accept()
        {
            Thread thread = new Thread(Process);
            thread.Start();
        }

        // Процесс нового соединения;
        private void Process()
        {
            byte[] buffer = new byte[256];
            
            do
            {
            
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    builder.Append(Encoding.Unicode.GetString(buffer, 0, bytes));
                } while (stream.DataAvailable);
            
                string message = builder.ToString();
            
                Console.WriteLine(message);
            
            } while (true);
        }
    }
}