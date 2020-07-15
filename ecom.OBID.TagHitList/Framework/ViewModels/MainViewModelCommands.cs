using ecom.TagHitList.Model;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ecom.TagHitList.Framework
{
    public partial class MainViewModel
    {

        /**
        * Relay Commands
        */
        #region Command Properties
        public RelayCommand StartReadCommand { get; private set; }
        public RelayCommand StopReadCommand { get; private set; }

        public RelayCommand ConnectReaderCommand { get; private set; }
        public RelayCommand DisconnectReaderCommand { get; private set; }

        public RelayCommand ExportCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand ResetCommand { get; set; }
        public RelayCommand ClearTagsCommand { get; set; }

        public RelayCommand RemoveCommand { get; private set; }
        #endregion

        private void InitializeCommands()
        {
            ConnectReaderCommand = new RelayCommand(o => ConnectReaderExecute());
            DisconnectReaderCommand = new RelayCommand(o => DisconnectReaderExecute());
            StartReadCommand = new RelayCommand(o => StartReadExecute(), StartReadCanExecute);
            StopReadCommand = new RelayCommand(o => StopReadExecute(), StopreadCanExecute);

            ExportCommand = new RelayCommand(o => ExportExecute(), ExportCanExecute);
            SaveCommand = new RelayCommand(o => SaveExecute(), SaveCanExecute);
            ResetCommand = new RelayCommand(o => ResetExecute());
            ClearTagsCommand = new RelayCommand(o => ClearTagsExecute(), ClearTagsCanExecute);

            RemoveCommand = new RelayCommand(o => RemoveExecute(), RemoveCanExecute);

            TagReads.CollectionChanged += TagReads_CollectionChanged;

        }

        private void TagReads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RemoveCommand.RaiseCanExecuteChanged();
            ClearTagsCommand.RaiseCanExecuteChanged();
            RaiseSaveExportCanExecuteChanged();
        }

        #region ConnectCommand

        private void ConnectReaderExecute()
        {
            try
            {

                if (ReaderManager.Reader.Connected)
                    throw new Exception("Reader Already Connected");

                bool success = false;

                switch (SelectedConnectionInterface)
                {
                    case "TCP":
                        success = _readerManager.ConnectTCP(_ipAddress, _ipPort);
                        break;
                    case "USB":
                        success = _readerManager.ConnectUSB();
                        break;
                    case "COM":
                        //TODO succes = _readerManager.ConnectCOM(COMPort);
                        break;
                }

                
                Status = (success) ? "Connected Successfully" : "Failed to Connect";
                Connected = success;

                if (Connected)
                    _readerManager.SetReaderTime();

            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }

        #endregion

        #region DisconnectCommand

        private void DisconnectReaderExecute()
        {
            try
            {
                bool success = _readerManager.Disconnect();
                Status = (success) ? "Disconnected" : "Failed to Disconnect";
                Connected = !success;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }
        #endregion;

        #region StartCommand
        private void StartReadExecute()
        {
            //if (Collect)
            //    TagReads.Clear();

            if (Simulate)
            {
                foreach (TagRead tag in TagReads)
                {
                    tag.Expired = TimeSpan.FromMilliseconds(MissingTimer);
                }

                //_cancellationTokenSource = new CancellationTokenSource();
                //Task.Run(() =>
                //            {
                //                while (!_cancellationTokenSource.IsCancellationRequested)
                //                {
                //                    ResetChecker();
                //                }
                //            }, _cancellationTokenSource.Token);
            }


            //try
            //{
            _readerManager.StartBRM()
                .ContinueWith(
                t =>
                {
                    Trace.WriteLine($"ReadThred Exceptions Thrown: {t.Exception.Message}");
                    Status = t.Exception.InnerException.Message;
                    _readerManager.StopBRM();
                    _readerManager.IsRunning = false;
                    RaiseBRMStartStopCanExecuteChanged();
                },
            TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);

            RaiseBRMStartStopCanExecuteChanged();

            //}
            //catch (Exception ex)
            //{
            //    Status = ex.Message;
            //}
        }

        private bool StartReadCanExecute()
        {

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;

            return !_readerManager.IsRunning;

        }
        #endregion

        #region StopCommand

        public void StopReadExecute()
        {
            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();

            try
            {
                _readerManager.StopBRM();
                RaiseBRMStartStopCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }

            if (_timeTagProcessing)
            {
                long total = 0;

                foreach (long ms in _timings)
                {
                    total += ms;
                }
                Trace.WriteLine($"{_timings.Count}: {total / _timings.Count}");
            }
        }

        public bool StopreadCanExecute()
        {

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;

            return _readerManager.IsRunning;

        }


        #endregion

        #region FileCommands

        private void ExportExecute()
        {
            if (ConfirmeOverwrite(WRITEFILE) == MessageBoxResult.Yes)
            {
                _fileReaderWriter.ExportToFile(WRITEFILE, TagReads);
                Status = $"{TagReads.Count} Tags Written to {WRITEFILE}";
            }


        }

        private bool ExportCanExecute()
        {
            return (TagReads.Count > 0);
        }

        private void SaveExecute()
        {
            if (ConfirmeOverwrite(READFILE) == MessageBoxResult.Yes)
            {
                _fileReaderWriter.ExportToFile(READFILE, TagReads);
                Status = $"{TagReads.Count} Tags Written to {READFILE}";
            }
        }

        private bool SaveCanExecute()
        {
            return (TagReads.Count > 0);
        }

        private MessageBoxResult ConfirmeOverwrite(string filename)
        {
            return MessageBox.Show($"Overwirte file '{filename}'?", $"Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
        }

        #endregion

        #region SelectionCommands
        private void RemoveExecute()
        {
            Trace.WriteLine($"Removing selected Tags");
            App.Current.Dispatcher.Invoke(() =>
            {

                foreach (TagRead tag in TagReads.Where(tr => tr.Selected).ToList())
                {
                    TagReads.Remove(tag);
                }
            });

        }

        private bool RemoveCanExecute()
        {
            return true;
        }

        #endregion

        #region OtherCommands

        private void ResetExecute()
        {
            foreach (var tag in TagReads)
            {
                for (int i = 0; i < tag.AntennaNumbers.Length; i++)
                {
                    tag.SetAntenna(i, false);
                    tag.Missing = true;
                }
            }

            for (int i = 0; i < AntennaCounts.Count; i++)
            {
                AntennaCounts[i] = 0;
            }

            Count = 0;
            Duplicates = 0;
        }

        private void ClearTagsExecute()
        {
            TagReads.Clear();
        }

        private bool ClearTagsCanExecute()
        {
            return (TagReads.Count > 0);
        }

        #endregion

        private void RaiseBRMStartStopCanExecuteChanged()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StartReadCommand.RaiseCanExecuteChanged();
                StopReadCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(IsRunning));
                RaisePropertyChanged(nameof(IsNotRunning));
            });

        }

        private void RaiseClearTagsCanExecuteChanged()
        {
            ClearTagsCommand.RaiseCanExecuteChanged();
        }

        private void RaiseSaveExportCanExecuteChanged()
        {
            SaveCommand.RaiseCanExecuteChanged();
            ExportCommand.RaiseCanExecuteChanged();
        }

    }
}
