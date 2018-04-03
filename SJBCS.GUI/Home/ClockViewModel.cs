﻿using AMS.Utilities;
using System;
using System.Globalization;
using System.Windows.Threading;

namespace SJBCS.GUI.Home
{
    public class ClockViewModel : BindableBase
    {
        private string _digitalClock;
        private string _digitalCalendar;

        public ClockViewModel()

        {//Start of Digital Clock
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DigitalClock = DateTime.Now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture);
            DigitalCalendar = DateTime.Now.ToString("dddd\nMMMM dd, yyyy", CultureInfo.InvariantCulture);
        }

        public string DigitalClock
        {
            get { return _digitalClock; }
            set { SetProperty(ref _digitalClock, value); }
        }
        public string DigitalCalendar
        {
            get { return _digitalCalendar; }
            set { SetProperty(ref _digitalCalendar, value); }
        }
    }
}
