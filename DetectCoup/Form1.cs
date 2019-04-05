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
using System.Diagnostics;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;


namespace DetectCoup
{
    /*public class CoupJSON
    {
        public string ImageName { get; set; }
        public CoupJSON Region { get; set; }
        public CoupJSON Main { get; set; }
        public string TopLeft { get; set; }
        public string BotRight { get; set; }
        public CoupJSON Alternative { get; set; }
        public string Center { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public CoupJSON EachPoint { get; set; }
        public string Comment { get; set; }
        public string Point { get; set; }
    }*/

    






    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        static string NameF;
        static string PathF;
        static bool cart = false;
        static bool bStop = false;
        static float x = 0;
        static float y = 0;
        static float width = 0;
        static float height = 0;

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Изображения (*.jpg; *.bmp; *.png)|*.jpg;*.bmp;*.png|Видеофайлы (*.avi; *mp4)|*.avi;*.mp4" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    //Удаление временных файлов
                    if (System.IO.Directory.Exists(@"temp") == true)
                    {
                        Directory.Delete(@"temp", true);
                    }
                    if (System.IO.File.Exists(@"temp.jpg") == true)
                    {
                        File.Delete(@"temp.jpg");
                    }
                    if (System.IO.File.Exists(@"tempWB.jpg") == true)
                    {
                        File.Delete(@"tempWB.jpg");
                    }
                    PathF = (string)ofd.FileName;
                    //Определение расширения файла
                    NameF = (string)ofd.SafeFileName;
                    var NameType = NameF.Substring(NameF.Length - 3, 3);

                    if ((NameType == ("jpg")) || (NameType == ("bmp")) || (NameType == ("png")))
                    {
                        pic.Image = Image.FromFile(ofd.FileName);
                        cart = true;
                    }
                    else
                    //Вставка в pictureBox первого кадра видео
                    {
                        cart = false;
                        var capture = new VideoCapture(ofd.FileName);
                        int sleepTime = (int)Math.Round(1000 / capture.Fps);
                        Mat image = new Mat();
                        while (true)
                        {
                            capture.Read(image);
                            if (image.Empty())
                            {
                                break;
                            }

                            {
                                if (System.IO.File.Exists(@"temp/temp.jpg") == true)
                                {
                                    break;
                                }

                                else
                                //Создание временной директории и сохранение первого кадра
                                {
                                    Directory.CreateDirectory(@"temp");
                                    using (FileStream fstream = new FileStream(@"temp/temp.jpg", FileMode.Create))
                                    {
                                        ImageConverter converter = new ImageConverter();
                                        byte[] bTemp = (byte[])converter.ConvertTo(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image), typeof(byte[]));
                                        fstream.Write(bTemp, 0, bTemp.Length);
                                    }
                                    pic.Image = Image.FromFile(@"temp/temp.jpg");
                                }
                            }
                            Cv2.WaitKey(sleepTime);
                        }

                    }
                }
            }
        }

        private void btnDetect_Click_1(object sender, EventArgs e)
        {
            //Проверка типа файла для распознавания
            if (cart == true)  //Для изображений
            {
                var configurationDetector = new ConfigurationDetector();
                var config = configurationDetector.Detect();
                using (var yoloWrapper = new YoloWrapper(config))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //Конвертация цветного изображения в черно-белое
                        var picWB = pic.Image;
                        picWB.Save(@"temp.jpg");
                        Mat src = new Mat(@"temp.jpg", ImreadModes.Grayscale);
                        ImageConverter converter = new ImageConverter();
                        byte[] WbTemp = (byte[])converter.ConvertTo(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src), typeof(byte[]));
                        using (FileStream fstream = new FileStream(@"tempWB.jpg", FileMode.Create))
                        {
                            fstream.Write(WbTemp, 0, WbTemp.Length);
                        }
                        //Без использования using файл tempWB.jpg занят процессом и не может быть удален
                        using (var WB = Image.FromFile(@"tempWB.jpg"))
                        {
                            WB.Save(ms, ImageFormat.Png);
                            var items = yoloWrapper.Detect(ms.ToArray());
                            yoloItemBindingSource.DataSource = items;
                            foreach (var item in items)
                            {
                                //Запись в лог
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
                                //Определение параметров для отображения сцепки и рисование рамки
                                x = (float)item.X;
                                y = (float)item.Y;
                                width = (float)item.Width;
                                height = (float)item.Height;
                                if ((double)item.Confidence > 0.85)
                                {
                                    if (item.Type == "coup1")
                                    {
                                        Graphics graph = Graphics.FromImage(pic.Image);
                                        Pen pen = new Pen(Color.Blue, 3f);
                                        graph.DrawRectangle(pen, x, y, width, height);
                                        pic.Image = pic.Image;
                                        using (StreamWriter w = File.AppendText("log.txt"))
                                        {
                                            Log($"Обнаружена полусцепка: {NameF} | X: {x} | Y: {y} | width: {width} | height: {height} | Confidence: {item.Confidence}", w);
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
                                            Log($"Обнаружена сцепка: {NameF} | X: {x} | Y: {y} | width: {width} | height: {height} | Confidence: {item.Confidence}", w);
                                        }
                                        /*CoupJSON image = new CoupJSON();
                                        image.ImageName = ($"{NameF}");
                                        CoupJSON TL = new CoupJSON();
                                        TL.TopLeft = new List<int> { $"{ x }", $"{ y }" };
                                        */

                                    }

                                        Directory.CreateDirectory(@"JSON");
                                        var CoupJSON = GetCoupJSON();
                                        var jsonToWrite = JsonConvert.SerializeObject(CoupJSON, Formatting.Indented);
                                        using (var writer = new StreamWriter(@"JSON/"+ NameF+ ".json"))
                                        {
                                            writer.Write(jsonToWrite);
                                        }
                                 }
                                
                            }

                        }
                        
                    }
                }
            }
            else //Для видео
            {
                var capture = new VideoCapture(PathF);
                int sleepTime = (int)Math.Round(1000 / capture.Fps);
                int count = 0;
                Mat image = new Mat();
                while (true)
                {
                    count++;
                    capture.Read(image);
                    if (image.Empty())
                    {
                        break;
                    }
                    capture.Read(image);

                    if (bStop == true)   //Действие на кнопку Стоп
                    { 
                        bStop = false;
                        break;
                    }
                    //Сохранение временных файлов, начиная с 1 кадра
                    using (FileStream fstream = new FileStream(@"temp/temp" + count + ".jpg", FileMode.Create))
                    {
                        ImageConverter converter = new ImageConverter();
                        byte[] bTemp = (byte[])converter.ConvertTo(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image), typeof(byte[]));
                        fstream.Write(bTemp, 0, bTemp.Length);
                    }
                    pic.Image = Image.FromFile(@"temp/temp" + count + ".jpg");  //Передача кадра в pictureBox 
                    var configurationDetector = new ConfigurationDetector();
                    var config = configurationDetector.Detect();
                    using (var yoloWrapper = new YoloWrapper(config))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            pic.Image.Save(ms, ImageFormat.Png);    //Загрузка кадра из pictureBox в Yolo
                            var items = yoloWrapper.Detect(ms.ToArray());
                            yoloItemBindingSource.DataSource = items;
                            foreach (var item in items)
                            {
                                var x = (float)item.X;
                                var y = (float)item.Y;
                                var width = (float)item.Width;
                                var height = (float)item.Height;
                                if ((double)item.Confidence > 0.85)
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
                    Cv2.WaitKey(sleepTime);
                }
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            Process.Start("log.txt");


        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            bStop = true;
        }

        private CoupJSON GetCoupJSON()
        {
            var CoupJSON = new CoupJSON
            {
                ImageName = NameF,
                Region = new List<Region>
                    {
                        new Region
                        {
                            Main = new List<Main>
                            {
                                new Main
                                {
                                   TopLeft = (x+","+y),
                                   BotRight = ((x+width)+","+(y+height))
                                }

                            },

                            Alternative = new List<Alternative>
                            {
                                new Alternative
                                {
                                   Center = ((x+(width/2))+","+(y+(height/2))),
                                   Width = (width+""),
                                   Height = (height+"")
                                }

                            },

                            EachPoint = new List<EachPoint>
                            {
                                new EachPoint
                                {
                                    ComPoints = new ComPoints
                                    {
                                        Comment = "top-left, (x;y)",
                                        Point = (x+","+y)
                                    },
                                },
                                new EachPoint
                                {
                                    ComPoints = new ComPoints
                                    {
                                        Comment = "top-right, (x;y)",
                                        Point = ((x+width)+","+y)
                                    },
                                },
                                new EachPoint
                                {
                                    ComPoints = new ComPoints
                                    {
                                        Comment = "bottom-right, (x;y)",
                                        Point = ((x+width)+","+(y+height))
                                    }
                                },
                                new EachPoint
                                {
                                    ComPoints = new ComPoints
                                    {
                                        Comment = "bottom-left, (x;y)",
                                        Point = (x+","+(y+height))
                                    }
                                }
                            }
                            }
                        }

            };

            return CoupJSON;
        }

        private void btnJSON_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", @"JSON");

        }
    }
    public class CoupJSON
    {
        public string ImageName { get; set; }
        public List<Region> Region { get; set; }
    }

    public class Region
    {
        public List<Main> Main { get; set; }
        public List<Alternative> Alternative { get; set; }
        public List<EachPoint> EachPoint { get; set; }
    }

    public class Main
    {
        public string TopLeft { get; set; }
        public string BotRight { get; set; }
    }

    public class Alternative
    {
        public string Center { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }

    public class EachPoint
    {
        public ComPoints ComPoints { get; set; }
    }

    public class ComPoints
    {
        public string Comment { get; set; }
        public string Point { get; set; }
    }
}


//items[0].Type -> "Person, Car, ..."
//items[0].Confidence -> 0.0 (low) -> 1.0 (high)
//items[0].X -> bounding box
//items[0].Y -> bounding box
//items[0].Width -> bounding box
//items[0].Height -> bounding box

/*var capture = new VideoCapture(filename);
richTextBox1.Text += filename+"\n";
int sleepTime = (int)Math.Round(1000 / capture.Fps);
// Frame image buffer
Mat image = new Mat();        // When the movie playback reaches end, Mat.data becomes NULL.
while (true)
{
    richTextBox1.Text += "Цикл"+ "\n";
    capture.Read(image);
    if (image.Empty())
    {
        richTextBox1.Text += "Цикл1" + "\n";
        break;
    }

    // Tried this way but nothing is shown
    count++;
    ImageConverter converter = new ImageConverter();
    byte[] bTemp = (byte[])converter.ConvertTo(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image), typeof(byte[]));
    //richTextBox1.Text += "Кадр номер " + count+ yoloC.ImgDec(bTemp)+"\n";


    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);

    //    window.ShowImage(image);   <==  this has no issue 

    Cv2.WaitKey(sleepTime);            }*/
