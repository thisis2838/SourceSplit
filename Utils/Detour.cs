using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utils
{
    [Serializable]
    public class ByteReplacement
    {
        private byte[] _old;
        private int _count;
        private IntPtr _address;

        public ByteReplacement(Process p, IntPtr address, int count)
        {
            _address = address;
            _count = count;
            _old = p.ReadBytes(_address, _count);
        }

        public void Replace(Process p, byte[] newBytes)
        {
            Trace.Assert(newBytes.Length == _count);
            p.WriteBytes(_address, newBytes);
        }

        public void Restore(Process p)
        {
            p.WriteBytes(_address, _old);
        }
    }

    [Serializable]
    public class Detour
    {
        private ByteReplacement _replaced;
        public IntPtr Destination;
        private byte[] _newBytes = new byte[] { };
        private IntPtr _newBytesLoc;

        public Detour(Process p, IntPtr from, IntPtr to, int preserve, byte[] newBytes, bool preserveAfter = false)
        {
            _replaced = new ByteReplacement(p, from, preserve);

            _newBytes = new byte[newBytes.Length];
            newBytes.CopyTo(_newBytes, 0);
            Destination = to;

            p.WriteBytes(to + (preserveAfter ? newBytes.Length : 0), p.ReadBytes(from, preserve));
            p.WriteJumpInstruction(from, to);
            _newBytesLoc = to + (preserveAfter ? 0 : preserve);
            p.WriteBytes(_newBytesLoc, newBytes);

            p.WriteJumpInstruction(to + preserve + newBytes.Length, from + preserve);
            Debug.WriteLine($"Written detour from 0x{from.ToString("X")} to 0x{to.ToString("X")} [{newBytes.Length} bytes]");
        }

        public Detour()
        {
            _replaced = null;
        }

        public bool VerifyIntegrity(Process p)
        {
           return p.ReadBytes(_newBytesLoc, _newBytes.Length)?.SequenceEqual(_newBytes)?? false;
        }

        public void Restore(Process p)
        {
            _replaced.Restore(p);
            p.FreeMemory(Destination);
        }
    }
}
