using OBID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecom.TagHitList.Framework.ViewModels
{
    public class ProtocolViewModel : ViewModelBase, IProtocolListenerPresenter
    {
        private string _protocol;
        private StringBuilder _fullProtocolBuilder;

        public string Protocol { get => _fullProtocolBuilder.ToString(); }
        public List<string> ProtocolList { get; private set; }

        public RelayCommand ClearProtocolCommand { get; set; }

        public ProtocolViewModel()
        {
            ClearProtocolCommand = new RelayCommand(o => ClearProtocolExecute());
            ProtocolList = new List<string>();
            _fullProtocolBuilder = new StringBuilder();
            //ReaderManager.Reader.AddEventListener(new ProtocolListener(this), FeIscListenerConst.RECEIVE_STRING_EVENT);
            //ReaderManager.Reader.AddEventListener(new ProtocolListener(this), FeIscListenerConst.SEND_STRING_EVENT);
        }

        private void ClearProtocolExecute()
        {
            ProtocolList.Clear();
            RaisePropertyChanged(nameof(Protocol));
        }

        public void OnNewProtocolStringTransceived(string p)
        {
            ProtocolList.Add(p.Replace("\r\n", ""));
            _fullProtocolBuilder.Append(p);
            RaisePropertyChanged(nameof(Protocol));
        }
    }
}
