using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Business.WindowsForm
{
    readonly struct WindowState
    {
        public WindowState(FormWindowState state, Point location)
        {
            State = state;
            Location = location;
        }

        public FormWindowState State { get; }

        public Point Location { get; }
    }

    public class MainForm : Form
    {
        WindowState windowState;
        Point mousePoint;

        const int TITLEBUTWIDTH = 31;
        const int TITLEBUTHEIGHT = 29;

        const int BORDERSIZE = 5;
        const int HTLEFT = 10;
        const int HTRIGHT = 11;
        const int HTTOP = 12;
        const int HTTOPLEFT = 13;
        const int HTTOPRIGHT = 14;
        const int HTBOTTOM = 15;
        const int HTBOTTOMLEFT = 0x10;
        const int HTBOTTOMRIGHT = 17;

        readonly Panel titleBlock = new() { Height = 30, BackColor = Color.Transparent, Dock = DockStyle.Top };
        readonly Label icoBox = new() { Anchor = AnchorStyles.Left, BackColor = Color.Transparent, Size = new Size(TITLEBUTWIDTH, TITLEBUTHEIGHT), Location = new Point(0, 0) };
        readonly Label closeBox = new() { Anchor = AnchorStyles.Right, BackColor = Color.Transparent, Size = new Size(TITLEBUTWIDTH, TITLEBUTHEIGHT) };
        readonly Label maximizeBox = new() { Anchor = AnchorStyles.Right, BackColor = Color.Transparent, Size = new Size(TITLEBUTWIDTH, TITLEBUTHEIGHT) };
        readonly Label minimizeBox = new() { Anchor = AnchorStyles.Right, BackColor = Color.Transparent, Size = new Size(TITLEBUTWIDTH, TITLEBUTHEIGHT) };
        readonly Label titleText = new() { Dock = DockStyle.Top, AutoSize = false, TextAlign = ContentAlignment.BottomCenter, UseCompatibleTextRendering = false, Text = string.Empty, BackColor = Color.Transparent };

        public MainForm()
        {
            SuspendLayout();

            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            windowState = new WindowState(WindowState, Location);
            Padding = new Padding(BORDERSIZE);
            Text = "MainForm";
            Controls.Add(titleBlock);

            //titleText.Font = new Font(titleText.Font, FontStyle.Bold);

            titleBlock.Controls.AddRange(new Control[] { icoBox, closeBox, maximizeBox, minimizeBox, titleText });

            //closeBox.BackColor = maximizeBox.BackColor = minimizeBox.BackColor = Color.Red;

            closeBox.Location = new Point(titleBlock.Width - (TITLEBUTWIDTH * 1), 0);
            maximizeBox.Location = new Point(titleBlock.Width - (TITLEBUTWIDTH * 2), 0);
            minimizeBox.Location = new Point(titleBlock.Width - (TITLEBUTWIDTH * 3), 0);
            //titleText.Location = new Point((titleBlock.Width / 2) - TITLEBUTWIDTH, BORDERSIZE - 1);

            titleBlock.SizeChanged += (object sender, EventArgs e) =>
            {
                using var g = CreateGraphics();
                Gradient(g, titleBlock.ClientRectangle, TitleColor);
                titleBlock.Refresh();
            };
            titleBlock.Paint += (object sender, PaintEventArgs e) => { Gradient(e.Graphics, titleBlock.ClientRectangle, TitleColor); e.Dispose(); };

            WindowChanged(titleBlock);
            WindowChanged(titleText);
            WindowChanged(icoBox);

            SetMouseStyle();
            closeBox.Click += (object sender, EventArgs e) => Close();
            maximizeBox.Click += (object sender, EventArgs e) => WindowStateChanged();
            minimizeBox.Click += (object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

            ResumeLayout(false);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Focus();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            using var g = CreateGraphics();
            DrawBorder(g, ClientRectangle, TitleColor);
            Refresh();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            DrawBorder(e.Graphics, ClientRectangle, TitleColor);
            e.Dispose();
        }

        void WindowChanged(Control control)
        {
            control.MouseDoubleClick += (object sender, MouseEventArgs e) => WindowStateChanged();
            control.MouseDown += (object sender, MouseEventArgs e) => mousePoint = e.Location;
            control.MouseMove += (object sender, MouseEventArgs e) =>
            {
                if (MouseButtons.Left != e.Button) { return; }
                SetDesktopLocation(Location.X + e.X - mousePoint.X, Location.Y + e.Y - mousePoint.Y);
            };
        }

        void SetMouseStyle()
        {
            SetMouseStyle(closeBox, titleDownColor, titleUpColor);
            SetMouseStyle(maximizeBox, titleDownColor, titleUpColor);
            SetMouseStyle(minimizeBox, titleDownColor, titleUpColor);
        }

        protected override void WndProc(ref Message m) => BorderProc(ref m);

        static void DrawBorder(Graphics g, Rectangle rect, Color color)
        {
            if (0 == rect.Width || 0 == rect.Height) { return; }

            ControlPaint.DrawBorder(g, rect,
                 color, BORDERSIZE, ButtonBorderStyle.Solid, //Left
                 color, BORDERSIZE, ButtonBorderStyle.Solid, //Top
                 color, BORDERSIZE, ButtonBorderStyle.Solid, //Right
                 color, BORDERSIZE, ButtonBorderStyle.Solid);//Bottom
        }

        static void Gradient(Graphics g, Rectangle rect, Color color)
        {
            if (0 == rect.Width || 0 == rect.Height) { return; }

            //Color.LightSkyBlue
            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, color, Color.Transparent, System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(brush, rect);
        }

        public void WindowStateChanged()
        {
            if (FormWindowState.Maximized != WindowState)
            {
                windowState = new WindowState(WindowState, Location);
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = windowState.State;
                Location = windowState.Location;
            }
        }

        public static Label SetMouseStyle(Label label, Color downColor, Color upColor, bool cursorHand = true)
        {
            label.MouseEnter += (sender, e) => (sender as Label).BackColor = upColor;// Color.LightSkyBlue;
            label.MouseLeave += (sender, e) => (sender as Label).BackColor = Color.Transparent;
            label.MouseDown += (sender, e) => (sender as Label).BackColor = downColor;//Color.DeepSkyBlue;
            label.MouseUp += (sender, e) => (sender as Label).BackColor = upColor;//Color.LightSkyBlue;

            if (cursorHand)
            {
                label.Cursor = Cursors.Hand;
            }

            return label;
        }

        public void BorderProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0084:
                    base.WndProc(ref m);

                    if (FormWindowState.Maximized == WindowState) { break; }

                    var vPoint = PointToClient(new((int)m.LParam & 0xFFFF, (int)m.LParam >> 16 & 0xFFFF));

                    if (vPoint.X <= BORDERSIZE)
                    {
                        if (vPoint.Y <= BORDERSIZE)
                        {
                            m.Result = (IntPtr)HTTOPLEFT;
                        }
                        else if (vPoint.Y >= ClientSize.Height - BORDERSIZE)
                        {
                            m.Result = (IntPtr)HTBOTTOMLEFT;
                        }
                        else
                        {
                            m.Result = (IntPtr)HTLEFT;
                        }
                    }
                    else if (vPoint.X >= ClientSize.Width - BORDERSIZE)
                    {
                        if (vPoint.Y <= BORDERSIZE)
                        {
                            m.Result = (IntPtr)HTTOPRIGHT;
                        }
                        else if (vPoint.Y >= ClientSize.Height - BORDERSIZE)
                        {
                            m.Result = (IntPtr)HTBOTTOMRIGHT;
                        }
                        else
                        {
                            m.Result = (IntPtr)HTRIGHT;
                        }
                    }
                    else if (vPoint.Y <= BORDERSIZE)
                    {
                        m.Result = (IntPtr)HTTOP;
                    }
                    else if (vPoint.Y >= ClientSize.Height - BORDERSIZE)
                    {
                        m.Result = (IntPtr)HTBOTTOM;
                    }
                    break;
                case 0x0201://MouseDown Left
                    if (FormWindowState.Maximized == WindowState) { break; }

                    m.Msg = 0x00A1;//Change the message to non customer area press the mouse
                    m.LParam = IntPtr.Zero;//Default value
                    m.WParam = new IntPtr(2);//Place the mouse in the title bar
                    base.WndProc(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [SettingsBindable(true)]
        public override string Text { get => base.Text; set { base.Text = value; titleText.Text = value; } }

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        [AmbientValue(null)]
        [System.Runtime.InteropServices.DispId(-512)]
        [Localizable(true)]
        public override Font Font { get => base.Font; set { base.Font = value; titleText.Font = value; Padding = new Padding(BORDERSIZE); } }

        Color titleColor = Color.SkyBlue;
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color TitleColor { get => titleColor; set { titleColor = value; Refresh(); } }

        Color titleDownColor = Color.DeepSkyBlue;
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color TitleDownColor
        {
            get => titleDownColor; set { titleDownColor = value; SetMouseStyle(); Refresh(); }
        }

        Color titleUpColor = Color.LightSkyBlue;
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color TitleUpColor
        {
            get => titleUpColor; set { titleUpColor = value; SetMouseStyle(); Refresh(); }
        }

        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Image IcoBoxImage { get => icoBox.Image; set => icoBox.Image = value; }

        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Image CloseBoxImage { get => closeBox.Image; set => closeBox.Image = value; }

        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Image MaximizeBoxImage { get => maximizeBox.Image; set => maximizeBox.Image = value; }

        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Image MinimizeBoxImage { get => minimizeBox.Image; set => minimizeBox.Image = value; }
    }
}
