using System;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public sealed class DockPanelExtender
    {
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IDockPaneFactory
        {
            DockPane CreateDockPane(IDockContent content, DockState visibleState, bool show);

            [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "1#")]
            DockPane CreateDockPane(IDockContent content, FloatWindow floatWindow, bool show);

            DockPane CreateDockPane(IDockContent content, DockPane previousPane, DockAlignment alignment,
                                    double proportion, bool show);

            [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "1#")]
            DockPane CreateDockPane(IDockContent content, Rectangle floatWindowBounds, bool show);
        }

        public interface IDockPaneSplitterControlFactory
        {
            DockPane.SplitterControlBase CreateSplitterControl(DockPane pane);
        }
        
        public interface IDockWindowSplitterControlFactory
        {
            SplitterBase CreateSplitterControl();
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IFloatWindowFactory
        {
            FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane);
            FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds);
        }

        public interface IDockWindowFactory
        {
            DockWindow CreateDockWindow(DockPanel dockPanel, DockState dockState);
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IDockPaneCaptionFactory
        {
            DockPaneCaptionBase CreateDockPaneCaption(DockPane pane);
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IDockPaneStripFactory
        {
            DockPaneStripBase CreateDockPaneStrip(DockPane pane);
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IAutoHideStripFactory
        {
            AutoHideStripBase CreateAutoHideStrip(DockPanel panel);
        }

        public interface IAutoHideWindowFactory
        {
            DockPanel.AutoHideWindowControl CreateAutoHideWindow(DockPanel panel);
        }

        public interface IPaneIndicatorFactory
        {
            DockPanel.IPaneIndicator CreatePaneIndicator();
        }

        public interface IPanelIndicatorFactory
        {
            DockPanel.IPanelIndicator CreatePanelIndicator(DockStyle style);
        }

        public interface IDockOutlineFactory
        {
            DockOutlineBase CreateDockOutline();
        }

        #region DefaultDockPaneFactory

        private class DefaultDockPaneFactory : IDockPaneFactory
        {
            public DockPane CreateDockPane(IDockContent content, DockState visibleState, bool show)
            {
                return new DockPane(content, visibleState, show);
            }

            public DockPane CreateDockPane(IDockContent content, FloatWindow floatWindow, bool show)
            {
                return new DockPane(content, floatWindow, show);
            }

            public DockPane CreateDockPane(IDockContent content, DockPane prevPane, DockAlignment alignment,
                                           double proportion, bool show)
            {
                return new DockPane(content, prevPane, alignment, proportion, show);
            }

            public DockPane CreateDockPane(IDockContent content, Rectangle floatWindowBounds, bool show)
            {
                return new DockPane(content, floatWindowBounds, show);
            }
        }

        #endregion

        #region DefaultDockPaneSplitterControlFactory

        private class DefaultDockPaneSplitterControlFactory : IDockPaneSplitterControlFactory
        {
            public DockPane.SplitterControlBase CreateSplitterControl(DockPane pane)
            {
                return new DockPane.DefaultSplitterControl(pane);
            }
        }

        #endregion
        
        #region DefaultDockWindowSplitterControlFactory

        private class DefaultDockWindowSplitterControlFactory : IDockWindowSplitterControlFactory
        {
            public SplitterBase CreateSplitterControl()
            {
                return new DockWindow.DefaultSplitterControl();
            }
        }

        #endregion

        #region DefaultFloatWindowFactory

        private class DefaultFloatWindowFactory : IFloatWindowFactory
        {
            public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane)
            {
                return new FloatWindow(dockPanel, pane);
            }

            public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
            {
                return new FloatWindow(dockPanel, pane, bounds);
            }
        }

        #endregion

        #region DefaultDockWindowFactory

        private class DefaultDockWindowFactory : IDockWindowFactory
        {
            public DockWindow CreateDockWindow(DockPanel dockPanel, DockState dockState)
            {
                return new DefaultDockWindow(dockPanel, dockState);
            }
        }

        #endregion

        #region DefaultDockPaneCaptionFactory

        private class DefaultDockPaneCaptionFactory : IDockPaneCaptionFactory
        {
            public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane)
            {
                return new VS2005DockPaneCaption(pane);
            }
        }

        #endregion

        #region DefaultDockPaneTabStripFactory

        private class DefaultDockPaneStripFactory : IDockPaneStripFactory
        {
            public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
            {
                return new VS2005DockPaneStrip(pane);
            }
        }

        #endregion

        #region DefaultAutoHideStripFactory

        private class DefaultAutoHideStripFactory : IAutoHideStripFactory
        {
            public AutoHideStripBase CreateAutoHideStrip(DockPanel panel)
            {
                return new VS2005AutoHideStrip(panel);
            }
        }

        #endregion

        #region DefaultAutoHideWindowFactory

        public class DefaultAutoHideWindowFactory : IAutoHideWindowFactory
        {
            public DockPanel.AutoHideWindowControl CreateAutoHideWindow(DockPanel panel)
            {
                return new DockPanel.DefaultAutoHideWindowControl(panel);
            }
        }

        #endregion

        public class DefaultPaneIndicatorFactory : IPaneIndicatorFactory
        {
            public DockPanel.IPaneIndicator CreatePaneIndicator()
            {
                return new DockPanel.DefaultPaneIndicator();
            }
        }

        public class DefaultPanelIndicatorFactory : IPanelIndicatorFactory
        {
            public DockPanel.IPanelIndicator CreatePanelIndicator(DockStyle style)
            {
                return new DockPanel.DefaultPanelIndicator(style);
            }
        }

        public class DefaultDockOutlineFactory : IDockOutlineFactory
        {
            public DockOutlineBase CreateDockOutline()
            {
                return new DockPanel.DefaultDockOutline();
            }
        }

        internal DockPanelExtender(DockPanel dockPanel)
        {
            this.m_dockPanel = dockPanel;
        }

        private DockPanel m_dockPanel;

        private DockPanel DockPanel
        {
            get { return this.m_dockPanel; }
        }

        private IDockPaneFactory m_dockPaneFactory = null;

        public IDockPaneFactory DockPaneFactory
        {
            get
            {
                if (this.m_dockPaneFactory == null)
                    this.m_dockPaneFactory = new DefaultDockPaneFactory();

                return this.m_dockPaneFactory;
            }
            set
            {
                if (this.DockPanel.Panes.Count > 0)
                    throw new InvalidOperationException();

                this.m_dockPaneFactory = value;
            }
        }

        private IDockPaneSplitterControlFactory m_dockPaneSplitterControlFactory;

        public IDockPaneSplitterControlFactory DockPaneSplitterControlFactory
        {
            get
            {
                return this.m_dockPaneSplitterControlFactory ??
                       (this.m_dockPaneSplitterControlFactory = new DefaultDockPaneSplitterControlFactory());
            }

            set
            {
                if (this.DockPanel.Panes.Count > 0)
                {
                    throw new InvalidOperationException();
                }

                this.m_dockPaneSplitterControlFactory = value;
            }
        }
        
        private IDockWindowSplitterControlFactory m_dockWindowSplitterControlFactory;

        public IDockWindowSplitterControlFactory DockWindowSplitterControlFactory
        {
            get
            {
                return this.m_dockWindowSplitterControlFactory ??
                       (this.m_dockWindowSplitterControlFactory = new DefaultDockWindowSplitterControlFactory());
            }

            set
            {
                this.m_dockWindowSplitterControlFactory = value;
                this.DockPanel.ReloadDockWindows();
            }
        }

        private IFloatWindowFactory m_floatWindowFactory = null;

        public IFloatWindowFactory FloatWindowFactory
        {
            get
            {
                if (this.m_floatWindowFactory == null)
                    this.m_floatWindowFactory = new DefaultFloatWindowFactory();

                return this.m_floatWindowFactory;
            }
            set
            {
                if (this.DockPanel.FloatWindows.Count > 0)
                    throw new InvalidOperationException();

                this.m_floatWindowFactory = value;
            }
        }

        private IDockWindowFactory m_dockWindowFactory;

        public IDockWindowFactory DockWindowFactory
        {
            get { return this.m_dockWindowFactory ?? (this.m_dockWindowFactory = new DefaultDockWindowFactory()); }
            set
            {
                this.m_dockWindowFactory = value;
                this.DockPanel.ReloadDockWindows();
            }
        }

        private IDockPaneCaptionFactory m_dockPaneCaptionFactory = null;

        public IDockPaneCaptionFactory DockPaneCaptionFactory
        {
            get
            {
                if (this.m_dockPaneCaptionFactory == null)
                    this.m_dockPaneCaptionFactory = new DefaultDockPaneCaptionFactory();

                return this.m_dockPaneCaptionFactory;
            }
            set
            {
                if (this.DockPanel.Panes.Count > 0)
                    throw new InvalidOperationException();

                this.m_dockPaneCaptionFactory = value;
            }
        }

        private IDockPaneStripFactory m_dockPaneStripFactory = null;

        public IDockPaneStripFactory DockPaneStripFactory
        {
            get
            {
                if (this.m_dockPaneStripFactory == null)
                    this.m_dockPaneStripFactory = new DefaultDockPaneStripFactory();

                return this.m_dockPaneStripFactory;
            }
            set
            {
                if (this.DockPanel.Contents.Count > 0)
                    throw new InvalidOperationException();

                this.m_dockPaneStripFactory = value;
            }
        }

        private IAutoHideStripFactory m_autoHideStripFactory = null;

        public IAutoHideStripFactory AutoHideStripFactory
        {
            get
            {
                if (this.m_autoHideStripFactory == null)
                    this.m_autoHideStripFactory = new DefaultAutoHideStripFactory();

                return this.m_autoHideStripFactory;
            }
            set
            {
                if (this.DockPanel.Contents.Count > 0)
                    throw new InvalidOperationException();

                if (this.m_autoHideStripFactory == value)
                    return;

                this.m_autoHideStripFactory = value;
                this.DockPanel.ResetAutoHideStripControl();
            }
        }

        private IAutoHideWindowFactory m_autoHideWindowFactory;
        
        public IAutoHideWindowFactory AutoHideWindowFactory
        {
            get { return this.m_autoHideWindowFactory ?? (this.m_autoHideWindowFactory = new DefaultAutoHideWindowFactory()); }
            set
            {
                if (this.DockPanel.Contents.Count > 0)
                {
                    throw new InvalidOperationException();
                }

                if (this.m_autoHideWindowFactory == value)
                {
                    return;
                }

                this.m_autoHideWindowFactory = value;
                this.DockPanel.ResetAutoHideStripWindow();
            }
        }

        private IPaneIndicatorFactory m_PaneIndicatorFactory;

        public IPaneIndicatorFactory PaneIndicatorFactory
        {
            get { return this.m_PaneIndicatorFactory ?? (this.m_PaneIndicatorFactory = new DefaultPaneIndicatorFactory()); }
            set { this.m_PaneIndicatorFactory = value; }
        }

        private IPanelIndicatorFactory m_PanelIndicatorFactory;

        public IPanelIndicatorFactory PanelIndicatorFactory
        {
            get { return this.m_PanelIndicatorFactory ?? (this.m_PanelIndicatorFactory = new DefaultPanelIndicatorFactory()); }
            set { this.m_PanelIndicatorFactory = value; }
        }

        private IDockOutlineFactory m_DockOutlineFactory;

        public IDockOutlineFactory DockOutlineFactory
        {
            get { return this.m_DockOutlineFactory ?? (this.m_DockOutlineFactory = new DefaultDockOutlineFactory()); }
            set { this.m_DockOutlineFactory = value; }
        }
    }
}
