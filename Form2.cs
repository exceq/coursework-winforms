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
        private int focusDistance = 6; // mm
        private double mmPerPixel = 0.2767;
        private double angleOfPixel;

        public Form2()
        {
            InitializeComponent();
            Image img = new Bitmap("..\\..\\sources\\image.jpg");
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
            ratioSize = (float)maxSize / greaterSide;
            pictureBox1.ClientSize = new Size((int)(img.Width * ratioSize), (int)(img.Height * ratioSize));
            // angleOfPixel = viewAnlge / (hIsGreater ? pictureBox1.Height : pictureBox1.Width);
            int a = pictureBox1.Height / 2;
            int b = pictureBox1.Width / 2;
            angleOfPixel = viewAnlge / Math.Sqrt(a * a + b * b);
        }

        private void pictureBox1_Click(object sender, MouseEventArgs e)
        {
            pictureBox1_MouseMove(sender, e);
            DrawLineFromCenter((int)(e.X / ratioSize), (int)(e.Y / ratioSize));
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            clickInfo.Text =
                $"Height {pictureBox1.Height}\nWidth {pictureBox1.Width}\n\nMouse click:\nX: {e.X} Y: {e.Y}";
            // var a = GetAngles(e.X, e.Y);
            var a = GetAnglesByPhoto(e.X, e.Y, 6, 0.0007);
            anglesLabel.Text = $"Угол между X и Z: {a.horizontal}\nУгол между Y и Z: {a.vertical}";
        }

        private Angles GetAngles(int x, int y)
        {
            Point fromCenter = new Point(x - centerBox.X, centerBox.Y - y);
            var angles = new Angles();
            // angles.horizontal = fromCenter.X * angleOfPixel;
            // angles.vertical = fromCenter.Y * angleOfPixel;
            angles.horizontal = Math.Atan((fromCenter.X * mmPerPixel) / focusDistance) * 180 / Math.PI / 2;
            angles.vertical = Math.Atan((fromCenter.Y * mmPerPixel) / focusDistance) * 180 / Math.PI / 2;

            label2.Text = $"От центра:\nX: {fromCenter.X} Y: {fromCenter.Y}";

            var ZfromY = GetAnotherCathetus(angles.vertical, fromCenter.Y);
            var ZfromX = GetAnotherCathetus(angles.horizontal, fromCenter.X);

            var directionVector = new Point<double>(fromCenter.X, fromCenter.Y, ZfromY);
            label1.Text = $"Z по X: {ZfromX}\nZ по Y: {ZfromY}";
            return angles;
        }

        private Angles GetAnglesByPhoto(int x, int y, int focusDistance, double sizeOfPixel) // 0.0007 мм
        {
            double k = (double)pictureBox1.Image.Width / pictureBox1.Width;
            Point fromCenter = new Point((int)(x * k) - centerImg.X, centerImg.Y - (int)(y * k));

            var angles = new Angles();

            angles.horizontal = Math.Atan(fromCenter.X * sizeOfPixel / focusDistance) * 180 / Math.PI;
            angles.vertical = Math.Atan(fromCenter.Y * sizeOfPixel / focusDistance) * 180 / Math.PI;

            label2.Text = $"От центра:\nX: {fromCenter.X} Y: {fromCenter.Y}";

            var ZfromY = GetAnotherCathetus(angles.vertical, fromCenter.Y);
            var ZfromX = GetAnotherCathetus(angles.horizontal, fromCenter.X);

            var directionVector = NormalizeVector(new Point<double>(fromCenter.X, fromCenter.Y, ZfromY));
            label1.Text = $"Z по X: {ZfromX}\nZ по Y: {ZfromY}" +
                          "\n\nNormal vector: " +
                          $"\nX: {directionVector.X}" +
                          $"\nY: {directionVector.Y}" +
                          $"\nZ: {directionVector.Z}";
            return angles;
        }

        private Point<double> NormalizeVector(Point<double> vector)
        {
            var len = GetVectorLen(vector);
            return new Point<double>(vector.X / len, vector.Y / len, vector.Z / len);
        }

        private double GetVectorLen(Point<double> vector)
        {
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        private double GetAnotherCathetus(double angleDegree, double knownCathetus)
        {
            var radian = angleDegree * Math.PI / 180;
            var tangent = Math.Tan(radian);
            return knownCathetus / tangent;
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
    }
}