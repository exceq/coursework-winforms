using System;
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
           
            Camera.f.PixelFormat.Value = PixelFormat.BGR8;
            Camera.f.ExposureAuto.Value = ExposureAuto.Off;
            Camera.f.ExposureTime.Value = 20000;

            double k = 0.5;

            Camera.f.Width.Value = (int)(3072 * k);
            Camera.f.Height.Value = (int)(2048 * k);
            Camera.f.BinningHorizontal.Value = 4; //уменьшение разрешения в 4 раза ???
            Camera.f.BinningVertical.Value = 4; //уменьшение разрешения в 4 раза
            Camera.f.Gain.Value = 20;
            
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