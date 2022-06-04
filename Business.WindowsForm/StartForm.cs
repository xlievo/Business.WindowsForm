namespace Business.WindowsForm
{
    public class StartForm : System.Windows.Forms.Form
    {
        public StartForm()
        {
            SuspendLayout();
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            TopMost = true;
            ShowIcon = ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            BackColor = TransparencyKey = System.Drawing.Color.White;
            DoubleBuffered = true;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(350, 80);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
