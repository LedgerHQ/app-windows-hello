using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Security.Authentication.Identity.Provider;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using WindowsHelloWithLedger;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data;
using Windows.Foundation;
using Windows.UI.Core;
using System.Numerics;
using Windows.System.UserProfile;
using Windows.Storage;
using System.Runtime.InteropServices;

namespace Tasks
{
    public sealed class myBGTask : IBackgroundTask
    {
        ManualResetEvent opCompletedEvent = null;
        //List<registeredDevice> presentRegisteredDevicesList = new List<registeredDevice>();

        private class registeredDevice
        {
            bool isPresent;
            string guid;

            public registeredDevice(string id)
            {
                isPresent = true;
                guid = id;
            }
            public string getGuid()
            {
                return guid;
            }
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            
            // This event is signaled when the operation completes
            opCompletedEvent = new ManualResetEvent(false);
            SecondaryAuthenticationFactorAuthentication.AuthenticationStageChanged += OnStageChanged;
            //ShowToastNotification("BG Task Hit!");        
            if (taskInstance.TriggerDetails is DeviceWatcherTriggerDetails)
            {
                DeviceWatcherTriggerDetails triggerDetails = (DeviceWatcherTriggerDetails)taskInstance.TriggerDetails;
                //Debugger.Break();

                foreach (DeviceWatcherEvent e in triggerDetails.DeviceWatcherEvents)
                {
                    switch (e.Kind)
                    {
                        case DeviceWatcherEventKind.Add:
                            Debug.WriteLine("[RUN] Add: " + e.DeviceInformation.Id);
                            //ShowToastNotification("[RUN] Add: " + e.DeviceInformation.Id);
                            //Debugger.Break();
                            //var tuple = await isPluggedDeviceRegisteredAsync();
                            //if (tuple.Item1 == true)
                            //{
                            //    bool alreadyInList = false;
                            //    foreach (registeredDevice device in presentRegisteredDevicesList)
                            //    {
                            //        if (tuple.Item2 == device.getGuid())
                            //        {
                            //            alreadyInList = true;
                            //            break;
                            //        }
                            //    }
                            //    if (!alreadyInList)
                            //    {
                            //        Debugger.Break();
                            //        registeredDevice deviceToAdd = new registeredDevice(tuple.Item2);
                            //        presentRegisteredDevicesList.Add(deviceToAdd);
                            //    }
                            //}                           

                            SecondaryAuthenticationFactorAuthenticationStageInfo authStageInfo = await SecondaryAuthenticationFactorAuthentication.GetAuthenticationStageInfoAsync();
                            if ((authStageInfo.Stage == SecondaryAuthenticationFactorAuthenticationStage.WaitingForUserConfirmation)
                                || (authStageInfo.Stage == SecondaryAuthenticationFactorAuthenticationStage.CollectingCredential))
                            //if (authStageInfo.Stage == SecondaryAuthenticationFactorAuthenticationStage.WaitingForUserConfirmation)
                            {
                                //String deviceName = deviceList.ElementAt(deviceList.Count() - 1).DeviceFriendlyName;

                                //await SecondaryAuthenticationFactorAuthentication.ShowNotificationMessageAsync(
                                //    "test",
                                //    SecondaryAuthenticationFactorAuthenticationMessage.SwipeUpWelcome);
                                System.Diagnostics.Debug.WriteLine("[RUN] Perform Auth / plug trigger");
                                PerformAuthentication();
                            }                            
                            break;

                        case DeviceWatcherEventKind.Update:
                            //Debug.WriteLine("[RUN] Update: " + e.DeviceInformationUpdate.Id);
                            break;

                        case DeviceWatcherEventKind.Remove:
                            Debug.WriteLine("[RUN] Remove: " + e.DeviceInformationUpdate.Id);
                            //ShowToastNotification("[RUN] Remove: " + e.DeviceInformationUpdate.Id);
                            //tuple = await isPluggedDeviceRegisteredAsync();
                            //Debugger.Break();
                            //await LockDevice();
                            break;
                    }
                }
            }
            // Wait until the operation completes
            opCompletedEvent.WaitOne();

            deferral.Complete();           
        }        
        private async Task AuthenticateWithSmartCardAsync(SmartCard card)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            String m_selectedDeviceId = localSettings.Values["SelectedDevice"] as String;

            bool foundCompanionDevice = false;

            byte[] response = { 0 };
            string sw1sw2 = null;

            SecondaryAuthenticationFactorAuthenticationStageInfo authStageInfo = await SecondaryAuthenticationFactorAuthentication.GetAuthenticationStageInfoAsync();

            if (authStageInfo.Stage != SecondaryAuthenticationFactorAuthenticationStage.CollectingCredential)
            {
                //Debugger.Break();
                //throw new Exception("Unexpected! Stage: " + authStageInfo.Stage);
            }
            //ShowToastNotification("Post Collecting Credential");
            System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Post Collecting Credential");
            IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                    SecondaryAuthenticationFactorDeviceFindScope.AllUsers);
            if (deviceList.Count == 0)
            {
                //ShowToastNotification("Unexpected exception, device list = 0");
                throw new Exception("Unexpected exception, device list = 0");
            }

            SmartCardConnection connection = await card.ConnectAsync();
            System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Connection");
            

            response = await Apdu.TransmitApduAsync(connection, Apdu.getDeviceGuidCmdApdu);
            sw1sw2 = Apdu.ApduResponseParser(response, out response);

            string deviceId = BitConverter.ToString(response).Replace("-", "");

            string deviceFriendlyName = null;
            //Debugger.Break();
            for (int i = 0; i < deviceList.Count(); i++)
            {
                deviceFriendlyName = deviceList.ElementAt(i).DeviceFriendlyName;
                if (deviceList.ElementAt(i).DeviceId == deviceId)
                {
                    m_selectedDeviceId = deviceId;
                    foundCompanionDevice = true;
                    //Debugger.Break();
                    break;
                }
            }

            if (!foundCompanionDevice)
            {
                throw new CompanionDeviceNotFoundException();
            }
            System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Start Nonce APDU sending");
            response = await Apdu.TransmitApduAsync(connection, Apdu.getNonceCmdApdu);
            sw1sw2 = Apdu.ApduResponseParser(response, out response);
            if (sw1sw2 != "9000")
            {
                throw new UnableTogetNonceFromDeviceException();
            }
            System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Nonce APDU recieved without error");

            string nonce = BitConverter.ToString(response).Replace("-", "");
            IBuffer svcNonce = CryptographicBuffer.DecodeFromHexString(nonce);

            System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Start Authentication");
            SecondaryAuthenticationFactorAuthenticationResult authResult = await SecondaryAuthenticationFactorAuthentication.StartAuthenticationAsync(
                m_selectedDeviceId, svcNonce);
            //Debugger.Break();
            if (authResult.Status != SecondaryAuthenticationFactorAuthenticationStatus.Started)
            {
                //ShowToastNotification("Unexpected! Could not start authentication!");
                throw new Exception("Unexpected! Could not start authentication! Status: " + authResult.Status);
            }

            byte[] devNonce = { 0 };
            byte[] svcHmac = { 0 };
            byte[] sessNonce = { 0 };

            CryptographicBuffer.CopyToByteArray(authResult.Authentication.ServiceAuthenticationHmac, out svcHmac);
            CryptographicBuffer.CopyToByteArray(authResult.Authentication.SessionNonce, out sessNonce);
            CryptographicBuffer.CopyToByteArray(authResult.Authentication.DeviceNonce, out devNonce);

            byte[] cmd = new byte[Apdu.challengeCmdApdu.Length + svcHmac.Length + sessNonce.Length + devNonce.Length];
            System.Buffer.BlockCopy(Apdu.challengeCmdApdu, 0, cmd, 0, Apdu.challengeCmdApdu.Length);
            System.Buffer.BlockCopy(svcHmac, 0, cmd, Apdu.challengeCmdApdu.Length, svcHmac.Length);
            System.Buffer.BlockCopy(sessNonce, 0, cmd, Apdu.challengeCmdApdu.Length + svcHmac.Length, sessNonce.Length);
            System.Buffer.BlockCopy(devNonce, 0, cmd, Apdu.challengeCmdApdu.Length + svcHmac.Length + sessNonce.Length, devNonce.Length);

            System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Send Challenge");
            // Notification device needs attention                        
            await SecondaryAuthenticationFactorAuthentication.ShowNotificationMessageAsync(
                deviceFriendlyName,
                SecondaryAuthenticationFactorAuthenticationMessage.DeviceNeedsAttention);
            // Notification device needs attention end

            response = await Apdu.TransmitApduAsync(connection, cmd);
            sw1sw2 = Apdu.ApduResponseParser(response, out response);

            if (sw1sw2 == "6985")
            {
                throw new UnauthorizedUserException();
            }
            else if (sw1sw2 == "6984")
            {
                //ShowToastNotification("Log-in denied by user");
                throw new LogInDeniedByUserException();
            }

            System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Response recieved");
            byte[] HMACdk = new byte[32];
            byte[] HMACsk = new byte[32];
            System.Buffer.BlockCopy(response, 0, HMACdk, 0, 32);
            System.Buffer.BlockCopy(response, 32, HMACsk, 0, 32);

            IBuffer deviceHmac = CryptographicBuffer.CreateFromByteArray(HMACdk);
            IBuffer sessionHmac = CryptographicBuffer.CreateFromByteArray(HMACsk);

            SecondaryAuthenticationFactorFinishAuthenticationStatus authStatus = await authResult.Authentication.FinishAuthenticationAsync(deviceHmac,
                sessionHmac);

            if (authStatus != SecondaryAuthenticationFactorFinishAuthenticationStatus.Completed)
            {
                //ShowToastNotification("Unable to complete authentication!");
                System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Unable to complete authentication");
                throw new Exception("Unable to complete authentication!");
            }
            System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Auth completed");
            //return 0;
        }

        async void PerformAuthentication()
        {
            string NanosATR = "3b00";
            bool showNotificationFlag = true;

            string selector = SmartCardReader.GetDeviceSelector();

            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);             

            foreach (DeviceInformation device in devices)
            {
                SmartCardReader reader = await SmartCardReader.FromIdAsync(device.Id);
                SmartCardReaderStatus readerstatus = await reader.GetStatusAsync();
                //System.Diagnostics.Debug.WriteLine("Reader : " + reader.Name + " status : " + readerstatus.ToString());
                IReadOnlyList<SmartCard> cards = await reader.FindAllCardsAsync();

                foreach (SmartCard card in cards)
                {
                    try
                    {
                        IBuffer ATR = await card.GetAnswerToResetAsync();
                        string ATR_str = CryptographicBuffer.EncodeToHexString(ATR);

                        if (ATR_str.Equals(NanosATR))
                        {
                            await AuthenticateWithSmartCardAsync(card);
                        }
                    }
                    catch (CompanionDeviceNotFoundException ex)
                    {
                        ex.DisplayError();
                        //await SecondaryAuthenticationFactorAuthentication.ShowNotificationMessageAsync(
                        //    "a registered companion device",
                        //    SecondaryAuthenticationFactorAuthenticationMessage.LookingForDevicePluggedin);
                        //showNotificationFlag = false;
                        break;
                    }
                    catch (UnableTogetNonceFromDeviceException ex)
                    {
                        ex.DisplayError();
                        showNotificationFlag = false;
                        break;
                    }
                    catch (UnauthorizedUserException ex)
                    {
                        ex.DisplayError();
                        await SecondaryAuthenticationFactorAuthentication.ShowNotificationMessageAsync(
                            "",
                            SecondaryAuthenticationFactorAuthenticationMessage.UnauthorizedUser);
                        //ShowToastNotification("Wrong Response");
                        showNotificationFlag = false;
                        break;
                    }
                    catch (LogInDeniedByUserException ex)
                    {
                        ex.DisplayError();
                        await SecondaryAuthenticationFactorAuthentication.ShowNotificationMessageAsync(
                            "",
                            SecondaryAuthenticationFactorAuthenticationMessage.TryAgain);
                        showNotificationFlag = false;
                        break;
                    }
                    catch (Exception ex)
                    {
                        //Debugger.Break();
                        System.Diagnostics.Debug.WriteLine("[PerformAuthentication] Unhandled Exception / " + ex.Message);
                        showNotificationFlag = false;
                        return;
                    }
                    finally
                    {

                    }                                     
                }
            }
            if (showNotificationFlag)
            {
                await SecondaryAuthenticationFactorAuthentication.ShowNotificationMessageAsync(
                                "a registered companion device",
                                SecondaryAuthenticationFactorAuthenticationMessage.LookingForDevicePluggedin);
            }
            showNotificationFlag = true;
        }
        private async Task<Tuple<bool, string>> isPluggedDeviceRegisteredAsync()
        {
            bool isRegistered = false;
            string guid = "no device";

            string NanosATR = "3b00";

            string selector = SmartCardReader.GetDeviceSelector();

            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

            foreach (DeviceInformation device in devices)
            {
                SmartCardReader reader = await SmartCardReader.FromIdAsync(device.Id);
                SmartCardReaderStatus readerstatus = await reader.GetStatusAsync();
                //System.Diagnostics.Debug.WriteLine("Reader : " + reader.Name + " status : " + readerstatus.ToString());
                IReadOnlyList<SmartCard> cards = await reader.FindAllCardsAsync();

                foreach (SmartCard card in cards)
                {
                    IBuffer ATR = await card.GetAnswerToResetAsync();
                    string ATR_str = CryptographicBuffer.EncodeToHexString(ATR);
                    Debugger.Break();
                    if (ATR_str.Equals(NanosATR))
                    {
                        SmartCardConnection connection = await card.ConnectAsync();

                        byte[] response = { 0 };
                        string sw1sw2 = null;

                        response = await Apdu.TransmitApduAsync(connection, Apdu.getDeviceGuidCmdApdu);
                        sw1sw2 = Apdu.ApduResponseParser(response, out response);

                        string deviceId = BitConverter.ToString(response).Replace("-", "");

                        IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                            SecondaryAuthenticationFactorDeviceFindScope.AllUsers);
                        for (int i = 0; i < deviceList.Count(); i++)
                        {
                            if (deviceList.ElementAt(i).DeviceId == deviceId)
                            {
                                guid = deviceId;
                                isRegistered = true;
                                break;
                            }
                        }
                    }
                }
            }
            return new Tuple<bool, string>(isRegistered, guid);
        }
        //private async Task LockDevice()
        //{
        //    // Query the devices which can do presence check for the console user
        //    IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceInfoList =
        //        await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(SecondaryAuthenticationFactorDeviceFindScope.User);

        //    if (deviceInfoList.Count == 0)
        //    {
        //        return;
        //    }

        //    foreach (SecondaryAuthenticationFactorInfo deviceInfo in deviceInfoList)
        //    {
        //        if (deviceInfo.PresenceMonitoringMode !=
        //                SecondaryAuthenticationFactorDevicePresenceMonitoringMode.AppManaged)
        //        {
        //            // Skip the device which doesn't need to be monitored in the background task
        //            continue;
        //        }

        //        SecondaryAuthenticationFactorDevicePresence state =
        //            SecondaryAuthenticationFactorDevicePresence.Absent;

        //        await deviceInfo.UpdateDevicePresenceAsync(state);
        //        //string buf = deviceInfo.DeviceConfigurationData.ToString();
        //        SecondaryAuthenticationFactorDevicePresenceMonitoringMode mode = deviceInfo.PresenceMonitoringMode;
        //    }
        //}
        public static void ShowToastNotification(string message)
        {

            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            // Set Text
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(message));

            // Set image
            // Images must be less than 200 KB in size and smaller than 1024 x 1024 pixels.
            XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///Images/logo-80px-80px.png");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("alt", "logo");

            // toast duration
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");

            // toast navigation
            var toastNavigationUriString = "#/MainPage.xaml?param1=12345";
            var toastElement = ((XmlElement)toastXml.SelectSingleNode("/toast"));
            toastElement.SetAttribute("launch", toastNavigationUriString);

            // Create the toast notification based on the XML content you've specified.
            ToastNotification toast = new ToastNotification(toastXml);

            // Send your toast notification.
            ToastNotificationManager.CreateToastNotifier().Show(toast);

        }        
        //private void Reader_CardAdded(object sender, CardAddedEventArgs e)
        //{
        //    Debugger.Break();
        //    PerformAuthentication();
        //}
        // WARNING: Test code
        // This code should be in background task
        async void OnStageChanged(Object sender, SecondaryAuthenticationFactorAuthenticationStageChangedEventArgs args)
        {
            //ShowToastNotification("In StageChanged!" + args.StageInfo.Stage.ToString());
            if (args.StageInfo.Stage == SecondaryAuthenticationFactorAuthenticationStage.WaitingForUserConfirmation)
            {
                //ShowToastNotification("Stage = WaitingForUserConfirmation");
                // This event is happening on a ThreadPool thread, so we need to dispatch to the UI thread.
                // Getting the dispatcher from the MainView works as long as we only have one view.
               

                IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                            SecondaryAuthenticationFactorDeviceFindScope.User);

                String deviceName = deviceList.ElementAt(deviceList.Count()-1).DeviceFriendlyName;               

                await SecondaryAuthenticationFactorAuthentication.ShowNotificationMessageAsync(
                    deviceName,
                    SecondaryAuthenticationFactorAuthenticationMessage.SwipeUpWelcome);
            }
            else if (args.StageInfo.Stage == SecondaryAuthenticationFactorAuthenticationStage.CollectingCredential)
            {
                //ShowToastNotification("Stage = CollectingCredential");
                System.Diagnostics.Debug.WriteLine("[OnStageChanged] Perform Auth / auth trigger");
                PerformAuthentication();
            }
            else
            {
                if (args.StageInfo.Stage == SecondaryAuthenticationFactorAuthenticationStage.StoppingAuthentication)
                {
                    SecondaryAuthenticationFactorAuthentication.AuthenticationStageChanged -= OnStageChanged;
                    opCompletedEvent.Set();
                }

                SecondaryAuthenticationFactorAuthenticationStage stage = args.StageInfo.Stage;
            }
        }
    }
    internal class CompanionDeviceNotFoundException : Exception
    {
        DateTime m_errorTime;
        static ushort s_errorNumber;

        public CompanionDeviceNotFoundException()
            : base("Companion device not found")
        {
            m_errorTime = DateTime.Now;
            s_errorNumber++;
        }
        public CompanionDeviceNotFoundException(string message)
            : base(message)
        {
            m_errorTime = DateTime.Now;
            s_errorNumber++;
        }
        public void DisplayError()
        {
            Debug.WriteLine("CompanionDeviceNotFoundException n." + s_errorNumber + " Date: " + m_errorTime);
        }
    }
    internal class UnableTogetNonceFromDeviceException : Exception
    {
        DateTime m_errorTime;
        static short s_errorNumber;

        public UnableTogetNonceFromDeviceException()
            : base("Unable to get GUID from device")
        {
            m_errorTime = DateTime.Now;
            s_errorNumber++;
        }

        public UnableTogetNonceFromDeviceException(string message)
            : base(message)
        {
            m_errorTime = DateTime.Now;
            s_errorNumber++;
        }
        public void DisplayError()
        {
            Debug.WriteLine("UnableTogetNonceFromDeviceException n." + s_errorNumber + " Date: " + m_errorTime);
        }
    }
    internal class LogInDeniedByUserException : Exception
    {
        DateTime m_errorTime;
        static short s_errorNumber;

        public LogInDeniedByUserException()
            : base("Log in denied by user")
        {
            m_errorTime = DateTime.Now;
            s_errorNumber++;
        }
        public LogInDeniedByUserException(string message)
            : base(message)
        {
            m_errorTime = DateTime.Now;
            s_errorNumber++;
        }
        public void DisplayError()
        {
            Debug.WriteLine("LogInDeniedByUserException n." + s_errorNumber + " Date: " + m_errorTime);
        }
    }
    internal class UnauthorizedUserException : Exception
    {
        DateTime m_errorTime;
        static short s_errorNumber;

        public UnauthorizedUserException()
            : base("Unauthorized User")
        {
            m_errorTime = DateTime.Now;
            s_errorNumber++;
        }
        public UnauthorizedUserException(string message)
            : base(message)
        {
            m_errorTime = DateTime.Now;
            s_errorNumber++;
        }
        public void DisplayError()
        {
            Debug.WriteLine("UnauthorizedUserException n." + s_errorNumber + " Date: " + m_errorTime);
        }
    }
}
