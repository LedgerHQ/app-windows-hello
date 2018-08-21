﻿//using SDKTemplate;
using System;
using System.Collections.Generic;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Security.Authentication.Identity.Provider;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Credentials;
using Windows.UI.Xaml.Data;
using System.Collections.ObjectModel;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI;
using System.Threading.Tasks;
using System.Threading;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LedgerHello
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public BooleanToVisibilityConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is Visibility && (Visibility)value == Visibility.Visible);
        }
    }

    public sealed partial class MainPage : Page
    {
        String m_selectedDeviceId = String.Empty;
        String m_selectedDeviceFriendlyName = String.Empty;
        bool taskRegistered = false;
        static string authBGTaskName = "authBGTask";
        static string authBGTaskEntryPoint = "Tasks.authBGTask";
        //DispatcherTimer resizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1500) };

        public MainPage()
        {
            this.InitializeComponent();

            //var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            //var str = loader.GetString("Assistance");

            Application.Current.Resources["SystemControlHighlightListLowBrush"] = new SolidColorBrush(Colors.Transparent);
            Application.Current.Resources["SystemControlHighlightListAccentLowBrush"] = new SolidColorBrush(Colors.Transparent);
            //Application.Current.Resources["SystemAltHighColor"] = new SolidColorBrush(Colors.Black);
            //this.SizeChanged += new SizeChangedEventHandler(this.Window_SizeChanged);
            StartWatcher();

            ObservableCollection<listContent> ContentList = new ObservableCollection<listContent>();

            //Window.Current.SizeChanged += Current_SizeChanged;            
            //resizeTimer.Stop();
            //resizeTimer.Tick += ResizeTimer_Tick;
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        private void ResizeTimer_Tick(object sender, object e)
        {
            throw new NotImplementedException();
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            //resizeTimer.Stop();
            //resizeTimer.Start();

            ApplicationView.GetForCurrentView().TryResizeView(new Size(480, 485));
            e.Handled = true;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(SecondaryAuthenticationFactorDeviceFindScope.User);
            if (deviceList.Count == 0)
            {
                this.Frame.Navigate(typeof(waitingForDevice));
            }
            RefreshDeviceList(deviceList, 1000); //1000 ie: no item selected, max nb of items is 5
            //await CommomMethods.SetWindowSize();
            //this.Frame.Navigate(typeof(TestRelativePanel));            
            return;
        }

        private string ExtractDateFromDeviceInfo(SecondaryAuthenticationFactorInfo info)
        {

            string deviceConfigurationData = CryptographicBuffer.ConvertBinaryToString(0, info.DeviceConfigurationData);
            string[] dcdArray = deviceConfigurationData.
                                Replace(info.DeviceFriendlyName, "").
                                Replace(info.DeviceId, "").
                                Split('-');

            return dcdArray[4];

        }


        void RefreshDeviceList(IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList, int slectedIndex)
        {
            string dateString = string.Empty;
            listContent listItem;
            List<DateTime> dateList = new List<DateTime>();
            int cpt = 0;
            for (int index = 0; index < deviceList.Count; ++index)
            {
                SecondaryAuthenticationFactorInfo deviceInfo = deviceList.ElementAt(index);
                //Debug.WriteLine(deviceInfo.DeviceConfigurationData);

                listItem = new listContent();
                //DateTime now = DateTime.Now;
                listItem.deviceFriendlyName = deviceInfo.DeviceFriendlyName;

                listItem.deviceGUID = deviceInfo.DeviceId;
                int count = deviceInfo.DeviceFriendlyName.Count();
                listItem.date = DateTime.Parse(ExtractDateFromDeviceInfo(deviceInfo));
                dateString = CommomMethods.FormatDate(listItem.date);
                listItem.dateString = dateString;
                if (DeviceListBox.Items.Count > index - cpt)
                {
                    DeviceListBox.Items.Remove(DeviceListBox.Items.ElementAt(index - cpt));
                    cpt++;
                }
            }
            for (int index = 0; index < deviceList.Count; ++index)
            {
                SecondaryAuthenticationFactorInfo deviceInfo = deviceList.ElementAt(index);

                listItem = new listContent();
                listItem.deviceFriendlyName = deviceInfo.DeviceFriendlyName;
                listItem.deviceGUID = deviceInfo.DeviceId;
                int count = deviceInfo.DeviceFriendlyName.Count();

                listItem.date = DateTime.Parse(ExtractDateFromDeviceInfo(deviceInfo));
                dateString = CommomMethods.FormatDate(listItem.date);
                listItem.dateString = dateString;
                if (index == deviceList.Count - 1)
                {
                    listItem.isVisible = false;
                }
                else
                {
                    listItem.isVisible = true;
                }
                DeviceListBox.Items.Add(listItem);
            }
        }


        private void StartWatcher()
        {
            DeviceWatcherEventKind[] triggerEventKinds = { DeviceWatcherEventKind.Add, DeviceWatcherEventKind.Remove/*, DeviceWatcherEventKind.Update */};
            //IEnumerable<DeviceWatcherEventKind> triggerEventKinds = DeviceWatcherEventKind.Add;
            DeviceWatcher deviceWatcher = null;

            string selector = SmartCardReader.GetDeviceSelector();
            selector += " AND System.Devices.DeviceInstanceId:~~\"Ledger\"";

            deviceWatcher = DeviceInformation.CreateWatcher(selector, null);
            DeviceWatcherTrigger deviceWatcherTrigger = deviceWatcher.GetBackgroundTrigger(triggerEventKinds);
            RegisterTask(deviceWatcherTrigger);
        }
        private async void UnregisterDevice_Click(object sender, RoutedEventArgs e)
        {
            if (m_selectedDeviceId == String.Empty)
            {
                return;
            }
            await SecondaryAuthenticationFactorRegistration.UnregisterDeviceAsync(m_selectedDeviceId);
            IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                SecondaryAuthenticationFactorDeviceFindScope.User);

            listContent listItem = new listContent();
            listItem.deviceFriendlyName = m_selectedDeviceFriendlyName;
            listItem.deviceGUID = m_selectedDeviceId;
            DeviceListBox.Items.Remove(listItem);
            if (deviceList.Count == 0)
            {
                this.Frame.Navigate(typeof(waitingForDevice));
            }
            else
            {
                this.Frame.Navigate(typeof(MainPage), "false");
            }
        }
        private async void OnBgTaskProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            // WARNING: Test code
            // Handle background task progress.
            if (args.Progress == 1)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    System.Diagnostics.Debug.WriteLine("[OnBgTaskProgress] Background task is started.");
                });
            }
        }
        async void RegisterTask(DeviceWatcherTrigger deviceWatcherTrigger)
        {
            System.Diagnostics.Debug.WriteLine("[RegisterTask] Register the background task.");
            //
            // Check for existing registrations of this background task.
            //

            BackgroundExecutionManager.RemoveAccess();
            var access = await BackgroundExecutionManager.RequestAccessAsync();

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == authBGTaskName)
                {
                    taskRegistered = true;
                    //task.Value.Unregister(true);
                    break;
                }
            }

            if (!taskRegistered)
            {

                if (access == BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
                {
                    BackgroundTaskBuilder authTaskBuilder = new BackgroundTaskBuilder();
                    authTaskBuilder.Name = authBGTaskName;
                    SecondaryAuthenticationFactorAuthenticationTrigger myTrigger = new SecondaryAuthenticationFactorAuthenticationTrigger();
                    authTaskBuilder.TaskEntryPoint = authBGTaskEntryPoint;
                    authTaskBuilder.SetTrigger(myTrigger);
                    BackgroundTaskRegistration taskReg = authTaskBuilder.Register();

                    BackgroundTaskBuilder plugTaskBuilder = new BackgroundTaskBuilder();
                    plugTaskBuilder.Name = authBGTaskName;
                    plugTaskBuilder.TaskEntryPoint = authBGTaskEntryPoint;
                    plugTaskBuilder.SetTrigger(deviceWatcherTrigger);
                    BackgroundTaskRegistration taskReg2 = plugTaskBuilder.Register();
                    //String taskRegName = taskReg.Name;
                    
                    //BackgroundTaskBuilder rebootTaskBuilder = new BackgroundTaskBuilder();
                    //rebootTaskBuilder.Name = authBGTaskName;
                    //rebootTaskBuilder.TaskEntryPoint = authBGTaskEntryPoint;
                    //SystemTrigger trigger = new SystemTrigger(SystemTriggerType.UserPresent, false);
                    //rebootTaskBuilder.SetTrigger(trigger);
                    //BackgroundTaskRegistration taskReg3 = rebootTaskBuilder.Register();
                    //String taskRegName = taskReg.Name;
                    //taskReg.Progress += OnBgTaskProgress;
                    System.Diagnostics.Debug.WriteLine("[RegisterTask] Background task registration is completed.");
                    taskRegistered = true;
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

        private void RegsiterDevice_pointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
            if (sender is StackPanel)
            {
                ((StackPanel)((Grid)((StackPanel)sender).Parent).Children.ElementAt(2)).Visibility = Visibility.Visible;
            }
            else
            {

            }

            //((Image)e.OriginalSource).Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Button-register-select.png"));
            e.Handled = true;
        }

        private void RegisterDevice_pointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            if ((sender is StackPanel) && (e.OriginalSource is Grid))
            {
                ((StackPanel)((Grid)((StackPanel)sender).Parent).Children.ElementAt(2)).Visibility = Visibility.Collapsed;
            }
            else if ((sender is StackPanel) && ((e.OriginalSource is TextBlock) || (e.OriginalSource is Image)))
            {
                ((StackPanel)sender).Visibility = Visibility.Collapsed;
            }
            else
            {

            }
            e.Handled = true;
        }

        //private void ListViewItem_pointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        //    if (e.OriginalSource is TextBlock)
        //    {
        //        //m_selectedDeviceFriendlyName = ((TextBlock)((StackPanel)((TextBlock)e.OriginalSource).Parent).Children.ElementAt(0)).Text;
        //        //((Image)((StackPanel)((TextBlock)e.OriginalSource).Parent).Children.ElementAt(2)).Visibility = Visibility.Visible;
        //        m_selectedDeviceFriendlyName = ((TextBlock)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Text;
        //        ((TextBlock)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Margin = new Thickness(25, 0, 0, 0);
        //        ((Image)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(0))).Visibility = Visibility.Visible;
        //        ((Image)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(3))).Visibility = Visibility.Visible;
        //    }
        //    else if (e.OriginalSource is Image)
        //    {
        //        m_selectedDeviceFriendlyName = ((TextBlock)(((StackPanel)(((Image)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Text;
        //        //((TextBlock)(((StackPanel)(((Image)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Margin = new Thickness(0);
        //        ((Image)(((StackPanel)(((Image)(e.OriginalSource)).Parent)).Children.ElementAt(0))).Visibility = Visibility.Visible;
        //        ((Image)(((StackPanel)(((Image)(e.OriginalSource)).Parent)).Children.ElementAt(3))).Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        //throw new Exception("Unknown pointer");
        //    }
        //    for (int i = 0; i < DeviceListBox.Items.Count; i++)
        //    {
        //        if (((listContent)(DeviceListBox.Items.ElementAt(i))).deviceFriendlyName == m_selectedDeviceFriendlyName)
        //        {
        //            m_selectedDeviceId = ((listContent)(DeviceListBox.Items.ElementAt(i))).deviceGUID;
        //        }
        //    }
        //    e.Handled = true;
        //}

        //private void ListViewItem_pointerExited(object sender, PointerRoutedEventArgs e)
        //{
        //    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        //    if (e.OriginalSource is Image)
        //    {
        //        ((Image)e.OriginalSource).Visibility = Visibility.Collapsed;
        //    }
        //    else if (e.OriginalSource is TextBlock)
        //    {
        //        ((Image)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Visibility = Visibility.Collapsed;
        //    }
        //    else if (e.OriginalSource is ListViewItemPresenter)
        //    {
        //        ((TextBlock)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(1))).Margin = new Thickness(30, 0, 0, 0);
        //        ((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(0))).Visibility = Visibility.Collapsed;
        //        ((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(3))).Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        //throw new Exception("Unknown pointer");
        //    }
        //    e.Handled = true;
        //    //((Windows.UI.Xaml.Controls.Primitives.ListViewItemPresenter)e.OriginalSource)
        //}

        //private void ElementStackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        //    if (e.OriginalSource is ListViewItemPresenter)
        //    {
        //        m_selectedDeviceFriendlyName = ((TextBlock)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(1))).Text;
        //        ((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(0))).Visibility = Visibility.Visible;
        //        ((TextBlock)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(1))).Margin = new Thickness(23, 0, 0, 0);
        //        ((TextBlock)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(2))).Width = 105;
        //        ((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(3))).Visibility = Visibility.Visible;
        //    }
        //    else if (e.OriginalSource is Image)
        //    {
        //        m_selectedDeviceFriendlyName = ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(1)).Text;
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(0)).Visibility = Visibility.Visible;
        //        ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(1)).Margin = new Thickness(23, 0, 0, 0);
        //        ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(2)).Width = 105;
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(3)).Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        //throw new Exception("Unknown pointer");
        //    }
        //    for (int i = 0; i < DeviceListBox.Items.Count; i++)
        //    {
        //        if (((listContent)(DeviceListBox.Items.ElementAt(i))).deviceFriendlyName == m_selectedDeviceFriendlyName)
        //        {
        //            m_selectedDeviceId = ((listContent)(DeviceListBox.Items.ElementAt(i))).deviceGUID;
        //        }
        //    }
        //    e.Handled = true;
        //}

        //private void ElementStackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        //{
        //    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        //    if (e.OriginalSource is ListViewItemPresenter)
        //    {
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(0)).Visibility = Visibility.Collapsed;
        //        ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(1)).Margin = new Thickness(28, 0, 0, 0);
        //        ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(2)).Width = 125;
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(3)).Visibility = Visibility.Collapsed;
        //        //((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(0))).Visibility = Visibility.Collapsed;
        //        //((TextBlock)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(1))).Margin = new Thickness(30, 0, 0, 0);
        //        //((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(3))).Visibility = Visibility.Collapsed;
        //    }
        //    /*else if ((e.OriginalSource is Image) && (((Image)e.OriginalSource).Parent is StackPanel))
        //    {
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)((Image)e.OriginalSource).Parent).Children.ElementAt(0)).Content).Children.ElementAt(0)).Visibility = Visibility.Collapsed;
        //        ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)((Image)e.OriginalSource).Parent).Children.ElementAt(0)).Content).Children.ElementAt(1)).Margin = new Thickness(28, 0, 0, 0);
        //        ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)((Image)e.OriginalSource).Parent).Children.ElementAt(0)).Content).Children.ElementAt(2)).Width = 125;
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)((Image)e.OriginalSource).Parent).Children.ElementAt(0)).Content).Children.ElementAt(3)).Visibility = Visibility.Collapsed;
        //    }*/
        //    else if (e.OriginalSource is Grid)
        //    {
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(0)).Visibility = Visibility.Collapsed;
        //        ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(1)).Margin = new Thickness(28, 0, 0, 0);
        //        ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(2)).Width = 125;
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(3)).Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        //throw new Exception("Unknown pointer");
        //    }
        //    e.Handled = true;
        //}

        //private void Divider_PointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    if (sender is Image)
        //    {
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)((Image)sender).Parent).Children.ElementAt(0)).Content).Children.ElementAt(0)).Visibility = Visibility.Collapsed;
        //        ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)((Image)sender).Parent).Children.ElementAt(0)).Content).Children.ElementAt(1)).Margin = new Thickness(28, 0, 0, 0);
        //        ((Image)((StackPanel)((ListViewItem)((StackPanel)((Image)sender).Parent).Children.ElementAt(0)).Content).Children.ElementAt(3)).Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        //throw new Exception("Unknown pointer");
        //    }
        //    e.Handled = true;
        //}

        private async void Assistance_Click(object sender, TappedRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            string uriToLaunch = @"http://support.ledgerwallet.com";
            var uri = new Uri(uriToLaunch);
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);
            e.Handled = true;
        }
        private void Regsiter_tapped(object sender, TappedRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            this.Frame.Navigate(typeof(waitingForDevice));
        }

        private async void Trash_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            ((RelativePanel)((Image)sender).Parent).Children.ElementAt(0).Visibility = Visibility.Collapsed;
            ((TextBlock)((RelativePanel)((Image)sender).Parent).Children.ElementAt(1)).Margin = new Thickness(28, 0, 0, 0);
            ((TextBlock)((RelativePanel)((Image)sender).Parent).Children.ElementAt(2)).Width = 125;
            ((RelativePanel)((Image)sender).Parent).Children.ElementAt(3).Visibility = Visibility.Collapsed;

            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string title = loader.GetString("DeletingDevice_title");

            string content = loader.GetString("DeletingDevice_content");


            var yesCommand = new UICommand(loader.GetString("Yes"), cmd => { UnregisterDevice_Click(sender, e); });
            var noCommand = new UICommand(loader.GetString("No"), cmd => { this.Frame.Navigate(typeof(MainPage)); });

            var dialog = new MessageDialog(content, title);
            dialog.Options = MessageDialogOptions.None;
            dialog.Commands.Add(yesCommand);

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;

            if (noCommand != null)
            {
                dialog.Commands.Add(noCommand);
                dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
            }

            var command = await dialog.ShowAsync();

            if (command == yesCommand)
            {
                // handle yes command
            }
            else if (command == noCommand)
            {
                // handle no command
            }
            else
            {
                // handle cancel command
            }
        }

        private void ListItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is ListViewItem)
            {
                m_selectedDeviceFriendlyName = ((TextBlock)((RelativePanel)((ListViewItem)sender).Content).Children.ElementAt(1)).Text;
                ((RelativePanel)((ListViewItem)sender).Content).Children.ElementAt(0).Visibility = Visibility.Visible;
                ((TextBlock)((RelativePanel)((ListViewItem)sender).Content).Children.ElementAt(1)).Margin = new Thickness(35, 0, 0, 0);
                ((TextBlock)((RelativePanel)((ListViewItem)sender).Content).Children.ElementAt(2)).Width = 104;
                ((RelativePanel)((ListViewItem)sender).Content).Children.ElementAt(3).Visibility = Visibility.Visible;


            }
            else
            {
                
            }

            for (int i = 0; i < DeviceListBox.Items.Count; i++)
            {
                if (((listContent)(DeviceListBox.Items.ElementAt(i))).deviceFriendlyName == m_selectedDeviceFriendlyName)
                {
                    m_selectedDeviceId = ((listContent)(DeviceListBox.Items.ElementAt(i))).deviceGUID;
                }
            }
            e.Handled = true;
        }

        private void ListItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is ListViewItem)
            {
                ((RelativePanel)((ListViewItem)sender).Content).Children.ElementAt(0).Visibility = Visibility.Collapsed;
                ((TextBlock)((RelativePanel)((ListViewItem)sender).Content).Children.ElementAt(1)).Margin = new Thickness(40, 0, 0, 0);
                ((TextBlock)((RelativePanel)((ListViewItem)sender).Content).Children.ElementAt(2)).Width = 124;
                ((RelativePanel)((ListViewItem)sender).Content).Children.ElementAt(3).Visibility = Visibility.Collapsed;
            }
            else
            {

            }
            e.Handled = true;
        }

        private void Trash_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
            e.Handled = true;
        }

        private void Trash_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            e.Handled = true;
        }
    }
}