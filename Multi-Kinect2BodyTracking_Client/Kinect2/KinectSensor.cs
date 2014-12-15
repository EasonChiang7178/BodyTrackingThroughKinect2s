using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//-- Using Kinect2 API --//
using Microsoft.Kinect;

//-- Design Patterns --//
using Patterns.Singleton;

namespace Kinect2
{
    class KinectSensor : SingletonBase< KinectSensor >, IDisposable
    {
        #region Members

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        protected Microsoft.Kinect.KinectSensor sensor = Microsoft.Kinect.KinectSensor.GetDefault();

        //===== COLOR STREAM =====//
        private ColorFrameOperator colorStream = null;

        ///// <summary>
        ///// Reader for color frames
        ///// </summary>
        //private Microsoft.Kinect.ColorFrameReader colorFrameReader = null;

        ///// <summary>
        ///// Bitmap to display
        ///// </summary>
        //private WriteableBitmap colorBitmap = null;

        //===== INFRARED STREAM =====//
        /// <summary>
        /// Reader for infrared frames
        /// </summary>
        private Microsoft.Kinect.InfraredFrameReader infraredFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap infraredBitmap = null;

        //===== DEPTH STREAM =====//
        /// <summary>
        /// Reader for depth frames
        /// </summary>
        private Microsoft.Kinect.DepthFrameReader depthFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap depthBitmap = null;

        //===== BODY STREAM =====//
        /// <summary>
        /// Reader for body frames
        /// </summary>
        private Microsoft.Kinect.BodyFrameReader bodyFrameReader = null;

        //===== BODY INDEX STREAM =====//
        /// <summary>
        /// Reader for body index frames
        /// </summary>
        private Microsoft.Kinect.BodyIndexFrameReader bodyindexFrameReader = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the color bitmap to display
        /// </summary>
        public ImageSource ColorSource
        {
            get
            {
                //return this.colorBitmap;
                return this.colorStream.ColorSource;
            }
        }

        /// <summary>
        /// Gets the depth bitmap to display
        /// </summary>
        public ImageSource DepthSource
        {
            get
            {
                return this.depthBitmap;
            }
        }

        /// <summary>
        /// Gets the infrared bitmap to display
        /// </summary>
        public ImageSource InfraredSource
        {
            get
            {
                return this.infraredBitmap;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new Kinect2 instance of the <see cref="Singleton"/> class. (Constructor)
        /// </summary>
        protected KinectSensor()
        {}

        /// <summary>
        /// Active the Kinect v2 sensor
        /// </summary>
        public void Open() {
            sensor.Open();
        }

        public void InitializeColorStream() {
            if (colorStream == null)
                colorStream = new ColorFrameOperator(this.sensor);
            this.colorStream.OpenStream();
        }

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
        protected virtual void CloseManagedResource() {
            //this.bodyFrameReader.Dispose();
            //this.colorFrameReader.Dispose();
            this.colorStream.Dispose();
            //this.depthFrameReader.Dispose();
            //this.infraredFrameReader.Dispose();
        }

        /// <summary>
        /// Alias for the dispose method
        /// </summary>
        public void Close() {
            this.Dispose();
        }

        #endregion
    }
}
