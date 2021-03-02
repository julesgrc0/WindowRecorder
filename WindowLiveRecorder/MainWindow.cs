using System;
using System.Windows.Forms;
using WindowRecorder;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace WindowLiveRecorder
{
    public partial class MainWindow : Form
    {
        private RecordProcess rec;
        private IntPtr actualwindow;
        private bool isRunning = true;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public MainWindow()
        {
            InitializeComponent();
            this.FormClosed += MainWindow_FormClosed;
            
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            Process[] processlist = Process.GetProcesses(); 

            foreach (Process process in processlist)
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.ProcessName == "chrome")
                {

                    this.actualwindow = process.MainWindowHandle;
                }
            }
            
            rec = new RecordProcess(actualwindow, Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
            Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(this.isRunning)
            {
                this.isRunning = false;
                rec.Stop();
            }
        }

        private void StartLive()
        {
            this.isRunning = true;
            new Thread(() =>
            {
                while (this.isRunning)
                {
                    this.pictureBox1.Image = ProcessContent.GetWindowContent(actualwindow);
                    Thread.Sleep(1000 / rec.GetFps());
                }
            }).Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.StartLive();
            rec.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.isRunning = false;
            rec.Stop();
        }
    }
}
