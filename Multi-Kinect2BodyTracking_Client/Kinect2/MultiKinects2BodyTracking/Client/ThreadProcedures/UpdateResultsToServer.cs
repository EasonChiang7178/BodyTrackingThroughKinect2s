using System;
using System.IO;
    // Using multi-threads
using System.Threading;

using Kinect2.MultiKinects2BodyTracking.Client.TCPConnection;
using Kinect2.MultiKinects2BodyTracking.DataStructure;

namespace Kinect2.MultiKinects2BodyTracking.Client.ThreadProcedures {
    /// <summary>
    /// The thread procedure used to update tracking results to the central server
    /// </summary>
    public class UpdateResultsToServer {

        #region Members

        /// <summary>
        /// MainWindow of the current application
        /// </summary>
        private MainWindow mw;

        /// <summary>
        /// Used to log the timestamp
        /// </summary>
        public DateTime time_start = DateTime.Now;

        public bool counting = false;

        #endregion // Members

        #region Methods

        /// <summary>
        /// Constructors, used to pass the current MainWindow of the application
        /// </summary>
        /// <param name="_m"></param>
        public UpdateResultsToServer(MainWindow _m) {
            mw = _m;
        }

        /// <summary>
        /// Save step time to log txt file
        /// </summary>
        void RecordFrameRate() {
            using (StreamWriter timeLog = File.AppendText("time.txt")) {
                timeLog.WriteLine((DateTime.Now - time_start).TotalMilliseconds.ToString());
                time_start = DateTime.Now;
            }
        }

        /// <summary>
        /// Main loop used for thread to update data to central server
        /// </summary>
        public void ThreadProc() {
            while (mw.updateResultThreadAlive == true)
            {
                //log step time to txt
                if (counting)
                    this.RecordFrameRate();

                int action;
                string dataToSend, resultData;

                /* If the server asks for image data, send it */
                // Receive data from server
                resultData = mw.tcpConnector.ReceiveData_wait();

                try
                {
                    string[] s = resultData.Split(' ');
                    if (int.Parse(s[1]) == (int) DownloadCommands.Get_all_kinect_images)
                        mw.tcpConnector.SendData("image data");
                }
                catch { }

                /* Send command to update data to server */
                action = (int) UploadCommands.Update_knect_data_in_Base64_format;
                dataToSend = "u " + action.ToString() + " " + mw.kinectparameters_local.GetAllParameterStringInBase64();
                mw.tcpConnector.SendData(dataToSend);

                /* Download fused data from central server */
                action = (int) DownloadCommands.Download_fused_kinect_data_in_Base64_string_format;// clientActions.downloadFusedKinectData;
                dataToSend = "d " + action.ToString() + " ";
                mw.tcpConnector.SendData(dataToSend);
                resultData = "";
                resultData = mw.tcpConnector.ReceiveData_wait();

                /* Save fused data */
                if (resultData != "")
                {
                    try
                    {
                        string[] ss = resultData.Split('#');
                        mw.fusedKinectParameter.AssignByAllParameterStringInBase64(ss[1]);
                    }
                    catch (Exception ex) { /* Ignore failed data and continue */ }
                }
            }
        }

        #endregion // Methods
    }
}