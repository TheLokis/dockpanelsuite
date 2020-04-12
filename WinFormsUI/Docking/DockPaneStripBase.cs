using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;

namespace WeifenLuo.WinFormsUI.Docking
{
    public abstract class DockPaneStripBase : Control
    {
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]        
        protected internal class Tab : IDisposable
        {
            private IDockContent m_content;

            public Tab(IDockContent content)
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

            public Form ContentForm
            {
                get { return this.m_content as Form; }
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

            private DockPane m_dockPane;
            public DockPane DockPane
            {
                get { return this.m_dockPane; }
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
                    return content.DockHandler.GetTab(this.DockPane.TabStripControl);
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

                return this.DockPane.DisplayingContents.IndexOf(tab.Content);
            }

            public int IndexOf(IDockContent content)
            {
                return this.DockPane.DisplayingContents.IndexOf(content);
            }
        }

        protected DockPaneStripBase(DockPane pane)
        {
            this.m_dockPane = pane;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.Selectable, false);
            this.AllowDrop = true;
        }

        private DockPane m_dockPane;
        protected DockPane DockPane
        {
            get	{	return this.m_dockPane;	}
        }

        protected DockPane.AppearanceStyle Appearance
        {
            get	{	return this.DockPane.Appearance;	}
        }

        private TabCollection m_tabs = null;
        protected TabCollection Tabs
        {
            get
            {
                if (this.m_tabs == null)
                    this.m_tabs = new TabCollection(this.DockPane);

                return this.m_tabs;
            }
        }

        internal void RefreshChanges()
        {
            if (this.IsDisposed)
                return;

            this.OnRefreshChanges();
        }

        protected virtual void OnRefreshChanges()
        {
        }

        protected internal abstract int MeasureHeight();

        protected internal abstract void EnsureTabVisible(IDockContent content);

        protected int HitTest()
        {
            return this.HitTest(this.PointToClient(Control.MousePosition));
        }

        protected internal abstract int HitTest(Point point);

        public abstract GraphicsPath GetOutline(int index);

        protected internal virtual Tab CreateTab(IDockContent content)
        {
            return new Tab(content);
        }

        private Rectangle _dragBox = Rectangle.Empty;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int index = this.HitTest();
            if (index != -1)
            {
                if (e.Button == MouseButtons.Middle)
                {
                    // Close the specified content.
                    IDockContent content = this.Tabs[index].Content;
                    this.DockPane.CloseContent(content);
                }
                else
                {
                    IDockContent content = this.Tabs[index].Content;
                    if (this.DockPane.ActiveContent != content)
                        this.DockPane.ActiveContent = content;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                var dragSize = SystemInformation.DragSize;
                this._dragBox = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                e.Y - (dragSize.Height / 2)), dragSize);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button != MouseButtons.Left || this._dragBox.Contains(e.Location)) 
                return;

            if (this.DockPane.ActiveContent == null)
                return;

            if (this.DockPane.DockPanel.AllowEndUserDocking && this.DockPane.DockPanel.AllowChangeLayout && this.DockPane.AllowDockDragAndDrop && this.DockPane.ActiveContent.DockHandler.AllowEndUserDocking)
                this.DockPane.DockPanel.BeginDrag(this.DockPane.ActiveContent.DockHandler);
        }

        protected bool HasTabPageContextMenu
        {
            get { return this.DockPane.HasTabPageContextMenu; }
        }

        protected void ShowTabPageContextMenu(Point position)
        {
            this.DockPane.ShowTabPageContextMenu(this, position);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
                this.ShowTabPageContextMenu(new Point(e.X, e.Y));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)Win32.Msgs.WM_LBUTTONDBLCLK)
            {
                base.WndProc(ref m);

                int index = this.HitTest();
                if (this.DockPane.DockPanel.AllowEndUserDocking && this.DockPane.DockPanel.AllowChangeLayout && index != -1)
                {
                    IDockContent content = this.Tabs[index].Content;
                    if (content.DockHandler.CheckDockState(!content.DockHandler.IsFloat) != DockState.Unknown)
                        content.DockHandler.IsFloat = !content.DockHandler.IsFloat;	
                }

                return;
            }

            base.WndProc(ref m);
            return;
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            int index = this.HitTest();
            if (index != -1)
            {
                IDockContent content = this.Tabs[index].Content;
                if (this.DockPane.ActiveContent != content)
                    this.DockPane.ActiveContent = content;
            }
        }

        protected abstract Rectangle GetTabBounds(Tab tab);

        internal static Rectangle ToScreen(Rectangle rectangle, Control parent)
        {
            if (parent == null)
                return rectangle;

            return new Rectangle(parent.PointToScreen(new Point(rectangle.Left, rectangle.Top)), new Size(rectangle.Width, rectangle.Height));
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DockPaneStripAccessibleObject(this);
        }

        public class DockPaneStripAccessibleObject : Control.ControlAccessibleObject
        {
            private DockPaneStripBase _strip;
            private DockState _state;

            public DockPaneStripAccessibleObject(DockPaneStripBase strip)
                : base(strip)
            {
                this._strip = strip;
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
                return this._strip.Tabs.Count;
            }

            public override AccessibleObject GetChild(int index)
            {
                return new DockPaneStripTabAccessibleObject(this._strip, this._strip.Tabs[index], this);
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                Point point = new Point(x, y);
                foreach (Tab tab in this._strip.Tabs)
                {
                    Rectangle rectangle = this._strip.GetTabBounds(tab);
                    if (ToScreen(rectangle, this._strip).Contains(point))
                        return new DockPaneStripTabAccessibleObject(this._strip, tab, this);
                }

                return null;
            }
        }

        protected class DockPaneStripTabAccessibleObject : AccessibleObject
        {
            private DockPaneStripBase _strip;
            private Tab _tab;

            private AccessibleObject _parent;

            internal DockPaneStripTabAccessibleObject(DockPaneStripBase strip, Tab tab, AccessibleObject parent)
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
