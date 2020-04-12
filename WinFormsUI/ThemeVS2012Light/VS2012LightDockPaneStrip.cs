using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    using WeifenLuo.WinFormsUI.ThemeVS2012Light;

    internal class VS2012LightDockPaneStrip : DockPaneStripBase
    {
        private class TabVS2012Light : Tab
        {
            public TabVS2012Light(IDockContent content)
                : base(content)
            {
            }

            private int m_tabX;
            public int TabX
            {
                get { return this.m_tabX; }
                set { this.m_tabX = value; }
            }

            private int m_tabWidth;
            public int TabWidth
            {
                get { return this.m_tabWidth; }
                set { this.m_tabWidth = value; }
            }

            private int m_maxWidth;
            public int MaxWidth
            {
                get { return this.m_maxWidth; }
                set { this.m_maxWidth = value; }
            }

            private bool m_flag;
            protected internal bool Flag
            {
                get { return this.m_flag; }
                set { this.m_flag = value; }
            }
        }

        protected internal override Tab CreateTab(IDockContent content)
        {
            return new TabVS2012Light(content);
        }

        private sealed class InertButton : InertButtonBase
        {
            private Bitmap m_image0, m_image1;

            public InertButton(Bitmap image0, Bitmap image1)
                : base()
            {
                this.m_image0 = image0;
                this.m_image1 = image1;
            }

            private int m_imageCategory = 0;
            public int ImageCategory
            {
                get { return this.m_imageCategory; }
                set
                {
                    if (this.m_imageCategory == value)
                        return;

                    this.m_imageCategory = value;
                    this.Invalidate();
                }
            }

            public override Bitmap Image
            {
                get { return this.ImageCategory == 0 ? this.m_image0 : this.m_image1; }
            }
        }

        #region Constants

        private const int _ToolWindowStripGapTop = 0;
        private const int _ToolWindowStripGapBottom = 1;
        private const int _ToolWindowStripGapLeft = 0;
        private const int _ToolWindowStripGapRight = 0;
        private const int _ToolWindowImageHeight = 16;
        private const int _ToolWindowImageWidth = 0;//16;
        private const int _ToolWindowImageGapTop = 3;
        private const int _ToolWindowImageGapBottom = 1;
        private const int _ToolWindowImageGapLeft = 2;
        private const int _ToolWindowImageGapRight = 0;
        private const int _ToolWindowTextGapRight = 3;
        private const int _ToolWindowTabSeperatorGapTop = 3;
        private const int _ToolWindowTabSeperatorGapBottom = 3;

        private const int _DocumentStripGapTop = 0;
        private const int _DocumentStripGapBottom = 0;
        private const int _DocumentTabMaxWidth = 200;
        private const int _DocumentButtonGapTop = 3;
        private const int _DocumentButtonGapBottom = 3;
        private const int _DocumentButtonGapBetween = 0;
        private const int _DocumentButtonGapRight = 3;
        private const int _DocumentTabGapTop = 0;//3;
        private const int _DocumentTabGapLeft = 0;//3;
        private const int _DocumentTabGapRight = 0;//3;
        private const int _DocumentIconGapBottom = 2;//2;
        private const int _DocumentIconGapLeft = 8;
        private const int _DocumentIconGapRight = 0;
        private const int _DocumentIconHeight = 16;
        private const int _DocumentIconWidth = 16;
        private const int _DocumentTextGapRight = 6;

        #endregion

        #region Members

        private ContextMenuStrip m_selectMenu;
        private static Bitmap m_imageButtonClose;
        private InertButton m_buttonClose;
        private static Bitmap m_imageButtonWindowList;
        private static Bitmap m_imageButtonWindowListOverflow;
        private InertButton m_buttonWindowList;
        private IContainer m_components;
        private ToolTip m_toolTip;
        private Font m_font;
        private Font m_boldFont;
        private int m_startDisplayingTab = 0;
        private int m_endDisplayingTab = 0;
        private int m_firstDisplayingTab = 0;
        private bool m_documentTabsOverflow = false;
        private static string m_toolTipSelect;
        private static string m_toolTipClose;
        private bool m_closeButtonVisible = false;
        private Rectangle _activeClose;
        private int _selectMenuMargin = 5;

        #endregion

        #region Properties

        private Rectangle TabStripRectangle
        {
            get
            {
                if (this.Appearance == DockPane.AppearanceStyle.Document)
                    return this.TabStripRectangle_Document;
                else
                    return this.TabStripRectangle_ToolWindow;
            }
        }

        private Rectangle TabStripRectangle_ToolWindow
        {
            get
            {
                Rectangle rect = this.ClientRectangle;
                return new Rectangle(rect.X, rect.Top + ToolWindowStripGapTop, rect.Width, rect.Height - ToolWindowStripGapTop - ToolWindowStripGapBottom);
            }
        }

        private Rectangle TabStripRectangle_Document
        {
            get
            {
                Rectangle rect = this.ClientRectangle;
                return new Rectangle(rect.X, rect.Top + DocumentStripGapTop, rect.Width, rect.Height + DocumentStripGapTop - ToolWindowStripGapBottom);
            }
        }

        private Rectangle TabsRectangle
        {
            get
            {
                if (this.Appearance == DockPane.AppearanceStyle.ToolWindow)
                    return this.TabStripRectangle;

                Rectangle rectWindow = this.TabStripRectangle;
                int x = rectWindow.X;
                int y = rectWindow.Y;
                int width = rectWindow.Width;
                int height = rectWindow.Height;

                x += DocumentTabGapLeft;
                width -= DocumentTabGapLeft +
                    DocumentTabGapRight +
                    DocumentButtonGapRight +
                    this.ButtonClose.Width +
                    this.ButtonWindowList.Width +
                    2 * DocumentButtonGapBetween;

                return new Rectangle(x, y, width, height);
            }
        }

        private ContextMenuStrip SelectMenu
        {
            get { return this.m_selectMenu; }
        }

        public int SelectMenuMargin
        {
            get { return this._selectMenuMargin; }
            set { this._selectMenuMargin = value; }
        }

        private static Bitmap ImageButtonClose
        {
            get
            {
                if (m_imageButtonClose == null)
                    m_imageButtonClose = Resources.DockPane_Close;

                return m_imageButtonClose;
            }
        }

        private InertButton ButtonClose
        {
            get
            {
                if (this.m_buttonClose == null)
                {
                    this.m_buttonClose = new InertButton(ImageButtonClose, ImageButtonClose);
                    this.m_toolTip.SetToolTip(this.m_buttonClose, ToolTipClose);
                    this.m_buttonClose.Click += new EventHandler(this.Close_Click);
                    this.Controls.Add(this.m_buttonClose);
                }

                return this.m_buttonClose;
            }
        }

        private static Bitmap ImageButtonWindowList
        {
            get
            {
                if (m_imageButtonWindowList == null)
                    m_imageButtonWindowList = Resources.DockPane_Option;

                return m_imageButtonWindowList;
            }
        }

        private static Bitmap ImageButtonWindowListOverflow
        {
            get
            {
                if (m_imageButtonWindowListOverflow == null)
                    m_imageButtonWindowListOverflow = Resources.DockPane_OptionOverflow;

                return m_imageButtonWindowListOverflow;
            }
        }

        private InertButton ButtonWindowList
        {
            get
            {
                if (this.m_buttonWindowList == null)
                {
                    this.m_buttonWindowList = new InertButton(ImageButtonWindowList, ImageButtonWindowListOverflow);
                    this.m_toolTip.SetToolTip(this.m_buttonWindowList, ToolTipSelect);
                    this.m_buttonWindowList.Click += new EventHandler(this.WindowList_Click);
                    this.Controls.Add(this.m_buttonWindowList);
                }

                return this.m_buttonWindowList;
            }
        }

        private static GraphicsPath GraphicsPath
        {
            get { return VS2012LightAutoHideStrip.GraphicsPath; }
        }

        private IContainer Components
        {
            get { return this.m_components; }
        }

        public Font TextFont
        {
            get { return this.DockPane.DockPanel.Skin.DockPaneStripSkin.TextFont; }
        }

        private Font BoldFont
        {
            get
            {
                if (this.IsDisposed)
                    return null;

                if (this.m_boldFont == null)
                {
                    this.m_font = this.TextFont;
                    this.m_boldFont = new Font(this.TextFont, FontStyle.Bold);
                }
                else if (this.m_font != this.TextFont)
                {
                    this.m_boldFont.Dispose();
                    this.m_font = this.TextFont;
                    this.m_boldFont = new Font(this.TextFont, FontStyle.Bold);
                }

                return this.m_boldFont;
            }
        }

        private int StartDisplayingTab
        {
            get { return this.m_startDisplayingTab; }
            set
            {
                this.m_startDisplayingTab = value;
                this.Invalidate();
            }
        }

        private int EndDisplayingTab
        {
            get { return this.m_endDisplayingTab; }
            set { this.m_endDisplayingTab = value; }
        }

        private int FirstDisplayingTab
        {
            get { return this.m_firstDisplayingTab; }
            set { this.m_firstDisplayingTab = value; }
        }

        private bool DocumentTabsOverflow
        {
            set
            {
                if (this.m_documentTabsOverflow == value)
                    return;

                this.m_documentTabsOverflow = value;
                if (value)
                    this.ButtonWindowList.ImageCategory = 1;
                else
                    this.ButtonWindowList.ImageCategory = 0;
            }
        }

        #region Customizable Properties

        private static int ToolWindowStripGapTop
        {
            get { return _ToolWindowStripGapTop; }
        }

        private static int ToolWindowStripGapBottom
        {
            get { return _ToolWindowStripGapBottom; }
        }

        private static int ToolWindowStripGapLeft
        {
            get { return _ToolWindowStripGapLeft; }
        }

        private static int ToolWindowStripGapRight
        {
            get { return _ToolWindowStripGapRight; }
        }

        private static int ToolWindowImageHeight
        {
            get { return _ToolWindowImageHeight; }
        }

        private static int ToolWindowImageWidth
        {
            get { return _ToolWindowImageWidth; }
        }

        private static int ToolWindowImageGapTop
        {
            get { return _ToolWindowImageGapTop; }
        }

        private static int ToolWindowImageGapBottom
        {
            get { return _ToolWindowImageGapBottom; }
        }

        private static int ToolWindowImageGapLeft
        {
            get { return _ToolWindowImageGapLeft; }
        }

        private static int ToolWindowImageGapRight
        {
            get { return _ToolWindowImageGapRight; }
        }

        private static int ToolWindowTextGapRight
        {
            get { return _ToolWindowTextGapRight; }
        }

        private static int ToolWindowTabSeperatorGapTop
        {
            get { return _ToolWindowTabSeperatorGapTop; }
        }

        private static int ToolWindowTabSeperatorGapBottom
        {
            get { return _ToolWindowTabSeperatorGapBottom; }
        }

        private static string ToolTipClose
        {
            get
            {
                if (m_toolTipClose == null)
                    m_toolTipClose = Strings.DockPaneStrip_ToolTipClose;
                return m_toolTipClose;
            }
        }

        private static string ToolTipSelect
        {
            get
            {
                if (m_toolTipSelect == null)
                    m_toolTipSelect = Strings.DockPaneStrip_ToolTipWindowList;
                return m_toolTipSelect;
            }
        }

        private TextFormatFlags ToolWindowTextFormat
        {
            get
            {
                TextFormatFlags textFormat = TextFormatFlags.EndEllipsis |
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.VerticalCenter;
                if (this.RightToLeft == RightToLeft.Yes)
                    return textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                else
                    return textFormat;
            }
        }

        private static int DocumentStripGapTop
        {
            get { return _DocumentStripGapTop; }
        }

        private static int DocumentStripGapBottom
        {
            get { return _DocumentStripGapBottom; }
        }

        private TextFormatFlags DocumentTextFormat
        {
            get
            {
                TextFormatFlags textFormat = TextFormatFlags.EndEllipsis |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.HorizontalCenter;
                if (this.RightToLeft == RightToLeft.Yes)
                    return textFormat | TextFormatFlags.RightToLeft;
                else
                    return textFormat;
            }
        }

        private static int DocumentTabMaxWidth
        {
            get { return _DocumentTabMaxWidth; }
        }

        private static int DocumentButtonGapTop
        {
            get { return _DocumentButtonGapTop; }
        }

        private static int DocumentButtonGapBottom
        {
            get { return _DocumentButtonGapBottom; }
        }

        private static int DocumentButtonGapBetween
        {
            get { return _DocumentButtonGapBetween; }
        }

        private static int DocumentButtonGapRight
        {
            get { return _DocumentButtonGapRight; }
        }

        private static int DocumentTabGapTop
        {
            get { return _DocumentTabGapTop; }
        }

        private static int DocumentTabGapLeft
        {
            get { return _DocumentTabGapLeft; }
        }

        private static int DocumentTabGapRight
        {
            get { return _DocumentTabGapRight; }
        }

        private static int DocumentIconGapBottom
        {
            get { return _DocumentIconGapBottom; }
        }

        private static int DocumentIconGapLeft
        {
            get { return _DocumentIconGapLeft; }
        }

        private static int DocumentIconGapRight
        {
            get { return _DocumentIconGapRight; }
        }

        private static int DocumentIconWidth
        {
            get { return _DocumentIconWidth; }
        }

        private static int DocumentIconHeight
        {
            get { return _DocumentIconHeight; }
        }

        private static int DocumentTextGapRight
        {
            get { return _DocumentTextGapRight; }
        }

        private static Pen PenToolWindowTabBorder
        {
            get { return SystemPens.ControlDark; }
        }

        private static Pen PenDocumentTabActiveBorder
        {
            get { return SystemPens.ControlDarkDark; }
        }

        private static Pen PenDocumentTabInactiveBorder
        {
            get { return SystemPens.GrayText; }
        }

        #endregion

        #endregion

        public VS2012LightDockPaneStrip(DockPane pane)
            : base(pane)
        {
            this.SetStyle(ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

            this.SuspendLayout();

            this.m_components = new Container();
            this.m_toolTip = new ToolTip(this.Components);
            this.m_selectMenu = new ContextMenuStrip(this.Components);

            this.ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Components.Dispose();
                if (this.m_boldFont != null)
                {
                    this.m_boldFont.Dispose();
                    this.m_boldFont = null;
                }
            }
            base.Dispose(disposing);
        }

        protected internal override int MeasureHeight()
        {
            if (this.Appearance == DockPane.AppearanceStyle.ToolWindow)
                return this.MeasureHeight_ToolWindow();
            else
                return this.MeasureHeight_Document();
        }

        private int MeasureHeight_ToolWindow()
        {
            if (this.DockPane.IsAutoHide || this.Tabs.Count <= 1)
                return 0;

            int height = Math.Max(this.TextFont.Height, ToolWindowImageHeight + ToolWindowImageGapTop + ToolWindowImageGapBottom)
                + ToolWindowStripGapTop + ToolWindowStripGapBottom;

            return height;
        }

        private int MeasureHeight_Document()
        {
            int height = Math.Max(this.TextFont.Height + DocumentTabGapTop,
                this.ButtonClose.Height + DocumentButtonGapTop + DocumentButtonGapBottom)
                + DocumentStripGapBottom + DocumentStripGapTop;

            return height;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = this.TabsRectangle;
            DockPanelGradient gradient = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.DockStripGradient;

            if (this.Appearance == DockPane.AppearanceStyle.Document)
            {
                rect.X -= DocumentTabGapLeft;

                // Add these values back in so that the DockStrip color is drawn
                // beneath the close button and window list button.
                // It is possible depending on the DockPanel DocumentStyle to have
                // a Document without a DockStrip.
                rect.Width += DocumentTabGapLeft +
                    DocumentTabGapRight +
                    DocumentButtonGapRight +
                    this.ButtonClose.Width +
                    this.ButtonWindowList.Width;
            
            }
            else
            {
                gradient = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient;
            }
            Color startColor = gradient.StartColor;
            Color endColor = gradient.EndColor;
            LinearGradientMode gradientMode = gradient.LinearGradientMode;

            DrawingRoutines.SafelyDrawLinearGradient(rect, startColor, endColor, gradientMode, e.Graphics);

            base.OnPaint(e);
            this.CalculateTabs();
            if (this.Appearance == DockPane.AppearanceStyle.Document && this.DockPane.ActiveContent != null)
            {
                if (this.EnsureDocumentTabVisible(this.DockPane.ActiveContent, false))
                    this.CalculateTabs();
            }

            this.DrawTabStrip(e.Graphics);
        }

        protected override void OnRefreshChanges()
        {
            this.SetInertButtons();
            this.Invalidate();
        }

        public override GraphicsPath GetOutline(int index)
        {

            if (this.Appearance == DockPane.AppearanceStyle.Document)
                return this.GetOutline_Document(index);
            else
                return this.GetOutline_ToolWindow(index);

        }

        private GraphicsPath GetOutline_Document(int index)
        {
            Rectangle rectTab = this.GetTabRectangle(index);
            rectTab.X -= rectTab.Height / 2;
            rectTab.Intersect(this.TabsRectangle);
            rectTab = this.RectangleToScreen(DrawHelper.RtlTransform(this, rectTab));
            Rectangle rectPaneClient = this.DockPane.RectangleToScreen(this.DockPane.ClientRectangle);

            GraphicsPath path = new GraphicsPath();
            GraphicsPath pathTab = this.GetTabOutline_Document(this.Tabs[index], true, true, true);
            path.AddPath(pathTab, true);

            if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
            {
                path.AddLine(rectTab.Right, rectTab.Top, rectPaneClient.Right, rectTab.Top);
                path.AddLine(rectPaneClient.Right, rectTab.Top, rectPaneClient.Right, rectPaneClient.Top);
                path.AddLine(rectPaneClient.Right, rectPaneClient.Top, rectPaneClient.Left, rectPaneClient.Top);
                path.AddLine(rectPaneClient.Left, rectPaneClient.Top, rectPaneClient.Left, rectTab.Top);
                path.AddLine(rectPaneClient.Left, rectTab.Top, rectTab.Right, rectTab.Top);
            }
            else
            {
                path.AddLine(rectTab.Right, rectTab.Bottom, rectPaneClient.Right, rectTab.Bottom);
                path.AddLine(rectPaneClient.Right, rectTab.Bottom, rectPaneClient.Right, rectPaneClient.Bottom);
                path.AddLine(rectPaneClient.Right, rectPaneClient.Bottom, rectPaneClient.Left, rectPaneClient.Bottom);
                path.AddLine(rectPaneClient.Left, rectPaneClient.Bottom, rectPaneClient.Left, rectTab.Bottom);
                path.AddLine(rectPaneClient.Left, rectTab.Bottom, rectTab.Right, rectTab.Bottom);
            }
            return path;
        }

        private GraphicsPath GetOutline_ToolWindow(int index)
        {
            Rectangle rectTab = this.GetTabRectangle(index);
            rectTab.Intersect(this.TabsRectangle);
            rectTab = this.RectangleToScreen(DrawHelper.RtlTransform(this, rectTab));
            Rectangle rectPaneClient = this.DockPane.RectangleToScreen(this.DockPane.ClientRectangle);

            GraphicsPath path = new GraphicsPath();
            GraphicsPath pathTab = this.GetTabOutline(this.Tabs[index], true, true);
            path.AddPath(pathTab, true);
            path.AddLine(rectTab.Left, rectTab.Top, rectPaneClient.Left, rectTab.Top);
            path.AddLine(rectPaneClient.Left, rectTab.Top, rectPaneClient.Left, rectPaneClient.Top);
            path.AddLine(rectPaneClient.Left, rectPaneClient.Top, rectPaneClient.Right, rectPaneClient.Top);
            path.AddLine(rectPaneClient.Right, rectPaneClient.Top, rectPaneClient.Right, rectTab.Top);
            path.AddLine(rectPaneClient.Right, rectTab.Top, rectTab.Right, rectTab.Top);
            return path;
        }

        private void CalculateTabs()
        {
            if (this.Appearance == DockPane.AppearanceStyle.ToolWindow)
                this.CalculateTabs_ToolWindow();
            else
                this.CalculateTabs_Document();
        }

        private void CalculateTabs_ToolWindow()
        {
            if (this.Tabs.Count <= 1 || this.DockPane.IsAutoHide)
                return;

            Rectangle rectTabStrip = this.TabStripRectangle;

            // Calculate tab widths
            int countTabs = this.Tabs.Count;
            foreach (TabVS2012Light tab in this.Tabs)
            {
                tab.MaxWidth = this.GetMaxTabWidth(this.Tabs.IndexOf(tab));
                tab.Flag = false;
            }

            // Set tab whose max width less than average width
            bool anyWidthWithinAverage = true;
            int totalWidth = rectTabStrip.Width - ToolWindowStripGapLeft - ToolWindowStripGapRight;
            int totalAllocatedWidth = 0;
            int averageWidth = totalWidth / countTabs;
            int remainedTabs = countTabs;
            for (anyWidthWithinAverage = true; anyWidthWithinAverage && remainedTabs > 0; )
            {
                anyWidthWithinAverage = false;
                foreach (TabVS2012Light tab in this.Tabs)
                {
                    if (tab.Flag)
                        continue;

                    if (tab.MaxWidth <= averageWidth)
                    {
                        tab.Flag = true;
                        tab.TabWidth = tab.MaxWidth;
                        totalAllocatedWidth += tab.TabWidth;
                        anyWidthWithinAverage = true;
                        remainedTabs--;
                    }
                }
                if (remainedTabs != 0)
                    averageWidth = (totalWidth - totalAllocatedWidth) / remainedTabs;
            }

            // If any tab width not set yet, set it to the average width
            if (remainedTabs > 0)
            {
                int roundUpWidth = (totalWidth - totalAllocatedWidth) - (averageWidth * remainedTabs);
                foreach (TabVS2012Light tab in this.Tabs)
                {
                    if (tab.Flag)
                        continue;

                    tab.Flag = true;
                    if (roundUpWidth > 0)
                    {
                        tab.TabWidth = averageWidth + 1;
                        roundUpWidth--;
                    }
                    else
                        tab.TabWidth = averageWidth;
                }
            }

            // Set the X position of the tabs
            int x = rectTabStrip.X + ToolWindowStripGapLeft;
            foreach (TabVS2012Light tab in this.Tabs)
            {
                tab.TabX = x;
                x += tab.TabWidth;
            }
        }

        private bool CalculateDocumentTab(Rectangle rectTabStrip, ref int x, int index)
        {
            bool overflow = false;

            var tab = this.Tabs[index] as TabVS2012Light;
            tab.MaxWidth = this.GetMaxTabWidth(index);
            int width = Math.Min(tab.MaxWidth, DocumentTabMaxWidth);
            if (x + width < rectTabStrip.Right || index == this.StartDisplayingTab)
            {
                tab.TabX = x;
                tab.TabWidth = width;
                this.EndDisplayingTab = index;
            }
            else
            {
                tab.TabX = 0;
                tab.TabWidth = 0;
                overflow = true;
            }
            x += width;

            return overflow;
        }

        /// <summary>
        /// Calculate which tabs are displayed and in what order.
        /// </summary>
        private void CalculateTabs_Document()
        {
            if (this.m_startDisplayingTab >= this.Tabs.Count)
                this.m_startDisplayingTab = 0;

            Rectangle rectTabStrip = this.TabsRectangle;

            int x = rectTabStrip.X; //+ rectTabStrip.Height / 2;
            bool overflow = false;

            // Originally all new documents that were considered overflow
            // (not enough pane strip space to show all tabs) were added to
            // the far left (assuming not right to left) and the tabs on the
            // right were dropped from view. If StartDisplayingTab is not 0
            // then we are dealing with making sure a specific tab is kept in focus.
            if (this.m_startDisplayingTab > 0)
            {
                int tempX = x;
                var tab = this.Tabs[this.m_startDisplayingTab] as TabVS2012Light;
                tab.MaxWidth = this.GetMaxTabWidth(this.m_startDisplayingTab);

                // Add the active tab and tabs to the left
                for (int i = this.StartDisplayingTab; i >= 0; i--)
                    this.CalculateDocumentTab(rectTabStrip, ref tempX, i);

                // Store which tab is the first one displayed so that it
                // will be drawn correctly (without part of the tab cut off)
                this.FirstDisplayingTab = this.EndDisplayingTab;

                tempX = x; // Reset X location because we are starting over

                // Start with the first tab displayed - name is a little misleading.
                // Loop through each tab and set its location. If there is not enough
                // room for all of them overflow will be returned.
                for (int i = this.EndDisplayingTab; i < this.Tabs.Count; i++)
                    overflow = this.CalculateDocumentTab(rectTabStrip, ref tempX, i);

                // If not all tabs are shown then we have an overflow.
                if (this.FirstDisplayingTab != 0)
                    overflow = true;
            }
            else
            {
                for (int i = this.StartDisplayingTab; i < this.Tabs.Count; i++)
                    overflow = this.CalculateDocumentTab(rectTabStrip, ref x, i);
                for (int i = 0; i < this.StartDisplayingTab; i++)
                    overflow = this.CalculateDocumentTab(rectTabStrip, ref x, i);

                this.FirstDisplayingTab = this.StartDisplayingTab;
            }

            if (!overflow)
            {
                this.m_startDisplayingTab = 0;
                this.FirstDisplayingTab = 0;
                x = rectTabStrip.X;// +rectTabStrip.Height / 2;
                foreach (TabVS2012Light tab in this.Tabs)
                {
                    tab.TabX = x;
                    x += tab.TabWidth;
                }
            }
            this.DocumentTabsOverflow = overflow;
        }

        protected internal override void EnsureTabVisible(IDockContent content)
        {
            if (this.Appearance != DockPane.AppearanceStyle.Document || !this.Tabs.Contains(content))
                return;

            this.CalculateTabs();
            this.EnsureDocumentTabVisible(content, true);
        }

        private bool EnsureDocumentTabVisible(IDockContent content, bool repaint)
        {
            int index = this.Tabs.IndexOf(content);
            var tab = this.Tabs[index] as TabVS2012Light;
            if (tab.TabWidth != 0)
                return false;

            this.StartDisplayingTab = index;
            if (repaint)
                this.Invalidate();

            return true;
        }

        private int GetMaxTabWidth(int index)
        {
            if (this.Appearance == DockPane.AppearanceStyle.ToolWindow)
                return this.GetMaxTabWidth_ToolWindow(index);
            else
                return this.GetMaxTabWidth_Document(index);
        }

        private int GetMaxTabWidth_ToolWindow(int index)
        {
            IDockContent content = this.Tabs[index].Content;
            Size sizeString = TextRenderer.MeasureText(content.DockHandler.TabText, this.TextFont);
            return ToolWindowImageWidth + sizeString.Width + ToolWindowImageGapLeft
                + ToolWindowImageGapRight + ToolWindowTextGapRight;
        }

        private const int TAB_CLOSE_BUTTON_WIDTH = 30;

        private int GetMaxTabWidth_Document(int index)
        {
            IDockContent content = this.Tabs[index].Content;
            int height = this.GetTabRectangle_Document(index).Height;
            Size sizeText = TextRenderer.MeasureText(content.DockHandler.TabText, this.BoldFont, new Size(DocumentTabMaxWidth, height), this.DocumentTextFormat);

            int width;
            if (this.DockPane.DockPanel.ShowDocumentIcon)
                width = sizeText.Width + DocumentIconWidth + DocumentIconGapLeft + DocumentIconGapRight + DocumentTextGapRight;
            else
                width = sizeText.Width + DocumentIconGapLeft + DocumentTextGapRight;
            
            width += TAB_CLOSE_BUTTON_WIDTH;
            return width;
        }

        private void DrawTabStrip(Graphics g)
        {
            if (this.Appearance == DockPane.AppearanceStyle.Document)
                this.DrawTabStrip_Document(g);
            else
                this.DrawTabStrip_ToolWindow(g);
        }

        private void DrawTabStrip_Document(Graphics g)
        {
            int count = this.Tabs.Count;
            if (count == 0)
                return;

            Rectangle rectTabStrip = this.TabStripRectangle;
            rectTabStrip.Height += 1;

            // Draw the tabs
            Rectangle rectTabOnly = this.TabsRectangle;
            Rectangle rectTab = Rectangle.Empty;
            TabVS2012Light tabActive = null;
            g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
            for (int i = 0; i < count; i++)
            {
                rectTab = this.GetTabRectangle(i);
                if (this.Tabs[i].Content == this.DockPane.ActiveContent)
                {
                    tabActive = this.Tabs[i] as TabVS2012Light;
                    continue;
                }
                if (rectTab.IntersectsWith(rectTabOnly))
                    this.DrawTab(g, this.Tabs[i] as TabVS2012Light, rectTab);
            }

            g.SetClip(rectTabStrip);

            if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                g.DrawLine(PenDocumentTabActiveBorder, rectTabStrip.Left, rectTabStrip.Top + 1,
                    rectTabStrip.Right, rectTabStrip.Top + 1);
            else
            {
                Color tabUnderLineColor;
                if (tabActive != null && this.DockPane.IsActiveDocumentPane)
                    tabUnderLineColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.StartColor;
                else
                    tabUnderLineColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.EndColor;

                g.DrawLine(new Pen(tabUnderLineColor, 4), rectTabStrip.Left, rectTabStrip.Bottom, rectTabStrip.Right, rectTabStrip.Bottom);
            }

            g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
            if (tabActive != null)
            {
                rectTab = this.GetTabRectangle(this.Tabs.IndexOf(tabActive));
                if (rectTab.IntersectsWith(rectTabOnly))
                {
                    rectTab.Intersect(rectTabOnly);
                    this.DrawTab(g, tabActive, rectTab);
                }
            }
        }

        private void DrawTabStrip_ToolWindow(Graphics g)
        {
            Rectangle rectTabStrip = this.TabStripRectangle;

            g.DrawLine(PenToolWindowTabBorder, rectTabStrip.Left, rectTabStrip.Top,
                rectTabStrip.Right, rectTabStrip.Top);

            for (int i = 0; i < this.Tabs.Count; i++)
                this.DrawTab(g, this.Tabs[i] as TabVS2012Light, this.GetTabRectangle(i));
        }

        private Rectangle GetTabRectangle(int index)
        {
            if (this.Appearance == DockPane.AppearanceStyle.ToolWindow)
                return this.GetTabRectangle_ToolWindow(index);
            else
                return this.GetTabRectangle_Document(index);
        }

        private Rectangle GetTabRectangle_ToolWindow(int index)
        {
            Rectangle rectTabStrip = this.TabStripRectangle;

            TabVS2012Light tab = (TabVS2012Light)(this.Tabs[index]);
            return new Rectangle(tab.TabX, rectTabStrip.Y, tab.TabWidth, rectTabStrip.Height);
        }

        private Rectangle GetTabRectangle_Document(int index)
        {
            Rectangle rectTabStrip = this.TabStripRectangle;
            var tab = (TabVS2012Light)this.Tabs[index];

            Rectangle rect = new Rectangle();
            rect.X = tab.TabX;
            rect.Width = tab.TabWidth;
            rect.Height = rectTabStrip.Height - DocumentTabGapTop;

            if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                rect.Y = rectTabStrip.Y + DocumentStripGapBottom;
            else
                rect.Y = rectTabStrip.Y + DocumentTabGapTop;

            return rect;
        }

        private void DrawTab(Graphics g, TabVS2012Light tab, Rectangle rect)
        {
            if (this.Appearance == DockPane.AppearanceStyle.ToolWindow)
                this.DrawTab_ToolWindow(g, tab, rect);
            else
                this.DrawTab_Document(g, tab, rect);
        }

        private GraphicsPath GetTabOutline(Tab tab, bool rtlTransform, bool toScreen)
        {
            if (this.Appearance == DockPane.AppearanceStyle.ToolWindow)
                return this.GetTabOutline_ToolWindow(tab, rtlTransform, toScreen);
            else
                return this.GetTabOutline_Document(tab, rtlTransform, toScreen, false);
        }

        private GraphicsPath GetTabOutline_ToolWindow(Tab tab, bool rtlTransform, bool toScreen)
        {
            Rectangle rect = this.GetTabRectangle(this.Tabs.IndexOf(tab));
            if (rtlTransform)
                rect = DrawHelper.RtlTransform(this, rect);
            if (toScreen)
                rect = this.RectangleToScreen(rect);

            DrawHelper.GetRoundedCornerTab(GraphicsPath, rect, false);
            return GraphicsPath;
        }

        private GraphicsPath GetTabOutline_Document(Tab tab, bool rtlTransform, bool toScreen, bool full)
        {
            GraphicsPath.Reset();
            Rectangle rect = this.GetTabRectangle(this.Tabs.IndexOf(tab));
            
            // Shorten TabOutline so it doesn't get overdrawn by icons next to it
            rect.Intersect(this.TabsRectangle);
            rect.Width--;

            if (rtlTransform)
                rect = DrawHelper.RtlTransform(this, rect);
            if (toScreen)
                rect = this.RectangleToScreen(rect);

            GraphicsPath.AddRectangle(rect);
            return GraphicsPath;
        }

        private void DrawTab_ToolWindow(Graphics g, TabVS2012Light tab, Rectangle rect)
        {
            rect.Y += 1;
            Rectangle rectIcon = new Rectangle(
                rect.X + ToolWindowImageGapLeft,
                rect.Y - 1 + rect.Height - ToolWindowImageGapBottom - ToolWindowImageHeight,
                ToolWindowImageWidth, ToolWindowImageHeight);
            Rectangle rectText = rectIcon;
            rectText.X += rectIcon.Width + ToolWindowImageGapRight;
            rectText.Width = rect.Width - rectIcon.Width - ToolWindowImageGapLeft -
                ToolWindowImageGapRight - ToolWindowTextGapRight;

            Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
            rectText = DrawHelper.RtlTransform(this, rectText);
            rectIcon = DrawHelper.RtlTransform(this, rectIcon);
            if (this.DockPane.ActiveContent == tab.Content && ((DockContent)tab.Content).IsActivated)
            {
                Color startColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.StartColor;
                Color endColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.EndColor;
                LinearGradientMode gradientMode = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.LinearGradientMode;
                g.FillRectangle(new LinearGradientBrush(rectTab, startColor, endColor, gradientMode), rect);

                Color textColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.TextColor;
                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, textColor, this.ToolWindowTextFormat);
            }
            else
            {
                Color textColor;
                if (tab.Content == this.DockPane.MouseOverTab)
                    textColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.TextColor;
                else
                    textColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.TextColor;

                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, textColor, this.ToolWindowTextFormat);
            }

            g.DrawLine(PenToolWindowTabBorder, rect.X + rect.Width - 1, rect.Y, rect.X + rect.Width - 1, rect.Height);

            if (rectTab.Contains(rectIcon))
                g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
        }

        private void DrawTab_Document(Graphics g, TabVS2012Light tab, Rectangle rect)
        {
            if (tab.TabWidth == 0)
                return;

            var rectCloseButton = this.GetCloseButtonRect(rect);
            Rectangle rectIcon = new Rectangle(
                rect.X + DocumentIconGapLeft,
                rect.Y + rect.Height - DocumentIconGapBottom - DocumentIconHeight,
                DocumentIconWidth, DocumentIconHeight);
            Rectangle rectText = rectIcon;
            if (this.DockPane.DockPanel.ShowDocumentIcon)
            {
                rectText.X += rectIcon.Width + DocumentIconGapRight;
                rectText.Y = rect.Y;
                rectText.Width = rect.Width - rectIcon.Width - DocumentIconGapLeft - DocumentIconGapRight - DocumentTextGapRight - rectCloseButton.Width;
                rectText.Height = rect.Height;
            }
            else
                rectText.Width = rect.Width - DocumentIconGapLeft - DocumentTextGapRight - rectCloseButton.Width;

            Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
            Rectangle rectBack = DrawHelper.RtlTransform(this, rect);
            rectBack.Width += rect.X;
            rectBack.X = 0;

            rectText = DrawHelper.RtlTransform(this, rectText);
            rectIcon = DrawHelper.RtlTransform(this, rectIcon);

            Color activeColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.StartColor;
            Color lostFocusColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.EndColor;
            Color inactiveColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.StartColor;
            Color mouseHoverColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.EndColor;

            Color activeText = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.TextColor;
            Color inactiveText = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.TextColor;
            Color lostFocusText = SystemColors.GrayText;

            if (this.DockPane.ActiveContent == tab.Content)
            {
                if (this.DockPane.IsActiveDocumentPane)
                {
                    g.FillRectangle(new SolidBrush(activeColor), rect);
                    TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, activeText, this.DocumentTextFormat);
                    g.DrawImage(rectCloseButton == this.ActiveClose ? Resources.ActiveTabHover_Close : Resources.ActiveTab_Close, rectCloseButton);
                }
                else
                {
                    g.FillRectangle(new SolidBrush(lostFocusColor), rect);
                    TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, lostFocusText, this.DocumentTextFormat);
                    g.DrawImage(rectCloseButton == this.ActiveClose ? Resources.LostFocusTabHover_Close : Resources.LostFocusTab_Close, rectCloseButton);
                }
            }
            else
            {
                if (tab.Content == this.DockPane.MouseOverTab)
                {
                    g.FillRectangle(new SolidBrush(mouseHoverColor), rect);
                    TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, activeText, this.DocumentTextFormat);
                    g.DrawImage(rectCloseButton == this.ActiveClose ? Resources.InactiveTabHover_Close : Resources.ActiveTabHover_Close, rectCloseButton);
                }
                else
                {
                    g.FillRectangle(new SolidBrush(inactiveColor), rect);
                    TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, inactiveText, this.DocumentTextFormat);
                }
            }

            if (rectTab.Contains(rectIcon) && this.DockPane.DockPanel.ShowDocumentIcon)
                g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left || this.Appearance != DockPane.AppearanceStyle.Document)
                return;

            var indexHit = this.HitTest();
            if (indexHit > -1)
                this.TabCloseButtonHit(indexHit);
        }

        private void TabCloseButtonHit(int index)
        {
            var mousePos = this.PointToClient(MousePosition);
            var tabRect = this.GetTabRectangle(index);
            var closeButtonRect = this.GetCloseButtonRect(tabRect);
            var mouseRect = new Rectangle(mousePos, new Size(1, 1));
            if (closeButtonRect.IntersectsWith(mouseRect))
                this.DockPane.CloseActiveContent();
        }

        private Rectangle GetCloseButtonRect(Rectangle rectTab)
        {
            if (this.Appearance != Docking.DockPane.AppearanceStyle.Document)
            {
                return Rectangle.Empty;
            }

            const int gap = 3;
            const int imageSize = 15;
            return new Rectangle(rectTab.X + rectTab.Width - imageSize - gap - 1, rectTab.Y + gap, imageSize, imageSize);
        }

        private void WindowList_Click(object sender, EventArgs e)
        {
            this.SelectMenu.Items.Clear();
            foreach (TabVS2012Light tab in this.Tabs)
            {
                IDockContent content = tab.Content;
                ToolStripItem item = this.SelectMenu.Items.Add(content.DockHandler.TabText, content.DockHandler.Icon.ToBitmap());
                item.Tag = tab.Content;
                item.Click += new EventHandler(this.ContextMenuItem_Click);
            }

            var workingArea = Screen.GetWorkingArea(this.ButtonWindowList.PointToScreen(new Point(this.ButtonWindowList.Width / 2, this.ButtonWindowList.Height / 2)));
            var menu = new Rectangle(this.ButtonWindowList.PointToScreen(new Point(0, this.ButtonWindowList.Location.Y + this.ButtonWindowList.Height)), this.SelectMenu.Size);
            var menuMargined = new Rectangle(menu.X - this.SelectMenuMargin, menu.Y - this.SelectMenuMargin, menu.Width + this.SelectMenuMargin, menu.Height + this.SelectMenuMargin);
            if (workingArea.Contains(menuMargined))
            {
                this.SelectMenu.Show(menu.Location);
            }
            else
            {
                var newPoint = menu.Location;
                newPoint.X = DrawHelper.Balance(this.SelectMenu.Width, this.SelectMenuMargin, newPoint.X, workingArea.Left, workingArea.Right);
                newPoint.Y = DrawHelper.Balance(this.SelectMenu.Size.Height, this.SelectMenuMargin, newPoint.Y, workingArea.Top, workingArea.Bottom);
                var button = this.ButtonWindowList.PointToScreen(new Point(0, this.ButtonWindowList.Height));
                if (newPoint.Y < button.Y)
                {
                    // flip the menu up to be above the button.
                    newPoint.Y = button.Y - this.ButtonWindowList.Height;
                    this.SelectMenu.Show(newPoint, ToolStripDropDownDirection.AboveRight);
                }
                else
                {
                    this.SelectMenu.Show(newPoint);
                }
            }
        }
        
        private void ContextMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                IDockContent content = (IDockContent)item.Tag;
                this.DockPane.ActiveContent = content;
            }
        }

        private void SetInertButtons()
        {
            if (this.Appearance == DockPane.AppearanceStyle.ToolWindow)
            {
                if (this.m_buttonClose != null)
                    this.m_buttonClose.Left = -this.m_buttonClose.Width;

                if (this.m_buttonWindowList != null)
                    this.m_buttonWindowList.Left = -this.m_buttonWindowList.Width;
            }
            else
            {
                this.ButtonClose.Enabled = false;
                this.m_closeButtonVisible = false;
                this.ButtonClose.Visible = this.m_closeButtonVisible;
                this.ButtonClose.RefreshChanges();
                this.ButtonWindowList.RefreshChanges();
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (this.Appearance == DockPane.AppearanceStyle.Document)
            {
                this.LayoutButtons();
                this.OnRefreshChanges();
            }

            base.OnLayout(levent);
        }

        private void LayoutButtons()
        {
            Rectangle rectTabStrip = this.TabStripRectangle;

            // Set position and size of the buttons
            int buttonWidth = this.ButtonClose.Image.Width;
            int buttonHeight = this.ButtonClose.Image.Height;
            int height = rectTabStrip.Height - DocumentButtonGapTop - DocumentButtonGapBottom;
            if (buttonHeight < height)
            {
                buttonWidth = buttonWidth * (height / buttonHeight);
                buttonHeight = height;
            }
            Size buttonSize = new Size(buttonWidth, buttonHeight);

            int x = rectTabStrip.X + rectTabStrip.Width - DocumentTabGapLeft
                - DocumentButtonGapRight - buttonWidth;
            int y = rectTabStrip.Y + DocumentButtonGapTop;
            Point point = new Point(x, y);
            this.ButtonClose.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));

            // If the close button is not visible draw the window list button overtop.
            // Otherwise it is drawn to the left of the close button.
            if (this.m_closeButtonVisible)
                point.Offset(-(DocumentButtonGapBetween + buttonWidth), 0);

            this.ButtonWindowList.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.DockPane.CloseActiveContent();
        }

        protected internal override int HitTest(Point ptMouse)
        {
            if (!this.TabsRectangle.Contains(ptMouse))
                return -1;

            foreach (Tab tab in this.Tabs)
            {
                GraphicsPath path = this.GetTabOutline(tab, true, false);
                if (path.IsVisible(ptMouse))
                    return this.Tabs.IndexOf(tab);
            }

            return -1;
        }

        protected override Rectangle GetTabBounds(Tab tab)
        {
            GraphicsPath path = this.GetTabOutline(tab, true, false);
            RectangleF rectangle = path.GetBounds();
            return new Rectangle((int)rectangle.Left, (int)rectangle.Top, (int)rectangle.Width, (int)rectangle.Height);
        }

        private Rectangle ActiveClose
        {
            get { return this._activeClose; }
        }

        private bool SetActiveClose(Rectangle rectangle)
        {
            if (this._activeClose == rectangle)
                return false;

            this._activeClose = rectangle;
            return true;
        }

        private bool SetMouseOverTab(IDockContent content)
        {
            if (this.DockPane.MouseOverTab == content)
                return false;

            this.DockPane.MouseOverTab = content;
            return true;
        }

        protected override void OnMouseHover(EventArgs e)
        {
            int index = this.HitTest(this.PointToClient(MousePosition));
            string toolTip = string.Empty;

            base.OnMouseHover(e);

            bool tabUpdate = false;
            bool buttonUpdate = false;
            if (index != -1)
            {
                var tab = this.Tabs[index] as TabVS2012Light;
                if (this.Appearance == DockPane.AppearanceStyle.ToolWindow || this.Appearance == DockPane.AppearanceStyle.Document)
                {
                    tabUpdate = this.SetMouseOverTab(tab.Content == this.DockPane.ActiveContent ? null : tab.Content);
                }

                if (!String.IsNullOrEmpty(tab.Content.DockHandler.ToolTipText))
                    toolTip = tab.Content.DockHandler.ToolTipText;
                else if (tab.MaxWidth > tab.TabWidth)
                    toolTip = tab.Content.DockHandler.TabText;

                var mousePos = this.PointToClient(MousePosition);
                var tabRect = this.GetTabRectangle(index);
                var closeButtonRect = this.GetCloseButtonRect(tabRect);
                var mouseRect = new Rectangle(mousePos, new Size(1, 1));
                buttonUpdate = this.SetActiveClose(closeButtonRect.IntersectsWith(mouseRect) ? closeButtonRect : Rectangle.Empty);
            }
            else
            {
                tabUpdate = this.SetMouseOverTab(null);
                buttonUpdate = this.SetActiveClose(Rectangle.Empty);
            }

            if (tabUpdate || buttonUpdate)
                this.Invalidate();

            if (this.m_toolTip.GetToolTip(this) != toolTip)
            {
                this.m_toolTip.Active = false;
                this.m_toolTip.SetToolTip(this, toolTip);
                this.m_toolTip.Active = true;
            }

            // requires further tracking of mouse hover behavior,
            this.ResetMouseEventArgs();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            var tabUpdate = this.SetMouseOverTab(null);
            var buttonUpdate = this.SetActiveClose(Rectangle.Empty);
            if (tabUpdate || buttonUpdate)
                this.Invalidate();

            base.OnMouseLeave(e);
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            this.PerformLayout();
        }
    }
}
