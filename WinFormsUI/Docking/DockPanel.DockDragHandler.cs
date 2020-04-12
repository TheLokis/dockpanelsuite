using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        #region PaneIndicator

        public interface IPaneIndicator : IHitTest
        {
            Point Location { get; set; }
            bool Visible { get; set; }
            int Left { get; }
            int Top { get; }
            int Right { get; }
            int Bottom { get; }
            Rectangle ClientRectangle { get; }
            int Width { get; }
            int Height { get; }
            GraphicsPath DisplayingGraphicsPath { get; }
        }

        public struct HotSpotIndex
        {
            public HotSpotIndex(int x, int y, DockStyle dockStyle)
            {
                this.m_x = x;
                this.m_y = y;
                this.m_dockStyle = dockStyle;
            }

            private int m_x;
            public int X
            {
                get { return this.m_x; }
            }

            private int m_y;
            public int Y
            {
                get { return this.m_y; }
            }

            private DockStyle m_dockStyle;
            public DockStyle DockStyle
            {
                get { return this.m_dockStyle; }
            }
        }

        internal class DefaultPaneIndicator : PictureBox, IPaneIndicator
        {
            private static Bitmap _bitmapPaneDiamond = Resources.DockIndicator_PaneDiamond;
            private static Bitmap _bitmapPaneDiamondLeft = Resources.DockIndicator_PaneDiamond_Left;
            private static Bitmap _bitmapPaneDiamondRight = Resources.DockIndicator_PaneDiamond_Right;
            private static Bitmap _bitmapPaneDiamondTop = Resources.DockIndicator_PaneDiamond_Top;
            private static Bitmap _bitmapPaneDiamondBottom = Resources.DockIndicator_PaneDiamond_Bottom;
            private static Bitmap _bitmapPaneDiamondFill = Resources.DockIndicator_PaneDiamond_Fill;
            private static Bitmap _bitmapPaneDiamondHotSpot = Resources.DockIndicator_PaneDiamond_HotSpot;
            private static Bitmap _bitmapPaneDiamondHotSpotIndex = Resources.DockIndicator_PaneDiamond_HotSpotIndex;
            private static HotSpotIndex[] _hotSpots = new[]
            {
                new HotSpotIndex(1, 0, DockStyle.Top),
                new HotSpotIndex(0, 1, DockStyle.Left),
                new HotSpotIndex(1, 1, DockStyle.Fill),
                new HotSpotIndex(2, 1, DockStyle.Right),
                new HotSpotIndex(1, 2, DockStyle.Bottom)
            };

            private GraphicsPath _displayingGraphicsPath = DrawHelper.CalculateGraphicsPathFromBitmap(_bitmapPaneDiamond);

            public DefaultPaneIndicator()
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
        #endregion PaneIndicator

        #region IHitTest
        public interface IHitTest
        {
            DockStyle HitTest(Point pt);
            DockStyle Status { get; set; }
        }
        #endregion

        #region PanelIndicator

        public interface IPanelIndicator : IHitTest
        {
            Point Location { get; set; }
            bool Visible { get; set; }
            Rectangle Bounds { get; }
            int Width { get; }
            int Height { get; }
        }

        internal class DefaultPanelIndicator : PictureBox, IPanelIndicator
        {
            private static Image _imagePanelLeft = Resources.DockIndicator_PanelLeft;
            private static Image _imagePanelRight = Resources.DockIndicator_PanelRight;
            private static Image _imagePanelTop = Resources.DockIndicator_PanelTop;
            private static Image _imagePanelBottom = Resources.DockIndicator_PanelBottom;
            private static Image _imagePanelFill = Resources.DockIndicator_PanelFill;
            private static Image _imagePanelLeftActive = Resources.DockIndicator_PanelLeft_Active;
            private static Image _imagePanelRightActive = Resources.DockIndicator_PanelRight_Active;
            private static Image _imagePanelTopActive = Resources.DockIndicator_PanelTop_Active;
            private static Image _imagePanelBottomActive = Resources.DockIndicator_PanelBottom_Active;
            private static Image _imagePanelFillActive = Resources.DockIndicator_PanelFill_Active;

            public DefaultPanelIndicator(DockStyle dockStyle)
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
        #endregion PanelIndicator

        internal class DefaultDockOutline : DockOutlineBase
        {
            public DefaultDockOutline()
            {
                this.m_dragForm = new DragForm();
                this.DragForm.Bounds = Rectangle.Empty;
                this.DragForm.BackColor = SystemColors.ActiveCaption;
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

			protected override void OnClose() {
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
                this.DragForm.Region = region;
            }
        }

        private sealed class DockDragHandler : DragHandler
        {
            public class DockIndicator : DragForm
            {
                #region consts
                private int _PanelIndicatorMargin = 10;
                #endregion

                private DockDragHandler m_dragHandler;

                public DockIndicator(DockDragHandler dragHandler)
                {
                    this.m_dragHandler = dragHandler;
                    this.Controls.AddRange(new[] {
                        (Control)this.PaneDiamond,
                        (Control)this.PanelLeft,
                        (Control)this.PanelRight,
                        (Control)this.PanelTop,
                        (Control)this.PanelBottom,
                        (Control)this.PanelFill
                        });
                    this.Region = new Region(Rectangle.Empty);
                }

                private IPaneIndicator m_paneDiamond = null;
                private IPaneIndicator PaneDiamond
                {
                    get
                    {
                        if (this.m_paneDiamond == null)
                            this.m_paneDiamond = this.m_dragHandler.DockPanel.Extender.PaneIndicatorFactory.CreatePaneIndicator();

                        return this.m_paneDiamond;
                    }
                }

                private IPanelIndicator m_panelLeft = null;
                private IPanelIndicator PanelLeft
                {
                    get
                    {
                        if (this.m_panelLeft == null)
                            this.m_panelLeft = this.m_dragHandler.DockPanel.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Left);

                        return this.m_panelLeft;
                    }
                }

                private IPanelIndicator m_panelRight = null;
                private IPanelIndicator PanelRight
                {
                    get
                    {
                        if (this.m_panelRight == null)
                            this.m_panelRight = this.m_dragHandler.DockPanel.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Right);

                        return this.m_panelRight;
                    }
                }

                private IPanelIndicator m_panelTop = null;
                private IPanelIndicator PanelTop
                {
                    get
                    {
                        if (this.m_panelTop == null)
                            this.m_panelTop = this.m_dragHandler.DockPanel.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Top);

                        return this.m_panelTop;
                    }
                }

                private IPanelIndicator m_panelBottom = null;
                private IPanelIndicator PanelBottom
                {
                    get
                    {
                        if (this.m_panelBottom == null)
                            this.m_panelBottom = this.m_dragHandler.DockPanel.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Bottom);

                        return this.m_panelBottom;
                    }
                }

                private IPanelIndicator m_panelFill = null;
                private IPanelIndicator PanelFill
                {
                    get
                    {
                        if (this.m_panelFill == null)
                            this.m_panelFill = this.m_dragHandler.DockPanel.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Fill);

                        return this.m_panelFill;
                    }
                }

                private bool m_fullPanelEdge = false;
                public bool FullPanelEdge
                {
                    get { return this.m_fullPanelEdge; }
                    set
                    {
                        if (this.m_fullPanelEdge == value)
                            return;

                        this.m_fullPanelEdge = value;
                        this.RefreshChanges();
                    }
                }

                public DockDragHandler DragHandler
                {
                    get { return this.m_dragHandler; }
                }

                public DockPanel DockPanel
                {
                    get { return this.DragHandler.DockPanel; }
                }

                private DockPane m_dockPane = null;
                public DockPane DockPane
                {
                    get { return this.m_dockPane; }
                    internal set
                    {
                        if (this.m_dockPane == value)
                            return;

                        DockPane oldDisplayingPane = this.DisplayingPane;
                        this.m_dockPane = value;
                        if (oldDisplayingPane != this.DisplayingPane)
                            this.RefreshChanges();
                    }
                }

                private IHitTest m_hitTest = null;
                private IHitTest HitTestResult
                {
                    get { return this.m_hitTest; }
                    set
                    {
                        if (this.m_hitTest == value)
                            return;

                        if (this.m_hitTest != null)
                            this.m_hitTest.Status = DockStyle.None;

                        this.m_hitTest = value;
                    }
                }

                private DockPane DisplayingPane
                {
                    get { return this.ShouldPaneDiamondVisible() ? this.DockPane : null; }
                }

                private void RefreshChanges()
                {
                    Region region = new Region(Rectangle.Empty);
                    Rectangle rectDockArea = this.FullPanelEdge ? this.DockPanel.DockArea : this.DockPanel.DocumentWindowBounds;

                    rectDockArea = this.RectangleToClient(this.DockPanel.RectangleToScreen(rectDockArea));
                    if (this.ShouldPanelIndicatorVisible(DockState.DockLeft))
                    {
                        this.PanelLeft.Location = new Point(rectDockArea.X + this._PanelIndicatorMargin, rectDockArea.Y + (rectDockArea.Height - this.PanelRight.Height) / 2);
                        this.PanelLeft.Visible = true;
                        region.Union(this.PanelLeft.Bounds);
                    }
                    else
                        this.PanelLeft.Visible = false;

                    if (this.ShouldPanelIndicatorVisible(DockState.DockRight))
                    {
                        this.PanelRight.Location = new Point(rectDockArea.X + rectDockArea.Width - this.PanelRight.Width - this._PanelIndicatorMargin, rectDockArea.Y + (rectDockArea.Height - this.PanelRight.Height) / 2);
                        this.PanelRight.Visible = true;
                        region.Union(this.PanelRight.Bounds);
                    }
                    else
                        this.PanelRight.Visible = false;

                    if (this.ShouldPanelIndicatorVisible(DockState.DockTop))
                    {
                        this.PanelTop.Location = new Point(rectDockArea.X + (rectDockArea.Width - this.PanelTop.Width) / 2, rectDockArea.Y + this._PanelIndicatorMargin);
                        this.PanelTop.Visible = true;
                        region.Union(this.PanelTop.Bounds);
                    }
                    else
                        this.PanelTop.Visible = false;

                    if (this.ShouldPanelIndicatorVisible(DockState.DockBottom))
                    {
                        this.PanelBottom.Location = new Point(rectDockArea.X + (rectDockArea.Width - this.PanelBottom.Width) / 2, rectDockArea.Y + rectDockArea.Height - this.PanelBottom.Height - this._PanelIndicatorMargin);
                        this.PanelBottom.Visible = true;
                        region.Union(this.PanelBottom.Bounds);
                    }
                    else
                        this.PanelBottom.Visible = false;

                    if (this.ShouldPanelIndicatorVisible(DockState.Document))
                    {
                        Rectangle rectDocumentWindow = this.RectangleToClient(this.DockPanel.RectangleToScreen(this.DockPanel.DocumentWindowBounds));
                        this.PanelFill.Location = new Point(rectDocumentWindow.X + (rectDocumentWindow.Width - this.PanelFill.Width) / 2, rectDocumentWindow.Y + (rectDocumentWindow.Height - this.PanelFill.Height) / 2);
                        this.PanelFill.Visible = true;
                        region.Union(this.PanelFill.Bounds);
                    }
                    else
                        this.PanelFill.Visible = false;

                    if (this.ShouldPaneDiamondVisible())
                    {
                        Rectangle rect = this.RectangleToClient(this.DockPane.RectangleToScreen(this.DockPane.ClientRectangle));
                        this.PaneDiamond.Location = new Point(rect.Left + (rect.Width - this.PaneDiamond.Width)/2, rect.Top + (rect.Height - this.PaneDiamond.Height)/2);
                        this.PaneDiamond.Visible = true;
                        using (GraphicsPath graphicsPath = this.PaneDiamond.DisplayingGraphicsPath.Clone() as GraphicsPath)
                        {
                            Point[] pts = new Point[]
                                {
                                    new Point(this.PaneDiamond.Left, this.PaneDiamond.Top),
                                    new Point(this.PaneDiamond.Right, this.PaneDiamond.Top),
                                    new Point(this.PaneDiamond.Left, this.PaneDiamond.Bottom)
                                };
                            using (Matrix matrix = new Matrix(this.PaneDiamond.ClientRectangle, pts))
                            {
                                graphicsPath.Transform(matrix);
                            }

                            region.Union(graphicsPath);
                        }
						if ( !this.Visible) {
                            this.Show( false );
						}
                    }
                    else
                        this.PaneDiamond.Visible = false;

                    this.Region = region;
                }

                private bool ShouldPanelIndicatorVisible(DockState dockState)
                {
                    if (!this.Visible)
                        return false;

                    if (this.DockPanel.DockWindows[dockState].Visible)
                        return false;

                    return this.DragHandler.DragSource.IsDockStateValid(dockState);
                }

                private bool ShouldPaneDiamondVisible()
                {
                    if (this.DockPane == null)
                        return false;

                    if (!this.DockPanel.AllowEndUserNestedDocking)
                        return false;

                    return this.DragHandler.DragSource.CanDockTo(this.DockPane);
                }

                public override void Show(bool bActivate)
                {
                    base.Show(bActivate);
                    this.Bounds = SystemInformation.VirtualScreen;
                    this.RefreshChanges();
                }

                public void TestDrop(DockHelper.CursorPoint  info)
                {
					Point pt = info.Cursor;
                    this.DockPane = info.Pane;

                    if (TestDrop(this.PanelLeft, pt) != DockStyle.None)
                        this.HitTestResult = this.PanelLeft;
                    else if (TestDrop(this.PanelRight, pt) != DockStyle.None)
                        this.HitTestResult = this.PanelRight;
                    else if (TestDrop(this.PanelTop, pt) != DockStyle.None)
                        this.HitTestResult = this.PanelTop;
                    else if (TestDrop(this.PanelBottom, pt) != DockStyle.None)
                        this.HitTestResult = this.PanelBottom;
                    else if (TestDrop(this.PanelFill, pt) != DockStyle.None)
                        this.HitTestResult = this.PanelFill;
                    else if (TestDrop(this.PaneDiamond, pt) != DockStyle.None)
                        this.HitTestResult = this.PaneDiamond;
                    else
                        this.HitTestResult = null;

                    if (this.HitTestResult != null)
                    {
                        if (this.HitTestResult is IPaneIndicator)
                            this.DragHandler.Outline.Show(this.DockPane, this.HitTestResult.Status);
                        else
                            this.DragHandler.Outline.Show(this.DockPanel, this.HitTestResult.Status, this.FullPanelEdge);
                    }
                }

                private static DockStyle TestDrop(IHitTest hitTest, Point pt)
                {
                    return hitTest.Status = hitTest.HitTest(pt);
                }
            }

            public DockDragHandler(DockPanel panel)
                : base(panel)
            {
            }

            public new IDockDragSource DragSource
            {
                get { return base.DragSource as IDockDragSource; }
                set { base.DragSource = value; }
            }

            private DockOutlineBase m_outline;
            public DockOutlineBase Outline
            {
                get { return this.m_outline; }
                private set { this.m_outline = value; }
            }

            private DockIndicator m_indicator;
            private DockIndicator Indicator
            {
                get { return this.m_indicator; }
                set { this.m_indicator = value; }
            }

            private Rectangle m_floatOutlineBounds;
            private Rectangle FloatOutlineBounds
            {
                get { return this.m_floatOutlineBounds; }
                set { this.m_floatOutlineBounds = value; }
            }

            public void BeginDrag(IDockDragSource dragSource)
            {
                this.DragSource = dragSource;

                if (!this.BeginDrag())
                {
                    this.DragSource = null;
                    return;
                }

                this.Outline = this.DockPanel.Extender.DockOutlineFactory.CreateDockOutline();
                this.Indicator = new DockIndicator(this);

                this.FloatOutlineBounds = this.DragSource.BeginDrag(this.StartMousePosition);
            }

            protected override void OnDragging()
            {
                this.TestDrop();
                this.DragSource.OnDragging( Control.MousePosition );
            }

            protected override void OnEndDrag(bool abort)
            {
                this.DockPanel.SuspendLayout(true);

                this.Outline.Close();
                this.Indicator.Close();

                this.EndDrag(abort);

                // Queue a request to layout all children controls
                this.DockPanel.PerformMdiClientLayout();

                this.DockPanel.ResumeLayout(true, true);

                this.DragSource.EndDrag();

                this.DragSource = null;
            }

            private void TestDrop()
            {
                this.Outline.FlagTestDrop = false;

                this.Indicator.FullPanelEdge = ((Control.ModifierKeys & Keys.Shift) != 0);

				DockHelper.CursorPoint cursorPointInfo = DockHelper.CursorPointInformation(this.DockPanel, this.DragSource );

				if ( ( Control.ModifierKeys & Keys.Control ) == 0 ) {
					if (this.Indicator.Visible == false ) {
						// show indicator when the cursor comes into the dock panel area
						if ( cursorPointInfo.DockPanel != null ) {
                            this.Indicator.Show( false );
						}
					}

                    this.Indicator.TestDrop( cursorPointInfo );

					if ( !this.Outline.FlagTestDrop ) {
						if ( cursorPointInfo.Pane != null && this.DragSource.IsDockStateValid( cursorPointInfo.Pane.DockState ) )
							cursorPointInfo.Pane.TestDrop( cursorPointInfo, this.Outline );
					}

					if ( !this.Outline.FlagTestDrop && this.DragSource.IsDockStateValid( DockState.Float ) ) {
						if ( cursorPointInfo.FloatWindow != null )
							cursorPointInfo.FloatWindow.TestDrop( cursorPointInfo, this.Outline );
					}
				} else
                    this.Indicator.DockPane = cursorPointInfo.Pane;

                if (!this.Outline.FlagTestDrop) {
					Rectangle rect = this.FloatOutlineBounds;
					rect.Offset( Control.MousePosition.X - this.StartMousePosition.X, Control.MousePosition.Y - this.StartMousePosition.Y );

					// do not show the outline when a user is moving a floating window
					if ( !(this.DragSource is FloatWindow) && this.DragSource.IsDockStateValid( DockState.Float ) ) {
                        this.Outline.Show( rect, true );
					} else {
						Cursor.Current = Cursors.No;
                        this.Outline.Show( rect, false );
					}
                }

                if (!this.Outline.FlagTestDrop)
                {
                }
                else
                    Cursor.Current = this.DragControl.Cursor;
            }

            private void EndDrag(bool abort)
            {
                if (abort)
                    return;

                if (!this.Outline.FloatWindowBounds.IsEmpty)
                    this.DragSource.FloatAt(this.Outline.FloatWindowBounds);
                else if (this.Outline.DockTo is DockPane)
                {
                    DockPane pane = this.Outline.DockTo as DockPane;
                    this.DragSource.DockTo(pane, this.Outline.Dock, this.Outline.ContentIndex);
                }
                else if (this.Outline.DockTo is DockPanel)
                {
                    DockPanel panel = this.Outline.DockTo as DockPanel;
                    panel.UpdateDockWindowZOrder(this.Outline.Dock, this.Outline.FlagFullEdge);
                    this.DragSource.DockTo(panel, this.Outline.Dock);
                }
            }
        }

        private DockDragHandler m_dockDragHandler = null;
        private DockDragHandler GetDockDragHandler()
        {
            if (this.m_dockDragHandler == null)
                this.m_dockDragHandler = new DockDragHandler(this);
            return this.m_dockDragHandler;
        }

        internal void BeginDrag(IDockDragSource dragSource)
        {
            this.GetDockDragHandler().BeginDrag(dragSource);
        }
    }
}
