using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeLightWPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private int count;
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            (DataContext as MainWindowViewModel)?.CloseVideoPlayback();

        }
        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape && ((DataContext as MainWindowViewModel)?.VideoIsVisible ?? false)) {
                count++;
                if (count == 2) {
                    (DataContext as MainWindowViewModel)?.HideVideoPlayback();
                    count = 0;
                }
            }
            else
                count = 0;
        }
    }
}
