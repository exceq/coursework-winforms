using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NeoAPI;
using Image = NeoAPI.Image;

namespace CourseworkWinforms
{
    public class BaumerCamera
    {
        public Cam Camera { get; }
        public CameraProperties CameraProperties { get; }

        public BaumerCamera(string cameraPropertiesXml, string identifier = "")
        {
            Camera = new Cam();

            Camera = Camera.Connect(identifier);

            // Подключение из примеров к neoApi
            // camera.Connect(); // connect to any camera on the host
            // camera.Connect("GigE");         // connect to a GigE camera on the host
            // camera.Connect("VCXU-23M");     // connect to a camera with the name VCXU-23M
            // camera.Connect("700004105902"); // connect to the camera with the specified serial number
            // camera.Connect("P10-2");        // connect to the camera on the specified USB port
            
            
            FeatureAccess f = new FeatureAccess(Camera);
            f.Width.Value = 1920;
            f.Height.Value = 1080;
            f.ExposureTime.Value = 331042.0;
            f.PixelFormat.Value = PixelFormat.BayerRG8; //Цветовое пространство
            f.BinningHorizontal.Value = 1; //уменьшение разрешения в 4 раза 
            f.BinningVertical.Value = 1; //уменьшение разрешения в 4 раза
            f.Gain.Value = 17.64;

            Camera.ImageBufferCount = 10; // set the size of the buffer queue to 10
            Camera.ImageBufferCycleCount = 1; // sets the cycle count to 1 

            CameraProperties = new XmlReader().GetCameraProperties(cameraPropertiesXml);
        }

        public static Bitmap ConvertNeoImageToBitmap(Image image)
        {
            // Возможно конвертация в bitmap неправильная
            Mat img = new Mat((int)image.Height, (int)image.Width, DepthType.Cv8U, 3,
                image.ImageData, 1);
            return img.ToImage<Bgr, byte>().ToBitmap();
        }
    }
}