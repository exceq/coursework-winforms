using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CourseworkWinforms
{
    class Client
    {
        // Поле порта;
        public int port { get; set; } = 54600;
        // Поле адреса;
        public string address { get; set; } = "192.168.7.107";
        // Поле кодировки;
        public Encoding encoding { get; set; } = Encoding.ASCII;
        // Поле соединения;
        private TcpClient tcpClient = new TcpClient();
        // Поле потока ввода;
        private NetworkStream networkStream;

        // Блок конструкторов;
        public Client()
        {
            Connect();
        }
        public Client(string address, int port)
        {
            this.port = port;
            this.address = address;

            Connect();
        }

        // Блок создания подключений;
        public void Connect()
        {
            try
            {
                // Создаем соединение;
                tcpClient.Connect(IPAddress.Parse(address), port);
                // Получаем поток данных соединения;
                networkStream = tcpClient.GetStream();
                //RefreshData();
            }
            catch (Exception error)
            {
                throw new Exception($" * Подключение по порту: {port} и адресу: {address} не удалось;");
            }

        }
        public void Connect(int port)
        {
            // Запоминаем старый порт;
            int portLast = this.port;
            // Задаем новый порт;
            this.port = port;
            // Вызываем подключение;
            try
            {
                Connect();
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($" осуществляется переподключение с исходным портом: {portLast};");
                Console.ResetColor();

                // Возвращаем исходный порт;
                this.port = portLast;
                // Переподключение;
                Connect();

                if (CheckConnect())
                {
                    Console.WriteLine(" * переподключение прошло успешно; ");
                }

            }
        }
        public void Connect(string address)
        {
            // Запоминаем старый адрес;
            string addressLast = this.address;
            // Задаем новый адрес;
            this.address = address;
            // Вызываем подключение;
            try
            {
                Connect();
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($" осуществляется переподключение с исходным адресом: {address};");
                Console.ResetColor();

                // Возвращаем исходный адрес;
                this.address = addressLast;
                // Переподключение;
                Connect();

                if (CheckConnect())
                {
                    Console.WriteLine(" * переподключение прошло успешно; ");
                }
            }
        }

        //private void RefreshData()
        //{
        //    Task.Run(() =>
        //    {
        //        while (true)
        //            if (CheckConnect())
        //                Read();
        //    });
        //}

        public void Connect(string address, int port)
        {
            // Запоминаем старый порт;
            int portLast = this.port;
            // Запоминаем старый адрес;
            string addressLast = this.address;
            // Задаем новый порт;
            this.port = port;
            // Задаем новый адрес;
            this.address = address;
            // Вызываем подлючение;
            try
            {
                Connect();
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($" осуществляется переподключение с исходными портом: {port} и адресом: {address};");
                Console.ResetColor();

                // Возвращаем исходный порт;
                this.port = portLast;
                // Возвращаем исходный адрес;
                this.address = addressLast;
                // Переподключение;
                Connect();

                if (CheckConnect())
                {
                    Console.WriteLine(" * переподключение прошло успешно; ");
                }
            }


        }

        // Проверка подключения;
        public bool CheckConnect()
        {
            return tcpClient.Connected;
        }

        // Проверка наличия данных в потоке;
        public bool CheckData()
        {
            return networkStream.DataAvailable;
        }

        // Отправка данных;
        public void Send()
        {

        }

        // Закрытие подключения;
        public void Close()
        {
            tcpClient.Close();
        }

        // Считывание данных;
        public PositionData Read()
        {
            int bytes = 0;
            byte[] data = new byte[1000];

            if (CheckData())
            {
                bytes = networkStream.Read(data, 0, data.Length);
            }
            PositionData positionData = new PositionData();
            positionData.CameraPosition = ParseFrameFromBytes(FindBytes(data, "CameraPosition"));
            positionData.FlangePosition = ParseFrameFromBytes(FindBytes(data, "FlangePosition"));
            positionData.RangeFinderDistanceIntern = ParseDistance(FindBytes(data, "RangefinderDistance"));

            return positionData;
        }

        public float ReadDistance()
        {
            byte[] data = new byte[1000];

            if (CheckData())
                networkStream.Read(data, 0, data.Length);

            return ParseDistance(FindBytes(data, "RangefinderDistance"));
        }

        // Блок вспомогательных методов;
        private byte[] FindBytes(byte[] bytes, string message)
        {
            var data = encoding.GetString(bytes);
            var index = data.LastIndexOf(message);
            if (index == -1)
                return null;
            return bytes.Skip(index + message.Length).Take(40).ToArray();
            //data = data.Substring(index).Replace(message, "");

            //return Encoding.ASCII.GetBytes(data);
        }

        private Frame ParseFrameFromBytes(byte[] bytes)
        {
            if (bytes == null)
                return null;
            float[] xyzabc = new float[6];
            for (int i = 0; i < 6; i++)
            {
                xyzabc[i] = floatConversion(bytes.Skip(i * 4).Take(4).ToArray());
            }

            return new Frame(xyzabc[0], xyzabc[1], xyzabc[2], xyzabc[3], xyzabc[4], xyzabc[5]);
        }

        private float ParseDistance(byte[] bytes)
        {
            if (bytes == null)
                return -1;
            return floatConversion(bytes) * 500 + 125;
        }

        public float floatConversion(byte[] bytes)
        {
            //if (BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(bytes); // Convert big endian to little endian
            //}
            float myFloat = BitConverter.ToSingle(bytes, 0);
            return myFloat;
        }

        public void MoveCortesian(Frame f, int iTool = 1, int speed = 10, int iBase = 0)
        {
            int MoveType = 1;

            System.Console.WriteLine("xml start");
            string arryByte = "<RobotCommand>" +
                        "<MoveCartesian>" +
                            "<Frame X=\"" + f.X + "\" Y=\"" + f.Y + "\" Z=\"" + f.Z + "\" A=\"" + f.A + "\" B=\"" + f.B + "\" C=\"" + f.C + "\" />" +
                            "<Tool>" + iTool + "</Tool>" +
                            "<MoveType>" + MoveType + "</MoveType>" +
                            "<Speed>" + speed + "</Speed>" +
                            "<Base>" + iBase + "</Base>" +
                         "</MoveCartesian>" +
                        "</RobotCommand>";
            arryByte = arryByte.Replace(',', '.');

            System.Console.WriteLine(arryByte);
            System.Console.WriteLine("xml end");

            tcpClient.Client.Send(Encoding.ASCII.GetBytes(arryByte));
        }
    }
}
