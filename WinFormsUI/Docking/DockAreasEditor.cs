using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class DockAreasEditor : UITypeEditor
    {
        private class DockAreasEditorControl : System.Windows.Forms.UserControl
        {
            private CheckBox checkBoxFloat;
            private CheckBox checkBoxDockLeft;
            private CheckBox checkBoxDockRight;
            private CheckBox checkBoxDockTop;
            private CheckBox checkBoxDockBottom;
            private CheckBox checkBoxDockFill;
            private DockAreas m_oldDockAreas;

            public DockAreas DockAreas
            {
                get
                {
                    DockAreas dockAreas = 0;
                    if (this.checkBoxFloat.Checked)
                        dockAreas |= DockAreas.Float;
                    if (this.checkBoxDockLeft.Checked)
                        dockAreas |= DockAreas.DockLeft;
                    if (this.checkBoxDockRight.Checked)
                        dockAreas |= DockAreas.DockRight;
                    if (this.checkBoxDockTop.Checked)
                        dockAreas |= DockAreas.DockTop;
                    if (this.checkBoxDockBottom.Checked)
                        dockAreas |= DockAreas.DockBottom;
                    if (this.checkBoxDockFill.Checked)
                        dockAreas |= DockAreas.Document;

                    if (dockAreas == 0)
                        return this.m_oldDockAreas;
                    else
                        return dockAreas;
                }
            }

            public DockAreasEditorControl()
            {
                this.checkBoxFloat = new CheckBox();
                this.checkBoxDockLeft = new CheckBox();
                this.checkBoxDockRight = new CheckBox();
                this.checkBoxDockTop = new CheckBox();
                this.checkBoxDockBottom = new CheckBox();
                this.checkBoxDockFill = new CheckBox();

                this.SuspendLayout();

                this.checkBoxFloat.Appearance = Appearance.Button;
                this.checkBoxFloat.Dock = DockStyle.Top;
                this.checkBoxFloat.Height = 24;
                this.checkBoxFloat.Text = Strings.DockAreaEditor_FloatCheckBoxText;
                this.checkBoxFloat.TextAlign = ContentAlignment.MiddleCenter;
                this.checkBoxFloat.FlatStyle = FlatStyle.System;

                this.checkBoxDockLeft.Appearance = System.Windows.Forms.Appearance.Button;
                this.checkBoxDockLeft.Dock = System.Windows.Forms.DockStyle.Left;
                this.checkBoxDockLeft.Width = 24;
                this.checkBoxDockLeft.FlatStyle = FlatStyle.System;

                this.checkBoxDockRight.Appearance = System.Windows.Forms.Appearance.Button;
                this.checkBoxDockRight.Dock = System.Windows.Forms.DockStyle.Right;
                this.checkBoxDockRight.Width = 24;
                this.checkBoxDockRight.FlatStyle = FlatStyle.System;

                this.checkBoxDockTop.Appearance = System.Windows.Forms.Appearance.Button;
                this.checkBoxDockTop.Dock = System.Windows.Forms.DockStyle.Top;
                this.checkBoxDockTop.Height = 24;
                this.checkBoxDockTop.FlatStyle = FlatStyle.System;

                this.checkBoxDockBottom.Appearance = System.Windows.Forms.Appearance.Button;
                this.checkBoxDockBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
                this.checkBoxDockBottom.Height = 24;
                this.checkBoxDockBottom.FlatStyle = FlatStyle.System;

                this.checkBoxDockFill.Appearance = System.Windows.Forms.Appearance.Button;
                this.checkBoxDockFill.Dock = System.Windows.Forms.DockStyle.Fill;
                this.checkBoxDockFill.FlatStyle = FlatStyle.System;

                this.Controls.AddRange(new Control[] {
                                                         this.checkBoxDockFill,
                                                         this.checkBoxDockBottom,
                                                         this.checkBoxDockTop,
                                                         this.checkBoxDockRight,
                                                         this.checkBoxDockLeft,
                                                         this.checkBoxFloat});

                this.Size = new System.Drawing.Size(160, 144);
                this.BackColor = SystemColors.Control;
                this.ResumeLayout();
            }

            public void SetStates(DockAreas dockAreas)
            {
                this.m_oldDockAreas = dockAreas;
                if ((dockAreas & DockAreas.DockLeft) != 0)
                    this.checkBoxDockLeft.Checked = true;
                if ((dockAreas & DockAreas.DockRight) != 0)
                    this.checkBoxDockRight.Checked = true;
                if ((dockAreas & DockAreas.DockTop) != 0)
                    this.checkBoxDockTop.Checked = true;
                if ((dockAreas & DockAreas.DockTop) != 0)
                    this.checkBoxDockTop.Checked = true;
                if ((dockAreas & DockAreas.DockBottom) != 0)
                    this.checkBoxDockBottom.Checked = true;
                if ((dockAreas & DockAreas.Document) != 0)
                    this.checkBoxDockFill.Checked = true;
                if ((dockAreas & DockAreas.Float) != 0)
                    this.checkBoxFloat.Checked = true;
            }
        }

        private DockAreasEditor.DockAreasEditorControl m_ui = null;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider sp, object value)
        {
            if (this.m_ui == null)
                this.m_ui = new DockAreasEditor.DockAreasEditorControl();

            this.m_ui.SetStates((DockAreas)value);

            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)sp.GetService(typeof(IWindowsFormsEditorService));
            edSvc.DropDownControl(this.m_ui);

            return this.m_ui.DockAreas;
        }
    }
}
