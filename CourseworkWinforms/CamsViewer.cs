using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CourseworkWinforms
{
    public partial class CamsViewer : Form
    {
        private VideoCapture capture;
        private DsDevice[] webCams;
        private int selectedCameraId = 0;

        private bool firstCrop = true;


        public CamsViewer()
        {
            InitializeComponent();
            AdjustButtons();
            ZoomPictureBox();
        }

        // Загрузка формы
        private void CamsViewer_Load(object sender, EventArgs e)
        {
            webCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            foreach (var dsDevice in webCams)
                toolStripComboBox1.Items.Add(dsDevice.Name);
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
        
        private void ZoomPictureBox(Bitmap img)
        {
            var k = (double)img.Width / img.Height;

            var w = tableLayoutPanel2.GetColumnWidths()[0];
            var h = tableLayoutPanel2.GetRowHeights()[0];

            bool widthGreater = w > h * k;
            var lowerImgSide = widthGreater ? img.Height : img.Width;
            var lowerFormSide = widthGreater ? h : w;

            float ratioSize = (float)lowerFormSide / lowerImgSide;
            var newWidth = (int)(img.Width * ratioSize);
            var newHeight = (int)(img.Height * ratioSize);
            pictureBox1.ClientSize = new Size(newWidth, newHeight);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCameraId = toolStripComboBox1.SelectedIndex;
            try
            {
                firstCrop = true;
                capture = new VideoCapture(selectedCameraId);
                capture.ImageGrabbed += CaptureOnImageGrabbed;
                capture.Start();
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
                var img = m.ToImage<Bgr, byte>().ToBitmap();
                pictureBox1.Image = img;
                if (firstCrop)
                { 
                    ZoomPictureBox();
                    firstCrop = false;
                }
            }
            catch (InvalidOperationException)
            {
            }
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

            AdjustButtons();
        }

        private IEnumerable<Point> GetSelectedPoints()
        {
            foreach (ListViewItem p in listView1.Items)
                yield return (Point)p.Tag;
        }

        private void buttonItemDelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem i in listView1.SelectedItems)
                listView1.Items.Remove(i);
            
            AdjustButtons();
        }

        private void buttonDeleteAllItems_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            AdjustButtons();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            AdjustButtons();
        }

        private void AdjustButtons()
        {
            var c = listView1.SelectedItems.Count;

            buttonItemDelete.Enabled = c > 0;

            buttonItemUp.Enabled = c == 1 && listView1.SelectedIndices[0] > 0;
            buttonItemDown.Enabled = c == 1 && listView1.SelectedIndices[0] < listView1.Items.Count - 1;

            buttonDeleteAllItems.Enabled = c > 0;
        }

        private void buttonItemUp_Click(object sender, EventArgs e)
        {
            SwapItems(listView1.SelectedIndices[0], false);
        }

        private void buttonItemDown_Click(object sender, EventArgs e)
        {
            SwapItems(listView1.SelectedIndices[0], true);
        }

        private void SwapItems(int index, bool up)
        {
            int swapIndex = index + (up ? 1 : -1);

            var item1 = listView1.Items[index];
            listView1.Items[index].Remove();
            listView1.Items.Insert(swapIndex, item1);
            listView1.SelectedIndices.Clear();
            item1.Focused = true;
            item1.Selected = true;
            listView1.Focus();
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            var points = GetSelectedPoints();
            
            //TODO Что-то связанное с pointToPixelLine

            MessageBox.Show("Кнопка в разработке :)", "Dev", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}