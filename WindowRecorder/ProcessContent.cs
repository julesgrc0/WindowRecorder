using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WindowRecorder
{
    class ProcessContent
    {
        [Flags()]
        private enum DeviceContextValues : uint
        {
            Window = 0x00000001,
            Cache = 0x00000002,
            NoResetAttrs = 0x00000004,
            ClipChildren = 0x00000008,
            ClipSiblings = 0x00000010,
            ParentClip = 0x00000020,
            ExcludeRgn = 0x00000040,
            IntersectRgn = 0x00000080,
            ExcludeUpdate = 0x00000100,
            IntersectUpdate = 0x00000200,
            LockWindowUpdate = 0x00000400,
            Validate = 0x00200000,
        }

        private const uint PW_RENDERFULLCONTENT = 0x00000002;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, DeviceContextValues flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static Bitmap GetWindowContent(string WindowName)
        {
            IntPtr hwnd = FindWindow(null, WindowName);
            return GetWindowContent(hwnd);
        }

        public static Bitmap GetWindowContent(IntPtr hwnd)
        {
            Bitmap bmp = new Bitmap(1, 1);
            RECT rect;
            if (GetWindowRect(new HandleRef(null, hwnd), out rect))
            {
                if ((rect.Right - rect.Left) != 0 && (rect.Bottom - rect.Top) != 0)
                {
                    bmp = new Bitmap((rect.Right - rect.Left), (rect.Bottom - rect.Top));
                    Graphics memoryGraphics = Graphics.FromImage(bmp);

                    IntPtr dc = memoryGraphics.GetHdc();
                    PrintWindow(hwnd, dc, PW_RENDERFULLCONTENT);
                    memoryGraphics.ReleaseHdc(dc);
                }
            }

            return bmp;
        }
    }
}
