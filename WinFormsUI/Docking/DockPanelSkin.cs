using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    #region DockPanelSkin classes
    /// <summary>
    /// The skin to use when displaying the DockPanel.
    /// The skin allows custom gradient color schemes to be used when drawing the
    /// DockStrips and Tabs.
    /// </summary>
    [TypeConverter(typeof(DockPanelSkinConverter))]
    public class DockPanelSkin
    {
        private AutoHideStripSkin m_autoHideStripSkin = new AutoHideStripSkin();
        private DockPaneStripSkin m_dockPaneStripSkin = new DockPaneStripSkin();

        /// <summary>
        /// The skin used to display the auto hide strips and tabs.
        /// </summary>
        public AutoHideStripSkin AutoHideStripSkin
        {
            get {
                //m_autoHideStripSkin.TabGradient.StartColor = Color.Black;
                //m_autoHideStripSkin.TabGradient.EndColor = Color.Black;
                return this.m_autoHideStripSkin ;
            }
            set { this.m_autoHideStripSkin = value; }
        }

        /// <summary>
        /// The skin used to display the Document and ToolWindow style DockStrips and Tabs.
        /// </summary>
        public DockPaneStripSkin DockPaneStripSkin
        {
            get { return this.m_dockPaneStripSkin; }
            set { this.m_dockPaneStripSkin = value; }
        }
    }

    /// <summary>
    /// The skin used to display the auto hide strip and tabs.
    /// </summary>
    [TypeConverter(typeof(AutoHideStripConverter))]
    public class AutoHideStripSkin
    {
        private DockPanelGradient m_dockStripGradient = new DockPanelGradient();
        private TabGradient m_TabGradient = new TabGradient();
        private DockStripBackground m_DockStripBackground = new DockStripBackground();
        
        private Font m_textFont = SystemFonts.MenuFont;

        /// <summary>
        /// The gradient color skin for the DockStrips.
        /// </summary>
        public DockPanelGradient DockStripGradient
        {
            get { return this.m_dockStripGradient; }
            set { this.m_dockStripGradient = value; }
        }

        /// <summary>
        /// The gradient color skin for the Tabs.
        /// </summary>
        public TabGradient TabGradient
        {
            get { return this.m_TabGradient; }
            set { this.m_TabGradient = value; }
        }

        /// <summary>
        /// The gradient color skin for the Tabs.
        /// </summary>
        public DockStripBackground DockStripBackground
        {
            get { return this.m_DockStripBackground; }
            set { this.m_DockStripBackground = value; }
        }

        /// <summary>
        /// Font used in AutoHideStrip elements.
        /// </summary>
        [DefaultValue(typeof(SystemFonts), "MenuFont")]
        public Font TextFont
        {
            get { return this.m_textFont; }
            set { this.m_textFont = value; }
        }

    }

    /// <summary>
    /// The skin used to display the document and tool strips and tabs.
    /// </summary>
    [TypeConverter(typeof(DockPaneStripConverter))]
    public class DockPaneStripSkin
    {
        private DockPaneStripGradient m_DocumentGradient = new DockPaneStripGradient();
        private DockPaneStripToolWindowGradient m_ToolWindowGradient = new DockPaneStripToolWindowGradient();
        private Font m_textFont = SystemFonts.MenuFont;

        /// <summary>
        /// The skin used to display the Document style DockPane strip and tab.
        /// </summary>
        public DockPaneStripGradient DocumentGradient
        {
            get { return this.m_DocumentGradient; }
            set { this.m_DocumentGradient = value; }
        }

        /// <summary>
        /// The skin used to display the ToolWindow style DockPane strip and tab.
        /// </summary>
        public DockPaneStripToolWindowGradient ToolWindowGradient
        {
            get { return this.m_ToolWindowGradient; }
            set { this.m_ToolWindowGradient = value; }
        }

        /// <summary>
        /// Font used in DockPaneStrip elements.
        /// </summary>
        [DefaultValue(typeof(SystemFonts), "MenuFont")]
        public Font TextFont
        {
            get { return this.m_textFont; }
            set { this.m_textFont = value; }
        }
    }

    /// <summary>
    /// The skin used to display the DockPane ToolWindow strip and tab.
    /// </summary>
    [TypeConverter(typeof(DockPaneStripGradientConverter))]
    public class DockPaneStripToolWindowGradient : DockPaneStripGradient
    {
        private TabGradient m_activeCaptionGradient = new TabGradient();
        private TabGradient m_inactiveCaptionGradient = new TabGradient();

        /// <summary>
        /// The skin used to display the active ToolWindow caption.
        /// </summary>
        public TabGradient ActiveCaptionGradient
        {
            get { return this.m_activeCaptionGradient; }
            set { this.m_activeCaptionGradient = value; }
        }

        /// <summary>
        /// The skin used to display the inactive ToolWindow caption.
        /// </summary>
        public TabGradient InactiveCaptionGradient
        {
            get { return this.m_inactiveCaptionGradient; }
            set { this.m_inactiveCaptionGradient = value; }
        }
    }

    /// <summary>
    /// The skin used to display the DockPane strip and tab.
    /// </summary>
    [TypeConverter(typeof(DockPaneStripGradientConverter))]
    public class DockPaneStripGradient
    {
        private DockPanelGradient m_dockStripGradient = new DockPanelGradient();
        private TabGradient m_activeTabGradient = new TabGradient();
        private TabGradient m_inactiveTabGradient = new TabGradient();
        private TabGradient m_hoverTabGradient = new TabGradient();
        

        /// <summary>
        /// The gradient color skin for the DockStrip.
        /// </summary>
        public DockPanelGradient DockStripGradient
        {
            get { return this.m_dockStripGradient; }
            set { this.m_dockStripGradient = value; }
        }

        /// <summary>
        /// The skin used to display the active DockPane tabs.
        /// </summary>
        public TabGradient ActiveTabGradient
        {
            get { return this.m_activeTabGradient; }
            set { this.m_activeTabGradient = value; }
        }

        public TabGradient HoverTabGradient
        {
            get { return this.m_hoverTabGradient; }
            set { this.m_hoverTabGradient = value; }
        }

        /// <summary>
        /// The skin used to display the inactive DockPane tabs.
        /// </summary>
        public TabGradient InactiveTabGradient
        {
            get { return this.m_inactiveTabGradient; }
            set { this.m_inactiveTabGradient = value; }
        }
    }

    /// <summary>
    /// The skin used to display the dock pane tab
    /// </summary>
    [TypeConverter(typeof(DockPaneTabGradientConverter))]
    public class TabGradient : DockPanelGradient
    {
        private Color m_textColor = SystemColors.ControlText;

        /// <summary>
        /// The text color.
        /// </summary>
        [DefaultValue(typeof(SystemColors), "ControlText")]
        public Color TextColor
        {
            get { return this.m_textColor; }
            set { this.m_textColor = value; }
        }
    }

        /// <summary>
    /// The skin used to display the dock pane tab
    /// </summary>
    [TypeConverter(typeof(DockPaneTabGradientConverter))]
    public class DockStripBackground
    {
        private Color m_startColor = SystemColors.Control;
        private Color m_endColor = SystemColors.Control;
        //private LinearGradientMode m_linearGradientMode = LinearGradientMode.Horizontal;

        /// <summary>
        /// The beginning gradient color.
        /// </summary>
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color StartColor
        {
            get { return this.m_startColor; }
            set { this.m_startColor = value; }
        }

        /// <summary>
        /// The ending gradient color.
        /// </summary>
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color EndColor
        {
            get { return this.m_endColor; }
            set { this.m_endColor = value; }
        }
    }
    

    /// <summary>
    /// The gradient color skin.
    /// </summary>
    [TypeConverter(typeof(DockPanelGradientConverter))]
    public class DockPanelGradient
    {
        private Color m_startColor = SystemColors.Control;
        private Color m_endColor = SystemColors.Control;
        private LinearGradientMode m_linearGradientMode = LinearGradientMode.Horizontal;

        /// <summary>
        /// The beginning gradient color.
        /// </summary>
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color StartColor
        {
            get { return this.m_startColor; }
            set { this.m_startColor = value; }
        }

        /// <summary>
        /// The ending gradient color.
        /// </summary>
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color EndColor
        {
            get { return this.m_endColor; }
            set { this.m_endColor = value; }
        }

        /// <summary>
        /// The gradient mode to display the colors.
        /// </summary>
        [DefaultValue(LinearGradientMode.Horizontal)]
        public LinearGradientMode LinearGradientMode
        {
            get { return this.m_linearGradientMode; }
            set { this.m_linearGradientMode = value; }
        }
    }

    #endregion

    #region Converters
    public class DockPanelSkinConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DockPanelSkin))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String) && value is DockPanelSkin)
            {
                return "DockPanelSkin";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class DockPanelGradientConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DockPanelGradient))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String) && value is DockPanelGradient)
            {
                return "DockPanelGradient";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class AutoHideStripConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(AutoHideStripSkin))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String) && value is AutoHideStripSkin)
            {
                return "AutoHideStripSkin";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class DockPaneStripConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DockPaneStripSkin))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String) && value is DockPaneStripSkin)
            {
                return "DockPaneStripSkin";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class DockPaneStripGradientConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DockPaneStripGradient))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String) && value is DockPaneStripGradient)
            {
                return "DockPaneStripGradient";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class DockPaneTabGradientConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(TabGradient))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            TabGradient val = value as TabGradient;
            if (destinationType == typeof(String) && val != null)
            {
                return "DockPaneTabGradient";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
    #endregion
}
