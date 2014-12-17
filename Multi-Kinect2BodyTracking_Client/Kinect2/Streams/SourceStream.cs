using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//-- Using Kinect2 API --//
using Microsoft.Kinect;

namespace Kinect2.Streams
{
    abstract class SourceStream : IDisposable
    {
        #region Members

        /// <summary>
        /// The core of KinectSensor API
        /// </summary>
        protected Microsoft.Kinect.KinectSensor sensor = null;

        #endregion

        #region Properties

        /// <summary>
        /// Identification for the stream
        /// </summary>
        public abstract string streamID{
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor of SourceStream, passing the KinectSensor core to this object
        /// </summary>
        /// <param name="kinectSensor"></param>
        public SourceStream(Microsoft.Kinect.KinectSensor kinectSensor) {
            this.sensor = kinectSensor;
        }

        /// <summary>
        /// Start to retrieve the frame
        /// </summary>
        abstract public void OpenStream();

        //==============================================//
        //              Disposing Methods               //
        //==============================================//
        /// <summary>
        /// Implement the IDisposable interface
        /// </summary>
        public void Dispose() {
            this.CloseManagedResource();
        }

        /// <summary>
        /// Base dipose method for inheritance using
        /// </summary>
        protected abstract void CloseManagedResource();

        /// <summary>
        /// Alias for the dispose method
        /// </summary>
        public void Close() {
            this.Dispose();
        }

        #endregion
    }

    abstract class ImageStream : SourceStream
    {
        #region Members

        /// <summary>
        /// Bitmap to display
        /// </summary>
        protected WriteableBitmap imageBitmap = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the bitmap of the source to display
        /// </summary>
        public ImageSource imageSource {
            get { return this.imageBitmap; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor of ImageStream, passing the KinectSensor core to this object
        /// </summary>
        /// <param name="kinectSensor"></param>
        public ImageStream(Microsoft.Kinect.KinectSensor kinectSensor)
            : base(kinectSensor)
        {}

        #endregion
    }
}
