//------------------------------------------------------------------------------
// <copyright file="KinectDiagnosticViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.Viewers {
    /// <summary>
    ///     Interaction logic for KinectDiagnosticViewer.xaml
    /// </summary>
    public partial class KinectDiagnosticViewer : UserControl {
        private readonly KinectSettings kinectSettings;
        private readonly Dictionary<KinectSensor, bool> sensorIsInitialized = new Dictionary<KinectSensor, bool>();
        private KinectSensor kinect;
        private bool kinectAppConflict;

        public KinectDiagnosticViewer() {
            InitializeComponent();
            kinectSettings = new KinectSettings(this);
            kinectSettings.PopulateComboBoxesWithFormatChoices();
            Settings.Content = kinectSettings;
            KinectColorViewer = colorViewer;
            StatusChanged();
        }

        public ImageViewer KinectColorViewer { get; set; }

        public KinectSensor Kinect {
            get { return kinect; }

            set {
                if (kinect != null) {
                    bool wasInitialized;
                    sensorIsInitialized.TryGetValue(kinect, out wasInitialized);
                    if (wasInitialized) {
                        UninitializeKinectServices(kinect);
                        sensorIsInitialized[kinect] = false;
                    }
                }

                kinect = value;
                kinectSettings.Kinect = value;
                if (kinect != null) {
                    if (kinect.Status == KinectStatus.Connected) {
                        kinect = InitializeKinectServices(kinect);

                        if (kinect != null) {
                            sensorIsInitialized[kinect] = true;
                        }
                    }
                }

                StatusChanged(); // update the UI about this sensor
            }
        }

        public void StatusChanged() {
            if (kinectAppConflict) {
                status.Text = "KinectAppConflict";
            }
            else if (Kinect == null) {
                status.Text = "Kinect initialize failed";
            }
            else {
                status.Text = Kinect.Status.ToString();

                if (Kinect.Status == KinectStatus.Connected) {
                    // Update comboboxes' selected value based on stream isenabled/format.
                    kinectSettings.colorFormats.SelectedValue = Kinect.ColorStream.Format;
                    kinectSettings.depthFormats.SelectedValue = Kinect.DepthStream.Format;
                    kinectSettings.trackingModes.SelectedValue = KinectSkeletonViewerOnDepth.TrackingMode;

                    kinectSettings.UpdateUiElevationAngleFromSensor();
                }
            }
        }

        // Kinect enabled apps should customize which Kinect services it initializes here.
        private KinectSensor InitializeKinectServices(KinectSensor sensor) {
            // Centralized control of the formats for Color/Depth and enabling skeletalViewer
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            kinectSettings.SkeletonStreamEnable.IsChecked = true; // will enable SkeletonStream if available

            // Inform the viewers of the Kinect KinectSensor.
            KinectColorViewer.Kinect = sensor;
            KinectDepthViewer.Kinect = sensor;
            KinectSkeletonViewerOnColor.Kinect = sensor;
            KinectSkeletonViewerOnDepth.Kinect = sensor;
            kinectAudioViewer.Kinect = sensor;

            // Start streaming
            try {
                sensor.Start();
                kinectAppConflict = false;
            }
            catch (IOException) {
                kinectAppConflict = true;
                return null;
            }

            sensor.AudioSource.Start();
            return sensor;
        }

        // Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        private void UninitializeKinectServices(KinectSensor sensor) {
            sensor.AudioSource.Stop();

            // Stop streaming
            sensor.Stop();

            // Inform the viewers that they no longer have a Kinect KinectSensor.
            KinectColorViewer.Kinect = null;
            KinectDepthViewer.Kinect = null;
            KinectSkeletonViewerOnColor.Kinect = null;
            KinectSkeletonViewerOnDepth.Kinect = null;
            kinectAudioViewer.Kinect = null;

            // Disable skeletonengine, as only one Kinect can have it enabled at a time.
            if (sensor.SkeletonStream != null) {
                sensor.SkeletonStream.Disable();
            }
        }
    }
}