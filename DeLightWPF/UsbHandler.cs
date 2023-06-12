using System;
using System.Linq;
using LibUsbDotNet;
using LibUsbDotNet.Main;


namespace DeLightWPF
{
    public class UsbHandler
    {
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(1234, 5678); // replace with your VID and PID

        public static void Main(string[] args)
        {
            // Find and open the USB device.
            UsbDevice usbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
            if (!usbDevice.IsOpen)
            {
                Console.WriteLine("Device not found.");
                return;
            }

            // Use the device.
            IUsbDevice? wholeUsbDevice = usbDevice as IUsbDevice;
            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                // Select config #1
                wholeUsbDevice.SetConfiguration(1);

                // Claim interface #0.
                wholeUsbDevice.ClaimInterface(0);
            }

            // Send data
            byte[] writeBuffer = new byte[1] { 0x00 }; // replace with your data
            int bytesWritten;
            usbDevice.ControlTransfer(UsbCtrlFlags.RequestType_Vendor, 0x01, 0x02, 0x0000, writeBuffer, writeBuffer.Length, out bytesWritten);

            // Read data
            byte[] readBuffer = new byte[1024];
            int bytesRead;
            usbDevice.ControlTransfer(UsbCtrlFlags.RequestType_Vendor | UsbCtrlFlags.Direction_In, 0x01, 0x02, 0x0000, readBuffer, readBuffer.Length, out bytesRead);

            // Close the device.
            usbDevice.Close();
        }
    }
}
