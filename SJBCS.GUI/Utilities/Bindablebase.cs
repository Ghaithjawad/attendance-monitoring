﻿using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SJBCS.GUI.Utilities
{

    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void SetProperty<T>(ref T member, T val, [CallerMemberName] string propertyName = null)
        {
            member = val;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }

        protected void ExtendedOpenedEventHandler(object sender, DialogOpenedEventArgs eventargs) { }

        protected void ExtendedClosingEventHandler(object sender, DialogClosingEventArgs eventArgs) { }

        public event EventHandler CloseWindowRequested;
        protected void OnCloseWindow()
        {
            CloseWindowRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
