using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpHID.SetupApiDll;
using static SharpHID.HidDll;
using System.Runtime.InteropServices;

namespace SharpHID
{
    public static class HID
    {
        private static Guid _interfaceClassGuid;

        static HID()
        {
            HidD_GetHidGuid(out _interfaceClassGuid);
        }

        public static bool IsDeviceConnected(ushort VID, ushort PID, ushort MI, ushort COL)
        {
            bool found = false;
            IntPtr h = SetupDiGetClassDevs(ref _interfaceClassGuid, IntPtr.Zero, IntPtr.Zero, 0x2 | 0x10);

            SP_DEVICE_INTERFACE_DATA spDID = new SP_DEVICE_INTERFACE_DATA();
            spDID.cbSize = Marshal.SizeOf(spDID);

            string vendor = $"vid_{VID:X4}";
            string product = $"pid_{PID:X4}";
            string @interface = $"mi_{MI:X2}";
            string collection = $"col_{COL:X2}";

            uint member = 0;
            while (SetupDiEnumDeviceInterfaces(h, IntPtr.Zero, ref _interfaceClassGuid, member, ref spDID))
            {
                SP_DEVINFO_DATA devInfo = new SP_DEVINFO_DATA();
                devInfo.cbSize = Marshal.SizeOf(devInfo);
                string devicePath;
                if (SetupDiGetDeviceInterfaceDetail(h, ref spDID, out devicePath, ref devInfo))
                {
                    if (devicePath.Contains(vendor) && devicePath.Contains(product))
                    {
                        if ((MI == 0 || devicePath.Contains(@interface)) &&
                            (COL == 0 || devicePath.Contains(collection)))
                        {
                            found = true;
                        }
                    }
                }

                if (found)
                    break;
                else
                    member++;
            }
            SetupDiDestroyDeviceInfoList(h);
            return found;
        }
    }
}
