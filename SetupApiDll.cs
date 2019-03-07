using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpHID
{
    internal static class SetupApiDll
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid interfaceClassGuid;
            public int flags;
            private UIntPtr reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            public char devicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]     // 2nd form uses an Enumerator only, with null ClassGUID 
        internal static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, int Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(
                                                        IntPtr hDevInfo,
                                                        ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
                                                        IntPtr deviceInterfaceDetailData,
                                                        int deviceInterfaceDetailDataSize,
                                                        ref uint requiredSize,
                                                        ref SP_DEVINFO_DATA deviceInfoData);

        internal static bool SetupDiGetDeviceInterfaceDetail(
                                                        IntPtr hDevInfo,
                                                        ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
                                                        out string devicePath,
                                                        ref SP_DEVINFO_DATA deviceInfoData)
        {
            bool gotDetails = false;
            devicePath = "";

            IntPtr detailData = IntPtr.Zero;
            UInt32 detailSize = 0;
            SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, detailData, 0, ref detailSize, ref deviceInfoData);
            if (detailSize > 0)
            {
                int structSize = Marshal.SystemDefaultCharSize;
                if (IntPtr.Size == 8)
                    structSize += 6;
                else
                    structSize += 4;

                detailData = Marshal.AllocHGlobal((int)detailSize + structSize);
                Marshal.WriteInt32(detailData, (int)structSize);
                bool success = SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, detailData, (int)detailSize, ref detailSize, ref deviceInfoData);
                if (success)
                {
                    devicePath = Marshal.PtrToStringUni(new IntPtr(detailData.ToInt64() + 4)).ToLower();
                    gotDetails = true;
                }
                Marshal.FreeHGlobal(detailData);
            }

            return gotDetails;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);    
    }
}
