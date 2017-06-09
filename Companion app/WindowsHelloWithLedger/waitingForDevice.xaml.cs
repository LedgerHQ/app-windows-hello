using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsHelloWithLedger
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class waitingForDevice : Page
    {
        public waitingForDevice()
        {
            this.InitializeComponent();
            waitingForDeviceInsertion();
        }
        private async void waitingForDeviceInsertion()
        {
            bool nanosDetected = false;
            string selector = SmartCardReader.GetDeviceSelector();
            selector += " AND System.Devices.DeviceInstanceId:~~\"Ledger\"";
            //string test = selector.Replace(" ", ((char)34).ToString());            

            while (!nanosDetected)
            {
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);
                if (devices.Count > 0)
                {
                    nanosDetected = true;
                    await Task.Delay(1000);
                    this.Frame.Navigate(typeof(RegisterDevice));
                }
            }
        }

        private void Assistance_pointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);

            if (sender is StackPanel)
            {
                ((StackPanel)((Grid)((StackPanel)sender).Parent).Children.ElementAt(2)).Visibility = Visibility.Visible;
            }
            else
            {

            }
            //((Image)((Grid)((Image)e.OriginalSource).Parent).Children.ElementAt(2)).Visibility = Visibility.Visible;
            //((Image)e.OriginalSource).Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Button-assistance-select.png"));
            e.Handled = true;
        }
        private void Assistance_pointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);

            if (sender is StackPanel)
            {
                ((StackPanel)((Grid)((StackPanel)sender).Parent).Children.ElementAt(2)).Visibility = Visibility.Collapsed;
            }
            else
            {

            }
            //((Image)((Grid)e.OriginalSource).Children.ElementAt(1)).Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Button-assistance.png"));
            e.Handled = true;
        }

        private async void Assistance_Click(object sender, TappedRoutedEventArgs e)
        {
            string uriToLaunch = @"http://www.ledgerwallet.com";
            var uri = new Uri(uriToLaunch);
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
