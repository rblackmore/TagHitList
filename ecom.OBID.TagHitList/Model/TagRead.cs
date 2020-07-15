using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ecom.TagHitList.Model
{
    public class TagRead : INotifyPropertyChanged, IComparable<TagRead>, IEquatable<TagRead>
    {
        private string _serialNumber;
        private string _description;
        private bool _missing;
        private bool _selected;
        private DateTime _lastRead;
        private Timer _timer;
        private TimeSpan _expired;

        public string SerialNumber { get => _serialNumber; set { if (_serialNumber == value) return; _serialNumber = value; RaisePropertyChanged(); } }
        public string Description { get => _description; set { if (_description == value) return; _description = value; RaisePropertyChanged(); } }
        public bool Selected { get => _selected; set { if (_selected == value) return; _selected = value; RaisePropertyChanged(); } }
        public bool[] AntennaNumbers { get; set; }

        public bool Antenna1 { get => AntennaNumbers[0]; private set { if (AntennaNumbers[0] == value) return; AntennaNumbers[0] = value; RaisePropertyChanged(); } }
        public bool Antenna2 { get => AntennaNumbers[1]; private set { if (AntennaNumbers[1] == value) return; AntennaNumbers[1] = value; RaisePropertyChanged(); } }
        public bool Antenna3 { get => AntennaNumbers[2]; private set { if (AntennaNumbers[2] == value) return; AntennaNumbers[2] = value; RaisePropertyChanged(); } }
        public bool Antenna4 { get => AntennaNumbers[3]; private set { if (AntennaNumbers[3] == value) return; AntennaNumbers[3] = value; RaisePropertyChanged(); } }
        public bool Antenna5 { get => AntennaNumbers[4]; private set { if (AntennaNumbers[4] == value) return; AntennaNumbers[4] = value; RaisePropertyChanged(); } }
        public bool Antenna6 { get => AntennaNumbers[5]; private set { if (AntennaNumbers[5] == value) return; AntennaNumbers[5] = value; RaisePropertyChanged(); } }
        public bool Antenna7 { get => AntennaNumbers[6]; private set { if (AntennaNumbers[6] == value) return; AntennaNumbers[6] = value; RaisePropertyChanged(); } }
        public bool Antenna8 { get => AntennaNumbers[7]; private set { if (AntennaNumbers[7] == value) return; AntennaNumbers[7] = value; RaisePropertyChanged(); } }

        public bool Missing { get => _missing; set { if (_missing == value) return; _missing = value; RaisePropertyChanged(); } }

        public DateTime LastRead { get => _lastRead; set { if (_lastRead == value) return; _lastRead = value; RaisePropertyChanged(); } }
        public DateTime[] LastReadAntenna { get; set; }

        public TimeSpan Expired { get => _expired; set { if (_expired == value) return; _expired = value; RaisePropertyChanged(); _timer.Interval = _expired.TotalMilliseconds; } }

        public TagRead()
        {
            AntennaNumbers = new bool[8];
            LastReadAntenna = new DateTime[8];
            Missing = true;
            _timer = new Timer(500);
            _timer.AutoReset = true;
            _timer.Elapsed += (s, e) => ResetAntennas();
        }

        public void ResetTimer()
        {
            _timer.Stop();
            _timer.Start();
        }

        public void ResetAntennas()
        {
            for (int i = 0; i < AntennaNumbers.Length; i++)
            {
                AntennaNumbers[i] = false;
            }

            Missing = true;

            RaisePropertyChanged(nameof(Antenna1));
            RaisePropertyChanged(nameof(Antenna2));
            RaisePropertyChanged(nameof(Antenna3));
            RaisePropertyChanged(nameof(Antenna4));
            RaisePropertyChanged(nameof(Antenna5));
            RaisePropertyChanged(nameof(Antenna6));
            RaisePropertyChanged(nameof(Antenna7));
            RaisePropertyChanged(nameof(Antenna8));
        }

        public void CheckAntennaTimer()
        {
            var now = DateTime.Now;
            var readGap = now - LastRead;

            for (int i = 0; i < LastReadAntenna.Length; i++)
            {
                var gap = (now - LastReadAntenna[i]);
                if (gap > Expired)
                    SetAntenna(i, false);
            }

            if (readGap > Expired)
                ResetAntennas();

        }

        public void SetAntenna(int index, bool isread)
        {
            //AntennaNumbers[index] = isread;
            //RaisePropertyChanged(nameof(AntennaNumbers));
            if (isread)
                Missing = false;


            switch (index)
            {
                case 0:
                    Antenna1 = isread;
                    break;
                case 1:
                    Antenna2 = isread;
                    break;
                case 2:
                    Antenna3 = isread;
                    break;
                case 3:
                    Antenna4 = isread;
                    break;
                case 4:
                    Antenna5 = isread;
                    break;
                case 5:
                    Antenna6 = isread;
                    break;
                case 6:
                    Antenna7 = isread;
                    break;
                case 7:
                    Antenna8 = isread;
                    break;
                default:
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(TagRead other)
        {
            for (int i = 0; i < AntennaNumbers.Length; i++)
            {
                if (other.AntennaNumbers[i] == AntennaNumbers[i])
                    return 0;
                if (other.AntennaNumbers[i])
                    return -1;
            }

            return 1;

        }

        public bool Equals(TagRead other)
        {
            if (SerialNumber != other.SerialNumber)
                return false;

            for (int i = 0; i < AntennaNumbers.Length; i++)
            {
                if (AntennaNumbers[i] != other.AntennaNumbers[i])
                    return false;
            }

            return true;
        }
    }
}
