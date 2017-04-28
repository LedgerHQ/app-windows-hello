using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace WindowsHelloWithLedger
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
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }
    }
}
