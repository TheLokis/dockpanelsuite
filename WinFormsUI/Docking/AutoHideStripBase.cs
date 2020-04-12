using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WeifenLuo.WinFormsUI.Docking
{
    public abstract partial class AutoHideStripBase : Control
    {
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected class Tab : IDisposable
        {
            private IDockContent m_content;

            protected internal Tab(IDockContent content)
            {
                this.m_content = content;
            }

            ~Tab()
            {
                this.Dispose(false);
            }

            public IDockContent Content
            {
                get { return this.m_content; }
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected sealed class TabCollection : IEnumerable<Tab>
        {
            #region IEnumerable Members
            IEnumerator<Tab> IEnumerable<Tab>.GetEnumerator()
            {
                for (int i = 0; i < this.Count; i++)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                for (int i = 0; i < this.Count; i++)
                    yield return this[i];
            }
            #endregion

            internal TabCollection(DockPane pane)
            {
                this.m_dockPane = pane;
            }

            private DockPane m_dockPane = null;
            public DockPane DockPane
            {
                get { return this.m_dockPane; }
            }

            public DockPanel DockPanel
            {
                get { return this.DockPane.DockPanel; }
            }

            public int Count
            {
                get { return this.DockPane.DisplayingContents.Count; }
            }

            public Tab this[int index]
            {
                get
                {
                    IDockContent content = this.DockPane.DisplayingContents[index];
                    if (content == null)
                        throw (new ArgumentOutOfRangeException("index"));
                    if (content.DockHandler.AutoHideTab == null)
                        content.DockHandler.AutoHideTab = (this.DockPanel.AutoHideStripControl.CreateTab(content));
                    return content.DockHandler.AutoHideTab as Tab;
                }
            }

            public bool Contains(Tab tab)
            {
                return (this.IndexOf(tab) != -1);
            }

            public bool Contains(IDockContent content)
            {
                return (this.IndexOf(content) != -1);
            }

            public int IndexOf(Tab tab)
            {
                if (tab == null)
                    return -1;

                return this.IndexOf(tab.Content);
            }

            public int IndexOf(IDockContent content)
            {
                return this.DockPane.DisplayingContents.IndexOf(content);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected class Pane : IDisposable
        {
            private DockPane m_dockPane;

            protected internal Pane(DockPane dockPane)
            {
                this.m_dockPane = dockPane;
            }

            ~Pane()
            {
                this.Dispose(false);
            }

            public DockPane DockPane
            {
                get { return this.m_dockPane; }
            }

            public TabCollection AutoHideTabs
            {
                get
                {
                    if (this.DockPane.AutoHideTabs == null)
                        this.DockPane.AutoHideTabs = new TabCollection(this.DockPane);
                    return this.DockPane.AutoHideTabs as TabCollection;
                }
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected sealed class PaneCollection : IEnumerable<Pane>
        {
            private class AutoHideState
            {
                public DockState m_dockState;
                public bool m_selected = false;

                public AutoHideState(DockState dockState)
                {
                    this.m_dockState = dockState;
                }

                public DockState DockState
                {
                    get { return this.m_dockState; }
                }

                public bool Selected
                {
                    get { return this.m_selected; }
                    set { this.m_selected = value; }
                }
            }

            private class AutoHideStateCollection
            {
                private AutoHideState[] m_states;

                public AutoHideStateCollection()
                {
                    this.m_states = new AutoHideState[]	{	
                                                new AutoHideState(DockState.DockTopAutoHide),
                                                new AutoHideState(DockState.DockBottomAutoHide),
                                                new AutoHideState(DockState.DockLeftAutoHide),
                                                new AutoHideState(DockState.DockRightAutoHide)
                                            };
                }

                public AutoHideState this[DockState dockState]
                {
                    get
                    {
                        for (int i = 0; i < this.m_states.Length; i++)
                        {
                            if (this.m_states[i].DockState == dockState)
                                return this.m_states[i];
                        }
                        throw new ArgumentOutOfRangeException("dockState");
                    }
                }

                public bool ContainsPane(DockPane pane)
                {
                    if (pane.IsHidden)
                        return false;

                    for (int i = 0; i < this.m_states.Length; i++)
                    {
                        if (this.m_states[i].DockState == pane.DockState && this.m_states[i].Selected)
                            return true;
                    }
                    return false;
                }
            }

            internal PaneCollection(DockPanel panel, DockState dockState)
            {
                this.m_dockPanel = panel;
                this.m_states = new AutoHideStateCollection();
                this.States[DockState.DockTopAutoHide].Selected = (dockState == DockState.DockTopAutoHide);
                this.States[DockState.DockBottomAutoHide].Selected = (dockState == DockState.DockBottomAutoHide);
                this.States[DockState.DockLeftAutoHide].Selected = (dockState == DockState.DockLeftAutoHide);
                this.States[DockState.DockRightAutoHide].Selected = (dockState == DockState.DockRightAutoHide);
            }

            private DockPanel m_dockPanel;
            public DockPanel DockPanel
            {
                get { return this.m_dockPanel; }
            }

            private AutoHideStateCollection m_states;
            private AutoHideStateCollection States
            {
                get { return this.m_states; }
            }

            public int Count
            {
                get
                {
                    int count = 0;
                    foreach (DockPane pane in this.DockPanel.Panes)
                    {
                        if (this.States.ContainsPane(pane))
                            count++;
                    }

                    return count;
                }
            }

            public Pane this[int index]
            {
                get
                {
                    int count = 0;
                    foreach (DockPane pane in this.DockPanel.Panes)
                    {
                        if (!this.States.ContainsPane(pane))
                            continue;

                        if (count == index)
                        {
                            if (pane.AutoHidePane == null)
                                pane.AutoHidePane = this.DockPanel.AutoHideStripControl.CreatePane(pane);
                            return pane.AutoHidePane as Pane;
                        }

                        count++;
                    }
                    throw new ArgumentOutOfRangeException("index");
                }
            }

            public bool Contains(Pane pane)
            {
                return (this.IndexOf(pane) != -1);
            }

            public int IndexOf(Pane pane)
            {
                if (pane == null)
                    return -1;

                int index = 0;
                foreach (DockPane dockPane in this.DockPanel.Panes)
                {
                    if (!this.States.ContainsPane(pane.DockPane))
                        continue;

                    if (pane == dockPane.AutoHidePane)
                        return index;

                    index++;
                }
                return -1;
            }

            #region IEnumerable Members

            IEnumerator<Pane> IEnumerable<Pane>.GetEnumerator()
            {
                for (int i = 0; i < this.Count; i++)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                for (int i = 0; i < this.Count; i++)
                    yield return this[i];
            }

            #endregion
        }

        protected AutoHideStripBase(DockPanel panel)
        {
            this.m_dockPanel = panel;
            this.m_panesTop = new PaneCollection(panel, DockState.DockTopAutoHide);
            this.m_panesBottom = new PaneCollection(panel, DockState.DockBottomAutoHide);
            this.m_panesLeft = new PaneCollection(panel, DockState.DockLeftAutoHide);
            this.m_panesRight = new PaneCollection(panel, DockState.DockRightAutoHide);

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.Selectable, false);
        }

        private DockPanel m_dockPanel;
        protected DockPanel DockPanel
        {
            get { return this.m_dockPanel; }
        }

        private PaneCollection m_panesTop;
        protected PaneCollection PanesTop
        {
            get { return this.m_panesTop; }
        }

        private PaneCollection m_panesBottom;
        protected PaneCollection PanesBottom
        {
            get { return this.m_panesBottom; }
        }

        private PaneCollection m_panesLeft;
        protected PaneCollection PanesLeft
        {
            get { return this.m_panesLeft; }
        }

        private PaneCollection m_panesRight;
        protected PaneCollection PanesRight
        {
            get { return this.m_panesRight; }
        }

        protected PaneCollection GetPanes(DockState dockState)
        {
            if (dockState == DockState.DockTopAutoHide)
                return this.PanesTop;
            else if (dockState == DockState.DockBottomAutoHide)
                return this.PanesBottom;
            else if (dockState == DockState.DockLeftAutoHide)
                return this.PanesLeft;
            else if (dockState == DockState.DockRightAutoHide)
                return this.PanesRight;
            else
                throw new ArgumentOutOfRangeException("dockState");
        }

        internal int GetNumberOfPanes(DockState dockState)
        {
            return this.GetPanes(dockState).Count;
        }

        protected Rectangle RectangleTopLeft
        {
            get
            {	
                int height = this.MeasureHeight();
                return this.PanesTop.Count > 0 && this.PanesLeft.Count > 0 ? new Rectangle(0, 0, height, height) : Rectangle.Empty;
            }
        }

        protected Rectangle RectangleTopRight
        {
            get
            {
                int height = this.MeasureHeight();
                return this.PanesTop.Count > 0 && this.PanesRight.Count > 0 ? new Rectangle(this.Width - height, 0, height, height) : Rectangle.Empty;
            }
        }

        protected Rectangle RectangleBottomLeft
        {
            get
            {
                int height = this.MeasureHeight();
                return this.PanesBottom.Count > 0 && this.PanesLeft.Count > 0 ? new Rectangle(0, this.Height - height, height, height) : Rectangle.Empty;
            }
        }

        protected Rectangle RectangleBottomRight
        {
            get
            {
                int height = this.MeasureHeight();
                return this.PanesBottom.Count > 0 && this.PanesRight.Count > 0 ? new Rectangle(this.Width - height, this.Height - height, height, height) : Rectangle.Empty;
            }
        }

        protected internal Rectangle GetTabStripRectangle(DockState dockState)
        {
            int height = this.MeasureHeight();
            if (dockState == DockState.DockTopAutoHide && this.PanesTop.Count > 0)
                return new Rectangle(this.RectangleTopLeft.Width, 0, this.Width - this.RectangleTopLeft.Width - this.RectangleTopRight.Width, height);
            else if (dockState == DockState.DockBottomAutoHide && this.PanesBottom.Count > 0)
                return new Rectangle(this.RectangleBottomLeft.Width, this.Height - height, this.Width - this.RectangleBottomLeft.Width - this.RectangleBottomRight.Width, height);
            else if (dockState == DockState.DockLeftAutoHide && this.PanesLeft.Count > 0)
                return new Rectangle(0, this.RectangleTopLeft.Width, height, this.Height - this.RectangleTopLeft.Height - this.RectangleBottomLeft.Height);
            else if (dockState == DockState.DockRightAutoHide && this.PanesRight.Count > 0)
                return new Rectangle(this.Width - height, this.RectangleTopRight.Width, height, this.Height - this.RectangleTopRight.Height - this.RectangleBottomRight.Height);
            else
                return Rectangle.Empty;
        }

        private GraphicsPath m_displayingArea = null;
        private GraphicsPath DisplayingArea
        {
            get
            {
                if (this.m_displayingArea == null)
                    this.m_displayingArea = new GraphicsPath();

                return this.m_displayingArea;
            }
        }

        private void SetRegion()
        {
            this.DisplayingArea.Reset();
            this.DisplayingArea.AddRectangle(this.RectangleTopLeft);
            this.DisplayingArea.AddRectangle(this.RectangleTopRight);
            this.DisplayingArea.AddRectangle(this.RectangleBottomLeft);
            this.DisplayingArea.AddRectangle(this.RectangleBottomRight);
            this.DisplayingArea.AddRectangle(this.GetTabStripRectangle(DockState.DockTopAutoHide));
            this.DisplayingArea.AddRectangle(this.GetTabStripRectangle(DockState.DockBottomAutoHide));
            this.DisplayingArea.AddRectangle(this.GetTabStripRectangle(DockState.DockLeftAutoHide));
            this.DisplayingArea.AddRectangle(this.GetTabStripRectangle(DockState.DockRightAutoHide));
            this.Region = new Region(this.DisplayingArea);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            IDockContent content = this.HitTest();
            if (content == null)
                return;

            this.SetActiveAutoHideContent(content);

            content.DockHandler.Activate();
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);

            if (!this.DockPanel.ShowAutoHideContentOnHover)
                return;

            IDockContent content = this.HitTest();
            this.SetActiveAutoHideContent(content);

            // requires further tracking of mouse hover behavior,
            this.ResetMouseEventArgs();
        }

        private void SetActiveAutoHideContent(IDockContent content)
        {
            if (content != null && this.DockPanel.ActiveAutoHideContent != content)
                this.DockPanel.ActiveAutoHideContent = content;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.RefreshChanges();
            base.OnLayout (levent);
        }

        internal void RefreshChanges()
        {
            if (this.IsDisposed)
                return;

            this.SetRegion();
            this.OnRefreshChanges();
        }

        protected virtual void OnRefreshChanges()
        {
        }

        protected internal abstract int MeasureHeight();

        private IDockContent HitTest()
        {
            Point ptMouse = this.PointToClient(Control.MousePosition);
            return this.HitTest(ptMouse);
        }

        protected virtual Tab CreateTab(IDockContent content)
        {
            return new Tab(content);
        }

        protected virtual Pane CreatePane(DockPane dockPane)
        {
            return new Pane(dockPane);
        }

        protected abstract IDockContent HitTest(Point point);

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new AutoHideStripsAccessibleObject(this);
        }

        protected abstract Rectangle GetTabBounds(Tab tab);

        internal static Rectangle ToScreen(Rectangle rectangle, Control parent)
        {
            if (parent == null)
                return rectangle;

            return new Rectangle(parent.PointToScreen(new Point(rectangle.Left, rectangle.Top)), new Size(rectangle.Width, rectangle.Height));
        }

        public class AutoHideStripsAccessibleObject : Control.ControlAccessibleObject
        {
            private AutoHideStripBase _strip;

            public AutoHideStripsAccessibleObject(AutoHideStripBase strip)
                : base(strip)
            {
                this._strip = strip;
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Window;
                }
            }

            public override int GetChildCount()
            {
                // Top, Bottom, Left, Right
                return 4;
            }

            public override AccessibleObject GetChild(int index)
            {
                switch (index)
                {
                    case 0:
                        return new AutoHideStripAccessibleObject(this._strip, DockState.DockTopAutoHide, this);
                    case 1:
                        return new AutoHideStripAccessibleObject(this._strip, DockState.DockBottomAutoHide, this);						
                    case 2:
                        return new AutoHideStripAccessibleObject(this._strip, DockState.DockLeftAutoHide, this);
                    case 3:
                    default:
                        return new AutoHideStripAccessibleObject(this._strip, DockState.DockRightAutoHide, this);
                }
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                Dictionary<DockState, Rectangle> rectangles = new Dictionary<DockState, Rectangle> {
                    { DockState.DockTopAutoHide,    this._strip.GetTabStripRectangle(DockState.DockTopAutoHide) },
                    { DockState.DockBottomAutoHide, this._strip.GetTabStripRectangle(DockState.DockBottomAutoHide) },
                    { DockState.DockLeftAutoHide,   this._strip.GetTabStripRectangle(DockState.DockLeftAutoHide) },
                    { DockState.DockRightAutoHide,  this._strip.GetTabStripRectangle(DockState.DockRightAutoHide) },
                };

                Point point = this._strip.PointToClient(new Point(x, y));
                foreach (var rectangle in rectangles)
                {
                    if (rectangle.Value.Contains(point))
                        return new AutoHideStripAccessibleObject(this._strip, rectangle.Key, this);
                }

                return null;
            }
        }

        public class AutoHideStripAccessibleObject : AccessibleObject
        {
            private AutoHideStripBase _strip;
            private DockState _state;
            private AccessibleObject _parent;

            public AutoHideStripAccessibleObject(AutoHideStripBase strip, DockState state, AccessibleObject parent)
            {
                this._strip = strip;
                this._state = state;

                this._parent = parent;
            }

            public override AccessibleObject Parent
            {
                get
                {
                    return this._parent;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.PageTabList;
                }
            }

            public override int GetChildCount()
            {
                int count = 0;
                foreach (Pane pane in this._strip.GetPanes(this._state))
                {
                    count += pane.AutoHideTabs.Count;
                }
                return count;
            }

            public override AccessibleObject GetChild(int index)
            {
                List<Tab> tabs = new List<Tab>();
                foreach (Pane pane in this._strip.GetPanes(this._state))
                {
                    tabs.AddRange(pane.AutoHideTabs);
                }

                return new AutoHideStripTabAccessibleObject(this._strip, tabs[index], this);
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle rectangle = this._strip.GetTabStripRectangle(this._state);
                    return ToScreen(rectangle, this._strip);
                }
            }
        }

        protected class AutoHideStripTabAccessibleObject : AccessibleObject
        {
            private AutoHideStripBase _strip;
            private Tab _tab;

            private AccessibleObject _parent;

            internal AutoHideStripTabAccessibleObject(AutoHideStripBase strip, Tab tab, AccessibleObject parent)
            {
                this._strip = strip;
                this._tab = tab;

                this._parent = parent;
            }

            public override AccessibleObject Parent
            {
                get
                {
                    return this._parent;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.PageTab;
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle rectangle = this._strip.GetTabBounds(this._tab);
                    return ToScreen(rectangle, this._strip);
                }
            }

            public override string Name
            {
                get
                {
                    return this._tab.Content.DockHandler.TabText;
                }
                set
                {
                    //base.Name = value;
                }
            }
        }
    }
}
