using System;
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
            // #define 	CV_AUTO_STEP   0x7fffffff
            // #define 	CV_AUTOSTEP   0x7fffffff
            // это написано в доках к open cv https://docs.opencv.org/3.4/d2/df8/group__core__c.html
            // это число = 2147483647 = Int32.MaxValue
            // Так что можно попробовать варинант ниже, где step = Int32.MaxValue
            
            // Mat img = new Mat((int)image.Height, (int)image.Width, DepthType.Cv8U, 3,
            //     image.ImageData, Int32.MaxValue);
            
            //Можно ещё попробовать через Matrix - это обертка над Mat, и не требует step
            Matrix<int> img = new Matrix<int>((int)image.Height, (int)image.Width,
                image.ImageData);

            // ага блин тут хоть и не указываешь step, но вызывается все равно со значением = 0
            // public Matrix(int rows, int cols, IntPtr data)
            //     : this(rows, cols, data, 0)
            // {
            // }

            return img.Mat.ToImage<Bgr, byte>().ToBitmap();
        }
    }
}