using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaTFTPServer.Misc
{
    public static class TransferSpeed
    {
        public static string ToString(double bytesPerSecond)
        {
            const double kibibyte = 1024.0;

            string[] formats =
            [
                "{0:F0} ", "{0:F1} Ki", "{0:F2} Mi",
                "{0:F2} Gi", "{0:F2} Ti", "{0:F2} Pi",
                "{0:F2} Ei", "{0:F2} Zi", "{0:F2} Yi"
            ];

            int t = (Math.Abs(bytesPerSecond) < kibibyte) ? 0 : Math.Min(formats.Length - 1, (int)Math.Log(Math.Abs(bytesPerSecond), kibibyte));
            return string.Format(formats[t], bytesPerSecond / Math.Pow(kibibyte, t)) + "B/s";
        }
    }
}
