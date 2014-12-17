using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//-- Using Kinect2 API --//
using Microsoft.Kinect;

using Kinect2.Streams;

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

        /// <summary>
        /// A dictionary used to store the opened stream. Key is the ID of the stream, and the value is the stream itself 
        /// </summary>
        protected Dictionary<string, SourceStream> openedStreams = new Dictionary<string, SourceStream>();

        //===== COLOR STREAM =====//
        private ColorStream colorStream = null;

        //===== INFRARED STREAM =====//

        //===== DEPTH STREAM =====//

        //===== BODY STREAM =====//

        //===== BODY INDEX STREAM =====//

        #endregion

        #region Properties

        /// <summary>
        /// Gets the color bitmap to display
        /// </summary>
        public ImageSource ColorSource {
            get { return this.colorStream.imageSource; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new Kinect2 instance of the <see cref="Singleton"/> class. (Constructor)
        /// </summary>
        private KinectSensor()
        {}

        /// <summary>
        /// Active the Kinect v2 sensor
        /// </summary>
        public void Open() {
            sensor.Open();
        }

        /// <summary>
        /// Active the reader for obtaining the color image
        /// </summary>
        public void InitializeColorStream() {
            if (colorStream == null)
                colorStream = new ColorStream(this.sensor);
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
            if (colorStream != null)    this.colorStream.Dispose();
            
            sensor.Close();
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
