using Emgu.CV;

namespace CourseworkWinforms
{
    public class CameraProperties
    {
        //отрефакторить
        public Matrix<double> CameraMatrix { get; set; }
        public Matrix<double> DistortionCoefficients { get; set; } 

        public Matrix<double> R_camera2flange { get; set; }
        public Matrix<double> T_camera2flange { get; set; }
        public double Focus { get; set; }

        public CameraProperties()
        {
        }
        public CameraProperties(Matrix<double> camM,
            Matrix<double> distor,
            Matrix<double> R_camera,
            Matrix<double> T_camera,
            double foc)
        {
            CameraMatrix = camM;
            DistortionCoefficients = distor;
            R_camera2flange = R_camera;
            T_camera2flange = T_camera;
            Focus = foc;
        }
    }
}