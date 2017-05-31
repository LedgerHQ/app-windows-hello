//using SDKTemplate;
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



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WindowsHelloWithLedger
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    //public class SmartCardListItem
    //{
    //    public string ReaderName
    //    {
    //        get;
    //        set;
    //    }

    //    public string CardName
    //    {
    //        get;
    //        set;
    //    }

    //}

    public class listContent
    {
        public string deviceFriendlyName { get; set; }
        public bool isVisible { get; set; }
        public DateTime date { get; set; }
        public string dateString { get; set; }
        public string deviceGUID { get; set; }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public Visibility OnTrue { get; set; }
        public Visibility OnFalse { get; set; }

        public BooleanToVisibilityConverter()
        {
            OnFalse = Visibility.Collapsed;
            OnTrue = Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = (bool)value;

            return v ? OnTrue : OnFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility == false)
                return DependencyProperty.UnsetValue;

            if ((Visibility)value == OnTrue)
                return true;
            else
                return false;
        }
    }



    public sealed partial class MainPage : Page
    {
        String m_selectedDeviceId = String.Empty;
        String m_selectedDeviceFriendlyName = String.Empty;
        bool taskRegistered = false;
        static string authBGTaskName = "authBGTask";
        static string authBGTaskEntryPoint = "Tasks.authBGTask";
        //static string dLockCheckBGTaskName = "dLockCheckBGTask";
        //static string dLockCheckBGTaskEntryPoint = "Tasks.dLockCheckBGTask";
        String deviceFriendlyName = "";
        // TODO : get deviceModelNumber from device
        String deviceModelNumber = "0001";
        bool first_registration = true;

        //public ObservableCollection<listContent> ContentList { get; set; }        

        //DeviceWatcher watcher = null;

        public MainPage()
        {
            this.InitializeComponent();

            //DeviceListBox.SelectionChanged += DeviceListBox_SelectionChanged;
            ObservableCollection<listContent> ContentList = new ObservableCollection<listContent>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(SecondaryAuthenticationFactorDeviceFindScope.AllUsers);

            //DeviceListBox.DataContext = ContentList;
            
            RefreshDeviceList(deviceList,1000); //1000 ie: no item selected, max nb of items is 5

            var registerDevice = e.Parameter as string;
            if (registerDevice == "true")
            {
                RegisterDevice_Click(null, null);
            }
            return;
        }

        void RefreshDeviceList(IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList, int slectedIndex)
        {
            //byte[] deviceConfigurationDataArray;
            string deviceConfigurationString;
            string dateString = string.Empty;
            listContent listItem;
            List<DateTime> dateList = new List<DateTime>();
            int cpt = 0;
            for (int index = 0; index < deviceList.Count; ++index)
            {
                SecondaryAuthenticationFactorInfo deviceInfo = deviceList.ElementAt(index);
                deviceConfigurationString = CryptographicBuffer.ConvertBinaryToString(0, deviceInfo.DeviceConfigurationData);
                listItem = new listContent();
                //DateTime now = DateTime.Now;
                listItem.deviceFriendlyName = deviceInfo.DeviceFriendlyName;
                listItem.deviceGUID = deviceInfo.DeviceId;
                int count = deviceInfo.DeviceFriendlyName.Count();
                listItem.date = DateTime.Parse(deviceConfigurationString.Substring(35 + 1 + count + 1 + 1));
                dateString = FormatDate(listItem.date);
                listItem.dateString = dateString;
                //addDate = ((listContent)(DeviceListBox.Items.ElementAt(0))).date;
                //string frit = ((listContent)DeviceListBox.Items.ElementAt(0)).date.ToString();
                //dateList.Add(((listContent)DeviceListBox.Items.ElementAt(0)).date);
                //dateList.Add(((listContent)DeviceListBox.Items.ElementAt(index - cpt)).date);
                if (DeviceListBox.Items.Count > index - cpt)
                {                    
                    DeviceListBox.Items.Remove(DeviceListBox.Items.ElementAt(index - cpt));
                    cpt++;
                }
            }
            for (int index = 0; index < deviceList.Count; ++index)
            {
                SecondaryAuthenticationFactorInfo deviceInfo = deviceList.ElementAt(index);
                deviceConfigurationString = CryptographicBuffer.ConvertBinaryToString(0, deviceInfo.DeviceConfigurationData);
                listItem = new listContent();
                listItem.deviceFriendlyName = deviceInfo.DeviceFriendlyName;
                listItem.deviceGUID = deviceInfo.DeviceId;
                int count = deviceInfo.DeviceFriendlyName.Count();
                listItem.date = DateTime.Parse(deviceConfigurationString.Substring(35 + 1 + count + 1 + 1));
                dateString = FormatDate(listItem.date);
                listItem.dateString = dateString;

                if (slectedIndex == index)
                {
                    listItem.isVisible = true;
                }
                else
                {
                    listItem.isVisible = false;
                }
                DeviceListBox.Items.Add(listItem);
            }
        }

        private string FormatDate(DateTime dateToFormat)
        {
            string dateString = string.Empty;            
            DateTime now = DateTime.Now;
            if (dateToFormat.Year - now.Year == 0)
            {
                if ( now.DayOfYear - dateToFormat.DayOfYear == 0) //Today
                {
                    if ((dateToFormat.TimeOfDay.Hours) > 12)
                    {
                        dateString = "TODAY, " + (dateToFormat.TimeOfDay.Hours - 12) + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " PM";
                    }
                    else
                    {
                        dateString = "TODAY, " + dateToFormat.TimeOfDay.Hours + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " AM";
                    }
                }
                else if (now.DayOfYear - dateToFormat.DayOfYear  == 1) //Yesterday
                {
                    if ((dateToFormat.TimeOfDay.Hours) > 12)
                    {
                        dateString = "YESTERDAY, " + (dateToFormat.TimeOfDay.Hours - 12) + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " PM";
                    }
                    else
                    {
                        dateString = "YESTERDAY, " + dateToFormat.TimeOfDay.Hours + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " AM";
                    }
                }
            }
            else
            {
                int month = dateToFormat.Month;
                string monthString = string.Empty;
                switch (month)
                {
                    case 1:
                        monthString = "JAN, ";
                        break;
                    case 2:
                        monthString = "FEB, ";
                        break;
                    case 3:
                        monthString = "MAR, ";
                        break;
                    case 4:
                        monthString = "APR, ";
                        break;
                    case 5:
                        monthString = "MAY, ";
                        break;
                    case 6:
                        monthString = "JUN, ";
                        break;
                    case 7:
                        monthString = "JUL, ";
                        break;
                    case 8:
                        monthString = "AUG, ";
                        break;
                    case 9:
                        monthString = "SEP, ";
                        break;
                    case 10:
                        monthString = "OCT, ";
                        break;
                    case 11:
                        monthString = "NOV, ";
                        break;
                    case 12:
                        monthString = "DEC, ";
                        break;
                }
                string dayOfWeek = dateToFormat.DayOfWeek.ToString().ToUpper().Substring(0, 3);
                if ((dateToFormat.TimeOfDay.Hours) > 12)
                {
                    dateString = dayOfWeek + " " + dateToFormat.Day + " " + monthString + (dateToFormat.TimeOfDay.Hours - 12) + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " PM";
                }
                else
                {
                    dateString = dayOfWeek + " " + dateToFormat.Day + " " + monthString + dateToFormat.TimeOfDay.Hours + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " AM";
                }
            }
            return dateString;
        }
        private async void RegisterDevice_Click(object sender, RoutedEventArgs e)
        {
            String deviceId = "";
            IBuffer deviceKey = CryptographicBuffer.GenerateRandom(32);
            IBuffer authKey = CryptographicBuffer.GenerateRandom(32);
            byte[] deviceKeyArray = new byte[32];
            byte[] authKeyArray = new byte[32];
            byte[] deviceIdArray = new byte[16];
            byte[] deviceDlockState = new byte[1];
            byte[] response = { 0 };
            string sw1sw2 = null;
            //byte[] combinedDataArray = new byte[64];
            string NanosATR = "3b00";
            //List<SmartCardListItem> cardItems = new List<SmartCardListItem>();
            MessageDialog myDlg;

            bool IsNanosPresent = false;
            bool isSupported;
            isSupported = await KeyCredentialManager.IsSupportedAsync();

            if (!isSupported)
            {
                myDlg = new MessageDialog("Please setup PIN for your device and try again.");
                await myDlg.ShowAsync();
                return;
            }

            string selector = SmartCardReader.GetDeviceSelector();
            selector += " AND System.Devices.DeviceInstanceId:~~\"Ledger\"";
            //string test = selector.Replace(" ", ((char)34).ToString());
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

            foreach (DeviceInformation device in devices)
            {
                SmartCardReader reader = await SmartCardReader.FromIdAsync(device.Id);
                IReadOnlyList<SmartCard> cards = await reader.FindAllCardsAsync();
                foreach (SmartCard card in cards)
                {
                    SmartCardProvisioning provisioning = await SmartCardProvisioning.FromSmartCardAsync(card);                    
                    IBuffer ATR = await card.GetAnswerToResetAsync();
                    string ATR_str = CryptographicBuffer.EncodeToHexString(ATR);

                    if (ATR_str.Equals(NanosATR))
                    {
                        IsNanosPresent = true;
                        
                        deviceFriendlyName = "";
                        bool foundCompanionDevice = false;
                        // List the registered devices to prevent registering twice the same device
                        IReadOnlyList<SecondaryAuthenticationFactorInfo> registeredDeviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                            SecondaryAuthenticationFactorDeviceFindScope.AllUsers);

                        SmartCardConnection connection = await card.ConnectAsync();
                        response = await Apdu.TransmitApduAsync(connection, Apdu.getDeviceGuidCmdApdu);
                        sw1sw2 = Apdu.ApduResponseParser(response, out response);
                        connection.Dispose();
                        deviceIdArray = response;
                        deviceId = BitConverter.ToString(response).Replace("-", "");
                        // Loop on registered devices to check if device to register has already been registered
                        for (int i = 0; i < registeredDeviceList.Count(); i++)
                        {
                            if (registeredDeviceList.ElementAt(i).DeviceId == deviceId)
                            {
                                deviceFriendlyName = registeredDeviceList.ElementAt(i).DeviceFriendlyName;
                                foundCompanionDevice = true;
                                break;
                            }
                        }
                        if (foundCompanionDevice)// This device has already been registered
                        {
                            // New message dialog to inform user, and break from card loop
                            //myDlg = null;
                            //myDlg = new MessageDialog("The device \"" + deviceFriendlyName + "\" has already been registered");
                            //await myDlg.ShowAsync();
                             continue;
                        }
                        
                        // Device naming loop
                        while (deviceFriendlyName == "")
                        {
                            deviceFriendlyName = await Textbox.InputTextDialogAsync("Name your device" + Environment.NewLine + "(Action required on device afterwards)");
                            if (deviceFriendlyName == "")
                            {
                                myDlg = null;
                                myDlg = new MessageDialog("You must type a device name");
                                await myDlg.ShowAsync();
                                //return;                                
                            }
                        }

                        connection = await card.ConnectAsync();

                        response = await Apdu.TransmitApduAsync(connection, Apdu.getDlockStateCmdApdu);
                        sw1sw2 = Apdu.ApduResponseParser(response, out response);
                        deviceDlockState = response;

                        response = await Apdu.TransmitApduAsync(connection, Apdu.startRegistrationCmdApdu);
                        sw1sw2 = Apdu.ApduResponseParser(response, out response);

                        connection.Dispose();

                        if (sw1sw2 != "9000")
                        {
                            myDlg = null;
                            myDlg = new MessageDialog("Registration denied by user");
                            await myDlg.ShowAsync();
                            first_registration = false;
                            return;
                        }
                        // Get device key from response
                        for (int index = 0; index < 32; index++)
                        {
                            deviceKeyArray[index] = response[index];
                        }
                        deviceKey = CryptographicBuffer.CreateFromByteArray(deviceKeyArray);
                        // Get auth key from response
                        for (int index = 0; index < 32; index++)
                        {
                            authKeyArray[index] = response[index+32];
                        }
                        authKey = CryptographicBuffer.CreateFromByteArray(authKeyArray);

                        byte[] deviceConfigDataArray = new byte[18]; //16 bytes for GUID and 1 byte for dLockstate

                        for (int i = 0; i < 16; i++)
                        {
                            deviceConfigDataArray[i] = deviceIdArray[i];
                        }
                        deviceConfigDataArray[16] = deviceDlockState[0];
                        deviceConfigDataArray[17] = 0; // 1 if used for last logon, 0 instead

                        string deviceConfigString = "";
                        DateTime addDate = DateTime.Now;
                        if (deviceDlockState[0] == 0)
                        {
                            deviceConfigString = deviceId + "-0-0-" + deviceFriendlyName + "-" + addDate.ToString();
                        }
                        else
                        {
                            deviceConfigString = deviceId + "-1-0-" + deviceFriendlyName + "-" + addDate.ToString();
                        }                        

                        // Get a Ibuffer from combinedDataArray
                        IBuffer deviceConfigData = CryptographicBuffer.ConvertStringToBinary(deviceConfigString, 0);
                        //IBuffer deviceConfigData = CryptographicBuffer.CreateFromByteArray(deviceConfigDataArray);

                        SecondaryAuthenticationFactorDeviceCapabilities capabilities = SecondaryAuthenticationFactorDeviceCapabilities.SecureStorage;                            
                        SecondaryAuthenticationFactorRegistrationResult registrationResult = await SecondaryAuthenticationFactorRegistration.RequestStartRegisteringDeviceAsync(
                                deviceId,
                                capabilities,
                                deviceFriendlyName,
                                deviceModelNumber,
                                deviceKey,
                                authKey);

                        if (registrationResult.Status != SecondaryAuthenticationFactorRegistrationStatus.Started)
                        {
                            myDlg = null;

                            if (registrationResult.Status == SecondaryAuthenticationFactorRegistrationStatus.DisabledByPolicy)
                            {
                                //For DisaledByPolicy Exception:Ensure secondary auth is enabled.
                                //Use GPEdit.msc to update group policy to allow secondary auth
                                //Local Computer Policy\Computer Configuration\Administrative Templates\Windows Components\Microsoft Secondary Authentication Factor\Allow Companion device for secondary authentication
                                myDlg = new MessageDialog("Disabled by Policy.  Please update the policy and try again.");
                            }

                            if (registrationResult.Status == SecondaryAuthenticationFactorRegistrationStatus.PinSetupRequired)
                            {
                                //For PinSetupRequired Exception:Ensure PIN is setup on the device
                                //Either use gpedit.msc or set reg key
                                //This setting can be enabled by creating the AllowDomainPINLogon REG_DWORD value under the HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System Registry key and setting it to 1.
                                myDlg = new MessageDialog("Please setup PIN for your device and try again.");
                            }

                            if (myDlg != null)
                            {
                                await myDlg.ShowAsync();
                                return;
                            }
                        }

                        System.Diagnostics.Debug.WriteLine("[RegisterDevice_Click] Device Registration Started!");
                        await registrationResult.Registration.FinishRegisteringDeviceAsync(deviceConfigData);
                        //DeviceListBox.Items.Add(deviceFriendlyName);
                        System.Diagnostics.Debug.WriteLine("[RegisterDevice_Click] Device Registration is Complete!");

                        IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                            SecondaryAuthenticationFactorDeviceFindScope.AllUsers);

                        SecondaryAuthenticationFactorDevicePresenceMonitoringRegistrationStatus status =
                            await SecondaryAuthenticationFactorRegistration.RegisterDevicePresenceMonitoringAsync(
                            deviceId,
                            device.Id,
                            SecondaryAuthenticationFactorDevicePresenceMonitoringMode.AppManaged/*,
                            deviceFriendlyName,
                            deviceModelNumber,
                            deviceConfigData*/);

                        switch (status)
                        {
                            //case SecondaryAuthenticationFactorDevicePresenceMonitoringRegistrationStatus.Succeeded:
                            //    await new MessageDialog("Registered for presence monitoring!").ShowAsync();
                            //    break;

                            case SecondaryAuthenticationFactorDevicePresenceMonitoringRegistrationStatus.DisabledByPolicy:
                                await new MessageDialog("Registered for presence disabled by policy!").ShowAsync();
                                break;
                        }
                        
                        listContent listItem = new listContent();
                        listItem.deviceFriendlyName = deviceFriendlyName;
                        listItem.deviceGUID = deviceId;
                        listItem.isVisible = false;
                        listItem.date = addDate;
                        DeviceListBox.Items.Add(listItem);
                        StartWatcher();
                        //this.Frame.Navigate(typeof(MainPage), "false");
                    }
                }
            }
            if (IsNanosPresent == false)
            {
                myDlg = new MessageDialog("Ledger Nano-s for Windows Hello not found" + Environment.NewLine + Environment.NewLine + "Please plug a ledger Nano-s in a usb port");
                await myDlg.ShowAsync();
                return;
            }
            return;
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

        private async void DeviceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //List<object>  test = DeviceListBox.Items.ToList();
            int selectedIndex;
            
            if (DeviceListBox.SelectedIndex >= 0)
            {
                IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                           SecondaryAuthenticationFactorDeviceFindScope.User);

                if (DeviceListBox.Items.Count > 0)
                {
                    selectedIndex = DeviceListBox.SelectedIndex;
                    // m_selectedDeviceId = 
                    //m_selectedDeviceId = DeviceListBox.SelectedItem.ToString();
                    //m_selectedDeviceId = ((listContent)DeviceListBox.SelectedItem).deviceFriendlyName;
                    m_selectedDeviceId = ((WindowsHelloWithLedger.listContent)DeviceListBox.SelectedItem).deviceGUID;
                    m_selectedDeviceFriendlyName = ((WindowsHelloWithLedger.listContent)DeviceListBox.SelectedItem).deviceFriendlyName;

                    //RefreshDeviceList(deviceList, selectedIndex);
                    //string frit = test[selectedIndex].ToString();
                    //m_selectedDeviceId = test[selectedIndex].ToString();
                    //m_selectedDeviceId = DeviceListBox.Items.;
                    //ItemIndexRange range = 
                    DeviceListBox.DeselectRange(new ItemIndexRange(0, (uint)DeviceListBox.Items.Count));
                }
                else
                {
                    m_selectedDeviceId = String.Empty;
                    return;
                }
                System.Diagnostics.Debug.WriteLine("[DeviceListBox_SelectionChanged] The device " + m_selectedDeviceFriendlyName + " is selected.");
                //SecondaryAuthenticationFactorInfo info = DeviceListBox.FindName("HelloDevice");



                if (deviceList.Count() != 0)
                {
                    //for (int i = 0; i < deviceList.Count(); i++)
                    //{
                    //    if (m_selectedDeviceId == deviceList.ElementAt(i).DeviceFriendlyName)
                    //    {
                    //        m_selectedDeviceId = deviceList.ElementAt(i).DeviceId;
                    //    }
                    //}
                    //m_selectedDeviceId = deviceList.ElementAt(deviceList.Count() - 1).DeviceId;
                    //Store the selected device in settings to be used in the BG task
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values["SelectedDevice"] = m_selectedDeviceId;
                    //UnregisterDevice_Click(null, null);
                }
                RefreshDeviceList(deviceList, selectedIndex);
            }
        }

        private async void UnregisterDevice_Click(object sender, RoutedEventArgs e)
        {
            //m_selectedDeviceId = "B898843290DCEC2952FF639D81584DB2";
            if (m_selectedDeviceId == String.Empty)
            {
                return;
            }

            //InfoList.Items.Add("Unregister a device:");

            await SecondaryAuthenticationFactorRegistration.UnregisterDeviceAsync(m_selectedDeviceId);

            //InfoList.Items.Add("Device unregistration is completed.");

            IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                SecondaryAuthenticationFactorDeviceFindScope.User);

            listContent listItem = new listContent();
            listItem.deviceFriendlyName = m_selectedDeviceFriendlyName;
            listItem.deviceGUID = m_selectedDeviceId;
            listItem.isVisible = true;
            DeviceListBox.Items.Remove(listItem);
            listItem.isVisible = false;
            DeviceListBox.Items.Remove(listItem);
            //DeviceListBox.DataContext = null;
            //DeviceListBox.DataContext = ContentList;


            //RefreshDeviceList(deviceList);

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
                    String taskRegName = taskReg.Name;
                    //taskReg.Progress += OnBgTaskProgress;
                    System.Diagnostics.Debug.WriteLine("[RegisterTask] Background task registration is completed.");
                    taskRegistered = true;
                }
            }
        }
        private void Assistance_pointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ((Image)e.OriginalSource).Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Button-assistance-select.png"));
            e.Handled = true;
        }
        private void Assistance_pointerExited(object sender, PointerRoutedEventArgs e)
        {
            ((Image)((Grid)e.OriginalSource).Children.ElementAt(1)).Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Button-assistance.png"));
            e.Handled = true;
        }

        private void RegsiterDevice_pointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ((Image)e.OriginalSource).Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Button-register-select.png"));
            e.Handled = true;
        }

        private void RegisterDevice_pointerExited(object sender, PointerRoutedEventArgs e)
        {
            ((Image)((Grid)e.OriginalSource).Children.ElementAt(1)).Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Button-register.png"));
            e.Handled = true;
        }

        private void ListViewItem_pointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //var _ListView = DeviceListBox as ListView;
            //foreach (var _ListViewItem in _ListView.Items)
            //{
            //    var _Container = _ListView.ContainerFromItem(_ListViewItem);
            //    var _Children = AllChildren(_Container);
            //}


            if (e.OriginalSource is TextBlock)
            {
                //m_selectedDeviceFriendlyName = ((TextBlock)((StackPanel)((TextBlock)e.OriginalSource).Parent).Children.ElementAt(0)).Text;
                //((Image)((StackPanel)((TextBlock)e.OriginalSource).Parent).Children.ElementAt(2)).Visibility = Visibility.Visible;
                m_selectedDeviceFriendlyName = ((TextBlock)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Text;
                ((TextBlock)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Margin = new Thickness(25,0,0,0);
                ((Image)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(0))).Visibility = Visibility.Visible;
                ((Image)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(3))).Visibility = Visibility.Visible;
            }
            else if (e.OriginalSource is Image)
            {
                m_selectedDeviceFriendlyName = ((TextBlock)(((StackPanel)(((Image)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Text;
                //((TextBlock)(((StackPanel)(((Image)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Margin = new Thickness(0);
                ((Image)(((StackPanel)(((Image)(e.OriginalSource)).Parent)).Children.ElementAt(0))).Visibility = Visibility.Visible;
                ((Image)(((StackPanel)(((Image)(e.OriginalSource)).Parent)).Children.ElementAt(3))).Visibility = Visibility.Visible;
            }
            else
            {
                throw new Exception("Unknown pointer");
            }
            for (int i = 0; i < DeviceListBox.Items.Count; i++)
            {
                if ( ((listContent)(DeviceListBox.Items.ElementAt(i))).deviceFriendlyName == m_selectedDeviceFriendlyName)
                {
                    m_selectedDeviceId = ((listContent)(DeviceListBox.Items.ElementAt(i))).deviceGUID;
                }
            }
            e.Handled = true;
        }

        private void ListViewItem_pointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.OriginalSource is Image)
            {
                ((Image)e.OriginalSource).Visibility = Visibility.Collapsed;
            }
            else if (e.OriginalSource is TextBlock)
            {
                ((Image)(((StackPanel)(((TextBlock)(e.OriginalSource)).Parent)).Children.ElementAt(1))).Visibility = Visibility.Collapsed;
            }
            else if (e.OriginalSource is ListViewItemPresenter)
            {
                ((TextBlock)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(1))).Margin = new Thickness(30,0,0,0);
                ((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(0))).Visibility = Visibility.Collapsed;
                ((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(3))).Visibility = Visibility.Collapsed;
                //string deviceName = ((listContent)((ListViewItemPresenter)e.OriginalSource).Content).deviceFriendlyName;
                //for (int i=0; i< DeviceListBox.Items.Count; i++)
                //{
                //    if (((listContent)DeviceListBox.Items.ElementAt(i)).deviceFriendlyName == deviceName)
                //    {
                //        //object img = ((Windows.UI.Xaml.Controls.Primitives.ListViewItemPresenter)e.OriginalSource).FindName("trash");                       
                //        //DeviceListBox.Parent
                //        //this.Visibility = Visibility.Collapsed;
                //        //((Image)(DeviceListBox.ItemsPanelRoot.Children.ElementAt(i))).Visibility = Visibility.Collapsed;
                //        ((listContent)DeviceListBox.Items.ElementAt(i)).isVisible = false;
                //        this.Frame.Navigate(typeof(MainPage), "false");
                //    }
                //}
            }
            else if (e.OriginalSource is Grid)
            {

                //throw new Exception("Grid");
            }
            else
            {
                throw new Exception("Unknown pointer");
            }
            e.Handled = true;
            //((Windows.UI.Xaml.Controls.Primitives.ListViewItemPresenter)e.OriginalSource)
        }
        public List<Control> AllChildren(DependencyObject parent)
        {
            var _List = new List<Control> { };
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is Control)
                {
                    _List.Add(_Child as Control);
                }
                //_List.Add(_Child as Control);
                _List.AddRange(AllChildren(_Child));
            }
            return _List;
        }

        private void ElementStackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.OriginalSource is ListViewItemPresenter)
            {
                ((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(0))).Visibility = Visibility.Collapsed;
                ((TextBlock)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(1))).Margin = new Thickness(30, 0, 0, 0);
                ((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(3))).Visibility = Visibility.Collapsed;
            }
            else if (e.OriginalSource is Image)
            {
                ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(0)).Visibility = Visibility.Collapsed;
                ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(1)).Margin = new Thickness(30, 0, 0, 0);
                ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(3)).Visibility = Visibility.Collapsed;
            }
            else
            {

            }
        }

        private void ElementStackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.OriginalSource is ListViewItemPresenter)
            {
                ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(0)).Visibility = Visibility.Collapsed;
                ((TextBlock)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(1)).Margin = new Thickness(30, 0, 0, 0);
                ((Image)((StackPanel)((ListViewItem)((StackPanel)sender).Children.ElementAt(0)).Content).Children.ElementAt(3)).Visibility = Visibility.Collapsed;
                //((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(0))).Visibility = Visibility.Collapsed;
                //((TextBlock)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(1))).Margin = new Thickness(30, 0, 0, 0);
                //((Image)(((StackPanel)(((ListViewItemPresenter)(e.OriginalSource)).Content)).Children.ElementAt(3))).Visibility = Visibility.Collapsed;
            }
            else if (e.OriginalSource is Image)
            {
                ((Image)((UIElementCollection)((StackPanel)((ListViewItem)((UIElementCollection)((StackPanel)((Image)e.OriginalSource).Parent).Children).ElementAt(0)).Content).Children).ElementAt(0)).Visibility = Visibility.Collapsed;
                ((TextBlock)((UIElementCollection)((StackPanel)((ListViewItem)((UIElementCollection)((StackPanel)((Image)e.OriginalSource).Parent).Children).ElementAt(0)).Content).Children).ElementAt(1)).Margin = new Thickness(30, 0, 0, 0);
                ((Image)((UIElementCollection)((StackPanel)((ListViewItem)((UIElementCollection)((StackPanel)((Image)e.OriginalSource).Parent).Children).ElementAt(0)).Content).Children).ElementAt(3)).Visibility = Visibility.Collapsed;
            }
            else if (e.OriginalSource is Grid)
            {
                ((Image)((UIElementCollection)((StackPanel)((ListViewItem)((UIElementCollection)((StackPanel)sender).Children).ElementAt(0)).Content).Children).ElementAt(0)).Visibility = Visibility.Collapsed;
                ((TextBlock)((UIElementCollection)((StackPanel)((ListViewItem)((UIElementCollection)((StackPanel)sender).Children).ElementAt(0)).Content).Children).ElementAt(1)).Margin = new Thickness(30, 0, 0, 0);
                ((Image)((UIElementCollection)((StackPanel)((ListViewItem)((UIElementCollection)((StackPanel)sender).Children).ElementAt(0)).Content).Children).ElementAt(3)).Visibility = Visibility.Collapsed;
            }
            else
            {

            }
        }
    }
}