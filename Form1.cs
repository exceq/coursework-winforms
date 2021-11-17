using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CourseworkWinforms
{
    public partial class Form1 : Form
    {
        private double scale = 4;
        private int offsetX = 0;
        private int offsetY = 0;

        private CancellationTokenSource lastToken;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DrawInNewThread();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta < 0)
                scale += 0.5;
            else if (scale > 0.5)
                scale -= 0.5;
            DrawInNewThread();
        }

        void NewFunction(CancellationTokenSource token)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }

            PlyReader plyReader = new PlyReader("1.ply");
            Pen pen = new Pen(Color.Red, 1);

            Graphics gr = pictureBox1.CreateGraphics();
            gr.Clear(Color.LightGray);
            var points = plyReader.ReadFile();
            var i = 0;
            foreach (var point in points)
            {
                if (token.IsCancellationRequested)
                    return;
                i++;
                if (i % 5 != 0)
                    continue;
                gr.DrawRectangle(pen, (int) (point.X / scale) + offsetX + 700, -(int) (point.Z / scale) + offsetY+200, 1, 1);
            }
        }

        private int lastX;
        private int lastY;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            lastX = e.X;
            lastY = e.Y;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            offsetX = e.X - lastX;
            offsetY = e.Y - lastY;
            DrawInNewThread();
        }

        private void DrawInNewThread()
        {
            lastToken?.Cancel();
            CancellationTokenSource token = new CancellationTokenSource();
            Task.Factory.StartNew(() => NewFunction(token));
            lastToken = token;
        }
    }
}