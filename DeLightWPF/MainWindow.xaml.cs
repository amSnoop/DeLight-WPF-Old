using LibUsbDotNet.DeviceNotify;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeLightWPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private int count;

        private bool isDragging;

        private IDeviceNotifier usbDeviceNotifier;

        private readonly int vendor_id = 0x1069;
        private readonly int product_id = 0x1040;

        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);

            usbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();
            usbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;
        }

        private void OnDeviceNotifyEvent(object? sender, DeviceNotifyEventArgs e) {
            if (e.Device.IdVendor == vendor_id && e.Device.IdProduct == product_id) {
                if (e.EventType == EventType.DeviceArrival) {
                    Console.WriteLine("Device connected.");
                    // handle connection event
                }
                else if (e.EventType == EventType.DeviceRemoveComplete) {
                    Console.WriteLine("Device disconnected.");
                    // handle disconnection event
                }
            }
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
            if(value < slider.Minimum)
                value = slider.Minimum;
            else if(value > slider.Maximum)
                value = slider.Maximum;
            slider.Value = value;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
