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
using System.Linq;

namespace CourseworkWinforms
{
    public partial class CamsViewer : Form
    {
        private VideoCapture capture;
        private DsDevice[] webCams;
        private int selectedCameraId = 0;

        public CamsViewer()
        {
            InitializeComponent();
            ZoomPictureBox();
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

        private void ZoomPictureBox()
        {
            var k = (double)pictureBox1.Image.Width / pictureBox1.Image.Height;

            var w = tableLayoutPanel2.GetColumnWidths()[0];
            var h = tableLayoutPanel2.GetRowHeights()[0];

            bool widthGreater = w > h * k;
            var lowerImgSide = widthGreater ? pictureBox1.Image.Height : pictureBox1.Image.Width;
            var lowerFormSide = widthGreater ? h : w;

            float ratioSize = (float)lowerFormSide / lowerImgSide;
            var newWidth = (int)(pictureBox1.Image.Width * ratioSize);
            var newHeight = (int)(pictureBox1.Image.Height * ratioSize);
            pictureBox1.ClientSize = new Size(newWidth, newHeight);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCameraId = toolStripComboBox1.SelectedIndex;
            try
            {
                capture = new VideoCapture(selectedCameraId);
                capture.ImageGrabbed += CaptureOnImageGrabbed;
                capture.Start();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ZoomPictureBox();
        }

        private void CaptureOnImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();
                capture.Retrieve(m);
                pictureBox1.Image = m.ToImage<Bgr, byte>().ToBitmap();
            }
            catch (InvalidOperationException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel2.Text = e.Location.ToString();
        }

        private void CamsViewer_Resize(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
                ZoomPictureBox();
        }

        private void pictureBox1_Click(object sender, MouseEventArgs e)
        {
            var k = (double)pictureBox1.Image.Height / pictureBox1.Height;
            var cameraPoint = new Point((int)(e.Location.X * k), (int)(e.Location.Y * k));
            
            var lvitem = new ListViewItem(cameraPoint.ToString());
            lvitem.Tag = cameraPoint;
            listView1.Items.Add(lvitem);
            var a = GetSelectedPoints();
        }

        private List<Point> GetSelectedPoints()
        {
            var result = new List<Point>();
            foreach (ListViewItem p in listView1.Items)
                result.Add((Point)p.Tag);
            return result;
        }
    }
}