using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

// To simplify the process of finding the toolbox bitmap resource:
// #1 Create an internal class called "resfinder" outside of the root namespace.
// #2 Use "resfinder" in the toolbox bitmap attribute instead of the control name.
// #3 use the "<default namespace>.<resourcename>" string to locate the resource.
// See: http://www.bobpowell.net/toolboxbitmap.htm
internal class resfinder
{
}

namespace WeifenLuo.WinFormsUI.Docking
{
    [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
    public delegate IDockContent DeserializeDockContent(string persistString);

    [LocalizedDescription("DockPanel_Description")]
    [Designer("System.Windows.Forms.Design.ControlDesigner, System.Design")]
    [ToolboxBitmap(typeof(resfinder), "WeifenLuo.WinFormsUI.Docking.DockPanel.bmp")]
    [DefaultProperty("DocumentStyle")]
    [DefaultEvent("ActiveContentChanged")]
    public partial class DockPanel : Panel
    {
        private readonly FocusManagerImpl m_focusManager;
        private readonly DockPanelExtender m_extender;
        private readonly DockPaneCollection m_panes;
        private readonly FloatWindowCollection m_floatWindows;
        private AutoHideWindowControl m_autoHideWindow;
        private DockWindowCollection m_dockWindows;
        private readonly DockContent m_dummyContent; 
        private readonly Control m_dummyControl;
        
        public DockPanel()
        {
            this.ShowAutoHideContentOnHover = true;

            this.m_focusManager = new FocusManagerImpl(this);
            this.m_extender = new DockPanelExtender(this);
            this.m_panes = new DockPaneCollection();
            this.m_floatWindows = new FloatWindowCollection();

            this.SuspendLayout();

            this.m_autoHideWindow = this.Extender.AutoHideWindowFactory.CreateAutoHideWindow(this);
            this.m_autoHideWindow.Visible = false;
            this.m_autoHideWindow.ActiveContentChanged += this.m_autoHideWindow_ActiveContentChanged;
            this.SetAutoHideWindowParent();

            this.m_dummyControl = new DummyControl();
            this.m_dummyControl.Bounds = new Rectangle(0, 0, 1, 1);
            this.Controls.Add(this.m_dummyControl);

            this.LoadDockWindows();

            this.m_dummyContent = new DockContent();
            this.ResumeLayout();
        }

        private Color m_BackColor;
        /// <summary>
        /// Determines the color with which the client rectangle will be drawn.
        /// If this property is used instead of the BackColor it will not have any influence on the borders to the surrounding controls (DockPane).
        /// The BackColor property changes the borders of surrounding controls (DockPane).
        /// Alternatively both properties may be used (BackColor to draw and define the color of the borders and DockBackColor to define the color of the client rectangle). 
        /// For Backgroundimages: Set your prefered Image, then set the DockBackColor and the BackColor to the same Color (Control)
        /// </summary>
        [Description("Determines the color with which the client rectangle will be drawn.\r\n" +
            "If this property is used instead of the BackColor it will not have any influence on the borders to the surrounding controls (DockPane).\r\n" +
            "The BackColor property changes the borders of surrounding controls (DockPane).\r\n" +
            "Alternatively both properties may be used (BackColor to draw and define the color of the borders and DockBackColor to define the color of the client rectangle).\r\n" +
            "For Backgroundimages: Set your prefered Image, then set the DockBackColor and the BackColor to the same Color (Control).")]
        public Color DockBackColor
        {
            get
            {
                return !this.m_BackColor.IsEmpty ? this.m_BackColor : base.BackColor;
            }
            set
            {
                if (this.m_BackColor != value)
                {
                    this.m_BackColor = value;
                    this.Refresh();
                }
            }
        }

        private bool ShouldSerializeDockBackColor()
        {
            return !this.m_BackColor.IsEmpty;
        }

        private void ResetDockBackColor()
        {
            this.DockBackColor = Color.Empty;
        }

        private AutoHideStripBase m_autoHideStripControl = null;
        internal AutoHideStripBase AutoHideStripControl
        {
            get
            {	
                if (this.m_autoHideStripControl == null)
                {
                    this.m_autoHideStripControl = this.AutoHideStripFactory.CreateAutoHideStrip(this);
                    this.Controls.Add(this.m_autoHideStripControl);
                }
                return this.m_autoHideStripControl;
            }
        }
        internal void ResetAutoHideStripControl()
        {
            if (this.m_autoHideStripControl != null)
                this.m_autoHideStripControl.Dispose();

            this.m_autoHideStripControl = null;
        }

        private void MdiClientHandleAssigned(object sender, EventArgs e)
        {
            this.SetMdiClient();
            this.PerformLayout();
        }

        private void MdiClient_Layout(object sender, LayoutEventArgs e)
        {
            if (this.DocumentStyle != DocumentStyle.DockingMdi)
                return;

            foreach (DockPane pane in this.Panes)
                if (pane.DockState == DockState.Document)
                    pane.SetContentBounds();

            this.InvalidateWindowRegion();
        }

        private bool m_disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!this.m_disposed && disposing)
            {
                this.m_focusManager.Dispose();
                if (this.m_mdiClientController != null)
                {
                    this.m_mdiClientController.HandleAssigned -= new EventHandler(this.MdiClientHandleAssigned);
                    this.m_mdiClientController.MdiChildActivate -= new EventHandler(this.ParentFormMdiChildActivate);
                    this.m_mdiClientController.Layout -= new LayoutEventHandler(this.MdiClient_Layout);
                    this.m_mdiClientController.Dispose();
                }
                this.FloatWindows.Dispose();
                this.Panes.Dispose();
                this.DummyContent.Dispose();

                this.m_disposed = true;
            }
                
            base.Dispose(disposing);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDockContent ActiveAutoHideContent
        {
            get	{	return this.AutoHideWindow.ActiveContent;	}
            set	{ this.AutoHideWindow.ActiveContent = value;	}
        }

        private bool m_allowEndUserDocking = !Win32Helper.IsRunningOnMono;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_AllowEndUserDocking_Description")]
        [DefaultValue(true)]
        public bool AllowEndUserDocking
        {
            get
            {
                if (Win32Helper.IsRunningOnMono && this.m_allowEndUserDocking)
                    this.m_allowEndUserDocking = false;

                return this.m_allowEndUserDocking;
            }
            set
            {
                if (Win32Helper.IsRunningOnMono && value)
                    throw new InvalidOperationException("AllowEndUserDocking can only be false if running on Mono");

                this.m_allowEndUserDocking = value;
            }
        }

        private bool m_allowEndUserNestedDocking = !Win32Helper.IsRunningOnMono;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_AllowEndUserNestedDocking_Description")]
        [DefaultValue(true)]
        public bool AllowEndUserNestedDocking
        {
            get
            {
                if (Win32Helper.IsRunningOnMono && this.m_allowEndUserDocking)
                    this.m_allowEndUserDocking = false;
                return this.m_allowEndUserNestedDocking;
            }
            set
            {
                if (Win32Helper.IsRunningOnMono && value)
                    throw new InvalidOperationException("AllowEndUserNestedDocking can only be false if running on Mono");

                this.m_allowEndUserNestedDocking = value;
            }
        }

		private bool m_allowChangeLayout = true;
		[LocalizedCategory( "Category_Docking" )]
		[LocalizedDescription( "DockPanel_AllowChangeLayout_Description" )]
		[DefaultValue( true )]
		public bool AllowChangeLayout {
			get {
				return this.m_allowChangeLayout;
			}
			set {
				if (this.m_allowChangeLayout == value )
					return;

                this.m_allowChangeLayout = value;
				foreach ( FloatWindow floatWindow in this.FloatWindows )
					floatWindow.AllowChangeLayout = value;
			}
		}

		// :(
		private bool m_canCloseFloatWindowInLock = false;
		[LocalizedCategory( "Category_Docking" )]
		[LocalizedDescription( "DockPanel_CanCloseFloatWindowInLock_Description" )]
		[DefaultValue( false )]
		public bool CanCloseFloatWindowInLock {
			get {
				return this.m_canCloseFloatWindowInLock;
			}
			set {
				if (this.m_canCloseFloatWindowInLock == value )
					return;

                this.m_canCloseFloatWindowInLock = value;
			}
		}
        private bool m_CanSizableFloatWindowInLock = false;
        [DefaultValue(false)]
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
                foreach (FloatWindow floatWindow in this.FloatWindows)
                    floatWindow.CanSizableFloatWindowInLock = value;
            }
        }

        private DockContentCollection m_contents = new DockContentCollection();
        [Browsable(false)]
        public DockContentCollection Contents
        {
            get	{	return this.m_contents;	}
        }

        internal DockContent DummyContent
        {
            get	{	return this.m_dummyContent;	}
        }

        private bool m_rightToLeftLayout = false;
        [DefaultValue(false)]
        [LocalizedCategory("Appearance")]
        [LocalizedDescription("DockPanel_RightToLeftLayout_Description")]
        public bool RightToLeftLayout
        {
            get { return this.m_rightToLeftLayout; }
            set
            {
                if (this.m_rightToLeftLayout == value)
                    return;

                this.m_rightToLeftLayout = value;
                foreach (FloatWindow floatWindow in this.FloatWindows)
                    floatWindow.RightToLeftLayout = value;
            }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            foreach (FloatWindow floatWindow in this.FloatWindows)
            {
                if (floatWindow.RightToLeft != this.RightToLeft)
                    floatWindow.RightToLeft = this.RightToLeft;
            }
        }

        private bool m_showDocumentIcon = false;
        [DefaultValue(false)]
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_ShowDocumentIcon_Description")]
        public bool ShowDocumentIcon
        {
            get	{	return this.m_showDocumentIcon;	}
            set
            {
                if (this.m_showDocumentIcon == value)
                    return;

                this.m_showDocumentIcon = value;
                this.Refresh();

				foreach ( FloatWindow floatWindow in this.FloatWindows ) {
					floatWindow.Refresh();
				}
            }
        }

        private DocumentTabStripLocation m_documentTabStripLocation = DocumentTabStripLocation.Top;
        [DefaultValue(DocumentTabStripLocation.Top)]
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DocumentTabStripLocation")]
        public DocumentTabStripLocation DocumentTabStripLocation
        {
            get { return this.m_documentTabStripLocation; }
            set { this.m_documentTabStripLocation = value; }
        }

        [Browsable(false)]
        public DockPanelExtender Extender
        {
            get	{	return this.m_extender;	}
        }

        [Browsable(false)]
        public DockPanelExtender.IDockPaneFactory DockPaneFactory
        {
            get	{	return this.Extender.DockPaneFactory;	}
        }

        [Browsable(false)]
        public DockPanelExtender.IFloatWindowFactory FloatWindowFactory
        {
            get	{	return this.Extender.FloatWindowFactory;	}
        }

        [Browsable(false)]
        public DockPanelExtender.IDockWindowFactory DockWindowFactory
        {
            get { return this.Extender.DockWindowFactory; }
        }

        internal DockPanelExtender.IDockPaneCaptionFactory DockPaneCaptionFactory
        {
            get	{	return this.Extender.DockPaneCaptionFactory;	}
        }

        internal DockPanelExtender.IDockPaneStripFactory DockPaneStripFactory
        {
            get	{	return this.Extender.DockPaneStripFactory;	}
        }

        internal DockPanelExtender.IAutoHideStripFactory AutoHideStripFactory
        {
            get	{	return this.Extender.AutoHideStripFactory;	}
        }

        [Browsable(false)]
        public DockPaneCollection Panes
        {
            get	{	return this.m_panes;	}
        }

        public Rectangle DockArea
        {
            get
            {
                return new Rectangle(this.DockPadding.Left, this.DockPadding.Top,
                    this.ClientRectangle.Width - this.DockPadding.Left - this.DockPadding.Right,
                    this.ClientRectangle.Height - this.DockPadding.Top - this.DockPadding.Bottom);
            }
        }

        private double m_dockBottomPortion = 0.25;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockBottomPortion_Description")]
        [DefaultValue(0.25)]
        public double DockBottomPortion
        {
            get	{	return this.m_dockBottomPortion;	}
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                if (value == this.m_dockBottomPortion)
                    return;

                this.m_dockBottomPortion = value;

                if (this.m_dockBottomPortion < 1 && this.m_dockTopPortion < 1)
                {
                    if (this.m_dockTopPortion + this.m_dockBottomPortion > 1)
                        this.m_dockTopPortion = 1 - this.m_dockBottomPortion;
                }

                this.PerformLayout();
            }
        }

        private double m_dockLeftPortion = 0.25;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockLeftPortion_Description")]
        [DefaultValue(0.25)]
        public double DockLeftPortion
        {
            get	{	return this.m_dockLeftPortion;	}
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                if (value == this.m_dockLeftPortion)
                    return;

                this.m_dockLeftPortion = value;

                if (this.m_dockLeftPortion < 1 && this.m_dockRightPortion < 1)
                {
                    if (this.m_dockLeftPortion + this.m_dockRightPortion > 1)
                        this.m_dockRightPortion = 1 - this.m_dockLeftPortion;
                }
                this.PerformLayout();
            }
        }

        private double m_dockRightPortion = 0.25;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockRightPortion_Description")]
        [DefaultValue(0.25)]
        public double DockRightPortion
        {
            get	{	return this.m_dockRightPortion;	}
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                if (value == this.m_dockRightPortion)
                    return;

                this.m_dockRightPortion = value;

                if (this.m_dockLeftPortion < 1 && this.m_dockRightPortion < 1)
                {
                    if (this.m_dockLeftPortion + this.m_dockRightPortion > 1)
                        this.m_dockLeftPortion = 1 - this.m_dockRightPortion;
                }
                this.PerformLayout();
            }
        }

        private double m_dockTopPortion = 0.25;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockTopPortion_Description")]
        [DefaultValue(0.25)]
        public double DockTopPortion
        {
            get	{	return this.m_dockTopPortion;	}
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                if (value == this.m_dockTopPortion)
                    return;

                this.m_dockTopPortion = value;

                if (this.m_dockTopPortion < 1 && this.m_dockBottomPortion < 1)
                {
                    if (this.m_dockTopPortion + this.m_dockBottomPortion > 1)
                        this.m_dockBottomPortion = 1 - this.m_dockTopPortion;
                }
                this.PerformLayout();
            }
        }

        [Browsable(false)]
        public DockWindowCollection DockWindows
        {
            get	{	return this.m_dockWindows;	}
        }

        public void UpdateDockWindowZOrder(DockStyle dockStyle, bool fullPanelEdge)
        {
            if (dockStyle == DockStyle.Left)
            {
                if (fullPanelEdge)
                    this.DockWindows[DockState.DockLeft].SendToBack();
                else
                    this.DockWindows[DockState.DockLeft].BringToFront();
            }
            else if (dockStyle == DockStyle.Right)
            {
                if (fullPanelEdge)
                    this.DockWindows[DockState.DockRight].SendToBack();
                else
                    this.DockWindows[DockState.DockRight].BringToFront();
            }
            else if (dockStyle == DockStyle.Top)
            {
                if (fullPanelEdge)
                    this.DockWindows[DockState.DockTop].SendToBack();
                else
                    this.DockWindows[DockState.DockTop].BringToFront();
            }
            else if (dockStyle == DockStyle.Bottom)
            {
                if (fullPanelEdge)
                    this.DockWindows[DockState.DockBottom].SendToBack();
                else
                    this.DockWindows[DockState.DockBottom].BringToFront();
            }
        }

        [Browsable(false)]
        public int DocumentsCount
        {
            get
            {
                int count = 0;
                foreach (IDockContent content in this.Documents)
                    count++;

                return count;
            }
        }

        public IDockContent[] DocumentsToArray()
        {
            int count = this.DocumentsCount;
            IDockContent[] documents = new IDockContent[count];
            int i = 0;
            foreach (IDockContent content in this.Documents)
            {
                documents[i] = content;
                i++;
            }

            return documents;
        }

        [Browsable(false)]
        public IEnumerable<IDockContent> Documents
        {
            get
            {
                foreach (IDockContent content in this.Contents)
                {
                    if (content.DockHandler.DockState == DockState.Document)
                        yield return content;
                }
            }
        }

        private Control DummyControl
        {
            get	{	return this.m_dummyControl;	}
        }

        [Browsable(false)]
        public FloatWindowCollection FloatWindows
        {
            get	{	return this.m_floatWindows;	}
        }

        private Size m_defaultFloatWindowSize = new Size(300, 300);
        [Category("Layout")]
        [LocalizedDescription("DockPanel_DefaultFloatWindowSize_Description")]
        public Size DefaultFloatWindowSize
        {
            get { return this.m_defaultFloatWindowSize; }
            set { this.m_defaultFloatWindowSize = value; }
        }
        private bool ShouldSerializeDefaultFloatWindowSize()
        {
            return this.DefaultFloatWindowSize != new Size(300, 300);
        }
        private void ResetDefaultFloatWindowSize()
        {
            this.DefaultFloatWindowSize = new Size(300, 300);
        }

        private DocumentStyle m_documentStyle = DocumentStyle.DockingMdi;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DocumentStyle_Description")]
        [DefaultValue(DocumentStyle.DockingMdi)]
        public DocumentStyle DocumentStyle
        {
            get	{	return this.m_documentStyle;	}
            set
            {
                if (value == this.m_documentStyle)
                    return;

                if (!Enum.IsDefined(typeof(DocumentStyle), value))
                    throw new InvalidEnumArgumentException();

                if (value == DocumentStyle.SystemMdi && this.DockWindows[DockState.Document].VisibleNestedPanes.Count > 0)
                    throw new InvalidEnumArgumentException();

                this.m_documentStyle = value;

                this.SuspendLayout(true);

                this.SetAutoHideWindowParent();
                this.SetMdiClient();
                this.InvalidateWindowRegion();

                foreach (IDockContent content in this.Contents)
                {
                    if (content.DockHandler.DockState == DockState.Document)
                        content.DockHandler.SetPaneAndVisible(content.DockHandler.Pane);
                }

                this.PerformMdiClientLayout();

                this.ResumeLayout(true, true);
            }
        }

        private bool _supprtDeeplyNestedContent = false;
        [LocalizedCategory("Category_Performance")]
        [LocalizedDescription("DockPanel_SupportDeeplyNestedContent_Description")]
        [DefaultValue(false)]
        public bool SupportDeeplyNestedContent
        {
            get { return this._supprtDeeplyNestedContent; }
            set { this._supprtDeeplyNestedContent = value; }
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_ShowAutoHideContentOnHover_Description")]
        [DefaultValue(true)]
        public bool ShowAutoHideContentOnHover { get; set; }

        public int GetDockWindowSize(DockState dockState)
        {
            if (dockState == DockState.DockLeft || dockState == DockState.DockRight)
            {
                int width = this.ClientRectangle.Width - this.DockPadding.Left - this.DockPadding.Right;
                int dockLeftSize = this.m_dockLeftPortion >= 1 ? (int)this.m_dockLeftPortion : (int)(width * this.m_dockLeftPortion);
                int dockRightSize = this.m_dockRightPortion >= 1 ? (int)this.m_dockRightPortion : (int)(width * this.m_dockRightPortion);

                if (dockLeftSize < MeasurePane.MinSize)
                    dockLeftSize = MeasurePane.MinSize;
                if (dockRightSize < MeasurePane.MinSize)
                    dockRightSize = MeasurePane.MinSize;

                if (dockLeftSize + dockRightSize > width - MeasurePane.MinSize)
                {
                    int adjust = (dockLeftSize + dockRightSize) - (width - MeasurePane.MinSize);
                    dockLeftSize -= adjust / 2;
                    dockRightSize -= adjust / 2;
                }

                return dockState == DockState.DockLeft ? dockLeftSize : dockRightSize;
            }
            else if (dockState == DockState.DockTop || dockState == DockState.DockBottom)
            {
                int height = this.ClientRectangle.Height - this.DockPadding.Top - this.DockPadding.Bottom;
                int dockTopSize = this.m_dockTopPortion >= 1 ? (int)this.m_dockTopPortion : (int)(height * this.m_dockTopPortion);
                int dockBottomSize = this.m_dockBottomPortion >= 1 ? (int)this.m_dockBottomPortion : (int)(height * this.m_dockBottomPortion);

                if (dockTopSize < MeasurePane.MinSize)
                    dockTopSize = MeasurePane.MinSize;
                if (dockBottomSize < MeasurePane.MinSize)
                    dockBottomSize = MeasurePane.MinSize;

                if (dockTopSize + dockBottomSize > height - MeasurePane.MinSize)
                {
                    int adjust = (dockTopSize + dockBottomSize) - (height - MeasurePane.MinSize);
                    dockTopSize -= adjust / 2;
                    dockBottomSize -= adjust / 2;
                }

                return dockState == DockState.DockTop ? dockTopSize : dockBottomSize;
            }
            else
                return 0;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.SuspendLayout(true);

            this.AutoHideStripControl.Bounds = this.ClientRectangle;

            this.CalculateDockPadding();

            this.DockWindows[DockState.DockLeft].Width = this.GetDockWindowSize(DockState.DockLeft);
            this.DockWindows[DockState.DockRight].Width = this.GetDockWindowSize(DockState.DockRight);
            this.DockWindows[DockState.DockTop].Height = this.GetDockWindowSize(DockState.DockTop);
            this.DockWindows[DockState.DockBottom].Height = this.GetDockWindowSize(DockState.DockBottom);

            this.AutoHideWindow.Bounds = this.GetAutoHideWindowBounds(this.AutoHideWindowRectangle);

            this.DockWindows[DockState.Document].BringToFront();
            this.AutoHideWindow.BringToFront();

            base.OnLayout(levent);

            if (this.DocumentStyle == DocumentStyle.SystemMdi && this.MdiClientExists)
            {
                this.SetMdiClientBounds(this.SystemMdiClientBounds);
                this.InvalidateWindowRegion();
            }
            else if (this.DocumentStyle == DocumentStyle.DockingMdi)
                this.InvalidateWindowRegion();

            this.ResumeLayout(true, true);
        }

        internal Rectangle GetTabStripRectangle(DockState dockState)
        {
            return this.AutoHideStripControl.GetTabStripRectangle(dockState);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.DockBackColor == this.BackColor) return;

            Graphics g = e.Graphics;
            SolidBrush bgBrush = new SolidBrush(this.DockBackColor);
            g.FillRectangle(bgBrush, this.ClientRectangle);
        }

        internal void AddContent(IDockContent content)
        {
            if (content == null)
                throw(new ArgumentNullException());

            if (!this.Contents.Contains(content))
            {
                this.Contents.Add(content);
                this.OnContentAdded(new DockContentEventArgs(content));
            }
        }

        internal void AddPane(DockPane pane)
        {
            if (this.Panes.Contains(pane))
                return;

            this.Panes.Add(pane);
        }

        internal void AddFloatWindow(FloatWindow floatWindow)
        {
            if (this.FloatWindows.Contains(floatWindow))
                return;

            this.FloatWindows.Add(floatWindow);
        }

        private void CalculateDockPadding()
        {
            this.DockPadding.All = 0;

            int height = this.AutoHideStripControl.MeasureHeight();

            if (this.AutoHideStripControl.GetNumberOfPanes(DockState.DockLeftAutoHide) > 0)
                this.DockPadding.Left = height;
            if (this.AutoHideStripControl.GetNumberOfPanes(DockState.DockRightAutoHide) > 0)
                this.DockPadding.Right = height;
            if (this.AutoHideStripControl.GetNumberOfPanes(DockState.DockTopAutoHide) > 0)
                this.DockPadding.Top = height;
            if (this.AutoHideStripControl.GetNumberOfPanes(DockState.DockBottomAutoHide) > 0)
                this.DockPadding.Bottom = height;
        }

        internal void RemoveContent(IDockContent content)
        {
            if (content == null)
                throw(new ArgumentNullException());
            
            if (this.Contents.Contains(content))
            {
                this.Contents.Remove(content);
                this.OnContentRemoved(new DockContentEventArgs(content));
            }
        }

        internal void RemovePane(DockPane pane)
        {
            if (!this.Panes.Contains(pane))
                return;

            this.Panes.Remove(pane);
        }

        internal void RemoveFloatWindow(FloatWindow floatWindow)
        {
            if (!this.FloatWindows.Contains(floatWindow))
                return;

            this.FloatWindows.Remove(floatWindow);
            if (this.FloatWindows.Count != 0)
                return;

            if (this.ParentForm == null) 
                return;

            this.ParentForm.Focus();
        }

        public void SetPaneIndex(DockPane pane, int index)
        {
            int oldIndex = this.Panes.IndexOf(pane);
            if (oldIndex == -1)
                throw(new ArgumentException(Strings.DockPanel_SetPaneIndex_InvalidPane));

            if (index < 0 || index > this.Panes.Count - 1)
                if (index != -1)
                    throw(new ArgumentOutOfRangeException(Strings.DockPanel_SetPaneIndex_InvalidIndex));
                
            if (oldIndex == index)
                return;
            if (oldIndex == this.Panes.Count - 1 && index == -1)
                return;

            this.Panes.Remove(pane);
            if (index == -1)
                this.Panes.Add(pane);
            else if (oldIndex < index)
                this.Panes.AddAt(pane, index - 1);
            else
                this.Panes.AddAt(pane, index);
        }

        public void SuspendLayout(bool allWindows)
        {
            this.FocusManager.SuspendFocusTracking();
            this.SuspendLayout();
            if (allWindows)
                this.SuspendMdiClientLayout();
        }

        public void ResumeLayout(bool performLayout, bool allWindows)
        {
            this.FocusManager.ResumeFocusTracking();
            this.ResumeLayout(performLayout);
            if (allWindows)
                this.ResumeMdiClientLayout(performLayout);
        }

        internal Form ParentForm
        {
            get
            {	
                if (!this.IsParentFormValid())
                    throw new InvalidOperationException(Strings.DockPanel_ParentForm_Invalid);

                return this.GetMdiClientController().ParentForm;
            }
        }

        private bool IsParentFormValid()
        {
            if (this.DocumentStyle == DocumentStyle.DockingSdi || this.DocumentStyle == DocumentStyle.DockingWindow)
                return true;

            if (!this.MdiClientExists)
                this.GetMdiClientController().RenewMdiClient();

            return (this.MdiClientExists);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            this.SetAutoHideWindowParent();
            this.GetMdiClientController().ParentForm = (this.Parent as Form);
            base.OnParentChanged (e);
        }

        private void SetAutoHideWindowParent()
        {
            Control parent;
            if (this.DocumentStyle == DocumentStyle.DockingMdi ||
                this.DocumentStyle == DocumentStyle.SystemMdi)
                parent = this.Parent;
            else
                parent = this;
            if (this.AutoHideWindow.Parent != parent)
            {
                this.AutoHideWindow.Parent = parent;
                this.AutoHideWindow.BringToFront();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged (e);

            if (this.Visible)
                this.SetMdiClient();
        }

        private Rectangle SystemMdiClientBounds
        {
            get
            {
                if (!this.IsParentFormValid() || !this.Visible)
                    return Rectangle.Empty;

                Rectangle rect = this.ParentForm.RectangleToClient(this.RectangleToScreen(this.DocumentWindowBounds));
                return rect;
            }
        }

        public Rectangle DocumentWindowBounds
        {
            get
            {
                Rectangle rectDocumentBounds = this.DisplayRectangle;
                if (this.DockWindows[DockState.DockLeft].Visible)
                {
                    rectDocumentBounds.X += this.DockWindows[DockState.DockLeft].Width;
                    rectDocumentBounds.Width -= this.DockWindows[DockState.DockLeft].Width;
                }
                if (this.DockWindows[DockState.DockRight].Visible)
                    rectDocumentBounds.Width -= this.DockWindows[DockState.DockRight].Width;
                if (this.DockWindows[DockState.DockTop].Visible)
                {
                    rectDocumentBounds.Y += this.DockWindows[DockState.DockTop].Height;
                    rectDocumentBounds.Height -= this.DockWindows[DockState.DockTop].Height;
                }
                if (this.DockWindows[DockState.DockBottom].Visible)
                    rectDocumentBounds.Height -= this.DockWindows[DockState.DockBottom].Height;

                return rectDocumentBounds;

            }
        }

        private PaintEventHandler m_dummyControlPaintEventHandler = null;
        private void InvalidateWindowRegion()
        {
            if (this.DesignMode)
                return;

            if (this.m_dummyControlPaintEventHandler == null)
                this.m_dummyControlPaintEventHandler = new PaintEventHandler(this.DummyControl_Paint);

            this.DummyControl.Paint += this.m_dummyControlPaintEventHandler;
            this.DummyControl.Invalidate();
        }

        void DummyControl_Paint(object sender, PaintEventArgs e)
        {
            this.DummyControl.Paint -= this.m_dummyControlPaintEventHandler;
            this.UpdateWindowRegion();
        }

        private void UpdateWindowRegion()
        {
            if (this.DocumentStyle == DocumentStyle.DockingMdi)
                this.UpdateWindowRegion_ClipContent();
            else if (this.DocumentStyle == DocumentStyle.DockingSdi ||
                this.DocumentStyle == DocumentStyle.DockingWindow)
                this.UpdateWindowRegion_FullDocumentArea();
            else if (this.DocumentStyle == DocumentStyle.SystemMdi)
                this.UpdateWindowRegion_EmptyDocumentArea();
        }

        private void UpdateWindowRegion_FullDocumentArea()
        {
            this.SetRegion(null);
        }

        private void UpdateWindowRegion_EmptyDocumentArea()
        {
            Rectangle rect = this.DocumentWindowBounds;
            this.SetRegion(new Rectangle[] { rect });
        }

        private void UpdateWindowRegion_ClipContent()
        {
            int count = 0;
            foreach (DockPane pane in this.Panes)
            {
                if (!pane.Visible || pane.DockState != DockState.Document)
                    continue;

                count ++;
            }

            if (count == 0)
            {
                this.SetRegion(null);
                return;
            }

            Rectangle[] rects = new Rectangle[count];
            int i = 0;
            foreach (DockPane pane in this.Panes)
            {
                if (!pane.Visible || pane.DockState != DockState.Document)
                    continue;

                rects[i] = this.RectangleToClient(pane.RectangleToScreen(pane.ContentRectangle));
                i++;
            }

            this.SetRegion(rects);
        }

        private Rectangle[] m_clipRects = null;
        private void SetRegion(Rectangle[] clipRects)
        {
            if (!this.IsClipRectsChanged(clipRects))
                return;

            this.m_clipRects = clipRects;

            if (this.m_clipRects == null || this.m_clipRects.GetLength(0) == 0)
                this.Region = null;
            else
            {
                Region region = new Region(new Rectangle(0, 0, this.Width, this.Height));
                foreach (Rectangle rect in this.m_clipRects)
                    region.Exclude(rect);
                if (this.Region != null)
                {
                    this.Region.Dispose();
                }

                this.Region = region;
            }
        }

        private bool IsClipRectsChanged(Rectangle[] clipRects)
        {
            if (clipRects == null && this.m_clipRects == null)
                return false;
            else if ((clipRects == null) != (this.m_clipRects == null))
                return true;

            foreach (Rectangle rect in clipRects)
            {
                bool matched = false;
                foreach (Rectangle rect2 in this.m_clipRects)
                {
                    if (rect == rect2)
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                    return true;
            }

            foreach (Rectangle rect2 in this.m_clipRects)
            {
                bool matched = false;
                foreach (Rectangle rect in clipRects)
                {
                    if (rect == rect2)
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                    return true;
            }
            return false;
        }

        private static readonly object ActiveAutoHideContentChangedEvent = new object();
        [LocalizedCategory("Category_DockingNotification")]
        [LocalizedDescription("DockPanel_ActiveAutoHideContentChanged_Description")]
        public event EventHandler ActiveAutoHideContentChanged
        {
            add { this.Events.AddHandler(ActiveAutoHideContentChangedEvent, value); }
            remove { this.Events.RemoveHandler(ActiveAutoHideContentChangedEvent, value); }
        }
        protected virtual void OnActiveAutoHideContentChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)this.Events[ActiveAutoHideContentChangedEvent];
            if (handler != null)
                handler(this, e);
        }
        private void m_autoHideWindow_ActiveContentChanged(object sender, EventArgs e)
        {
            this.OnActiveAutoHideContentChanged(e);
        }


        private static readonly object ContentAddedEvent = new object();
        [LocalizedCategory("Category_DockingNotification")]
        [LocalizedDescription("DockPanel_ContentAdded_Description")]
        public event EventHandler<DockContentEventArgs> ContentAdded
        {
            add	{ this.Events.AddHandler(ContentAddedEvent, value);	}
            remove	{ this.Events.RemoveHandler(ContentAddedEvent, value);	}
        }
        protected virtual void OnContentAdded(DockContentEventArgs e)
        {
            EventHandler<DockContentEventArgs> handler = (EventHandler<DockContentEventArgs>)this.Events[ContentAddedEvent];
            if (handler != null)
                handler(this, e);
        }

        private static readonly object ContentRemovedEvent = new object();
        [LocalizedCategory("Category_DockingNotification")]
        [LocalizedDescription("DockPanel_ContentRemoved_Description")]
        public event EventHandler<DockContentEventArgs> ContentRemoved
        {
            add	{ this.Events.AddHandler(ContentRemovedEvent, value);	}
            remove	{ this.Events.RemoveHandler(ContentRemovedEvent, value);	}
        }
        protected virtual void OnContentRemoved(DockContentEventArgs e)
        {
            EventHandler<DockContentEventArgs> handler = (EventHandler<DockContentEventArgs>)this.Events[ContentRemovedEvent];
            if (handler != null)
                handler(this, e);
        }

        internal void ReloadDockWindows()
        {
            var old = this.m_dockWindows;
            this.LoadDockWindows();
            foreach (var dockWindow in old)
            {
                this.Controls.Remove(dockWindow);
                dockWindow.Dispose();
            }
        }

        internal void LoadDockWindows()
        {
            this.m_dockWindows = new DockWindowCollection(this);
            foreach (var dockWindow in this.DockWindows)
            {
                this.Controls.Add(dockWindow);
            }
        }

        public void ResetAutoHideStripWindow()
        {
            var old = this.m_autoHideWindow;
            this.m_autoHideWindow = this.Extender.AutoHideWindowFactory.CreateAutoHideWindow(this);
            this.m_autoHideWindow.Visible = false;
            this.SetAutoHideWindowParent();

            old.Visible = false;
            old.Parent = null;
            old.Dispose();
        }
    }
}
