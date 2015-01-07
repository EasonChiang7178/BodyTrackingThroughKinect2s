using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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

        private WriteableBitmap bodyBitmap;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private Microsoft.Kinect.ColorFrameReader colorFrameReader = null;

        private RenderTargetBitmap _bodySourceRTB;
        Grid rootGrid;
        Image bodyImage;

        private WriteableBitmap _colorWriteableBitmap;
        private WriteableBitmap _bodyWriteableBitmap;

        /// <summary>
        /// Size of the RGB pixel in the bitmap
        /// </summary>
        private readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Intermediate storage for receiving frame data from the sensor
        /// </summary>
        private byte[] pixels = null;
        private byte[] bodyBytespixels = null;

        #endregion

        #region Properties

        /// <summary>
        /// Identification for the stream
        /// </summary>
        public override string StreamID
        {
            get { return "BodyColorStream"; }
        }

        /// <summary>
        /// Hide original ImageSource with new, we used imageBitmap in SourceStream
        /// </summary>
        public override ImageSource ImageSource
        {
            get { return this.imageBitmap; }
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
            this.bodyBitmap = new WriteableBitmap(frameDescription.Width, frameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Initialize body image
                // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();
                // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // allocate space to put the pixels being received
            this.pixels = new byte[frameDescription.Width * frameDescription.Height * this.bytesPerPixel];
            this.bodyBytespixels = new byte[frameDescription.Width * frameDescription.Height * this.bytesPerPixel];

            _bodySourceRTB = new RenderTargetBitmap(displayWidth, displayHeight, 96.0, 96.0, PixelFormats.Pbgra32);
            rootGrid = new Grid();

            //_colorWriteableBitmap = BitmapFactory.New(frameDescription.Width, frameDescription.Height);
            //_bodyWriteableBitmap = BitmapFactory.New(frameDescription.Width, frameDescription.Height);

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
        private unsafe void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            /* ColorFrame is IDisposable */
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer()) {
                        this.imageBitmap.Lock();

                        // Verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.imageBitmap.PixelWidth) && (colorFrameDescription.Height == this.imageBitmap.PixelHeight)) {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.imageBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.imageBitmap.AddDirtyRect(new Int32Rect(0, 0, this.imageBitmap.PixelWidth, this.imageBitmap.PixelHeight));
                        }

                        this.imageBitmap.Unlock();

                        //imageBitmap.FromByteArray(this.pixels);
                        var rec = new Rect(0, 0, colorFrameDescription.Width, colorFrameDescription.Height);
                        using (imageBitmap.GetBitmapContext())
                        {
                            using (bodyBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
                            {
                                //imageBitmap.Blit(rec, bodyBitmap, rec, WriteableBitmapExtensions.BlendMode.None);

                                    // Copy the source image into a byte buffer
                                //int srcStride = imageBitmap.PixelWidth * (imageBitmap.Format.BitsPerPixel >> 3);
                                //byte[] srcBuffer = new byte[srcStride * imageBitmap.PixelHeight];
                                //imageBitmap.CopyPixels(srcBuffer, srcStride, 0);

                                    // Copy the dest image into a byte buffer
                                //int destStride = imageBitmap.PixelWidth * (bodyBitmap.Format.BitsPerPixel >> 3);
                                //byte[] destBuffer = new byte[(imageBitmap.PixelWidth * imageBitmap.PixelHeight) << 2];
                                //bodyBitmap.CopyPixels(new Int32Rect(0, 0, imageBitmap.PixelWidth, imageBitmap.PixelHeight), destBuffer, destStride, 0);

                                // Do merge
                                Byte* colorPtr = (Byte*)imageBitmap.BackBuffer;
                                Byte* bodyPtr = (Byte*)bodyBitmap.BackBuffer;
                                //int length = imageBitmap.BackBufferStride * imageBitmap.PixelHeight;
                                for (int i = 0; i < 8294400; i = i + 4)//imageBitmap.BackBufferStride * imageBitmap.PixelHeight; i = i + 3)
                                {
                                    if (bodyPtr[i] == 255 && bodyPtr[i + 1] == 255 && bodyPtr[i + 2] == 255)
                                        continue;

                                    colorPtr[i + 0] = bodyPtr[i + 0];
                                    colorPtr[i + 1] = bodyPtr[i + 1];
                                    colorPtr[i + 2] = bodyPtr[i + 2];
                                }

                                // Do merge
                                //Byte* destPtr = (Byte*)bodyBitmap.BackBuffer;
                                //for (int i = 0; i < imageBitmap.BackBufferStride * imageBitmap.PixelHeight; i = i + 4, destPtr++)
                                //{
                                //    if (destPtr[i] == 255 && destPtr[i + 1] == 255 && destPtr[i + 2] == 255)
                                //        continue;

                                    //float srcAlpha = ((float) srcBuffer[i + 3] / 255);
                                    //destBuffer[i + 0] = (byte)((srcBuffer[i + 0] * srcAlpha) + destBuffer[i + 0] * (1.0 - srcAlpha));
                                    //destBuffer[i + 1] = (byte)((srcBuffer[i + 1] * srcAlpha) + destBuffer[i + 1] * (1.0 - srcAlpha));
                                    //destBuffer[i + 2] = (byte)((srcBuffer[i + 2] * srcAlpha) + destBuffer[i + 2] * (1.0 - srcAlpha));
                                //}

                                // copy dest buffer back to the dest WriteableBitmap
                                //imageBitmap.WritePixels(new Int32Rect(0, 0, imageBitmap.PixelWidth, imageBitmap.PixelHeight), destBuffer, destStride, 0);
                            }
                        }
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
                    dc.DrawRectangle(Brushes.White, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

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

                                ColorSpacePoint colorSpacePoint = this.coordinateMapper.MapCameraPointToColorSpace(joints[jointType].Position);
                                jointPoints[jointType] = new Point(colorSpacePoint.X, colorSpacePoint.Y);
                            }

                            this.DrawBody(joints, jointPoints, dc, drawPen);

                            this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                            this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                        }
                    }
                    // Prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    bodyImage = new Image { Source = imageSource, Width = this.displayWidth, Height = this.displayHeight };
                    rootGrid.Children.Clear();
                    rootGrid.Children.Add(bodyImage);
                    rootGrid.Measure(new Size(bodyImage.Width, bodyImage.Height));
                    rootGrid.Arrange(new Rect(0, 0, bodyImage.Width, bodyImage.Height));
                    _bodySourceRTB.Clear();
                    _bodySourceRTB.Render(rootGrid);
                    _bodySourceRTB.CopyPixels(this.bodyBytespixels, displayWidth * this.bytesPerPixel, 0);
                    bodyBitmap.FromByteArray(this.bodyBytespixels);
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
