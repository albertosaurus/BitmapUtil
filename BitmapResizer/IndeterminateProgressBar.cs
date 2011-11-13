using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace BitmapResizer {
    /// <summary>
    ///
    /// DISCLAIMER:
    ///
    /// This sample code is not supported under any Microsoft standard support program or service.    
    /// The sample code is provided AS IS without warranty of any kind. Microsoft further disclaims   
    /// all implied warranties including, without limitation, any implied warranties of merchantability  
    /// or of fitness for a particular purpose. The entire risk arising out of the use or performance   
    /// of the sample code and documentation remains with you. In no event shall Microsoft, its      
    /// authors, or anyone else involved in the creation, production, or delivery of the code be     
    /// liable for any damages whatsoever (including, without limitation, damages for loss of business  
    /// profits, business interruption, loss of business information, or other pecuniary loss) arising  
    /// out of the use of or inability to use the sample code or documentation, even if Microsoft    
    /// has been advised of the possibility of such damages.
    ///
    /// </summary>
    /// 
    public class IndeterminateProgressBar : System.Windows.Forms.Panel {
        private System.Windows.Forms.Timer tmrPulse;
        private System.ComponentModel.Container components = null;

        protected int numPips = 5;
        protected int pnlWidth = 100;
        protected int pnlHeight = 16;
        protected int pulseIndex = 0;
        protected float alphaFactor = 0.2f;
        protected bool FadeIn = true;
        protected bool start = false;
        protected LinearGradientMode gMode = LinearGradientMode.ForwardDiagonal;
        protected Color dkBorder = Color.Black;
        protected Color ltBorder = Color.White;
        protected Color from = Color.LimeGreen;
        protected Color to = Color.FromArgb(5, 149, 20);
        protected int speed = 25;
        protected Bitmap imgBase;
        protected Bitmap curBase;
        protected ImageAttributes imageAtt = new ImageAttributes();
        protected ColorMatrix clrMtrx = new ColorMatrix(new float[][] {new float[]    {1.0f,		0,		0,		0,    0},
																					new float[]		{	0,	 1.0f,		0,		0,    0},
																					new float[]		{	0,		0,	 1.0f,		0,    0},
																					new float[]		{	0,		0,		0,	 0.5f,    0}, 
																					new float[]		{	0,		0,		0,		0, 1.0f}});
        public IndeterminateProgressBar(System.ComponentModel.IContainer container) {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();
            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            this.Width = pnlWidth;
            this.Height = pnlHeight;
        }

        #region Properties

        [DefaultValue(typeof(bool), "False"),
        Description("If return is true the bar is currently pulsing, else the bar is in idle state."),
        Category("Pip State")]
        public bool IsPulsing {
            get {
                return start;
            }
        }

        [DefaultValue(typeof(Color), "Color.Black"),
        Description("The color of the dark boarder for each pip."),
        Category("Pip Options")]
        public Color BorderDark {
            get {
                return dkBorder;
            }
            set {
                dkBorder = value;
                CreateBaseImage();
                Invalidate();
            }
        }

        [DefaultValue(typeof(Color), "Color.White"),
        Description("The color of the inner light border for each pip."),
        Category("Pip Options")]
        public Color BorderLight {
            get {
                return ltBorder;
            }
            set {
                ltBorder = value;
                CreateBaseImage();
                Invalidate();
            }
        }

        [DefaultValue(typeof(Color), "Color,LimeGreen"),
        Description("The start color for each pip's inner body."),
        Category("Pip Options")]
        public Color ColorFrom {
            get {
                return from;
            }
            set {
                from = value;
                CreateBaseImage();
                Invalidate();
            }
        }

        [DefaultValue(typeof(Color), "Color.FromArgb(5,149,20)"),
        Description("The end color for each pip's inner body."),
        Category("Pip Options")]
        public Color ColorTo {
            get {
                return to;
            }
            set {
                to = value;
                CreateBaseImage();
                Invalidate();
            }
        }

        [DefaultValue(typeof(LinearGradientMode), "ForwardDiagonal"),
        Description("Direction of the gradient within the pip's body."),
        Category("Pip Options")]

        public LinearGradientMode GradientDirection {
            get {
                return gMode;
            }
            set {
                gMode = value;
                CreateBaseImage();
                Invalidate();
            }
        }

        [DefaultValue(typeof(int), "25"),
        Description("Speed of the alpha transition between each of the pip's."),
        Category("Pip Options")]

        public int Speed {
            get {
                return speed;
            }
            set {
                if (value < 5)
                    speed = 5;
                else
                    speed = value;

                tmrPulse.Interval = speed;
                Invalidate();
            }
        }

        [DefaultValue(typeof(int), "5"),
        Description("The number of pips to display in the control."),
        Category("Pip Options")]

        public int Pips {
            get {
                return numPips;
            }
            set {
                numPips = value;
                if (numPips < 1) numPips = 1;
                this.Width = numPips * 20;
                CreateBaseImage();
                Invalidate();
            }
        }

        #endregion

        public IndeterminateProgressBar() {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///

            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public void Start() {
            if (tmrPulse.Enabled) return;

            //Start the pulse
            tmrPulse.Enabled = true;
            start = tmrPulse.Enabled;
            Invalidate();
        }

        public void Stop() {
            if (!tmrPulse.Enabled) return;

            //Reset state
            tmrPulse.Enabled = false;
            start = tmrPulse.Enabled;
            pulseIndex = 0;
            FadeIn = true;
            curBase = imgBase.Clone() as Bitmap;
            Invalidate();
        }

        protected void CreateBaseImage() {
            int pipX = 0;
            Bitmap tmpBase = new Bitmap(numPips * 20, 17, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Bitmap pipBlank = GeneratePip();
            Graphics g = Graphics.FromImage(tmpBase);

            try {
                for (int i = 0; i < numPips; i++) {
                    clrMtrx[3, 3] = .2f;
                    imageAtt.SetColorMatrix(clrMtrx, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(pipBlank, new Rectangle(pipX, 0, 17, 17), 0, 0, 17, 17, GraphicsUnit.Pixel, imageAtt);
                    pipX = pipX + 20;
                }

                curBase = tmpBase.Clone() as Bitmap;
                imgBase = tmpBase.Clone() as Bitmap;
            } finally {
                g.Dispose();
                pipBlank.Dispose();
                tmpBase.Dispose();
            }

        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
            if (imgBase == null) CreateBaseImage();

            if (start)
                e.Graphics.DrawImage(curBase, 0, 0);
            else
                e.Graphics.DrawImage(imgBase, 0, 0);
        }

        public void PaintPulseIncrement() {
            int pipX = pulseIndex * 20;
            float tmpAlpha = .1f;
            Bitmap tmpBase = imgBase.Clone() as Bitmap;
            Bitmap pipBase = GeneratePip();
            Graphics g = Graphics.FromImage(tmpBase);

            try {
                if (FadeIn) {
                    //Fade In...
                    alphaFactor = alphaFactor + 0.1f;
                    if (alphaFactor > 1.0f) {
                        alphaFactor = 1.0f;
                        tmpAlpha = .1f;
                        FadeIn = false;
                    }
                } else {
                    //fade out...
                    alphaFactor = alphaFactor - 0.1f;
                    if (alphaFactor < 0.2f) {
                        alphaFactor = 0.2f;
                        FadeIn = true;
                        if (++pulseIndex > numPips) pulseIndex = 0;
                    }

                    //start fading in the next pip half way through 
                    //the current pips fade out
                    if (alphaFactor <= 0.5f) tmpAlpha = tmpAlpha + .1f;
                }

                clrMtrx[3, 3] = alphaFactor;
                imageAtt.SetColorMatrix(clrMtrx, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(pipBase, new Rectangle(pipX, 0, 17, 17), 0, 0, 17, 17, GraphicsUnit.Pixel, imageAtt);

                if (tmpAlpha > .1f) {
                    clrMtrx[3, 3] = tmpAlpha;
                    imageAtt.SetColorMatrix(clrMtrx, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(pipBase, new Rectangle(pipX + 20, 0, 17, 17), 0, 0, 17, 17, GraphicsUnit.Pixel, imageAtt);
                }
            } finally {
                lock (curBase) {
                    if (curBase != null) curBase.Dispose();
                    curBase = tmpBase.Clone() as Bitmap;
                }
                g.Dispose();
                pipBase.Dispose();
                tmpBase.Dispose();
            }
        }

        private void tmrPulse_Tick(object sender, System.EventArgs e) {
            this.PaintPulseIncrement();
            this.Invalidate();
        }

        private Bitmap GeneratePip() {
            Bitmap pipBase = new Bitmap(17, 17, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(pipBase);
            Pen dkpen = new Pen(dkBorder, 1);
            Pen ltpen = new Pen(ltBorder, 1);

            try {
                g.Clear(this.BackColor);
                g.DrawRectangle(dkpen, 0, 0, 16, 16);
                g.DrawRectangle(ltpen, 1, 1, 14, 14);
                g.DrawRectangle(dkpen, 2, 2, 12, 12);
                Rectangle ca = new Rectangle(3, 3, 11, 11);
                using (LinearGradientBrush gBrush = new LinearGradientBrush(ca, from, to, gMode))
                    g.FillRectangle(gBrush, ca);

                return pipBase.Clone() as Bitmap;
            } finally {
                //free resources
                g.Dispose();
                dkpen.Dispose();
                ltpen.Dispose();
                pipBase.Dispose();
            }

        }
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    //free global resources
                    imgBase.Dispose();
                    curBase.Dispose();
                    imageAtt.Dispose();
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();

            this.tmrPulse = new System.Windows.Forms.Timer(this.components);

            this.SuspendLayout();

            this.tmrPulse.Enabled = false;
            this.tmrPulse.Interval = 25;
            this.tmrPulse.Tick += new System.EventHandler(this.tmrPulse_Tick);

            this.ResumeLayout(false);
        }
        #endregion
    }
}
