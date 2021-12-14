using CourseworkWinforms.Properties;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using NeoAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Image = System.Drawing.Image;

namespace CourseworkWinforms
{
    public partial class CamsViewer : Form
    {
        private DsDevice[] webCams;
        private VideoCapture capture;
        private int selectedIndex;


        private BaumerCamera baumer;
        private bool firstCrop;

        public CamsViewer()
        {
            InitializeComponent();
            AdjustButtons();
            ZoomPictureBox();
        }

        #region Подключение к камерам

        private void toolStripButtonConnectBaumer_Click(object sender, EventArgs e)
        {
            firstCrop = true;
            string id = toolStripTextBox1.Text;
            id = string.IsNullOrWhiteSpace(id) ? "" : id;
            try
            {
                // ConnectToBaumer(id);
                ConnectToBaumerButWhileTrue(id);

                capture?.Stop();
            }
            catch (NotConnectedException exception)
            {
                MessageBox.Show("Не удалось подключиться к баумерской камере.\n\nПодробности:\n" + exception,
                    "NotConnectedException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButtonConnectCamera_Click(object sender, EventArgs e)
        {
            firstCrop = true;
            try
            {
                ConnectToWebCameras();
                toolStripComboBox1.Visible = true;
                toolStripButtonConnectCamera.Enabled = false;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectToWebCameras()
        {
            webCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            if (webCams.Length == 0)
                throw new ArgumentException("Не удалось подключиться к веб камерам.");

            foreach (var dsDevice in webCams)
                toolStripComboBox1.Items.Add(dsDevice.Name);

            toolStripComboBox1.Visible = true;
            toolStripButtonConnectCamera.Enabled = false;
        }

        private void ConnectToBaumerButWhileTrue(string id = "")
        {
            baumer = new BaumerCamera(Resources.camera_properties, id);
            int i = 1;
            while (baumer.Camera.IsConnected)
            {
                Bitmap bitmap = null;
                try
                {
                    bitmap = BaumerCamera.ConvertNeoImageToBitmap(baumer.Camera.GetImage());
                }
                catch (Exception e)
                {
                    // MessageBox.Show("Не удалось сконвертировать изображение.\n\nПодробности:\n" + e,
                    //     "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    toolStripTextBox1.Text = "Ошибка " + i++; // Чтобы в случае чего не было слишком много мессаджей
                }

                if (bitmap != null)
                    SetImageToPictureBox(bitmap);
            }
        }

        private void ConnectToBaumer(string id = "")
        {
            baumer = new BaumerCamera(Resources.camera_properties, id);

            // Включение эвентов у камеры, чтобы получать изображения
            // как только они поступают с камеры
            baumer.Camera.f.TriggerMode.Value = TriggerMode.On;
            baumer.Camera.f.TriggerSource.Value = TriggerSource.Software;
            baumer.Camera.ImageCallback.Handler += OnImageReceived; // обработчик изображений

            baumer.Camera.EnableImageCallback();

            if (baumer.Camera.IsConnected)
                toolStripButtonConnectBaumer.Enabled = false;

            for (int i = 0; i < 5; i++) // send 5 triggers to trigger some image callbacks
            {
                baumer.Camera.f.TriggerSoftware.Execute();
                System.Threading.Thread.Sleep(100);
            }

            baumer.Camera.DisableImageCallback(); // disable callback
            baumer.Camera.Dispose();

            toolStripLabelCameraName.Text = baumer.Camera.f.DeviceModelName.ValueString;
        }

        private void OnImageReceivedFromWebCams(object sender, EventArgs e)
        {
            Mat m = new Mat();
            capture.Retrieve(m);
            SetImageToPictureBox(m.ToImage<Bgr, byte>().ToBitmap());
        }

        private void OnImageReceived(object sender, ImageEventArgs imageEventArgs)
        {
            Bitmap img = BaumerCamera.ConvertNeoImageToBitmap(imageEventArgs.Image);
            SetImageToPictureBox(img);
        }

        private void SetImageToPictureBox(Image image)
        {
            if (checkBoxPaintSelected.Checked)
                DrawPointOnImage(image);
            pictureBox1.Image = image;
            if (firstCrop)
            {
                ZoomPictureBox();
                firstCrop = false;
            }
        }

        #endregion

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
            try
            {
                pictureBox1.ClientSize = new Size(newWidth, newHeight);
            }
            catch (Exception e)
            {
                // ignored
            }
        }


        #region Drawing

        Pen greenPen = new Pen(Color.GreenYellow, 3);
        Pen bluePen = new Pen(Color.Blue, 3);

        private void DrawPointOnImage(Image img)
        {
            try
            {
                var gr = Graphics.FromImage(img);
                for (var i = 0; i < listView1.Items.Count; i++)
                {
                    Point p = (Point)listView1.Items[i].Tag;
                    if (listView1.Items.Count > 1 && i > 0)
                    {
                        Point pp = (Point)listView1.Items[i - 1].Tag;
                        if (listView1.SelectedIndices.Contains(i) && listView1.SelectedIndices.Contains(i - 1))
                            gr.DrawLine(bluePen, p.X, p.Y, pp.X, pp.Y);
                        else
                            gr.DrawLine(greenPen, p.X, p.Y, pp.X, pp.Y);
                    }

                    DrawCircle((i + 1).ToString(), greenPen, Brushes.GreenYellow, p, 5, gr);
                }

                for (var i = 0; i < listView1.SelectedIndices.Count; i++)
                {
                    int index = listView1.SelectedIndices[i];
                    var p = (Point)listView1.Items[index].Tag;
                    DrawCircle((index + 1).ToString(), bluePen, Brushes.Blue, p, 5, gr);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void DrawCircle(string text, Pen pen, Brush brush, Point p, int radius, Graphics gr)
        {
            gr.DrawEllipse(pen, GetCircle(p, radius));
            gr.DrawString(text, new Font("Arial", 14, FontStyle.Bold),
                brush, new Point(p.X + radius, p.Y - radius));
        }

        private Rectangle GetCircle(Point p, int radius)
        {
            return new Rectangle(p.X - radius, p.Y - radius, radius * 2, radius * 2);
        }

        #endregion


        #region Events

        private void pictureBox1_Click(object sender, MouseEventArgs e)
        {
            var imagePoint = PictureBoxPointToImagePoint(e.Location);

            var lvitem = new ListViewItem($"{listView1.Items.Count + 1}. X: {imagePoint.X} Y: {imagePoint.Y}");
            lvitem.Tag = imagePoint;
            listView1.Items.Add(lvitem);

            AdjustButtons();

            //Рисование на статической картинке ограничено
            //потому что pictureBox не обновляется самостояельно
            if (firstCrop && checkBoxPaintSelected.Checked)
            {
                var img = Resources.big3000x2000;
                DrawPointOnImage(img);
                pictureBox1.Image = img;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel2.Text = PictureBoxPointToImagePoint(e.Location).ToString();
        }

        private void CamsViewer_Resize(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
                ZoomPictureBox();
        }

        private void buttonItemDelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem i in listView1.SelectedItems)
                listView1.Items.Remove(i);
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

        private void buttonItemUp_Click(object sender, EventArgs e)
        {
            SwapItems(listView1.SelectedIndices[0], false);
        }

        private void buttonItemDown_Click(object sender, EventArgs e)
        {
            SwapItems(listView1.SelectedIndices[0], true);
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            try
            {
                IEnumerable<Frame> points = GetSelectedPoints().Select(GetFrameFromPoint);
                StringBuilder sb = new StringBuilder();
                foreach (var p in points)
                {
                    sb.Append("X: " + p.X + "\n");
                    sb.Append("Y: " + p.Y + "\n");
                    sb.Append("Z: " + p.Z + "\n");
                    sb.Append("A: " + p.A + "\n");
                    sb.Append("B: " + p.B + "\n");
                    sb.Append("C: " + p.C + "\n\n");
                }

                // string output = string.Join("\n", points.Select(x => "X: " + x.X));
                string output = sb.ToString();

                MessageBox.Show(output, "Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Кнопка в разработке :)\n\n" + exception, "Dev", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        #endregion

        private Point PictureBoxPointToImagePoint(Point pbPoint)
        {
            var k = (double)pictureBox1.Image.Height / pictureBox1.Height;
            var imagePoint = new Point((int)(pbPoint.X * k), (int)(pbPoint.Y * k));
            return imagePoint;
        }

        private IEnumerable<Point> GetSelectedPoints()
        {
            foreach (ListViewItem p in listView1.Items)
                yield return (Point)p.Tag;
        }

        private void AdjustButtons()
        {
            var c = listView1.SelectedItems.Count;

            buttonItemDelete.Enabled = c > 0;

            buttonItemUp.Enabled = c == 1 && listView1.SelectedIndices[0] > 0;
            buttonItemDown.Enabled = c == 1 && listView1.SelectedIndices[0] < listView1.Items.Count - 1;


            buttonDeleteAllItems.Enabled = listView1.Items.Count > 0;
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

        private Frame GetFrameFromPoint(Point point)
        {
            var prop = baumer?.CameraProperties;
            if (prop == null)
                prop = new XmlReader().GetCameraProperties(Resources.camera_properties);
            return CameraMath.CalculatePixelLineFromCameraProperties(prop, point);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (baumer != null)
                baumer.Camera.f.TriggerMode.Value = TriggerMode.Off;

            capture?.Stop();

            selectedIndex = toolStripComboBox1.SelectedIndex;

            capture = new VideoCapture();
            capture.ImageGrabbed += OnImageReceivedFromWebCams;
            capture.Start();
            listView1.Clear();
            AdjustButtons();
        }
    }
}