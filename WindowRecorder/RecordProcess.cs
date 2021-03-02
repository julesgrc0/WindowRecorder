using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;
using System.Reflection;

namespace WindowRecorder
{
    class RecordProcess
    {
        private IntPtr process;
        public bool isRecording { get; private set; }  = false;
        private string outDir;
        public string tempFolderDir { get; private set; } = string.Empty;
        public string VideoName { get; private set; } = string.Empty;
        private Thread record;
        private int FPS = 24;
        private int Vquality = 23;
        private string ffmpegLocation;

        public RecordProcess(IntPtr process, string outDir, string videoName = "video", int quality = 23)
        {
            this.process = process;
            this.outDir = outDir;
            this.VideoName = videoName;

            if (quality <= 26 || quality >= 0)
            {
                this.Vquality = quality;
            }

            this.ffmpegLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ffmpeg.exe");
            this.tempFolderDir = Path.Combine(Path.GetTempPath(), this.VideoName);
        }

        public void SetTempImagesDir(string temp)
        {
            this.tempFolderDir = temp;
        }


        public void SetFps(int fps)
        {
            this.FPS = fps;
        }

        public int GetFps()
        {
            return this.FPS;
        }

        public void SetFFmpegLocation(string path)
        {
            this.ffmpegLocation = path;
        }

        public string GetFFmpegLocation()
        {
            return this.ffmpegLocation;
        }

        public void Start()
        {
            if (!this.isRecording)
            {
                this.StartRecord();
            }
        }

        public void Stop()
        {
            if (this.isRecording)
            {
                this.record.Abort();
                this.isRecording = false;
                this.Execute(this.tempFolderDir);
            }
        }

        private void StartRecord()
        {
            if (!Directory.Exists(tempFolderDir))
            {
                try
                {
                    Directory.CreateDirectory(this.tempFolderDir);
                    this.record = new Thread(() =>
                    {
                        int index = 0;
                        while (true)
                        {
                            string imageLocation = Path.Combine(this.tempFolderDir, "img-" + index + ".jpg");
                            Bitmap img = ProcessContent.GetWindowContent(this.process);
                            img.Save(imageLocation, ImageFormat.Jpeg);

                            this.max_height = Math.Max(this.max_height, img.Height);
                            this.max_width = Math.Max(this.max_width, img.Width);

                            Thread.Sleep(1000 / FPS);
                            index++;
                        }
                    });
                    this.record.Start();
                    this.isRecording = true;
                }
                catch { }
            }
        }

        private int max_height = 1;
        private int max_width = 1;

        private void Execute(string dir)
        {
            new Thread(() =>
            {
                this.max_height = (int)Math.Floor((decimal)(this.max_height / 10)) * 10;
                this.max_width = (int)Math.Floor((decimal)(this.max_width / 10)) * 10;

                string cmd = "/c cd " + dir + " &&  "+this.ffmpegLocation+" -i \"img-%d.jpg\" -r " + FPS + " -c:v libx264 -preset slow -crf " + this.Vquality + " -s "+ max_width + "x"+ max_height + " " + this.VideoName + ".mp4";

                using (var process = new Process())
                {
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");
                    process.StartInfo.Arguments = cmd;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }

                string VideoLocation = Path.Combine(dir,this.VideoName+".mp4");
                if(File.Exists(VideoLocation))
                {
                    File.Copy(VideoLocation, Path.Combine(this.outDir,this.VideoName+".mp4"),true);
                    if(Directory.Exists(dir))
                    {
                        Directory.Delete(dir,true);
                    }
                }
            }).Start();   
        }
    }
}
