using System;

namespace WeifenLuo.WinFormsUI.Docking
{
    using System.ComponentModel;

    public partial class DockPanel
    {
        private DockPanelSkin m_dockPanelSkin = VS2005Theme.CreateVisualStudio2005();
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockPanelSkin")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Please use Theme instead.")]
        [Browsable(false)]
        public DockPanelSkin Skin
        {
            get { return this.m_dockPanelSkin;  }
            set { this.m_dockPanelSkin = value; }
        }
        
        private ThemeBase m_dockPanelTheme = new VS2005Theme();
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockPanelTheme")]
        public ThemeBase Theme
        {
            get { return this.m_dockPanelTheme; }
            set
            {
                if (value == null)
                {
                    return;
                }

                if (this.m_dockPanelTheme.GetType() == value.GetType())
                {
                    return;
                }

                this.m_dockPanelTheme = value;
                this.m_dockPanelTheme.Apply(this);
            }
        }
    }
}
