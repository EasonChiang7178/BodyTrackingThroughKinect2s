using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
    // Using multi-threads
using System.Threading;

using Kinect2.Streams;

using Kinect2.MultiKinects2BodyTracking.Client.TCPConnection;
using Kinect2.MultiKinects2BodyTracking.Client.ThreadProcedures;

using Kinect2.MultiKinects2BodyTracking.DataStructure;

namespace Kinect2.MultiKinects2BodyTracking.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        ///* About Kinectv2 Senser */
        /// <summary>
        /// Main object of Kinectv2 sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /* About the TCP/IP client */
        /// <summary>
        /// The relative path for the text file that store the address of the server
        /// </summary>
        private string filePathToServerIP = "../../ServerIP.txt";

        /// <summary>
        /// The IP address of the Kinectv2 server
        /// </summary>
        private string serverIP;

        /// <summary>
        /// Object used to manipulate the TCP/IP connection
        /// </summary>
        public TCPConnector tcpConnector;

        ///* About the data updata thread */
        private UpdateResultsToServer updateResults;
        private Thread updateResultsThread;
        public bool updateResultThreadAlive = false;

        private PrintTrackingResults printResults;
        private Thread printResultsThread;
        public bool printResultThreadAlive = true;

        public KinectData fusedKinectParameter = new KinectData();
        public KinectData kinectParameter_global = new KinectData();
        public KinectData kinectparameters_local = new KinectData();

        /* About UI control */
        /// <summary>
        /// The boolean used to indicate the connection status of the server
        /// </summary>
        public bool isConnectingToServer = false;

        #endregion 

        #region Properties

        public ImageSource ImageSource {
            get { return this.kinectSensor["BodyColorStream"]; }
        }

        public Microsoft.Kinect.Body[] Bodies {
            get { return this.kinectSensor.Bodies; }
        }

        #endregion

        #region Methods

        public MainWindow()
        {
            /* Open the Kinect sensor */
            kinectSensor = KinectSensor.Instance;
                // Initialize the stream we interested
            kinectSensor.AddStream<BodyColorStream>();
                // Run Kinect!
            kinectSensor.Open();

            /* GUI Initialization */
                // Use the window object as the view model in this simple example
            this.DataContext = this;
                // Initialize the components (controls) of the window
            InitializeComponent();

            /* Set up the TCP/IP connection */
            tcpConnector = new TCPConnector(clientTypes.KINECT);

            serverIP = System.IO.File.ReadAllText(filePathToServerIP);
            serverIP_TextBox.Text = serverIP;
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e) {
            this.kinectSensor.Close();

            this.printResultThreadAlive = false;
            if (this.updateResultThreadAlive) {
                e.Cancel = true;
                this.updateResultThreadAlive = false;
                MessageBox.Show("Please disconnect before closing");
            }
        }

        ///// <summary>
        ///// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        ///// </summary>
        //public event PropertyChangedEventHandler PropertyChanged;

        #endregion // Methods

        #region EventHandler

        /// <summary>
        /// Connect to central Kinectv2 server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, RoutedEventArgs e) {
                // Retrieve the final decision of the IP address of the server
            serverIP = serverIP_TextBox.Text;
                // Write back as the default IP address
            System.IO.File.WriteAllText(filePathToServerIP, serverIP);

            if (isConnectingToServer == false) {
                /* Try to make the connection... */
                try {
                    status_TextBlock.Text = "Connecting... " + serverIP;
                    /* Connect to server */
                    tcpConnector.ConnectToServer(serverIP);

                    status_TextBlock.Text = "Connected!";

                    /* Waiting for server welcome messages */
                    string serverFeedback = "";
                    while (serverFeedback == "")
                        serverFeedback = tcpConnector.ReceiveData_wait();

                    /* Tell the server this client type */
                    tcpConnector.SendData(tcpConnector.clientType.ToString());

                    /* Receive client ID from server */
                    serverFeedback = "";
                    while (serverFeedback == "")
                        serverFeedback = tcpConnector.ReceiveData_wait();

                    /* Extract the client ID from the message of server */
                    string[] temp = serverFeedback.Split(' ');
                    this.Title += "ID :" + temp[3];
                    //kinectID = Int32.Parse(temp[3]);
                    kinectparameters_local.kinectID = Int32.Parse(temp[3]);

                    Thread.Sleep(200);

                    status_TextBlock.Text = "Sending Data...";
                    
                    /* Start value updating thread */
                    updateResults = new UpdateResultsToServer(this);
                    updateResultsThread = new Thread(new ThreadStart(this.updateResults.ThreadProc));
                    updateResultsThread.IsBackground = true;
                    updateResultThreadAlive = true;
                    updateResultsThread.Start();
                    
                    /* Start kinect value printing thread */
                    printResults = new PrintTrackingResults(this);
                    printResultsThread = new Thread(new ThreadStart(this.printResults.ThreadProc));
                    printResultsThread.IsBackground = true;
                    printResultThreadAlive = true;
                    printResultsThread.Start();

                    // State changed
                    this.isConnectingToServer = true;

                    /* Control the appearance of UI */
                    serverIP_TextBox.IsEnabled = false;
                    connect_Button.Content = "Disconnect";
                    connect_Button.Background = Brushes.LightPink;

                } catch (Exception ex) {
                    status_TextBlock.Text = "Connection failed! : " + ex.ToString();
                }
            } /* Disconnect from server... */ 
            else {
                /* Close results updating thread */
                this.updateResultThreadAlive = false;
                if (updateResultsThread != null)
                    while (updateResultsThread.IsAlive == true)
                        this.status_TextBlock.Text = "Closing update value thread...";

                /* Close results printing thread */
                this.printResultThreadAlive = false;
                if (printResultsThread != null)
                    while (printResultsThread.IsAlive == true)
                        this.status_TextBlock.Text = "Closing print value thread...";

                try {
                    string messageFromServer = tcpConnector.CloseConnection();
                    status_TextBlock.Text = messageFromServer;

                    Thread.Sleep(1500);
                        // State changed
                    this.isConnectingToServer = false;

                    /* Control the appearance of UI */
                    serverIP_TextBox.IsEnabled = true;
                    connect_Button.Content = "Connect";
                    connect_Button.Background = Brushes.LightGreen;

                    status_TextBlock.Text = "Disconnected";
                } catch (Exception ex) {
                    status_TextBlock.Text = "Disconnection failed! : " + ex.ToString();
                }
            }
        }

        #endregion // EventHandler
    }
}
