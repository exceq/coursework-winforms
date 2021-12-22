using System.Collections.Generic;
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

        public BaumerCamera(CameraProperties cameraProperties, string identifier = "")
        {
            Camera = new Cam();
            Camera = Camera.Connect(identifier);
            
            Dictionary<string, Feature> featureList = Camera.GetFeatureList();

            Camera.f.PixelFormat.Value = PixelFormat.BGR8;
            Camera.f.Width.Value = 1920;
            Camera.f.Height.Value = 1280;
            Camera.f.ExposureAuto.Value = ExposureAuto.Off;
            Camera.f.ExposureTime.Value = 50000;
            
            //f.BinningHorizontal.Value = 1; //уменьшение разрешения в 4 раза ???
            //f.BinningVertical.Value = 1; //уменьшение разрешения в 4 раза
            //Camera.f.Gain.Value = 17.64;
            
            Camera.ImageBufferCount = 10; // set the size of the buffer queue to 10
            Camera.ImageBufferCycleCount = 1; // sets the cycle count to 1 

            CameraProperties = cameraProperties;
        }

        public static Bitmap ConvertNeoImageToBitmap(Image image, int step = 0)
        {
            Mat img = new Mat((int)image.Height, (int)image.Width, DepthType.Cv8U, 3,
                image.ImageData, step);
            var a = img.ToImage<Bgr, byte>();
            var b = a.ToBitmap();

            return b;
        }
    }
}