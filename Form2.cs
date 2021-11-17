using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace CourseworkWinforms
{
    public partial class Form2 : Form
    {
        class Angles
        {
            public double horizontal;
            public double vertical;
        }

        private Point centerImg;
        private Point centerBox;
        private float ratioSize = 1f;
        private double viewAnlge = 73.8; // degrees
        private double angleOfPixel;

        public Form2()
        {
            InitializeComponent();
            Image img = new Bitmap("..\\..\\..\\sources\\image.jpg");
            pictureBox1.Image = img;
            ZoomImage(img, 800);
            centerImg = new Point(pictureBox1.Image.Width / 2, pictureBox1.Image.Height / 2);
            centerBox = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            DrawAxis(img);
        }

        private void ZoomImage(Image img, int maxSize)
        {
            bool hIsGreater = img.Height > img.Width;
            int greaterSide = hIsGreater ? img.Height : img.Width;
            ratioSize = (float) maxSize / greaterSide;
            pictureBox1.ClientSize = new Size((int) (img.Width * ratioSize), (int) (img.Height * ratioSize));
            angleOfPixel = viewAnlge / (hIsGreater ? pictureBox1.Height : pictureBox1.Width);
        }

        private void pictureBox1_Click(object sender, MouseEventArgs e)
        {
            clickInfo.Text =
                $"Height {pictureBox1.Height}\nWidth {pictureBox1.Width}\n\nMouse click:\nX: {e.X} Y: {e.Y}";
            DrawLineFromCenter((int) (e.X / ratioSize), (int) (e.Y / ratioSize));
            var a = GetAngles(e.X, e.Y);
            anglesLabel.Text = $"Угол между X и Z: {a.horizontal}\nУгол между Y и Z: {a.vertical}";
        }

        private Angles GetAngles(int x, int y)
        {
            Point fromCenter = new Point(x - centerBox.X, centerBox.Y - y);
            var angles = new Angles();
            angles.horizontal = fromCenter.X * angleOfPixel;
            angles.vertical = fromCenter.Y * angleOfPixel;

            label2.Text = $"От центра:\nX: {fromCenter.X} Y: {fromCenter.Y}";

            var angleVert = Math.Atan((double) 75/200) * 180 / Math.PI;
            var angleHoriz = Math.Atan((double) 200/75) * 180 / Math.PI;

            var radianVertical = angles.vertical * Math.PI / 180;
            var radianHorizontal = angles.horizontal * Math.PI / 180;

            var tanVert = Math.Tan(radianVertical);
            var tanHoriz = Math.Tan(radianHorizontal);

            var Z = fromCenter.Y / tanVert;
            var Z1 = fromCenter.X / tanHoriz;

            Vector3 directionVector = new Vector3(fromCenter.X, fromCenter.Y, (float)Z);
            label1.Text = $"Z по X: {Z1}\nZ по Y: {Z}";
            return angles;
        }

        private void DrawLineFromCenter(int x, int y)
        {
            Graphics gr = Graphics.FromImage(pictureBox1.Image);
            Pen pen = new Pen(Color.Green, 10);

            gr.DrawLine(pen, centerImg.X, centerImg.Y, x, y);

            pen.Color = Color.GreenYellow;
            gr.DrawEllipse(pen, x - 25, y - 25, 50, 50);
            pictureBox1.Image = pictureBox1.Image;
        }

        private void DrawAxis(Image img)
        {
            Graphics gr = Graphics.FromImage(pictureBox1.Image);
            Pen pen = new Pen(Color.Green, 10);

            gr.DrawLine(pen, 0, centerImg.Y, centerImg.X * 2, centerImg.Y);
            gr.DrawLine(pen, centerImg.X, 0, centerImg.X, centerImg.Y * 2);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}