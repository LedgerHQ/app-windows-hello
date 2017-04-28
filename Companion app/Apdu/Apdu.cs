using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace Apdu
{
    public static class apdu
    {
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
}
