using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paint
{ 
    enum Tool
    {
        Line,
        Rectangle,
        Pen,
        Text,
        Eraser,
        Circle,
        Clear,
        Fill,
        Pipette
    }
    public partial class Form1 : Form
    {
        Bitmap bitmap = default(Bitmap);
        Graphics graphics = default(Graphics);
        Pen pen = new Pen(Color.Black);
        Pen eraser = new Pen(Color.White, 10);
        Color color = default(Color);
        Point prevPoint = default(Point);
        Point currentPoint = default(Point);
        bool isMousePressed = false;
        Tool currentTool = Tool.Line;
        List<Graphics> drawHistory = new List<Graphics>();
        
        public Form1()
        {
            InitializeComponent();
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;           
            
            pictureBox1.Image = bitmap;
            graphics.Clear(Color.White);
            drawHistory.Add(graphics);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Line;
        }

        Rectangle GetMRectangle(Point pPoint, Point cPoint)
        {
            return new Rectangle
            {
                X = Math.Min(pPoint.X, cPoint.X),
                Y = Math.Min(pPoint.Y, cPoint.Y),
                Width = Math.Abs(pPoint.X - cPoint.X),
                Height = Math.Abs(pPoint.Y - cPoint.Y)
            };
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel1.Text = e.Location.ToString();
            if (isMousePressed)
            {
                switch (currentTool)
                {
                    case Tool.Line:
                    case Tool.Rectangle:
                        currentPoint = e.Location;
                        break;
                    case Tool.Pen:
                        prevPoint = currentPoint;
                        currentPoint = e.Location;
                        graphics.DrawLine(pen, prevPoint, currentPoint);
                        break;
                    case Tool.Eraser:
                        prevPoint = currentPoint;
                        currentPoint = e.Location;
                        eraser.Width = (float) numericUpDown1.Value;
                        graphics.DrawLine(eraser, prevPoint, currentPoint);
                        break;
                    case Tool.Circle:
                        currentPoint = e.Location;
                        break;
                    default:
                        break;
                }
                pictureBox1.Refresh();
                drawHistory.Add(graphics);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            prevPoint = e.Location;
            currentPoint = e.Location;
            isMousePressed = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isMousePressed = false;

            switch (currentTool)
            {
                case Tool.Line:
                    graphics.DrawLine(pen, prevPoint, currentPoint);
                    break;
                case Tool.Rectangle:
                    graphics.DrawRectangle(pen, GetMRectangle(prevPoint, currentPoint));
                    break;
                case Tool.Pen:
                    break;
                case Tool.Text:
                    TextBox textBox = new TextBox();
                    textBox.Location = currentPoint;
                    textBox.Size = new Size(50, (int)pen.Width);
                    textBox.Font = new Font("Microsoft Sans Serif", pen.Width, FontStyle.Regular, GraphicsUnit.Point, 204);
                    textBox.KeyDown += (o, args) =>
                    {
                        if (args.KeyCode == Keys.Enter)
                        {
                            graphics.DrawString(textBox.Text, textBox.Font, new SolidBrush(color),
                                new RectangleF(textBox.Location.X, textBox.Location.Y, textBox.TextLength * (int)pen.Width, textBox.Height));
                            pictureBox1.Refresh();
                            Controls.Remove(textBox);
                        }
                    };
                    textBox.Leave += (o, args) =>
                    {
                        graphics.DrawString(textBox.Text, textBox.Font, new SolidBrush(color),
                            new RectangleF(textBox.Location.X, textBox.Location.Y, textBox.TextLength * (int)pen.Width, textBox.Height));
                        pictureBox1.Refresh();
                        Controls.Remove(textBox);
                    };
                    Controls.Add(textBox);
                    textBox.Select();
                    textBox.BringToFront();
                    break;
                case Tool.Circle:
                    graphics.DrawEllipse(pen, GetMRectangle(prevPoint, currentPoint));
                    break;
                case Tool.Fill:
                    MapFill mf = new MapFill();
                    mf.Fill(graphics, currentPoint, pen.Color, ref bitmap);
                    graphics = Graphics.FromImage(bitmap);
                    pictureBox1.Image = bitmap;
                    pictureBox1.Refresh();
                    break;
                case Tool.Pipette:
                    currentPoint = e.Location;
                    Color pixelColor = bitmap.GetPixel(currentPoint.X, currentPoint.Y);
                    pen.Color = pixelColor;
                    button7.BackColor = pixelColor;
                    /*pictureBox1.Refresh();*/
                    break;
                default:
                    break;
            }
            prevPoint = e.Location;
            drawHistory.Add(graphics);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            switch (currentTool)
            {
                case Tool.Line:
                    e.Graphics.DrawLine(pen, prevPoint, currentPoint);
                    break;
                case Tool.Rectangle:
                    e.Graphics.DrawRectangle(pen, GetMRectangle(prevPoint, currentPoint));
                    break;
                case Tool.Pen:
                    break;
                case Tool.Circle:
                    e.Graphics.DrawEllipse(pen, GetMRectangle(prevPoint, currentPoint));
                    break;
                default:
                    break;
            }
            drawHistory.Add(graphics);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Rectangle;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Pen;
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void файлToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                bitmap = Bitmap.FromFile(openFileDialog1.FileName) as Bitmap;
                pictureBox1.Image = bitmap;
                graphics = Graphics.FromImage(bitmap);
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        bitmap.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        bitmap.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        bitmap.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }
                fs.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Eraser;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Circle;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            graphics.Clear(Color.White);
            drawHistory.Add(graphics);
            pictureBox1.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DialogResult colorResult = colorDialog1.ShowDialog();
            if (colorResult == DialogResult.OK)
            {
                color = colorDialog1.Color;
                pen.Color = color;
                button7.BackColor = color;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            pen.Width = float.Parse(numericUpDown1.Value.ToString());
        }

        private void button9_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Fill;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Pipette;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Text;
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void отменадействияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawHistory.RemoveAt(drawHistory.Count - 1);
            //Console.WriteLine(drawHistory);
            graphics = drawHistory.Last();
            pictureBox1.Refresh();
            //Console.WriteLine(pictureBox1.Image == drawHistory.Last());
        }
    }
}
