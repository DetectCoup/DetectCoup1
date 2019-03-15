using Alturos.Yolo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Alturos.Yolo.Model;


namespace DetectCoup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "JPG, BMP, PNG (*.jpg; *.bmp; *.png)|*.jpg; *.BMP; *.png | Видеофайлы (*.avi; *mp4)|*.avi;*.mp4" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string NameF = (string)ofd.SafeFileName;
                    NameF = NameF.Substring(NameF.Length - 3, 3);


                    if ((NameF == ("jpg")) || (NameF == ("bmp")) || (NameF == ("png")))
                        pic.Image = Image.FromFile(ofd.FileName);

                }
            }
        }

                //items[0].Type -> "Person, Car, ..."
                //items[0].Confidence -> 0.0 (low) -> 1.0 (high)
                //items[0].X -> bounding box
                //items[0].Y -> bounding box
                //items[0].Width -> bounding box
                //items[0].Height -> bounding box
            

        

        private void btnDetect_Click_1(object sender, EventArgs e)
        {

            var configurationDetector = new ConfigurationDetector();
            var config = configurationDetector.Detect();
            using (var yoloWrapper = new YoloWrapper(config))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pic.Image.Save(ms, ImageFormat.Png);
                    var items = yoloWrapper.Detect(ms.ToArray());
                    yoloItemBindingSource.DataSource = items;
                    foreach (var item in items)
                    {
                        var x = (float)item.X;
                        var y = (float)item.Y;
                        var width = (float)item.Width;
                        var height = (float)item.Height;
                        if ((double)item.Confidence > 0.9)
                        {
                            if (item.Type == "coup1")
                            {
                                Graphics graph = Graphics.FromImage(pic.Image);
                                Pen pen = new Pen(Color.Blue, 3f);
                                graph.DrawRectangle(pen, x, y, width, height);
                                pic.Image = pic.Image;
                            }
                            if (item.Type == "coup2")
                            {
                                Graphics graph = Graphics.FromImage(pic.Image);
                                Pen pen = new Pen(Color.Red, 3f);
                                graph.DrawRectangle(pen, x, y, width, height);
                                pic.Image = pic.Image;
                            }
                        }
                    }

                }
            }
        }
    }
}
