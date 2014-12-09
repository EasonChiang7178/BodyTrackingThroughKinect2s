using System;
using Patterns.Singleton;

namespace Kinect2
{
    class Kinect2 : SingletonBase< Kinect2 >
    {
        /// <summary>
        /// Initializes a new Kinect2 instance of the <see cref="Singleton"/> class.
        /// </summary>
        private Kinect2()
        {
            Console.WriteLine("Hello Kinect2!");
        }
    }
}
