using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    using WeifenLuo.WinFormsUI.ThemeVS2012Light;

    /// <summary>
    /// Visual Studio 2013 Light theme.
    /// </summary>
    public class VS2013BlueTheme : ThemeBase
    {
        /// <summary>
        /// Applies the specified theme to the dock panel.
        /// </summary>
        /// <param name="dockPanel">The dock panel.</param>
        public override void Apply(DockPanel dockPanel)
        {
            if (dockPanel == null)
            {
                throw new NullReferenceException("dockPanel");
            }

            Measures.SplitterSize = 6;
            dockPanel.Extender.DockPaneCaptionFactory = new VS2013BlueDockPaneCaptionFactory();
            dockPanel.Extender.AutoHideStripFactory = new VS2013BlueAutoHideStripFactory();
            dockPanel.Extender.AutoHideWindowFactory = new VS2013BlueAutoHideWindowFactory();
            dockPanel.Extender.DockPaneStripFactory = new VS2013BlueDockPaneStripFactory();
            dockPanel.Extender.DockPaneSplitterControlFactory = new VS2013BlueDockPaneSplitterControlFactory();
            dockPanel.Extender.DockWindowSplitterControlFactory = new VS2013BlueDockWindowSplitterControlFactory();
            dockPanel.Extender.DockWindowFactory = new VS2013BlueDockWindowFactory();
            dockPanel.Extender.PaneIndicatorFactory = new VS2013BluePaneIndicatorFactory();
            dockPanel.Extender.PanelIndicatorFactory = new VS2013BluePanelIndicatorFactory();
            dockPanel.Extender.DockOutlineFactory = new VS2013BlueDockOutlineFactory();
            dockPanel.Skin = CreateVisualStudio2013Blue();
        }

        private class VS2013BlueDockOutlineFactory : DockPanelExtender.IDockOutlineFactory
        {
            public DockOutlineBase CreateDockOutline()
            {
                return new VS2013BlueDockOutline();
            }

            private class VS2013BlueDockOutline : DockOutlineBase
            {
                public VS2013BlueDockOutline()
                {
                    this.m_dragForm = new DragForm();
                    this.SetDragForm(Rectangle.Empty);
                    this.DragForm.BackColor = Color.FromArgb(0xff, 91, 173, 255);
                    this.DragForm.Opacity = 0.5;
                    this.DragForm.Show(false);
                }

                DragForm m_dragForm;
                private DragForm DragForm
                {
                    get { return this.m_dragForm; }
                }

                protected override void OnShow()
                {
                    this.CalculateRegion();
                }

                protected override void OnClose()
                {
                    this.DragForm.Close();
                }

                private void CalculateRegion()
                {
                    if (this.SameAsOldValue)
                        return;

                    if (!this.FloatWindowBounds.IsEmpty)
                        this.SetOutline(this.FloatWindowBounds);
                    else if (this.DockTo is DockPanel)
                        this.SetOutline(this.DockTo as DockPanel, this.Dock, (this.ContentIndex != 0));
                    else if (this.DockTo is DockPane)
                        this.SetOutline(this.DockTo as DockPane, this.Dock, this.ContentIndex);
                    else
                        this.SetOutline();
                }

                private void SetOutline()
                {
                    this.SetDragForm(Rectangle.Empty);
                }

                private void SetOutline(Rectangle floatWindowBounds)
                {
                    this.SetDragForm(floatWindowBounds);
                }

                private void SetOutline(DockPanel dockPanel, DockStyle dock, bool fullPanelEdge)
                {
                    Rectangle rect = fullPanelEdge ? dockPanel.DockArea : dockPanel.DocumentWindowBounds;
                    rect.Location = dockPanel.PointToScreen(rect.Location);
                    if (dock == DockStyle.Top)
                    {
                        int height = dockPanel.GetDockWindowSize(DockState.DockTop);
                        rect = new Rectangle(rect.X, rect.Y, rect.Width, height);
                    }
                    else if (dock == DockStyle.Bottom)
                    {
                        int height = dockPanel.GetDockWindowSize(DockState.DockBottom);
                        rect = new Rectangle(rect.X, rect.Bottom - height, rect.Width, height);
                    }
                    else if (dock == DockStyle.Left)
                    {
                        int width = dockPanel.GetDockWindowSize(DockState.DockLeft);
                        rect = new Rectangle(rect.X, rect.Y, width, rect.Height);
                    }
                    else if (dock == DockStyle.Right)
                    {
                        int width = dockPanel.GetDockWindowSize(DockState.DockRight);
                        rect = new Rectangle(rect.Right - width, rect.Y, width, rect.Height);
                    }
                    else if (dock == DockStyle.Fill)
                    {
                        rect = dockPanel.DocumentWindowBounds;
                        rect.Location = dockPanel.PointToScreen(rect.Location);
                    }

                    this.SetDragForm(rect);
                }

                private void SetOutline(DockPane pane, DockStyle dock, int contentIndex)
                {
                    if (dock != DockStyle.Fill)
                    {
                        Rectangle rect = pane.DisplayingRectangle;
                        if (dock == DockStyle.Right)
                            rect.X += rect.Width / 2;
                        if (dock == DockStyle.Bottom)
                            rect.Y += rect.Height / 2;
                        if (dock == DockStyle.Left || dock == DockStyle.Right)
                            rect.Width -= rect.Width / 2;
                        if (dock == DockStyle.Top || dock == DockStyle.Bottom)
                            rect.Height -= rect.Height / 2;
                        rect.Location = pane.PointToScreen(rect.Location);

                        this.SetDragForm(rect);
                    }
                    else if (contentIndex == -1)
                    {
                        Rectangle rect = pane.DisplayingRectangle;
                        rect.Location = pane.PointToScreen(rect.Location);
                        this.SetDragForm(rect);
                    }
                    else
                    {
                        using (GraphicsPath path = pane.TabStripControl.GetOutline(contentIndex))
                        {
                            RectangleF rectF = path.GetBounds();
                            Rectangle rect = new Rectangle((int)rectF.X, (int)rectF.Y, (int)rectF.Width, (int)rectF.Height);
                            using (Matrix matrix = new Matrix(rect, new Point[] { new Point(0, 0), new Point(rect.Width, 0), new Point(0, rect.Height) }))
                            {
                                path.Transform(matrix);
                            }

                            Region region = new Region(path);
                            this.SetDragForm(rect, region);
                        }
                    }
                }

                private void SetDragForm(Rectangle rect)
                {
                    this.DragForm.Show(false);
                    this.DragForm.Bounds = rect;
                    if (rect == Rectangle.Empty)
                    {
                        if (this.DragForm.Region != null)
                        {
                            this.DragForm.Region.Dispose();
                        }

                        this.DragForm.Region = new Region(Rectangle.Empty);
                    }
                    else if (this.DragForm.Region != null)
                    {
                        this.DragForm.Region.Dispose();
                        this.DragForm.Region = null;
                    }
                }

                private void SetDragForm(Rectangle rect, Region region)
                {
                    this.DragForm.Show(false);
                    this.DragForm.Bounds = rect;
                    if (this.DragForm.Region != null)
                    {
                        this.DragForm.Region.Dispose();
                    }

                    this.DragForm.Region = region;
                }


				protected override void OnHide()
				{
                    this.DragForm.Visible = false;
				}
			}
        }

        private class VS2013BluePanelIndicatorFactory : DockPanelExtender.IPanelIndicatorFactory
        {
            public DockPanel.IPanelIndicator CreatePanelIndicator(DockStyle style)
            {
                return new VS2013BluePanelIndicator(style);
            }

            private class VS2013BluePanelIndicator : PictureBox, DockPanel.IPanelIndicator
            {
                private static Image _imagePanelLeft = Resources.DockIndicator_PanelLeft;
                private static Image _imagePanelRight = Resources.DockIndicator_PanelRight;
                private static Image _imagePanelTop = Resources.DockIndicator_PanelTop;
                private static Image _imagePanelBottom = Resources.DockIndicator_PanelBottom;
                private static Image _imagePanelFill = Resources.DockIndicator_PanelFill;
                private static Image _imagePanelLeftActive = Resources.DockIndicator_PanelLeft;
                private static Image _imagePanelRightActive = Resources.DockIndicator_PanelRight;
                private static Image _imagePanelTopActive = Resources.DockIndicator_PanelTop;
                private static Image _imagePanelBottomActive = Resources.DockIndicator_PanelBottom;
                private static Image _imagePanelFillActive = Resources.DockIndicator_PanelFill;

                public VS2013BluePanelIndicator(DockStyle dockStyle)
                {
                    this.m_dockStyle = dockStyle;
                    this.SizeMode = PictureBoxSizeMode.AutoSize;
                    this.Image = this.ImageInactive;
                }

                private DockStyle m_dockStyle;

                private DockStyle DockStyle
                {
                    get { return this.m_dockStyle; }
                }

                private DockStyle m_status;

                public DockStyle Status
                {
                    get { return this.m_status; }
                    set
                    {
                        if (value != this.DockStyle && value != DockStyle.None)
                            throw new InvalidEnumArgumentException();

                        if (this.m_status == value)
                            return;

                        this.m_status = value;
                        this.IsActivated = (this.m_status != DockStyle.None);
                    }
                }

                private Image ImageInactive
                {
                    get
                    {
                        if (this.DockStyle == DockStyle.Left)
                            return _imagePanelLeft;
                        else if (this.DockStyle == DockStyle.Right)
                            return _imagePanelRight;
                        else if (this.DockStyle == DockStyle.Top)
                            return _imagePanelTop;
                        else if (this.DockStyle == DockStyle.Bottom)
                            return _imagePanelBottom;
                        else if (this.DockStyle == DockStyle.Fill)
                            return _imagePanelFill;
                        else
                            return null;
                    }
                }

                private Image ImageActive
                {
                    get
                    {
                        if (this.DockStyle == DockStyle.Left)
                            return _imagePanelLeftActive;
                        else if (this.DockStyle == DockStyle.Right)
                            return _imagePanelRightActive;
                        else if (this.DockStyle == DockStyle.Top)
                            return _imagePanelTopActive;
                        else if (this.DockStyle == DockStyle.Bottom)
                            return _imagePanelBottomActive;
                        else if (this.DockStyle == DockStyle.Fill)
                            return _imagePanelFillActive;
                        else
                            return null;
                    }
                }

                private bool m_isActivated = false;

                private bool IsActivated
                {
                    get { return this.m_isActivated; }
                    set
                    {
                        this.m_isActivated = value;
                        this.Image = this.IsActivated ? this.ImageActive : this.ImageInactive;
                    }
                }

                public DockStyle HitTest(Point pt)
                {
                    return this.Visible && this.ClientRectangle.Contains(this.PointToClient(pt)) ? this.DockStyle : DockStyle.None;
                }
            }
        }

        private class VS2013BluePaneIndicatorFactory : DockPanelExtender.IPaneIndicatorFactory
        {
            public DockPanel.IPaneIndicator CreatePaneIndicator()
            {
                return new VS2013BluePaneIndicator();
            }

            private class VS2013BluePaneIndicator : PictureBox, DockPanel.IPaneIndicator
            {
                private static Bitmap _bitmapPaneDiamond = Resources.DockIndicator_PaneDiamond;
                private static Bitmap _bitmapPaneDiamondLeft = Resources.DockIndicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondRight = Resources.DockIndicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondTop = Resources.DockIndicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondBottom = Resources.DockIndicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondFill = Resources.DockIndicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondHotSpot = Resources.DockIndicator_PaneDiamond_HotSpot;
                private static Bitmap _bitmapPaneDiamondHotSpotIndex = Resources.DockIndicator_PaneDiamond_HotSpotIndex;

                private static DockPanel.HotSpotIndex[] _hotSpots = new[]
                    {
                        new DockPanel.HotSpotIndex(1, 0, DockStyle.Top),
                        new DockPanel.HotSpotIndex(0, 1, DockStyle.Left),
                        new DockPanel.HotSpotIndex(1, 1, DockStyle.Fill),
                        new DockPanel.HotSpotIndex(2, 1, DockStyle.Right),
                        new DockPanel.HotSpotIndex(1, 2, DockStyle.Bottom)
                    };

                private GraphicsPath _displayingGraphicsPath = DrawHelper.CalculateGraphicsPathFromBitmap(_bitmapPaneDiamond);

                public VS2013BluePaneIndicator()
                {
                    this.SizeMode = PictureBoxSizeMode.AutoSize;
                    this.Image = _bitmapPaneDiamond;
                    this.Region = new Region(this.DisplayingGraphicsPath);
                }

                public GraphicsPath DisplayingGraphicsPath
                {
                    get { return this._displayingGraphicsPath; }
                }

                public DockStyle HitTest(Point pt)
                {
                    if (!this.Visible)
                        return DockStyle.None;

                    pt = this.PointToClient(pt);
                    if (!this.ClientRectangle.Contains(pt))
                        return DockStyle.None;

                    for (int i = _hotSpots.GetLowerBound(0); i <= _hotSpots.GetUpperBound(0); i++)
                    {
                        if (_bitmapPaneDiamondHotSpot.GetPixel(pt.X, pt.Y) == _bitmapPaneDiamondHotSpotIndex.GetPixel(_hotSpots[i].X, _hotSpots[i].Y))
                            return _hotSpots[i].DockStyle;
                    }

                    return DockStyle.None;
                }

                private DockStyle m_status = DockStyle.None;

                public DockStyle Status
                {
                    get { return this.m_status; }
                    set
                    {
                        this.m_status = value;
                        if (this.m_status == DockStyle.None)
                            this.Image = _bitmapPaneDiamond;
                        else if (this.m_status == DockStyle.Left)
                            this.Image = _bitmapPaneDiamondLeft;
                        else if (this.m_status == DockStyle.Right)
                            this.Image = _bitmapPaneDiamondRight;
                        else if (this.m_status == DockStyle.Top)
                            this.Image = _bitmapPaneDiamondTop;
                        else if (this.m_status == DockStyle.Bottom)
                            this.Image = _bitmapPaneDiamondBottom;
                        else if (this.m_status == DockStyle.Fill)
                            this.Image = _bitmapPaneDiamondFill;
                    }
                }
            }
        }

        private class VS2013BlueAutoHideWindowFactory : DockPanelExtender.IAutoHideWindowFactory
        {
            public DockPanel.AutoHideWindowControl CreateAutoHideWindow(DockPanel panel)
            {
                return new VS2012LightAutoHideWindowControl(panel);
            }
        }

        private class VS2013BlueDockPaneSplitterControlFactory : DockPanelExtender.IDockPaneSplitterControlFactory
        {
            public DockPane.SplitterControlBase CreateSplitterControl(DockPane pane)
            {
                return new VS2013BlueSplitterControl(pane);
            }
        }

        private class VS2013BlueDockWindowSplitterControlFactory : DockPanelExtender.IDockWindowSplitterControlFactory
        {
            public SplitterBase CreateSplitterControl()
            {
                return new VS2013BlueDockWindowSplitterControl();
            }
        }

        private class VS2013BlueDockPaneStripFactory : DockPanelExtender.IDockPaneStripFactory
        {
            public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
            {
                return new VS2013BlueDockPaneStrip(pane);
            }
        }

        private class VS2013BlueAutoHideStripFactory : DockPanelExtender.IAutoHideStripFactory
        {
            public AutoHideStripBase CreateAutoHideStrip(DockPanel panel)
            {
                return new VS2012LightAutoHideStrip(panel);
            }
        }

        private class VS2013BlueDockPaneCaptionFactory : DockPanelExtender.IDockPaneCaptionFactory
        {
            public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane)
            {
                return new VS2012LightDockPaneCaption(pane);
            }
        }

        private class VS2013BlueDockWindowFactory : DockPanelExtender.IDockWindowFactory
        {
            public DockWindow CreateDockWindow(DockPanel dockPanel, DockState dockState)
            {
                return new VS2012LightDockWindow(dockPanel, dockState);
            }
        }

        public static DockPanelSkin CreateVisualStudio2013Blue()
        {
            var border      = Color.FromArgb(0xFF, 41, 57, 85);
            var specialyellow = Color.FromArgb(0xFF, 255, 242, 157);
            var hover = Color.FromArgb(0xFF, 155, 167, 183);

            var activeTab = specialyellow;
            var mouseHoverTab = Color.FromArgb(0xFF, 91, 113, 153);
            var inactiveTab = Color.FromArgb(0xFF, 54, 78, 111);
            var lostFocusTab = Color.FromArgb(0xFF, 77, 96, 130);
            var skin = new DockPanelSkin();

            skin.AutoHideStripSkin.DockStripGradient.StartColor = hover;
            skin.AutoHideStripSkin.DockStripGradient.EndColor = inactiveTab;
            skin.AutoHideStripSkin.TabGradient.TextColor = Color.Black;
            skin.AutoHideStripSkin.DockStripBackground.StartColor = border;

            skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.StartColor = border;
            skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.EndColor = border;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.StartColor = activeTab;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.EndColor = lostFocusTab;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.TextColor = Color.Black;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.StartColor = inactiveTab;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.EndColor = mouseHoverTab;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.TextColor = Color.White;

            skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.StartColor = inactiveTab;
            skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.EndColor = inactiveTab;

            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.StartColor = Color.White;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.EndColor = Color.White;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.TextColor = Color.Black;

            skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.StartColor = border;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.EndColor = border;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.TextColor = Color.White;

            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.StartColor = specialyellow;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.EndColor = specialyellow;
            //skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.TextColor = Color.Black;

            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.StartColor = lostFocusTab;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.EndColor = lostFocusTab;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.TextColor = Color.White;

            skin.DockPaneStripSkin.ToolWindowGradient.HoverTabGradient.TextColor = Color.White;

            skin.DockPaneStripSkin.DocumentGradient.HoverTabGradient.StartColor = mouseHoverTab;
            skin.DockPaneStripSkin.DocumentGradient.HoverTabGradient.EndColor = mouseHoverTab;
            skin.DockPaneStripSkin.DocumentGradient.HoverTabGradient.TextColor = Color.White;

            return skin;
        }

        internal class VS2013BlueDockWindowSplitterControl : SplitterBase
        {
            private SolidBrush _brush;

            protected override int SplitterSize
            {
                get { return Measures.SplitterSize; }
            }

            protected override void StartDrag()
            {
                DockWindow window = this.Parent as DockWindow;
                if (window == null)
                    return;
                if (!window.DockPanel.AllowChangeLayout)
                    return;
                window.DockPanel.BeginDrag(window, window.RectangleToScreen(this.Bounds));
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Rectangle rect = this.ClientRectangle;

                if (rect.Width <= 0 || rect.Height <= 0)
                    return;

                DockWindow window = this.Parent as DockWindow;
                if (window == null)
                    return;

                if (this._brush == null)
                {
                    this._brush = new SolidBrush(window.DockPanel.Skin.AutoHideStripSkin.DockStripBackground.StartColor);
                }

                switch (this.Dock)
                {
                    case DockStyle.Right:
                    case DockStyle.Left:
                        {
                            e.Graphics.FillRectangle(this._brush, rect.X, rect.Y,
                                                             Measures.SplitterSize, rect.Height);
                        }
                        break;
                    case DockStyle.Bottom:
                    case DockStyle.Top:
                        {
                            e.Graphics.FillRectangle(this._brush, rect.X, rect.Y, rect.Width, Measures.SplitterSize);
                        }
                        break;
                }

            }
        }
    }
}
