using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class SigScannerUtils
    {
        static public IntPtr ReadCall(this SignatureScanner scanner, IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return IntPtr.Zero;
            return (IntPtr)(scanner.Process.ReadValue<int>(ptr + 0x1) + (int)(ptr + 5));
        }

        public struct ScanGetAllResult
        {
            public IntPtr Location;
            public IntPtr OnFoundResult;
        }
        static public List<ScanGetAllResult> ScanGetAll(this SignatureScanner scanner, SigScanTarget target)
        {
            SigScanTarget newTarget = new SigScanTarget();
            target.Signatures.ToList().ForEach(x => newTarget.AddSignature(x.Offset, x.Pattern));

            long end = (long)scanner.Address + scanner.Size;
            List<ScanGetAllResult> ptrs = new List<ScanGetAllResult>();
            while (scanner.Size > 0)
            {
                var res = scanner.Scan(newTarget);
                if (res != IntPtr.Zero)
                {
                    ptrs.Add(new ScanGetAllResult()
                    {
                        Location = res,
                        OnFoundResult = target.OnFound?.Invoke(scanner.Process, scanner, res) ?? res
                    });
                    scanner.Address = res + 1;
                    scanner.Size = (int)((long)end - (long)scanner.Address);
                }
                else break;
            }

            return ptrs;
        }

        static public bool IsWithin(this SignatureScanner scanner, long value)
        {
            if (value == 0) return false;

            uint start = (uint)scanner.Address;
            uint end = start + (uint)scanner.Size;

            return start < value && value < end;
        }

        static public bool IsWithin(this SignatureScanner scanner, uint value) => IsWithin(scanner, (long)value);
        static public bool IsWithin(this SignatureScanner scanner, IntPtr value) => IsWithin(scanner, (long)value);
    }
}
