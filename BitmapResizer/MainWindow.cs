/*
Copyright (c) 2011 Arthur Shagall, Mindflight, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BitmapResizer {
    public partial class MainWindow : Form {
        private delegate void ShowError(string error);

        private InterpolationMode[] intModes;
        private string currentBitmap;

        public MainWindow() {
            InitializeComponent();
            currentBitmap = null;
            intModes = (InterpolationMode[])Enum.GetValues(typeof (InterpolationMode));
            string[] intModeNames = new string[intModes.Length-1];
            for(int x=0; x< intModes.Length-1; x++) {
                intModeNames[x] = intModes[x].ToString();
            }
            resizeQualityComboBox.Items.AddRange(intModeNames);
            Load += new EventHandler(MainWindow_Load);
            //BackgroundWorker
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            saveButton.Enabled = true;
            openToolStripMenuItem.Enabled = true;
            this.Cursor = Cursors.Default;
            progressBar.Stop();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e) {
            object[] args = (object[]) e.Argument;
            string saveBitmap = (string) args[0];
            InterpolationMode iMode = (InterpolationMode) args[1];
            int width = (int) args[2];
            int height = (int) args[3];
            try {
                using (Bitmap b = (Bitmap)Bitmap.FromFile(currentBitmap)) {
                    using (Bitmap bim = BitmapUtil.Resize.ResizeBitmap(b, width, height, iMode)) {
                        bim.Save(saveBitmap);
                    }
                }
            } catch (Exception x) {
                ShowSaveError("Error while saving " + saveBitmap + "\r\n" + x.Message);
            }
        }

        void MainWindow_Load(object sender, EventArgs e) {
            resizeQualityComboBox.SelectedIndex = 7;
            saveButton.Enabled = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            Open();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            AboutWindow w = new AboutWindow();
            w.ShowDialog(this);
        }

        private void Open() {
            DialogResult dr = ofDialog.ShowDialog(this);
            if (dr == DialogResult.OK) {
                string filename = ofDialog.FileName;
                try {
                    using (Bitmap b = (Bitmap)Bitmap.FromFile(filename)) {
                        Bitmap bim = BitmapUtil.Resize.ResizeBitmap(b, 192, 192, InterpolationMode.Bilinear);
                        currentBitmap = filename;
                        Image oi = picBox.Image;
                        picBox.Image = bim;
                        if (oi != null) {
                            oi.Dispose();
                        }
                        filenameLabel.Text = filename;
                        widthLabel.Text = "Width: " + b.Width;
                        heightLabel.Text = "Height: " + b.Height;
                        saveButton.Enabled = true;
                    }
                } catch (Exception e) {
                    MessageBox.Show(this, "An error occured while opening " + filename + "\r\n" + e.Message,
                                    "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
            }
        }

        private void ShowSaveError(string error) {
            if (InvokeRequired) {
                this.Invoke(new ShowError(ShowSaveError), error);
            } else {
                MessageBox.Show(this, error, "Image Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveButton_Click(object sender, EventArgs e) {
            string extension = Path.GetExtension(currentBitmap);
            string fp = currentBitmap.Substring(0, currentBitmap.Length - extension.Length) + "_resized" + extension;
            saveDialog.FileName = fp;
            DialogResult dr = saveDialog.ShowDialog(this);
            if (dr == DialogResult.OK) {
                InterpolationMode iMode = intModes[resizeQualityComboBox.SelectedIndex];
                int width = (int)widthSpinner.Value;
                int height = (int) heightSpinner.Value;
                saveButton.Enabled = false;
                openToolStripMenuItem.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                progressBar.Start();
                object[] args = new object[4];
                args[0] = saveDialog.FileName;
                args[1] = iMode;
                args[2] = width;
                args[3] = height;
                bw.RunWorkerAsync(args);
            }
        }
    }
}