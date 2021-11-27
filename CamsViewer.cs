using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Emgu;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace CourseworkWinforms
{
    public partial class CamsViewer : Form
    {
        private VideoCapture capture = null;
        private DsDevice[] webCams = null;
        private int selectedCameraId = 0;
        private Size imageSize;

        public CamsViewer()
        {
            InitializeComponent();
        }

        // Загрузка формы
        private void CamsViewer_Load(object sender, EventArgs e)
        {
            webCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            foreach (var dsDevice in webCams)
            {
                toolStripComboBox1.Items.Add(dsDevice.Name);
            }
        }

        private void ZoomPictureBox(Size imgSize)
        {
            //int greaterImgSide = GetGreater(imgSize.Height,imgSize.Width);
            //int greaterFormSide = GetGreater(this.Height,this.Width);
            //float ratioSize = (float)greaterFormSide / greaterImgSide;
            //pictureBox1.ClientSize = new Size((int)(imgSize.Width * ratioSize), (int)(imgSize.Height * ratioSize));
            int lowerImgSide = GetGreater(imgSize.Height,imgSize.Width);
            var a = tableLayoutPanel2.GetColumnWidths();
            var b = tableLayoutPanel2.GetRowHeights();
            int lowerFormSide = GetLower(b[0],a[0]);

            float ratioSize = (float)lowerFormSide / lowerImgSide;
            pictureBox1.Size = new Size((int)(imgSize.Width * ratioSize), (int)(imgSize.Height * ratioSize));
        }

        private int GetGreater(int a, int b)
        {
            return a > b ? a : b;
        }

        private int GetLower(int a, int b)
        {
            return a < b ? a : b;
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCameraId = toolStripComboBox1.SelectedIndex;
            try
            {
                capture = new VideoCapture(selectedCameraId);
                capture.ImageGrabbed += CaptureOnImageGrabbed;
                capture.Start();

                imageSize = new Size(capture.Width, capture.Height);
                ZoomPictureBox(imageSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CaptureOnImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();
                capture.Retrieve(m);
                pictureBox1.Image = m.ToImage<Bgr, byte>().ToBitmap();
            }
            catch (InvalidOperationException ex) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private Point[] points = new Point[30];
        private int current = 0;
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel2.Text = e.Location.ToString();

            if (current == points.Length)
                current = 0;
            points[current] = e.Location;

            label1.Text = string.Join("\n", points.Skip(current));
            label1.Text += "\n"+string.Join("\n", points.Take(current));

            current++;
            //label1.Text = string.Join("\n", list.Take(40));
        }

        private void CamsViewer_Resize(object sender, EventArgs e)
        {
            if (capture != null)
                ZoomPictureBox(imageSize);
        }
    }
}