using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class VS2013BlueSplitterControl : DockPane.SplitterControlBase
    {
        private readonly SolidBrush _brush;

        public VS2013BlueSplitterControl(DockPane pane)
            : base(pane)
        {
            this._brush = new SolidBrush(pane.DockPanel.Skin.AutoHideStripSkin.DockStripBackground.StartColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle rect = this.ClientRectangle;

            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            switch (this.Alignment)
            {
                case DockAlignment.Right:
                case DockAlignment.Left:
                    {
                        e.Graphics.FillRectangle(this._brush, rect.X, rect.Y,
                                                         Measures.SplitterSize, rect.Height);
                    }
                    break;
                case DockAlignment.Bottom:
                case DockAlignment.Top:
                    {
                        e.Graphics.FillRectangle(this._brush, rect.X, rect.Y, rect.Width, Measures.SplitterSize);
                    }
                    break;
            }
        }
    }
}