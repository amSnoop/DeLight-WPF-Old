using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;

namespace DeLightNew {
    public partial class MainWindow : Window {

        private int count;
        public MainWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new MainWindowViewModel(this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            (DataContext as MainWindowViewModel)?.CloseVideoPlayback();

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape && ((DataContext as MainWindowViewModel)?.VideoIsVisible ?? false))
            {
                count++;
                if (count == 2)
                {
                    (DataContext as MainWindowViewModel)?.HideVideoPlayback();
                    count = 0;
                }
            }
            else
                count = 0;
        }

    }
}
