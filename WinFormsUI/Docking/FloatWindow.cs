using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;

namespace WeifenLuo.WinFormsUI.Docking
{
    public class FloatWindow : Form, INestedPanesContainer, IDockDragSource
    {
        private NestedPaneCollection m_nestedPanes;
        internal const int WM_CHECKDISPOSE = (int)(Win32.Msgs.WM_USER + 1);

        internal protected FloatWindow(DockPanel dockPanel, DockPane pane)
        {
            this.InternalConstruct(dockPanel, pane, false, Rectangle.Empty);
        }

        internal protected FloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
        {
            this.InternalConstruct(dockPanel, pane, true, bounds);
        }

        private void InternalConstruct(DockPanel dockPanel, DockPane pane, bool boundsSpecified, Rectangle bounds)
        {
            if (dockPanel == null)
                throw(new ArgumentNullException(Strings.FloatWindow_Constructor_NullDockPanel));

            this.m_nestedPanes = new NestedPaneCollection(this);

            this.AllowChangeLayout = dockPanel.AllowChangeLayout;
            this.CanSizableFloatWindowInLock = dockPanel.CanSizableFloatWindowInLock;
            this.ShowInTaskbar = false;
            if (dockPanel.RightToLeft != this.RightToLeft)
                this.RightToLeft = dockPanel.RightToLeft;
            if (this.RightToLeftLayout != dockPanel.RightToLeftLayout)
                this.RightToLeftLayout = dockPanel.RightToLeftLayout;

            this.SuspendLayout();
            if (boundsSpecified)
            {
                this.Bounds = bounds;
                this.StartPosition = FormStartPosition.Manual;
            }
            else
            {
                this.StartPosition = FormStartPosition.WindowsDefaultLocation;
                this.Size = dockPanel.DefaultFloatWindowSize;
            }

            this.m_dockPanel = dockPanel;
            this.Owner = this.DockPanel.FindForm();
            this.DockPanel.AddFloatWindow(this);
            if (pane != null)
                pane.FloatWindow = this;

            this.ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.DockPanel != null)
                    this.DockPanel.RemoveFloatWindow(this);
                this.m_dockPanel = null;
            }
            base.Dispose(disposing);
        }

        private bool m_allowEndUserDocking = true;
        public bool AllowEndUserDocking
        {
            get	{	return this.m_allowEndUserDocking;	}
            set	{ this.m_allowEndUserDocking = value;	}
        }

        private bool m_doubleClickTitleBarToDock = true;
        public bool DoubleClickTitleBarToDock
        {
            get { return this.m_doubleClickTitleBarToDock; }
            set { this.m_doubleClickTitleBarToDock = value; }
        }

		private bool m_allowChangeLayout = true;
		public bool AllowChangeLayout {
			get { return this.m_allowChangeLayout; }
			set {
				if (this.m_allowChangeLayout == value )
					return;

                this.m_allowChangeLayout = value;
                this.FormBorderStyle = this.m_allowChangeLayout ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
            }
		}

        private bool m_CanSizableFloatWindowInLock = false;
        public bool CanSizableFloatWindowInLock
        {
            get
            {
                return this.m_CanSizableFloatWindowInLock;
            }
            set
            {
                if (this.m_CanSizableFloatWindowInLock == value)
                    return;

                this.m_CanSizableFloatWindowInLock = value;
                this.FormBorderStyle = this.m_CanSizableFloatWindowInLock ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;

            }
        }


        public NestedPaneCollection NestedPanes
        {
            get	{	return this.m_nestedPanes;	}
        }

        public VisibleNestedPaneCollection VisibleNestedPanes
        {
            get	{	return this.NestedPanes.VisibleNestedPanes;	}
        }

        private DockPanel m_dockPanel;
        public DockPanel DockPanel
        {
            get	{	return this.m_dockPanel;	}
        }

        public DockState DockState
        {
            get	{	return DockState.Float;	}
        }
    
        public bool IsFloat
        {
            get	{	return this.DockState == DockState.Float;	}
        }

        internal bool IsDockStateValid(DockState dockState)
        {
            foreach (DockPane pane in this.NestedPanes)
                foreach (IDockContent content in pane.Contents)
                    if (!DockHelper.IsDockStateValid(dockState, content.DockHandler.DockAreas))
                        return false;

            return true;
        }

        protected override void OnActivated(EventArgs e)
        {
            this.DockPanel.FloatWindows.BringWindowToFront(this);
            base.OnActivated (e);
            // Propagate the Activated event to the visible panes content objects
            foreach (DockPane pane in this.VisibleNestedPanes)
                foreach (IDockContent content in pane.Contents)
                    content.OnActivated(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            // Propagate the Deactivate event to the visible panes content objects
            foreach (DockPane pane in this.VisibleNestedPanes)
                foreach (IDockContent content in pane.Contents)
                    content.OnDeactivate(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.VisibleNestedPanes.Refresh();
            this.RefreshChanges();
            this.Visible = (this.VisibleNestedPanes.Count > 0);
            this.SetText();

            base.OnLayout(levent);
        }


        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
        internal void SetText()
        {
			DockPane activePane = null;

			foreach ( var pane in this.VisibleNestedPanes ) {
				if ( pane.IsActivated ) {
					activePane = pane;
					break;
				}
			}

			if ( activePane == null )
				activePane = this.VisibleNestedPanes.Count > 0 ? this.VisibleNestedPanes[0] : null;

			if ( activePane == null || activePane.ActiveContent == null )
            {
                this.Text = " ";	// use " " instead of string.Empty because the whole title bar will disappear when ControlBox is set to false.
                this.Icon = null;
            }
            else
            {
                this.Text = activePane.ActiveContent.DockHandler.TabText;
                this.Icon = activePane.ActiveContent.DockHandler.Icon;
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            Rectangle rectWorkArea = SystemInformation.VirtualScreen;

            if (y + height > rectWorkArea.Bottom)
                y -= (y + height) - rectWorkArea.Bottom;

            if (y < rectWorkArea.Top)
                y += rectWorkArea.Top - y;

            base.SetBoundsCore (x, y, width, height, specified);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)Win32.Msgs.WM_NCLBUTTONDOWN:
                    {
                        if (this.IsDisposed)
                            return;

                        uint result = Win32Helper.IsRunningOnMono ? 0 : NativeMethods.SendMessage(this.Handle, (int)Win32.Msgs.WM_NCHITTEST, 0, (uint)m.LParam);
                        if (result == 2 && this.DockPanel.AllowEndUserDocking && this.DockPanel.AllowChangeLayout && this.AllowEndUserDocking)	// HITTEST_CAPTION
                        {
                            this.Activate();
                            this.m_dockPanel.BeginDrag(this);
                        }
                        else
                            base.WndProc(ref m);

                        return;
                    }
                case (int)Win32.Msgs.WM_NCRBUTTONDOWN:
                    {
                        uint result = Win32Helper.IsRunningOnMono ? 0 : NativeMethods.SendMessage(this.Handle, (int)Win32.Msgs.WM_NCHITTEST, 0, (uint)m.LParam);
                        if (result == 2)	// HITTEST_CAPTION
                        {
                            DockPane theOnlyPane = (this.VisibleNestedPanes.Count == 1) ? this.VisibleNestedPanes[0] : null;
                            if (theOnlyPane != null && theOnlyPane.ActiveContent != null)
                            {
                                theOnlyPane.ShowTabPageContextMenu(this, this.PointToClient(Control.MousePosition));
                                return;
                            }
                        }

                        base.WndProc(ref m);
                        return;
                    }
                case (int)Win32.Msgs.WM_CLOSE:
					if ( !this.m_dockPanel.AllowChangeLayout && !this.m_dockPanel.CanCloseFloatWindowInLock )
						return;

                    if (this.NestedPanes.Count == 0)
                    {
                        base.WndProc(ref m);
                        return;
                    }
                    for (int i = this.NestedPanes.Count - 1; i >= 0; i--)
                    {
                        DockContentCollection contents = this.NestedPanes[i].Contents;
                        for (int j = contents.Count - 1; j >= 0; j--)
                        {
                            IDockContent content = contents[j];
                            if (content.DockHandler.DockState != DockState.Float)
                                continue;

                            if (!content.DockHandler.CloseButton)
                                continue;

                            if (content.DockHandler.HideOnClose)
                                content.DockHandler.Hide();
                            else
                                content.DockHandler.Close();
                        }
                    }
                    return;
                case (int)Win32.Msgs.WM_NCLBUTTONDBLCLK:
                    {
                        uint result = !this.DoubleClickTitleBarToDock || !this.DockPanel.AllowChangeLayout || Win32Helper.IsRunningOnMono 
                            ? 0
                            : NativeMethods.SendMessage(this.Handle, (int)Win32.Msgs.WM_NCHITTEST, 0, (uint)m.LParam);

                        if (result != 2)	// HITTEST_CAPTION
                        {
                            base.WndProc(ref m);
                            return;
                        }

                        this.DockPanel.SuspendLayout(true);

                        // Restore to panel
                        foreach (DockPane pane in this.NestedPanes)
                        {
                            if (pane.DockState != DockState.Float)
                                continue;
                            pane.RestoreToPanel();
                        }


                        this.DockPanel.ResumeLayout(true, true);
                        return;
                    }
                case WM_CHECKDISPOSE:
                    if (this.NestedPanes.Count == 0)
                        this.Dispose();
                    return;
            }

            base.WndProc(ref m);
        }

        internal void RefreshChanges()
        {
            if (this.IsDisposed)
                return;

            if (this.VisibleNestedPanes.Count == 0)
            {
                this.ControlBox = true;
                return;
            }

            for (int i= this.VisibleNestedPanes.Count - 1; i>=0; i--)
            {
                DockContentCollection contents = this.VisibleNestedPanes[i].Contents;
                for (int j=contents.Count - 1; j>=0; j--)
                {
                    IDockContent content = contents[j];
                    if (content.DockHandler.DockState != DockState.Float)
                        continue;

                    if (content.DockHandler.CloseButton && content.DockHandler.CloseButtonVisible)
                    {
                        this.ControlBox = true;
                        return;
                    }
                }
            }
            //Only if there is a ControlBox do we turn it off
            //old code caused a flash of the window.
            if (this.ControlBox)
                this.ControlBox = false;
        }

        public virtual Rectangle DisplayingRectangle
        {
            get	{	return this.ClientRectangle;	}
        }

		internal void TestDrop( DockHelper.CursorPoint info, DockOutlineBase dockOutline )
        {
            if (this.VisibleNestedPanes.Count == 1)
            {
                DockPane pane = this.VisibleNestedPanes[0];
				if ( !info.DragSource.CanDockTo( pane ) )
                    return;

				Point ptMouse = info.Cursor;
                uint lParam = Win32Helper.MakeLong(ptMouse.X, ptMouse.Y);
                if (!Win32Helper.IsRunningOnMono)
                {
                    if (NativeMethods.SendMessage(this.Handle, (int)Win32.Msgs.WM_NCHITTEST, 0, lParam) == (uint)Win32.HitTest.HTCAPTION)
                    {
                        dockOutline.Show(this.VisibleNestedPanes[0], -1);
                    }
                }
            }
        }

        #region IDockDragSource Members

        #region IDragSource Members

        Control IDragSource.DragControl
        {
            get { return this; }
        }

        #endregion

        bool IDockDragSource.IsDockStateValid(DockState dockState)
        {
            return this.IsDockStateValid(dockState);
        }

        bool IDockDragSource.CanDockTo(DockPane pane)
        {
            if (!this.IsDockStateValid(pane.DockState))
                return false;

            if (pane.FloatWindow == this)
                return false;

            return true;
        }

        private int m_preDragExStyle;
		private Point m_preDragPosition;
		private Point m_dragStartPoint;

        Rectangle IDockDragSource.BeginDrag(Point ptMouse)
        {
            this.m_preDragPosition = this.Location;
            this.m_dragStartPoint = ptMouse;
            this.m_preDragExStyle = NativeMethods.GetWindowLong(this.Handle, (int)Win32.GetWindowLongIndex.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(this.Handle, 
                                        (int)Win32.GetWindowLongIndex.GWL_EXSTYLE,
                                        this.m_preDragExStyle | (int)(Win32.WindowExStyles.WS_EX_TRANSPARENT | Win32.WindowExStyles.WS_EX_LAYERED) );
            return this.Bounds;
        }

		void IDockDragSource.OnDragging( Point ptMouse ) {
            this.Location = new Point(
                this.m_preDragPosition.X + ( ptMouse.X - this.m_dragStartPoint.X ),
                this.m_preDragPosition.Y + ( ptMouse.Y - this.m_dragStartPoint.Y ) );
		}

        void IDockDragSource.EndDrag()
        {
            NativeMethods.SetWindowLong(this.Handle, (int)Win32.GetWindowLongIndex.GWL_EXSTYLE, this.m_preDragExStyle);

            this.Invalidate(true);
            NativeMethods.SendMessage(this.Handle, (int)Win32.Msgs.WM_NCPAINT, 1, 0);
        }

        public  void FloatAt(Rectangle floatWindowBounds)
        {
            this.Bounds = floatWindowBounds;
        }

        public void DockTo(DockPane pane, DockStyle dockStyle, int contentIndex)
        {
            if (dockStyle == DockStyle.Fill)
            {
                for (int i = this.NestedPanes.Count - 1; i >= 0; i--)
                {
                    DockPane paneFrom = this.NestedPanes[i];
                    for (int j = paneFrom.Contents.Count - 1; j >= 0; j--)
                    {
                        IDockContent c = paneFrom.Contents[j];
                        c.DockHandler.Pane = pane;
                        if (contentIndex != -1)
                            pane.SetContentIndex(c, contentIndex);
                        c.DockHandler.Activate();
                    }
                }
            }
            else
            {
                DockAlignment alignment = DockAlignment.Left;
                if (dockStyle == DockStyle.Left)
                    alignment = DockAlignment.Left;
                else if (dockStyle == DockStyle.Right)
                    alignment = DockAlignment.Right;
                else if (dockStyle == DockStyle.Top)
                    alignment = DockAlignment.Top;
                else if (dockStyle == DockStyle.Bottom)
                    alignment = DockAlignment.Bottom;

                MergeNestedPanes(this.VisibleNestedPanes, pane.NestedPanesContainer.NestedPanes, pane, alignment, 0.5);
            }
        }

        public void DockTo(DockPanel panel, DockStyle dockStyle)
        {
            if (panel != this.DockPanel)
                throw new ArgumentException(Strings.IDockDragSource_DockTo_InvalidPanel, "panel");

            NestedPaneCollection nestedPanesTo = null;

            if (dockStyle == DockStyle.Top)
                nestedPanesTo = this.DockPanel.DockWindows[DockState.DockTop].NestedPanes;
            else if (dockStyle == DockStyle.Bottom)
                nestedPanesTo = this.DockPanel.DockWindows[DockState.DockBottom].NestedPanes;
            else if (dockStyle == DockStyle.Left)
                nestedPanesTo = this.DockPanel.DockWindows[DockState.DockLeft].NestedPanes;
            else if (dockStyle == DockStyle.Right)
                nestedPanesTo = this.DockPanel.DockWindows[DockState.DockRight].NestedPanes;
            else if (dockStyle == DockStyle.Fill)
                nestedPanesTo = this.DockPanel.DockWindows[DockState.Document].NestedPanes;

            DockPane prevPane = null;
            for (int i = nestedPanesTo.Count - 1; i >= 0; i--)
                if (nestedPanesTo[i] != this.VisibleNestedPanes[0])
                    prevPane = nestedPanesTo[i];
            MergeNestedPanes(this.VisibleNestedPanes, nestedPanesTo, prevPane, DockAlignment.Left, 0.5);
        }

        private static void MergeNestedPanes(VisibleNestedPaneCollection nestedPanesFrom, NestedPaneCollection nestedPanesTo, DockPane prevPane, DockAlignment alignment, double proportion)
        {
            if (nestedPanesFrom.Count == 0)
                return;

            int count = nestedPanesFrom.Count;
            DockPane[] panes = new DockPane[count];
            DockPane[] prevPanes = new DockPane[count];
            DockAlignment[] alignments = new DockAlignment[count];
            double[] proportions = new double[count];

            for (int i = 0; i < count; i++)
            {
                panes[i] = nestedPanesFrom[i];
                prevPanes[i] = nestedPanesFrom[i].NestedDockingStatus.PreviousPane;
                alignments[i] = nestedPanesFrom[i].NestedDockingStatus.Alignment;
                proportions[i] = nestedPanesFrom[i].NestedDockingStatus.Proportion;
            }

            DockPane pane = panes[0].DockTo(nestedPanesTo.Container, prevPane, alignment, proportion);
            panes[0].DockState = nestedPanesTo.DockState;

            for (int i = 1; i < count; i++)
            {
                for (int j = i; j < count; j++)
                {
                    if (prevPanes[j] == panes[i - 1])
                        prevPanes[j] = pane;
                }
                pane = panes[i].DockTo(nestedPanesTo.Container, prevPanes[i], alignments[i], proportions[i]);
                panes[i].DockState = nestedPanesTo.DockState;
            }
        }

        #endregion
    }
}
