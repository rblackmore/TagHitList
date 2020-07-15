using ecom.TagHitList.Framework;
using ecom.TagHitList.Model;
using OBID;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ecom.TagHitList
{
    public class ReaderManager
    {
        private bool _timeReadThread = false;
        IList<long> _stopwatchTimers = new List<long>();

        private static FedmIscReader _reader;
        public static FedmIscReader Reader { get => _reader; }

        public ReaderManager()
        {
            _reader = new FedmIscReader();
            _reader.SetTableSize(FedmIscReaderConst.BRM_TABLE, 255);

        }

        #region BRMManagement

        public event EventHandler<TagReadEventArgs> TagReadReceived;

        private CancellationTokenSource _cancellationTokenSource;

        public bool IsRunning { get; set; }

        public async Task StartBRM()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Trace.WriteLine($"Begin Read Thread and Initilize");
            _reader.SendProtocol(0x32);
            _reader.SendProtocol(0x33);
            IsRunning = true;

            await Task.Run(() => ReadThread(), _cancellationTokenSource.Token);
        }

        private void ReadThread()
        {
            int status;
            int ReqSets = 255;
            bool? validSerial = null;
            bool? validDate = null;
            bool? validTime = null;

            _reader.SetData(FedmIscReaderID.FEDM_ISC_TMP_ADV_BRM_SETS, ReqSets);
            Stopwatch stopwatch = new Stopwatch();
            DateTime now = DateTime.Now;
            DateTime timer;
            int day = now.Day;
            int month = now.Month;
            int year = now.Year;
            int hour = 0;
            int minute = 0;
            int second = 0;
            int milliSecond = 0;
            IList<TagRead> tags = new List<TagRead>();
            FedmBrmTableItem[] brmItems = new FedmBrmTableItem[ReqSets];

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_timeReadThread)
                    stopwatch.Restart();

                //try
                //{

                //Trace.WriteLine("Sending Read Buffer Command");
                status = _reader.SendProtocol(0x22);

                if ((status == 0x00) || (status == 0x83) || (status == 0x84) || (status == 0x85) || (status == 0x93) || (status == 0x94))
                {


                    try
                    {
                        brmItems = (FedmBrmTableItem[])_reader.GetTable(FedmIscReaderConst.BRM_TABLE);
                    }
                    catch (Exception ex)
                    {
                        //return;
                    }


                    tags.Clear();

                    if (brmItems != null)
                    {
                        for (int i = 0; i < brmItems.Length; i++)
                        {
                            TagRead newTag = new TagRead();

                            switch (validSerial)
                            {
                                case null:
                                    validSerial = brmItems[i].IsDataValid(FedmIscReaderConst.DATA_SNR);
                                    if (validSerial == true)
                                        newTag.SerialNumber = GetSerial(brmItems[i]);
                                    break;
                                case true:
                                    newTag.SerialNumber = GetSerial(brmItems[i]);
                                    break;
                                default:
                                    break;
                            }

                            switch (validDate)
                            {
                                case null:
                                    validDate = brmItems[i].IsDataValid(FedmIscReaderConst.DATA_DATE);
                                    if (validDate == true)
                                    {
                                        day = brmItems[i].GetReaderTime().Day;
                                        month = brmItems[i].GetReaderTime().Month;
                                        year = brmItems[i].GetReaderTime().Year;
                                    }
                                    break;
                                case true:
                                    day = brmItems[i].GetReaderTime().Day;
                                    month = brmItems[i].GetReaderTime().Month;
                                    year = brmItems[i].GetReaderTime().Year;
                                    break;
                                default:
                                    day = now.Day;
                                    month = now.Month;
                                    year = now.Year;
                                    break;
                            }

                            switch (validTime)
                            {
                                case null:
                                    validTime = brmItems[i].IsDataValid(FedmIscReaderConst.DATA_TIMER);
                                    if (validTime == true)
                                    {
                                        hour = brmItems[i].GetReaderTime().Hour;
                                        minute = brmItems[i].GetReaderTime().Minute;
                                        milliSecond = brmItems[i].GetReaderTime().MilliSecond;
                                        second = (milliSecond - (milliSecond % 1000)) / 1000;
                                        milliSecond = milliSecond % 1000;
                                    }
                                    break;
                                case true:
                                    hour = brmItems[i].GetReaderTime().Hour;
                                    minute = brmItems[i].GetReaderTime().Minute;
                                    milliSecond = brmItems[i].GetReaderTime().MilliSecond;
                                    second = (milliSecond - (milliSecond % 1000)) / 1000;
                                    milliSecond = milliSecond % 1000;
                                    break;
                                default:
                                    hour = now.Hour;
                                    minute = now.Minute;
                                    second = now.Second;
                                    milliSecond = now.Millisecond;
                                    break;
                            }

                            timer = new DateTime(year, month, day, hour, minute, second, milliSecond);
                            newTag.LastRead = timer;

                            if (brmItems[i].IsDataValid(FedmIscReaderConst.DATA_ANT_RSSI))
                            {
                                foreach (var item in brmItems[i].GetRSSI().Values)
                                {
                                    Trace.WriteLine($"Antenna Number: {item.antennaNumber}");
                                    newTag.AntennaNumbers[item.antennaNumber - 1] = true;
                                    newTag.LastReadAntenna[item.antennaNumber - 1] = timer;
                                }
                            }

                            tags.Add(newTag);
                        }
                    }
                    Task.Run(() =>
                    {
                        var handler = TagReadReceived;
                        handler?.Invoke(this, new TagReadEventArgs(tags));
                    });


                    //try
                    //{
                    _reader.SendProtocol(0x32);

                    //}
                    //catch (Exception ex)
                    //{
                    //    return;
                    //}

                }

                //}
                //catch (FePortDriverException ex)
                //{
                //    _cancellationTokenSource.Cancel();
                //    Disconnect();
                //    MessageBox.Show(ex.Message);
                //}
                if (_timeReadThread)
                {
                    stopwatch.Stop();
                    _stopwatchTimers.Add(stopwatch.ElapsedTicks);
                    if (_stopwatchTimers.Count == 5000)
                        _cancellationTokenSource.Cancel();
                    Trace.WriteLine($"{_stopwatchTimers.Count}: {stopwatch.Elapsed}");
                }


            }

            if (_timeReadThread)
            {
                long total = 0;

                foreach (long ms in _stopwatchTimers)
                {
                    total += ms;
                }
                Trace.WriteLine($"Samples: {_stopwatchTimers.Count}");
                Trace.WriteLine($"Average {total / _stopwatchTimers.Count}");
            }

        }

        private string GetSerial(FedmBrmTableItem brmItem)
        {
            brmItem.GetData(FedmIscReaderConst.DATA_SNR, out string serialNumber);
            return serialNumber;

        }

        public void StopBRM()
        {
            _cancellationTokenSource.Cancel();
            IsRunning = false;
        }

        #endregion

        #region ConnectionMethods
        public bool ConnectTCP(string ipAddress, int port)
        {
            if (!_reader.Connected)
                _reader.ConnectTCP(ipAddress, port);
            else
                return false;

            return _reader.Connected;
        }

        public bool ConnectUSB()
        {
            if (!_reader.Connected)
                _reader.ConnectUSB(0);
            else
                return false;

            return _reader.Connected;
        }

        public bool ConnectCOMM(int port)
        {
            if (!_reader.Connected)
                _reader.ConnectCOMM(port, true);
            else
                return false;

            return _reader.Connected;
        }

        public bool Disconnect()
        {

            _reader.DisConnect();

            return !_reader.Connected;
        }

        #endregion

        public bool SetReaderTime()
        {
            DateTime now = DateTime.Now;
            uint hour = (uint)now.Hour;
            uint minute = (uint)now.Minute;
            uint millisecond = (uint)((now.Second * 1000) + now.Millisecond);

            _reader.SetData(OBID.ReaderCommand._0x85.Req.TIMER_HOUR, hour);
            _reader.SetData(OBID.ReaderCommand._0x85.Req.TIMER_MINUTE, minute);
            _reader.SetData(OBID.ReaderCommand._0x85.Req.TIMER_MILLISECONDS, millisecond);

            int status = _reader.SendProtocol(0x85);

            if (status == 0)
                return true;

            return false;

        }
    }
}
