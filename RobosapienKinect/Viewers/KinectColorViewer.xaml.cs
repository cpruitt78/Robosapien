//------------------------------------------------------------------------------
// <copyright file="KinectColorViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.Viewers {
    /// <summary>
    ///     Interaction logic for KinectColorViewer.xaml
    /// </summary>
    public partial class KinectColorViewer : ImageViewer {
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7)/8;

        private ColorImageFormat lastImageFormat = ColorImageFormat.Undefined;
        private WriteableBitmap outputImage;
        private byte[] pixelData;

        public KinectColorViewer() {
            InitializeComponent();
        }

        protected override void OnKinectChanged(KinectSensor oldKinectSensor, KinectSensor newKinectSensor) {
            if (oldKinectSensor != null) {
                oldKinectSensor.ColorFrameReady -= ColorImageReady;
                kinectColorImage.Source = null;
                lastImageFormat = ColorImageFormat.Undefined;
            }

            if (newKinectSensor != null && newKinectSensor.Status == KinectStatus.Connected) {
                ResetFrameRateCounters();

                if (newKinectSensor.ColorStream.Format == ColorImageFormat.RawYuvResolution640x480Fps15) {
                    throw new NotImplementedException("RawYuv conversion is not yet implemented.");
                }
                else {
                    newKinectSensor.ColorFrameReady += ColorImageReady;
                }
            }
        }

        private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e) {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame()) {
                if (imageFrame != null) {
                    // We need to detect if the format has changed.
                    bool haveNewFormat = lastImageFormat != imageFrame.Format;

                    if (haveNewFormat) {
                        pixelData = new byte[imageFrame.PixelDataLength];
                    }

                    imageFrame.CopyPixelDataTo(pixelData);

                    // A WriteableBitmap is a WPF construct that enables resetting the Bits of the image.
                    // This is more efficient than creating a new Bitmap every frame.
                    if (haveNewFormat) {
                        kinectColorImage.Visibility = Visibility.Visible;
                        outputImage = new WriteableBitmap(
                            imageFrame.Width,
                            imageFrame.Height,
                            96, // DpiX
                            96, // DpiY
                            PixelFormats.Bgr32,
                            null);

                        kinectColorImage.Source = outputImage;
                    }

                    outputImage.WritePixels(
                        new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height),
                        pixelData,
                        imageFrame.Width*Bgr32BytesPerPixel,
                        0);

                    lastImageFormat = imageFrame.Format;

                    UpdateFrameRate();
                }
            }
        }
    }
}