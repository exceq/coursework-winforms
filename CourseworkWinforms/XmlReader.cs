using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Emgu.CV;

namespace CourseworkWinforms
{
    public static class XmlReader
    {
        private static Matrix<double> ParseMatrixFromXml(XmlElement node)
        {
            GetMatrixParamsFromNode(out int rows, out int col, out double[] data, node);
            return ParseMatrixByRowColData(rows, col, data);
        }

        private static Frame ParseFrameFromXml(XmlElement node)
        {
            float[] data = {};
            foreach (XmlNode child in node.ChildNodes)
                if (child.Name == "data")
                    data = child.InnerText.Split()
                        .Where(x=> x.Length>0)
                        .Select(x=> float.Parse(x, CultureInfo.InvariantCulture))
                        .ToArray();
            return new Frame(data[0], data[1], data[2], data[3], data[4], data[5]);
        }

        private static void GetMatrixParamsFromNode(out int r, out int c, out double[] data, XmlElement node)
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

        private static Matrix<double> ParseMatrixByRowColData(int row, int col, double[] data)
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

        public static CameraProperties GetCameraPropertiesFromFile(string path)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(path);
            return ParseXml(xDoc);
        }
        public static CameraProperties GetCameraProperties(string xmlString)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xmlString);
            return ParseXml(xDoc);
        }

        private static CameraProperties ParseXml(XmlDocument xDoc)
        {
            CameraProperties camera = new CameraProperties();
            // ?????????????? ???????????????? ??????????????
            XmlElement root = xDoc.DocumentElement;
            if (root != null)
            {
                // ?????????? ???????? ?????????? ?? ???????????????? ????????????????
                foreach (XmlElement node in root)
                {
                    switch (node.Name)
                    {
                        case "camera_matrix":
                            camera.CameraMatrix = ParseMatrixFromXml(node);
                            break;
                        case "distortion_coefficients":
                            camera.DistortionCoefficients = ParseMatrixFromXml(node);
                            break;
                        case "R_camera":
                            camera.R_camera2flange = ParseMatrixFromXml(node);
                            break;
                        case "T_camera":
                            camera.T_camera2flange = ParseMatrixFromXml(node);
                            break;
                        // case "camera_pos":
                        //     camera.cameraCartesianPosition = ParseFrameFromXml(node);
                        //     break;
                        // case "flange_position":
                        //     camera.flangeFrame = ParseFrameFromXml(node);
                        //     break;
                        case "focus":
                            foreach (XmlNode child in node.ChildNodes)
                                if (child.Name == "data")
                                    camera.Focus = double.Parse(child.InnerText);
                            break;
                    }
                }
            }

            return camera;
        }
    }
}