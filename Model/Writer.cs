using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wrench.Model;

internal class Writer : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly ObservableCollection<string> _kuLogList;

    private bool? _isWriterRunning;
    public bool? IsWriterRunning { get => _isWriterRunning; set => ChangeProperty(ref _isWriterRunning, value); }

    private string? _status = null;
    public string? OperationStatus { get => _status; set => ChangeProperty(ref _status, value, nameof(OperationStatus)); }

    private string? _passwordText = null;
    public string? PasswordText { get => _passwordText; set => ChangeProperty(ref _passwordText, value, nameof(PasswordText)); }

    public Writer(ObservableCollection<string> kULogList) => _kuLogList = kULogList;

    private bool ChangeProperty<T>(ref T property, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(property, value))
        {
            property = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        return false;
    }

    public void Run()
    {
        IsWriterRunning = !(IsWriterRunning ?? false);
        Task.Factory.StartNew(() => LogMsg("message"));
    }

    public void LogMsg(string? message) => _kuLogList.Insert(0, message ?? string.Empty);
}
