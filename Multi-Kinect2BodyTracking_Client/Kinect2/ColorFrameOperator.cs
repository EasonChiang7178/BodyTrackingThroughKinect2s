using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//-- Using Kinect2 API --//
using Microsoft.Kinect;

namespace Kinect2
{
    class ColorFrameOperator : IDisposable
    {
        #region Members

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private Microsoft.Kinect.ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;

        /// <summary>
        /// The KinectSensor with 
        /// </summary>
        private Microsoft.Kinect.KinectSensor sensor = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the color bitmap to display
        /// </summary>
        public ImageSource ColorSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        #endregion

        #region Methods

        public ColorFrameOperator(Microsoft.Kinect.KinectSensor kinectSensor) {
            this.sensor = kinectSensor;
        }

        /// <summary>
        /// Start to retrieve the frame
        /// </summary>
        public void OpenStream() {
                // Open the reader for the color frames
            this.colorFrameReader = this.sensor.ColorFrameSource.OpenReader();
                // Wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
                // Create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
                // Create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e) {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // Verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        //==============================================//
        //              Disposing Methods               //
        //==============================================//
        /// <summary>
        /// Implement the IDisposable interface
        /// </summary>
        public void Dispose()
        {
            this.CloseManagedResource();
        }

        /// <summary>
        /// Base dipose method for inheritance using
        /// </summary>
        protected virtual void CloseManagedResource()
        {
            this.colorFrameReader.Dispose();
        }

        /// <summary>
        /// Alias for the dispose method
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        #endregion
    }
}
