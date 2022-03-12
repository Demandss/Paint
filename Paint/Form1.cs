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
        Selection,
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
        public Bitmap patternImage = default(Bitmap);
        Graphics graphics = default(Graphics);
        Pen pen = new Pen(Color.Black);
        Pen eraser = new Pen(Color.White, 10);
        Color color = Color.Black;
        Point prevPoint = default(Point);
        Point currentPoint = default(Point);
        bool isMousePressed = false;
        Tool currentTool = Tool.Line;
        Stack<Bitmap> drawHistory = new Stack<Bitmap>();

        private Point firstSelectionPoint, lastSelectionPoint;
        private Bitmap selectedImage;
        private Graphics selectedGraphics;
        private Rectangle selectedRectangle;
        private static Form1 INSTANCE;

        public Form1()
        {
            InitializeComponent();
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            pictureBox1.Image = bitmap;
            graphics.Clear(Color.White);
            saveDrawInHistory(new Bitmap(bitmap));
            INSTANCE = this;
        }

        public static Form1 get() {return INSTANCE;}

        private void saveDrawInHistory(Bitmap bitmap)
        {
            drawHistory.Push(bitmap);
            /* TODO - кода нибудь
            if (drawHistory.Count < 5) return;
            var array = drawHistory.ToArray();
            array.Skip(Math.Max(0,array.Length-5));
            drawHistory = new Stack<Bitmap>(array);*/
        }

        private void CopyToClipboard(Rectangle src_rect)
        {
            Bitmap bm = new Bitmap(src_rect.Width, src_rect.Height);

            using (Graphics gr = Graphics.FromImage(bm))
            {
                Rectangle dest_rect =
                    new Rectangle(0, 0, src_rect.Width, src_rect.Height);
                gr.DrawImage(bitmap, dest_rect, src_rect,
                    GraphicsUnit.Pixel);
            }
            Clipboard.SetImage(bm);
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
                        int pw2 = (int)Math.Max(1, pen.Width / 2);
                        using (var brush = new SolidBrush(pen.Color))
                        {
                            graphics.FillRectangle(brush, currentPoint.X - pw2, currentPoint.Y - pw2, pen.Width, pen.Width);
                        }
                        //graphics.DrawLine(pen, prevPoint, currentPoint);
                        break;
                    case Tool.Eraser:
                        prevPoint = currentPoint;
                        currentPoint = e.Location;
                        eraser.Width = (float) numericUpDown1.Value;
                        pw2 = (int)Math.Max(1, pen.Width / 2);
                        using (var brush = new SolidBrush(eraser.Color))
                        {
                            graphics.FillRectangle(brush, currentPoint.X - pw2, currentPoint.Y - pw2, eraser.Width, eraser.Width);
                        }
                        break;
                    case Tool.Circle:
                        currentPoint = e.Location;
                        break;
                    case Tool.Selection:
                        lastSelectionPoint = e.Location;

                        selectedGraphics.DrawImage(bitmap, 0, 0);

                        using (Pen pen = new Pen(Color.Red))
                        {
                            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                            Rectangle rectangle = new Rectangle(
                                            Math.Min(firstSelectionPoint.X, lastSelectionPoint.X),
                                            Math.Min(firstSelectionPoint.Y, lastSelectionPoint.Y),
                                            Math.Abs(firstSelectionPoint.X - lastSelectionPoint.X),
                                            Math.Abs(firstSelectionPoint.Y - lastSelectionPoint.Y));
                            selectedGraphics.DrawRectangle(pen, rectangle);
                        }
                        pictureBox1.Refresh();
                        break;
                    default:
                        break;
                }
                pictureBox1.Refresh();
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            prevPoint = e.Location;
            currentPoint = e.Location;
            isMousePressed = true;
            if (currentTool != Tool.Pipette)
            {
                saveDrawInHistory(new Bitmap(bitmap));
            }
            if (currentTool == Tool.Selection)
            {
                firstSelectionPoint = e.Location;
                
                selectedImage = new Bitmap(bitmap);
                selectedGraphics = Graphics.FromImage(selectedImage);
                pictureBox1.Image = selectedImage;
            }
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
                    textBox.Size = new Size(50, (int)numericUpDown1.Value * 2);
                    textBox.Font = new Font("Microsoft Sans Serif", pen.Width, FontStyle.Regular, GraphicsUnit.Point, 204);
                    textBox.Multiline = true;
                    textBox.KeyDown += (o, args) =>
                    {
                        if (args.KeyCode == Keys.Enter)
                        {
                            graphics.DrawString(textBox.Text, textBox.Font, new SolidBrush(color),
                                new RectangleF(textBox.Location.X, textBox.Location.Y, textBox.Right * (int)numericUpDown1.Value * 2, textBox.Height));
                            pictureBox1.Refresh();
                            Controls.Remove(textBox);
                        }
                    };
                    textBox.Leave += (o, args) =>
                    {
                        graphics.DrawString(textBox.Text, textBox.Font, new SolidBrush(color),
                            new RectangleF(textBox.Location.X, textBox.Location.Y, textBox.Right * (int)numericUpDown1.Value * 2, textBox.Height));
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
                case Tool.Selection:
                    selectedImage = null;
                    selectedGraphics = null;
                    pictureBox1.Image = bitmap;
                    pictureBox1.Refresh();

                    Rectangle rectangle = new Rectangle(
                                            Math.Min(firstSelectionPoint.X, lastSelectionPoint.X),
                                            Math.Min(firstSelectionPoint.Y, lastSelectionPoint.Y),
                                            Math.Abs(firstSelectionPoint.X - lastSelectionPoint.X),
                                            Math.Abs(firstSelectionPoint.Y - lastSelectionPoint.Y));
                    if ((rectangle.Width > 0) && (rectangle.Height > 0))
                    {
                        selectedRectangle = rectangle;
                    }
                    break;
                default:
                    break;
            }
            prevPoint = e.Location;
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
                saveDrawInHistory(new Bitmap(bitmap));
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
            saveDrawInHistory(new Bitmap(bitmap));
            graphics.Clear(Color.White);
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
            if (drawHistory.Count < 1) return;
            bitmap = drawHistory.Pop();   
            graphics = Graphics.FromImage(bitmap);
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }

        private void отменаВыделенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Stop selecting.
            selectedImage = null;
            selectedGraphics = null;
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((selectedRectangle.Width < 1) && (selectedRectangle.Height < 1))
            {
                MessageBox.Show("Невозможно копировать, выделите область копирования!");
                return;
            }                
            CopyToClipboard(selectedRectangle);
            selectedRectangle.Width = 0;
            selectedRectangle.Height = 0;
            System.Media.SystemSounds.Beep.Play();
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyToClipboard(selectedRectangle);

            using (Graphics gr = Graphics.FromImage(bitmap))
            {
                using (SolidBrush br = new SolidBrush(pictureBox1.BackColor))
                {
                    gr.FillRectangle(br, selectedRectangle);
                }
            }

            selectedImage = new Bitmap(bitmap);
            pictureBox1.Image = selectedImage;

            selectedImage = null;
            selectedGraphics = null;

            System.Media.SystemSounds.Beep.Play();
        }

        private void вставкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage()) return;

            Image clipboard_image = Clipboard.GetImage();

            int cx = selectedRectangle.X +
                (selectedRectangle.Width - clipboard_image.Width) / 2;
            int cy = selectedRectangle.Y +
                (selectedRectangle.Height - clipboard_image.Height) / 2;
            Rectangle dest_rect = new Rectangle(
                cx, cy,
                clipboard_image.Width,
                clipboard_image.Height);

            using (Graphics gr = Graphics.FromImage(bitmap))
            {
                gr.DrawImage(clipboard_image, dest_rect);
            }

            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();

            selectedImage = null;
            selectedGraphics = null;
        }
        
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            graphics.Clear(Color.White);
            pictureBox1.Refresh();
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult exitMessageResult = MessageBox.Show("Вы уверены что хотите закрыть приложение, не сохранив изображение","Внимание",MessageBoxButtons.YesNo);
            if (exitMessageResult == DialogResult.No) e.Cancel = true;
        }

        private void шаблоныToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Selection;
        }
    }
}
