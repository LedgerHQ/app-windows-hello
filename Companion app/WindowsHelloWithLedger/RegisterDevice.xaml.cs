using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class RegisterDevice : Page
    {
        public RegisterDevice()
        {
            this.InitializeComponent();

            //string text = NameYourDevice.Text;
        }

        private void Assistance_pointerEntered(object sender, PointerRoutedEventArgs e)
        {
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

        private void StackCancel_pointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is StackPanel)
            {
                ((StackPanel)sender).Children.ElementAt(1).Visibility = Visibility.Visible;
            }
            else
            {

            }
        }

        private void StackCancel_pointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is StackPanel)
            {
                ((StackPanel)sender).Children.ElementAt(1).Visibility = Visibility.Collapsed;
            }
            else
            {

            }
        }

        private void StackRegister_pointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is StackPanel)
            {
                ((StackPanel)sender).Children.ElementAt(1).Visibility = Visibility.Visible;
            }
            else
            {

            }
        }

        private void StackRegsiter_pointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is StackPanel)
            {
                ((StackPanel)sender).Children.ElementAt(1).Visibility = Visibility.Collapsed;
            }
            else
            {

            }
        }

        private void StackCancel_tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void StackRegsiter_tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender is StackPanel) && (e.OriginalSource is TextBlock))
            {
                ((Grid)((StackPanel)sender).Parent).Children.ElementAt(3).Visibility = Visibility.Visible;
                ((StackPanel)sender).Children.ElementAt(1).Visibility = Visibility.Collapsed;
            }
            string deviceFriendlyName = NameYourDevice.Text;
            await CommomMethods.RegisterDevice_Click(deviceFriendlyName);
            this.Frame.Navigate(typeof(MainPage), "false");
        }
    }

    
}
