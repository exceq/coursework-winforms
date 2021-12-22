using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using server_connect;

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

            var positionData = new PositionData();
            
            if (CheckData())
            {
                bytes = networkStream.Read(data, 0, data.Length);
            }

            do
            {
                string message = Encoding.ASCII.GetString(data);

                Console.WriteLine($" * Считано байт: {bytes};");

                Console.WriteLine($"{GetCIndex(data)} - индекс;");

                //if (GetCIndex(data) != -1)
                //{
                //    PositionData.cFrame = GetFrame(data);
                //    data = data.Skip(45).ToArray();
                //}
                if (data[0] == 'C' && data[1] == 'a')
                {
                    if (message.IndexOf(ConfiguratePositionData.CAMERA_POSITION_MSG) + ConfiguratePositionData.CAMERA_STRING_OFFSET + ConfiguratePositionData.AXIS_NUM * ConfiguratePositionData.BYTES_PER_KRC_REAL < data.Length)
                    {
                        positionData.CameraPosition = GetFrame(data);
                        data = data.Skip(45).ToArray();
                    }
                }
                else if (data[0] == 'F' && data[1] == 'l')
                {
                    if (message.IndexOf(ConfiguratePositionData.FLANGE_POS_MSG) + ConfiguratePositionData.FLANGE_STRING_OFFSET + ConfiguratePositionData.AXIS_NUM * ConfiguratePositionData.BYTES_PER_KRC_REAL < message.Length)
                    {
                        positionData.FlangePosition = GetFrame(data);
                        data = data.Skip(34).ToArray();
                    }
                }
                else if (data[0] == 'R' && data[1] == 'a')
                {
                    if (message.IndexOf(ConfiguratePositionData.RANGE_FINDER_DISTANCE_MSG) + ConfiguratePositionData.RANGE_FINDER_DISTANCE_MSG.Length + ConfiguratePositionData.BYTES_PER_KRC_REAL < message.Length)
                    {
                        positionData.RangeFinderDistanceIntern = BitConverter.ToSingle(data.Skip(ConfiguratePositionData.RANGE_FINDER_DISTANCE_MSG.Length).Take(ConfiguratePositionData.BYTES_PER_KRC_REAL).ToArray(), 0) * 500 + 125;
                        data = data.Skip(34).ToArray();
                    }
                }
                else
                {
                    break;
                }

                Console.WriteLine($" * Осталось байт: {data.Length};");

            } while (CheckData() && data.Length != 0 && data[0] != 0);

            return positionData;
        }
        
        public PositionData Read1()
        {
            int bytes = 0;
            byte[] data = new byte[1000];
            var positionData = new PositionData();

            if (CheckData())
            {
                bytes = networkStream.Read(data, 0, data.Length);
            }

            for (int i = 0; i < data.Length; i++)
            {

                Console.WriteLine((char)data[i]);

                if (data[i] == 'C' && data[i + 1] == 'a')
                {
                    positionData.CameraPosition = GetFrame(data);
                }
                else if (data[i] == 'F' && data[i + 1] == 'l')
                {
                    positionData.FlangePosition = GetFrame(data);
                }
                else if (data[i] == 'R' && data[i + 1] == 'a')
                {
                    positionData.RangeFinderDistanceIntern = BitConverter.ToSingle(data.Skip(ConfiguratePositionData.RANGE_FINDER_DISTANCE_MSG.Length).Take(ConfiguratePositionData.BYTES_PER_KRC_REAL).ToArray(), 0) * 500 + 125;
                }
            }

            return positionData;
        }

        // Блок вспомогательных методов;

        // Создание фрейма;
        private Frame GetFrame(byte[] data)
        {
            return new Frame
            (
                BitConverter.ToSingle(data.Skip(ConfiguratePositionData.CAMERA_X_OFFSET).Take(ConfiguratePositionData.BYTES_PER_KRC_REAL).ToArray(), 0),
                BitConverter.ToSingle(data.Skip(ConfiguratePositionData.CAMERA_Y_OFFSET).Take(ConfiguratePositionData.BYTES_PER_KRC_REAL).ToArray(), 0),
                BitConverter.ToSingle(data.Skip(ConfiguratePositionData.CAMERA_Z_OFFSET).Take(ConfiguratePositionData.BYTES_PER_KRC_REAL).ToArray(), 0),
                BitConverter.ToSingle(data.Skip(ConfiguratePositionData.CAMERA_AROUND_Z_OFFSET).Take(ConfiguratePositionData.BYTES_PER_KRC_REAL).ToArray(), 0),
                BitConverter.ToSingle(data.Skip(ConfiguratePositionData.CAMERA_AROUND_Y_OFFSET).Take(ConfiguratePositionData.BYTES_PER_KRC_REAL).ToArray(), 0),
                BitConverter.ToSingle(data.Skip(ConfiguratePositionData.CAMERA_AROUND_X_OFFSET).Take(ConfiguratePositionData.BYTES_PER_KRC_REAL).ToArray(), 0)
            );
        }
        // Получение индекса позиции камеры;
        private int GetCIndex(byte[] data)
        {
            int result = -1;

            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i] == 'C' && data[i + 1] == 'a')
                {
                    result = i;
                    break;
                }
            }

            return result;
        }
    }
}
