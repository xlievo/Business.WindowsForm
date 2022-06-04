using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Linq;

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
        static readonly System.Resources.ResourceManager resourceMan = new("Business.WindowsForm.MainForm", typeof(MainForm).Assembly);
        static readonly Icon microsoft_icon = resourceMan.GetObject("microsoft_icon") as Icon;
        static readonly Image microsoft_img = resourceMan.GetObject("microsoft_png") as Image;

        readonly Size maximumSize;

        WindowState windowState;
        Point mousePoint;

        const int TITLEBUTWIDTH = 31;
        const int TITLEBUTHEIGHT = 28;

        const int BORDERSIZE = 5;
        const int HTLEFT = 10;
        const int HTRIGHT = 11;
        const int HTTOP = 12;
        const int HTTOPLEFT = 13;
        const int HTTOPRIGHT = 14;
        const int HTBOTTOM = 15;
        const int HTBOTTOMLEFT = 0x10;
        const int HTBOTTOMRIGHT = 17;

        public readonly Panel TitleBlock = new() { Height = TITLEBUTHEIGHT, BackColor = Color.Transparent, Dock = DockStyle.Top, TabStop = false };
        public readonly FlowLayoutPanel ControlBlock = new() { WrapContents = false, FlowDirection = FlowDirection.RightToLeft, Height = TITLEBUTHEIGHT, BackColor = Color.Transparent, Dock = DockStyle.Right };

        readonly PictureBox iconBox = new() { Padding = new Padding(0), Anchor = AnchorStyles.Left, Dock = DockStyle.Left, BackColor = Color.Transparent, Size = new Size(TITLEBUTWIDTH, TITLEBUTHEIGHT), Location = new Point(0, 0), SizeMode = PictureBoxSizeMode.Zoom };
        readonly Label titleText = new() { Margin = new Padding(0), Dock = DockStyle.Fill, AutoSize = false, TextAlign = ContentAlignment.TopCenter, UseCompatibleTextRendering = false, Text = string.Empty, BackColor = Color.Transparent };

        readonly PictureBox closeBox = new() { Margin = new Padding(1, 0, 0, 0), Padding = new Padding(3), Anchor = AnchorStyles.Right, BackColor = Color.Transparent, Size = new Size(TITLEBUTWIDTH, TITLEBUTHEIGHT), SizeMode = PictureBoxSizeMode.CenterImage };
        readonly PictureBox maximizeBox = new() { Margin = new Padding(1, 0, 0, 0), Padding = new Padding(3), Anchor = AnchorStyles.Right, BackColor = Color.Transparent, Size = new Size(TITLEBUTWIDTH, TITLEBUTHEIGHT), SizeMode = PictureBoxSizeMode.CenterImage };
        readonly PictureBox minimizeBox = new() { Margin = new Padding(0, 0, 0, 0), Padding = new Padding(3), Anchor = AnchorStyles.Right, BackColor = Color.Transparent, Size = new Size(TITLEBUTWIDTH, TITLEBUTHEIGHT), SizeMode = PictureBoxSizeMode.CenterImage };

        readonly Control[] controlBoxs;

        public MainForm()
        {
            SuspendLayout();

            maximumSize = Screen.FromHandle(Handle).WorkingArea.Size;

            iconBox.Image = microsoft_img;
            base.Icon = microsoft_icon;

            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
            base.FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;

            windowState = new WindowState(WindowState, Location);
            //Padding = new Padding(BORDERSIZE);
            Text = "MainForm";
            Controls.Add(TitleBlock);

            //titleText.Font = new Font(titleText.Font, FontStyle.Bold);

            controlBoxs = new Control[] { closeBox, maximizeBox, minimizeBox };
            ControlBlock.Controls.AddRange(controlBoxs);
            //titleBlockControls = new Control[] { iconBox, titleText, minimizeBox, maximizeBox, closeBox };
            TitleBlock.Controls.AddRange(new Control[] { titleText, iconBox, ControlBlock });

            //closeBox.BackColor = maximizeBox.BackColor = minimizeBox.BackColor = Color.Red;

            SetTitleBoxWidth();

            TitleBlock.SizeChanged += (object sender, EventArgs e) =>
            {
                using var g = CreateGraphics();
                Gradient(g, TitleBlock.ClientRectangle, TitleColor);
                TitleBlock.Refresh();
            };
            TitleBlock.Paint += (object sender, PaintEventArgs e) => { Gradient(e.Graphics, TitleBlock.ClientRectangle, TitleColor); e.Dispose(); };

            WindowChanged(TitleBlock);
            WindowChanged(titleText);
            WindowChanged(iconBox);
            WindowChanged(ControlBlock);

            SetMouseStyle();

            closeBox.Click += (object sender, EventArgs e) =>
            {
                if (e is MouseEventArgs mouse && MouseButtons.Left == mouse.Button)
                {
                    Close();
                }
            };
            maximizeBox.Click += (object sender, EventArgs e) =>
            {
                if (e is MouseEventArgs mouse && MouseButtons.Left == mouse.Button)
                {
                    WindowStateChanged();
                }
            };
            minimizeBox.Click += (object sender, EventArgs e) =>
            {
                if (e is MouseEventArgs mouse && MouseButtons.Left == mouse.Button)
                {
                    WindowState = FormWindowState.Minimized;
                }
            };

            Shown += (object sender, EventArgs e) => SetTitleBoxWidth();

            ResumeLayout(false);
            PerformLayout();
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
            DrawBorder(g, ClientRectangle, TitleColor, BorderSize);
            Refresh();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            DrawBorder(e.Graphics, ClientRectangle, TitleColor, BorderSize);
            e.Dispose();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;
                var cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;
                return cp;
            }
        }

        void WindowChanged(Control control)
        {
            control.MouseDoubleClick += (object sender, MouseEventArgs e) =>
            {
                if (MouseButtons.Left != e.Button) { return; }
                if (FixedWindow) { return; }
                WindowStateChanged();
            };
            WindowMove(control);
        }

        void SetMouseStyle()
        {
            Utils.SetMouseStyle(closeBox, titleDownColor, titleUpColor);
            Utils.SetMouseStyle(maximizeBox, titleDownColor, titleUpColor);
            Utils.SetMouseStyle(minimizeBox, titleDownColor, titleUpColor);

            TitleBlock.Refresh();
        }

        void SetTitleHeight()
        {
            TitleBlock.Height = TitleHeight;
            closeBox.Height = TitleHeight;
            maximizeBox.Height = TitleHeight;
            minimizeBox.Height = TitleHeight;

            TitleBlock.Refresh();
        }

        void SetTitleBoxWidth()
        {
            iconBox.Width = TitleBoxWidth;
            closeBox.Width = TitleBoxWidth;
            maximizeBox.Width = TitleBoxWidth;
            minimizeBox.Width = TitleBoxWidth;

            iconBox.Visible = ShowIcon;

            if (!ControlBox)
            {
                ControlBlock.Visible = closeBox.Visible = maximizeBox.Visible = minimizeBox.Visible = ControlBox;
            }
            else
            {
                maximizeBox.Visible = MaximizeBox;
                minimizeBox.Visible = MinimizeBox;
                ControlBlock.Visible = closeBox.Visible = ControlBox;
            }

            /*
            closeBox.Location = new Point(TitleBlock.Width - (TitleBoxWidth * 1), 0);

            if (!MaximizeBox && MinimizeBox)
            {
                minimizeBox.Location = new Point(TitleBlock.Width - (TitleBoxWidth * 2), 0);
            }
            else
            {
                maximizeBox.Location = new Point(TitleBlock.Width - (TitleBoxWidth * 2), 0);
                minimizeBox.Location = new Point(TitleBlock.Width - (TitleBoxWidth * 3), 0);
            }
            */

            int controlBlockWidth = 0;

            foreach (Control item in ControlBlock.Controls)
            {
                if (!item.Visible) { continue; }
                controlBlockWidth += item.Width + item.Margin.Left + item.Margin.Right;
            }

            ControlBlock.Width = controlBlockWidth;

            base.Padding = new Padding(BorderSize);

            TitleBlock.Refresh();
        }

        protected override void WndProc(ref Message m) => BorderProc(ref m);

        static void DrawBorder(Graphics g, Rectangle rect, Color color, int size)
        {
            if (0 == rect.Width || 0 == rect.Height) { return; }

            ControlPaint.DrawBorder(g, rect,
                 color, size, ButtonBorderStyle.Solid, //Left
                 color, size, ButtonBorderStyle.Solid, //Top
                 color, size, ButtonBorderStyle.Solid, //Right
                 color, size, ButtonBorderStyle.Solid);//Bottom
        }

        static void Gradient(Graphics g, Rectangle rect, Color color)
        {
            if (0 == rect.Width || 0 == rect.Height) { return; }

            //Color.LightSkyBlue
            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, color, Color.Transparent, System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(brush, rect);
        }

        bool mouseDown;

        public void WindowMove(Control control)
        {
            control.MouseDown += (object sender, MouseEventArgs e) =>
            {
                if (MouseButtons.Left != e.Button) { return; }
                mousePoint = e.Location;
                mouseDown = true;
            };
            control.MouseUp += (object sender, MouseEventArgs e) =>
            {
                if (MouseButtons.Left != e.Button || !mouseDown) { return; }
                mouseDown = false;
            };
            control.MouseMove += (object sender, MouseEventArgs e) =>
            {
                if (!mouseDown) { return; }
                SetDesktopLocation(Location.X + e.X - mousePoint.X, Location.Y + e.Y - mousePoint.Y);
            };
        }

        public void WindowMove(ToolStripItem control)
        {
            control.MouseDown += (object sender, MouseEventArgs e) =>
            {
                mousePoint = e.Location;
            };
            control.MouseMove += (object sender, MouseEventArgs e) =>
            {
                if (MouseButtons.Left != e.Button) { return; }
                SetDesktopLocation(Location.X + e.X - mousePoint.X, Location.Y + e.Y - mousePoint.Y);
            };
        }

        public void WindowStateChanged()
        {
            if (FormWindowState.Maximized != WindowState)
            {
                if (MaximumSize.IsEmpty || MaximumSize.Height > maximumSize.Height || MaximumSize.Width > maximumSize.Width)
                {
                    MaximumSize = maximumSize;
                }

                windowState = new WindowState(WindowState, Location);
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = windowState.State;
                Location = windowState.Location;
            }
        }

        public void TitleBlockAdd(params Control[] control)
        {
            if (control is null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            if (!(control?.Any() ?? false))
            {
                return;
            }

            var controls = ControlBlock.Controls.Cast<Control>();
            var except = controls.Except(controlBoxs).ToArray();
            ControlBlock.Controls.Clear();
            //controlBlock.Controls.AddRange(new Control[] { titleText, iconBox });
            ControlBlock.Controls.AddRange(controlBoxs);
            ControlBlock.Controls.AddRange(control);
            if (0 < except.Length)
            {
                ControlBlock.Controls.AddRange(except);
            }

            //SetTitleBoxWidth();
        }

        public virtual void BorderProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0084:
                    base.WndProc(ref m);

                    if (FixedWindow)
                    {
                        break;
                    }

                    if (FormWindowState.Maximized == WindowState) { break; }

                    var vPoint = PointToClient(new((int)m.LParam & 0xFFFF, (int)m.LParam >> 16 & 0xFFFF));

                    if (vPoint.X <= BorderSize)
                    {
                        if (vPoint.Y <= BorderSize)
                        {
                            m.Result = (IntPtr)HTTOPLEFT;
                        }
                        else if (vPoint.Y >= ClientSize.Height - BorderSize)
                        {
                            m.Result = (IntPtr)HTBOTTOMLEFT;
                        }
                        else
                        {
                            m.Result = (IntPtr)HTLEFT;
                        }
                    }
                    else if (vPoint.X >= ClientSize.Width - BorderSize)
                    {
                        if (vPoint.Y <= BorderSize)
                        {
                            m.Result = (IntPtr)HTTOPRIGHT;
                        }
                        else if (vPoint.Y >= ClientSize.Height - BorderSize)
                        {
                            m.Result = (IntPtr)HTBOTTOMRIGHT;
                        }
                        else
                        {
                            m.Result = (IntPtr)HTRIGHT;
                        }
                    }
                    else if (vPoint.Y <= BorderSize)
                    {
                        m.Result = (IntPtr)HTTOP;
                    }
                    else if (vPoint.Y >= ClientSize.Height - BorderSize)
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

        FormBorderStyle formBorderStyle;
        /// <summary>
        /// Gets or sets the border style of the form.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(FormBorderStyle.None)]
        [System.Runtime.InteropServices.DispId(-504)]
        public new FormBorderStyle FormBorderStyle { get => formBorderStyle; set { formBorderStyle = value; } }

        int borderSize = BORDERSIZE;
        /// <summary>
        /// Border thickness size.
        /// </summary>
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int BorderSize { get => borderSize; set { borderSize = value; SetTitleBoxWidth(); } }

        Padding padding;
        /// <summary>
        /// Gets or sets padding within the control.
        /// </summary>
        //[Localizable(true)]
        [Browsable(false)]
        public new Padding Padding { get => padding; set => padding = value; }

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
        public override Font Font { get => base.Font; set { base.Font = value; SetTitleHeight(); SetTitleBoxWidth(); } }

        [AmbientValue(null)]
        [System.Runtime.InteropServices.DispId(-512)]
        [Localizable(true)]
        public Font TitleFont { get => titleText.Font; set { titleText.Font = value; SetTitleHeight(); SetTitleBoxWidth(); } }

        /// <summary>
        /// Gets or sets a value indicating whether a control box is displayed in the caption bar of the form.
        /// </summary>
        [DefaultValue(true)]
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool ControlBox { get => base.ControlBox; set { base.ControlBox = value; SetTitleBoxWidth(); } }

        /// <summary>
        /// Gets or sets a value indicating whether an icon is displayed in the caption bar of the form.
        /// </summary>
        [DefaultValue(true)]
        public new bool ShowIcon { get => base.ShowIcon; set { base.ShowIcon = value; SetTitleBoxWidth(); } }

        /// <summary>
        /// Whether the border is fixed or not. It cannot be adjusted.
        /// </summary>
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool FixedWindow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Maximize button is displayed in the caption bar of the form.
        /// </summary>
        [DefaultValue(true)]
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool MaximizeBox { get => base.MaximizeBox; set { base.MaximizeBox = value; SetTitleBoxWidth(); } }

        /// <summary>
        /// Gets or sets a value indicating whether the Minimize button is displayed in the caption bar of the form.
        /// </summary>
        [DefaultValue(true)]
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool MinimizeBox { get => base.MinimizeBox; set { base.MinimizeBox = value; SetTitleBoxWidth(); } }

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
            get => titleDownColor; set { titleDownColor = value; SetMouseStyle(); }
        }

        Color titleUpColor = Color.LightSkyBlue;
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color TitleUpColor
        {
            get => titleUpColor; set { titleUpColor = value; SetMouseStyle(); }
        }

        //[Localizable(true)]
        //[Browsable(true)]
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public new Image Icon
        //{
        //    get => iconBox.Image ?? default;
        //    set
        //    {
        //        iconBox.Image?.Dispose();
        //        base.Icon?.Dispose();

        //        if (null == value) { value = default; }

        //        iconBox.Image = value;
        //        base.Icon = value?.ToIcon(32);

        //        TitleBlock.Refresh();
        //    }
        //}

        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Image Icon
        {
            get => iconBox.Image ?? microsoft_img;
            set
            {
                if (null != iconBox.Image && !microsoft_img.Equals(iconBox.Image))
                {
                    iconBox.Image.Dispose();
                }

                if (null != base.Icon && !microsoft_icon.Equals(base.Icon))
                {
                    base.Icon.Dispose();
                }

                if (null == value)
                {
                    iconBox.Image = microsoft_img;
                    base.Icon = microsoft_icon;
                }
                else
                {
                    iconBox.Image = value;
                    base.Icon = value.ToIcon(32);
                }

                TitleBlock.Refresh();
            }
        }

        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Image CloseBoxImage { get => closeBox.Image ?? default; set { closeBox.Image?.Dispose(); closeBox.Image = value; } }

        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Image MaximizeBoxImage { get => maximizeBox.Image ?? default; set { maximizeBox.Image?.Dispose(); maximizeBox.Image = value; } }

        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Image MinimizeBoxImage { get => minimizeBox.Image ?? default; set { minimizeBox.Image?.Dispose(); minimizeBox.Image = value; } }

        int titleHeight = TITLEBUTHEIGHT;
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int TitleHeight { get => titleHeight; set { titleHeight = value; SetTitleHeight(); } }

        int titleBoxWidth = TITLEBUTWIDTH;
        [Localizable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int TitleBoxWidth { get => titleBoxWidth; set { titleBoxWidth = value; SetTitleBoxWidth(); } }
    }
}
