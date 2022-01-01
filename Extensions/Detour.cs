using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Extensions
{
    public class ByteReplacement
    {
        private byte[] _old;
        private int _count;
        private IntPtr _address;
        Process _process;

        public ByteReplacement(Process p, IntPtr address, int count)
        {
            _address = address;
            _process = p;
            _count = count;
            _old = p.ReadBytes(_address, _count);
        }

        public void Replace(byte[] newBytes)
        {
            Trace.Assert(newBytes.Length == _count);
            _process.WriteBytes(_address, newBytes);
        }

        public void Restore()
        {
            _process.WriteBytes(_address, _old);
        }
    }
    public class Detour
    {
        private ByteReplacement _replaced;

        public Detour(Process p, IntPtr from, IntPtr to, int preserve, byte[] newBytes, bool preserveAfter = false)
        {
            _replaced = new ByteReplacement(p, from, preserve);

            p.WriteBytes(to + (preserveAfter ? newBytes.Length : 0), p.ReadBytes(from, preserve));
            p.WriteJumpInstruction(from, to);
            p.WriteBytes(to + (preserveAfter ? 0 : preserve), newBytes);

            p.WriteJumpInstruction(to + preserve + newBytes.Length, from + preserve);
            Debug.WriteLine($"Written detour from 0x{from.ToString("X")} to 0x{to.ToString("X")} [{newBytes.Length} bytes]");
        }

        public void Restore()
        {
            _replaced.Restore();
        }
    }
}
