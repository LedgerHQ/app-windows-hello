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
using System.IO;

namespace Tasks
{
    public sealed class dLock
    {
        public string DeviceId { get; set; }
        public bool isDlockEnabled { get; set; }

        public dLock(string id, bool state)
        {
            DeviceId = id;
            isDlockEnabled = state;
        }
    }

    public sealed class authBGTask : IBackgroundTask
    {
        ManualResetEvent opCompletedEvent = null;
        BackgroundTaskDeferral deferral;

        //static List<dLock> pluggedRegisteredDeviceList = new List<dLock>();
        //static List<dLock> pluggedRegisteredDeviceListAfterRemove = new List<dLock>();
        //private static Windows.Storage.StorageFolder storageFolder;
        //private static Windows.Storage.StorageFile sampleFile;
        //private static Windows.Storage.StorageFile deviceListsFile;
        
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            //Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            //Windows.Storage.StorageFile sampleFile = await storageFolder.CreateFileAsync("logs.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
            //Windows.Storage.StorageFile deviceListsFile = await storageFolder.CreateFileAsync("deviceLists.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
            string txt = "";

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
                            //storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                            //sampleFile = await storageFolder.CreateFileAsync("logs.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                            //txt = await Windows.Storage.FileIO.ReadTextAsync(sampleFile) + Environment.NewLine ;
                            //await Windows.Storage.FileIO.WriteTextAsync(sampleFile, txt + "[RUN] Add: " + e.DeviceInformation.Id);
                            //Debugger.Break();
                            IReadOnlyList<SecondaryAuthenticationFactorInfo> RegisteredDeviceList_addEvent = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                                SecondaryAuthenticationFactorDeviceFindScope.AllUsers);
                            await UpdateDevicesConfigData(RegisteredDeviceList_addEvent);
                            if (e.DeviceInformation.Name.Contains("Ledger Nano S"))
                            {
                                //storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                                //deviceListsFile = await storageFolder.CreateFileAsync("deviceLists.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                                //pluggedRegisteredDeviceList = await getPluggedRegisteredDeviceListAsync(null);
                            }

                            //Debug.WriteLine("\tNbDevices = " + pluggedRegisteredDeviceList.Count());
                            //txt = await Windows.Storage.FileIO.ReadTextAsync(sampleFile) + Environment.NewLine;
                            //await Windows.Storage.FileIO.WriteTextAsync(sampleFile, txt + "\tNbDevices = " + pluggedRegisteredDeviceList.Count());

                            SecondaryAuthenticationFactorAuthenticationStageInfo authStageInfo = await SecondaryAuthenticationFactorAuthentication.GetAuthenticationStageInfoAsync();
                            if ((authStageInfo.Stage == SecondaryAuthenticationFactorAuthenticationStage.WaitingForUserConfirmation)
                                || (authStageInfo.Stage == SecondaryAuthenticationFactorAuthenticationStage.CollectingCredential))
                            {
                                System.Diagnostics.Debug.WriteLine("[RUN] Perform Auth / plug trigger");
                                PerformAuthentication();
                            }                            
                            break;

                        case DeviceWatcherEventKind.Update:
                            Debug.WriteLine("[RUN] Update: " + e.DeviceInformationUpdate.Id);
                            //storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                            //sampleFile = await storageFolder.CreateFileAsync("logs.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                            //txt = await Windows.Storage.FileIO.ReadTextAsync(sampleFile) + Environment.NewLine;
                            //await Windows.Storage.FileIO.WriteTextAsync(sampleFile,txt + "[RUN] Update: " + e.DeviceInformationUpdate.Id);
                            break;

                        case DeviceWatcherEventKind.Remove:
                            Debug.WriteLine("[RUN] Remove: " + e.DeviceInformationUpdate.Id);
                            txt = "[RUN] Remove: " + e.DeviceInformationUpdate.Id;
                            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                            StorageFile logsFile = await folder.CreateFileAsync("test.txt", CreationCollisionOption.OpenIfExists);
                            await FileIO.WriteTextAsync(logsFile, txt);

                            IReadOnlyList<SecondaryAuthenticationFactorInfo> registeredDeviceList_removeEvent = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                                SecondaryAuthenticationFactorDeviceFindScope.AllUsers);
                            txt = Environment.NewLine + "nb registered devices = " + registeredDeviceList_removeEvent.Count();
                            await FileIO.WriteTextAsync(logsFile, txt);

                            List<SecondaryAuthenticationFactorInfo> list = await AreRegisteredDeviceConnected(registeredDeviceList_removeEvent);

                            txt = Environment.NewLine + "nb registered connected devices = " + list.Count();
                            await FileIO.WriteTextAsync(logsFile, txt);
                            if (list.Count() == 0)
                            {
                                txt = Environment.NewLine + "LOCK" ;
                                await FileIO.WriteTextAsync(logsFile, txt);
                                await LockDevice();
                                txt = Environment.NewLine + "2LOCK";
                                await FileIO.WriteTextAsync(logsFile, txt);
                            }
                            //await FileIO.WriteTextAsync(logsFile, txt);
                            break;
                    }
                }
            }
            else
            {
                Debug.WriteLine("[RUN] Unknown trigger");
                //storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                //deviceListsFile = await storageFolder.CreateFileAsync("deviceLists.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                //pluggedRegisteredDeviceList = await getPluggedRegisteredDeviceListAsync(null);
                //Debugger.Break();
            }
            // Wait until the operation completes
            opCompletedEvent.WaitOne();

            deferral.Complete();           
        }
        private async Task UpdateDevicesConfigData(IReadOnlyList<SecondaryAuthenticationFactorInfo> devicesToUpdate)
        {
            string NanosATR = "3b00";
            string selector = SmartCardReader.GetDeviceSelector();
            selector += " AND System.Devices.DeviceInstanceId:~~\"Ledger\"";
            byte[] response = { 0 };
            string sw1sw2 = null;
            byte[] deviceDlockState = new byte[1];
            byte[] deviceIdArray = new byte[16];
            byte[] deviceConfigurationDataArray;

            DeviceInformationCollection readers = await DeviceInformation.FindAllAsync(selector);

            foreach (SecondaryAuthenticationFactorInfo device in devicesToUpdate)
            {
                //Debugger.Break();
                
                

                foreach (DeviceInformation smartcardreader in readers)
                {
                    //Debugger.Break();
                    SmartCardReader reader = await SmartCardReader.FromIdAsync(smartcardreader.Id);
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
                                SmartCardConnection connection = await card.ConnectAsync();
                                response = await Apdu.TransmitApduAsync(connection, Apdu.getDeviceGuidCmdApdu);
                                sw1sw2 = Apdu.ApduResponseParser(response, out response);
                                deviceIdArray = response;
                                string deviceId = BitConverter.ToString(response).Replace("-", "");                    
                                if (deviceId == device.DeviceId) //update config data with dLockState and increment counter
                                {
                                    CryptographicBuffer.CopyToByteArray(device.DeviceConfigurationData, out deviceConfigurationDataArray);
                                    //Debugger.Break();
                                    response = await Apdu.TransmitApduAsync(connection, Apdu.getDlockStateCmdApdu);
                                    sw1sw2 = Apdu.ApduResponseParser(response, out response);
                                    deviceDlockState = response;

                                    deviceConfigurationDataArray[16] = deviceDlockState[0];
                                    deviceConfigurationDataArray[17]++;

                                    IBuffer deviceConfigData = CryptographicBuffer.CreateFromByteArray(deviceConfigurationDataArray);
                                    await SecondaryAuthenticationFactorRegistration.UpdateDeviceConfigurationDataAsync(deviceId, deviceConfigData);
                                    //Debugger.Break();
                                }                                
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

        }
        private async Task<List<SecondaryAuthenticationFactorInfo>> AreRegisteredDeviceConnected(IReadOnlyList<SecondaryAuthenticationFactorInfo> devicesToCheck)
        {
            byte[] deviceConfigurationDataArray;
            string selector = SmartCardReader.GetDeviceSelector();
            selector += " AND System.Devices.DeviceInstanceId:~~\"Ledger\"";
            byte[] response = { 0 };
            string sw1sw2 = null;
            string NanosATR = "3b00";
            byte[] deviceDlockState = new byte[1];
            byte[] deviceIdArray = new byte[16];            

            List<SecondaryAuthenticationFactorInfo> outList = new List<SecondaryAuthenticationFactorInfo>();

            DeviceInformationCollection readers = await DeviceInformation.FindAllAsync(selector);

            foreach (SecondaryAuthenticationFactorInfo device in devicesToCheck)
            {
                CryptographicBuffer.CopyToByteArray(device.DeviceConfigurationData, out deviceConfigurationDataArray);

                foreach (DeviceInformation smartcardreader in readers)
                {
                    //Debugger.Break();
                    SmartCardReader reader = await SmartCardReader.FromIdAsync(smartcardreader.Id);
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
                                SmartCardConnection connection = await card.ConnectAsync();
                                response = await Apdu.TransmitApduAsync(connection, Apdu.getDeviceGuidCmdApdu);
                                sw1sw2 = Apdu.ApduResponseParser(response, out response);
                                deviceIdArray = response;
                                string deviceId = BitConverter.ToString(response).Replace("-", "");
                                if (deviceId == device.DeviceId) //update config data with dLockState and increment counter
                                {
                                    outList.Add(device);
                                    //Debugger.Break();
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }


                //Debugger.Break();
            }
            return outList;
        }
       /* private bool checkIfRemovedDeviceHasDlockEnabled()
        {
            List<dLock> listbkp = pluggedRegisteredDeviceList;
            foreach (dLock device in pluggedRegisteredDeviceListAfterRemove)
            {
                int idx = 0;
                foreach (dLock dev in listbkp)
                {
                    if (dev.DeviceId == device.DeviceId)
                    {
                        listbkp.RemoveAt(idx);
                    }
                    idx++;
                }
            }
            return listbkp[0].isDlockEnabled;
        }*/
        /*private async Task<List<dLock>> getPluggedRegisteredDeviceListAsync(StorageFile file)
        {
            System.Diagnostics.Debug.WriteLine("[getPluggedRegisteredDeviceListAsync] start listing devices");
            string NanosATR = "3b00";

            byte[] response = { 0 };
            string sw1sw2 = null;

            List<dLock> pluggedRegisteredDeviceList = new List<dLock>();

            IReadOnlyList<SecondaryAuthenticationFactorInfo> registeredDeviceList = await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(
                    SecondaryAuthenticationFactorDeviceFindScope.AllUsers);

            string selector = SmartCardReader.GetDeviceSelector();
            selector += " AND System.Devices.DeviceInstanceId:~~\"Ledger\"";
            IRandomAccessStream test;
            if (file != null)
            {
                test = await file.OpenAsync(FileAccessMode.ReadWrite);
                await test.FlushAsync();
                test.Dispose();
            }            

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
                            SmartCardConnection connection = await card.ConnectAsync();
                            response = await Apdu.TransmitApduAsync(connection, Apdu.getDeviceGuidCmdApdu);
                            sw1sw2 = Apdu.ApduResponseParser(response, out response);
                            string deviceId = BitConverter.ToString(response).Replace("-", "");

                            response = await Apdu.TransmitApduAsync(connection, Apdu.getDlockStateCmdApdu);
                            sw1sw2 = Apdu.ApduResponseParser(response, out response);

                            foreach (SecondaryAuthenticationFactorInfo registeredDevice in registeredDeviceList)
                            {
                                if (registeredDevice.DeviceId == deviceId)
                                {
                                    //pluggedRegisteredDeviceList.Add(deviceId);
                                    //pluggedRegisteredDeviceList = new Tuple<deviceId,true>();
                                    dLock deviceToAdd = new dLock(deviceId, true);
                                    if (response[0] == 0)
                                    {
                                        deviceToAdd.isDlockEnabled = false;
                                    }
                                    pluggedRegisteredDeviceList.Add(deviceToAdd);
                                    if (file != null)
                                    {
                                        await Windows.Storage.FileIO.WriteTextAsync(file, deviceToAdd.DeviceId + " " + deviceToAdd.isDlockEnabled + Environment.NewLine);
                                    }
                                    System.Diagnostics.Debug.WriteLine("[getPluggedRegisteredDeviceListAsync] found new device "+ deviceId + "dLockEnabled : " + pluggedRegisteredDeviceList.ElementAt(pluggedRegisteredDeviceList.Count()-1).isDlockEnabled.ToString());
                                }
                            }
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
                    catch (Exception ex)
                    {
                        //Debugger.Break();
                        System.Diagnostics.Debug.WriteLine("[getPluggedRegisteredDeviceListAsync] Unhandled Exception / " + ex.Message);
                        //showNotificationFlag = false;
                        break;
                    }
                    finally
                    {

                    }
                }
            }
            return pluggedRegisteredDeviceList;
        }*/
        private async Task AuthenticateWithSmartCardAsync(SmartCard card)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            String m_selectedDeviceId = localSettings.Values["SelectedDevice"] as String;
            byte[] deviceIdArray = new byte[16];
            byte[] deviceDlockState = new byte[1];

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
            deviceIdArray = response;
            string deviceId = BitConverter.ToString(response).Replace("-", "");

            response = await Apdu.TransmitApduAsync(connection, Apdu.getDlockStateCmdApdu);
            sw1sw2 = Apdu.ApduResponseParser(response, out response);
            deviceDlockState = response;

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

            byte[] deviceConfigDataArray = new byte[17]; //16 bytes for GUID and 1 byte for dLockstate

            for (int i = 0; i < 16; i++)
            {
                deviceConfigDataArray[i] = deviceIdArray[i];
            }
            deviceConfigDataArray[16] = deviceDlockState[0];
            IBuffer deviceConfigData = CryptographicBuffer.CreateFromByteArray(deviceConfigDataArray);
            //Update the device configuration 
            await SecondaryAuthenticationFactorRegistration.UpdateDeviceConfigurationDataAsync(deviceId, deviceConfigData);

            System.Diagnostics.Debug.WriteLine("[AuthenticateWithSmartCardAsync] Auth completed");
            //return 0;
        }

        async void PerformAuthentication()
        {
            string NanosATR = "3b00";
            bool showNotificationFlag = true;

            string selector = SmartCardReader.GetDeviceSelector();
            selector += " AND System.Devices.DeviceInstanceId:~~\"Ledger\"";
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
        private async Task LockDevice()
        {
            Debug.WriteLine("[LockDevice]");

            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile = await storageFolder.CreateFileAsync("logs.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);

            string txt = await Windows.Storage.FileIO.ReadTextAsync(sampleFile) + Environment.NewLine;
            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, txt + "[LockDevice]");
            //Debugger.Break();
            // Query the devices which can do presence check for the console user
            IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceInfoList =
                await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(SecondaryAuthenticationFactorDeviceFindScope.User);

            if (deviceInfoList.Count == 0)
            {
                return;
            }

            foreach (SecondaryAuthenticationFactorInfo deviceInfo in deviceInfoList)
            {
                if (deviceInfo.PresenceMonitoringMode !=
                        SecondaryAuthenticationFactorDevicePresenceMonitoringMode.AppManaged)
                {
                    // Skip the device which doesn't need to be monitored in the background task
                    continue;
                }

                SecondaryAuthenticationFactorDevicePresence state =
                    SecondaryAuthenticationFactorDevicePresence.Absent;

                await deviceInfo.UpdateDevicePresenceAsync(state);
                //string buf = deviceInfo.DeviceConfigurationData.ToString();
                SecondaryAuthenticationFactorDevicePresenceMonitoringMode mode = deviceInfo.PresenceMonitoringMode;
            }
        }
        private async Task PresenceMonitor()
        {
            //ShowToastNotification("Presence monitor triggered!");
            System.Diagnostics.Debug.WriteLine("[PresenceMonitor] triggered");
            //Debugger.Break();
            // Query the devices which can do presence check for the console user
            IReadOnlyList<SecondaryAuthenticationFactorInfo> deviceInfoList =
                await SecondaryAuthenticationFactorRegistration.FindAllRegisteredDeviceInfoAsync(SecondaryAuthenticationFactorDeviceFindScope.User);

            if (deviceInfoList.Count == 0)
            {
                return;
            }

            

            foreach (SecondaryAuthenticationFactorInfo deviceInfo in deviceInfoList)
            {
                if (deviceInfo.PresenceMonitoringMode !=
                        SecondaryAuthenticationFactorDevicePresenceMonitoringMode.AppManaged)
                {
                    // Skip the device which doesn't need to be monitored in the background task
                    continue;
                }
                else
                {
                    //Debugger.Break();
                    //pluggedRegisteredDeviceList = await getPluggedRegisteredDeviceListAsync();
                }

                //
                // 3rd party device specific code
                //
                // The background task should check if the device is near-by or not and update the state value
                // if (device is nearby)
                // {
                //     state = SecondaryAuthenticationFactorDevicePresenceState.Present;
                // }

                //use firstTimePresenceMonitoringCheck only for test/debug purposes
                //The first call for presence monitoring comes after 10seconds of STOPPING AUTH
                //At this time, we need to send Present back to natural auth, else it won't ask again.
                //The subsequent calls come after 30 seconds of system idle which should send
                //actual prsence state.   We are sending Absent, just to test/illustrate how
                //goodbye works.
                await deviceInfo.UpdateDevicePresenceAsync(SecondaryAuthenticationFactorDevicePresence.Present);
                //firstTimePresenceMonitoringCheck = false;

                //if (firstTimePresenceMonitoringCheck)
                //{
                //    //firsttime after lock should set to "yes" else cdf goodbye will not check for this logon until
                //    //locked again
                //    await deviceInfo.UpdateDevicePresenceAsync(SecondaryAuthenticationFactorDevicePresence.Present);
                //    firstTimePresenceMonitoringCheck = false;
                //}
                //else
                //{
                //    firstTimePresenceMonitoringCheck = true;
                //    await deviceInfo.UpdateDevicePresenceAsync(SecondaryAuthenticationFactorDevicePresence.Absent);
                //}

            }
        }
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
            if (args.StageInfo.Stage == SecondaryAuthenticationFactorAuthenticationStage.CheckingDevicePresence)
            {
                Task t = PresenceMonitor();
                t.Wait();
                opCompletedEvent.Set();
                deferral.Complete();
                return;
            }
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
