using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Emgu.CV;

namespace CourseworkWinforms
{
    public class XmlReader
    {
        private Matrix<double> ParseMatrixFromXml(XmlElement node)
        {
            GetMatrixParamsFromNode(out int rows, out int col, out double[] data, node);
            return ParseMatrixByRowColData(rows, col, data);
        }

        private Frame ParseFrameFromXml(XmlElement node)
        {
            float[] data = {};
            foreach (XmlNode child in node.ChildNodes)
                if (child.Name == "data")
                    data = child.InnerText.Split(' ').Select(float.Parse).ToArray();

            Frame fr = new Frame(data[0], data[1], data[2], data[3], data[4], data[5]);
            return fr;
        }

        private void GetMatrixParamsFromNode(out int r, out int c, out double[] data, XmlElement node)
        {
            r = 0;
            c = 0;
            data = Array.Empty<double>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "rows") r = int.Parse(child.InnerText);
                if (child.Name == "cols") c = int.Parse(child.InnerText);
                if (child.Name == "data")
                    data = child.InnerText.Split()
                        .Where(x => x.Length > 0)
                        .Select(x => double.Parse(x, CultureInfo.InvariantCulture))
                        .ToArray();
            }
        }

        private Matrix<double> ParseMatrixByRowColData(int row, int col, double[] data)
        {
            double[,] arr = new double[col, row];

            int i = 0;
            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < col; c++)
                {
                    arr[c, r] = data[i];
                    i++;
                }
            }

            Matrix<double> mat = new Matrix<double>(arr);
            return mat;
        }

        public CameraProperties GetCameraProperties(string path)
        {
            Console.WriteLine("Reading properties from Xml");
            CameraProperties camera = new CameraProperties();
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.Load(path); //добавить обработку исключения

                // получим корневой элемент
                XmlElement root = xDoc.DocumentElement;
                if (root != null)
                {
                    // обход всех узлов в корневом элементе
                    foreach (XmlElement node in root)
                    {
                        switch (node.Name)
                        {
                            case "camera_matrix":
                                camera.cameraMatrix = ParseMatrixFromXml(node);
                                break;
                            case "distortion_coefficients":
                                camera.distorsionCoefficients = ParseMatrixFromXml(node);
                                break;
                            case "R_camera":
                                camera.R_camera2flange = ParseMatrixFromXml(node);
                                break;
                            case "T_camera":
                                camera.T_camera2flange = ParseMatrixFromXml(node);
                                break;
                            case "camera_pos":
                                camera.cameraCartesianPosition = ParseFrameFromXml(node);
                                break;
                            case "flange_position":
                                camera.flangeFrame = ParseFrameFromXml(node);
                                break;
                            case "focus":
                                foreach (XmlNode child in node.ChildNodes)
                                    if (child.Name == "data") 
                                        camera.focus = double.Parse(child.InnerText);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }

            return camera;
        }
    }
}