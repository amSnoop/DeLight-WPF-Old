using DeLightWPF.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeLightWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int count;

        private bool isDragging;

        public MainWindow()
        {
            WindowState = WindowState.Normal;
            Top = GlobalSettings.Instance.LastScreenTop;
            Left = GlobalSettings.Instance.LastScreenLeft;
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);

            PreviewKeyDown += OnKeyDown;
            SizeChanged += MainWindow_OnSizeChanged;
            MouseDown += MainWindow_MouseDown;
            Loaded += MainWindow_Loaded;
        }

        public void MainWindow_Loaded(object? sender, EventArgs e)
        {
            WindowState = GlobalSettings.Instance.WindowState;
        }
        public void MainWindow_MouseDown(object sender, MouseButtonEventArgs e) {
            Keyboard.ClearFocus();
            Activate();
            Focus();
        }

        protected override void OnClosed(EventArgs e) {
            GlobalSettings.Instance.LastScreenTop = (int)Top;
            GlobalSettings.Instance.LastScreenLeft = (int)Left;
            GlobalSettings.Instance.WindowState = WindowState;
            base.OnClosed(e);
            (DataContext as MainWindowViewModel)?.HideVideoPlayback();

        }
        protected void OnKeyDown(object? sender, KeyEventArgs e)
        
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape && ((DataContext as MainWindowViewModel)?.VideoIsVisible ?? false) && Keyboard.FocusedElement is not TextBox)
            {
                count++;
                if (count == 2)
                {
                     (DataContext as MainWindowViewModel)?.HideVideoPlayback();
                    count = 0;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Space && DataContext is MainWindowViewModel vm && Keyboard.FocusedElement is not TextBox)
            {
                vm.PlayNextCue();
                e.Handled = true;
            }
            else
                count = 0;
        }

        #region Improved Slider Behavior

        private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var slider = (Slider)sender;
            UpdateSliderValue(slider, e);
            slider.CaptureMouse();
        }

        private void Slider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var slider = (Slider)sender;
                UpdateSliderValue(slider, e);
            }
        }
        private void Slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var slider = (Slider)sender;
            UpdateSliderValue(slider, e);
            slider.ReleaseMouseCapture();
        }

        private void UpdateSliderValue(Slider slider, MouseEventArgs e)
        {
            var point = e.GetPosition(slider);
            var ratio = point.X / slider.ActualWidth;
            var value = ratio * (slider.Maximum - slider.Minimum) + slider.Minimum;
            if (value < slider.Minimum)
                value = slider.Minimum;
            else if (value > slider.Maximum)
                value = slider.Maximum;
            slider.Value = value;
        }
        #endregion

        private void Play_Button_Clicked(object sender, RoutedEventArgs e) {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.Play();
        }
        private void Stop_Button_Clicked(object sender, RoutedEventArgs e) {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.Stop();
        }

        private void MainWindow_OnSizeChanged(object? sender, SizeChangedEventArgs e) {
            if (DataContext is MainWindowViewModel vm) {
                vm.UpdateWindowSize();
            }
        }
    }
}
