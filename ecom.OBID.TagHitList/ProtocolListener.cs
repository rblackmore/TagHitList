using ecom.TagHitList.Framework.ViewModels;
using OBID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecom.TagHitList
{
    public class ProtocolListener : FeIscListener
    {

        IProtocolListenerPresenter _presenter;

        public ProtocolListener(IProtocolListenerPresenter presenter)
        {
            _presenter = presenter;
        }

        public void OnReceiveProtocol(FedmIscReader reader, byte[] receiveProtocol)
        {
            throw new NotImplementedException();
        }

        public void OnReceiveProtocol(FedmIscReader reader, string receiveProtocol)
        {
            _presenter.OnNewProtocolStringTransceived(receiveProtocol);
        }

        public void OnSendProtocol(FedmIscReader reader, byte[] sendProtocol)
        {
            throw new NotImplementedException();
        }

        public void OnSendProtocol(FedmIscReader reader, string sendProtocol)
        {
            _presenter.OnNewProtocolStringTransceived(sendProtocol);
        }
    }
}
