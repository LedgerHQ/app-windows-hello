using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Security.Authentication.Identity.Provider;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LedgerHello
{
    
    class Apdu
    {
        // Apdu commands to be used as parameter in TransmitApduAsync method
        public static readonly byte[] getDeviceGuidCmdApdu = { 0x80, 0xCA, 0x00, 0x00, 0x00 };
        public static readonly byte[] startRegistrationCmdApdu = { 0x80, 0xCA, 0x01, 0x00, 0x00 };
        public static readonly byte[] getDlockStateCmdApdu = { 0x80, 0xCA, 0x02, 0x00, 0x00 };
        public static readonly byte[] getNonceCmdApdu = { 0x80, 0x84, 0x00, 0x00, 0x00 };
        public static readonly byte[] challengeCmdApdu = { 0x80, 0x82, 0x00, 0x00, 0x60 };        

        public static async Task<byte[]> TransmitApduAsync(SmartCardConnection connection, byte[] ApduCommand)
        {
            IBuffer command = CryptographicBuffer.CreateFromByteArray(ApduCommand);
            IBuffer response = await connection.TransmitAsync(command);
            return response.ToArray();
        }
        public static string ApduResponseParser(byte[] ApduResponse, out byte[] Data)
        {
            byte[] Sw1Sw2 = { 0, 0 };
            Data = null;

            //string test;
            if (ApduResponse.Length > 2)
            {
                Data = new byte[ApduResponse.Length - 2];
                for (int i = 0; i < ApduResponse.Length - 2; i++)
                {
                    Data[i] = ApduResponse[i];
                }
                Sw1Sw2[0] = ApduResponse[ApduResponse.Length - 2];
                Sw1Sw2[1] = ApduResponse[ApduResponse.Length - 1];
            }
            else
            {
                Sw1Sw2 = ApduResponse;
            }

            return BitConverter.ToString(Sw1Sw2).Replace("-", "");
        }
    }
    class listContent
    {
        public string deviceFriendlyName { get; set; }
        public DateTime date { get; set; }
        public string dateString { get; set; }
        public string deviceGUID { get; set; }
        public bool isVisible { get; set; }
    }
    class CommomMethods
    {
        public static void SetWindowSize()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(480, 485));            
            ApplicationView.PreferredLaunchViewSize = new Size(480, 485);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            //ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            //compactOptions.CustomSize = new Windows.Foundation.Size(480, 485);
            //compactOptions.ViewSizePreference = ViewSizePreference.Custom;
            //await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);            
        }

        public static async Task RegisterDevice_Click(string deviceFriendlyName)
        {
            String deviceId = "";
            IBuffer deviceKey = CryptographicBuffer.GenerateRandom(32);
            IBuffer authKey = CryptographicBuffer.GenerateRandom(32);
            byte[] deviceKeyArray = new byte[32];
            byte[] authKeyArray = new byte[32];
            byte[] deviceIdArray = new byte[16];
            byte[] deviceDlockState = new byte[1];
            byte[] response = { 0 };
            int numberOfDevices = 0;
            int numberOfRegisteredDevices = 0;
            string sw1sw2 = null;
            //byte[] combinedDataArray = new byte[64];
            string NanosATR = "3b00";
            String deviceModelNumber = "0001";
            //List<SmartCardListItem> cardItems = new List<SmartCardListItem>();
            MessageDialog myDlg;

            bool isSupported;
            isSupported = await KeyCredentialManager.IsSupportedAsync();           

            if (!isSupported)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                string PleaseSetUpPinContent = loader.GetString("PleaseSetupPin_content_error");
                string PleaseSetUpPinTitle = loader.GetString("PleaseSetupPin_title_error");

                myDlg = new MessageDialog(PleaseSetUpPinContent, PleaseSetUpPinTitle);
                await myDlg.ShowAsync();
                return;
            }

            IReadOnlyList<User> users = await User.FindAllAsync(UserType.LocalUser, UserAuthenticationStatus.LocallyAuthenticated);
            string userId = users.ElementAt(0).NonRoamableId;

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
                        numberOfDevices++;
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
                                //deviceFriendlyName = registeredDeviceList.ElementAt(i).DeviceFriendlyName;
                                numberOfRegisteredDevices++;
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

                        connection = await card.ConnectAsync();

                        response = await Apdu.TransmitApduAsync(connection, Apdu.getDlockStateCmdApdu);
                        sw1sw2 = Apdu.ApduResponseParser(response, out response);
                        deviceDlockState = response;

                        response = await Apdu.TransmitApduAsync(connection, Apdu.startRegistrationCmdApdu);
                        sw1sw2 = Apdu.ApduResponseParser(response, out response);

                        connection.Dispose();

                        if (sw1sw2 != "9000")
                        {
                            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                            string RegistrationDeniedContent = loader.GetString("RegsitrationDenied_content_error");
                            string RegistrationDeniedTitle = loader.GetString("RegsitrationDenied_title_error");

                            myDlg = null;
                            myDlg = new MessageDialog(RegistrationDeniedContent, RegistrationDeniedTitle);
                            await myDlg.ShowAsync();                            
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
                            authKeyArray[index] = response[index + 32];
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
                        //DateTime addDate = new DateTime(2017, 5, 31, 13, 23, 45);
                        if (deviceDlockState[0] == 0)
                        {
                            deviceConfigString = deviceId + "-0-0-" + deviceFriendlyName + "-" + addDate.ToString() + "-" + userId;
                        }
                        else
                        {
                            deviceConfigString = deviceId + "-1-0-" + deviceFriendlyName + "-" + addDate.ToString() + "-" + userId;
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
                                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                                var str = loader.GetString("PleaseSetupPin_error");
                                myDlg = new MessageDialog(str);
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
                        //listItem.isVisible = false;
                        listItem.date = addDate;
                        listItem.dateString = FormatDate(addDate);                        
                        //DeviceListBox.Items.Add(listItem);
                        //StartWatcher();
                        //this.Frame.Navigate(typeof(MainPage), "false");
                    }
                }
            }
            if (numberOfDevices == numberOfRegisteredDevices)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                string str = loader.GetString("DeviceAlreadyRegistered_content_error");


                throw new Exception(str);
                //myDlg = new MessageDialog("Ledger Nano-s for Windows Hello not found" + Environment.NewLine + Environment.NewLine + "Please plug a ledger Nano-s in a usb port");
                //await myDlg.ShowAsync();
                //return;
            }
            return;
        }
        public static string FormatDate(DateTime dateToFormat)
        {
            string dateString = string.Empty;
            DateTime now = DateTime.Now;
            if ((now.DayOfYear - dateToFormat.DayOfYear == 0) && (dateToFormat.Year - now.Year == 0))
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var str = loader.GetString("Today");

                if ((dateToFormat.TimeOfDay.Hours) > 12)
                {
                    dateString = str + (dateToFormat.TimeOfDay.Hours - 12) + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " PM";
                }
                else
                {
                    dateString = str + dateToFormat.TimeOfDay.Hours + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " AM";
                }
            }
            else if ((now.DayOfYear - dateToFormat.DayOfYear == 1) && (dateToFormat.Year - now.Year == 0))
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var str = loader.GetString("Yesterday");
                if ((dateToFormat.TimeOfDay.Hours) > 12)
                {
                    dateString = str + (dateToFormat.TimeOfDay.Hours - 12) + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " PM";
                }
                else
                {
                    dateString = str + dateToFormat.TimeOfDay.Hours + ":" + dateToFormat.TimeOfDay.Minutes.ToString("00") + " AM";
                }
            }
            else
            {
                int month = dateToFormat.Month;
                string monthString = string.Empty;
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                switch (month)
                {
                    case 1:                        
                        monthString = loader.GetString("January");                        
                        break;
                    case 2:
                        monthString = loader.GetString("February");
                        break;
                    case 3:
                        monthString = loader.GetString("March");
                        break;
                    case 4:
                        monthString = loader.GetString("April");
                        break;
                    case 5:
                        monthString = loader.GetString("May");
                        break;
                    case 6:
                        monthString = loader.GetString("June");
                        break;
                    case 7:
                        monthString = loader.GetString("July");
                        break;
                    case 8:
                        monthString = loader.GetString("August");
                        break;
                    case 9:
                        monthString = loader.GetString("September");
                        break;
                    case 10:
                        monthString = loader.GetString("October");
                        break;
                    case 11:
                        monthString = loader.GetString("November");
                        break;
                    case 12:
                        monthString = loader.GetString("December");
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
    }
    class Textbox
    {
        public static async Task<string> InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            dialog.PrimaryButtonText = loader.GetString("OK");
            dialog.SecondaryButtonText = loader.GetString("Cancel");
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }
    }
}
