﻿using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using MaterialDesignThemes.Wpf;
using SJBCS.Model;
using SJBCS.Wrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJBCS.ViewModel
{

    class EnrollStudentBiometricsViewModel : FingerScanner, INotifyPropertyChanged
    {
        delegate void Function();

        private bool _canShowNotification;
        private bool _canSave;
        private int _indicator;
        private Enrollment _enroller;
        private Bitmap _fingerScanImage;
        private String _notification;
        private DPFP.Template Template;
        private SampleConversion _converter;
        private Biometric _bio;
        private static AMSEntities DBContext = new AMSEntities();
        private BiometricWrapper _biometricWrapper;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public EnrollStudentBiometricsViewModel()
        {
            _indicator = 0;
            _canShowNotification = false;
            _enroller = new Enrollment();
            _converter = new SampleConversion();
            _bio = new Biometric();
            _biometricWrapper = new BiometricWrapper();
            Start();

        }

        public Object FingerScanImage => _fingerScanImage;
        public String Notification => _notification;
        public int Indicator
        {
            get
            {
                return _indicator;
            }
            set
            {
                _indicator = value;
            }
        }

        public bool CanShowNotification => _canShowNotification;
        public bool CanSave => _canSave;


        protected override void Process(DPFP.Sample Sample)
        {
            base.Process(Sample);
            Bitmap bitmap = null;
            _converter.ConvertToPicture(Sample, ref bitmap);
            _fingerScanImage = bitmap;
            RaisePropertyChanged("FingerScanImage");

            // Process the sample and create a feature set for the enrollment purpose.
            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Enrollment);

            // Check quality of the sample and add to enroller if it's good
            if (features != null) try
                {

                    _enroller.AddFeatures(features);     // Add feature set to template.
                    _notification = "The fingerprint feature set was created.";
                    _canShowNotification = true;
                    _indicator += 25;
                    RaisePropertyChanged(null);
                }
                finally
                {
                    //UpdateStatus();

                    // Check if template has been created.
                    switch (_enroller.TemplateStatus)
                    {
                        case DPFP.Processing.Enrollment.Status.Ready:   // report success and stop capturing
                            OnTemplate(_enroller.Template);
                            _notification = "Finger scan completed.!!";
                            _canShowNotification = true;
                            RaisePropertyChanged(null);
                            Stop();
                            break;

                        case DPFP.Processing.Enrollment.Status.Failed:  // report failure and restart capturing
                            _enroller.Clear();
                            Stop();
                            _notification = "Keep on pressing. . .";
                            _canShowNotification = true;
                            RaisePropertyChanged(null);
                            OnTemplate(null);
                            Start();
                            break;
                    }
                }
        }

        private void OnTemplate(DPFP.Template template)
        {
            Template = template;
            _canSave = (Template != null);
            if (Template != null)
            {
                _notification = "The fingerprint template is ready for fingerprint verification.";
                Console.WriteLine("The fingerprint template is ready for fingerprint verification.");
                MemoryStream fingerprintData = new MemoryStream();
                Template.Serialize(fingerprintData);
                fingerprintData.Position = 0;
                BinaryReader br = new BinaryReader(fingerprintData);
                Byte[] bytes = br.ReadBytes((Int32)fingerprintData.Length);
                _bio.FingerID = Guid.NewGuid();
                _bio.FingerPrintTemplate = bytes;
                _biometricWrapper.Add(DBContext,_bio);
            }
            else
                _notification = "The fingerprint template is not valid. Repeat fingerprint enrollment.";
            RaisePropertyChanged(null);
        }

        private void RaisePropertyChanged(string v)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(v));
            }
        }

    }
}
