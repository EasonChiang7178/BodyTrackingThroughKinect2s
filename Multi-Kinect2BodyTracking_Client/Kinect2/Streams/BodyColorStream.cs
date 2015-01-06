using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//-- Using Kinect2 API --//
using Microsoft.Kinect;
using Kinect2.Parameters.Body;

namespace Kinect2.Streams
{
    class BodyColorStream : BodyStream
    {
        #region Members

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private Microsoft.Kinect.ColorFrameReader colorFrameReader = null;
        
        #endregion

        #region Properties

        /// <summary>
        /// Identification for the stream
        /// </summary>
        public override string StreamID
        {
            get { return "BodyColorStream"; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor of BodyStream
        /// </summary>
        /// <param name="kinectSensor"></param>
        public BodyColorStream(Microsoft.Kinect.KinectSensor kinectSensor)
            : base(kinectSensor)
        { }

        /// <summary>
        /// Start to retrieve the frame
        /// </summary>
        public override void Open()
        {
            // Get the coordinate mapper
            this.coordinateMapper = this.sensor.CoordinateMapper;

            /* Preparing the bodycolor image to display bodies */
                // Get the color (display) extents
            FrameDescription frameDescription = this.sensor.ColorFrameSource.FrameDescription;
                // Get size of joint space
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

                // Initialize color image
            this.imageBitmap = new WriteableBitmap(frameDescription.Width, frameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Initialize body image
                // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();
                // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

                // Access the parameters of body structure
            bodyStructure = BodyStructure.Instance;

                // Open the reader for the color frames
            this.colorFrameReader = this.sensor.ColorFrameSource.OpenReader();
                // Wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

                // Open the reader for the body frames
            this.bodyFrameReader = this.sensor.BodyFrameSource.OpenReader();
            this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
        }

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            /* ColorFrame is IDisposable */
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.imageBitmap.Lock();

                        // Verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.imageBitmap.PixelWidth) && (colorFrameDescription.Height == this.imageBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.imageBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.imageBitmap.AddDirtyRect(new Int32Rect(0, 0, this.imageBitmap.PixelWidth, this.imageBitmap.PixelHeight));
                        }

                        this.imageBitmap.Unlock();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                        this.bodies = new Body[bodyFrame.BodyCount];

                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    // Draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    int penIndex = 0;
                    foreach (Body body in this.bodies)
                    {
                        Pen drawPen = this.bodyStructure.bodyColors[penIndex++];

                        if (body.IsTracked == true)
                        {
                            this.DrawClippedEdges(body, dc);

                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                            /* Convert the joint points to depth (display) space */
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in joints.Keys)
                            {
                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0)
                                    position.Z = this.bodyStructure.inferredZPositionClamp;

                                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                            }

                            this.DrawBody(joints, jointPoints, dc, drawPen);

                            this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                            this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                        }
                    }
                    // Prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        /// <summary>
        /// Base dipose method for inheritance using
        /// </summary>
        protected override void CloseManagedResource()
        {
            if (this.colorFrameReader != null)
                this.colorFrameReader.Dispose();

            if (this.bodyFrameReader != null)
                this.bodyFrameReader.Dispose();
        }

        #endregion
    }
}
