namespace Business.WindowsForm
{
    /// <summary>
    /// Manages a related set of tab pages.
    /// </summary>
    [System.Drawing.ToolboxBitmap(typeof(System.Windows.Forms.TabControl))]
    public class TabControl : System.Windows.Forms.TabControl
    {
        readonly NativeTabControl nativeTabControl = new();

        public TabControl()
        {
            nativeTabControl.AssignHandle(Handle);

            ItemSize = new System.Drawing.Size(ItemSize.Width, 30);
        }

        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e)
        {
            if (e.Control is System.Windows.Forms.TabPage page)
            {
                page.Padding = new System.Windows.Forms.Padding(0);
            }

            base.OnControlAdded(e);
        }

        protected override void OnSelectedIndexChanged(System.EventArgs e)
        {
            base.OnSelectedIndexChanged(e);

            if (-1 < SelectedIndex)
            {
                TabPages[SelectedIndex].Focus();
            }
        }

        private class NativeTabControl : System.Windows.Forms.NativeWindow
        {
            protected override void WndProc(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == TCM_ADJUSTRECT)
                {
                    RECT rc = (RECT)m.GetLParam(typeof(RECT));
                    //Adjust these values to suit, dependant upon Appearance
                    rc.Top -= 3;

                    rc.Left -= 4;
                    rc.Right += 4;
                    rc.Bottom += 4;
                    System.Runtime.InteropServices.Marshal.StructureToPtr(rc, m.LParam, true);
                }
                base.WndProc(ref m);
            }

            private const int TCM_FIRST = 0x1300;
            private const int TCM_ADJUSTRECT = (TCM_FIRST + 40);

            private struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

        }
    }
}
