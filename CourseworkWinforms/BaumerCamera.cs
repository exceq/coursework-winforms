using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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


            Dictionary<string,Feature> featureList = Camera.GetFeatureList();

            Feature pixelformat = new Feature();
            DepthType type = DepthType.Cv8U;
            if (Camera.f.PixelFormat.GetEnumValueList().TryGetValue("BGR8", out pixelformat)
                && pixelformat.IsAvailable)
            {
                Camera.f.PixelFormat.Value = NeoAPI.PixelFormat.BGR8;
                //Camera.f.PixelFormat.ValueString = "BGR8";
            }
            else if (Camera.f.PixelFormat.GetEnumValueList().TryGetValue("Mono8", out pixelformat)
                     && pixelformat.IsAvailable)
            {
                Camera.f.PixelFormat.Value = NeoAPI.PixelFormat.Mono8;
                Camera.f.PixelFormat.ValueString = "Mono8";
            }
            else
            {
                Console.Write("no supported pixel format");
            }

            Camera.f.Width.Value = 1920;
            Camera.f.Height.Value = 1280;
            Camera.f.ExposureAuto.Value = ExposureAuto.Off;
            Camera.f.ExposureTime.Value = 50000;
            
            //f.BinningHorizontal.Value = 1; //уменьшение разрешения в 4 раза ???
            //f.BinningVertical.Value = 1; //уменьшение разрешения в 4 раза
            //Camera.f.Gain.Value = 17.64;
            
            Camera.ImageBufferCount = 10; // set the size of the buffer queue to 10
            Camera.ImageBufferCycleCount = 1; // sets the cycle count to 1 

            CameraProperties = new XmlReader().GetCameraProperties(cameraPropertiesXml);
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