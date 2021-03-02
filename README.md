# WindowRecorder


> Example code

```cs
using System;
using WindowRecorderLib;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Threading;


Process[] processlist = Process.GetProcesses(); // list all running process

foreach (Process process in processlist)
{
    if (!string.IsNullOrEmpty(process.MainWindowTitle))
    {
        new Thread(() => 
        {
            string outDir =  Environment.GetFolderPath(Environment.SpecialFoldMyPictures);
            Inptr Handle = process.MainWindowHandle;
            string VideoName = process.Id.ToString();

            RecordProcess rec = new RecordProcess(Handle, outDir, VideoName); 

            rec.Start();    // start recording
            Thread.Sleep(5000); // wait 5s
            rec.Stop(); // end recording

            Bitmap bmp = ProcessContent.GetWindowContent(process.MainWindowHandle); // take picture from handle
            bmp.Save(Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),process.Id+".bmp")); // save the image
        }).Start();  
    }
}

```
