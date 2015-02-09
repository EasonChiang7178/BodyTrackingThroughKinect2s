//**************************************************************************************************
//This is a asynchronous server for multi-kinect human tracking system
//For more information, please refer to user manual.
//**************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using KinectData;
using _3DTools;


namespace Kinect2.MultiKinects2BodyTracking.Server {
    public partial class MainWindow : Window {
        int kinectNUM = 0;
        TextBox iniTxtBx = new TextBox();
        Label iniLabel = new Label();
        Grid iniGrid = new Grid();
        public static bool sendFusedDataToKinect = false;
        public static bool updateGUI = true;

        static private Object sendFusedDataLock = new Object();
        static private Object updateGUILock = new Object();

        static public void setFusedDataToKinect(bool value) {
            lock (sendFusedDataLock) {
                sendFusedDataToKinect = value;
            }
        }

        static public void setUpdateGUI(bool value) {
            lock (updateGUILock) {

                updateGUI = value;
            }
        }

        static public bool getFusedDataToKinect() {
            bool value = false;
            lock (sendFusedDataLock) {
                value = sendFusedDataToKinect;
            }
            return value;
        }

        static public bool getUpdateGUI() {
            bool value = false;
            lock (updateGUILock) {
                value = updateGUI;
            }
            return value;
        }


        public MainWindow() {
            InitializeComponent();

            iniLabel.Content = "Input Kinect number and press enter";
            iniLabel.Height = 30;
            iniLabel.VerticalAlignment = VerticalAlignment.Top;

            iniTxtBx.KeyDown += new KeyEventHandler(t_KeyDown);
            iniTxtBx.Height = 30;
            iniTxtBx.Margin = new Thickness(0, iniLabel.Height + 5, 0, 0);
            iniTxtBx.VerticalAlignment = VerticalAlignment.Top;

            iniGrid.Children.Add(iniLabel);
            iniGrid.Children.Add(iniTxtBx);
            iniGrid.Height = 100;
            iniGrid.Width = 250;



            this.Content = iniGrid;

        }

        //input kinect num when enter key is pressed
        void t_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                try {
                    kinectNUM = Convert.ToInt32(iniTxtBx.Text);

                    if (kinectNUM > 0) {
                        this.Content = viewer;
                        viewer.Content = rootGrid;
                        GUIComponents guiComp = new GUIComponents(kinectNUM, this);
                        myServer server = new myServer(this, guiComp);

                        guiComp.setServer(server, this);
                    } else {
                        MessageBox.Show("Please enter number >=0");
                    }
                } catch {
                    MessageBox.Show("Please enter a positive number");

                }
            }

        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            FusedDataProcessor.isWorking = false;

            //  waiting for thread to close
            for (int i = 0; i < 100; ++i) { Thread.Sleep(10); }

            //close all existing windows
            App.Current.Shutdown();
        }


    }

    public class initWindow : Window {
        public initWindow() {

        }



    }
}