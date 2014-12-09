using System;

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
        private Microsoft.Kinect.KinectSensor sensor = null;

        #endregion

        /// <summary>
        /// Initializes a new Kinect2 instance of the <see cref="Singleton"/> class.
        /// </summary>
        private KinectSensor()
        {}
    }
}
