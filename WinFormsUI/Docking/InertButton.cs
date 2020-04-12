using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    public class InertButton : Button
    {
        private enum RepeatClickStatus
        {
            Disabled,
            Started,
            Repeating,
            Stopped
        }

        private class RepeatClickEventArgs : EventArgs
        {
            private static RepeatClickEventArgs _empty;

            static RepeatClickEventArgs()
            {
                _empty = new RepeatClickEventArgs();
            }

            public new static RepeatClickEventArgs Empty
            {
                get    {    return _empty;    }
            }
        }

        private IContainer components = new Container();
        private int m_borderWidth = 1;
        private bool m_mouseOver = false;
        private bool m_mouseCapture = false;
        private bool m_isPopup = false;
        private Image m_imageEnabled = null;
        private Image m_imageDisabled = null;
        private int m_imageIndexEnabled = -1;
        private int m_imageIndexDisabled = -1;
        private bool m_monochrom = true;
        private ToolTip m_toolTip = null;
        private string m_toolTipText = "";
        private Color m_borderColor = Color.Empty;

        public InertButton()
        {
            this.InternalConstruct(null, null);
        }

        public InertButton(Image imageEnabled)
        {
            this.InternalConstruct(imageEnabled, null);
        }

        public InertButton(Image imageEnabled, Image imageDisabled)
        {
            this.InternalConstruct(imageEnabled, imageDisabled);
        }
        
        private void InternalConstruct(Image imageEnabled, Image imageDisabled)
        {
            // Remember parameters
            this.ImageEnabled = imageEnabled;
            this.ImageDisabled = imageDisabled;

            // Prevent drawing flicker by blitting from memory in WM_PAINT
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // Prevent base class from trying to generate double click events and
            // so testing clicks against the double click time and rectangle. Getting
            // rid of this allows the user to press then release button very quickly.
            //SetStyle(ControlStyles.StandardDoubleClick, false);

            // Should not be allowed to select this control
            this.SetStyle(ControlStyles.Selectable, false);

            this.m_timer = new Timer();
            this.m_timer.Enabled = false;
            this.m_timer.Tick += new EventHandler(this.Timer_Tick);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                    this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        public Color BorderColor
        {
            get    {    return this.m_borderColor;    }
            set
            {
                if (this.m_borderColor != value)
                {
                    this.m_borderColor = value;
                    this.Invalidate();
                }
            }
        }

        private bool ShouldSerializeBorderColor()
        {
            return (this.m_borderColor != Color.Empty);
        }

        public int BorderWidth
        {
            get { return this.m_borderWidth; }

            set
            {
                if (value < 1)
                    value = 1;
                if (this.m_borderWidth != value)
                {
                    this.m_borderWidth = value;
                    this.Invalidate();
                }
            }
        }

        public Image ImageEnabled
        {
            get
            { 
                if (this.m_imageEnabled != null)
                    return this.m_imageEnabled;

                try
                {
                    if (this.ImageList == null || this.ImageIndexEnabled == -1)
                        return null;
                    else
                        return this.ImageList.Images[this.m_imageIndexEnabled];
                }
                catch
                {
                    return null;
                }
            }

            set
            {
                if (this.m_imageEnabled != value)
                {
                    this.m_imageEnabled = value;
                    this.Invalidate();
                }
            }
        }

        private bool ShouldSerializeImageEnabled()
        {
            return (this.m_imageEnabled != null);
        }

        public Image ImageDisabled
        {
            get
            {
                if (this.m_imageDisabled != null)
                    return this.m_imageDisabled;

                try
                {
                    if (this.ImageList == null || this.ImageIndexDisabled == -1)
                        return null;
                    else
                        return this.ImageList.Images[this.m_imageIndexDisabled];
                }
                catch
                {
                    return null;
                }
            }

            set
            {
                if (this.m_imageDisabled != value)
                {
                    this.m_imageDisabled = value;
                    this.Invalidate();
                }
            }
        }

        public int ImageIndexEnabled
        {
            get    {    return this.m_imageIndexEnabled;    }
            set
            {
                if (this.m_imageIndexEnabled != value)
                {
                    this.m_imageIndexEnabled = value;
                    this.Invalidate();
                }
            }
        }

        public int ImageIndexDisabled
        {
            get    {    return this.m_imageIndexDisabled;    }
            set
            {
                if (this.m_imageIndexDisabled != value)
                {
                    this.m_imageIndexDisabled = value;
                    this.Invalidate();
                }
            }
        }

        public bool IsPopup
        {
            get { return this.m_isPopup; }

            set
            {
                if (this.m_isPopup != value)
                {
                    this.m_isPopup = value;
                    this.Invalidate();
                }
            }
        }

        public bool Monochrome
        {
            get    {    return this.m_monochrom;    }
            set
            {
                if (value != this.m_monochrom)
                {
                    this.m_monochrom = value;
                    this.Invalidate();
                }
            }
        }

        public bool RepeatClick
        {
            get    {    return (this.ClickStatus != RepeatClickStatus.Disabled);    }
            set    { this.ClickStatus = RepeatClickStatus.Stopped;    }
        }

        private RepeatClickStatus m_clickStatus = RepeatClickStatus.Disabled;
        private RepeatClickStatus ClickStatus
        {
            get    {    return this.m_clickStatus;    }
            set
            {
                if (this.m_clickStatus == value)
                    return;

                this.m_clickStatus = value;
                if (this.ClickStatus == RepeatClickStatus.Started)
                {
                    this.Timer.Interval = this.RepeatClickDelay;
                    this.Timer.Enabled = true;
                }
                else if (this.ClickStatus == RepeatClickStatus.Repeating)
                    this.Timer.Interval = this.RepeatClickInterval;
                else
                    this.Timer.Enabled = false;
            }
        }

        private int m_repeatClickDelay = 500;
        public int RepeatClickDelay
        {
            get    {    return this.m_repeatClickDelay;    } 
            set    { this.m_repeatClickDelay = value;    }
        }

        private int m_repeatClickInterval = 100;
        public int RepeatClickInterval
        {
            get    {    return this.m_repeatClickInterval;    }
            set    { this.m_repeatClickInterval = value;    }
        }

        private Timer m_timer;
        private Timer Timer
        {
            get    {    return this.m_timer;    }
        }

        public string ToolTipText
        {
            get    {    return this.m_toolTipText;    }
            set
            {
                if (this.m_toolTipText != value)
                {
                    if (this.m_toolTip == null)
                        this.m_toolTip = new ToolTip(this.components);
                    this.m_toolTipText = value;
                    this.m_toolTip.SetToolTip(this, value);
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (this.m_mouseCapture && this.m_mouseOver)
                this.OnClick(RepeatClickEventArgs.Empty);
            if (this.ClickStatus == RepeatClickStatus.Started)
                this.ClickStatus = RepeatClickStatus.Repeating;
        }

        /// <exclude/>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            if (this.m_mouseCapture == false || this.m_mouseOver == false)
            {
                this.m_mouseCapture = true;
                this.m_mouseOver = true;

                //Redraw to show button state
                this.Invalidate();
            }

            if (this.RepeatClick)
            {
                this.OnClick(RepeatClickEventArgs.Empty);
                this.ClickStatus = RepeatClickStatus.Started;
            }
        }

        /// <exclude/>
        protected override void OnClick(EventArgs e)
        {
            if (this.RepeatClick && !(e is RepeatClickEventArgs))
                return;

            base.OnClick (e);
        }

        /// <exclude/>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left)
                return;

            if (this.m_mouseOver == true || this.m_mouseCapture == true)
            {
                this.m_mouseOver = false;
                this.m_mouseCapture = false;

                // Redraw to show button state
                this.Invalidate();
            }

            if (this.RepeatClick)
                this.ClickStatus = RepeatClickStatus.Stopped;
        }

        /// <exclude/>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Is mouse point inside our client rectangle
            bool over = this.ClientRectangle.Contains(new Point(e.X, e.Y));

            // If entering the button area or leaving the button area...
            if (over != this.m_mouseOver)
            {
                // Update state
                this.m_mouseOver = over;

                // Redraw to show button state
                this.Invalidate();
            }
        }

        /// <exclude/>
        protected override void OnMouseEnter(EventArgs e)
        {
            // Update state to reflect mouse over the button area
            if (!this.m_mouseOver)
            {
                this.m_mouseOver = true;

                // Redraw to show button state
                this.Invalidate();
            }

            base.OnMouseEnter(e);
        }

        /// <exclude/>
        protected override void OnMouseLeave(EventArgs e)
        {
            // Update state to reflect mouse not over the button area
            if (this.m_mouseOver)
            {
                this.m_mouseOver = false;

                // Redraw to show button state
                this.Invalidate();
            }

            base.OnMouseLeave(e);
        }

        /// <exclude/>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            this.DrawBackground(e.Graphics);
            this.DrawImage(e.Graphics);
            this.DrawText(e.Graphics);
            this.DrawBorder(e.Graphics);
        }

        private void DrawBackground(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void DrawImage(Graphics g)
        {
            Image image = this.Enabled ? this.ImageEnabled : ((this.ImageDisabled != null) ? this.ImageDisabled : this.ImageEnabled);
            ImageAttributes imageAttr = null;

            if (null == image)
                return;

            if (this.m_monochrom)
            {
                imageAttr = new ImageAttributes();

                // transform the monochrom image
                // white -> BackColor
                // black -> ForeColor
                ColorMap[] colorMap = new ColorMap[2];
                colorMap[0] = new ColorMap();
                colorMap[0].OldColor = Color.White;
                colorMap[0].NewColor = this.BackColor;
                colorMap[1] = new ColorMap();
                colorMap[1].OldColor = Color.Black;
                colorMap[1].NewColor = this.ForeColor;
                imageAttr.SetRemapTable(colorMap);
            }

            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            if ((!this.Enabled) && (null == this.ImageDisabled))
            {
                using (Bitmap bitmapMono = new Bitmap(image, this.ClientRectangle.Size))
                {
                    if (imageAttr != null)
                    {
                        using (Graphics gMono = Graphics.FromImage(bitmapMono))
                        {
                            gMono.DrawImage(image, new Point[3] { new Point(0, 0), new Point(image.Width - 1, 0), new Point(0, image.Height - 1) }, rect, GraphicsUnit.Pixel, imageAttr);
                        }
                    }
                    ControlPaint.DrawImageDisabled(g, bitmapMono, 0, 0, this.BackColor);
                }
            }
            else
            {
                // Three points provided are upper-left, upper-right and 
                // lower-left of the destination parallelogram. 
                Point[] pts = new Point[3];
                pts[0].X = (this.Enabled && this.m_mouseOver && this.m_mouseCapture) ? 1 : 0;
                pts[0].Y = (this.Enabled && this.m_mouseOver && this.m_mouseCapture) ? 1 : 0;
                pts[1].X = pts[0].X + this.ClientRectangle.Width;
                pts[1].Y = pts[0].Y;
                pts[2].X = pts[0].X;
                pts[2].Y = pts[1].Y + this.ClientRectangle.Height;

                if (imageAttr == null)
                    g.DrawImage(image, pts, rect, GraphicsUnit.Pixel);
                else
                    g.DrawImage(image, pts, rect, GraphicsUnit.Pixel, imageAttr);
            }
        }    

        private void DrawText(Graphics g)
        {
            if (this.Text == string.Empty)
                return;

            Rectangle rect = this.ClientRectangle;

            rect.X += this.BorderWidth;
            rect.Y += this.BorderWidth;
            rect.Width -= 2 * this.BorderWidth;
            rect.Height -= 2 * this.BorderWidth;

            StringFormat stringFormat = new StringFormat();

            if (this.TextAlign == ContentAlignment.TopLeft)
            {
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.LineAlignment = StringAlignment.Near;
            }
            else if (this.TextAlign == ContentAlignment.TopCenter)
            {
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Near;
            }
            else if (this.TextAlign == ContentAlignment.TopRight)
            {
                stringFormat.Alignment = StringAlignment.Far;
                stringFormat.LineAlignment = StringAlignment.Near;
            }
            else if (this.TextAlign == ContentAlignment.MiddleLeft)
            {
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.LineAlignment = StringAlignment.Center;
            }
            else if (this.TextAlign == ContentAlignment.MiddleCenter)
            {
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
            }
            else if (this.TextAlign == ContentAlignment.MiddleRight)
            {
                stringFormat.Alignment = StringAlignment.Far;
                stringFormat.LineAlignment = StringAlignment.Center;
            }
            else if (this.TextAlign == ContentAlignment.BottomLeft)
            {
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.LineAlignment = StringAlignment.Far;
            }
            else if (this.TextAlign == ContentAlignment.BottomCenter)
            {
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Far;
            }
            else if (this.TextAlign == ContentAlignment.BottomRight)
            {
                stringFormat.Alignment = StringAlignment.Far;
                stringFormat.LineAlignment = StringAlignment.Far;
            }

            using (Brush brush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, this.Font, brush, rect, stringFormat);
            }
        }

        private void DrawBorder(Graphics g)
        {
            ButtonBorderStyle bs;

            // Decide on the type of border to draw around image
            if (!this.Enabled)
                bs = this.IsPopup ? ButtonBorderStyle.Outset : ButtonBorderStyle.Solid;
            else if (this.m_mouseOver && this.m_mouseCapture)
                bs = ButtonBorderStyle.Inset;
            else if (this.IsPopup || this.m_mouseOver)
                bs = ButtonBorderStyle.Outset;
            else
                bs = ButtonBorderStyle.Solid;

            Color colorLeftTop;
            Color colorRightBottom;
            if (bs == ButtonBorderStyle.Solid)
            {
                colorLeftTop = this.BackColor;
                colorRightBottom = this.BackColor;
            }
            else if (bs == ButtonBorderStyle.Outset)
            {
                colorLeftTop = this.m_borderColor.IsEmpty ? this.BackColor : this.m_borderColor;
                colorRightBottom = this.BackColor;
            }
            else
            {
                colorLeftTop = this.BackColor;
                colorRightBottom = this.m_borderColor.IsEmpty ? this.BackColor : this.m_borderColor;
            }
            ControlPaint.DrawBorder(g, this.ClientRectangle,
                colorLeftTop, this.m_borderWidth, bs,
                colorLeftTop, this.m_borderWidth, bs,
                colorRightBottom, this.m_borderWidth, bs,
                colorRightBottom, this.m_borderWidth, bs);
        }

        /// <exclude/>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (this.Enabled == false)
            {
                this.m_mouseOver = false;
                this.m_mouseCapture = false;
                if (this.RepeatClick && this.ClickStatus != RepeatClickStatus.Stopped)
                    this.ClickStatus = RepeatClickStatus.Stopped;
            }
            this.Invalidate();
        }
    }
}