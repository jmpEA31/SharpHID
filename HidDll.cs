using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpHID
{
    internal static class HidDll
    {
        [DllImport("hid.dll", EntryPoint = "HidD_GetHidGuid", SetLastError = true)]
        static extern internal void HidD_GetHidGuid(out Guid Guid);
    }
}
