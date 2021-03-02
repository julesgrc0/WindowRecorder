using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Threading;


namespace WindowRecorder
{
    public partial class MainWindow : Form
    {


        public MainWindow()
        {
            InitializeComponent();
            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle))
                {
                    new Thread(() =>
                    {
                        RecordProcess rec = new RecordProcess(process.MainWindowHandle, @"C:\Users\jules\OneDrive\Images\Pellicule", process.Id.ToString());
                        rec.Start();
                        Thread.Sleep(5000);
                        rec.Stop();
                    }).Start();

                    Bitmap bmp = ProcessContent.GetWindowContent(process.MainWindowHandle);
                    bmp.Save(@"C:\Users\jules\OneDrive\Images\Pellicule\" + process.Id + ".bmp");
                }
            }

            this.RenderImages("Pellicule");
        }

        private List<Tuple<Image, string>> images = new List<Tuple<Image, string>>();
        private int index = 0;

        private void RenderImages(string ImageFolder)
        {
            string images = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string dir = Path.Combine(images, ImageFolder);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }


            foreach (string file in Directory.GetFiles(dir))
            {
                if (Path.GetExtension(file) == ".bmp")
                {
                    Image img = Image.FromFile(file);
                    Tuple<Image, string> dataImage = new Tuple<Image, string>(img, file);
                    this.images.Add(dataImage);
                }
            }

            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox1.Image = this.images[index].Item1;
        }

        private void UpdateIndex()
        {
            if (this.images.Count > 0)
            {
                index++;

                if (index >= this.images.Count)
                {
                    this.index = 0;
                }

                this.pictureBox1.Image = this.images[index].Item1;
                this.label1.Text = "Index: " + (this.index + 1).ToString();
            }
            else
            {
                this.pictureBox1.Image = new Bitmap(1, 1);
                this.label1.Text = "Index: 0";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.UpdateIndex();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.images.Count > 0)
            {
                string deletePath = this.images[this.index].Item2;
                this.images[this.index].Item1.Dispose();
                this.images.RemoveAt(this.index);

                try
                {
                    File.Delete(deletePath);
                }
                catch (IOException er)
                {
                    Console.WriteLine(er.Message);
                }

            }
            this.UpdateIndex();
        }
    }
}

