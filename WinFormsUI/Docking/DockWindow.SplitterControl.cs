using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public partial class DockWindow
    {
        internal class DefaultSplitterControl : SplitterBase
        {
            protected override int SplitterSize
            {
                get { return Measures.SplitterSize; }
            }

            protected override void StartDrag()
            {
                DockWindow window = this.Parent as DockWindow;
                if (window == null)
                    return;
				if ( !window.DockPanel.AllowChangeLayout )
					return;

                window.DockPanel.BeginDrag(window, window.RectangleToScreen(this.Bounds));
            }
        }
    }
}
