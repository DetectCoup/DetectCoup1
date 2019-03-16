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
using FFMpegSharp;
using FFMpegSharp.FFMPEG;
using FFMpegSharp.Extend;
using AForge;
using AForge.Video;
using System.Diagnostics;


namespace DetectCoup
{
    

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        static string NameF;

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "JPG, BMP, PNG (*.jpg; *.bmp; *.png)|*.jpg; *.BMP; *.png | Видеофайлы (*.avi; *mp4)|*.avi;*.mp4" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    NameF = (string)ofd.SafeFileName;
                    var NameType = NameF.Substring(NameF.Length - 3, 3);


                    if ((NameType == ("jpg")) || (NameType == ("bmp")) || (NameType == ("png")))
                        pic.Image = Image.FromFile(ofd.FileName);
                    else
                     {
                        FFMpeg encoder = new FFMpeg();

                        string inputFile = "D:/YandexDisk/Малленом/nexus_project/Составы_сцепки/S.Day.0610.8vX2/sort_20181025061007_1.avi";
                        FileInfo output = new FileInfo("D:/DetectCoup/1.png");

                        var video = VideoInfo.FromPath(inputFile);

                        new FFMpeg()
                            .Snapshot(
                                video,
                                output,
                                new Size(200, 400),
                                TimeSpan.FromMinutes(0)
                            );






                    }
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
                        using (StreamWriter w = File.AppendText("log.txt"))
                        {

                        }
                        void Log(string logMessage, TextWriter w)
                        {
                            w.Write("\r\nLog Entry : ");
                            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                            w.WriteLine("  :");
                            w.WriteLine($"  :{logMessage}");
                            w.WriteLine("-------------------------------");
                        }
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
                                using (StreamWriter w = File.AppendText("log.txt"))
                                {
                                    Log($"Обнаружена полусцепка: {NameF} | X: {x} | Y: {y} | width: {width} | height: {height}", w);
                                }
                            }
                            if (item.Type == "coup2")
                            {
                                Graphics graph = Graphics.FromImage(pic.Image);
                                Pen pen = new Pen(Color.Red, 3f);
                                graph.DrawRectangle(pen, x, y, width, height);
                                pic.Image = pic.Image;
                                using (StreamWriter w = File.AppendText("log.txt"))
                                {
                                    Log($"Обнаружена сцепка: {NameF} | X: {x} | Y: {y} | width: {width} | height: {height}", w);
                                }

                            }
                        }
                    }

                }
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            Process.Start("log.txt");


        }

    }
}
