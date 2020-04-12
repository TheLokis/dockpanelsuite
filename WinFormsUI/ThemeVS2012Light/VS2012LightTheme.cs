using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    using WeifenLuo.WinFormsUI.ThemeVS2012Light;

    /// <summary>
    /// Visual Studio 2012 Light theme.
    /// </summary>
    public class VS2012LightTheme : ThemeBase
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
            dockPanel.Extender.DockPaneCaptionFactory = new VS2012LightDockPaneCaptionFactory();
            dockPanel.Extender.AutoHideStripFactory = new VS2012LightAutoHideStripFactory();
            dockPanel.Extender.AutoHideWindowFactory = new VS2012LightAutoHideWindowFactory();
            dockPanel.Extender.DockPaneStripFactory = new VS2012LightDockPaneStripFactory();
            dockPanel.Extender.DockPaneSplitterControlFactory = new VS2012LightDockPaneSplitterControlFactory();
            dockPanel.Extender.DockWindowSplitterControlFactory = new VS2012LightDockWindowSplitterControlFactory();
            dockPanel.Extender.DockWindowFactory = new VS2012LightDockWindowFactory();
            dockPanel.Extender.PaneIndicatorFactory = new VS2012LightPaneIndicatorFactory();
            dockPanel.Extender.PanelIndicatorFactory = new VS2012LightPanelIndicatorFactory();
            dockPanel.Extender.DockOutlineFactory = new VS2012LightDockOutlineFactory();
            dockPanel.Skin = CreateVisualStudio2012Light();
        }

        private class VS2012LightDockOutlineFactory : DockPanelExtender.IDockOutlineFactory
        {
            public DockOutlineBase CreateDockOutline()
            {
                return new VS2012LightDockOutline();
            }

            private class VS2012LightDockOutline : DockOutlineBase
            {
                public VS2012LightDockOutline()
                {
                    this.m_dragForm = new DragForm();
                    this.SetDragForm(Rectangle.Empty);
                    this.DragForm.BackColor = Color.FromArgb(0xff, 91, 173, 255);
                    this.DragForm.Opacity = 0.5;
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

                protected override void OnHide()
                {
                    this.DragForm.Visible = false;
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
                    this.DragForm.Show( false );
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
                    this.DragForm.Show( false );
                    this.DragForm.Bounds = rect;
                    if (this.DragForm.Region != null)
                    {
                        this.DragForm.Region.Dispose();
                    }

                    this.DragForm.Region = region;
                }
            }
        }

        private class VS2012LightPanelIndicatorFactory : DockPanelExtender.IPanelIndicatorFactory
        {
            public DockPanel.IPanelIndicator CreatePanelIndicator(DockStyle style)
            {
                return new VS2012LightPanelIndicator(style);
            }

            private class VS2012LightPanelIndicator : PictureBox, DockPanel.IPanelIndicator
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

                public VS2012LightPanelIndicator(DockStyle dockStyle)
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

        private class VS2012LightPaneIndicatorFactory : DockPanelExtender.IPaneIndicatorFactory
        {
            public DockPanel.IPaneIndicator CreatePaneIndicator()
            {
                return new VS2012LightPaneIndicator();
            }

            private class VS2012LightPaneIndicator : PictureBox, DockPanel.IPaneIndicator
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

                public VS2012LightPaneIndicator()
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

        private class VS2012LightAutoHideWindowFactory : DockPanelExtender.IAutoHideWindowFactory
        {
            public DockPanel.AutoHideWindowControl CreateAutoHideWindow(DockPanel panel)
            {
                return new VS2012LightAutoHideWindowControl(panel);
            }
        }

        private class VS2012LightDockPaneSplitterControlFactory : DockPanelExtender.IDockPaneSplitterControlFactory
        {
            public DockPane.SplitterControlBase CreateSplitterControl(DockPane pane)
            {
                return new VS2012LightSplitterControl(pane);
            }
        }

        private class VS2012LightDockWindowSplitterControlFactory : DockPanelExtender.IDockWindowSplitterControlFactory
        {
            public SplitterBase CreateSplitterControl()
            {
                return new VS2012LightDockWindow.VS2012LightDockWindowSplitterControl();
            }
        }

        private class VS2012LightDockPaneStripFactory : DockPanelExtender.IDockPaneStripFactory
        {
            public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
            {
                return new VS2012LightDockPaneStrip(pane);
            }
        }

        private class VS2012LightAutoHideStripFactory : DockPanelExtender.IAutoHideStripFactory
        {
            public AutoHideStripBase CreateAutoHideStrip(DockPanel panel)
            {
                return new VS2012LightAutoHideStrip(panel);
            }
        }

        private class VS2012LightDockPaneCaptionFactory : DockPanelExtender.IDockPaneCaptionFactory
        {
            public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane)
            {
                return new VS2012LightDockPaneCaption(pane);
            }
        }

        private class VS2012LightDockWindowFactory : DockPanelExtender.IDockWindowFactory
        {
            public DockWindow CreateDockWindow(DockPanel dockPanel, DockState dockState)
            {
                return new VS2012LightDockWindow(dockPanel, dockState);
            }
        }

        public static DockPanelSkin CreateVisualStudio2012Light()
        {
            var specialBlue = Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC);
            var dot = Color.FromArgb(80, 170, 220);
            var activeTab = specialBlue;
            var mouseHoverTab = Color.FromArgb(0xFF, 28, 151, 234);
            var inactiveTab = SystemColors.Control;
            var lostFocusTab = Color.FromArgb(0xFF, 204, 206, 219);
            var skin = new DockPanelSkin();

            skin.AutoHideStripSkin.DockStripGradient.StartColor = specialBlue;
            skin.AutoHideStripSkin.DockStripGradient.EndColor = SystemColors.ControlLight;
            skin.AutoHideStripSkin.TabGradient.TextColor = SystemColors.ControlDarkDark;

            skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.StartColor = SystemColors.Control;
            skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.EndColor = SystemColors.Control;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.StartColor = activeTab;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.EndColor = lostFocusTab;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.TextColor = Color.White;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.StartColor = inactiveTab;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.EndColor = mouseHoverTab;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.TextColor = Color.Black;

            skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.StartColor = SystemColors.Control;
            skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.EndColor = SystemColors.Control;

            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.StartColor = SystemColors.ControlLightLight;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.EndColor = SystemColors.ControlLightLight;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.TextColor = specialBlue;

            skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.StartColor = SystemColors.Control;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.EndColor = SystemColors.Control;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.TextColor = SystemColors.GrayText;

            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.StartColor = specialBlue;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.EndColor = dot;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.TextColor = Color.White;

            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.StartColor = SystemColors.Control;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.EndColor = SystemColors.ControlDark;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.TextColor = SystemColors.GrayText;

            return skin;
        }
    }
}