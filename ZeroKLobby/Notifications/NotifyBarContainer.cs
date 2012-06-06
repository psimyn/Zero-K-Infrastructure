using System;
using System.Windows.Forms;
using System.Drawing;

namespace ZeroKLobby.Notifications
{
    public partial class NotifyBarContainer: UserControl
    {
        public INotifyBar BarContent { get; protected set; }
        public NotifyBarContainer() {}

        public NotifyBarContainer(INotifyBar barContent)
        {
            BarContent = barContent;
            InitializeComponent();
            btnDetail.Click += detail_Click;
            btnStop.Click += stop_Click;
            btnStop.Image = Resources.Remove;
            var control = barContent.GetControl();
            
            //-- code for countering the 'DPI scaling cause bar to grow infinitely big'. The bar grew larger because each time the bar was created the bar is scaled to high-DPI, but since old height is used again for new bar: the bar get bigger & bigger after each scaling.
            Graphics formGraphics = this.CreateGraphics(); //Reference: http://msdn.microsoft.com/en-us/library/system.drawing.graphics.dpix.aspx
            float formDPIvertical = formGraphics.DpiY; //get current DPI
            float scaleDownRatio = 96 / formDPIvertical; //get scaleDown ratio (to counter-act DPI virtualization/scaling)
            //--
            float newHeight = control.Height + 8; //get old height + margin
            newHeight = newHeight*scaleDownRatio; //multiply the scaledown to old height

            Height = (int) newHeight; // apply height (the margin will again be automatically be scaled up according to Window's DPI) 
            tableLayoutPanel1.Controls.Add(control, 1, 0);
            control.Dock = DockStyle.Fill;
            Dock = DockStyle.Top;
            BarContent.AddedToContainer(this);
        }


        protected virtual void detail_Click(object sender, EventArgs e)
        {
            BarContent.DetailClicked(this);
        }


        protected virtual void stop_Click(object sender, EventArgs e)
        {
            BarContent.CloseClicked(this);
        }
    }
}