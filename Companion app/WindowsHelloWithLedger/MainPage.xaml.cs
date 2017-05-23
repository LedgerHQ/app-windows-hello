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
        //public string deviceGUID { get; set; }
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

            DeviceListBox.SelectionChanged += DeviceListBox_SelectionChanged;
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
            listContent listItem;
            int cpt = 0;
            for (int index = 0; index < deviceList.Count; ++index)
            {
                SecondaryAuthenticationFactorInfo deviceInfo = deviceList.ElementAt(index);
                listItem = new listContent();
                listItem.deviceFriendlyName = deviceInfo.DeviceFriendlyName;
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
                        if (deviceDlockState[0] == 0)
                        {
                            deviceConfigString = deviceId + "-0-0-" + deviceFriendlyName;
                        }
                        else
                        {
                            deviceConfigString = deviceId + "-1-0-" + deviceFriendlyName;
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
                        listItem.isVisible = false;
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
                    m_selectedDeviceId = ((WindowsHelloWithLedger.listContent)DeviceListBox.SelectedItem).deviceFriendlyName;

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
                System.Diagnostics.Debug.WriteLine("[DeviceListBox_SelectionChanged] The device " + m_selectedDeviceId + " is selected.");
                //SecondaryAuthenticationFactorInfo info = DeviceListBox.FindName("HelloDevice");



                if (deviceList.Count() != 0)
                {
                    for (int i = 0; i < deviceList.Count(); i++)
                    {
                        if (m_selectedDeviceId == deviceList.ElementAt(i).DeviceFriendlyName)
                        {
                            m_selectedDeviceId = deviceList.ElementAt(i).DeviceId;
                        }
                    }
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
            listItem.deviceFriendlyName = deviceFriendlyName;
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
    }
}