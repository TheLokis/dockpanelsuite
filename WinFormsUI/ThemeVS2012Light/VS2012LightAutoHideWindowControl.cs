using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class VS2012LightAutoHideWindowControl : DockPanel.AutoHideWindowControl
    {
        private class VS2012LightAutoHideWindowSplitterControl : SplitterBase
        {
            private static readonly SolidBrush _horizontalBrush = new SolidBrush(Color.FromArgb(0xFF, 204, 206, 219));
            private static readonly Color[] _verticalSurroundColors = new[] { SystemColors.Control };

            public VS2012LightAutoHideWindowSplitterControl(DockPanel.AutoHideWindowControl autoHideWindow)
            {
                this.AutoHideWindow = autoHideWindow;
            }

            private DockPanel.AutoHideWindowControl AutoHideWindow { get; set; }

            protected override int SplitterSize
            {
                get { return Measures.SplitterSize; }
            }

            protected override void StartDrag()
            {
                this.AutoHideWindow.DockPanel.BeginDrag(this.AutoHideWindow, this.AutoHideWindow.RectangleToScreen(this.Bounds));
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Rectangle rect = this.ClientRectangle;

                if (rect.Width <= 0 || rect.Height <= 0)
                    return;

                switch (this.AutoHideWindow.DockState)
                {
                    case DockState.DockRightAutoHide:
                    case DockState.DockLeftAutoHide:
                        {
                            using (var path = new GraphicsPath())
                            {
                                path.AddRectangle(rect);
                                using (var brush = new PathGradientBrush(path)
                                    {
                                        CenterColor = Color.FromArgb(0xFF, 204, 206, 219), SurroundColors = _verticalSurroundColors
                                    })
                                {
                                    e.Graphics.FillRectangle(brush, rect.X + Measures.SplitterSize / 2 - 1, rect.Y,
                                                             Measures.SplitterSize / 3, rect.Height);
                                }
                            }
                        }
                        break;
                    case DockState.DockBottomAutoHide:
                    case DockState.DockTopAutoHide:
                        {
                            e.Graphics.FillRectangle(_horizontalBrush, rect.X, rect.Y,
                                            rect.Width, Measures.SplitterSize);
                        }
                        break;
                }
            }
        }

        public VS2012LightAutoHideWindowControl(DockPanel dockPanel) : base(dockPanel)
        {
            this.m_splitter = new VS2012LightAutoHideWindowSplitterControl(this);
            this.Controls.Add(this.m_splitter);
        }

        protected override Rectangle DisplayingRectangle
        {
            get
            {
                Rectangle rect = this.ClientRectangle;

                // exclude the border and the splitter
                if (this.DockState == DockState.DockBottomAutoHide)
                {
                    rect.Y += Measures.SplitterSize;
                    rect.Height -= Measures.SplitterSize;
                }
                else if (this.DockState == DockState.DockRightAutoHide)
                {
                    rect.X += Measures.SplitterSize;
                    rect.Width -= Measures.SplitterSize;
                }
                else if (this.DockState == DockState.DockTopAutoHide)
                    rect.Height -= Measures.SplitterSize;
                else if (this.DockState == DockState.DockLeftAutoHide)
                    rect.Width -= Measures.SplitterSize;

                return rect;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.DockPadding.All = 0;
            if (this.DockState == DockState.DockLeftAutoHide)
            {
                //DockPadding.Right = 2;
                this.m_splitter.Dock = DockStyle.Right;
            }
            else if (this.DockState == DockState.DockRightAutoHide)
            {
                //DockPadding.Left = 2;
                this.m_splitter.Dock = DockStyle.Left;
            }
            else if (this.DockState == DockState.DockTopAutoHide)
            {
                //DockPadding.Bottom = 2;
                this.m_splitter.Dock = DockStyle.Bottom;
            }
            else if (this.DockState == DockState.DockBottomAutoHide)
            {
                //DockPadding.Top = 2;
                this.m_splitter.Dock = DockStyle.Top;
            }

            Rectangle rectDisplaying = this.DisplayingRectangle;
            Rectangle rectHidden = new Rectangle(-rectDisplaying.Width, rectDisplaying.Y, rectDisplaying.Width, rectDisplaying.Height);
            foreach (Control c in this.Controls)
            {
                DockPane pane = c as DockPane;
                if (pane == null)
                    continue;


                if (pane == this.ActivePane)
                    pane.Bounds = rectDisplaying;
                else
                    pane.Bounds = rectHidden;
            }

            base.OnLayout(levent);
        }
    }
}