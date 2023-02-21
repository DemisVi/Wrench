using System;
using System.ComponentModel;
using System.Windows.Media;

namespace Wrench.Model
{
    internal interface IWriter
    {
        string ContactUnitTitle { get; set; }
        string DeviceSerial { get; set; }
        int FailValue { get; set; }
        bool IsWriterRunning { get; set; }
        string OperationStatus { get; set; }
        int PassValue { get; set; }
        bool ProgressIndeterminate { get; set; }
        int ProgressValue { get; set; }
        Brush StatusColor { get; set; }
        TimeSpan TimeAvgValue { get; set; }
        string WorkingDir { get; set; }

        event PropertyChangedEventHandler? PropertyChanged;

        void LogMsg(string? message);
        void Start();
        void Stop();
    }
}