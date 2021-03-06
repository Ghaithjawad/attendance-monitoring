﻿using DPFP;
using DPFP.Verification;
using MaterialDesignThemes.Wpf;
using SJBCS.Data;
using SJBCS.GUI.Dialogs;
using SJBCS.GUI.Home;
using SJBCS.GUI.SMS;
using SJBCS.GUI.Utilities;
using SJBCS.Services.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Timers;
using System.Windows;
using Unity;
using static DPFP.Verification.Verification;

namespace SJBCS.GUI.AMS
{
    public class AttendanceViewModel : FingerScanner
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties
        private IStudentsRepository _studentsRepository;
        private IBiometricsRepository _biometricsRepository;
        private IRelBiometricsRepository _relBiometricsRepository;
        private IAttendancesRepository _attendancesRepository;
        private bool _IsFingerEnrolled;
        private Attendance _attendance;
        private Biometric _biometric;
        private RelBiometric _relBiometric;
        private Template _Template;
        private Verification _Verificator;
        private List<Biometric> _Biometrics;

        public RelayCommand ResendCommand { get; private set; }
        private Timer smsTimer;

        private Data.Student _student;
        public Data.Student Student
        {
            get { return _student; }
            set { SetProperty(ref _student, value); }
        }

        private ObservableCollection<AttendanceLog> _attendanceLogs;
        public ObservableCollection<AttendanceLog> AttendanceLogs
        {
            get { return _attendanceLogs; }
            set
            {
                SetProperty(ref _attendanceLogs, value);
            }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private string _scannerStatus;
        public string ScannerStatus
        {
            get { return _scannerStatus; }
            set { SetProperty(ref _scannerStatus, value); }
        }

        private string _smsStatus;
        public string SMSStatus
        {
            get { return _smsStatus; }
            set { SetProperty(ref _smsStatus, value); }
        }

        private Visibility _smsStatusVisibility = Visibility.Collapsed;
        public Visibility SMSStatusVisibility
        {
            get { return _smsStatusVisibility; }
            set { SetProperty(ref _smsStatusVisibility, value); }
        }

        private string _remarks;
        public string Remarks
        {
            get { return _remarks; }
            set { SetProperty(ref _remarks, value); }
        }

        public MainClockViewModel _mainClockViewModel;
        public MainClockViewModel MainClockViewModel
        {
            get { return _mainClockViewModel; }
            set { SetProperty(ref _mainClockViewModel, value); }

        }

        #endregion

        public AttendanceViewModel(IStudentsRepository studentsRepository, BiometricsRepository biometricsRepository, RelBiometricsRepository relBiometricsRepository, AttendancesRepository attendancesRepository)
        {
            _studentsRepository = studentsRepository;
            _biometricsRepository = biometricsRepository;
            _relBiometricsRepository = relBiometricsRepository;
            _attendancesRepository = attendancesRepository;

            Initialize(true);
            _attendanceLogs = new ObservableCollection<AttendanceLog>();
            _mainClockViewModel = ContainerHelper.Container.Resolve<MainClockViewModel>();
            ResendCommand = new RelayCommand(OnResend);
            Start();    //Begin capture
        }

        #region Methods

        public override void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            ScannerStatus = "Connected";
        }

        public override void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            ScannerStatus = "Disconnected";
        }

        protected override async void Process(Sample Sample)
        {
            try
            {
                //Save current finger print sample for processing and stop capturing incoming fingerprint samples
                base.Process(Sample);
                Stop();

                // Process the sample and create a feature set for the enrollment purpose.
                FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

                // Check quality of the sample and start verification if it's good
                if (features != null)
                {
                    IsLoading = true;
                    _IsFingerEnrolled = ValidateFinger(features);
                    IsLoading = false;
                    
                    if (!_IsFingerEnrolled)
                    {
                        Remarks = "Fingerprint not recognized.";
                        Student = null;
                        Initialize(false);
                    }

                    _IsFingerEnrolled = false;
                }
            }
            catch (Exception error)
            {
                Logger.Error(error);
                Remarks = "Failed to process fingerprint, try again.";
                //var result =  await DialogHelper.ShowDialog(DialogType.Error, "Something went wrong with the finger scanner. Please restart the application.");
                //Application.Current.Shutdown();
            }

            Start();
        }

        private bool ValidateFinger(FeatureSet FeatureSet)
        {
            MemoryStream fingerprintData = null;
            Result result = null;

            foreach (Biometric biometric in _Biometrics) // Loop on the FPT List from DB to Compare the feature set with the DB templates                    
            {
                fingerprintData = new MemoryStream(biometric.FingerPrintTemplate);
                _Template = new Template(fingerprintData);
                result = new Result();
                _Verificator.Verify(FeatureSet, _Template, ref result);

                if (result.Verified)
                {
                    _Verificator = new DPFP.Verification.Verification();
                    _relBiometric = _relBiometricsRepository.GetRelBiometric(biometric.FingerID);

                    if (_relBiometric != null)  //Check if finger is enrolled.
                    {
                        _student = _studentsRepository.GetStudent(_relBiometric.StudentID);
                        Student = _student;
                        _attendance = _attendancesRepository.GetAttendanceByStudentID(_student.StudentGuid);

                        if (_attendance != null)    //Check if student has already logged in for the day.
                        {
                            if (_attendance.TimeOut == null)    //Check if student already logged out for the day
                            {
                                DateTime login = _attendance.TimeIn;
                                DateTime logout = DateTime.Now;
                                logout = new DateTime(logout.Year, logout.Month, logout.Day, logout.Hour, logout.Minute, 0);

                                if (logout > login.Add(new TimeSpan(1, 0, 0))) //Check if TimeSpan between login and logout is greater than allowed threshold
                                {
                                    _attendance.IsOverstay = false;
                                    _attendance.IsEarlyOut = false;

                                    //Update student logout
                                    _attendance.TimeOut = logout;

                                    //Check if student is overstay or undertime;
                                    TimeSpan timeOut = logout.TimeOfDay;
                                    timeOut = new TimeSpan(timeOut.Hours, timeOut.Minutes, 0);
                                    TimeSpan endTime = _student.Section.EndTime;

                                    if (timeOut >= endTime.Add(new TimeSpan(1, 0, 0)))
                                    {
                                        _attendance.IsOverstay = true;
                                    }

                                    if (timeOut < endTime)
                                    {
                                        _attendance.IsEarlyOut = true;
                                    }

                                    _attendancesRepository.UpdateAttendance(_attendance); //Updated attendance record
                                    Application.Current.Dispatcher.Invoke(delegate
                                    {
                                        _attendanceLogs.Insert(0, new AttendanceLog(_student.ImageData, _student.FirstName, _student.LastName, "logged out.", _attendance.TimeOut)); //Add action to attendance log

                                    });

                                    ProcessSMSIntegration(_attendance.AttendanceID.ToString(), false, (DateTime)_attendance.TimeOut, _student);
                                    Remarks = "Student logged out.";
                                }
                                else
                                {
                                    Remarks = "Student is not allowed to logout 1 hour after logging in.";
                                }
                            }
                            else
                            {
                                Remarks = "Student is not allowed to logout twice a day.";
                            }

                        }
                        else
                        {
                            DateTime login = DateTime.Now;
                            login = new DateTime(login.Year, login.Month, login.Day, login.Hour, login.Minute, 0);
                            //Create new attendance record for the student.
                            _attendance = new Attendance
                            {
                                AttendanceID = Guid.NewGuid(),
                                StudentID = _student.StudentGuid,
                                TimeIn = login
                            };

                            TimeSpan timeIn = _attendance.TimeIn.TimeOfDay;
                            TimeSpan startTime = _student.Section.StartTime;

                            if (timeIn > startTime)
                                _attendance.IsLate = true;
                            else
                                _attendance.IsLate = false;

                            _attendancesRepository.AddAttendance(_attendance); //Add Record
                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                _attendanceLogs.Insert(0, new AttendanceLog(_student.ImageData, _student.FirstName, _student.LastName, "logged in.", _attendance.TimeIn));
                            });

                            ProcessSMSIntegration(_attendance.AttendanceID.ToString(), true, _attendance.TimeIn, _student);
                            Remarks = "Student logged in.";
                        }

                        return true;
                    }
                    else
                    {
                        Remarks = "Fingerprint in system but not linked to any student.";
                        break;
                    }
                }
            }

            return false;
        }

        private void Initialize(bool isToReloadBiometrics)
        {
            _student = new Data.Student();
            _biometric = new Biometric();
            _relBiometric = new RelBiometric();
            _attendance = new Attendance();

            _IsFingerEnrolled = false;
            _Verificator = new DPFP.Verification.Verification(); // Create a fingerprint template verificator

            if (isToReloadBiometrics)
            {
                _Biometrics = _biometricsRepository.GetBiometrics(); //Load all FingerPrintTemplate (fpt);
                Remarks = (_Biometrics.Count == 0) ? "No fingerprint template available in our records." : "";
            }

            Student.ImageData = null;

            if (smsTimer == null)
            {
                smsTimer = new Timer(1000 * 60 * 60);
                smsTimer.Elapsed += new ElapsedEventHandler(OnSMSTimerEvent);
                smsTimer.Start();
            }
        }

        public void SwitchOff()
        {
            Stop();
        }

        public void SwitchOn()
        {
            Initialize(true);
            Start();
        }

        private void ProcessSMSIntegration(string attendanceID, bool isTimeIn, DateTime time, Data.Student student)
        {
            foreach (Contact contact in student.Contacts)
            {
                string text = String.Format("STJOHNBCS Messaging:\nPlease be informed that {0} {1} St. John the Baptist Catholic School at {2:h:mm tt}.",
                    student.FirstName, isTimeIn ? "ENTERED" : "EXITED", time);

                SMSUtility.SendSMS(text, contact.ContactNumber, attendanceID, isTimeIn, null, error =>
                {
                    SMSStatus = "Failed to send SMS.";
                    SMSStatusVisibility = Visibility.Visible;
                });
            }
        }

        private void OnResend()
        {
            SMSStatusVisibility = Visibility.Collapsed;
            List<Attendance> attendanceList = _attendancesRepository.GetAttendancesWithFailedSMSRecord();

            if (attendanceList == null || attendanceList.Count == 0)
            {
                return;
            }

            foreach (Attendance attendance in attendanceList)
            {
                Data.Student student = _studentsRepository.GetStudent(attendance.StudentID);
                if (String.IsNullOrEmpty(attendance.TimeInSMSID) || String.IsNullOrEmpty(attendance.TimeInSMSStatus))
                {
                    ProcessSMSIntegration(attendance.AttendanceID.ToString(), true, attendance.TimeIn, student);
                }

                if (attendance.TimeOut.HasValue && (String.IsNullOrEmpty(attendance.TimeOutSMSID) || String.IsNullOrEmpty(attendance.TimeOutSMSStatus)))
                {
                    ProcessSMSIntegration(attendance.AttendanceID.ToString(), false, (DateTime)attendance.TimeOut, student);
                }
            }
        }

        private void OnSMSTimerEvent(object source, ElapsedEventArgs e)
        {
            List<Attendance> attendanceList = _attendancesRepository.GetAttendancesWithFailedSMSRecord();
            if (attendanceList == null || attendanceList.Count == 0)
            {
                return;
            }
            else
            {
                SMSStatus = "Failed to send SMS.";
                SMSStatusVisibility = Visibility.Visible;
            }
        }
        #endregion
    }
}
