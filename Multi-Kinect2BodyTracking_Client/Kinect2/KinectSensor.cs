using System;

using System.Windows.Media.Imaging;

//-- Using Kinect2 API --//
using Microsoft.Kinect;

//-- Design Patterns --//
using Patterns.Singleton;

namespace Kinect2
{
    class KinectSensor : SingletonBase< KinectSensor >
    {
        #region Members

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        protected Microsoft.Kinect.KinectSensor sensor = Microsoft.Kinect.KinectSensor.GetDefault();

        //----- COLOR STREAM -----//
        /// <summary>
        /// Reader for color frames
        /// </summary>
        private Microsoft.Kinect.ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;

        //----- INFRARED STREAM -----//
        /// <summary>
        /// Reader for infrared frames
        /// </summary>
        private Microsoft.Kinect.InfraredFrameReader infraredFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap infraredBitmap = null;

        //----- DEPTH STREAM -----//
        /// <summary>
        /// Reader for depth frames
        /// </summary>
        private Microsoft.Kinect.DepthFrameReader depthFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap depthBitmap = null;

        //----- BODY STREAM -----//
        /// <summary>
        /// Reader for body frames
        /// </summary>
        private Microsoft.Kinect.BodyFrameReader bodyFrameReader = null;

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new Kinect2 instance of the <see cref="Singleton"/> class.
        /// </summary>
        private KinectSensor()
        {}

        /// <summary>
        /// Active the Kinect v2 sensor
        /// </summary>
        public void Open() {
            sensor.Open();
        }

        #endregion
    }
}
