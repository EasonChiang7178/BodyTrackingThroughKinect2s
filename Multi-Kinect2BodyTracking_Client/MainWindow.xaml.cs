using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kinect2.MultiKinect2BodyTracking.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        private KinectSensor kinectSensor = null;

        #endregion 

        #region Properties

        public ImageSource ColorImageSource {
            get { return this.kinectSensor.ColorSource; }
        }

        #endregion

        #region Methods

        public MainWindow()
        {
            /* Open the Kinect sensor */
            kinectSensor = KinectSensor.Instance;
                // Initialize the stream we interested
            kinectSensor.InitializeColorStream();
                // Run Kinect!
            kinectSensor.Open();

            /* GUI Initialization */
                // Use the window object as the view model in this simple example
            this.DataContext = this;
                // Initialize the components (controls) of the window
            InitializeComponent();
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e) {
            this.kinectSensor.Close();
        }

        ///// <summary>
        ///// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        ///// </summary>
        //public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
