using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
    // Using TCP/IP functions
using System.Net.Sockets;
    // Using multi-threads
using System.Threading;

using Kinect2.Streams;

namespace Kinect2.MultiKinect2BodyTracking.Client
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
        string filePathToServerIP = "../../ServerIP.txt";

        /// <summary>
        /// The IP address of the Kinectv2 server
        /// </summary>
        string serverIP;
        
        private int clientType = (int) clientTypes.KINECT;

        System.Net.Sockets.TcpClient readingSocket = new System.Net.Sockets.TcpClient();
        System.Net.Sockets.TcpClient sendingSocket = new System.Net.Sockets.TcpClient();

        ///* About the data updata thread */
        private UpdateValueToServerThread valueUpdateThread;
        private Thread valueUpdateThreadInMain;

        private PrintKinectParameterThread printValueThread;
        private Thread printValueThreadInMain;

        /* About UI control */
        /// <summary>
        /// The boolean used to indicate the connection status of the server
        /// </summary>
        bool isConnectingToServer = false;

        #endregion 

        #region Properties

        public ImageSource ImageSource {
            get { return this.kinectSensor["BodyColorStream"]; }
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
                    sendingSocket.Connect(serverIP, 8888);
                    readingSocket.Connect(serverIP, 8889);

                    status_TextBlock.Text = "Connected!";

                    /* Waiting for server welcome messages */
                    string serverFeedback = "";
                    while (serverFeedback == "")
                        serverFeedback = ReceiveData_wait();

                    /* Tell the server this client type */
                    SendData(clientType.ToString());

                    /* Receive client ID from server */
                    serverFeedback = "";
                    while (serverFeedback == "")
                        serverFeedback = ReceiveData_wait();

                    /* Extract the client ID from the message of server */
                    string[] temp = serverFeedback.Split(' ');
                    this.Title += "ID :" + temp[3];
                    kinectID = Int32.Parse(temp[3]);
                    kinectparameters_local.kinectID = kinectID;

                    Thread.Sleep(200);

                    /* Start value updating thread */
                    status_TextBlock.Text = "Sending Data...";
                    isUpdating = true;
                    valueUpdateThread = new UpdateValueToServerThread(this);
                    valueUpdateThreadInMain = new Thread(this.valueUpdateThread.ThreadProc);
                    valueUpdateThreadInMain.IsBackground = true;
                    valueUpdateThreadInMain.Start();

                    /* Start kinect value printing thread */
                    isPrinting = true;
                    printValueThread = new PrintKinectParameterThread(this);
                    printValueThreadInMain = new Thread(this.printValueThread.ThreadProc);
                    printValueThreadInMain.IsBackground = true;
                    printValueThreadInMain.Start();

                    // State changed
                    this.isConnectingToServer = true;

                    /* Control the appearance of UI */
                    serverIP_TextBox.IsEnabled = false;
                    connect_Button.Content = "Disconnect";
                    connect_Button.Background = Brushes.LightPink;

                }
                catch (Exception ex)
                {
                    status_TextBlock.Text = "Connection failed! : " + ex.ToString();
                }
            } /* Disconnect from server... */ 
            else {
                    //close value updating thread
                if (valueUpdateThreadInMain != null)
                    while (valueUpdateThreadInMain.IsAlive)
                        setStatus("closing update value thread...");
                    //close value printing thread
                if (printValueThreadInMain != null)
                    while (printValueThreadInMain.IsAlive)
                        setStatus("closing print value thread...");

                try {
                    CloseConnection();
                    status_TextBlock.Text = "Disconnected";

                    // State changed
                    this.isConnectingToServer = false;

                    /* Control the appearance of UI */
                    serverIP_TextBox.IsEnabled = true;
                    connect_Button.Content = "Connect";
                    connect_Button.Background = Brushes.LightGreen;
                } catch (Exception ex) {
                    status_TextBlock.Text = "Disconnection failed! : " + ex.ToString();
                }
            }
        }

        #endregion // EventHandler

        #region Threads

            //this is the thread used to update tracking results to central server
        public class UpdateValueToServerThread
        {
            private MainWindow mw;
            public UpdateValueToServerThread(MainWindow m)
            {
                mw = m;
            }
            public DateTime time_start;
            public bool counting = false;

            //save step time to log txt file
            void recordFrameRate()
            {
                using (StreamWriter timeLog = File.AppendText("time.txt"))
                {
                    timeLog.WriteLine((DateTime.Now - time_start).TotalMilliseconds.ToString());
                    time_start = DateTime.Now;
                }
            }

            public void ThreadProc()
            {
                //the loop of updating data to central server
                while (mw.isUpdating)
                {
                    //log step time to txt
                    if (counting)
                    {
                        recordFrameRate();
                    }
                    int action;
                    string datatosend, resultdata;

                    //receive data from server
                    resultdata = mw.ReceiveData_wait();

                    //if server asks for image data, send it
                    try
                    {
                        string[] s = resultdata.Split(' ');
                        if (int.Parse(s[1]) == (int)DownloadCommands.Get_all_kinect_images)
                        {
                            mw.SendData("image data");
                        }
                    }
                    catch { }

                    //send command to update data to server
                    action = (int)UploadCommands.Update_knect_data_in_Base64_format;
                    datatosend = "u " + action.ToString() + " " + mw.kinectparameters_local.getAllParameterStringInBase64();
                    mw.SendData(datatosend);

                    //download fused data from central server
                    action = (int)DownloadCommands.Download_fused_kinect_data_in_Base64_string_format;// clientActions.downloadFusedKinectData;
                    datatosend = "d " + action.ToString() + " ";
                    mw.SendData(datatosend);
                    resultdata = "";
                    resultdata = mw.ReceiveData_wait();

                    //save fused data to object
                    if (resultdata != "")
                    {
                        try
                        {
                            string[] ss = resultdata.Split('#');
                            mw.fusedKinectParameter.assignByAllParameterStringInBase64(ss[1]);
                        }
                        catch (Exception ex)
                        {
                            //ignore failed data and continue
                        }
                    }
                }
            }
        }

        //this is the thread used to print kinect data on GUI
        public class PrintKinectParameterThread
        {
            private MainWindow mw;
            public PrintKinectParameterThread(MainWindow m)
            {
                mw = m;
            }

            delegate void printValueCallback(TextBlock t);
            public void printValue(TextBlock t)
            {
                if (mw.kinect_status_textBlock.Dispatcher.Thread != Thread.CurrentThread)
                {
                    mw.kinect_status_textBlock.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new printValueCallback(this.printValue), mw.kinect_status_textBlock);
                }
                else
                {
                    mw.kinect_status_textBlock.Text = mw.kinectparameters_local.printKinectParameters();

                }
            }
            public void ThreadProc()
            {
                while (mw.isPrinting)
                {
                    printValue(mw.kinect_status_textBlock);
                    //slow down the GUI updating rate to reduce cost
                    Thread.Sleep(500);
                }
            }

        }

        #endregion // Threads
    }
}
