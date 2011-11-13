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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BitmapUtil {
    public static class Resize {

        public static Bitmap ResizeBitmap(Bitmap source, int maxWidth, int maxHeight, InterpolationMode i) {
            double scalingFactor; //Fraction which determines how much to downscale the source image.
            int sourceHeight = source.Height;
            int sourceWidth = source.Width;
            if (sourceHeight<=maxHeight && sourceWidth<=maxWidth) {
                scalingFactor = 1;
            } else {
                double heightRatio = (double) maxHeight/(double) sourceHeight;
                double widthRatio = (double) maxWidth/(double) sourceWidth;
                scalingFactor = heightRatio < widthRatio ? heightRatio : widthRatio;
            }
            return ResizeBitmap(source, scalingFactor, i);
        }

        public static Bitmap ResizeBitmap(Bitmap source, int maxWidth, int maxHeight) {
            return ResizeBitmap(source, maxWidth, maxHeight, InterpolationMode.HighQualityBicubic);
        }

        public static Bitmap ResizeBitmap(Bitmap source, double scalingFactor, InterpolationMode i) {
            int sourceHeight = source.Height;
            int sourceWidth = source.Width;
            int resultHeight = (int)(sourceHeight * scalingFactor);
            int resultWidth = (int)(sourceWidth * scalingFactor);
            Bitmap result = new Bitmap(resultWidth, resultHeight, PixelFormat.Format32bppArgb);
            result.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (Graphics g = Graphics.FromImage(result)) {
                g.InterpolationMode = i;
                g.DrawImage(source, 0, 0, resultWidth, resultHeight);
            }
            return result;
        }

        public static Bitmap ResizeBitmap(Bitmap source, double scalingFactor) {
            return ResizeBitmap(source, scalingFactor, InterpolationMode.HighQualityBicubic);
        }
    }
}
