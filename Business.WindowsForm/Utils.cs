using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Business.WindowsForm
{
    public static class Utils
    {
        public static void Invoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        public static T SetMouseStyle<T>(T control, Color downColor, Color upColor, bool cursorHand = true)
            where T : Control
        {
            control.MouseEnter += (sender, e) => (sender as Control).BackColor = upColor;// Color.LightSkyBlue;
            control.MouseLeave += (sender, e) => (sender as Control).BackColor = Color.Transparent;
            control.MouseDown += (sender, e) => (sender as Control).BackColor = downColor;//Color.DeepSkyBlue;
            control.MouseUp += (sender, e) => (sender as Control).BackColor = upColor;//Color.LightSkyBlue;

            if (cursorHand)
            {
                control.Cursor = Cursors.Hand;
            }

            return control;
        }

        public static void Enableds(this Control control, bool enabled)
        {
            foreach (Control item in control.Controls)
            {
                item.Enabled = enabled;
            }
        }

        public static void Enableds(bool enabled, params Control[] control)
        {
            if (!(control?.Any() ?? false)) { return; }

            foreach (Control item in control)
            {
                item.Enabled = enabled;
            }
        }

        public static void SetCenterScreen(this Form form, int? x = null, int? y = null)
        {
            var screen = Screen.FromControl(form);
            var secondScreen = Screen.AllScreens.FirstOrDefault(c => !c.Equals(screen)) ?? screen;
            form.Location = new Point((secondScreen.Bounds.Width - form.Width) / 2 + (x ?? 0), (secondScreen.Bounds.Height - form.Height) / 2 + (y ?? 0));
        }

        public static Icon ToIcon(this Image img, int size)
        {
            using var msImg = new MemoryStream();
            img.Save(msImg, System.Drawing.Imaging.ImageFormat.Png);
            using var msIco = new MemoryStream();
            using var bw = new BinaryWriter(msIco);
            bw.Write((short)0);           //0-1 reserved
            bw.Write((short)1);           //2-3 image type, 1 = icon, 2 = cursor
            bw.Write((short)1);           //4-5 number of images
            bw.Write((byte)size);         //6 image width
            bw.Write((byte)size);         //7 image height
            bw.Write((byte)0);            //8 number of colors
            bw.Write((byte)0);            //9 reserved
            bw.Write((short)0);           //10-11 color planes
            bw.Write((short)32);          //12-13 bits per pixel
            bw.Write((int)msImg.Length);  //14-17 size of image data
            bw.Write(22);                 //18-21 offset of image data
            bw.Write(msImg.ToArray());    // write image data
            bw.Flush();
            bw.Seek(0, SeekOrigin.Begin);
            return new Icon(msIco);
        }

        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //public static extern bool SetProcessDPIAware();
    }
}
