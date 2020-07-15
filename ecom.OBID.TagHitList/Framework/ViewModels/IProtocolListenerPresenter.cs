namespace ecom.TagHitList.Framework.ViewModels
{
    public interface IProtocolListenerPresenter
    {
        void OnNewProtocolStringTransceived(string p);
    }
}