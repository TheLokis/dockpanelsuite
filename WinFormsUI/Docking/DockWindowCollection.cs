using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    public class DockWindowCollection : ReadOnlyCollection<DockWindow>
    {
        internal DockWindowCollection(DockPanel dockPanel)
            : base(new List<DockWindow>())
        {
            this.Items.Add(dockPanel.DockWindowFactory.CreateDockWindow(dockPanel, DockState.Document));
            this.Items.Add(dockPanel.DockWindowFactory.CreateDockWindow(dockPanel, DockState.DockLeft));
            this.Items.Add(dockPanel.DockWindowFactory.CreateDockWindow(dockPanel, DockState.DockRight));
            this.Items.Add(dockPanel.DockWindowFactory.CreateDockWindow(dockPanel, DockState.DockTop));
            this.Items.Add(dockPanel.DockWindowFactory.CreateDockWindow(dockPanel, DockState.DockBottom));
        }

        public DockWindow this [DockState dockState]
        {
            get
            {
                if (dockState == DockState.Document)
                    return this.Items[0];
                else if (dockState == DockState.DockLeft || dockState == DockState.DockLeftAutoHide)
                    return this.Items[1];
                else if (dockState == DockState.DockRight || dockState == DockState.DockRightAutoHide)
                    return this.Items[2];
                else if (dockState == DockState.DockTop || dockState == DockState.DockTopAutoHide)
                    return this.Items[3];
                else if (dockState == DockState.DockBottom || dockState == DockState.DockBottomAutoHide)
                    return this.Items[4];

                throw (new ArgumentOutOfRangeException());
            }
        }
    }
}
