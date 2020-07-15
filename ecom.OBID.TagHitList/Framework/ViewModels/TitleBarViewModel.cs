using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ecom.TagHitList.Framework.ViewModels
{
    public class TitleBarViewModel : ViewModelBase
    {
        public ICommand CloseCommand { get; private set; }
        public ICommand MaximizeCommand { get; private set; }
        public ICommand MinimizeCommand { get; private set; }

        public TitleBarViewModel()
        {
            CloseCommand = new RelayCommand(w => CloseWindow((Window)w));
            MaximizeCommand = new RelayCommand(w => MaximizeWindow((Window)w));
            MinimizeCommand = new RelayCommand(w => MinimizeWindow((Window)w));
        }

        private void CloseWindow(Window window)
        {
            window.Close();
        }

        private void MaximizeWindow(Window window)
        {
            window.WindowState ^= WindowState.Maximized;
        }

        private void MinimizeWindow(Window window)
        {
            window.WindowState = WindowState.Minimized;
        }
    }
}
