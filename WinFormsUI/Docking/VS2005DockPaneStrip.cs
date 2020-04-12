using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class VS2005DockPaneStrip : DockPaneStripBase
    {
        private class TabVS2005 : Tab
        {
            public TabVS2005(IDockContent content)
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

        protected internal override DockPaneStripBase.Tab CreateTab(IDockContent content)
        {
            return new TabVS2005(content);
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
        private const int _ToolWindowImageWidth = 16;
        private const int _ToolWindowImageGapTop = 3;
        private const int _ToolWindowImageGapBottom = 1;
        private const int _ToolWindowImageGapLeft = 2;
        private const int _ToolWindowImageGapRight = 0;
        private const int _ToolWindowTextGapRight = 3;
        private const int _ToolWindowTabSeperatorGapTop = 3;
        private const int _ToolWindowTabSeperatorGapBottom = 3;

        private const int _DocumentStripGapTop = 0;
        private const int _DocumentStripGapBottom = 1;
        private const int _DocumentTabMaxWidth = 200;
        private const int _DocumentButtonGapTop = 4;
        private const int _DocumentButtonGapBottom = 4;
        private const int _DocumentButtonGapBetween = 0;
        private const int _DocumentButtonGapRight = 3;
        private const int _DocumentTabGapTop = 3;
        private const int _DocumentTabGapLeft = 3;
        private const int _DocumentTabGapRight = 3;
        private const int _DocumentIconGapBottom = 2;
        private const int _DocumentIconGapLeft = 8;
        private const int _DocumentIconGapRight = 0;
        private const int _DocumentIconHeight = 16;
        private const int _DocumentIconWidth = 16;
        private const int _DocumentTextGapRight = 3;

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
                return new Rectangle(rect.X, rect.Top + DocumentStripGapTop, rect.Width, rect.Height - DocumentStripGapTop - ToolWindowStripGapBottom);
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
            get { return VS2005AutoHideStrip.GraphicsPath; }
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
            get { return SystemPens.GrayText; }
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

        public VS2005DockPaneStrip(DockPane pane)
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
            foreach (TabVS2005 tab in this.Tabs)
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
                foreach (TabVS2005 tab in this.Tabs)
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
                foreach (TabVS2005 tab in this.Tabs)
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
            foreach (TabVS2005 tab in this.Tabs)
            {
                tab.TabX = x;
                x += tab.TabWidth;
            }
        }

        private bool CalculateDocumentTab(Rectangle rectTabStrip, ref int x, int index)
        {
            bool overflow = false;

            TabVS2005 tab = this.Tabs[index] as TabVS2005;
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

            int x = rectTabStrip.X + rectTabStrip.Height / 2;
            bool overflow = false;

            // Originally all new documents that were considered overflow
            // (not enough pane strip space to show all tabs) were added to
            // the far left (assuming not right to left) and the tabs on the
            // right were dropped from view. If StartDisplayingTab is not 0
            // then we are dealing with making sure a specific tab is kept in focus.
            if (this.m_startDisplayingTab > 0)
            {
                int tempX = x;
                TabVS2005 tab = this.Tabs[this.m_startDisplayingTab] as TabVS2005;
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
                x = rectTabStrip.X + rectTabStrip.Height / 2;
                foreach (TabVS2005 tab in this.Tabs)
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
            TabVS2005 tab = this.Tabs[index] as TabVS2005;
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

        private int GetMaxTabWidth_Document(int index)
        {
            IDockContent content = this.Tabs[index].Content;

            int height = this.GetTabRectangle_Document(index).Height;

            Size sizeText = TextRenderer.MeasureText(content.DockHandler.TabText, this.BoldFont, new Size(DocumentTabMaxWidth, height), this.DocumentTextFormat);

            if (this.DockPane.DockPanel.ShowDocumentIcon)
                return sizeText.Width + DocumentIconWidth + DocumentIconGapLeft + DocumentIconGapRight + DocumentTextGapRight;
            else
                return sizeText.Width + DocumentIconGapLeft + DocumentTextGapRight;
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

            // Draw the tabs
            Rectangle rectTabOnly = this.TabsRectangle;
            Rectangle rectTab = Rectangle.Empty;
            TabVS2005 tabActive = null;
            g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
            for (int i = 0; i < count; i++)
            {
                rectTab = this.GetTabRectangle(i);
                if (this.Tabs[i].Content == this.DockPane.ActiveContent)
                {
                    tabActive = this.Tabs[i] as TabVS2005;
                    continue;
                }
                if (rectTab.IntersectsWith(rectTabOnly))
                    this.DrawTab(g, this.Tabs[i] as TabVS2005, rectTab);
            }

            g.SetClip(rectTabStrip);

            if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                g.DrawLine(PenDocumentTabActiveBorder, rectTabStrip.Left, rectTabStrip.Top + 1,
                    rectTabStrip.Right, rectTabStrip.Top + 1);
            else
                g.DrawLine(PenDocumentTabActiveBorder, rectTabStrip.Left, rectTabStrip.Bottom - 1,
                    rectTabStrip.Right, rectTabStrip.Bottom - 1);

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
                this.DrawTab(g, this.Tabs[i] as TabVS2005, this.GetTabRectangle(i));
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

            TabVS2005 tab = (TabVS2005)(this.Tabs[index]);
            return new Rectangle(tab.TabX, rectTabStrip.Y, tab.TabWidth, rectTabStrip.Height);
        }

        private Rectangle GetTabRectangle_Document(int index)
        {
            Rectangle rectTabStrip = this.TabStripRectangle;
            TabVS2005 tab = (TabVS2005)this.Tabs[index];

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

        private void DrawTab(Graphics g, TabVS2005 tab, Rectangle rect)
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
            int curveSize = 6;

            GraphicsPath.Reset();
            Rectangle rect = this.GetTabRectangle(this.Tabs.IndexOf(tab));
            
            // Shorten TabOutline so it doesn't get overdrawn by icons next to it
            rect.Intersect(this.TabsRectangle);
            rect.Width--;

            if (rtlTransform)
                rect = DrawHelper.RtlTransform(this, rect);
            if (toScreen)
                rect = this.RectangleToScreen(rect);

            // Draws the full angle piece for active content (or first tab)
            if (tab.Content == this.DockPane.ActiveContent || full || this.Tabs.IndexOf(tab) == this.FirstDisplayingTab)
            {
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        // For some reason the next line draws a line that is not hidden like it is when drawing the tab strip on top.
                        // It is not needed so it has been commented out.
                        //GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right + rect.Height / 2, rect.Bottom);
                        GraphicsPath.AddLine(rect.Right + rect.Height / 2, rect.Top, rect.Right - rect.Height / 2 + curveSize / 2, rect.Bottom - curveSize / 2);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right + rect.Height / 2, rect.Bottom);
                        GraphicsPath.AddLine(rect.Right + rect.Height / 2, rect.Bottom, rect.Right - rect.Height / 2 + curveSize / 2, rect.Top + curveSize / 2);
                    }
                }
                else
                {
                    if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        // For some reason the next line draws a line that is not hidden like it is when drawing the tab strip on top.
                        // It is not needed so it has been commented out.
                        //GraphicsPath.AddLine(rect.Left, rect.Top, rect.Left - rect.Height / 2, rect.Top);
                        GraphicsPath.AddLine(rect.Left - rect.Height / 2, rect.Top, rect.Left + rect.Height / 2 - curveSize / 2, rect.Bottom - curveSize / 2);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Bottom, rect.Left - rect.Height / 2, rect.Bottom);
                        GraphicsPath.AddLine(rect.Left - rect.Height / 2, rect.Bottom, rect.Left + rect.Height / 2 - curveSize / 2, rect.Top + curveSize / 2);
                    }
                }
            }
            // Draws the partial angle for non-active content
            else
            {
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Top, rect.Right, rect.Top + rect.Height / 2);
                        GraphicsPath.AddLine(rect.Right, rect.Top + rect.Height / 2, rect.Right - rect.Height / 2 + curveSize / 2, rect.Bottom - curveSize / 2);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom - rect.Height / 2);
                        GraphicsPath.AddLine(rect.Right, rect.Bottom - rect.Height / 2, rect.Right - rect.Height / 2 + curveSize / 2, rect.Top + curveSize / 2);
                    }
                }
                else
                {
                    if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Top, rect.Left, rect.Top + rect.Height / 2);
                        GraphicsPath.AddLine(rect.Left, rect.Top + rect.Height / 2, rect.Left + rect.Height / 2 - curveSize / 2, rect.Bottom - curveSize / 2);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Bottom - rect.Height / 2);
                        GraphicsPath.AddLine(rect.Left, rect.Bottom - rect.Height / 2, rect.Left + rect.Height / 2 - curveSize / 2, rect.Top + curveSize / 2);
                    }
                }
            }

            if (this.RightToLeft == RightToLeft.Yes)
            {
                if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                {
                    // Draws the bottom horizontal line (short side)
                    GraphicsPath.AddLine(rect.Right - rect.Height / 2 - curveSize / 2, rect.Bottom, rect.Left + curveSize / 2, rect.Bottom);

                    // Drawing the rounded corner is not necessary. The path is automatically connected
                    //GraphicsPath.AddArc(new Rectangle(rect.Left, rect.Top, curveSize, curveSize), 180, 90);
                }
                else
                {
                    // Draws the bottom horizontal line (short side)
                    GraphicsPath.AddLine(rect.Right - rect.Height / 2 - curveSize / 2, rect.Top, rect.Left + curveSize / 2, rect.Top);
                    GraphicsPath.AddArc(new Rectangle(rect.Left, rect.Top, curveSize, curveSize), 180, 90);
                }
            }
            else
            {
                if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                {
                    // Draws the bottom horizontal line (short side)
                    GraphicsPath.AddLine(rect.Left + rect.Height / 2 + curveSize / 2, rect.Bottom, rect.Right - curveSize / 2, rect.Bottom);

                    // Drawing the rounded corner is not necessary. The path is automatically connected
                    //GraphicsPath.AddArc(new Rectangle(rect.Right - curveSize, rect.Bottom, curveSize, curveSize), 90, -90);
                }
                else
                {
                    // Draws the top horizontal line (short side)
                    GraphicsPath.AddLine(rect.Left + rect.Height / 2 + curveSize / 2, rect.Top, rect.Right - curveSize / 2, rect.Top);

                    // Draws the rounded corner oppposite the angled side
                    GraphicsPath.AddArc(new Rectangle(rect.Right - curveSize, rect.Top, curveSize, curveSize), -90, 90);
                }
            }

            if (this.Tabs.IndexOf(tab) != this.EndDisplayingTab &&
                (this.Tabs.IndexOf(tab) != this.Tabs.Count - 1 && this.Tabs[this.Tabs.IndexOf(tab) + 1].Content == this.DockPane.ActiveContent)
                && !full)
            {
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Bottom - curveSize / 2, rect.Left, rect.Bottom - rect.Height / 2);
                        GraphicsPath.AddLine(rect.Left, rect.Bottom - rect.Height / 2, rect.Left + rect.Height / 2, rect.Top);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Top + curveSize / 2, rect.Left, rect.Top + rect.Height / 2);
                        GraphicsPath.AddLine(rect.Left, rect.Top + rect.Height / 2, rect.Left + rect.Height / 2, rect.Bottom);
                    }
                }
                else
                {
                    if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Bottom - curveSize / 2, rect.Right, rect.Bottom - rect.Height / 2);
                        GraphicsPath.AddLine(rect.Right, rect.Bottom - rect.Height / 2, rect.Right - rect.Height / 2, rect.Top);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Top + curveSize / 2, rect.Right, rect.Top + rect.Height / 2);
                        GraphicsPath.AddLine(rect.Right, rect.Top + rect.Height / 2, rect.Right - rect.Height / 2, rect.Bottom);
                    }
                }
            }
            else
            {
                // Draw the vertical line opposite the angled side
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                        GraphicsPath.AddLine(rect.Left, rect.Bottom - curveSize / 2, rect.Left, rect.Top);
                    else
                        GraphicsPath.AddLine(rect.Left, rect.Top + curveSize / 2, rect.Left, rect.Bottom);
                }
                else
                {
                    if (this.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                        GraphicsPath.AddLine(rect.Right, rect.Bottom - curveSize / 2, rect.Right, rect.Top);
                    else
                        GraphicsPath.AddLine(rect.Right, rect.Top + curveSize / 2, rect.Right, rect.Bottom);
                }
            }

            return GraphicsPath;
        }

        private void DrawTab_ToolWindow(Graphics g, TabVS2005 tab, Rectangle rect)
        {
            Rectangle rectIcon = new Rectangle(
                rect.X + ToolWindowImageGapLeft,
                rect.Y + rect.Height - 1 - ToolWindowImageGapBottom - ToolWindowImageHeight,
                ToolWindowImageWidth, ToolWindowImageHeight);
            Rectangle rectText = rectIcon;
            rectText.X += rectIcon.Width + ToolWindowImageGapRight;
            rectText.Width = rect.Width - rectIcon.Width - ToolWindowImageGapLeft -
                ToolWindowImageGapRight - ToolWindowTextGapRight;

            Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
            rectText = DrawHelper.RtlTransform(this, rectText);
            rectIcon = DrawHelper.RtlTransform(this, rectIcon);
            GraphicsPath path = this.GetTabOutline(tab, true, false);
            if (this.DockPane.ActiveContent == tab.Content)
            {
                Color startColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.StartColor;
                Color endColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.EndColor;
                LinearGradientMode gradientMode = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.LinearGradientMode;
                g.FillPath(new LinearGradientBrush(rectTab, startColor, endColor, gradientMode), path);
                g.DrawPath(PenToolWindowTabBorder, path);

                Color textColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.TextColor;
                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, textColor, this.ToolWindowTextFormat);
            }
            else
            {
                Color startColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.StartColor;
                Color endColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.EndColor;
                LinearGradientMode gradientMode = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.LinearGradientMode;
                g.FillPath(new LinearGradientBrush(rectTab, startColor, endColor, gradientMode), path);

                if (this.Tabs.IndexOf(this.DockPane.ActiveContent) != this.Tabs.IndexOf(tab) + 1)
                {
                    Point pt1 = new Point(rect.Right, rect.Top + ToolWindowTabSeperatorGapTop);
                    Point pt2 = new Point(rect.Right, rect.Bottom - ToolWindowTabSeperatorGapBottom);
                    g.DrawLine(PenToolWindowTabBorder, DrawHelper.RtlTransform(this, pt1), DrawHelper.RtlTransform(this, pt2));
                }

                Color textColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.TextColor;
                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, textColor, this.ToolWindowTextFormat);
            }

            if (rectTab.Contains(rectIcon))
                g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
        }

        private void DrawTab_Document(Graphics g, TabVS2005 tab, Rectangle rect)
        {
            if (tab.TabWidth == 0)
                return;

            Rectangle rectIcon = new Rectangle(
                rect.X + DocumentIconGapLeft,
                rect.Y + rect.Height - 1 - DocumentIconGapBottom - DocumentIconHeight,
                DocumentIconWidth, DocumentIconHeight);
            Rectangle rectText = rectIcon;
            if (this.DockPane.DockPanel.ShowDocumentIcon)
            {
                rectText.X += rectIcon.Width + DocumentIconGapRight;
                rectText.Y = rect.Y;
                rectText.Width = rect.Width - rectIcon.Width - DocumentIconGapLeft -
                    DocumentIconGapRight - DocumentTextGapRight;
                rectText.Height = rect.Height;
            }
            else
                rectText.Width = rect.Width - DocumentIconGapLeft - DocumentTextGapRight;

            Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
            Rectangle rectBack = DrawHelper.RtlTransform(this, rect);
            rectBack.Width += rect.X;
            rectBack.X = 0;

            rectText = DrawHelper.RtlTransform(this, rectText);
            rectIcon = DrawHelper.RtlTransform(this, rectIcon);
            GraphicsPath path = this.GetTabOutline(tab, true, false);
            if (this.DockPane.ActiveContent == tab.Content)
            {
                Color startColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.StartColor;
                Color endColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.EndColor;
                LinearGradientMode gradientMode = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.LinearGradientMode;
                g.FillPath(new LinearGradientBrush(rectBack, startColor, endColor, gradientMode), path);
                g.DrawPath(PenDocumentTabActiveBorder, path);

                Color textColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.TextColor;
                if (this.DockPane.IsActiveDocumentPane)
                    TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.BoldFont, rectText, textColor, this.DocumentTextFormat);
                else
                    TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, textColor, this.DocumentTextFormat);
            }
            else
            {
                Color startColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.StartColor;
                Color endColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.EndColor;
                LinearGradientMode gradientMode = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.LinearGradientMode;
                g.FillPath(new LinearGradientBrush(rectBack, startColor, endColor, gradientMode), path);
                g.DrawPath(PenDocumentTabInactiveBorder, path);

                Color textColor = this.DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.TextColor;
                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, this.TextFont, rectText, textColor, this.DocumentTextFormat);
            }

            if (rectTab.Contains(rectIcon) && this.DockPane.DockPanel.ShowDocumentIcon)
                g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
        }

        private void WindowList_Click(object sender, EventArgs e)
        {
            this.SelectMenu.Items.Clear();
            foreach (TabVS2005 tab in this.Tabs)
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
                this.ButtonClose.Enabled = this.DockPane.ActiveContent == null ? true : this.DockPane.ActiveContent.DockHandler.CloseButton;
                this.m_closeButtonVisible = this.DockPane.ActiveContent == null ? true : this.DockPane.ActiveContent.DockHandler.CloseButtonVisible;
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

        protected override void OnMouseHover(EventArgs e)
        {
            int index = this.HitTest(this.PointToClient(Control.MousePosition));
            string toolTip = string.Empty;

            base.OnMouseHover(e);

            if (index != -1)
            {
                TabVS2005 tab = this.Tabs[index] as TabVS2005;
                if (!String.IsNullOrEmpty(tab.Content.DockHandler.ToolTipText))
                    toolTip = tab.Content.DockHandler.ToolTipText;
                else if (tab.MaxWidth > tab.TabWidth)
                    toolTip = tab.Content.DockHandler.TabText;
            }

            if (this.m_toolTip.GetToolTip(this) != toolTip)
            {
                this.m_toolTip.Active = false;
                this.m_toolTip.SetToolTip(this, toolTip);
                this.m_toolTip.Active = true;
            }

            // requires further tracking of mouse hover behavior,
            this.ResetMouseEventArgs();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            this.PerformLayout();
        }
    }
}