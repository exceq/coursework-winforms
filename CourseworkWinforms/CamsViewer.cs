using CourseworkWinforms.Properties;
using NeoAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Image = System.Drawing.Image;

namespace CourseworkWinforms
{
    public partial class CamsViewer : Form
    {
        private BaumerCamera baumer;
        private bool firstCrop;
        private Client client;

        public CamsViewer()
        {
            InitializeComponent();
            AdjustButtons();
            ZoomPictureBox();
            //toolStripButtonConnectBaumer_Click(null, null); // запуск камеры
            client = new Client();
        }

        #region Подключение к камерам

        private void toolStripButtonConnectBaumer_Click(object sender, EventArgs e)
        {
            firstCrop = true;
            try
            {
                ConnectToBaumer();
            }
            catch (NeoAPI.NotConnectedException exception)
            {
                MessageBox.Show("Не удалось подключиться к баумерской камере.\n\nПодробности:\n" + exception,
                    "NotConnectedException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void ConnectToBaumerAsync()
        {
            CameraProperties prop = XmlReader.GetCameraProperties(Resources.camera_properties);
            baumer = new BaumerCamera(prop);
        }

        private async void ConnectToBaumer(string id = "")
        {
            toolStripLabelCameraName.Text = "Подключение...";
            toolStripButtonConnectBaumer.Enabled = false;
            await Task.Run(ConnectToBaumerAsync);

            toolStripLabelCameraName.Text = baumer.Camera.f.DeviceModelName.ValueString;
            toolStripButtonConnectBaumer.Enabled = !baumer.Camera.IsConnected;

            StartGettingImagesBaumer();
        }

        private Bitmap GetImageAsync()
        {
            var neoImg = baumer.Camera.GetImage();
            Bitmap bitmap = BaumerCamera.ConvertNeoImageToBitmap(neoImg);
            Thread.Sleep(40);
            neoImg.Dispose();
            return bitmap;
        }

        private async void StartGettingImagesBaumer()
        {
            for (int i = 0; baumer.Camera.IsConnected; i++)
            {
                try
                {
                    Console.WriteLine("Image " + i + " Start");
                    Bitmap b = await Task.Run(GetImageAsync);
                    SetImageToPictureBox(b);
                }
                catch
                {
                    //ignored
                }
            }
        }


        private void OnImageReceived(object sender, ImageEventArgs imageEventArgs)
        {
            pictureBox1.Image?.Dispose();
            Bitmap image = BaumerCamera.ConvertNeoImageToBitmap(imageEventArgs.Image);
            SetImageToPictureBox(image);
        }

        private void SetImageToPictureBox(Image image)
        {
            if (checkBoxPaintSelected.Checked)
                DrawPointOnImage(image);
            pictureBox1.Image?.Dispose();
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
                double k = pictureBox1.Image.Width / (double)pictureBox1.Width * 1.2;
                greenPen.Width = (int)(k * 1.5);
                bluePen.Width = greenPen.Width;
                int fontSize = (int)(10 * k);
                int radius = (int)(4 * k);
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

                    DrawCircle((i + 1).ToString(), greenPen, Brushes.GreenYellow, p, radius, gr, fontSize);
                }

                for (var i = 0; i < listView1.SelectedIndices.Count; i++)
                {
                    int index = listView1.SelectedIndices[i];
                    var p = (Point)listView1.Items[index].Tag;
                    DrawCircle((index + 1).ToString(), bluePen, Brushes.Blue, p, radius, gr, fontSize);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void DrawCircle(string text, Pen pen, Brush brush, Point p, int radius, Graphics gr, int sizeFont)
        {
            gr.DrawEllipse(pen, GetCircle(p, radius));
            gr.DrawString(text, new Font("Arial", sizeFont, FontStyle.Bold),
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
                PositionData positionData = client.Read1();

                IEnumerable<Frame> newCoords =
                    GetSelectedPoints().Select(point => CalculateNewCoordinate(point, positionData));
                StringBuilder sb = new StringBuilder();

                foreach (var p in newCoords)
                {
                    sb.Append(p).Append("\n\n");
                }

                labelCameraPosition.Text = "Позиция камеры: \n" + positionData.CameraPosition;
                labelFlangePosition.Text = "Позиция Фланца: \n" + positionData.FlangePosition;
                // TODO сделать обновление позиции камеры при передвижении


                // TODO отправить манипулятору команду на передвижение по заданным точкам  
                // Постоянно считывать данные, если предыдущая и текущая точка равны,
                // это значит, что манипулятор достиг точки и можно отправлять следующую

                // foreach (var newFrame in newCoords)
                // {
                //     client.Send(newFrame); // idk
                // }

                MessageBox.Show(sb.ToString(), "Новые координаты", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Ошибка\n\n" + exception, "Dev", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        #endregion

        private Point PictureBoxPointToImagePoint(Point pbPoint)
        {
            while (true)
            {
                try
                {
                    var k = (double)pictureBox1.Image.Height / pictureBox1.Height;
                    var imagePoint = new Point((int)(pbPoint.X * k), (int)(pbPoint.Y * k));
                    return imagePoint;
                }
                catch
                {
                }
            }
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

        private Frame CalculateNewCoordinate(Point point, PositionData positionData)
        {
            Frame direction = CameraMath.CalculatePixelLineFromCameraProperties(baumer.CameraProperties,
                positionData.CameraPosition,
                positionData.FlangePosition,
                point);
            return CameraMath.CalculateNewCoordinate(direction, positionData.RangeFinderDistanceIntern);
        }
    }
}