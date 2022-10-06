using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utilities
{
    public class SigScanTargets
    {
        public string Name;
        public List<Tuple<string, SigScanTarget>> Targets = new List<Tuple<string, SigScanTarget>>();

        public void Add(string name, SigScanTarget target)
        {
            Targets.Add(new Tuple<string, SigScanTarget>(name, target));
        }

        public void Add(string name, Func<Process, SignatureScanner, IntPtr, IntPtr> onFound, params object[] pairs)
        {
            var sig = new SigScanTarget();
            sig.OnFound = (proc, scanner, ptr) => onFound(proc, scanner, ptr);

            string tmpStr = "";
            int off = 0;
            for (int i = 0; i < pairs.Length; i++)
            {
                if (pairs[i] is int)
                {
                    if (tmpStr != "")
                    {
                        sig.AddSignature(off, tmpStr);
                        tmpStr = "";
                    }
                    off = (int)pairs[i];
                    continue;
                }

                tmpStr += " " + (string)pairs[i];
            }

            if (tmpStr != "")
                sig.AddSignature(off, tmpStr);

            Add(name, sig);
        }
    }

    public static class SigScanExtensions
    {
        public static bool Scan(this SignatureScanner scanner, SigScanTargets targets, out IntPtr ptr)
        {
            ptr = IntPtr.Zero;
            foreach (var target in targets.Targets)
            {
                if ((ptr = scanner.Scan(target.Item2)) != IntPtr.Zero)
                {
                    Debug.WriteLine($"Found {targets.Name} : 0x{ptr.ToString("x")} through sig {target.Item1}");
                    return true;
                }
            }
            Debug.WriteLine($"Couldn't find {targets.Name} through any sig!");
            return false;
        }
    }
}
