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

        public BaumerCamera(string cameraPropertiesXmlPath)
        {
            Camera = new Cam();
            Camera.Connect();
            FeatureAccess f = new FeatureAccess(Camera);
            f.Width.Value = 3072;
            f.Height.Value = 2048;
            f.ExposureTime.Value = 331042;
            f.PixelFormat.Value = PixelFormat.BayerRG8; //Цветовое пространство
            f.BinningHorizontal.Value = 1; //уменьшение разрешения в 4 раза 
            f.BinningVertical.Value = 1; //уменьшение разрешения в 4 раза
            f.Gain.Value = 17.64;

            Camera.ImageBufferCount = 10; // set the size of the buffer queue to 10
            Camera.ImageBufferCycleCount = 1; // sets the cycle count to 1 

            CameraProperties = new XmlReader().GetCameraProperties(cameraPropertiesXmlPath);
        }

        public static Bitmap ConvertNeoImageToBitmap(Image image)
        {
            Mat img = new Mat((int)image.Height, (int)image.Width, DepthType.Cv8U, 3,
                image.ImageData, 0);
            Bitmap bitmap = img.ToImage<Bgr, byte>().ToBitmap();
            return bitmap;
        }
    }
}