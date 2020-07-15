using ecom.TagHitList.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ecom.TagHitList.Framework
{
    public partial class MainViewModel : ViewModelBase
    {
        private bool _timeTagProcessing = false;
        private Stopwatch _stopWatch = new Stopwatch();
        private IList<long> _timings = new List<long>();

        /**
         * Constants and Private Attributes
         */
        #region Private Attributes and Constants
        private string READFILE = AppDomain.CurrentDomain.BaseDirectory + "\\tags.csv";
        private string WRITEFILE = AppDomain.CurrentDomain.BaseDirectory + "\\output.csv";

        private ReaderManager _readerManager;
        private FileReaderWriter _fileReaderWriter;

        private string _selectedConnectionInterface;
        private string _statusMessage;
        private string _ipAddress;
        private string _title;
        private int _ipPort;
        private int _count;
        private int _duplicates;
        private int _missingTimer; //Missing timer in milliseconds
        private bool _collect;
        private bool _simulate;
        private bool _connected;

        #endregion

        /**
         * Public Properties
         */
        #region Public Properties
        public ObservableCollection<string> ConnectionInterfaces { get; set; } = new ObservableCollection<string>(new string[] { "TCP", "COM", "USB" });
        public ObservableCollection<TagRead> TagReads { get; set; }
        public ObservableCollection<int> AntennaCounts { get; set; }

        public string SelectedConnectionInterface
        {
            get => _selectedConnectionInterface;
            set { if (_selectedConnectionInterface == value) return; _selectedConnectionInterface = value; RaisePropertyChanged(); }
        }

        public string Status
        {
            get => _statusMessage;
            set { string newValue = value.Replace("\r\n", "").Replace("\r", "").Replace("\n", ""); if (_statusMessage == newValue) return; _statusMessage = newValue; RaisePropertyChanged(); }
        }

        public string IPAddress
        {
            get => _ipAddress;
            set { if (_ipAddress == value) return; _ipAddress = value; RaisePropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { if (_title == value) return; _title = value; RaisePropertyChanged(); }
        }

        public int IpPort
        {
            get => _ipPort;
            set { if (_ipPort == value) return; _ipPort = value; RaisePropertyChanged(); }
        }

        public int Count
        {
            get => _count;
            set { if (_count == value) return; _count = value; RaisePropertyChanged(); }
        }

        public int Duplicates
        {
            get => _duplicates;
            set { if (_duplicates == value) return; _duplicates = value; RaisePropertyChanged(); }
        }

        public int MissingTimer
        {
            get => _missingTimer;
            set { if (_missingTimer == value) return; _missingTimer = value; RaisePropertyChanged(); }
        }

        public bool Collect
        {
            get => _collect;
            set { if (_collect == value) return; _collect = value; RaisePropertyChanged(); }
        }

        public bool Simulate
        {
            get => _simulate;
            set { if (_simulate == value) return; _simulate = value; RaisePropertyChanged(); }
        }

        public bool Connected
        {
            get => _connected;
            private set { if (_connected == value) return; _connected = value; RaisePropertyChanged(); }
        }

        public bool NotConnected
        {
            get => !Connected;
        }

        public bool IsRunning { get { return _readerManager.IsRunning; } }
        public bool IsNotRunning { get { return !IsRunning; } }
        #endregion


        public MainViewModel()
        {
            Title = $"Tag Hit List (version: {Assembly.GetExecutingAssembly().GetName().Version.ToString()})";

            MissingTimer = 2000;

            AntennaCounts = new ObservableCollection<int>(new int[8]);

            SelectedConnectionInterface = "TCP";


            _fileReaderWriter = new FileReaderWriter();


            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                SetDesignDate();
            }
            else
            {
                _readerManager = new ReaderManager();
                _readerManager.TagReadReceived += TagReadReceivedHandler;
                SetMainData();
            }

            InitializeCommands();

        }

        private void SetDesignDate()
        {
            TagReads = new ObservableCollection<TagRead>();

            Random rando = new Random();

            IPAddress = "192.168.10.10";
            IpPort = 10001;

            for (int i = 0; i < 5; i++)
            {
                var tagRead = new TagRead();
                tagRead.SerialNumber = $"SN {rando.Next(100000, 999999).ToString()}";

                for (int j = 0; j < tagRead.AntennaNumbers.Count(); j++)
                {
                    tagRead.SetAntenna(j, (rando.Next(0, 1) == 1 ? true : false));
                }
            }
        }

        private void SetMainData()
        {
            IPAddress = "192.168.10.10";
            IpPort = 10001;
            Collect = false;
            TagReads = new ObservableCollection<TagRead>(_fileReaderWriter.LoadFromFile(READFILE).OrderBy(tr => tr.SerialNumber));
        }

        CancellationTokenSource _cancellationTokenSource;

        private void ResetChecker()
        {
            foreach (TagRead tag in TagReads.Where(tr => !tr.Missing))
            {
                tag.CheckAntennaTimer();
            }
        }

        private void TagReadReceivedHandler(object sender, TagReadEventArgs e)
        {
            if (_timeTagProcessing)
                _stopWatch.Restart();


            if (Collect)
            {
                AddTagsCallBack callBack = new AddTagsCallBack(AddTagsToList);

                App.Current.Dispatcher.BeginInvoke(callBack, DispatcherPriority.Normal, e.Tags);
            }
            else if (Simulate)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (TagRead readTag in e.Tags)
                    {
                        TagRead existingTag = TagReads.Where(t => t.SerialNumber == readTag.SerialNumber).FirstOrDefault();

                        if (existingTag != null)
                        {
                            existingTag.ResetTimer();
                            existingTag.LastRead = readTag.LastRead;

                            for (int i = 0; i < existingTag.AntennaNumbers.Length; i++)
                            {
                                if (!existingTag.AntennaNumbers[i])
                                    existingTag.SetAntenna(i, readTag.AntennaNumbers[i]);
                            }

                            for (int i = 0; i < existingTag.LastReadAntenna.Length; i++)
                            {
                                if (readTag.LastReadAntenna[i] != default(DateTime))
                                    existingTag.LastReadAntenna[i] = readTag.LastReadAntenna[i];
                            }
                        }

                        Task.Run(() => Calculate());
                    }

                });
            }
            else
            {
                App.Current.Dispatcher.Invoke(() =>
                   {

                       foreach (TagRead readTag in e.Tags)
                       {
                           //Get Tag From List is Serial Number Exists
                           TagRead existingTag = TagReads.Where(t => t.SerialNumber == readTag.SerialNumber).FirstOrDefault();

                           if (existingTag != null)
                           {
                               existingTag.LastRead = readTag.LastRead;
                               //Loop Through all Antennas on Existing Tag
                               for (int i = 0; i < existingTag.AntennaNumbers.Length; i++)
                               {

                                   /**
                                    * This could break if both tags have differet length antennaNumbe arrays, 
                                    * however since we set thelength in the contstructor, 
                                    * this should never be the case, 
                                    * if it is i've really fucked up somewhere.
                                    */
                                   if (!existingTag.AntennaNumbers[i]) // If this antenna hasn't already been set to true;
                                   {
                                       /**
                                        * Set Antenna as per current read state. True if Read on this antenna, False if Not.
                                        * If Above if statement shows it's already been read on this antenna, then we do not set it again, as this could cause it to be reset to false.
                                        */
                                       existingTag.SetAntenna(i, readTag.AntennaNumbers[i]);
                                   }
                               }
                               Calculate();
                           }
                       }

                   });
            }

            if (_timeTagProcessing)
            {
                _stopWatch.Stop();
                _timings.Add(_stopWatch.ElapsedTicks);
                Trace.WriteLine($"{_timings.Count}: {_stopWatch.Elapsed}");
            }


        }

        private delegate void AddTagsCallBack(IEnumerable<TagRead> tags);

        private void AddTagsToList(IEnumerable<TagRead> tags)
        {
            Trace.WriteLine($"Adding {tags.Count()} Tags");
            foreach (TagRead Read in tags)
            {
                if (!TagReads.ContainsSerial(Read.SerialNumber))
                {
                    TagReads.Add(new TagRead() { SerialNumber = Read.SerialNumber, LastRead = Read.LastRead });
                    Count++;
                    Trace.WriteLine($"New Tag Added {Read.SerialNumber}");
                }


            }
        }

        private void Calculate()
        {
            Count = TagReads.Where(
                tr =>
                {
                    for (int i = 0; i < tr.AntennaNumbers.Length; i++)
                    {
                        if (tr.AntennaNumbers[i])
                            return true;
                    }
                    return false;
                }).Count();

            Duplicates = TagReads.Where(
                    tr =>
                    {
                        int count = 0;

                        for (int i = 0; i < tr.AntennaNumbers.Length; i++)
                        {
                            if (tr.AntennaNumbers[i])
                                count++;

                            if (count > 1)
                                return true;
                        }
                        return false;
                    }).Count();

            for (int i = 0; i < 8; i++)
            {
                AntennaCounts[i] = TagReads.Where(
                    tr =>
                    {
                        return tr.AntennaNumbers[i];

                    }).Count();
            }
        }
    }
}
