﻿using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//-- Using Kinect2 API --//
using Microsoft.Kinect;

using Kinect2.Streams;
using Kinect2.Exceptions;

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
        protected Dictionary<string, ImageStream> openedStreams = new Dictionary<string, ImageStream>();

        #endregion

        #region Properties

        public ImageSource this[string streamName] {
            get {
                if (openedStreams.ContainsKey(streamName) == true)
                    return openedStreams[streamName].imageSource;

                throw new StreamHasNotBeenOpened(streamName);
            }
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
        /// Active the specific stream to obtain the image
        /// </summary>
        public void AddStream< T >() where T : ImageStream {
            ImageStream newStream = (T) Activator.CreateInstance(typeof(T), new object[] { sensor });
            newStream.Open();

            openedStreams.Add(newStream.StreamID, newStream);
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
            /* Releasing all the opened streams */
            foreach (KeyValuePair<string, ImageStream> stream in openedStreams)
                stream.Value.Dispose();

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
