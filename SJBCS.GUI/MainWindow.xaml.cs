﻿using SJBCS.GUI.SMS;
using SJBCS.GUI.Utilities;
using System;
using System.Threading;
using System.Windows;
using Unity;

namespace SJBCS.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static System.Threading.Mutex singleton = new Mutex(true, "SJBCS");

        public MainWindow()
        {
            if (!singleton.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Another instance of this application is already running.", "Important Note", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            InitializeComponent();
            DataContext = ContainerHelper.Container.Resolve<MainWindowViewModel>();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            SMSSetup.Instance.StopSMSService();
            Close();
        }
    }
}
